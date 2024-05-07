namespace LibWinNFSServer;

public class filename3 :  opaque
{
    public string name;

    public filename3() { }
    ~filename3() { }
    public override void SetSize(uint len) => length = len;
    void Set(string str) => name = str;
}
