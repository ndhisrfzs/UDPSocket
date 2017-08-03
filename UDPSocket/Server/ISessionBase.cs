using System;
using System.Net;

namespace UDPSocket.Server
{
    public interface ISessionBase
    {
        UInt64 SessionID { get; }
        IPEndPoint RemoteEndPoint { get; }
    }
}
