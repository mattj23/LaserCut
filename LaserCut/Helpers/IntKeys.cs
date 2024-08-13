namespace LaserCut.Helpers;

public static class IntKeys
{
    /// <summary>
    /// Make a key from two integers.  The first int will be the most significant 32 bits, the second int will be the
    /// least significant 32 bits.
    /// </summary>
    public static ulong Make(int a, int b)
    {
        return (ulong)a << 32 | (uint)b;
    }

    /// <summary>
    /// Make an ordered key from two integers.  The most significant 32 bits will be the smaller of the two ints, the
    /// least significant 32 bits will be the larger of the two ints.
    /// </summary>
    public static ulong MakeOrdered(int a, int b)
    {
        return Make(Math.Min(a, b), Math.Max(a, b));
    }

}
