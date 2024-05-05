namespace LibWinNFSServer;

public interface IOutputStream
{
    void Write(byte[] pData);
    void Write(int nValue);
    void Write8(long nValue);
    void Write(uint nValue);
    void Write8(ulong nValue);
    void Seek(int nOffset, SEEKS nFrom);
    int GetPosition() ;
}