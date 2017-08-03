using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UDPSocket.Common;
using UDPSocket.Server;

namespace UDPSocket.Engine
{
    abstract class SocketSession : ISocketSession
    {
        public UInt64 SessionID { get; private set; }
        public IAppSession AppSession { get; private set; }
        public Socket Client { get; private set; }
        public virtual IPEndPoint LocalEndPoint { get; protected set; }
        public virtual IPEndPoint RemoteEndPoint { get; protected set; }
        public Action<ISocketSession, CloseReason> Closed { get; set; }

        private ISmartPool<SendingQueue> m_SendingQueuePool;
        private SendingQueue m_SendingQueue;

        public SocketSession(UInt64 sessionID)
        {
            SessionID = sessionID;
        }

        public void Close(CloseReason reason)
        {
            throw new NotImplementedException();
        }

        public void Initialize(IAppSession appSession)
        {
            AppSession = appSession;
            if (m_SendingQueuePool == null)
                m_SendingQueuePool = ((SocketServerBase)((ISocketServerAccessor)appSession.AppServer).SocketServer).SendingQueuePool;
            SendingQueue queue;
            if(m_SendingQueuePool.TryGet(out queue))
            {
                m_SendingQueue = queue;
                queue.StartEqueue();
            }
        }

        public abstract void Start();
        public abstract void SendAsync(SendingQueue queue);
        public abstract void SendSync(SendingQueue queue);
        
        protected virtual void StartSession()
        {
            AppSession.StartSession();
        }

        public bool TrySend(ArraySegment<byte> segment)
        {
            var queue = m_SendingQueue;
            if (queue == null)
                return false;

            var trackID = queue.TrackID;
            if (!queue.Enqueue(segment, trackID))
                return false;

            StartSend(queue, trackID);
            return true;
        }

        private void StartSend(SendingQueue queue, int sendingTrackID)
        {
            SendingQueue newQueue;
            if(!m_SendingQueuePool.TryGet(out newQueue))
            {
                return;
            }

            var oldQueue = Interlocked.CompareExchange(ref m_SendingQueue, newQueue, queue);
            if(!ReferenceEquals(oldQueue, queue))
            {
                if (newQueue != null)
                    m_SendingQueuePool.Push(newQueue);

                return;
            }

            newQueue.StartEqueue();
            queue.StopEqueue();
            if(queue.Count == 0)
            {
                m_SendingQueuePool.Push(queue);
                return;
            }

            Send(queue);
        }

        private void Send(SendingQueue queue)
        {
            SendAsync(queue);
        }

        protected virtual void OnSendingCompleted(SendingQueue queue)
        {
            queue.Clear();
            m_SendingQueuePool.Push(queue);

            var newQueue = m_SendingQueue;
            if(newQueue.Count == 0)
            {
            }
            else
            {
                StartSend(newQueue, newQueue.TrackID);
            }
        }
    }
}
