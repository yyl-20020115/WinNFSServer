namespace LibWinNFSServer;

public class nfspath3 : opaque
{
public
	string path;

    public nfspath3() { }
    ~nfspath3() { }
    public override void SetSize(uint len) { }
    void Set(string str) => path = str;
}
