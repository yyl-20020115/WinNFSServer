namespace LibWinNFSServer;

public class CNFS3Prog :CRPCProg
{
    uint m_nUID, m_nGID;
    IInputStream m_pInStream;
    IOutputStream m_pOutStream;
    ProcessParam m_pParam;

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


    uint ProcedureNULL();
    uint ProcedureGETATTR();
    uint ProcedureSETATTR();
    uint ProcedureLOOKUP();
    uint ProcedureACCESS();
    uint ProcedureREADLINK();
    uint ProcedureREAD();
    uint ProcedureWRITE();
    uint ProcedureCREATE();
    uint ProcedureMKDIR();
    uint ProcedureSYMLINK();
    uint ProcedureMKNOD();
    uint ProcedureREMOVE();
    uint ProcedureRMDIR();
    uint ProcedureRENAME();
    uint ProcedureLINK();
    uint ProcedureREADDIR();
    uint ProcedureREADDIRPLUS();
    uint ProcedureFSSTAT();
    uint ProcedureFSINFO();
    uint ProcedurePATHCONF();
    uint ProcedureCOMMIT();
    uint ProcedureNOIMP();

    void Read(bool* pBool);
    void Read(uint32* pUint32);
    void Read(uint64* pUint64);
    void Read(sattr3* pAttr);
    void Read(sattrguard3* pGuard);
    void Read(diropargs3* pDir);
    void Read(opaque* pOpaque);
    void Read(nfstime3* pTime);
    void Read(createhow3* pHow);
    void Read(symlinkdata3* pSymlink);
    void Write(bool* pBool);
    void Write(uint32* pUint32);
    void Write(uint64* pUint64);
    void Write(fattr3* pAttr);
    void Write(opaque* pOpaque);
    void Write(wcc_data* pWcc);
    void Write(post_op_attr* pAttr);
    void Write(pre_op_attr* pAttr);
    void Write(post_op_fh3* pObj);
    void Write(nfstime3* pTime);
    void Write(specdata3* pSpec);
    void Write(wcc_attr* pAttr);

    private:
	int m_nResult;

    bool GetPath(ref string path);
    bool ReadDirectory(ref string dirName, ref string fileName);
    string GetFullPath(ref string dirName, ref string fileName);
    uint CheckFile(string fullPath);
    uint CheckFile(string directory, string fullPath);
    bool GetFileHandle(string path, nfs_fh3* pObject);
	bool GetFileAttributesForNFS(string path, wcc_attr* pAttr);
	bool GetFileAttributesForNFS(string path, fattr3* pAttr);
	uint FileTimeToPOSIX(FILETIME ft);
    Dictionary<int, FILE> unstableStorageFile;
}
