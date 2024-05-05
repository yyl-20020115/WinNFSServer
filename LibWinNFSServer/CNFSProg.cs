namespace LibWinNFSServer;

public class CNFSProg : CRPCProg
{
    private uint m_nUID = 0;
    private uint m_nGID = 0;
    private CNFS3Prog? m_pNFS3Prog;

    public CNFSProg() { }
    public void SetUserID(uint nUID, uint nGID)
    {
        this.m_nGID= nGID;
        this.m_nUID = nUID;
    }
    public override int Process(IInputStream pInStream, IOutputStream pOutStream, ProcessParam pParam)
    {
        if (pParam.nVersion == 3)
        {
            if (m_pNFS3Prog == null)
            {
                m_pNFS3Prog = new CNFS3Prog();
                m_pNFS3Prog.SetUserID(m_nUID, m_nGID);
                m_pNFS3Prog.SetLogOn(m_bLogOn);
            }

            return m_pNFS3Prog.Process(pInStream, pOutStream, pParam);
        }
        else
        {
            PrintLog("Client requested NFS version {0} which isn't supported.\n", pParam.nVersion);
            return (int)PRC_STATUS.PRC_NOTIMP;
        }
    }
    public override void SetLogOn(bool bLogOn)
    {
        base.SetLogOn(bLogOn);
        m_pNFS3Prog?.SetLogOn(bLogOn);
    }


}
