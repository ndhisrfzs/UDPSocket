using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UDPSocket.Protocol;
using UDPSocket.Server;

namespace UDPSocket.Engine
{
    class UdpSocketServer<TRequestInfo> : SocketServerBase
        where TRequestInfo : IRequestInfo
    {
        private IPEndPoint m_EndPointIPv4;
        private IPEndPoint m_EndPointIPv6;
        private IRequestPacker<TRequestInfo> m_Packer;
        private IRequestHandler<TRequestInfo> m_RequestHandler;
        public UdpSocketServer(IAppServer appServer, int port) 
            : base(appServer, port)
        {
            m_RequestHandler = appServer as IRequestHandler<TRequestInfo>;
            m_EndPointIPv4 = new IPEndPoint(IPAddress.Any, 0);
            m_EndPointIPv6 = new IPEndPoint(IPAddress.IPv6Any, 0);
            m_Packer = new UdpRequestPacker() as IRequestPacker<TRequestInfo>;
        }

        protected override ISocketListener CreateListener()
        {
            return new UdpSocketListener(new IPEndPoint(IPAddress.Any, Port));
        }

        protected override void OnNewClientAccepted(ISocketListener listener, Socket client, object state)
        {
            var paramArray = state as object[];

            var receivedData = paramArray[0] as byte[];
            var socketAddress = paramArray[1] as SocketAddress;
            var remoteEndPoint = (socketAddress.Family == AddressFamily.InterNetworkV6 ? m_EndPointIPv6.Create(socketAddress) : m_EndPointIPv4.Create(socketAddress)) as IPEndPoint;

            try
            {
                ProcessPackage(client, remoteEndPoint, receivedData);
            }
            catch (Exception)
            {
                throw;
            }
        }

        void ProcessPackage(Socket listenSocket, IPEndPoint remoteEndPoint, byte[] receivedData)
        {
            TRequestInfo requestInfo;
            UInt64 sessionID;
            int rest;
            try
            {
                requestInfo = this.m_Packer.UnPack(receivedData, 0, receivedData.Length);
            }
            catch
            {
                Debug.WriteLine("Failed to parse UDP Package");
                return;
            }

            var udpRequestInfo = requestInfo as UdpRequestInfo;

            if(udpRequestInfo == null)
            {
                Debug.WriteLine("Invalid UDP Package format!");
                return;
            }
            sessionID = udpRequestInfo.SessionID;
            if(sessionID <= 0)
            {
                Debug.WriteLine("Failed to get session key from UDP package");
                return;
            }

            var appSession = AppServer.GetSessionByID(sessionID);
            if(appSession == null)
            {
                appSession = CreateNewSession(listenSocket, remoteEndPoint, sessionID);

                if (appSession == null)
                    return;
            }
            else
            {
                var socketSession = appSession.SocketSession as UdpSocketSession;
                socketSession.UpdateRemoteEndPoint(remoteEndPoint);
            }

            m_RequestHandler.ExecuteCommand(appSession, requestInfo);
        }

        IAppSession CreateNewSession(Socket listenSocket, IPEndPoint remoteEndPoint, UInt64 sessionID)
        {
            var socketSession = new UdpSocketSession(listenSocket, remoteEndPoint, sessionID);
            var appSession = AppServer.CreateAppSession(socketSession);
            if (appSession == null)
                return null;
            if (!AppServer.RegisterSession(appSession))
                return null;

            socketSession.Closed += OnSocketSessionClosed;
            socketSession.Start();

            return appSession;
        }

        void OnSocketSessionClosed(ISocketSession socketSession, CloseReason closeReason)
        {
            Console.WriteLine("Closed");
        }
    }
}
