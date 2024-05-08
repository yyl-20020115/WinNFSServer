using System.Text;

namespace LibWinNFSServer;

public class Filename3 :  Opaque
{
    public string? name = null;

    public Filename3() { }
    ~Filename3() { }
    public override void SetSize(uint len) => length = len;
    public void Set(string str) => this.contents = Encoding.UTF8.GetBytes(name = str);
}
