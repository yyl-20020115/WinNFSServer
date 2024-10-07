namespace LibWinNFSServer;

public class Opaque
{
	public uint length = 0;
    public byte[]? contents = null;

    public Opaque() { }
    public Opaque(uint len) => this.SetSize(len);

    public virtual void SetSize(uint len)
    {
        this.contents = new byte[this.length = len];
    }
}
