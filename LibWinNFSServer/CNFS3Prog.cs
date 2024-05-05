using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


    uint ProcedureNULL(void);
    uint ProcedureGETATTR(void);
    uint ProcedureSETATTR(void);
    uint ProcedureLOOKUP(void);
    uint ProcedureACCESS(void);
    uint ProcedureREADLINK(void);
    uint ProcedureREAD(void);
    uint ProcedureWRITE(void);
    uint ProcedureCREATE(void);
    uint ProcedureMKDIR(void);
    uint ProcedureSYMLINK(void);
    uint ProcedureMKNOD(void);
    uint ProcedureREMOVE(void);
    uint ProcedureRMDIR(void);
    uint ProcedureRENAME(void);
    uint ProcedureLINK(void);
    uint ProcedureREADDIR(void);
    uint ProcedureREADDIRPLUS(void);
    uint ProcedureFSSTAT(void);
    uint ProcedureFSINFO(void);
    uint ProcedurePATHCONF(void);
    uint ProcedureCOMMIT(void);
    uint ProcedureNOIMP(void);

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

    bool GetPath(std::string& path);
    bool ReadDirectory(std::string& dirName, std::string& fileName);
    string GetFullPath(std::string& dirName, std::string& fileName);
    uint CheckFile(const char* fullPath);
    uint CheckFile(const char* directory, const char* fullPath);
    bool GetFileHandle(const char* path, nfs_fh3* pObject);
	bool GetFileAttributesForNFS(const char* path, wcc_attr* pAttr);
	bool GetFileAttributesForNFS(const char* path, fattr3* pAttr);
	uint FileTimeToPOSIX(FILETIME ft);
    Dictionary<int, FILE*> unstableStorageFile;
}
