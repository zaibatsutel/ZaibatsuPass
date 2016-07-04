using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZaibatsuPass.TransitCard.ORCA
{
    class ORCATransitEvent : TransitEvent
    {
        DateTime mTimestamp;
        long mCoach;
        long mEventCost;
        long mNewBalance;


        ORCA.ORCACard.CardActionType mActionType;
        ORCA.ORCACard.AgencyType mTransitAgency;

        public override string EventTitle
        {
            get
            {
                String fAgency = "(unknown) ";
                if (ORCACard.ORCATransitAgenciesShort.ContainsKey(mTransitAgency))
                    fAgency = ORCACard.ORCATransitAgenciesShort[mTransitAgency];
                return string.Format("{0}{1}",fAgency, mCoach);
            }
        }

        public override string EventDetailsShort
        {
            get
            {
                if (mActionType == ORCACard.CardActionType.TAP_OUT && this.EventType == TransitEventType.Metro)
                    return "Exited ST LINK";
                else if (mActionType == ORCACard.CardActionType.TAP_OUT && this.EventType == TransitEventType.Train)
                    return "Exited Sounder";


                return String.Format(new System.Globalization.CultureInfo("en-US"), "Action: {0}", mActionType.ToString()  );
            }

        }

        public override TransitEventType EventType
        {
            get
            {
                switch(mTransitAgency)
                {
                    case ORCACard.AgencyType.SOUND_TRANSIT:
                        if (mCoach < 20) return TransitEventType.Train;
                        if (mCoach > 10000) return TransitEventType.Metro;
                        else return TransitEventType.Bus;
                    case ORCACard.AgencyType.WASHINGTON_STATE_FERRIES:
                        return TransitEventType.Ferry;
                    case ORCACard.AgencyType.KING_COUNTY_METRO:
                    case ORCACard.AgencyType.EVERETT_TRANSIT:
                    case ORCACard.AgencyType.PIERCE_TRANSIT:
                        return TransitEventType.Bus;
                    default: return TransitEventType.Other;

                }
            }
        }

        public override string LocalCost
        {
            get
            {
                if (mActionType == ORCACard.CardActionType.TAP_OUT && mEventCost < 1)
                    return "";
                else if (mActionType == ORCACard.CardActionType.CANCEL_TRIP)
                    return "N/A";
                else if (mActionType == ORCACard.CardActionType.USE_PASS)
                    return "PASS";
                else return String.Format(new System.Globalization.CultureInfo("en-US"), "{0:C}", (float)mEventCost / 100.0);
            }
        }


        public static ORCATransitEvent parseRecrd(PhysicalCard.Desfire.DesfireRecord record)
        {
            ORCATransitEvent _event = new ORCATransitEvent();

            byte[] d = record.Data;

            long tStamp = ((0x0F & d[3]) << 28)
                | (d[4] << 20)
                | (d[5] << 12)
                | (d[6] << 4)
                | (d[7] >> 4);

            _event.mTimestamp = DateTimeOffset.FromUnixTimeSeconds(tStamp).DateTime;

            _event.mCoach = ((d[9] & 0xf) << 12) | (d[10] << 4) | ((d[11] & 0xf0) >> 4);

            if (d[15] == 0xFF || d[15] == 0x00)
            {
                // According to FareBot, this is a strange edge-case that has not yet been figured out. 
                _event.mEventCost = 0;
            }
            else
            {
                _event.mEventCost = (d[15] << 7) | (d[16] >> 1);
            }

            _event.mNewBalance = (d[34] << 8) | d[35];
            _event.mTransitAgency = (ORCA.ORCACard.AgencyType)( (byte)( ( d[3] & 0xF0 ) >> 4));
            _event.mActionType = (ORCA.ORCACard.CardActionType)((byte)(d[17]));

            return _event;
        }
    }
}
