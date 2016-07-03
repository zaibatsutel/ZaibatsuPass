using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZaibatsuPass.TransitCard.ORCA
{
    class ORCATransitEvent : TransitEvent
    {
        DateTime _when;
        long _coach;
        long _cost;
        long _newBalance;


        byte transitType;
        byte Agency;

        public override string EventTitle
        {
            get
            {
                return string.Format("Auth = {0} Coach = {1}",Agency,_coach);
            }
        }

        public override string EventDetails
        {
            get
            {
                return String.Format(new System.Globalization.CultureInfo("en-US"), "Cost = {0:C} newBalance = {1:C} ", ((float)_cost)/100.0, ((float)_newBalance)/100.0 );
            }

        }

        public override string LocalCost
        {
            get
            {
                return String.Format(new System.Globalization.CultureInfo("en-US"), "{0:C}", (float)_cost / 100.0);
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

            _event._when = DateTimeOffset.FromUnixTimeSeconds(tStamp).DateTime;

            _event._coach = ((d[9] & 0xf) << 12) | (d[10] << 4) | ((d[11] & 0xf0) >> 4);

            if (d[15] == 0xFF || d[15] == 0x00)
            {
                // According to FareBot, this is a strange edge-case that has not yet been figured out. 
                _event._cost = 0;
            }
            else
            {
                _event._cost = (d[15] << 7) | (d[16] >> 1);
            }

            _event._newBalance = (d[34] << 8) | d[35];
            _event.Agency = (byte)( ( d[3] & 0xF0 ) >> 4);
            _event.transitType = (byte)(d[17]);

            return _event;
        }
    }
}
