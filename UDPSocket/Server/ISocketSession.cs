using System;
using System.Net;
using System.Net.Sockets;

namespace UDPSocket.Server
{
    public enum CloseReason : int
    {
        /// <summary>
        /// The socket is closed for unknown reason
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Close for server shutdown
        /// </summary>
        ServerShutdown = 1,

        /// <summary>
        /// The client close the socket
        /// </summary>
        ClientClosing = 2,

        /// <summary>
        /// The server side close the socket
        /// </summary>
        ServerClosing = 3,

        /// <summary>
        /// Application error
        /// </summary>
        ApplicationError = 4,

        /// <summary>
        /// The socket is closed for a socket error
        /// </summary>
        SocketError = 5,

        /// <summary>
        /// The socket is closed by server for timeout
        /// </summary>
        TimeOut = 6,

        /// <summary>
        /// Protocol error 
        /// </summary>
        ProtocolError = 7,

        /// <summary>
        /// SuperSocket internal error
        /// </summary>
        InternalError = 8,
    }

    public interface ISocketSession : ISessionBase
    {
        IPEndPoint LocalEndPoint { get; }
        IAppSession AppSession { get; }
        Socket Client { get; }
        Action<ISocketSession, CloseReason> Closed { get; set; }
        void Initialize(IAppSession appSession);
        void Start();
        void Close(CloseReason reason);
        bool TrySend(ArraySegment<byte> segment);
    }
}
