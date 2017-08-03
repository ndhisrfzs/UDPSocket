using System;
using UDPSocket.Protocol;

namespace UDPSocket.Server
{
    public interface IAppSession : ISessionBase
    {
        IAppServer AppServer { get; }
        ISocketSession SocketSession { get; }
        DateTime LastActiveTime { get; set; }
        DateTime StartTime { get; }
        bool Connected { get; }
        void Close(CloseReason reason);
        void StartSession();
    }

    public interface IAppSession<TAppSession, TRequestInfo> : IAppSession
        where TRequestInfo : IRequestInfo
        where TAppSession : IAppSession, IAppSession<TAppSession, TRequestInfo>, new()
    {
        void Initialize(IAppServer<TAppSession, TRequestInfo> server, ISocketSession socketSession);
    }
}
