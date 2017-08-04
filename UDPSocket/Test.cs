using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDPSocket.Command;
using UDPSocket.Protocol;
using UDPSocket.Server;

namespace UDPSocket
{
    public class Test : UdpCommandBase<TestSession>
    {
        public override UInt16 ID
        {
            get { return (UInt16)CMD.Test; }
        }

        public override void ExecuteCommand(TestSession session, UdpRequestInfo requestInfo)
        {
            Console.WriteLine("Test Command");
            session.Send(new byte[] { 0x35, 0x36 }, 0, 2);
        }
    }
}
