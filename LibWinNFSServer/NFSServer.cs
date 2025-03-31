namespace LibWinNFSServer;

public class NFSServer
{
    public uint UID = 0;
    public uint GID = 0;
    public bool UseLog = false;
    public string FileName = "";
    public string SocketAddress = "0.0.0.0";
    private const int SOCKET_NUM = 3;
    private FileTable? fileTable = new();
    private RPCServer? RPCServer = new();
    private PortmapProcedure? PortmapProg = new();
    private NFSProcedure? NFSProg = new();
    private MountProcedure? MountProg = new();
    private readonly DatagramSocket[] _DatagramSockets = new DatagramSocket[SOCKET_NUM];
    private readonly ServerSocket[] _ServerSockets = new ServerSocket[SOCKET_NUM];


    public NFSServer() { }

    private void MountPaths(List<(string path, string alias)> paths)
    {
        for (var i = 0; i < paths.Count; i++)
            MountProg?.Export(paths[i].path, paths[i].alias);  //export path for mount
    }

    public void Stop()
    {
        for (var i = 0; i < SOCKET_NUM; i++)
        {
            _DatagramSockets[i].Close();
            _ServerSockets[i].Close();
        }
        this.fileTable = null;
        this.RPCServer = null;
        this.PortmapProg = null;
        this.NFSProg = null;
        this.MountProg = null;

    }
    public bool Start(List<(string path, string alias)> paths,
        int NFSPort = (int)NFS_PORTS.NFS_PORT,
        int MountPort = (int)NFS_PORTS.MOUNT_PORT,
        int RPCPort = (int)NFS_PORTS.PORTMAP_PORT)
    {
        var success = false;

        this.fileTable = new();
        this.RPCServer = new();
        this.PortmapProg = new();
        this.NFSProg = new();
        this.MountProg = new(this.fileTable);

        PortmapProg.Set(PROGS.PROG_MOUNT,MountPort);  //map port for mount
        PortmapProg.Set(PROGS.PROG_NFS, NFSPort);  //map port for nfs
        NFSProg.SetUserID(UID, GID);  //set uid and gid of files

        MountPaths(paths);

        RPCServer.Set(PROGS.PROG_PORTMAP, PortmapProg);  //program for portmap
        RPCServer.Set(PROGS.PROG_NFS, NFSProg);  //program for nfs
        RPCServer.Set(PROGS.PROG_MOUNT, MountProg);  //program for mount
        Log.EnableLog(UseLog);

        for (var i = 0; i < SOCKET_NUM; i++)
        {
            _DatagramSockets[i].SetListener(RPCServer);
            _ServerSockets[i].SetListener(RPCServer);
        }

        if (_ServerSockets[0].Open(SocketAddress, RPCPort, 3)
            && _DatagramSockets[0].Open(SocketAddress, RPCPort))
        { 
            if (_ServerSockets[1].Open(SocketAddress, NFSPort, 10)
                && _DatagramSockets[1].Open(SocketAddress, NFSPort))
            { 
                if (_ServerSockets[2].Open(SocketAddress, MountPort, 3)
                    && _DatagramSockets[2].Open(SocketAddress, MountPort))
                { 
                    success = true;  //all daemon started
                }
            }
        }
        return success;
    }
}
