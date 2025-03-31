namespace LibWinNFSServer;
public interface InputStream
{
    int Read(byte[] data);
    int Read(out int value);
    int Read8(out long value);
    int Read(out uint value);
    int Read(out ulong value);
    int Skip(int size);
    uint Skip(uint size);
    int GetSize();
}


public interface OutputStream
{
    void Write(byte[] data);
    void Write(int value);
    void Write8(long value);
    void Write(uint value);
    void Write8(ulong value);
    void Seek(int offset, SEEKS from);
    int Position { get; }
}

public interface SocketListener
{
    void SocketReceived(ThreadSocket socket);
}
