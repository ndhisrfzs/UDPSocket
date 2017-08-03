using System;
using UDPSocket.Common;

namespace UDPSocket.Protocol
{
    public class UdpRequestPacker : IRequestPacker<UdpRequestInfo>
    {
        public UdpRequestInfo UnPack(byte[] buffer, int offset, int length)
        {
            if (length <= 12)
            {
                return null;
            }

            var trackID = (UInt16)(buffer[offset] | buffer[offset + 1] << 8);
            var sessionID = (UInt64)buffer[offset + 2] | (UInt64)buffer[offset + 3] << 8 | (UInt64)buffer[offset + 4] << 16 | (UInt64)buffer[offset + 5] << 24 | (UInt64)buffer[offset + 6] << 32 | (UInt64)buffer[offset + 7] << 40 | (UInt64)buffer[offset + 8] << 48 | (UInt64)buffer[offset + 9] << 56;
            var key = (UInt16)(buffer[offset + 10] | buffer[offset + 11] << 8);
            var body = buffer.CloneRange(offset + 12, length - 12);

            return new UdpRequestInfo(trackID, sessionID, key, body);
        }

        public byte[] Pack(UdpRequestInfo info)
        {
            var len = 12 + info.body.Length;
            byte[] buf = new byte[len];
            buf[0] = (byte)info.TrackID;
            buf[1] = (byte)(info.TrackID >> 8);

            buf[2] = (byte)info.SessionID;
            buf[3] = (byte)(info.SessionID>>8);
            buf[4] = (byte)(info.SessionID>>16);
            buf[5] = (byte)(info.SessionID>>24);
            buf[6] = (byte)(info.SessionID>>32);
            buf[7] = (byte)(info.SessionID>>40);
            buf[8] = (byte)(info.SessionID>>48);
            buf[9] = (byte)(info.SessionID>>56);

            buf[10] = (byte)info.key;
            buf[11] = (byte)(info.key >> 8);

            Buffer.BlockCopy(info.body, 0, buf, 12, info.body.Length);

            return buf;
        }
    }
}
