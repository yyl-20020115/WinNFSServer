namespace LibWinNFSServer;
public interface IInputStream
{
    int Read(byte[] pData);
    int Read(ref int pnValue);
    int Read8(ref long pnValue);
    int Read(ref uint pnValue);
    int Read8(ref ulong pnValue);
    int Skip(int nSize);
    uint Skip(uint nSize);
    int GetSize();
}
