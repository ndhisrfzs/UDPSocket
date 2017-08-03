using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UDPSocket.Common
{
    public sealed class SendingQueue : IList<ArraySegment<byte>>
    {
        private static ArraySegment<byte> m_Null = default(ArraySegment<byte>);

        private readonly int m_Offset;
        private readonly int m_Capacity;
        private int m_CurrentCount = 0;
        private ArraySegment<byte>[] m_GlobalQueue;
        private int m_UpdatingCount;
        private bool m_ReadOnly = false;
        private ushort m_TrackID = 1;

        public ushort TrackID
        {
            get { return m_TrackID; }
        }

        public int Capacity
        {
            get { return m_Capacity; }
        }

        public SendingQueue(ArraySegment<byte>[] globalQueue, int offset, int capcity)
        {
            m_GlobalQueue = globalQueue;
            m_Offset = offset;
            m_Capacity = capcity;
        }

        public void StartEqueue()
        {
            m_ReadOnly = false;
        }

        public void StopEqueue()
        {
            if (m_ReadOnly)
                return;
            m_ReadOnly = true;

            if (m_UpdatingCount <= 0)
                return;

            var spinWait = new SpinWait();
            do
            {
                spinWait.SpinOnce();
            }
            while (m_UpdatingCount > 0);
        }

        private bool TryEnqueue(ArraySegment<byte> item, out bool conflict, ushort trackID)
        {
            conflict = false;
            var oldCount = m_CurrentCount;
            if (oldCount >= m_Capacity)
                return false;
            if (m_ReadOnly)
                return false;
            if (m_TrackID != trackID)
                return false;

            int comparCount = Interlocked.CompareExchange(ref m_CurrentCount, oldCount + 1, oldCount);
            if(comparCount != oldCount)
            {
                conflict = true;
                return false;
            }

            m_GlobalQueue[m_Offset + oldCount] = item;

            return true;
        }

        public bool Enqueue(ArraySegment<byte> item, ushort trackID)
        {
            if (m_ReadOnly)
                return false;

            Interlocked.Increment(ref m_UpdatingCount);

            bool conflict;
            while (!m_ReadOnly)
            {
                if(TryEnqueue(item, out conflict, trackID))
                {
                    Interlocked.Decrement(ref m_UpdatingCount);
                    return true;
                }

                if (!conflict)
                    break;
            }

            Interlocked.Decrement(ref m_UpdatingCount);
            return false;
        }

        private bool TryEnqueue(IList<ArraySegment<byte>> items, out bool conflict, ushort trackID)
        {
            conflict = false;
            var oldCount = m_CurrentCount;

            int newItemCount = items.Count;
            int expectedCount = oldCount + newItemCount;
            if (expectedCount > m_Capacity)
                return false;
            if (m_ReadOnly)
                return false;
            if (m_TrackID != trackID)
                return false;

            int compareCount = Interlocked.CompareExchange(ref m_CurrentCount, expectedCount, oldCount);
            if(compareCount != oldCount)
            {
                conflict = true;
                return false;
            }

            var queue = m_GlobalQueue;
            for(var i = 0; i < items.Count; i++)
            {
                queue[m_Offset + oldCount + i] = items[i];
            }

            return true;
        }

        public bool Enqueue(IList<ArraySegment<byte>> items, ushort trackID)
        {
            if (m_ReadOnly)
                return false;
            Interlocked.Increment(ref m_UpdatingCount);

            bool conflict;
            while (!m_ReadOnly)
            {
                if(TryEnqueue(items, out conflict, trackID))
                {
                    Interlocked.Decrement(ref m_UpdatingCount);
                    return true;
                }

                if (!conflict)
                    break;
            }

            Interlocked.Decrement(ref m_UpdatingCount);
            return false;
        }

        public ArraySegment<byte> this[int index]
        {
            get
            {
                var targetIndex = m_Offset + index;
                var value = m_GlobalQueue[targetIndex];
                if (value.Array != null)
                    return value;

                var spinWait = new SpinWait();
                while (true)
                {
                    spinWait.SpinOnce();
                    value = m_GlobalQueue[targetIndex];

                    if (value.Array != null)
                        return value;

                    if (spinWait.Count > 50)
                        return value;
                }
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        public int Count
        {
            get
            {
                return m_CurrentCount;
            }
        }

        public int Position { get; set; }

        public bool IsReadOnly
        {
            get
            {
                return m_ReadOnly;
            }
        }

        public void Add(ArraySegment<byte> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            if (m_TrackID >= ushort.MaxValue)
                m_TrackID = 1;
            else
                m_TrackID++;

            for(var i = 0; i < m_CurrentCount; i++)
            {
                m_GlobalQueue[m_Offset + i] = m_Null;
            }

            m_CurrentCount = 0;
            Position = 0;
        }

        public bool Contains(ArraySegment<byte> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(ArraySegment<byte>[] array, int arrayIndex)
        {
            for(var i = 0; i < Count; i++)
            {
                array[arrayIndex + i] = this[i];
            }
        }

        public IEnumerator<ArraySegment<byte>> GetEnumerator()
        {
            for(var i = 0; i < Count; i++)
            {
                yield return m_GlobalQueue[m_Offset + i];
            }
        }

        public int IndexOf(ArraySegment<byte> item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, ArraySegment<byte> item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(ArraySegment<byte> item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
