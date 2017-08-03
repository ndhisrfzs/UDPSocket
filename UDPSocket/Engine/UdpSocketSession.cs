using System;
using System.Net;
using System.Net.Sockets;
using UDPSocket.Common;

namespace UDPSocket.Engine
{
    class UdpSocketSession : SocketSession
    {
        private Socket m_ServerSocket;
        public UdpSocketSession(Socket serverSocket, IPEndPoint remoteEndPoint)
            : base(remoteEndPoint.Ipv4PortToUInt64())
        {
            m_ServerSocket = serverSocket;
            RemoteEndPoint = remoteEndPoint;
        }

        public UdpSocketSession(Socket serverSocket, IPEndPoint remoteEndPoint, UInt64 sessionID)
            : base(sessionID)
        {
            m_ServerSocket = serverSocket;
            RemoteEndPoint = remoteEndPoint;
        }

        public override IPEndPoint LocalEndPoint
        {
            get
            {
                return (IPEndPoint)m_ServerSocket.LocalEndPoint;
            }
        }

        internal void  UpdateRemoteEndPoint(IPEndPoint remoteEndPoint)
        {
            RemoteEndPoint = remoteEndPoint;
        }

        public override void Start()
        {
            StartSession();
        }

        public override void SendAsync(SendingQueue queue)
        {
            var e = new SocketAsyncEventArgs();

            e.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendingCompleted);
            e.RemoteEndPoint = RemoteEndPoint;
            e.UserToken = queue;

            var item = queue[queue.Position];
            e.SetBuffer(item.Array, item.Offset, item.Count);
            if (!m_ServerSocket.SendToAsync(e))
                OnSendingCompleted(this, e);
        }

        void OnSendingCompleted(object sender, SocketAsyncEventArgs e)
        {
            var queue = e.UserToken as SendingQueue;
            if(e.SocketError != SocketError.Success)
            {
                return;
            }

            CleanSocketAsyncEventArgs(e);
            var newPos = queue.Position + 1;
            if(newPos >= queue.Count)
            {
                OnSendingCompleted(queue);
                return;
            }

            queue.Position = newPos;
            SendAsync(queue);
        }

       

        void CleanSocketAsyncEventArgs(SocketAsyncEventArgs e)
        {
            e.UserToken = null;
            e.Completed -= new EventHandler<SocketAsyncEventArgs>(OnSendingCompleted);
            e.Dispose();
        }

        public override void SendSync(SendingQueue queue)
        {
            throw new NotImplementedException();
        }
    }
}
