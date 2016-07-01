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
                    case TransitCard.TransitEventType.Ban: return "\xe900;";

                    // forms of transit
                    case TransitCard.TransitEventType.Bus: return "\xe901;";
                    case TransitCard.TransitEventType.Ferry: return "\xe902";
                    case TransitCard.TransitEventType.Trolley: return "\xe908";
                    case TransitCard.TransitEventType.Metro: return "\xe903";
                    case TransitCard.TransitEventType.Train: return "\xe906";
                    case TransitCard.TransitEventType.TicketMachine: return "\xe905";
                    case TransitCard.TransitEventType.Tram: return "\xe907";
                    // things that take money
                    case TransitCard.TransitEventType.VendingMachine:
                    case TransitCard.TransitEventType.POS:
                        return "\xe904";
                    // fallback/other
                    case TransitCard.TransitEventType.Other:
                    default:
                        return "\xe909";
                }
            }
            else return "\xe909";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
