using System;
using System.Collections.Generic;

namespace UDPSocket.Common
{
    public static class BinaryUtil
    {
        public static T[] CloneRange<T>(this IList<T> source, int offset, int length)
        {
            T[] target;
            var array = source as T[];

            if(array != null)
            {
                target = new T[length];
                Array.Copy(array, offset, target, 0, length);
                return target;
            }

            target = new T[length];
            for(int i = 0; i < length; i++)
            {
                target[i] = source[offset + i];
            }

            return target;
        }
    }
}
