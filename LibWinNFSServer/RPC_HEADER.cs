namespace LibWinNFSServer;

struct RPC_HEADER
{
    public uint header;
    public uint XID;
    public uint msg;
    public uint rpcvers;
    public uint prog;
    public uint vers;
    public uint proc;
    public OPAQUE_AUTH cred;
    public OPAQUE_AUTH verf;
}

