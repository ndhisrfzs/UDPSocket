using UDPSocket.Common;

namespace UDPSocket.Engine
{
    public interface ISocketServer
    {
        IPoolInfo SendingQueuePool { get; }
        bool Start();
        bool IsRunning { get; }
        void Stop();
    }
}
