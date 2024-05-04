namespace LibWinNFSServer;

public class CPortmapProg :  CRPCProg
{
    public const int PORT_NUM = 10;
    public const int MIN_PROG_NUM = 100000;

    protected uint[] m_nPortTable = new uint[PORT_NUM];
    protected IInputStream? m_pInStream;
    protected IOutputStream? m_pOutStream;

    private ProcessParam m_pParam = new();
    private PRC_STATUS m_nResult = PRC_STATUS.PRC_OK;

    public CPortmapProg() { }
    public void Set(uint nProg, uint nPort)
    {
        m_nPortTable[nProg - MIN_PROG_NUM] = nPort;
    }
    delegate void PPROC();
 
    public override int Process(IInputStream pInStream, IOutputStream pOutStream, ProcessParam pParam)
    {
        PPROC[] pf = [
            ProcedureNULL, ProcedureSET, ProcedureUNSET,
            ProcedureGETPORT, ProcedureDUMP, ProcedureCALLIT
        ];

        PrintLog("PORTMAP ");

        if (pParam.nProc >= pf.Length)
        {
            PrintLog("NOIMP");
            PrintLog("\n");
            return (int)(m_nResult = PRC_STATUS.PRC_NOTIMP);
        }

        m_pInStream = pInStream;
        m_pOutStream = pOutStream;
        m_pParam = pParam;
        m_nResult = PRC_STATUS.PRC_OK;
        pf[pParam.nProc]();
        PrintLog("\n");

        return (int)m_nResult;
    }


    protected void ProcedureNOIMP()
    {
        PrintLog("NOIMP");
        m_nResult =PRC_STATUS.PRC_NOTIMP;
    }
    protected void ProcedureNULL()
    {
        PrintLog("NULL");
    }

    protected void ProcedureSET()
    {
        PrintLog("SET - NOIMP");
        m_nResult = PRC_STATUS.PRC_NOTIMP;
    }

    protected void ProcedureUNSET()
    {
        PrintLog("UNSET - NOIMP");
        m_nResult = PRC_STATUS.PRC_NOTIMP;
    }
    protected void ProcedureGETPORT()
    {
        PORTMAP_HEADER header = new();
        uint nPort;

        PrintLog("GETPORT");
        m_pInStream?.Read(ref header.prog);  //program
        m_pInStream?.Skip(12);
        nPort = header.prog >= MIN_PROG_NUM && header.prog < MIN_PROG_NUM + PORT_NUM
            ? m_nPortTable[header.prog - MIN_PROG_NUM] 
            : 0;
        PrintLog(" {0} {1}", header.prog, nPort);
        m_pOutStream?.Write(nPort);  //port

    }

    protected void ProcedureDUMP()
    {
        PrintLog("DUMP");

        Write(PROG_PORTS.PROG_PORTMAP, 2, IPPROTOS.IPPROTO_TCP, PPORTS.PORTMAP_PORT);
        Write(PROG_PORTS.PROG_PORTMAP, 2, IPPROTOS.IPPROTO_UDP, PPORTS.PORTMAP_PORT);
        Write(PROG_PORTS.PROG_NFS, 3, IPPROTOS.IPPROTO_TCP, PPORTS.NFS_PORT);
        Write(PROG_PORTS.PROG_NFS, 3, IPPROTOS.IPPROTO_UDP, PPORTS.NFS_PORT);
        Write(PROG_PORTS.PROG_MOUNT, 3, IPPROTOS.IPPROTO_TCP, PPORTS.MOUNT_PORT);
        Write(PROG_PORTS.PROG_MOUNT, 3, IPPROTOS.IPPROTO_UDP, PPORTS.MOUNT_PORT);

        m_pOutStream?.Write(0);

    }

    protected void ProcedureCALLIT()
    {
        PrintLog("CALLIT - NOIMP");
        m_nResult = PRC_STATUS.PRC_NOTIMP;
    }

    private void Write(PROG_PORTS prog, uint vers, IPPROTOS proto, PPORTS port)
    {
        m_pOutStream?.Write(1);
        m_pOutStream?.Write((uint)prog);
        m_pOutStream?.Write(vers);
        m_pOutStream?.Write((uint)proto);
        m_pOutStream?.Write((uint)port);
    }
}