namespace LibWinNFSServer;

public class CSocketStream : IInputStream, IOutputStream
{
    public CSocketStream();
     ~CSocketStream();
    byte[] GetInput();
    void SetInputSize(uint nSize);
    byte[] GetOutput();
    uint GetOutputSize();
    uint GetBufferSize();
    uint Read(void* pData, uint nSize);
    uint Read(unsigned long* pnValue);
    uint Read8(unsigned __int64 *pnValue);
    uint Skip(uint nSize);
    uint GetSize();
    void Write(void* pData, uint nSize);
    void Write(unsigned long nValue);
    void Write8(unsigned __int64 nValue);
    void Seek(int nOffset, int nFrom);
    int GetPosition();
    void Reset();

    private
    byte[] m_pInBuffer, *m_pOutBuffer;
}
