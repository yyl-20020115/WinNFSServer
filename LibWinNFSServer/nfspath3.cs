using System.Text;

namespace LibWinNFSServer;

public class NfsPath3 : Opaque
{
    public string? path = null;

    public NfsPath3() { }
    ~NfsPath3() { }
    public override void SetSize(uint len) { }
    void Set(string str) => this.contents = Encoding.UTF8.GetBytes(path = str);
}
