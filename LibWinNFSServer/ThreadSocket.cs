using System.Net;
using System.Net.Sockets;

namespace LibWinNFSServer;

public class ThreadSocket(int type) : IDisposable
{
    public const int SOCK_STREAM = 1;
    public const int SOCK_DGRAM = 2;

    private readonly int type = type;
    private Socket? socket;
    private EndPoint? remote;
    private SocketListener? listener;
    private SocketStream? stream;
    private bool active;
    private Thread? thread;
    private bool disposed;
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {

            }

            this.socket?.Dispose();
            this.socket = null;
            disposed = true;
        }
    }

    ~ThreadSocket()
    {
        Dispose(disposing: false);
    }
    public int SocketType => this.type;
    public void Open(Socket socket, SocketListener? listener, EndPoint? remote = null)
    {
        Close();

        this.socket = socket;  //socket
        this.listener = listener;  //listener

        if (remote != null)
        {
            this.remote = remote;  //remote address
        }

        if (this.socket != null)
        {
            active = true;
            //TODO: how to use threading
            //m_hThread = (HANDLE)_beginthreadex(NULL, 0, ThreadProc, this, 0, &id);  //begin thread
        }

    }
    public void Close()
    {
        this.socket?.Close();
        this.socket = null;
    }
    public void Send()
    {
        if (socket == null || stream == null)
            return;

        switch (type)
        {
            case SOCK_STREAM:
                socket.Send(stream!.Output);
                break;
            case SOCK_DGRAM:
                socket.SendTo(stream!.Output, remote);
                break;
        }

        stream!.Reset();  //clear output buffer

    }
    public bool Active => active;  //thread is active or not
    public string RemoteAddress => (remote is IPEndPoint ipe)
            ? ipe.Address.ToString()
            : string.Empty
            ;
    public int RemotePort => (remote is IPEndPoint ipe)
            ? ipe.Port
            : -1
            ;
    public InputStream? InputStream => this.stream;
    public OutputStream? OutputStream => this.stream;

    protected void Run()
    {
        int nSize, nBytes = 0, fragmentHeaderMsb, fragmentHeaderLengthBytes;
        int fragmentHeader = 0;

        nSize = 0;// sizeof(m_RemoteAddr);

        for (; ; )
        {
            if (type == SOCK_STREAM)
            {
                // When using tcp we cannot ensure that everything we need is already
                // received. When using RCP over TCP a fragment header is added to
                // work around this. The MSB of the fragment header determines if the
                // fragment is complete (not used here) and the remaining bits define the
                // length of the rpc call (this is what we want)
                nBytes = socket.Receive(stream.Input);

                // only if at least 4 bytes are availabe (the fragment header) we can continue
                if (nBytes == 4)
                {
                    stream.SetInputSize(4);
                    stream.Read(out fragmentHeader);
                    fragmentHeaderMsb = (int)(fragmentHeader & 0x80000000);
                    fragmentHeaderLengthBytes = (int)(fragmentHeader ^ 0x80000000) + 4;
                    while (nBytes != fragmentHeaderLengthBytes)
                    {
                        nBytes = socket.Receive(stream.Input, fragmentHeaderLengthBytes, SocketFlags.None);
                    }
                    nBytes = socket.Receive(stream.Input, fragmentHeaderLengthBytes, 0);
                }
                else
                {
                    nBytes = 0;
                }
            }
            else if (type == SOCK_DGRAM)
            {
                EndPoint ep = remote;
                nBytes = socket.ReceiveFrom(stream.Input, stream.BufferSize, SocketFlags.None, ref ep);
            }


            if (nBytes > 0)
            {
                stream.SetInputSize(nBytes);  //bytes received

                if (listener != null)
                {
                    listener.SocketReceived(this);  //notify listener
                }
            }
            else
            {
                break;
            }
        }

        active = false;
    }
}
