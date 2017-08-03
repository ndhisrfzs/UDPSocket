using UDPSocket.Protocol;
using UDPSocket.Server;

namespace UDPSocket.Command
{
    public abstract class UdpCommandBase<TAppSession> : CommandBase<TAppSession, UdpRequestInfo>
        where TAppSession : IAppSession<TAppSession, UdpRequestInfo>, new()
    {
    }
}
