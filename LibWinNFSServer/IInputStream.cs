namespace LibWinNFSServer;
public interface IInputStream
{
    int Read(byte[] pData);
    int Read(ref int pnValue);
    int Read8(ref long pnValue);
    int Skip(int nSize);
    int GetSize();
};
