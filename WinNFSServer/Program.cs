using LibWinNFSServer;

namespace WinNFSServer;

public class Program
{
    private static uint UID = 0;
    private static uint GID = 0;
    private static bool UseLog = false;
    private static string FileName = "";
    private static string SocketAddress ="0.0.0.0";
    private static CFileTable fileTable = new ();
    private static readonly CRPCServer RPCServer = new();
    private static readonly CPortmapProg PortmapProg = new();
    private static readonly CNFSProg NFSProg = new();
    private static readonly CMountProg MountProg = new(fileTable);

    public const int SOCKET_NUM = 3;

    private static void PrintUsage(string pExe)
    {
        Console.WriteLine("Usage: {0} [-id <uid> <gid>] [-log on | off] [-pathFile <file>] [-addr <ip>] [export path] [alias path]\n", pExe);
        Console.WriteLine("At least a file or a path is needed");
        Console.WriteLine("For example:");
        Console.WriteLine("On Windows> {0} d:\\work\n", pExe);
        Console.WriteLine("On Linux> mount -t nfs 192.168.12.34:/d/work mount\n");
        Console.WriteLine("For another example:");
        Console.WriteLine("On Windows> {0} d:\\work /exports", pExe);
        Console.WriteLine("On Linux> mount -t nfs 192.168.12.34:/exports\n");
        Console.WriteLine("Another example where WinNFSd is only bound to a specific interface:");
        Console.WriteLine("On Windows> {0} -addr 192.168.12.34 d:\\work /exports", pExe);
        Console.WriteLine("On Linux> mount - t nfs 192.168.12.34: / exports\n");
        Console.WriteLine("Use \".\" to export the current directory (works also for -filePath):");
        Console.WriteLine("On Windows> {0} . /exports", pExe);
    }
    private static void PrintLine()
    {
        Console.WriteLine("=====================================================");
    }

    private static void PrintAbout()
    {
        PrintLine();
        Console.WriteLine("WinNFSd {{VERSION}} [{{HASH}}]");
        Console.WriteLine("Network File System server for Windows");
        Console.WriteLine("Copyright (C) 2005 Ming-Yang Kao");
        Console.WriteLine("Edited in 2011 by ZeWaren");
        Console.WriteLine("Edited in 2013 by Alexander Schneider (Jankowfsky AG)");
        Console.WriteLine("Edited in 2014 2015 by Yann Schepens");
        Console.WriteLine("Edited in 2016 by Peter Philipp (Cando Image GmbH), Marc Harding");
        PrintLine();
    }

    private static void PrintHelp()
    {
        PrintLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("about: display messages about this program");
        Console.WriteLine("help: display help");
        Console.WriteLine("log on/off: display log messages or not");
        Console.WriteLine("list: list mounted clients");
        Console.WriteLine("refresh: refresh the mounted folders");
        Console.WriteLine("reset: reset the service");
        Console.WriteLine("quit: quit this program");
        PrintLine();
    }

    private static void PrintCount()
    {
        var nNum = MountProg?.GetMountNumber();

        if (nNum == 0)
        {
            Console.WriteLine("There is no client mounted.");
        }
        else if (nNum == 1)
        {
            Console.WriteLine("There is 1 client mounted.");
        }
        else
        {
            Console.WriteLine("There are {0} clients mounted.", nNum);
        }
    }

    private static void PrintList()
    {
        PrintLine();
        var nNum = MountProg.GetMountNumber();

        for (var i = 0; i < nNum; i++)
        {
            Console.WriteLine("{0}", MountProg?.GetClientAddr(i)??"");
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
        int i;
        var numberOfElements = paths.Count;

        for (i = 0; i < numberOfElements; i++)
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

            if (string.Compare(command, "about", true) == 0)
            {
                PrintAbout();
            }
            else if (string.Compare(command, "help", true) == 0)
            {
                PrintHelp();
            }
            else if (string.Compare(command, "log on", true) == 0)
            {
                RPCServer.SetLogOn(true);
            }
            else if (string.Compare(command, "log off", true) == 0)
            {
                RPCServer.SetLogOn(false);
            }
            else if (string.Compare(command, "list", true) == 0)
            {
                PrintList();
            }
            else if (string.Compare(command, "quit", true) == 0)
            {
                if (MountProg.GetMountNumber() == 0)
                {
                    break;
                }
                else
                {
                    PrintConfirmQuit();
                    command = Console.ReadLine();
                    if (command?.ToUpper() == "Y")
                    {
                        break;
                    }
                }
            }
            else if (string.Compare(command, "refresh", true) == 0)
            {
                MountProg.Refresh();
            }
            else if (string.Compare(command, "reset", true) == 0)
            {
                RPCServer.Set((int)PROGS.PROG_NFS, null);
            }
            else if (command != "")
            {
                Console.WriteLine("Unknown command: '{0}'", command);
                Console.WriteLine("Type 'help' to see help");
            }
        }
    }

    private static void Start(List<(string path, string alias)> paths)
    {
        int i;
        CDatagramSocket[] DatagramSockets = new CDatagramSocket[SOCKET_NUM];
        CServerSocket[] ServerSockets = new CServerSocket[SOCKET_NUM];
        bool bSuccess;

        PortmapProg.Set((int)PROGS.PROG_MOUNT, (int)NFS_PORTS.MOUNT_PORT);  //map port for mount
        PortmapProg.Set((int)PROGS.PROG_NFS, (int)NFS_PORTS.NFS_PORT);  //map port for nfs
        NFSProg.SetUserID(UID, GID);  //set uid and gid of files

        MountPaths(paths);

        RPCServer.Set((int)PROGS.PROG_PORTMAP, PortmapProg);  //program for portmap
        RPCServer.Set((int)PROGS.PROG_NFS, NFSProg);  //program for nfs
        RPCServer.Set((int)PROGS.PROG_MOUNT, MountProg);  //program for mount
        RPCServer.SetLogOn(UseLog);

        for (i = 0; i < SOCKET_NUM; i++)
        {
            DatagramSockets[i].SetListener(RPCServer);
            ServerSockets[i].SetListener(RPCServer);
        }

        bSuccess = false;

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
                    && DatagramSockets[2].Open(SocketAddress,(int)NFS_PORTS.MOUNT_PORT))
                { //start mount daemon
                    Console.WriteLine("Mount daemon started");
                    bSuccess = true;  //all daemon started
                }
                else
                {
                    Console.WriteLine("Mount daemon starts failed (check if port 1058 is not already in use ;) ).");
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


        if (bSuccess)
        {
            Console.WriteLine("Listening on {0}", SocketAddress);  //local address
            InputCommand();  //wait for commands
        }

        for (i = 0; i < SOCKET_NUM; i++)
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

        List<(string path, string alias)> paths = [];
        UID = GID = 0;
        UseLog = true;
        FileName = "";
        SocketAddress = "0.0.0.0";

        for (int i = 1; i < args.Length; i++)
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
                    Console.WriteLine("Can't open file {0}.", FileName);
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
