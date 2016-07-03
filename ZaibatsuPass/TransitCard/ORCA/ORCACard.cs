using System;
using System.Collections.Generic;

namespace ZaibatsuPass.TransitCard.ORCA
{
    class ORCACard : TransitCard
    {
        public static byte[] AID_ORCA = { 0x30, 0x10, 0xF2 };
        public static byte[] AID_SPECIAL = { 0xFF, 0xFF, 0xFF };

        private int balance = 000;
        private int serial = 0000;

        private ORCATransitEvent[] events; 

        public override string Balance
        {
            get
            {
                return String.Format(new System.Globalization.CultureInfo("en-US"), "{0:C}", (float)balance / 100.0);
            }
        }

        public override List<TransitEvent> Events
        {
            get {
                return new List<TransitEvent>(events);
            }
        }

        public override String Name
        {
            get { return "ORCA";  }
        }

        public override String SerialNumber
        {
            get
            {
                return String.Format("{0:D}", serial);
            }
        }

        public ORCACard(PhysicalCard.PhysicalCard card) : base(card)
        {
            // get serial no and friends.

            PhysicalCard.Desfire.DesfireCard c = card as PhysicalCard.Desfire.DesfireCard;
            serial = (int) c.getApplication(AID_SPECIAL).getFile(0xF).Data.MakeLong(5, 3);
            balance = c.getApplication(AID_ORCA).getFile(0x04).Data.MakeUint16(41, false);

            PhysicalCard.Desfire.File.RecordFile recFile = (PhysicalCard.Desfire.File.RecordFile)(c.getApplication(AID_ORCA).getFile(0x02));

            PhysicalCard.Desfire.File.Settings.RecordSettings recSettings = (PhysicalCard.Desfire.File.Settings.RecordSettings)recFile.Settings;

            ORCATransitEvent[] evs = new ORCATransitEvent[recSettings.CurRecords];

            for(int evIdx = 0; evIdx < recSettings.CurRecords; evIdx++)
            {
                evs[evIdx] = ORCATransitEvent.parseRecrd(recFile[evIdx]);
                System.Diagnostics.Debug.WriteLine("Got event: ''{0}'' -> ''{1}''", evs[evIdx].EventTitle, evs[evIdx].EventDetails);
            }
            events = evs;
        }
    }
}
