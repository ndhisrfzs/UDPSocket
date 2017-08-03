using System;
using System.Net;
using System.Net.Sockets;

namespace UDPSocket.Engine
{
    delegate void ErrorHandler(ISocketListener listener, Exception e);
    delegate void NewClientAcceptHandler(ISocketListener listener, Socket client, object state);
    interface ISocketListener
    {
        IPEndPoint EndPoint { get; }

        bool Start();

        void Stop();

        event NewClientAcceptHandler NewClientAccepted;
        event ErrorHandler Error;
        event EventHandler Stopped;
    }
}
