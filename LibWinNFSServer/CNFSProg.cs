using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibWinNFSServer;

public class CNFSProg : CRPCProg
{
    public CNFSProg() { }
    
    public void SetUserID(uint nUID, uint nGID);
    public int Process(IInputStream pInStream, IOutputStream pOutStream, ProcessParam pParam);
    public void SetLogOn(bool bLogOn);

   
    private uint m_nUID, m_nGID;
    private CNFS3Prog m_pNFS3Prog;
}
