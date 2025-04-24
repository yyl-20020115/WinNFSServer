using LibWinNFSServer;

namespace WinNFSServerRepl;
/// <summary>
///    端口111:  RPC
///    端口2048：用于NFS服务器的NFS mount协议，用于将远程文件系统挂载到本地文件系统。
///    端口4045：用于NFS服务器的lockd进程，用于处理文件锁定操作。
///    端口4046：用于NFS服务器的statd进程，用于处理客户端状态信息。
/// </summary>
public class Program
{
    private static uint UID = 0;
    private static uint GID = 0;
    private static bool UseLog = false;
    private static string FileName = "";
    private static string SocketAddress = "0.0.0.0";
    private static readonly FileTable fileTable = new();
    private static readonly RPCServer RPCServer = new();
    private static readonly PortmapProcedure PortmapProg = new();
    private static readonly NFSProcedure NFSProg = new();
    private static readonly MountProcedure MountProg = new();

    public const int SOCKET_NUM = 3;

    private static void PrintUsage(string exe_path)
    {
        Console.WriteLine($"Usage: {exe_path} [-id <uid> <gid>] [-log on | off] [-pathFile <file>] [-addr <ip>] [export path] [alias path]\n");
        Console.WriteLine($"At least a file or a path is needed");
        Console.WriteLine($"For example:");
        Console.WriteLine($"On Windows> {exe_path} d:\\work");
        Console.WriteLine($"On Linux> mount -t nfs 192.168.12.34:/d/work mount");
        Console.WriteLine($"For another example:");
        Console.WriteLine($"On Windows> {exe_path} d:\\work /exports");
        Console.WriteLine($"On Linux> mount -t nfs 192.168.12.34:/exports");
        Console.WriteLine($"Another example where WinNFSServer is only bound to a specific interface:");
        Console.WriteLine($"On Windows> {exe_path} -addr 192.168.12.34 d:\\work /exports");
        Console.WriteLine($"On Linux> mount - t nfs 192.168.12.34: / exports");
        Console.WriteLine($"Use \".\" to export the current directory (works also for -filePath):");
        Console.WriteLine($"On Windows> {exe_path} . /exports");
    }
    private static void PrintLine()
    {
        Console.WriteLine("=====================================================");
    }

    private static void PrintAbout()
    {
        PrintLine();
        Console.WriteLine("WinNFSServer");
        Console.WriteLine("Network File System Server for Windows");
        Console.WriteLine("Copyright (C) 2005 Ming-Yang Kao");
        Console.WriteLine("Edited in 2011 by ZeWaren");
        Console.WriteLine("Edited in 2013 by Alexander Schneider (Jankowfsky AG)");
        Console.WriteLine("Edited in 2014 2015 by Yann Schepens");
        Console.WriteLine("Edited in 2016 by Peter Philipp (Cando Image GmbH), Marc Harding");
        Console.WriteLine("Edited in 2024 by Yilin Yang (NOC)");

        PrintLine();
    }

    private static void PrintHelp()
    {
        PrintLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("log on/off: display log messages or not");
        Console.WriteLine("about:   display messages about this program");
        Console.WriteLine("help:    display help");
        Console.WriteLine("list:    list mounted clients");
        Console.WriteLine("refresh: refresh the mounted folders");
        Console.WriteLine("reset:   reset the service");
        Console.WriteLine("quit:    quit this program");
        PrintLine();
    }

    private static void PrintCount()
    {
        var n = MountProg?.Clients;
        switch (n)
        {
            case 0:
                Console.WriteLine("There is no client mounted.");
                break;
            case 1:
                Console.WriteLine("There is 1 client mounted.");
                break;
            default:
                Console.WriteLine($"There are {n} clients mounted.");
                break;
        }
    }

    private static void PrintList()
    {
        PrintLine();
        var nNum = MountProg.Clients;

        for (var i = 0; i < nNum; i++)
        {
            Console.WriteLine($"{MountProg?.GetClientAddr(i) ?? ""}");
        }

        PrintCount();
        PrintLine();
    }

    private static void PrintConfirmQuit()
    {
        Console.WriteLine();
        PrintCount();
        Console.Write("Are you sure to quit? (y/N): ");
    }

    private static void MountPaths(List<(string path, string alias)> paths)
    {
        for (var i = 0; i < paths.Count; i++)
        {
            MountProg.Export(paths[i].path, paths[i].alias);  //export path for mount
        }
    }

    private static void InputCommand()
    {
        Console.WriteLine("Type 'help' to see help");
        while (true)
        {
            var command = Console.ReadLine();
            switch (command?.ToLower() ?? "")
            {
                case "about":
                    PrintAbout();
                    break;
                case "help":
                    PrintHelp();
                    break;
                case "log on":
                    Log.EnableLog(true);
                    break;
                case "log off":
                    Log.EnableLog(false);
                    break;
                case "list":
                    PrintList();
                    break;
                case "quit":
                    if (MountProg.Clients == 0)
                    {
                        goto exit_me;
                    }
                    else
                    {
                        PrintConfirmQuit();
                        command = Console.ReadLine();
                        if (command?.ToUpper() == "Y")
                        {
                            goto exit_me;
                        }
                    }
                    break;
                case "refresh":
                    MountProg.Refresh();
                    break;
                case "reset":
                    RPCServer.Reset(PROGS.PROG_NFS);
                    break;
                case "":
                    break;
                default:
                    Console.WriteLine($"Unknown command: '{command}'");
                    Console.WriteLine("Type 'help' to see help");
                    break;
            }
        }
    exit_me:
        return;
    }

    private static void Start(List<(string path, string alias)> paths)
    {
        var DatagramSockets = new DatagramSocket[SOCKET_NUM];
        var ServerSockets = new ServerSocket[SOCKET_NUM];
        var success = false;
        Log.EnableLog(UseLog);

        PortmapProg.Set(PROGS.PROG_MOUNT, (int)NFS_PORTS.MOUNT_PORT);  //map port for mount
        PortmapProg.Set(PROGS.PROG_NFS, (int)NFS_PORTS.NFS_PORT);  //map port for nfs
        NFSProg.SetUserID(UID, GID);  //set uid and gid of files

        MountPaths(paths);

        RPCServer.Set(PROGS.PROG_PORTMAP, PortmapProg);  //program for portmap
        RPCServer.Set(PROGS.PROG_NFS, NFSProg);  //program for nfs
        RPCServer.Set(PROGS.PROG_MOUNT, MountProg);  //program for mount

        for (var i = 0; i < SOCKET_NUM; i++)
        {
            DatagramSockets[i].SetListener(RPCServer);
            ServerSockets[i].SetListener(RPCServer);
        }

        if (ServerSockets[0].Open(SocketAddress, (int)NFS_PORTS.PORTMAP_PORT, 3)
            && DatagramSockets[0].Open(
            SocketAddress,
            (int)NFS_PORTS.PORTMAP_PORT))
        { //start portmap daemon
            Console.WriteLine("Portmap daemon started");

            if (ServerSockets[1].Open(SocketAddress,
                (int)NFS_PORTS.NFS_PORT, 10)
                && DatagramSockets[1].Open(SocketAddress, (int)NFS_PORTS.NFS_PORT))
            { //start nfs daemon
                Console.WriteLine("NFS daemon started");

                if (ServerSockets[2].Open(SocketAddress, (int)NFS_PORTS.MOUNT_PORT, 3)
                 && DatagramSockets[2].Open(SocketAddress, (int)NFS_PORTS.MOUNT_PORT))
                { //start mount daemon
                    Console.WriteLine("Mount daemon started");
                    success = true;  //all daemon started
                }
                else
                {
                    Console.WriteLine("Mount daemon starts failed, check if port 1058 is not already in use.");
                }
            }
            else
            {
                Console.WriteLine("NFS daemon starts failed.");
            }
        }
        else
        {
            Console.WriteLine("Portmap daemon starts failed.");
        }


        if (success)
        {
            Console.WriteLine($"Listening on {SocketAddress}");  //local address
            InputCommand();  //wait for commands
        }

        for (var i = 0; i < SOCKET_NUM; i++)
        {
            DatagramSockets[i].Close();
            ServerSockets[i].Close();
        }
    }

    public static int Main(string[] args)
    {
        PrintAbout();
        if (args.Length < 2)
        {
            PrintUsage(Path.GetFileName(args[0]));
            return 1;
        }

        MountProg.SetFileTable(fileTable);

        List<(string path, string alias)> paths = [];
        UID = GID = 0;
        UseLog = true;
        FileName = "";
        SocketAddress = "0.0.0.0";

        for (var i = 1; i < args.Length; i++)
        {//parse parameters
            if (string.Compare(args[i], "-id", true) == 0)
            {
                UID = uint.TryParse(args[++i], out var u) ? u : 0;
                GID = uint.TryParse(args[++i], out var g) ? g : 0;
            }
            else if (string.Compare(args[i], "-log", true) == 0)
            {
                UseLog = string.Compare(args[++i], "off", true) != 0;
            }
            else if (string.Compare(args[i], "-addr", true) == 0)
            {
                SocketAddress = args[++i];
            }
            else if (string.Compare(args[i], "-pathFile", true) == 0)
            {
                FileName = args[++i];

                if (!MountProg.SetPathFile(FileName))
                {
                    Console.WriteLine($"Can't open file \"{FileName}\".");
                    return 1;
                }
                else
                {
                    MountProg.Refresh();
                    //pathFile = true;
                }
            }
            else if (i == args.Length - 2)
            {
                var path = args[^2];  //path is before the last parameter
                var alias = args[^1]; //path alias is the last parameter
                if (path != null && alias != null)
                    paths.Add((path, alias));

                break;
            }
            else if (i == args.Length - 1)
            {
                var path = args[^1];  //path is the last parameter
                if (path.Length > 0) paths.Add((path, path));
                break;
            }
        }

        Start(paths);

        return 0;
    }
}
