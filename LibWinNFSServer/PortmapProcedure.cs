namespace LibWinNFSServer;

public class PortmapProcedure :  RPCProcedure
{
    public const int PORT_NUM = 10;
    public const int MIN_PROG_NUM = 100000;

    protected uint[] ports = new uint[PORT_NUM];
    protected InputStream? ins;
    protected OutputStream? outs;

    private ProcessParam parameters = new();
    private PRC_STATUS result = PRC_STATUS.PRC_OK;

    public PortmapProcedure() { }
    public void Set(PROGS prog, NFS_PORTS port) 
        => this.ports[(int)prog - MIN_PROG_NUM] = (uint)port;

    public override int Process(InputStream in_stream, OutputStream out_stream, ProcessParam parameters)
    {
        PROC[] procs = [
            NULL, 
            SET, 
            UNSET,
            GETPORT,
            DUMP,
            CALLIT
        ];

        Log.Print("PORTMAP ");

        if (parameters.Procedure >= procs.Length)
        {
            Log.Print("NOIMP");
            Log.Print("\n");
            return (int)(result = PRC_STATUS.PRC_NOTIMP);
        }

        this.ins = in_stream;
        this.outs = out_stream;
        this.parameters = parameters;
        result = PRC_STATUS.PRC_OK;
        procs[parameters.Procedure]();
        Log.Print("\n");

        return (int)result;
    }

    protected void NULL()
    {
        Log.Print("NULL");
    }

    protected void SET()
    {
        Log.Print("SET - NOIMP");
        result = PRC_STATUS.PRC_NOTIMP;
    }

    protected void UNSET()
    {
        Log.Print("UNSET - NOIMP");
        result = PRC_STATUS.PRC_NOTIMP;
    }
    protected void GETPORT()
    {
        PORTMAP_HEADER header = new();
        Log.Print("GETPORT");
        ins?.Read(out header.Procedure);  //program
        ins?.Skip(12);
        uint port = header.Procedure >= MIN_PROG_NUM && header.Procedure < MIN_PROG_NUM + PORT_NUM
            ? ports[header.Procedure - MIN_PROG_NUM] 
            : 0;
        Log.Print($" {header.Procedure} {port}");
        outs?.Write(port);  //port

    }

    protected void DUMP()
    {
        Log.Print("DUMP");

        Write(PROG_PORTS.PROG_PORTMAP, 2, IPPROTOS.IPPROTO_TCP, PPORTS.PORTMAP_PORT);
        Write(PROG_PORTS.PROG_PORTMAP, 2, IPPROTOS.IPPROTO_UDP, PPORTS.PORTMAP_PORT);
        Write(PROG_PORTS.PROG_NFS, 3, IPPROTOS.IPPROTO_TCP, PPORTS.NFS_PORT);
        Write(PROG_PORTS.PROG_NFS, 3, IPPROTOS.IPPROTO_UDP, PPORTS.NFS_PORT);
        Write(PROG_PORTS.PROG_MOUNT, 3, IPPROTOS.IPPROTO_TCP, PPORTS.MOUNT_PORT);
        Write(PROG_PORTS.PROG_MOUNT, 3, IPPROTOS.IPPROTO_UDP, PPORTS.MOUNT_PORT);

        outs?.Write(0);
    }

    protected void CALLIT()
    {
        Log.Print("CALLIT - NOIMP");
        result = PRC_STATUS.PRC_NOTIMP;
    }

    private void Write(PROG_PORTS prog, uint vers, IPPROTOS proto, PPORTS port)
    {
        outs?.Write(1);
        outs?.Write((uint)prog);
        outs?.Write(vers);
        outs?.Write((uint)proto);
        outs?.Write((uint)port);
    }
}