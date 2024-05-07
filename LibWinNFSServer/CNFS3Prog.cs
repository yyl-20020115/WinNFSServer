namespace LibWinNFSServer;

public partial class CNFS3Prog :CRPCProg
{
    int m_nUID, m_nGID;
    IInputStream m_pInStream;
    IOutputStream m_pOutStream;
    ProcessParam m_pParam;
    Dictionary<int, IntPtr> unstableStorageFile;

    public CNFS3Prog() { } 
    ~CNFS3Prog() { }
    public void SetUserID(int nUID, int nGID)
    {
        m_nUID = nUID;
        m_nGID = nGID;
    }
    public override int Process(IInputStream pInStream, IOutputStream pOutStream, ProcessParam pParam)
    {
        return 0;
    }


    int ProcedureNULL() 
    {
        return 0;
    }
    int ProcedureGETATTR()
    {
        return 0;
    }
    int ProcedureSETATTR()
    {
        return 0;
    }
    int ProcedureLOOKUP()
    {
        return 0;
    }
    int ProcedureACCESS()
    {
        return 0;
    }
    int ProcedureREADLINK()
    {
        return 0;
    }
    int ProcedureREAD()
    {
        return 0;
    }
    int ProcedureWRITE()
    {
        return 0;
    }
    int ProcedureCREATE()
    {
        return 0;
    }
    int ProcedureMKDIR()
    {
        return 0;
    }
    int ProcedureSYMLINK()
    {
        return 0;
    }
    int ProcedureMKNOD()
    {
        return 0;
    }
    int ProcedureREMOVE()
    {
        return 0;
    }
    int ProcedureRMDIR()
    {
        return 0;
    }
    int ProcedureRENAME()
    {
        return 0;
    }
    int ProcedureLINK()
    {
        return 0;
    }
    int ProcedureREADDIR()
    {
        return 0;
    }
    int ProcedureREADDIRPLUS()
    {
        return 0;
    }
    int ProcedureFSSTAT()
    {
        return 0;
    }
    int ProcedureFSINFO()
    {
        return 0;
    }
    int ProcedurePATHCONF()
    {
        return 0;
    }
    int ProcedureCOMMIT()
    {
        return 0;
    }
    int ProcedureNOIMP()
    {
        return 0;
    }

    void Read(ref bool pBool)
    {
    }
    void Read(ref int pUint32)
    {
    }
    void Read(ref ulong pUint64)
    {
    }
    void Read(ref sattr3 pAttr)
    {
    }
    void Read(ref sattrguard3 pGuard)
    {
    }
    void Read(ref diropargs3 pDir)
    {
    }
    void Read(ref opaque pOpaque)
    {
    }
    void Read(ref nfstime3 pTime)
    {
    }
    void Read(ref createhow3 pHow)
    {
    }
    void Read(ref symlinkdata3 pSymlink)
    {
    }
    void Write(bool pBool)
    {
    }
    void Write(int pUint32)
    {
    }
    void Write(ulong pUint64)
    {
    }
    void Write(fattr3 pAttr)
    {
    }
    void Write(opaque pOpaque)
    {
    }
    void Write(wcc_data pWcc)
    {
    }
    void Write(post_op_attr pAttr)
    {
    }
    void Write(pre_op_attr pAttr)
    {
    }
    void Write(post_op_fh3 pObj)
    {
    }
    void Write(nfstime3 pTime)
    {
    }
    void Write(specdata3 pSpec)
    {
    }
    void Write(wcc_attr pAttr)
    {
    }

	int m_nResult;

    bool GetPath(ref string path)
    {
        return false;
    }
    bool ReadDirectory(ref string dirName, ref string fileName)
    {
        return false;
    }
    string GetFullPath(ref string dirName, ref string fileName)
    {
        return "";
    }
    int CheckFile(string fullPath)
    {
        return 0;
    }
    int CheckFile(string directory, string fullPath)
    {
        return 0;
    }
    bool GetFileHandle(string path, nfs_fh3 pObject)
    {
        return false;
    }
    bool GetFileAttributesForNFS(string path, wcc_attr pAttr)
    {
        return false;
    }
    bool GetFileAttributesForNFS(string path, fattr3 pAttr)
    {
        return false;
    }
    int FileTimeToPOSIX(FILETIME ft)
    {
        return 0;
    }
}
