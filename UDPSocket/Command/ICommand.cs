using System;
using UDPSocket.Protocol;
using UDPSocket.Server;

namespace UDPSocket.Command
{
    public interface ICommand
    {
        UInt16 ID { get; }
    }

    public interface ICommand<TAppSession, TRequestInfo> : ICommand
        where TRequestInfo : IRequestInfo
        where TAppSession : IAppSession
    {
        void ExecuteCommand(TAppSession session, TRequestInfo requestInfo);
    }
}
