using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UDPSocket.Command;
using UDPSocket.Common;
using UDPSocket.Engine;
using UDPSocket.Protocol;
using UDPSocket.Server;

namespace UDPSocket
{
    class Program
    {
        static void Main(string[] args)
        {
            //UdpSocketServer server = new UdpSocketServer(8088);
            //server.Start();
            //ReflectCommandLoader<ICommand<TestSession, UdpRequestInfo>> loader = new ReflectCommandLoader<ICommand<TestSession, UdpRequestInfo>>();
            //IEnumerable<ICommand<TestSession, UdpRequestInfo >> commands;
            //loader.TryLoadCommand(out commands);

            //foreach (var item in commands)
            //{
            //    int i = item.ID;
            //    //item.UnPack();
            //}

            TestAppServer appServer = new TestAppServer();
            appServer.Setup();
            appServer.Start();

            Console.Read();
        }
    }
}
