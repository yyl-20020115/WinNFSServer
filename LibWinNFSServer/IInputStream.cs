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
