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
            return (int)(me.MakeLong(0, 3));
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
            return (int)(me.MakeLong(0, 4));
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
            byte[] buff = new byte[count];

            for (int idx = 0; idx < count; idx++)
                buff[idx] = me[start + idx];

            return buff;
        }

        public static byte[] CopyReverseFrom(this byte[] me, long start, long length)
        {
            byte[] block = new byte[length];
            long posRight = start + length;
            for(int i =0; i < length; i++)
            {
                block[i] = me[posRight-i-1];
            }
            return block;
        }

        /* Based on function from mfocGUI by 'Huuf' (http://www.huuf.info/OV/)
         *
         * This version is turned into an extension method.
         * It also returns a Long, which is a 64-bit value (as opposed to the 32-bit one
         * that .NET starts with) -- This means you can fetch 32 bit values easily, but
         * also get weird 48-bit values just as easy. 
         * 
         */
        public static long getBitsFromBuffer(this byte[] buffer, int iStartBit, int iLength)
        {
            // Note: Assumes big-endian
            int iEndBit = iStartBit + iLength - 1;
            int iSByte = iStartBit / 8;
            int iSBit = iStartBit % 8;
            int iEByte = iEndBit / 8;
            int iEBit = iEndBit % 8;

            if (iSByte == iEByte)
            {
                return ((char)buffer[iEByte] >> (7 - iEBit)) & ((char)0xFF >> (8 - iLength));
            }
            else
            {
                int uRet = (((char)buffer[iSByte] & (char)((char)0xFF >> iSBit)) << (((iEByte - iSByte - 1) * 8)
                        + (iEBit + 1)));

                for (int i = iSByte + 1; i < iEByte; i++)
                {
                    uRet |= (((char)buffer[i] & (char)0xFF) << (((iEByte - i - 1) * 8) + (iEBit + 1)));
                }

                uRet |= (((char)buffer[iEByte] & (char)0xFF)) >> (7 - iEBit);

                return uRet;
            }
        }

    }

}
