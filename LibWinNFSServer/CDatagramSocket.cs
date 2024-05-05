using System.Net;
using System.Net.Sockets;

namespace LibWinNFSServer;

public class CDatagramSocket : IDisposable
{
    public const int SendBufferSize = 1 * (1 << 20);
    public const int ReceiveBufferSize = 8 * (1 << 20);

    public CDatagramSocket() { }

    ~CDatagramSocket()
    {

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
        m_Socket.SendBufferSize = SendBufferSize;
        m_Socket.ReceiveBufferSize = ReceiveBufferSize;

        m_Socket.Bind(new IPEndPoint(ipa, nPort));

        m_bClosed = false;
        m_pSocket = new CSocket(CSocket.SOCK_DGRAM);
        m_pSocket.Open(m_Socket, m_pListener);  //wait for receiving data
        return true;
    }
    public void Close()
    {
        if (m_bClosed) return;
        m_bClosed = true;
        m_pSocket?.Dispose();
        m_pSocket = null;
        m_Socket = null;
    }
    public int GetPort() => m_nPort;


    private Socket? m_Socket;
    private CSocket? m_pSocket;
    private ISocketListener? m_pListener;
    private int m_nPort;
    private bool m_bClosed;
    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
            }

            this.Close();
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
