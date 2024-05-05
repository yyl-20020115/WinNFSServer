namespace LibWinNFSServer;

public class CNFS3Prog :CRPCProg
{
    uint m_nUID, m_nGID;
    IInputStream m_pInStream;
    IOutputStream m_pOutStream;
    ProcessParam m_pParam;
    Dictionary<int, IntPtr> unstableStorageFile;

    public CNFS3Prog() { } 
    ~CNFS3Prog() { }
    public void SetUserID(uint nUID, uint nGID)
    {
        m_nUID = nUID;
        m_nGID = nGID;
    }
    public override int Process(IInputStream pInStream, IOutputStream pOutStream, ProcessParam pParam)
    {

    }


    uint ProcedureNULL() 
    {
        return 0;
    }
    uint ProcedureGETATTR()
    {
        return 0;
    }
    uint ProcedureSETATTR()
    {
        return 0;
    }
    uint ProcedureLOOKUP()
    {
        return 0;
    }
    uint ProcedureACCESS()
    {
        return 0;
    }
    uint ProcedureREADLINK()
    {
        return 0;
    }
    uint ProcedureREAD()
    {
        return 0;
    }
    uint ProcedureWRITE()
    {
        return 0;
    }
    uint ProcedureCREATE()
    {
        return 0;
    }
    uint ProcedureMKDIR()
    {
        return 0;
    }
    uint ProcedureSYMLINK()
    {
        return 0;
    }
    uint ProcedureMKNOD()
    {
        return 0;
    }
    uint ProcedureREMOVE()
    {
        return 0;
    }
    uint ProcedureRMDIR()
    {
        return 0;
    }
    uint ProcedureRENAME()
    {
        return 0;
    }
    uint ProcedureLINK()
    {
        return 0;
    }
    uint ProcedureREADDIR()
    {
        return 0;
    }
    uint ProcedureREADDIRPLUS()
    {
        return 0;
    }
    uint ProcedureFSSTAT()
    {
        return 0;
    }
    uint ProcedureFSINFO()
    {
        return 0;
    }
    uint ProcedurePATHCONF()
    {
        return 0;
    }
    uint ProcedureCOMMIT()
    {
        return 0;
    }
    uint ProcedureNOIMP()
    {
        return 0;
    }

    void Read(ref bool pBool)
    {
    }
    void Read(uint32* pUint32)
    {
    }
    void Read(uint64* pUint64)
    {
    }
    void Read(sattr3* pAttr)
    {
    }
    void Read(sattrguard3* pGuard)
    {
    }
    void Read(diropargs3* pDir)
    {
    }
    void Read(opaque* pOpaque)
    {
    }
    void Read(nfstime3* pTime)
    {
    }
    void Read(createhow3* pHow)
    {
    }
    void Read(symlinkdata3* pSymlink)
    {
    }
    void Write(ref bool pBool)
    {
    }
    void Write(uint32* pUint32)
    {
    }
    void Write(uint64* pUint64)
    {
    }
    void Write(fattr3* pAttr)
    {
    }
    void Write(opaque* pOpaque)
    {
    }
    void Write(wcc_data* pWcc)
    {
    }
    void Write(post_op_attr* pAttr)
    {
    }
    void Write(pre_op_attr* pAttr)
    {
    }
    void Write(post_op_fh3* pObj)
    {
    }
    void Write(nfstime3* pTime)
    {
    }
    void Write(specdata3* pSpec)
    {
    }
    void Write(wcc_attr* pAttr)
    {
    }

	int m_nResult;

    bool GetPath(ref string path)
    {
    }
    bool ReadDirectory(ref string dirName, ref string fileName)
    {
    }
    string GetFullPath(ref string dirName, ref string fileName)
    {
    }
    uint CheckFile(string fullPath)
    {
    }
    uint CheckFile(string directory, string fullPath)
    {
    }
    bool GetFileHandle(string path, nfs_fh3* pObject)
    {
    }
    bool GetFileAttributesForNFS(string path, wcc_attr* pAttr)
    {
    }
    bool GetFileAttributesForNFS(string path, fattr3* pAttr)
    {
    }
    uint FileTimeToPOSIX(FILETIME ft)
    {
    }
}
