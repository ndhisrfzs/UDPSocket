using System;
using System.Net;
using System.Net.Sockets;
using UDPSocket.Common;

namespace UDPSocket.Engine
{
    class UdpSocketListener : SocketListenerBase
    {
        private Socket m_ListenSocket;
        private SocketAsyncEventArgs m_ReceiveSAE;

        public UdpSocketListener(IPEndPoint iPEndPoint) 
            : base(iPEndPoint)
        {
        }

        public override bool Start()
        {
            try
            {
                m_ListenSocket = new Socket(this.EndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                m_ListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                m_ListenSocket.Bind(this.EndPoint);

                uint IOC_IN = 0x80000000;
                uint IOC_VENDOR = 0x18000000;
                uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;

                byte[] optionInValue = { Convert.ToByte(false) };
                byte[] optionOutValue = new byte[4];
                m_ListenSocket.IOControl((int)SIO_UDP_CONNRESET, optionInValue, optionOutValue);

                var eventArgs = new SocketAsyncEventArgs();
                m_ReceiveSAE = eventArgs;

                eventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(eventArgs_Completed);
                eventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                int receiveBufferSize = 512;
                var buffer = new byte[receiveBufferSize];
                eventArgs.SetBuffer(buffer, 0, buffer.Length);

                m_ListenSocket.ReceiveFromAsync(eventArgs);

                return true;
            }
            catch (Exception e)
            {
                OnError(e);
                return false;
            }
        }

        void eventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            if(e.SocketError != SocketError.Success)
            {
                var errorCode = (int)e.SocketError;
                if (errorCode == 995 || errorCode == 10004 || errorCode == 10038)
                    return;

                OnError(new SocketException(errorCode));
            }

            if(e.LastOperation == SocketAsyncOperation.ReceiveFrom)
            {
                try
                {
                    OnNewClientAcceptedAsync(m_ListenSocket, new object[] { e.Buffer.CloneRange(e.Offset, e.BytesTransferred), e.RemoteEndPoint.Serialize() });
                }
                catch (Exception err)
                {
                    OnError(err);
                }

                try
                {
                    m_ListenSocket.ReceiveFromAsync(e);
                }
                catch(Exception err)
                {
                    OnError(err);
                }
            }
        }

        public override void Stop()
        {
            if (m_ListenSocket == null)
                return;

            lock (this)
            {
                if (m_ListenSocket == null)
                    return;

                m_ReceiveSAE.Completed -= new EventHandler<SocketAsyncEventArgs>(eventArgs_Completed);
                m_ReceiveSAE.Dispose();
                m_ReceiveSAE = null;

                try
                {
                    m_ListenSocket.Close();
                }
                finally
                {
                    m_ListenSocket = null;
                }
            }

            OnStopped();
        }
    }
}
