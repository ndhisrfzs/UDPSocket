using System;
using System.Net;
using System.Threading;
using UDPSocket.Protocol;

namespace UDPSocket.Server
{
    public abstract class AppSession<TAppSession, TRequestInfo> : IAppSession<TAppSession, TRequestInfo>
        where TAppSession : AppSession<TAppSession, TRequestInfo>, IAppSession, new()
        where TRequestInfo : class, IRequestInfo
    {
        public virtual AppServerBase<TAppSession, TRequestInfo> AppServer { get; private set; }
        IAppServer IAppSession.AppServer
        {
            get { return this.AppServer; }
        }
        public bool Connected { get; internal set; }

        public IPEndPoint RemoteEndPoint
        {
            get { return SocketSession.RemoteEndPoint; }
        }

        public ulong SessionID { get; private set; }

        public ISocketSession SocketSession { get; private set; }

        public DateTime StartTime { get; private set; }
        public DateTime LastActiveTime { get; set; }


        public void Close(CloseReason reason)
        {
            this.SocketSession.Close(reason);
        }

        public void Initialize(IAppServer<TAppSession, TRequestInfo> server, ISocketSession socketSession)
        {
            var castedAppServer = (AppServerBase<TAppSession, TRequestInfo>)server;
            AppServer = castedAppServer;
            SocketSession = socketSession;
            SessionID = socketSession.SessionID;
            Connected = true;
            socketSession.Initialize(this);
        }

        public void StartSession()
        {
            OnSessionStarted();
        }

        protected virtual void OnSessionStarted()
        {

        }

        internal protected virtual void OnSessionClosed(CloseReason reason)
        {

        }

        public virtual void Send(byte[] data, int offset, int length)
        {
            InternalSend(new ArraySegment<byte>(data, offset, length));
        }

        private bool InternalTrySend(ArraySegment<byte> segment)
        {
            if (!SocketSession.TrySend(segment))
                return false;

            LastActiveTime = DateTime.Now;
            return true;
        }

        private void InternalSend(ArraySegment<byte> segment)
        {
            if (!Connected)
                return;
            if (InternalTrySend(segment))
                return;

            var timeOutTime = DateTime.Now.AddMilliseconds(1);
            var spinWait = new SpinWait();
            while (Connected)
            {
                spinWait.SpinOnce();
                if (InternalTrySend(segment))
                    return;

                if(DateTime.Now >= timeOutTime)
                {
                    throw new TimeoutException("The Sending attempt time out");
                }
            }
        } 
    }
}
