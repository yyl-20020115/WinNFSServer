using System.Net;
using System.Net.Sockets;

namespace LibWinNFSServer;

public class CSocket : IDisposable
{
    public const int SOCK_DGRAM = 2;
    public CSocket(int nType)
    {

    }
    // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
    ~CSocket()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: false);
    }
    public int GetType();
    public void Open(Socket socket, ISocketListener? pListener, IPEndPoint pRemoteAddr = null);
    public void Close();
    public void Send();
    public bool Active();
    public string GetRemoteAddress();
    public int GetRemotePort();
    public IInputStream GetInputStream();
    public IOutputStream GetOutputStream();
    public void Run();


    private int m_nType;
    private Socket m_Socket;
    private IPEndPoint m_RemoteAddr;
    private ISocketListener m_pListener;
    private CSocketStream m_SocketStream;
    private bool m_bActive;
    private Thread m_hThread;
    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                
            }

            this.m_Socket?.Dispose();
            this.m_Socket = null;
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
