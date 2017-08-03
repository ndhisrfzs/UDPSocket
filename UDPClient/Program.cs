using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UDPSocket.Protocol;

namespace UDPClient
{
    class Program
    {
        static void Main(string[] args)
        {
            UdpClient udpClient = new UdpClient();
            udpClient.Connect("127.0.0.1", 8888);
            IPEndPoint ep = null;
            try
            {
                UdpRequestInfo info = new UdpRequestInfo(1, 1, 1, new byte[] { 0x31, 0x32 });
                UdpRequestPacker packer = new UdpRequestPacker();
                var buf = packer.Pack(info);
                byte[] rddata = new byte[100];
                udpClient.Send(buf, buf.Length);
                //udpClient.Send(new byte[] { 0x33, 0x34 }, 2);
                while (true)
                {
                    rddata = udpClient.Receive(ref ep);
                    string aa = Encoding.ASCII.GetString(rddata);
                    Console.WriteLine(aa);
                    udpClient.Send(buf, buf.Length);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
