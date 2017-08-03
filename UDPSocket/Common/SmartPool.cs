using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UDPSocket.Common
{
    public interface IPoolInfo
    {
        int MinPoolSize { get; }
        int MaxPoolSize { get; }
        int AvialableItemsCount { get; }
        int TotalItemsCount { get; }
    }

    public interface ISmartPool<T> : IPoolInfo
    {
        void Initialize(int minPoolSize, int maxPoolSize, ISmartPoolSourceCreator<T> sourceCreator);
        void Push(T item);
        bool TryGet(out T item);
    }
    public interface ISmartPoolSource
    {
        int Count { get; }
    }
    public class SmartPoolSource : ISmartPoolSource
    {
        public object Source { get; private set; }
        public int Count { get; private set; }
        public SmartPoolSource(object source, int itemsCount)
        {
            Source = source;
            Count = itemsCount;
        }
    }
    public interface ISmartPoolSourceCreator<T>
    {
        ISmartPoolSource Create(int size, out T[] poolItems);
    }
    public class SendingQueueSourceCreator : ISmartPoolSourceCreator<SendingQueue>
    {
        private int m_SendingQueueSize;
        public SendingQueueSourceCreator(int sendingQueueSize)
        {
            m_SendingQueueSize = sendingQueueSize;
        }
        public ISmartPoolSource Create(int size, out SendingQueue[] poolItems)
        {
            var source = new ArraySegment<byte>[size * m_SendingQueueSize];
            poolItems = new SendingQueue[size];
            for(var i = 0; i < size; i++)
            {
                poolItems[i] = new SendingQueue(source, i * m_SendingQueueSize, m_SendingQueueSize);
            }
            return new SmartPoolSource(source, size);
        }
    }
    public class SmartPool<T> : ISmartPool<T>
    {
        private ConcurrentStack<T> m_GlobalStack;
        private ISmartPoolSource[] m_ItemSource;
        private int m_CurrentSoureCount;
        private ISmartPoolSourceCreator<T> m_SourceCreator;
        private int m_IsIncreasing = 1;
        public int AvialableItemsCount { get { return m_GlobalStack.Count; } }
        public int MaxPoolSize { get; private set; }
        public int MinPoolSize { get; private set; }
        public int TotalItemsCount { get; private set; }

        public void Initialize(int minPoolSize, int maxPoolSize, ISmartPoolSourceCreator<T> sourceCreator)
        {
            MinPoolSize = minPoolSize;
            MaxPoolSize = maxPoolSize;
            m_SourceCreator = sourceCreator;
            m_GlobalStack = new ConcurrentStack<T>();
            int n = 0;
            if(minPoolSize != maxPoolSize)
            {
                var currentValue = minPoolSize;
                while (true)
                {
                    n++;
                    var thisValue = currentValue * 2;
                    if (thisValue >= maxPoolSize)
                        break;

                    currentValue = thisValue;
                }
            }

            m_ItemSource = new ISmartPoolSource[n + 1];

            T[] items;
            m_ItemSource[0] = sourceCreator.Create(minPoolSize, out items);
            m_CurrentSoureCount = 1;
            for(var i = 0; i < items.Length; i++)
            {
                m_GlobalStack.Push(items[i]);
            }

            TotalItemsCount = minPoolSize;
        }

        public void Push(T item)
        {
            m_GlobalStack.Push(item);
        }

        bool TryPopWithWait(out T item, int waitTicks)
        {
            var spinWait = new SpinWait();
            while (true)
            {
                spinWait.SpinOnce();
                if (m_GlobalStack.TryPop(out item))
                    return true;

                if (spinWait.Count >= waitTicks)
                {
                    return false;
                }
            }
        }

        public bool TryGet(out T item)
        {
            if (m_GlobalStack.TryPop(out item))
                return true;

            var currentSourceCount = m_CurrentSoureCount;
            if(currentSourceCount >= m_ItemSource.Length)
            {
                return TryPopWithWait(out item, 100);
            }

            var isIncreasing = m_IsIncreasing;
            if (isIncreasing == 1)
                return TryPopWithWait(out item, 100);

            if(Interlocked.CompareExchange(ref m_IsIncreasing, 1, isIncreasing) != isIncreasing)
                return TryPopWithWait(out item, 100);

            IncreaseCapacity();

            m_IsIncreasing = 0;
            if (!m_GlobalStack.TryPop(out item))
            {
                return false;
            }

            return true;
        }

        private void IncreaseCapacity()
        {
            var newItemsCount = Math.Min(TotalItemsCount, MaxPoolSize - TotalItemsCount);
            T[] items;
            m_ItemSource[m_CurrentSoureCount++] = m_SourceCreator.Create(newItemsCount, out items);

            TotalItemsCount += newItemsCount;

            for(var i = 0; i< items.Length; i++)
            {
                m_GlobalStack.Push(items[i]);
            }
        }
    }
}
