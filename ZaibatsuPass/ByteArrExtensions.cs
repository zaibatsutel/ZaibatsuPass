using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZaibatsuPass
{
    static class extUtils
    {
        public static int MakeUint24(this byte[] me)
        {
            if (me.Length != 3) throw new ArgumentException("Array must be 3 bytes in size.");
            return (me[0] << 16) + (me[1] << 8) + me[2];
        }

        public static int MakeUint24(this byte[] me, int offset, bool little_endian)
        {
            if (little_endian)
                return (me[offset]) + (me[offset + 1] << 8) + (me[offset + 2] << 16);
            else
                return (me[offset] << 15) + (me[offset + 1] << 8) + me[offset + 2];
        }

        public static int MakeUint32(this byte[] me)
        {
            if (me.Length != 4) throw new ArgumentException("Array must be 4 bytes in size.");
            return (me[0] << 24) + (me[1] << 16) + (me[2] << 8) + me[3];
        }

        public static int MakeUint32(this byte[] me, int offset, bool little_endian)
        {
            if (little_endian)
                return (me[offset]) + (me[offset + 1] << 8) + (me[offset + 2] << 16) + (me[offset + 3] << 24);
            else
                return (me[offset+3]) + (me[offset + 2] << 8) + (me[offset + 1] << 16) + (me[offset] << 24);
        }
        public static UInt16 MakeUint16(this byte[] me,int offset, bool little_endian)
        {
            return little_endian ? (ushort) (me[offset] + me[offset + 1] << 8) : (ushort) (me[offset + 1] +( me[offset] << 8 ) );
        }

        public static long MakeLong(this byte[] me, int offset, int length)
        {
            // This is a direct port out of the FareBot sources.
            // I'm sorry, but this is ugly.
            long value = 0;
            for(int i = 0; i < length; i++)
                value += (me[i + offset] & 0x000000FF) << ( (length - 1 - i) * 8);
            return value;
        }

        public static byte[] CopyFrom(this byte[] me, long start, long count)
        {
            byte[] buff = new byte[start + count];

            for (int idx = 0; idx < count; idx++)
                buff[idx] = me[start + idx];

            return buff;
        }
    }

}
