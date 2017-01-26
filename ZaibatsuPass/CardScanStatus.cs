using System;
using System.Collections.Generic;

namespace ZaibatsuPass
{
    public enum ScanningStatus
    {
        NoCard,
        CardScanning,
        ScanningFailure,
        ScanningSuccess,
        ParsingCard,
        UnknownCard,
        NFCOff,
        Other
    }


    public class StatusConverter : Windows.UI.Xaml.Data.IValueConverter
    {

        private Dictionary<String, Object> words = new Dictionary<string, Object>();

        public StatusConverter()
        {
            // defaults
            words.Add("default.Title", "? title ?");
            words.Add("default.ShowSpinner", false);
            words.Add("default.Subtitle", "? subtitle ?");

            // Waiting to scan card.
            words.Add("NoCard.Title", "Tap a card");
            words.Add("NoCard.Subtitle", "Make sure NFC is on");
            // Scanning the card itself.
            words.Add("CardScanning.Title", "Give us a second");
            words.Add("CardScanning.Subtitle", "Scanning card...");
            words.Add("CardScanning.ShowSpinner", true);
            // Scanning was OK
            words.Add("ScanningSuccess.Title", "OK!");
            words.Add("ScanningSuccess.Subtitle", "Card GET!");
            // Scanning was not OK
            words.Add("ScanningFailure.Title", "Oops...");
            words.Add("ScanningFailure.Subtitle", "That didn't work. Try again.");
            // NFC is off.
            words.Add("NFCOff.Title", "NFC is off");
            words.Add("NFCOff.Subtitle", "You should go turn that on.");

            // Parsing the card
            words.Add("ParsingCard.Title", "Almost there");
            words.Add("ParsingCard.Subtitle", "Hang on tight");
            words.Add("ParsingCard.ShowSpinner", true);
            // Cards that are unkonwn to us.
            words.Add("UnknownCard.Title", "Oops...");
            words.Add("UnkonwnCard.Subtitle", "I don't speak that card's language.");

            // The "Other" state was used.
            words.Add("Other.Title", "Oops...");
            words.Add("Other.Subtitle", "I can't talk to your NFC reader. Is another app using it?");

        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ScanningStatus == false) { return "!! " + value.ToString() + " ??"; }
            ScanningStatus cStatus = (ScanningStatus)value;
            String translationToken = cStatus.ToString() + "." + (string)parameter;
            if(!words.ContainsKey(translationToken))
            {
                if (words.ContainsKey("default." + parameter)) return words["default." + parameter];
                else if (targetType == typeof(Boolean)) return false;
                else if (targetType == typeof(String)) return translationToken;
                else if (targetType == typeof(int)) return 0;
                else return null;
            } 
            else return words[translationToken];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
