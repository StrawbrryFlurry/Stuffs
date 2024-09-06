using static Pippy.Pipes;

int[] ints = [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 ];
Console.WriteLine(
    ints | Where<int>(s => s > 4) | Select<int, int>(x => x * 2) | Take<int>(3) | ToArray<int>()
);


Console.WriteLine(
    "E ll ooo ! :3"
        | To<string, string>(x => x.ToLower())
        | To<string, IEnumerable<char>>(x => x)
        | Where<char>(x => x != ' ')
        | ToArray<char>()
        | To<char[], string>(x => new string(x.ToArray())) 
        | Unwrap<string>()
);
