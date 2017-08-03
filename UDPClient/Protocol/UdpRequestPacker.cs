using System;

namespace UDPSocket.Protocol
{
    public class UdpRequestPacker : IRequestPacker<UdpRequestInfo>
    {
        public UdpRequestInfo UnPack(byte[] buffer, int offset, int length)
        {
            return null;
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
