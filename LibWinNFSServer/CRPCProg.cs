namespace LibWinNFSServer;
public abstract class CRPCProg
{
    protected delegate void PPROC();

    /* The maximum number of bytes in a pathname argument. */
    public const int MAXPATHLEN = 1024;

    /* The maximum number of bytes in a file name argument. */
    public const int MAXNAMELEN = 255;

    /* The size in bytes of the opaque file handle. */
    public const int FHSIZE = 32;
    public const int NFS3_FHSIZE = 64;
    public CRPCProg() { }
    public abstract int Process(IInputStream pInStream, IOutputStream pOutStream, ProcessParam pParam);
    public virtual void SetLogOn(bool on) => this.m_bLogOn = on;

    protected bool m_bLogOn = false;
    public virtual int PrintLog(string format, params object[] ops)
    {
        if (m_bLogOn) Console.Out.WriteLine(format, ops);
        return 0;
    }

}
