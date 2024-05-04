using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibWinNFSServer;

public interface IOutputStream
{
    
    void Write(byte[] pData);
    void Write(uint nValue);
    void Write8(ulong nValue);
    void Seek(int nOffset, int nFrom);
    int GetPosition() ;
}