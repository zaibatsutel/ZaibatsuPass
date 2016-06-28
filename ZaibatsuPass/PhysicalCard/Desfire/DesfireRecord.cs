using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZaibatsuPass.PhysicalCard.Desfire
{
    public class DesfireRecord
    {
        public byte[] Data { get; private set; }
        public DesfireRecord(byte[] d)
        {
            Data = d;
        }
    }
}
