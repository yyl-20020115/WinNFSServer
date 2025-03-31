using System.Net;
using System.Net.Sockets;

namespace LibWinNFSServer;

public class DatagramSocket : IDisposable
{
    public const int SendBufferSize = 1 * (1 << 20);
    public const int ReceiveBufferSize = 8 * (1 << 20);
    public int Port => port;
    protected Socket? socket;
    protected ThreadSocket? csocket;
    protected SocketListener? listener;
    protected int port;
    protected bool closed;
    protected bool disposed;

    public DatagramSocket() { }

    ~DatagramSocket()
    {
        Dispose(disposing: false);
    }
    public void SetListener(SocketListener listener) => this.listener = listener;
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

        this.closed = false;
        this.csocket = new ThreadSocket(ThreadSocket.SOCK_DGRAM);
        this.csocket.Open(socket, listener);  //wait for receiving data
        return true;
    }
    public void Close()
    {
        if (this.closed) return;
        this.closed = true;
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
