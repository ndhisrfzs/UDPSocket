using System;
using System.Net;
using System.Net.Sockets;

namespace UDPSocket.Engine
{
    abstract class SocketListenerBase : ISocketListener
    {
        private IPEndPoint m_EndPoint;
        public IPEndPoint EndPoint
        {
            get { return m_EndPoint; }
        }

        protected SocketListenerBase(IPEndPoint iPEndPoint)
        {
            m_EndPoint = iPEndPoint;
        }
        public abstract bool Start();
        public abstract void Stop();

        public event ErrorHandler Error;
        public event NewClientAcceptHandler NewClientAccepted;
        public event EventHandler Stopped;

        protected void OnError(Exception e)
        {
            Error?.Invoke(this, e);
        }

        protected void OnError(string errorMessage)
        {
            OnError(new Exception(errorMessage));
        }

        protected virtual void OnNewClientAccepted(Socket socket, object state)
        {
            NewClientAccepted?.Invoke(this, socket, state);
        }

        protected void OnNewClientAcceptedAsync(Socket socket, object state)
        {
            var handler = NewClientAccepted;
            if(handler != null)
            {
                handler.BeginInvoke(this, socket, state, null, null);
            }
        }

        protected void OnStopped()
        {
            Stopped?.Invoke(this, EventArgs.Empty);
        }
    }
}
