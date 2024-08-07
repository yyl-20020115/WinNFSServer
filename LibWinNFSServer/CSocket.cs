﻿using System.Net;
using System.Net.Sockets;

namespace LibWinNFSServer;

public class CSocket : IDisposable
{
    public const int SOCK_STREAM = 1;
    public const int SOCK_DGRAM = 2;

    private int m_nType = 0;
    private Socket? m_Socket;
    private EndPoint? m_RemoteAddr;
    private ISocketListener? m_pListener;
    private CSocketStream? m_SocketStream;
    private bool m_bActive;
    private Thread? m_hThread;
    private bool disposedValue;
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

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

    public CSocket(int nType)
    {
        this.m_nType = nType;
    }
    
    ~CSocket()
    {
        
        Dispose(disposing: false);
    }
    public int GetSocketType() => this.m_nType;
    public void Open(Socket socket, ISocketListener? pListener, EndPoint pRemoteAddr = null)
    {
        Close();

        m_Socket = socket;  //socket
        m_pListener = pListener;  //listener

        if (pRemoteAddr != null)
        {
            m_RemoteAddr = pRemoteAddr;  //remote address
        }

        if (m_Socket != null)
        {
            m_bActive = true;
            //TODO: how to use threading
            //m_hThread = (HANDLE)_beginthreadex(NULL, 0, ThreadProc, this, 0, &id);  //begin thread
        }

    }
    public void Close()
    {
        m_Socket?.Close();
        m_Socket = null;
    }
    public void Send()
    {
        if (m_Socket == null)
            return;

        if (m_nType == SOCK_STREAM)
        {
            m_Socket.Send(m_SocketStream.GetOutput());
        }
        else if (m_nType == SOCK_DGRAM)
        {
            m_Socket.SendTo(m_SocketStream.GetOutput(), m_RemoteAddr);
        }

        m_SocketStream.Reset();  //clear output buffer

    }
    public bool Active()
    {
        return m_bActive;  //thread is active or not
    }
    public string GetRemoteAddress()
    {
        return (m_RemoteAddr is IPEndPoint ipe)
            ? ipe.Address.ToString()
            :""
            ;
    }
    public int GetRemotePort()
    {
        return (m_RemoteAddr is IPEndPoint ipe)
            ? ipe.Port
            : 0
            ;
    }
    public IInputStream GetInputStream()
    {
        return this.m_SocketStream;
    }
    public IOutputStream GetOutputStream()
    {
        return this.m_SocketStream;
    }

    protected void Run()
    {
        int nSize, nBytes = 0, fragmentHeaderMsb, fragmentHeaderLengthBytes;
        int fragmentHeader = 0;

        nSize = 0;// sizeof(m_RemoteAddr);

        for (; ; )
        {
            if (m_nType == SOCK_STREAM)
            {
                // When using tcp we cannot ensure that everything we need is already
                // received. When using RCP over TCP a fragment header is added to
                // work around this. The MSB of the fragment header determines if the
                // fragment is complete (not used here) and the remaining bits define the
                // length of the rpc call (this is what we want)
                nBytes = m_Socket.Receive(m_SocketStream.GetInput());

                // only if at least 4 bytes are availabe (the fragment header) we can continue
                if (nBytes == 4)
                {
                    m_SocketStream.SetInputSize(4);
                    m_SocketStream.Read(out fragmentHeader);
                    fragmentHeaderMsb = (int)(fragmentHeader & 0x80000000);
                    fragmentHeaderLengthBytes = (int)(fragmentHeader ^ 0x80000000) + 4;
                    while (nBytes != fragmentHeaderLengthBytes)
                    {
                        nBytes = m_Socket.Receive(m_SocketStream.GetInput(), fragmentHeaderLengthBytes, SocketFlags.None);
                    }
                    nBytes = m_Socket.Receive(m_SocketStream.GetInput(), fragmentHeaderLengthBytes, 0);
                }
                else
                {
                    nBytes = 0;
                }
            }
            else if (m_nType == SOCK_DGRAM)
            {
                EndPoint ep = m_RemoteAddr;
                nBytes = m_Socket.ReceiveFrom(m_SocketStream.GetInput(), m_SocketStream.GetBufferSize(),SocketFlags.None, ref ep);
            }


            if (nBytes > 0)
            {
                m_SocketStream.SetInputSize(nBytes);  //bytes received

                if (m_pListener != null)
                {
                    m_pListener.SocketReceived(this);  //notify listener
                }
            }
            else
            {
                break;
            }
        }

        m_bActive = false;
    }
}
