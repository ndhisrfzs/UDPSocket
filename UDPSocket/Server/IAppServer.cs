using System;
using System.Collections.Generic;
using UDPSocket.Engine;
using UDPSocket.Protocol;

namespace UDPSocket.Server
{
    public interface IAppServer
    {
        DateTime StartedTime { get; }
        IAppSession CreateAppSession(ISocketSession socketSession);
        bool RegisterSession(IAppSession appSession);
        IAppSession GetSessionByID(UInt64 sessionID);
    }

    public interface IAppServer<TAppSession> : IAppServer
        where TAppSession : IAppSession
    {
        IEnumerable<TAppSession> GetSessions(Func<TAppSession, bool> critera);
        IEnumerable<TAppSession> GetAllSessions();
    }

    public interface IAppServer<TAppSession, TRequestInfo> : IAppServer<TAppSession>
        where TRequestInfo : IRequestInfo
        where TAppSession : IAppSession, new()
    {
    }

    public interface ISocketServerAccessor
    {
        ISocketServer SocketServer { get; }
    }
}
