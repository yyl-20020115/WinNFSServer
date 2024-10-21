namespace LibWinNFSServer;
public interface IInputStream
{
    int Read(byte[] pData);
    int Read(out int pnValue);
    int Read8(out long pnValue);
    int Read(out uint pnValue);
    int Read8(out ulong pnValue);
    int Skip(int nSize);
    uint Skip(uint nSize);
    int GetSize();
}


public interface IOutputStream
{
    void Write(byte[] pData);
    void Write(int nValue);
    void Write8(long nValue);
    void Write(uint nValue);
    void Write8(ulong nValue);
    void Seek(int nOffset, SEEKS nFrom);
    int Position { get; }
}

public interface ISocketListener
{
    void SocketReceived(CSocket pSocket);
}
