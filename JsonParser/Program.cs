using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using ArrayBuilder;

var content = await File.ReadAllTextAsync("some-file.json");
var elements = JsonSerializer.Deserialize<FileStuffs[]>(content);

var parserEls = JsonParser.Parse(content);
var serialized = new ValueSyntaxWriter();
parserEls.FormatInto(ref serialized);
Console.WriteLine(serialized.ToString());

Console.WriteLine(elements);

public ref struct JsonParser {
    private readonly ReadOnlySpan<char> _json;
    private int _pos;
    
    private ref readonly char Current => ref _json[_pos];
    private ReadOnlySpan<char> Remaining => _json[_pos..];
    
    private bool HasNext => _pos < _json.Length;
    
    private const string FalseString = "false";
    private const string TrueString = "true";
    private const string NullString = "null";
    
    private JsonParser(in ReadOnlySpan<char> json) {
        _json = json;
    }
    
    private IJSonNode Parse() {
        if (_json.IsEmpty) {
            return new ObjectNode { Properties = new Dictionary<string, IJSonNode>()};
        }

        SkipWhiteSpace();
        return ParseNextNode();
    }

    private IJSonNode ParseNextNode() {
        return Current switch {
            '{' => ParseObject(),
            '[' => ParseArray(),
            '"' => ParseString(),
            _ when char.IsDigit(Current) || Current == '-' => ParseNumber(),
            _ when TryParseBoolean(out var boolean) => boolean,
            _ when TryParseNull() => NullNode.Instance,
            _ => throw new JsonException($"Unexpected character '{Current}' at position {_pos}.")
        };
    }

    private bool TryParseNull() {
        if (Current != 'n') {
            return false;
        }
        
        if (Remaining.StartsWith(NullString, StringComparison.Ordinal)) {
            Skip(NullString.Length);
            return true;
        }
        
        return false;
    }

    private bool TryParseBoolean([NotNullWhen(true)] out BooleanNode? boolean) {
        if (Current is not 'f' and not 't') {
            boolean = null;
            return false;
        }
        
        if (Remaining.StartsWith(TrueString, StringComparison.Ordinal)) {
            boolean = new BooleanNode { Value = true };
            Skip(TrueString.Length);
            return true;
        }
        
        if (Remaining.StartsWith(FalseString, StringComparison.Ordinal)) {
            boolean = new BooleanNode { Value = false };
            Skip(FalseString.Length);
            return true;
        }

        boolean = null; 
        return false;
    }

    private StringNode ParseString() {
        var str = ParseStringCore();
        return new  StringNode { Value = str };
    }

    private string ParseStringCore() {
        Next(); // Skip the opening quote
        FindEndOfString:
        var startPos = _pos;
        var endPos = Remaining.IndexOf('"');
        if (endPos < 0) {
            throw new JsonException("Unterminated string.");
        }

        var isEscaped = Remaining[endPos - 1] == '\\' && Remaining[endPos - 2] != '\\';
        if (isEscaped) {
            SkipTo(endPos + _pos + 1);
            goto FindEndOfString;
        }

        var endPosInGlobal = endPos + _pos;
        var str = _json[startPos..endPosInGlobal].ToString();

        SkipTo(endPos + _pos + 1);  // Skip to the end of the string, including the closing quote
        return str;
    }

    // Might need a more complex implementation to handle scientific notation, etc.
    public NumberNode ParseNumber() {
        var isNegative = Current == '-';
        if (isNegative) {
            Next(); // Skip the negative sign
        }
        
        var startPos = _pos;
        while (HasNext && char.IsDigit(Current)) {
            Next();
        }

        if (Current == '.') {
            Next();

            while (HasNext && char.IsDigit(Current)) {
                Next();
            }
        }
        
        var endPos = _pos;
        var numberStr = _json[startPos..endPos].ToString();
        return new NumberNode { Value = numberStr };
    }
    
    private ObjectNode ParseObject() {
        Next(); // Skip the opening brace
        var properties = new Dictionary<string, IJSonNode>();
        while (HasNext && Current != '}') {
            SkipWhiteSpace();
            var p = ParseProperty();
            properties.Add(p.Key, p.Value);
            SkipWhiteSpace();
            if (Current == ',') {
                Next(); // Skip the comma
                continue;
            }

            if (Current != '}') {
                throw new JsonException($"Expected ',' or '}}' but found '{Current}' at position {_pos}.");
            }
        }
        
        if (Current != '}') {
            throw new JsonException($"Unexpected end of object. Expected '}}' but found '{Current}' at position {_pos}.");
        }
        
        Next(); // Skip the closing brace
        return new  ObjectNode { Properties = properties };
    }

    private (string Key, IJSonNode Value) ParseProperty() {
        // We assume white space has been skipped before calling this method
        var key = ParseStringCore();
        SkipWhiteSpace();
        if (Current != ':') {
            throw new JsonException($"Expected ':' after property name but found '{Current}' at position {_pos}.");
        }
        Next(); // Skip the colon
        SkipWhiteSpace();
        var value = ParseNextNode();
        return (key, value);
    }

    private ArrayNode ParseArray() {
        Next(); // Skip the opening bracket
        var elements = ImmutableArray.CreateBuilder<IJSonNode>();
        while (HasNext && Current != ']') {
            SkipWhiteSpace();
            var node = ParseNextNode();
            elements.Add(node);
            SkipWhiteSpace();
            if (Current == ',') {
                Next(); // Skip the comma
                continue;
            }
            
            if (Current != ']') {
                throw new JsonException($"Expected ',' or ']' but found '{Current}' at position {_pos}.");
            }
        }
        
        if (Current != ']') {
            throw new JsonException($"Unexpected end of array. Expected ']' but found '{Current}' at position {_pos}.");
        }
        
        Next(); // Skip the closing bracket
        return new ArrayNode { Elements = elements.ToImmutableArray() };
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Next() {
        _pos++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Skip(int count) {
        _pos += count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SkipTo(int pos) {
        _pos = pos;
    }
    
    private void SkipWhiteSpace() {
        while (_pos < _json.Length && char.IsWhiteSpace(Current)) {
            Next();
        }
    }
    
    public static IJSonNode Parse(ReadOnlySpan<char> json) {
        var p = new JsonParser(json);
        return p.Parse();
    }
}

public interface IJSonNode {
    public void FormatInto(ref ValueSyntaxWriter builder);
};

public sealed class StringNode : IJSonNode {
    public required string Value { get; set; }
    
    public void FormatInto(ref ValueSyntaxWriter builder) {
        builder.Write("\"");
        foreach (var c in Value) {
            builder.Write(c);
        }
        builder.Write("\"");
    }
}

public sealed class NullNode : IJSonNode {
    public static readonly NullNode Instance = new();
    
    public void FormatInto(ref ValueSyntaxWriter builder) {
        builder.Write("null");
    }
}

public sealed class NumberNode : IJSonNode {
    public required string Value { get; set; }
    
    public double ToDouble() => double.Parse(Value);
    
    public void FormatInto(ref ValueSyntaxWriter builder) {
        builder.Write(Value);
    }
}

public sealed class ArrayNode : IJSonNode {
    public required ImmutableArray<IJSonNode> Elements { get; set; }
    
    public void FormatInto(ref ValueSyntaxWriter builder) {
        builder.WriteLine("[");
        builder.Indent();
        for (var i = 0; i < Elements.Length; i++) {
            var element = Elements[i];
            element.FormatInto(ref builder);
            var isLastElement = i == Elements.Length - 1;
            if (!isLastElement) {
                builder.WriteLine(",");
            }
        }

        builder.WriteLine();
        builder.Dedent();
        builder.Write("]");
    }
}

public sealed class ObjectNode : IJSonNode {
    public required Dictionary<string, IJSonNode> Properties { get; set; }
    
    public void FormatInto(ref ValueSyntaxWriter builder) {
        var keys = Properties.Keys.ToArray();
        var count = keys.Length;
        builder.WriteLine("{");
        builder.Indent();
        for (var i = 0; i < count; i++) {
            var k = keys[i];
            var v = Properties[keys[i]];
            builder.Write($"\"{k}\": ");
            v.FormatInto(ref builder);
            
            var isLastElement = i == count - 1;
            if (!isLastElement) {
                builder.WriteLine(",");
            }
        }
        builder.WriteLine();
        builder.Dedent();
        builder.Write("}");
    }
}

public sealed class BooleanNode : IJSonNode {
    public required bool Value { get; set; }
    
    public void FormatInto(ref ValueSyntaxWriter builder) {
        builder.Write(Value ? "true" : "false");
    }
}

sealed class FileStuffs {
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("age")]
    public int Age { get; set; }

    [JsonPropertyName("skills")]
    public Skill[] Skills { get; set; }
    
    public sealed class Skill {
        public string Name { get; set; }
    }
}