using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDPSocket.Protocol;
using UDPSocket.Server;

namespace UDPSocket
{
    class TestAppServer : AppServer<TestSession, UdpRequestInfo>
    {
        public void Setup()
        {
            base.Setup(8888);
        }
    }
}
