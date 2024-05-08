namespace LibWinNFSServer;

public class FILE_ITEM
{
    public string Path = "";
    public int PathLength = 0;
    public byte[] Handle = new byte[64]; //64
    public bool IsCached = false;
}
