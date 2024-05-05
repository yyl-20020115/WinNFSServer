using System.Net;
using System.Net.Sockets;

namespace LibWinNFSServer;

public class CServerSocket
{
    private int m_nPort = 0, m_nMaxNum = 0;
    private Socket? m_ServerSocket = null;
    private ISocketListener? m_pListener = null;
    private bool m_bClosed = true;
    Thread m_hThread;
    CSocket[] m_pSockets;
    public CServerSocket()
    {

    }
    ~CServerSocket()
    {
        this.Close();
    }
    public void SetListener(ISocketListener pListener)
    {
        m_pListener = pListener;
    }
    public bool Open(string address,int nPort, int nMaxNum)
    {
        if (!IPEndPoint.TryParse(address, out var localAddr))
            return false;
        int i;
        uint id;

        Close();

        m_nPort = nPort;
        m_nMaxNum = nMaxNum;  //max number of concurrent clients
        m_ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
        if (m_ServerSocket == null)
        {
            return false;
        }

        m_ServerSocket.Bind(localAddr);
        m_ServerSocket.Listen();

        m_pSockets = new CSocket[m_nMaxNum];

        for (i = 0; i < m_nMaxNum; i++)
        {
            m_pSockets[i] = new CSocket(CSocket.SOCK_STREAM);
        }

        m_bClosed = false;
        //m_hThread = (HANDLE)_beginthreadex(NULL, 0, ThreadProc, this, 0, &id);  //begin thread

        return true;
    }
    public void Close()
    {
        int i;

        if (m_bClosed) return;

        m_bClosed = true;
        m_ServerSocket?.Close();

        //if (m_hThread != NULL)
        //{
        //    WaitForSingleObject(m_hThread, INFINITE);
        //    CloseHandle(m_hThread);
        //}

        if (m_pSockets != null)
        {
            for (i = 0; i < m_nMaxNum; i++)
            {
                m_pSockets[i].Dispose();
                m_pSockets[i] = null;
            }

            m_pSockets = null;
        }

    }
    public int GetPort()
    {
        return m_nPort;

    }
    public void Run()
    {
        int i, nSize;
        IPEndPoint remoteAddr;
        Socket socket;

        //nSize = sizeof(remoteAddr);

        while (!m_bClosed)
        {
            socket = m_ServerSocket.Accept();
            //accept(, (sockaddr*)&remoteAddr, &nSize);  //accept connection

            if (socket != null)
            {
                for (i = 0; i < m_nMaxNum; i++)
                {
                    if (!m_pSockets[i].Active())
                    { //find an inactive CSocket
                        m_pSockets[i].Open(socket, m_pListener, socket.RemoteEndPoint);  //receive input data
                        break;
                    }
                }
            }

        }
    }
}
