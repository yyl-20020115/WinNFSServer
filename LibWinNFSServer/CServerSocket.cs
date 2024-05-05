using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace LibWinNFSServer;

public class CServerSocket
{
    public CServerSocket()
    {

    }
    ~CServerSocket()
    {

    }
    public void SetListener(ISocketListener pListener)
    {

    }
    public bool Open(int nPort, int nMaxNum)
    {
    }
    public void Close()
    {

    }
    public int GetPort()
    {

    }
    public void Run()
    {

    }

    
    private int m_nPort, m_nMaxNum;
    private Socket m_ServerSocket;
    private bool m_bClosed;
    private ISocketListener m_pListener;
    Thread m_hThread;
    CSocket[] m_pSockets;
}
