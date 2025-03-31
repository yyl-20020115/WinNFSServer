using System.Text;
using System.Runtime.InteropServices;

namespace LibWinNFSServer;

public enum FSF3S : int
{
    FSF3_LINK = 0x0001,
    FSF3_SYMLINK = 0x0002,
    FSF3_HOMOGENEOUS = 0x0008,
    FSF3_CANSETTIME = 0x0010
}

public enum SYNC_MODES : int
{
    UNSTABLE = 0,
    DATA_SYNC = 1,
    FILE_SYNC = 2
}

public enum ACCESS3S : int
{
    ACCESS3_READ = 0x0001,
    ACCESS3_LOOKUP = 0x0002,
    ACCESS3_MODIFY = 0x0004,
    ACCESS3_EXTEND = 0x0008,
    ACCESS3_DELETE = 0x0010,
    ACCESS3_EXECUTE = 0x0020
}
public enum STAS : int
{
    UNCHECKED = 0,
    GUARDED = 1,
    EXCLUSIVE = 2
}

public enum SEEKS : int
{
    SEEK_SET = 0,
    SEEK_CUR = 1,
    SEEK_END = 2
}

public enum TIME_SETS : int
{
    DONT_CHANGE = 0,
    SET_TO_SERVER_TIME = 1,
    SET_TO_CLIENT_TIME = 2
}

public class CreateHow3
{
    public STAS Mode = 0;
    public Sattr3 Obj_Attributes = new();
    public ulong Verification = 0;
}

public class CACHE_LIST
{
    public FILE_ITEM? Item;
    public CACHE_LIST? Next;
}

public class WccData
{
    public PreOpAttr Before = new();
    public PostOpAttr After = new();
}

public class SetATime
{
    public TIME_SETS DoSet = 0;
    public NfsTime3 ATime = new();
}

public class Sattrguard3
{
    public bool Check = false;
    public NfsTime3 Obj_CTime = new();
}
public class SetAtime
{
    public TIME_SETS SetIt = 0;
    public NfsTime3 ATime = new();
}
public class SetMtime
{
    public TIME_SETS SetIt = 0;
    public NfsTime3 MTime = new();
}

public class Sattr3
{
    public SetMode3 Mode = new();
    public SetUid3 Uid = new();
    public SetGid3 Gid = new();
    public SetSize3 Size = new();
    public SetAtime ATime = new();
    public SetMtime MTime = new();
}

public class SymLinkData3
{
    public Sattr3 Symlink_Attributes = new();
    public NfsPath3 Symlink_Data = new();
}
public class Specdata3
{
    public uint SpecData1 = 0;
    public uint SpecData2 = 0;
}

public class SetGid3
{
    public bool SetIt = false;
    public uint Gid = 0;
}

public class Fattr3
{
    public uint Type = 0;
    public uint Mode = 0;
    public uint NLink = 0;
    public uint Uid = 0;
    public uint Gid = 0;
    public ulong Size = 0;
    public ulong Used = 0;
    public Specdata3 Rdev = new();
    public ulong Fsid = 0;
    public ulong FileId = 0;
    public NfsTime3 ATime = new();
    public NfsTime3 MTime = new();
    public NfsTime3 CTime = new();
}

public class Filename3 : Opaque
{
    public string? Name = null;
    public Filename3() { }
    public override void SetSize(uint length)
        => this.Length = length;
    public void Set(string text) 
        => this.Contents = Encoding.UTF8.GetBytes(Name = text);
}

public class DirOpArgs3
{
    public NfsFh3? Dir = new();
    public Filename3? Name = new();
}

public class FILE_ITEM
{
    public string Path = "";
    public int PathLength = 0;
    public byte[] Handle = new byte[64]; //64
    public bool IsCached = false;
}

public class FILE_TABLE
{
    public const int TABLE_SIZE = 1024;
    public TreeNode<FILE_ITEM>[] Items = new TreeNode<FILE_ITEM>[TABLE_SIZE];
    public FILE_TABLE? Next = null;
}
public struct FILETIME
{
    public uint LowDateTime;
    public uint HighDateTime;
}
public enum IPPROTOS : int
{
    IPPROTO_TCP = 6,
    IPPROTO_UDP = 17
}

public enum MAP_PROCS : int
{
    MAPPROC_NULL = 0,
    MAPPROC_SET = 1,
    MAPPROC_UNSET = 2,
    MAPPROC_GETPORT = 3,
    MAPPROC_DUMP = 4,
    MAPPROC_CALLIT = 5
}

public enum MNTS : int
{
    MNT_OK = 0,
    MNTERR_PERM = 1,
    MNTERR_NOENT = 2,
    MNTERR_IO = 5,
    MNTERR_ACCESS = 13,
    MNTERR_NOTDIR = 20,
    MNTERR_INVAL = 22
}

public enum MOUNT_PROCS : int
{
    MOUNTPROC_NULL = 0,
    MOUNTPROC_MNT = 1,
    MOUNTPROC_DUMP = 2,
    MOUNTPROC_UMNT = 3,
    MOUNTPROC_UMNTALL = 4,
    MOUNTPROC_EXPORT = 5
}

public enum MSGREPS : int
{
    MSG_ACCEPTED = 0,
    MSG_DENIED = 1
}

public enum NFS3S : int
{
    NFS3_OK = 0,
    NFS3ERR_PERM = 1,
    NFS3ERR_NOENT = 2,
    NFS3ERR_IO = 5,
    NFS3ERR_NXIO = 6,
    NFS3ERR_ACCES = 13,
    NFS3ERR_EXIST = 17,
    NFS3ERR_XDEV = 18,
    NFS3ERR_NODEV = 19,
    NFS3ERR_NOTDIR = 20,
    NFS3ERR_ISDIR = 21,
    NFS3ERR_INVAL = 22,
    NFS3ERR_FBIG = 27,
    NFS3ERR_NOSPC = 28,
    NFS3ERR_ROFS = 30,
    NFS3ERR_MLINK = 31,
    NFS3ERR_NAMETOOLONG = 63,
    NFS3ERR_NOTEMPTY = 66,
    NFS3ERR_DQUOT = 69,
    NFS3ERR_STALE = 70,
    NFS3ERR_REMOTE = 71,
    NFS3ERR_BADHANDLE = 10001,
    NFS3ERR_NOT_SYNC = 10002,
    NFS3ERR_BAD_COOKIE = 10003,
    NFS3ERR_NOTSUPP = 10004,
    NFS3ERR_TOOSMALL = 10005,
    NFS3ERR_SERVERFAULT = 10006,
    NFS3ERR_BADTYPE = 10007,
    NFS3ERR_JUKEBOX = 10008
}

public enum NF3S : uint
{
    NF3REG = 1,
    NF3DIR = 2,
    NF3BLK = 3,
    NF3CHR = 4,
    NF3LNK = 5,
    NF3SOCK = 6,
    NF3FIFO = 7
}

public enum NFS_PORTS : uint
{
    PORTMAP_PORT = 111,
    MOUNT_PORT = 1058,
    NFS_PORT = 2049
}

public class NfsFh3 : Opaque
{
    public NfsFh3() : base(FileTable.NFS3_FHSIZE) { }
}

public class WccAttribute
{
    public ulong Size = 0;
    public NfsTime3 MTime = new();
    public NfsTime3 CTime = new();
}

public class NfsPath3 : Opaque
{
    public string? Path = null;
    public NfsPath3() { }
    public override void SetSize(uint len) { }
    public void Set(string text) => this.Contents = Encoding.UTF8.GetBytes(Path = text);
}


public class SetUid3
{
    public bool SetIt = false;
    public uint Uid = 0;
}

public class SetSize3
{
    public bool SetIt = false;
    public ulong Size = 0;
}

public class SetMode3
{
    public bool SetIt = false;
    public uint Mode = 0;
}

public enum NFS_PROC3S : int
{
    NFSPROC3_NULL = 0,
    NFSPROC3_GETATTR = 1,
    NFSPROC3_SETATTR = 2,
    NFSPROC3_LOOKUP = 3,
    NFSPROC3_ACCESS = 4,
    NFSPROC3_READLINK = 5,
    NFSPROC3_READ = 6,
    NFSPROC3_WRITE = 7,
    NFSPROC3_CREATE = 8,
    NFSPROC3_MKDIR = 9,
    NFSPROC3_SYMLINK = 10,
    NFSPROC3_MKNOD = 11,
    NFSPROC3_REMOVE = 12,
    NFSPROC3_RMDIR = 13,
    NFSPROC3_RENAME = 14,
    NFSPROC3_LINK = 15,
    NFSPROC3_READDIR = 16,
    NFSPROC3_READDIRPLUS = 17,
    NFSPROC3_FSSTAT = 18,
    NFSPROC3_FSINFO = 19,
    NFSPROC3_PATHCONF = 20,
    NFSPROC3_COMMIT = 21
}
public class NfsTime3
{
    public uint Seconds = 0;
    public uint NSeconds = 0;
}

public class Opaque
{
    public uint Length = 0;
    public byte[]? Contents = null;

    public Opaque() { }
    public Opaque(uint length) => this.SetSize(length);
    public virtual void SetSize(uint length)
        => this.Contents = new byte[this.Length = length];
}

public class OPAQUE_AUTH
{
    public uint Flavor = 0;
    public uint Length = 0;
}

public enum OPS : int
{
    CALL = 0,
    REPLY = 1
}

public class PORTMAP_HEADER
{
    public uint Procedure = 0;
    public uint Vers = 0;
    public uint Proto = 0;
    public uint Port = 0;
}
public enum PathFormats : int
{
    FORMAT_PATH = 1,
    FORMAT_PATHALIAS = 2
}
public class PostOpAttr
{
    public bool AttributesFollow = false;
    public Fattr3 Attributes = new();
}

public class PostOpFh3
{
    public bool HandleFollows = false;
    public NfsFh3 Handle = new();
}

public enum PPORTS : int
{
    PORTMAP_PORT = 111,
    MOUNT_PORT = 1058,
    NFS_PORT = 2049
}
public enum PRC_STATUS : int
{
    PRC_OK,
    PRC_FAIL,
    PRC_NOTIMP
}

public class PreOpAttr
{
    public bool AttributesFollow = false;
    public WccAttribute Attributes = new();
}

public class ProcessParam
{
    public uint Version = 0;
    public uint Procedure = 0;
    public string RemoteAddress = "";
}

public enum PROG_RESULT : int
{
    SUCCESS = 0,
    PROG_UNAVAIL = 1,
    PROG_MISMATCH = 2,
    PROC_UNAVAIL = 3,
    GARBAGE_ARGS = 4
}

public enum PROGS : int
{
    PROG_PORTMAP = 100000,
    PROG_NFS = 100003,
    PROG_MOUNT = 100005
}

public enum PROG_PORTS : int
{
    PROG_PORTMAP = 100000,
    PROG_NFS = 100003,
    PROG_MOUNT = 100005
}

public class RPC_HEADER
{
    public uint Header;
    public uint XID;
    public uint Msg;
    public uint Rpcvers;
    public uint Prog;
    public uint Vers;
    public uint Proc;
    public OPAQUE_AUTH Credential = new ();
    public OPAQUE_AUTH Verification = new();
}

[StructLayout(LayoutKind.Sequential)]
public struct SymbolicLinkReparseBuffer
{
    public ushort SubstituteNameOffset;
    public ushort SubstituteNameLength;
    public ushort PrintNameOffset;
    public ushort PrintNameLength;
    public uint Flags;
    public StringBuilder PathBuffer;
}

[StructLayout(LayoutKind.Sequential)]
public struct MountPointReparseBuffer
{
    public ushort SubstituteNameOffset;
    public ushort SubstituteNameLength;
    public ushort PrintNameOffset;
    public ushort PrintNameLength;
    public StringBuilder PathBuffer;
}

[StructLayout(LayoutKind.Sequential)]
public struct GenericReparseBuffer
{
    public StringBuilder DataBuffer;
}

[StructLayout(LayoutKind.Explicit)]
public struct REPARSE_DATA_BUFFER
{
    [FieldOffset(0)]
    public uint ReparseTag;
    [FieldOffset(4)]
    public ushort ReparseDataLength;
    [FieldOffset(6)]
    public ushort Reserved;
    [FieldOffset(8)]
    public SymbolicLinkReparseBuffer SBuffer;
    [FieldOffset(8)]
    public MountPointReparseBuffer MBuffer;
    [FieldOffset(8)]
    public GenericReparseBuffer GBuffer;
}
