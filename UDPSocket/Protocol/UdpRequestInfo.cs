using System;
using UDPSocket.Common;

namespace UDPSocket.Protocol
{
    public class UdpRequestInfo : RequestInfo<byte[]>
    {
        public UInt16 TrackID { get; private set; }
        public UInt64 SessionID { get; private set; }
        public UdpRequestInfo(UInt16 trackID, UInt64 sessionID, UInt16 key, byte[] body) 
            : base(key, body)
        {
            this.TrackID = trackID;
            this.SessionID = sessionID;
        }

       
    }
}
