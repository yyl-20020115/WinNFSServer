namespace LibWinNFSServer;
public interface IInputStream
{
    uint Read(byte[] pData);
    uint Read(ref uint pnValue);
    uint Read8(ref ulong pnValue);
    uint Skip(uint nSize);
    uint GetSize();
};
