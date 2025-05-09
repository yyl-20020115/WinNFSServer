﻿using System.Net;
using System.Net.Sockets;

namespace LibWinNFSServer;

public class ServerSocket
{
    private int port = 0;
    private int max = 0;
    private Socket? socket = null;
    private SocketListener? listener = null;
    private bool closed = true;
    private readonly Thread? thread;
    private ThreadSocket[]? sockets;
    public ServerSocket()
    {

    }
    ~ServerSocket()
    {
        this.Close();
    }
    public void SetListener(SocketListener listener)
        => this.listener = listener;
    public bool Open(string address,int nPort, int nMaxNum)
    {
        if (!IPEndPoint.TryParse(address, out var localAddr))
            return false;
        Close();

        port = nPort;
        max = nMaxNum;  //max number of concurrent clients
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
        if (socket == null)
        {
            return false;
        }

        socket.Bind(localAddr);
        socket.Listen();

        sockets = new ThreadSocket[max];

        for (var i = 0; i < max; i++)
        {
            sockets[i] = new ThreadSocket(ThreadSocket.SOCK_STREAM);
        }

        closed = false;
        //m_hThread = (HANDLE)_beginthreadex(NULL, 0, ThreadProc, this, 0, &id);  //begin thread

        return true;
    }
    public void Close()
    {
        if (closed) return;

        closed = true;
        socket?.Close();

        //if (m_hThread != NULL)
        //{
        //    WaitForSingleObject(m_hThread, INFINITE);
        //    CloseHandle(m_hThread);
        //}

        if (sockets != null)
        {
            for (var i = 0; i < max; i++)
            {
                sockets[i].Dispose();
                sockets[i] = null;
            }

            sockets = null;
        }

    }
    public int Port => port;
    public void Run()
    {
        int i, nSize;
        IPEndPoint remoteAddr;
        Socket? socket;

        while (!closed)
        {
            socket = this.socket?.Accept();
            //accept(, (sockaddr*)&remoteAddr, &nSize);  //accept connection

            if (socket != null)
            {
                for (i = 0; i < max; i++)
                {
                    if (!sockets[i].Active)
                    { //find an inactive CSocket
                        sockets[i].Open(socket, listener, socket.RemoteEndPoint);  //receive input data
                        break;
                    }
                }
            }

        }
    }
}
