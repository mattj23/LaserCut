namespace LaserCut.Box;

public class FaceSet<T>
{
    private readonly T?[] _array = new T[6];

    public T? Front
    {
        get => _array[0];
        set => _array[0] = value;
    }

    public T? Back
    {
        get => _array[1];
        set => _array[1] = value;
    }

    public T? Bottom
    {
        get => _array[2];
        set => _array[2] = value;
    }

    public T? Top
    {
        get => _array[3];
        set => _array[3] = value;
    }

    public T? Left
    {
        get => _array[4];
        set => _array[4] = value;
    }

    public T? Right
    {
        get => _array[5];
        set => _array[5] = value;
    }

    public IReadOnlyList<T> All => _array.Where(x => x != null).ToArray()!;

    public T? this[BoxFaceType faceType]
    {
        get => _array[Index(faceType)];
        set => _array[Index(faceType)] = value;
    }

    public void Clear() => Array.Clear(_array, 0, _array.Length);

    public FaceSet<T1> Select<T1>(Func<T?, T1> selector) => new()
    {
        Front = selector(Front),
        Back = selector(Back),
        Bottom = selector(Bottom),
        Top = selector(Top),
        Left = selector(Left),
        Right = selector(Right)
    };

    private int Index(BoxFaceType faceType) => faceType switch
    {
        BoxFaceType.Front => 0,
        BoxFaceType.Back => 1,
        BoxFaceType.Bottom => 2,
        BoxFaceType.Top => 3,
        BoxFaceType.Left => 4,
        BoxFaceType.Right => 5,
        _ => throw new ArgumentOutOfRangeException(nameof(faceType), faceType, null)
    };

}
