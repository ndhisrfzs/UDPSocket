using UDPSocket.Protocol;

namespace UDPSocket.Server
{
    public interface IRequestHandler<TRequestInfo>
        where TRequestInfo : IRequestInfo
    {
        void ExecuteCommand(IAppSession session, TRequestInfo requestInfo);
    }
}
