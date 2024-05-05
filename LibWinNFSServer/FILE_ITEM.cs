namespace LibWinNFSServer;

public class FILE_ITEM
{
    public string path = "";
    public int nPathLen = 0;
    public byte[] handle = new byte[64]; //64
    public bool bCached = false;
};
