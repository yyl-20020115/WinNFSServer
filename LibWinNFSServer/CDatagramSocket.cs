using System.Net;
using System.Net.Sockets;

namespace LibWinNFSServer;

public class CDatagramSocket : IDisposable
{
    public const int SendBufferSize = 1 * (1 << 20);
    public const int ReceiveBufferSize = 8 * (1 << 20);
    public int Port => port;
    protected Socket? socket;
    protected CSocket? csocket;
    protected ISocketListener? listener;
    protected int port;
    protected bool is_closed;
    protected bool disposed;

    public CDatagramSocket() { }

    ~CDatagramSocket()
    {
        Dispose(disposing: false);
    }
    public void SetListener(ISocketListener listener)
    {
        this.listener = listener;
    }
    public bool Open(string address, int nPort)
    {
        if (!IPAddress.TryParse(address, out var ipa))
            return false;

        Close();

        this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
        {
            SendBufferSize = SendBufferSize,
            ReceiveBufferSize = ReceiveBufferSize
        };
        this.socket.Bind(new IPEndPoint(ipa, nPort));

        this.is_closed = false;
        this.csocket = new CSocket(CSocket.SOCK_DGRAM);
        this.csocket.Open(socket, listener);  //wait for receiving data
        return true;
    }
    public void Close()
    {
        if (this.is_closed) return;
        this.is_closed = true;
        this.csocket?.Dispose();
        this.csocket = null;
        this.socket = null;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
            }

            this.Close();
            this.disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
