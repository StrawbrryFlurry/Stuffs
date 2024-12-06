using System.Runtime.CompilerServices;

namespace ArrayBuilder;

public ref partial struct ArrayBuilder<TElement> {
  [ThreadStatic]
  private static ArrayBuffer128 _currentBuffer;

  private static NullBuffer _nullBufferInstance;
  
  [InlineArray(8)]
  [SkipLocalsInit]
  private struct ArrayBuffer4 {
    private TElement _element0;
  }

  [InlineArray(16)]
  [SkipLocalsInit]
  private struct ArrayBuffer16 {
    private TElement _element0;
  }

  [InlineArray(32)]
  [SkipLocalsInit]
  private struct ArrayBuffer32 {
    private TElement _element0;
  }

  [InlineArray(64)]
  [SkipLocalsInit]
  private struct ArrayBuffer64 {
    private TElement _element0;
  }

  [InlineArray(128)]
  [SkipLocalsInit]
  private struct ArrayBuffer128 {
    private TElement _element0;
  }

  [SkipLocalsInit]
  private struct NullBuffer;
}