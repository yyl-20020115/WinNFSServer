using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibWinNFSServer;
public class CMountProg : CRPCProg
{
    public CMountProg();

    public bool SetPathFile(string file);
    public void Export(string path, string pathAlias);
    public bool Refresh();
    public string GetClientAddr(int nIndex);
    public int GetMountNumber();
    public int Process(IInputStream pInStream, IOutputStream pOutStream, ProcessParam pParam);
    public string FormatPath(string pPath, PathFormats format);


    protected int m_nMountNum;
    protected string m_pPathFile;
    protected Dictionary<string,string> m_PathMap;
    protected string[] m_pClientAddr[MOUNT_NUM_MAX];
    protected IInputStream m_pInStream;
    protected IOutputStream m_pOutStream;

    protected void ProcedureNULL();
    protected void ProcedureMNT();
    protected void ProcedureUMNT();
    protected void ProcedureUMNTALL();
    protected void ProcedureEXPORT();
    protected void ProcedureNOIMP();


    private ProcessParam m_pParam;
    private int m_nResult;

    private bool GetPath(ref string returnPath);
    private string GetPath(ref int pathNumber);
    private bool ReadPathsFromFile(string sFileName);
}
