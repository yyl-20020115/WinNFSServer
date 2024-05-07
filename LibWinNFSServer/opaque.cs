namespace LibWinNFSServer;

public class opaque
{
	public uint length;
    public byte[] contents;

    public opaque() { }
    public opaque(uint len)
    {
        this.length = len;
    }

    public virtual void SetSize(uint len) => this.length = len;
}
