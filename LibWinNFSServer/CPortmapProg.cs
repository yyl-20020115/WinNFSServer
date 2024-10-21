namespace LibWinNFSServer;

public class CPortmapProg :  CRPCProg
{
    public const int PORT_NUM = 10;
    public const int MIN_PROG_NUM = 100000;

    protected uint[] port_table = new uint[PORT_NUM];
    protected IInputStream? in_stream;
    protected IOutputStream? out_stream;

    private ProcessParam parameters = new();
    private PRC_STATUS result = PRC_STATUS.PRC_OK;

    public CPortmapProg() { }
    public void Set(PROGS prog, NFS_PORTS port) => this.port_table[(int)prog - MIN_PROG_NUM] = (uint)port;

    public override int Process(IInputStream in_stream, IOutputStream out_stream, ProcessParam parameters)
    {
        PPROC[] procs = [
            NULL, 
            SET, 
            UNSET,
            GETPORT,
            DUMP,
            CALLIT
        ];

        PrintLog("PORTMAP ");

        if (parameters.nProc >= procs.Length)
        {
            PrintLog("NOIMP");
            PrintLog("\n");
            return (int)(result = PRC_STATUS.PRC_NOTIMP);
        }

        this.in_stream = in_stream;
        this.out_stream = out_stream;
        this.parameters = parameters;
        result = PRC_STATUS.PRC_OK;
        procs[parameters.nProc]();
        PrintLog("\n");

        return (int)result;
    }

    protected void NULL()
    {
        PrintLog("NULL");
    }

    protected void SET()
    {
        PrintLog("SET - NOIMP");
        result = PRC_STATUS.PRC_NOTIMP;
    }

    protected void UNSET()
    {
        PrintLog("UNSET - NOIMP");
        result = PRC_STATUS.PRC_NOTIMP;
    }
    protected void GETPORT()
    {
        PORTMAP_HEADER header = new();
        PrintLog("GETPORT");
        in_stream?.Read(out header.prog);  //program
        in_stream?.Skip(12);
        uint nPort = header.prog >= MIN_PROG_NUM && header.prog < MIN_PROG_NUM + PORT_NUM
            ? port_table[header.prog - MIN_PROG_NUM] 
            : 0;
        PrintLog(" {0} {1}", header.prog, nPort);
        out_stream?.Write(nPort);  //port

    }

    protected void DUMP()
    {
        PrintLog("DUMP");

        Write(PROG_PORTS.PROG_PORTMAP, 2, IPPROTOS.IPPROTO_TCP, PPORTS.PORTMAP_PORT);
        Write(PROG_PORTS.PROG_PORTMAP, 2, IPPROTOS.IPPROTO_UDP, PPORTS.PORTMAP_PORT);
        Write(PROG_PORTS.PROG_NFS, 3, IPPROTOS.IPPROTO_TCP, PPORTS.NFS_PORT);
        Write(PROG_PORTS.PROG_NFS, 3, IPPROTOS.IPPROTO_UDP, PPORTS.NFS_PORT);
        Write(PROG_PORTS.PROG_MOUNT, 3, IPPROTOS.IPPROTO_TCP, PPORTS.MOUNT_PORT);
        Write(PROG_PORTS.PROG_MOUNT, 3, IPPROTOS.IPPROTO_UDP, PPORTS.MOUNT_PORT);

        out_stream?.Write(0);
    }

    protected void CALLIT()
    {
        PrintLog("CALLIT - NOIMP");
        result = PRC_STATUS.PRC_NOTIMP;
    }

    private void Write(PROG_PORTS prog, uint vers, IPPROTOS proto, PPORTS port)
    {
        out_stream?.Write(1);
        out_stream?.Write((uint)prog);
        out_stream?.Write(vers);
        out_stream?.Write((uint)proto);
        out_stream?.Write((uint)port);
    }
}