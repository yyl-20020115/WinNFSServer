﻿namespace LibWinNFSServer;
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
    public abstract int Process(IInputStream in_stream, IOutputStream out_stream, ProcessParam parameters);
    public virtual void SetLogOn(bool on) => this.enable_log = on;

    protected bool enable_log = false;
    public virtual int PrintLog(string format = "", params object[] ops)
    {
        if (enable_log) Console.Out.WriteLine(format, ops);
        return 0;
    }
}
