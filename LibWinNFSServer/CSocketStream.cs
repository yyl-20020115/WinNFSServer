namespace LibWinNFSServer;

public class CSocketStream : IInputStream, IOutputStream
{
    public const int MAXDATA = 1 << 20;

    private byte[] m_pInBuffer = new byte[MAXDATA];
    private byte[] m_pOutBuffer = new byte[MAXDATA];
    private int m_nInBufferIndex = 0;
    private int m_nInBufferSize = 0;
    private int m_nOutBufferIndex = 0;
    private int m_nOutBufferSize = 0;

    public CSocketStream()
    {

    }
    ~CSocketStream()
    {

    }
    public byte[] GetInput() => m_pInBuffer;
    public void SetInputSize(int nSize)
    {
        m_nInBufferIndex = 0;  //seek to the beginning of the input buffer
        m_nInBufferSize = nSize;
    }
    public byte[] GetOutput() => m_pOutBuffer;

    public int GetOutputSize() => m_nOutBufferSize;
    public int GetBufferSize() => MAXDATA;
    public int Read(byte[] pData)
    {
        int nSize = pData.Length;
        if (nSize > m_nInBufferSize - m_nInBufferIndex)
        { //over the number of bytes of data in the input buffer
            nSize = m_nInBufferSize - m_nInBufferIndex;
        }

        Array.Copy(m_pInBuffer , m_nInBufferIndex, pData, 0, nSize);
        m_nInBufferIndex += nSize;

        return nSize;
    }
    public int Read(ref int pnValue)
    {
        byte[] pData = new byte[sizeof(int)];
        int s = Read(pData);
        pnValue = BitConverter.ToInt32(pData);
        return s;
    }
    public int Read8(ref long pnValue)
    {
        byte[] pData = new byte[sizeof(long)];
        int s = Read(pData);
        pnValue = BitConverter.ToInt64(pData);
        return s;
    }
    public int Read(ref uint pnValue)
    {
        byte[] pData = new byte[sizeof(uint)];
        int s = Read(pData);
        pnValue = BitConverter.ToUInt32(pData);
        return s;
    }
    public int Read8(ref ulong pnValue)
    {
        byte[] pData = new byte[sizeof(ulong)];
        int s = Read(pData);
        pnValue = BitConverter.ToUInt64(pData);
        return s;
    }
    public int Skip(int nSize)
    {
        if (nSize > m_nInBufferSize - m_nInBufferIndex)
        { //over the number of bytes of data in the input buffer
            nSize = m_nInBufferSize - m_nInBufferIndex;
        }

        m_nInBufferIndex += nSize;

        return nSize;
    }
    public uint Skip(uint nSize)
    {
        if (nSize > m_nInBufferSize - m_nInBufferIndex)
        { //over the number of bytes of data in the input buffer
            nSize = (uint)(m_nInBufferSize - m_nInBufferIndex);
        }

        m_nInBufferIndex += (int)nSize;

        return nSize;
    }
    public int GetSize()
    {
        return m_nInBufferSize - m_nInBufferIndex;
    }
    public void Write(byte[] pData) 
    {
        int nSize = pData.Length;
        if (m_nOutBufferIndex + nSize > GetBufferSize())
        { //over the size of output buffer
            nSize = MAXDATA - m_nOutBufferIndex;
        }

        Array.Copy(pData, 0 ,m_pOutBuffer, m_nOutBufferIndex,nSize);
        m_nOutBufferIndex += nSize;

        if (m_nOutBufferIndex > m_nOutBufferSize)
        {
            m_nOutBufferSize = m_nOutBufferIndex;
        }

    }
    public void Write(int nValue)
    {
        Write(BitConverter.GetBytes(nValue));
    }
    public void Write8(long nValue)
    {
        Write(BitConverter.GetBytes(nValue));
    }
    public void Write(uint nValue)
    {
        Write(BitConverter.GetBytes(nValue));
    }
    public void Write8(ulong nValue)
    {
        Write(BitConverter.GetBytes(nValue));
    }
    public void Seek(int nOffset, SEEKS nFrom)
    {
        switch (nFrom)
        {
            case SEEKS.SEEK_SET:
                m_nOutBufferIndex = nOffset;
                break;
            case SEEKS.SEEK_CUR:
                m_nOutBufferIndex += nOffset;
                break;
            case SEEKS.SEEK_END:
                m_nOutBufferIndex = m_nOutBufferSize + nOffset;
                break;
        }

    }
    public int GetPosition()
    {
        return m_nOutBufferIndex;
    }
    public void Reset()
    {
        m_nOutBufferIndex = m_nOutBufferSize = 0;  
    }
}
