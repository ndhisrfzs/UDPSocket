using System;

namespace UDPSocket.Protocol
{
    public interface IRequestInfo
    {
        UInt16 key { get; }
    }

    public interface IRequestInfo<TRequestBody> : IRequestInfo
    {
        TRequestBody body { get; }
    }

    public interface IRequestInfo<TRequestHeader, TRequestBody> : IRequestInfo<TRequestBody>
    {
        TRequestHeader header { get; }
    }
}
