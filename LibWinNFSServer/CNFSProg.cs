namespace LibWinNFSServer;

public class CNFSProg : CRPCProg
{
    private uint uid = 0;
    private uint gid = 0;
    private CNFS3Prog? prog;

    public CNFSProg() { }
    public void SetUserID(uint uid, uint gid)
    {
        this.gid= gid;
        this.uid = uid;
    }
    public override int Process(IInputStream in_stream, IOutputStream out_stream, ProcessParam parameters)
    {
        if (parameters.nVersion == 3)
        {
            if (prog == null)
            {
                prog = new ();
                prog.SetUserID(uid, gid);
                prog.SetLogOn(enable_log);
            }

            return prog.Process(in_stream, out_stream, parameters);
        }
        else
        {
            PrintLog("Client requested NFS version {0} which isn't supported.\n", parameters.nVersion);
            return (int)PRC_STATUS.PRC_NOTIMP;
        }
    }
    public override void SetLogOn(bool bLogOn)
    {
        base.SetLogOn(bLogOn);
        prog?.SetLogOn(bLogOn);
    }
}
