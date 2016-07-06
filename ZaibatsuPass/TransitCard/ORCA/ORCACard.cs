using System;
using System.Collections.Generic;

namespace ZaibatsuPass.TransitCard.ORCA
{
    class ORCACard : TransitCard
    {
        public static byte[] AID_ORCA = { 0x30, 0x10, 0xF2 };
        public static byte[] AID_SPECIAL = { 0xFF, 0xFF, 0xFF };

        // We are not a stub card
        public override bool isStub { get { return false; } }
        // we do provide events
        public override bool hasEvents { get { return true; } }

        public enum AgencyType :Byte
        {
            COMMUNITY_TRANSIT = 0x02,
            EVERETT_TRANSIT = 0x03,
            KING_COUNTY_METRO = 0x04,
            PIERCE_TRANSIT = 0x06,
            SOUND_TRANSIT = 0x07,
            WASHINGTON_STATE_FERRIES = 0x08
        }

        public enum CardActionType :Byte
        {
            /// <summary>
            /// A trip has been canceled (unused?)
            /// </summary>
            CANCEL_TRIP = 0x01,
            /// <summary>
            /// The fare was compared aganst a wallet balance
            /// </summary>
            USE_PURSE = 0x0C,
            /// <summary>
            /// Entry into sounder/link happened
            /// </summary>
            TAP_IN = 0x03,
            /// <summary>
            /// Exit from sounder/link happened.
            /// </summary>
            TAP_OUT = 0x07,
            /// <summary>
            /// PASSPort was used
            /// 
            /// It is unknown at this time just how many things use this.
            /// </summary>
            USE_PASS = 0x60,

        }

        public static System.Collections.Generic.Dictionary<AgencyType, String> ORCATransitAgencies = new Dictionary<AgencyType, string>()
        {
         { AgencyType.KING_COUNTY_METRO , "King County Metro" },
         { AgencyType.PIERCE_TRANSIT, "Pierce Transit" },
         { AgencyType.SOUND_TRANSIT, "SoundTransit" },
         { AgencyType.COMMUNITY_TRANSIT, "Community Transit" },
         { AgencyType.WASHINGTON_STATE_FERRIES, "Washington State Ferries" },
         { AgencyType.EVERETT_TRANSIT, "Everett Transit" }
        };

        public static Dictionary<AgencyType, String> ORCATransitAgenciesShort = new Dictionary<AgencyType, string>()
        {
         { AgencyType.KING_COUNTY_METRO , "KCM" },
         { AgencyType.PIERCE_TRANSIT, "PT" },
         { AgencyType.SOUND_TRANSIT, "ST" },
         { AgencyType.COMMUNITY_TRANSIT, "CT" },
         { AgencyType.WASHINGTON_STATE_FERRIES, "WSF" },
         { AgencyType.EVERETT_TRANSIT, "ET" }
        };

        public static Dictionary<long, String> LinkStations = new Dictionary<long, string>()
        {
            /* ... */
        };

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
            get
            {
                return new List<TransitEvent>(events);
            }
        }

        public override String Name
        {
            get { return "ORCA"; }
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
            serial = (int)c.getApplication(AID_SPECIAL).getFile(0xF).Data.MakeLong(5, 3);
            balance = c.getApplication(AID_ORCA).getFile(0x04).Data.MakeUint16(41, false);

            PhysicalCard.Desfire.File.RecordFile recFile = (PhysicalCard.Desfire.File.RecordFile)(c.getApplication(AID_ORCA).getFile(0x02));

            PhysicalCard.Desfire.File.Settings.RecordSettings recSettings = (PhysicalCard.Desfire.File.Settings.RecordSettings)recFile.Settings;

            ORCATransitEvent[] evs = new ORCATransitEvent[recSettings.CurRecords];

            for (int evIdx = 0; evIdx < recSettings.CurRecords; evIdx++)
            {
                evs[evIdx] = ORCATransitEvent.parseRecrd(recFile[evIdx]);
            }
            events = evs;
        }
    }
}
