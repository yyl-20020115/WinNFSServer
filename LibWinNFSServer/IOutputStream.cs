namespace LibWinNFSServer;

public interface IOutputStream
{
    void Write(byte[] pData);
    void Write(uint nValue);
    void Write8(ulong nValue);
    void Seek(int nOffset, int nFrom);
    int GetPosition() ;
}