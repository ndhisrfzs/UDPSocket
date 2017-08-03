using System;
using System.Net.Sockets;
using UDPSocket.Common;
using UDPSocket.Server;

namespace UDPSocket.Engine
{
    abstract class SocketServerBase : ISocketServer, IDisposable
    {
        protected int Port;
        protected ISocketListener Listener { get; private set; }
        public IAppServer AppServer { get; private set; }
        public bool IsRunning { get; private set; }
        protected bool IsStopped { get; set; }

        internal ISmartPool<SendingQueue> SendingQueuePool { get; private set; }
        IPoolInfo ISocketServer.SendingQueuePool
        {
            get { return this.SendingQueuePool; }
        }
        public SocketServerBase(IAppServer appServer, int port)
        {
            AppServer = appServer;
            Port = port;
            IsRunning = false;
        }

        protected abstract ISocketListener CreateListener();
        void OnListenerError(ISocketListener listener, Exception e)
        {
            Console.WriteLine("ListenerError"); 
        }
        void OnListenerStopped(object sender, EventArgs e)
        {
            Console.WriteLine("ListenerStopped"); 
        }
        protected abstract void OnNewClientAccepted(ISocketListener listener, Socket client, object state);

        public virtual bool Start()
        {
            IsStopped = false;

            var sendingQueuePool = new SmartPool<SendingQueue>();
            sendingQueuePool.Initialize(Math.Max(5000 / 6, 256), Math.Max(5000 * 2, 256), new SendingQueueSourceCreator(64));

            SendingQueuePool = sendingQueuePool;

            var listener = CreateListener();
            listener.Error += new ErrorHandler(OnListenerError);
            listener.Stopped += new EventHandler(OnListenerStopped);
            listener.NewClientAccepted += new NewClientAcceptHandler(OnNewClientAccepted);

            if (listener.Start())
            {
                Listener = listener;
            }
            else
            {
                Listener.Stop();
                return false;
            }

            IsRunning = true;

            return true;
        }
        public void Stop()
        {
            IsStopped = true;

            Listener.Stop();

            IsRunning = false;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (IsRunning)
                    Stop();
            }
        }
    }
}
