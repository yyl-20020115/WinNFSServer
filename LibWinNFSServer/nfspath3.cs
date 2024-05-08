using System.Text;

namespace LibWinNFSServer;

public class Nfspath3 : Opaque
{
    public string? path = null;

    public Nfspath3() { }
    ~Nfspath3() { }
    public override void SetSize(uint len) { }
    void Set(string str) => this.contents = Encoding.UTF8.GetBytes(path = str);
}
