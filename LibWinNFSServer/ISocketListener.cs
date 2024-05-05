using System.Net.Sockets;

namespace LibWinNFSServer;

public interface ISocketListener
{
    void SocketReceived(CSocket pSocket);
}
