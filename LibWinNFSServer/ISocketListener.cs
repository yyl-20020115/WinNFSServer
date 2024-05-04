using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LibWinNFSServer;

public interface ISocketListener
{
    void SocketReceived(Socket pSocket);
}
