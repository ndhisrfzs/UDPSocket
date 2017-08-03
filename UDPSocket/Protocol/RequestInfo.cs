using System;

namespace UDPSocket.Protocol
{
    public abstract class RequestInfo<TRequestBody> : IRequestInfo<TRequestBody>
    {
        public UInt16 key { get; set; }
        public TRequestBody body { get; set; }
        public RequestInfo(ushort key, TRequestBody body)
        {
            this.key = key;
            this.body = body;
        }
    }

    public abstract class RequestInfo<TRequestHeader, TRequestBody> : RequestInfo<TRequestBody>, IRequestInfo<TRequestHeader, TRequestBody>
    {
        public TRequestHeader header { get; set; }
        public RequestInfo(ushort key, TRequestHeader header, TRequestBody body)
            :base(key, body)
        {
            this.header = header;
        }
    }
}
