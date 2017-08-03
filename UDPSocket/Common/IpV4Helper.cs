using System;
using System.Net;

namespace UDPSocket.Common
{
    public static class IpV4Helper
    {
        public static UInt64 Ipv4PortToUInt64(this IPEndPoint ipEndPoint)
        {
            return ((UInt64)(BitConverter.ToUInt32(ipEndPoint.Address.GetAddressBytes(), 0)) << 32) + (UInt32)ipEndPoint.Port;
        }

        public static UInt64 Ipv4PortToUInt64(IPAddress ipAddress, int port)
        {
            return ((UInt64)(BitConverter.ToUInt32(ipAddress.GetAddressBytes(), 0)) << 32) + (UInt32)port;
        }

        public static UInt64 Ipv4PortToUInt64(uint ip, uint port)
        {
            return ((UInt64)ip << 32) + port;
        }


        public static UInt64 Ipv4PortToUInt64IP(UInt64 ipPort, uint newPort)
        {
            return (ipPort & 0xffffffff00000000 ) + newPort;
        }

        public static IPEndPoint UInt64ToIpv4Port(UInt64 ipNum)
        {

            return new IPEndPoint(new IPAddress(BitConverter.GetBytes((UInt32)(ipNum >> 32))), (int)(ipNum & 0x000000000000ffff));
        }

        public static string UInt64ToString(UInt64 ipNum)
        {
            UInt32 ip = (UInt32)(ipNum >> 32);
            return (ip & 0x000000ff).ToString() + "." + ((ip & 0x0000ff00) >> 8).ToString() + "." + ((ip & 0x00ff0000) >> 16).ToString() + "." + ((ip & 0xff000000) >> 24).ToString() + ":" + ((int)(ipNum & 0x000000000000ffff)).ToString();
        }

        public static string UInt64ToIPString(UInt64 ipNum)
        {
            UInt32 ip = (UInt32)(ipNum >> 32);
            return (ip & 0x000000ff).ToString() + "." + ((ip & 0x0000ff00) >> 8).ToString() + "." + ((ip & 0x00ff0000) >> 16).ToString() + "." + ((ip & 0xff000000) >> 24).ToString();
        }

        public static UInt64 StringToUInt64(string ipString)
        {
            UInt64 ipPort = 0;
            int length = ipString.Length;

            int pi = 0;
            int meet = 0;

            for (int i = 0; i < length; i++)
            {
                if (ipString[i] == '.')
                {
                    ipPort |= Convert.ToUInt64(ipString.Substring(pi, i - pi)) << (meet * 8 + 32);
                    pi = i + 1;
                    meet++;
                }
                if (ipString[i] == ':')
                {
                    if (meet != 3)
                    {
                        throw new FormatException();
                    }
                    ipPort |= Convert.ToUInt64(ipString.Substring(pi, i - pi)) << (meet * 8 + 32);
                    ipPort |= Convert.ToUInt64(ipString.Substring(i + 1, length - i - 1));
                }
            }

            return ipPort;
        }


        public static UInt32 UInt64GetIpv4(UInt64 ipNum)
        {
            return (UInt32)(ipNum >> 32);
        }

        public static UInt32 UInt64GetPort(UInt64 ipNum)
        {
            return (UInt32)(ipNum & 0x000000000000ffff);
        }
    }
}