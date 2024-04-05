namespace LaserCut.Mesh;

public class BinaryStlReader
{
    private readonly BinaryReader _reader;

    public BinaryStlReader(BinaryReader reader)
    {
        _reader = reader;
    }
    
    public byte[] ReadBytes(int count)
    {
        return _reader.ReadBytes(count);
    }
    
    public uint ReadUInt32()
    {
        var bytes = _reader.ReadBytes(4);
        
        // Little endian
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        
        return BitConverter.ToUInt32(bytes);
    }
    
    public ushort ReadUInt16()
    {
        var bytes = _reader.ReadBytes(2);
        
        // Little endian
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        
        return BitConverter.ToUInt16(bytes);
    }
    
    public float ReadSingle()
    {
        var bytes = _reader.ReadBytes(4);
        
        // Little endian
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        
        return BitConverter.ToSingle(bytes);
    }
}