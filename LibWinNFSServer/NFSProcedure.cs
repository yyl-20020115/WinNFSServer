﻿namespace LibWinNFSServer;

public class NFSProcedure : RPCProcedure
{
    private uint uid = 0;
    private uint gid = 0;
    private NFS3Procedure? procedure;

    public NFSProcedure() { }
    public void SetUserID(uint uid, uint gid)
    {
        this.gid= gid;
        this.uid = uid;
    }
    public override int Process(InputStream in_stream, OutputStream out_stream, ProcessParam parameters)
    {
        if (parameters.Version == 3)
        {
            if (procedure == null)
            {
                procedure = new ();
                procedure.SetUserID(uid, gid);
            }

            return procedure.Process(in_stream, out_stream, parameters);
        }
        else
        {
            Log.Print("Client requested NFS version {0} which isn't supported.\n", parameters.Version);
            return (int)PRC_STATUS.PRC_NOTIMP;
        }
    }
    public virtual void SetLogOn(bool bLogOn)
    {
        Log.EnableLog(bLogOn);
    }
}
