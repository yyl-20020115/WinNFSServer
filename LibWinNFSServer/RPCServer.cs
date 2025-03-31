namespace LibWinNFSServer;

public class RPCServer : SocketListener
{
    public const int PROG_NUM = 10;
    public const int MIN_PROG_NUM = 100000;

    protected RPCProcedure[] procedures = new RPCProcedure[PROG_NUM];
    protected Mutex mutex = new();

    public RPCServer()
    {

    }
    public void Reset(PROGS prog)
    {
        Set(prog, null);
    }

    public void Set(PROGS prog, RPCProcedure? rpc)
    {
        procedures[(int)prog - MIN_PROG_NUM] = rpc;
    }
    public void SetLogOn(bool bLogOn)
    {
        for (var i = 0; i < procedures.Length; i++)
        {
            procedures[i]?.SetLogOn(bLogOn);
        }
    }
    public void SocketReceived(ThreadSocket socket)
    {
        int nResult;

        mutex.WaitOne();
        var ins = socket.InputStream;

        while (ins.GetSize() > 0)
        {
            nResult = Process((int)socket.SocketType, ins, socket.OutputStream, socket.RemoteAddress);  //process input data
            socket?.Send();  //send response

            if (nResult != (int)PRC_STATUS.PRC_OK || socket?.SocketType == ThreadSocket.SOCK_DGRAM)
            {
                break;
            }
        }
        mutex.ReleaseMutex();
    }

    public virtual int Process(int type, InputStream ins, OutputStream outs, string remote_address)
    {
        RPC_HEADER header = new();
        int pos = 0;
        ProcessParam param = new();

        int result = (int)PRC_STATUS.PRC_OK;

        if (type == ThreadSocket.SOCK_STREAM)
        {
            ins?.Read(out header.Header);
        }

        ins?.Read(out header.XID);
        ins?.Read(out header.Msg);
        ins?.Read(out header.Rpcvers);  //rpc version
        ins?.Read(out header.Prog);  //program
        ins?.Read(out header.Vers);  //program version
        ins?.Read(out header.Proc);  //procedure
        ins?.Read(out header.Credential.Flavor);
        ins?.Read(out header.Credential.Length);
        ins?.Skip(header.Credential.Length);
        ins?.Read(out header.Verification.Flavor);  //vefifier

        if (ins?.Read(out header.Verification.Length) < sizeof(uint))
        {
            result = (int)PRC_STATUS.PRC_FAIL;
        }

        if (ins?.Skip(header.Verification.Length) < header.Verification.Length)
        {
            result = (int)PRC_STATUS.PRC_FAIL;
        }

        if (type == ThreadSocket.SOCK_STREAM)
        {
            pos = outs.Position;  //remember current position
            outs?.Write(header.Header);  //this value will be updated later
        }

        outs?.Write(header.XID);
        outs?.Write((int)OPS.REPLY);
        outs?.Write((int)MSGREPS.MSG_ACCEPTED);
        outs?.Write(header.Verification.Flavor);
        outs?.Write(header.Verification.Length);

        if (result == (int)PRC_STATUS.PRC_FAIL)
        { //input data is truncated
            outs?.Write((int)PROG_RESULT.GARBAGE_ARGS);
        }
        else if (header.Prog < MIN_PROG_NUM || header.Prog >= MIN_PROG_NUM + PROG_NUM)
        { //program is unavailable
            outs?.Write((int)PROG_RESULT.PROG_UNAVAIL);
        }
        else if (procedures[header.Prog - MIN_PROG_NUM] == null)
        { //program is unavailable
            outs?.Write((int)PROG_RESULT.PROG_UNAVAIL);
        }
        else
        {
            outs?.Write((int)PROG_RESULT.SUCCESS);  //this value may be modified later if process failed
            param.Version = header.Vers;
            param.Procedure = header.Proc;
            param.RemoteAddress = remote_address;
            result = procedures[header.Prog - MIN_PROG_NUM].Process(ins, outs, param);  //process rest input data by program

            if (result == (int)PRC_STATUS.PRC_NOTIMP)
            { //procedure is not implemented
                outs?.Seek(-4, SEEKS.SEEK_CUR);
                outs?.Write((int)PROG_RESULT.PROC_UNAVAIL);
            }
            else if (result == (int)PRC_STATUS.PRC_FAIL)
            { //input data is truncated
                outs?.Seek(-4, SEEKS.SEEK_CUR);
                outs?.Write((int)PROG_RESULT.GARBAGE_ARGS);
            }
        }

        if (type == ThreadSocket.SOCK_STREAM)
        {
            int nSize = outs.Position;  //remember current position
            outs?.Seek(pos, SEEKS.SEEK_SET);  //seek to the position of head
            header.Header = (uint)(0x80000000 + nSize - pos - 4);  //size of output data
            outs?.Write(header.Header);  //update header
        }

        return result;
    }
}

