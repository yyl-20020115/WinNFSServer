namespace LibWinNFSServer;

public static class Log
{
    public static void EnableLog(bool on) => Enabled = on;
   
    public static bool Enabled { get; private set; } = false;

    public static int Print(string format = "", params object[] ops)
    {
        if (Enabled)
            Console.WriteLine(format, ops);
        return 0;
    }

}
