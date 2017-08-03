using System;
using UDPSocket.Protocol;
using UDPSocket.Server;

namespace UDPSocket.Command
{
    public abstract class CommandBase<TAppSession, TRequestInfo> : ICommand<TAppSession, TRequestInfo>
        where TAppSession : IAppSession<TAppSession, TRequestInfo>, new()
        where TRequestInfo : IRequestInfo
    {
        public abstract UInt16 ID { get; }

        public abstract void ExecuteCommand(TAppSession session, TRequestInfo requestInfo);
    }
}
