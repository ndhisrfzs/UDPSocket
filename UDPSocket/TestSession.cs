using UDPSocket.Protocol;
using UDPSocket.Server;

namespace UDPSocket
{
    public class TestSession : AppSession<TestSession, UdpRequestInfo>
    {
        protected override void OnSessionStarted()
        {
            System.Console.WriteLine("OnSessionStarted");
            Send(new byte[] { 0x33, 0x34 }, 0, 2);
        }
    }
}
