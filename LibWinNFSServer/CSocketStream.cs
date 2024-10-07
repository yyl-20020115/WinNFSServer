namespace LibWinNFSServer;

public class CSocketStream : IInputStream, IOutputStream
{
    public const int MAXDATA = 1 << 20;

    private readonly byte[] inBuffer = new byte[MAXDATA];
    private readonly byte[] outBuffer = new byte[MAXDATA];
    private int inBufferIndex = 0;
    private int inBufferSize = 0;
    private int outBufferIndex = 0;
    private int outBufferSize = 0;

    public byte[] Input => inBuffer;
    public byte[] Output => outBuffer;
    public int OutputSize => outBufferSize;
    public int BufferSize => MAXDATA;
    public CSocketStream()
    {

    }

    public void SetInputSize(int size)
    {
        inBufferIndex = 0;  //seek to the beginning of the input buffer
        inBufferSize = size;
    }

    public int Read(byte[] data)
    {
        int nSize = data.Length;
        if (nSize > inBufferSize - inBufferIndex)
        { //over the number of bytes of data in the input buffer
            nSize = inBufferSize - inBufferIndex;
        }

        Array.Copy(inBuffer , inBufferIndex, data, 0, nSize);
        inBufferIndex += nSize;

        return nSize;
    }
    public int Read(out int value)
    {
        var data = new byte[sizeof(int)];
        int s = Read(data);
        value = BitConverter.ToInt32(data);
        return s;
    }
    public int Read8(out long value)
    {
        var pData = new byte[sizeof(long)];
        int s = Read(pData);
        value = BitConverter.ToInt64(pData);
        return s;
    }
    public int Read(out uint value)
    {
        var pData = new byte[sizeof(uint)];
        int s = Read(pData);
        value = BitConverter.ToUInt32(pData);
        return s;
    }
    public int Read8(out ulong value)
    {
        var pData = new byte[sizeof(ulong)];
        int s = Read(pData);
        value = BitConverter.ToUInt64(pData);
        return s;
    }
    public int Skip(int size)
    {
        if (size > inBufferSize - inBufferIndex)
        { //over the number of bytes of data in the input buffer
            size = inBufferSize - inBufferIndex;
        }

        inBufferIndex += size;

        return size;
    }
    public uint Skip(uint nSize)
    {
        if (nSize > inBufferSize - inBufferIndex)
        { //over the number of bytes of data in the input buffer
            nSize = (uint)(inBufferSize - inBufferIndex);
        }

        inBufferIndex += (int)nSize;

        return nSize;
    }
    public int GetSize() => inBufferSize - inBufferIndex;
    public void Write(byte[] data) 
    {
        int size = data.Length;
        if (outBufferIndex + size > BufferSize)
        { //over the size of output buffer
            size = MAXDATA - outBufferIndex;
        }

        Array.Copy(data, 0 ,outBuffer, outBufferIndex,size);
        outBufferIndex += size;

        if (outBufferIndex > outBufferSize)
        {
            outBufferSize = outBufferIndex;
        }

    }
    public void Write(int value)
    {
        Write(BitConverter.GetBytes(value));
    }
    public void Write8(long value)
    {
        Write(BitConverter.GetBytes(value));
    }
    public void Write(uint value)
    {
        Write(BitConverter.GetBytes(value));
    }
    public void Write8(ulong value)
    {
        Write(BitConverter.GetBytes(value));
    }
    public void Seek(int offset, SEEKS seek)
    {
        switch (seek)
        {
            case SEEKS.SEEK_SET:
                outBufferIndex = offset;
                break;
            case SEEKS.SEEK_CUR:
                outBufferIndex += offset;
                break;
            case SEEKS.SEEK_END:
                outBufferIndex = outBufferSize + offset;
                break;
        }

    }
    public int Position => outBufferIndex;
    public void Reset() => outBufferIndex = outBufferSize = 0;
}
