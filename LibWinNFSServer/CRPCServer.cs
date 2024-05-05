namespace LibWinNFSServer;

public class CRPCServer : ISocketListener
{
    public const int PROG_NUM = 10;
    public const int MIN_PROG_NUM = 100000;

    protected CRPCProg[] m_pProgTable = new CRPCProg[PROG_NUM];
    protected Mutex m_hMutex = new();

    public CRPCServer()
    {

    }
    public void Set(int nProg, CRPCProg pRPCProg)
    {
        m_pProgTable[nProg - MIN_PROG_NUM] = pRPCProg;
    }
    public void SetLogOn(bool bLogOn)
    {
        for (var i = 0; i < PROG_NUM; i++)
        {
            m_pProgTable[i]?.SetLogOn(bLogOn);
        }
    }
    public void SocketReceived(CSocket pSocket)
    {
        int nResult;

        m_hMutex.WaitOne();
        IInputStream pInStream = pSocket.GetInputStream();

        while (pInStream.GetSize() > 0)
        {
            nResult = Process((int)pSocket.GetType(), pInStream, pSocket.GetOutputStream(), pSocket.GetRemoteAddress());  //process input data
            pSocket?.Send();  //send response

            if (nResult != (int)PRC_STATUS.PRC_OK || pSocket?.GetType() == CSocket.SOCK_DGRAM)
            {
                break;
            }
        }
        m_hMutex.ReleaseMutex();
    }

    public virtual int Process(int nType, IInputStream pInStream, IOutputStream pOutStream, string pRemoteAddr)
    {
        RPC_HEADER header = new();
        int nPos = 0, nSize = 0;
        ProcessParam param = new();
        int nResult;

        nResult = (int)PRC_STATUS.PRC_OK;

        if (nType ==CSocket.SOCK_STREAM)
        {
            pInStream?.Read(ref header.header);
        }

        pInStream?.Read(ref header.XID);
        pInStream?.Read(ref header.msg);
        pInStream?.Read(ref header.rpcvers);  //rpc version
        pInStream?.Read(ref header.prog);  //program
        pInStream?.Read(ref header.vers);  //program version
        pInStream?.Read(ref header.proc);  //procedure
        pInStream?.Read(ref header.cred.flavor);
        pInStream?.Read(ref header.cred.length);
        pInStream?.Skip(header.cred.length);
        pInStream?.Read(ref header.verf.flavor);  //vefifier

        if (pInStream?.Read(ref header.verf.length) < sizeof(uint))
        {
            nResult = (int)PRC_STATUS.PRC_FAIL;
        }

        if (pInStream?.Skip(header.verf.length) < header.verf.length)
        {
            nResult = (int)PRC_STATUS.PRC_FAIL;
        }

        if (nType == CSocket.SOCK_STREAM)
        {
            nPos = pOutStream.GetPosition();  //remember current position
            pOutStream?.Write(header.header);  //this value will be updated later
        }

        pOutStream?.Write(header.XID);
        pOutStream?.Write((int)OPS.REPLY);
        pOutStream?.Write((int)MSGREPS.MSG_ACCEPTED);
        pOutStream?.Write(header.verf.flavor);
        pOutStream?.Write(header.verf.length);

        if (nResult == (int)PRC_STATUS.PRC_FAIL)
        { //input data is truncated
            pOutStream?.Write((int)PROGCS.GARBAGE_ARGS);
        }
        else if (header.prog < MIN_PROG_NUM || header.prog >= MIN_PROG_NUM + PROG_NUM)
        { //program is unavailable
            pOutStream?.Write((int)PROGCS.PROG_UNAVAIL);
        }
        else if (m_pProgTable[header.prog - MIN_PROG_NUM] == null)
        { //program is unavailable
            pOutStream?.Write((int)PROGCS.PROG_UNAVAIL);
        }
        else
        {
            pOutStream?.Write((int)PROGCS.SUCCESS);  //this value may be modified later if process failed
            param.nVersion = header.vers;
            param.nProc = header.proc;
            param.pRemoteAddr = pRemoteAddr;
            nResult = m_pProgTable[header.prog - MIN_PROG_NUM].Process(pInStream, pOutStream, param);  //process rest input data by program

            if (nResult ==(int)PRC_STATUS.PRC_NOTIMP)
            { //procedure is not implemented
                pOutStream?.Seek(-4, SEEKS.SEEK_CUR);
                pOutStream?.Write((int)PROGCS.PROC_UNAVAIL);
            }
            else if (nResult == (int)PRC_STATUS.PRC_FAIL)
            { //input data is truncated
                pOutStream?.Seek(-4, SEEKS.SEEK_CUR);
                pOutStream?.Write((int)PROGCS.GARBAGE_ARGS);
            }
        }

        if (nType == CSocket.SOCK_STREAM)
        {
            nSize = pOutStream.GetPosition();  //remember current position
            pOutStream?.Seek(nPos, SEEKS.SEEK_SET);  //seek to the position of head
            header.header = (uint)(0x80000000 + nSize - nPos - 4);  //size of output data
            pOutStream?.Write(header.header);  //update header
        }

        return nResult;
    }
}

