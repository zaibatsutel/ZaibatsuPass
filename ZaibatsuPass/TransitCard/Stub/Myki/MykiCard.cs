using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZaibatsuPass.PhysicalCard;

namespace ZaibatsuPass.TransitCard.Stub.Myki
{
    class MykiCard : ZaibatsuPass.TransitCard.TransitCard
    {

        public readonly static byte[] AID_MYKI_A = { 0x00, 0x11, 0xF2 };
        public readonly static byte[] AID_MYKI_B = { 0xF0, 0x10, 0xF2 };

        private long serialLower = 0;
        private long serialUpper = 0;

        public MykiCard(PhysicalCard.PhysicalCard c) : base(c)
        {
            if(c is PhysicalCard.Desfire.DesfireCard)
            {
                PhysicalCard.Desfire.DesfireCard dCard = (c as PhysicalCard.Desfire.DesfireCard);
                try
                {
                    byte[] meta = dCard.getApplication(AID_MYKI_A).getFile(15).Data.CopyReverseFrom(0,16);
                    serialUpper = meta.getBitsFromBuffer(96, 32);
                    serialLower = meta.getBitsFromBuffer(64, 32);
                }
                catch
                {
                    throw new ArgumentException("Could not parse Myki data.");
                }
            }
            else
            {
                throw new ArgumentException("Not a Myki card");
            }
        }

        public override string Name
        {
            get
            {
                return "Myki";
            }
        }

        public override bool isStub
        {
            get
            {
                return true;
            }
        }
        public override string SerialNumber
        {
            get
            {
                return String.Format("{0:6D}{1:8D}{2}", serialUpper, serialLower, "X");
            }
        }
    }
}
