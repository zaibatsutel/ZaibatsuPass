using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZaibatsuPass.Converters
{
    class EventGlyphConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ZaibatsuPass.TransitCard.TransitEventType)
            {
                switch ((TransitCard.TransitEventType)value)
                {
                    // banned card
                    case TransitCard.TransitEventType.Ban: return "j";

                    // forms of transit
                    case TransitCard.TransitEventType.Bus: return "a";
                    case TransitCard.TransitEventType.Ferry: return "c";
                    case TransitCard.TransitEventType.Trolley: return "b";
                    case TransitCard.TransitEventType.Metro: return "d";
                    case TransitCard.TransitEventType.Train: return "e";
                    case TransitCard.TransitEventType.Tram: return "f";
                    case TransitCard.TransitEventType.TicketMachine: return "g";
                    // things that take money
                    case TransitCard.TransitEventType.VendingMachine:
                    case TransitCard.TransitEventType.POS:
                        return "h";
                    // fallback/other
                    case TransitCard.TransitEventType.Other:
                    default:
                        return "i";
                }
            }
            else return "i";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
