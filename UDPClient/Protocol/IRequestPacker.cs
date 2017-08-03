using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPSocket.Protocol
{
    interface IRequestPacker<TRequestInfo>
        where TRequestInfo : IRequestInfo
    {
        TRequestInfo UnPack(byte[] buffer, int offset, int length);
    }
}
