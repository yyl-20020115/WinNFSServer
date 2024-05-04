using System.Net;
using System.Net.Sockets;

namespace LibWinNFSServer;

public class CDatagramSocket : IDisposable
{
    public CDatagramSocket() { }
    // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
    ~CDatagramSocket()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: false);
    }
    public void SetListener(ISocketListener pListener)
    {
        m_pListener = pListener;

    }
    public bool Open(string address,int nPort)
    {
        if (!IPAddress.TryParse(address, out var ipa))
            return false;

        Close();

        this.m_Socket = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        m_Socket.SendBufferSize = 1 * 1024 * 1024;
        m_Socket.ReceiveBufferSize = 8 * 1024 * 1024;

        m_Socket.Bind(new IPEndPoint(ipa, nPort));

        m_bClosed = false;
        m_pSocket = new CSocket(CSocket.SOCK_DGRAM);
        m_pSocket.Open(m_Socket, m_pListener);  //wait for receiving data
        return true;
    }
    public void Close()
    {
        if (m_bClosed)
        {
            return;
        }

        m_bClosed = true;
        m_pSocket?.Dispose();
        m_pSocket = null;
    }
    public int GetPort()
    {
        return m_nPort;
    }


    private int m_nPort;
    private Socket m_Socket;
    private CSocket m_pSocket;
    private bool m_bClosed;
    private ISocketListener m_pListener;
    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                this.Close();
            }

            disposedValue = true;
        }
    }



    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
