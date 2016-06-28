using Pcsc;
using Pcsc.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.SmartCards;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.ComponentModel;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ZaibatsuPass
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {

        #region __ init page __

        public MainPage()
        {
            this.InitializeComponent();
        }

        SmartCardReader m_reader;

        public event PropertyChangedEventHandler PropertyChanged;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            setupCard();
        }

        private async void setupCard()
        {
            var deviceInfo = await SmartCardReaderUtils.GetFirstSmartCardReaderInfo(SmartCardReaderKind.Nfc);
            if(deviceInfo == null )
            {
                System.Diagnostics.Debug.WriteLine("Failed: No readers!");
                // Bah, device doesn't support NFC. 
                CardStatus = ScanningStatus.Other;
                return;
            }


            if(!deviceInfo.IsEnabled)
            {
                System.Diagnostics.Debug.WriteLine("Failed: NFC is off!");
                CardStatus = ScanningStatus.NFCOff;
                return;
            }

            // At this point, deviceInfo should be legit.

            if(m_reader == null )
            {
                System.Diagnostics.Debug.WriteLine("Setting up card for id " + deviceInfo.Id);
                m_reader = await SmartCardReader.FromIdAsync(deviceInfo.Id);
                var status = await m_reader.GetStatusAsync();
                System.Diagnostics.Debug.WriteLine("CardStatus is " + status);
                switch (status)
                {
                    case SmartCardReaderStatus.Disconnected:
                        CardStatus = ScanningStatus.NFCOff;
                        return;
                    case SmartCardReaderStatus.Exclusive:
                        CardStatus = ScanningStatus.Other;
                        return;
                    case SmartCardReaderStatus.Ready:
                        CardStatus = ScanningStatus.NoCard;
                        m_reader.CardAdded += cardEV_Tap;
                        break;
                    default:
                        break;
                }
            }
        }



        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            tCard = null;
            if(m_reader != null)
            {
                m_reader.CardAdded -= cardEV_Tap;
                m_reader = null;
            }
            base.OnNavigatingFrom(e);
        }

        #endregion


        #region __ local variables and friends __


        private ScanningStatus _cardStatus = ScanningStatus.NoCard;
        private void notifyPropertyChanged(string tProperty)
        {
            if (!Dispatcher.HasThreadAccess)
            {
                var ignored = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { notifyPropertyChanged(tProperty); });
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Call Property Changed");
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(tProperty));
            }
        }
        public ScanningStatus CardStatus
        {
            get { return _cardStatus; }
            private set
            {
                System.Diagnostics.Debug.WriteLine("... set card status: " + value);
                _cardStatus = value;
                notifyPropertyChanged("CardStatus");
            }
        }

        private PhysicalCard.Desfire.DesfireCard tCard { get; set; }

        #endregion

        

       private void setCard(PhysicalCard.Desfire.DesfireCard card)
        {

            if (Dispatcher.HasThreadAccess)
            {
                tCard = card;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("cardInfo"));
            }
            else
            {
                var ign = Dispatcher.RunAsync(
                      Windows.UI.Core.CoreDispatcherPriority.Normal,
                      () => { setCard(card); });
            }

        } 

        #region __ read card event __

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void cardEV_Tap(SmartCardReader sender, CardAddedEventArgs args)
        {
            CardStatus = ScanningStatus.CardScanning;
            try
            {
                System.Diagnostics.Debug.WriteLine("Got card tap event.");
                // woooot
                using (SmartCardConnection connection = await args.SmartCard.ConnectAsync())
                {
                    // figure out what kind of card this is.
                    IccDetection cardIdentification = new IccDetection(args.SmartCard, connection);
                    await cardIdentification.DetectCardTypeAync();
                    System.Diagnostics.Debug.WriteLine("FOUND CARD");
                    System.Diagnostics.Debug.WriteLine(cardIdentification.PcscDeviceClass.ToString());
                    if (cardIdentification.PcscDeviceClass == Pcsc.Common.DeviceClass.MifareDesfire)
                    {
                        Pcsc.Desfire.AccessHandler desfireAccess = new Pcsc.Desfire.AccessHandler(connection);
                        PhysicalCard.Desfire.DesfireCard _card = await ZaibatsuPass.PhysicalCard.Desfire.DesfireCard.SnapshotAsync(desfireAccess);
                                     var ignored = Dispatcher.RunAsync(
                                         Windows.UI.Core.CoreDispatcherPriority.Normal, 
                                         () => {
                                             // try to parse the card
                                             tCard = _card;
                                             TransitCard.TransitCard parsedCard = TransitCard.TransitCardParser.parseTransitCard(tCard);
                                             if (parsedCard == null)
                                                 CardStatus = ScanningStatus.ScanningFailure;
                                             else
                                                 (Window.Current.Content as Frame).Navigate(typeof(DetailsPage), parsedCard);
                                         });
                        CardStatus = ScanningStatus.ScanningSuccess;
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(" ** OOPS: " + e.ToString());
                CardStatus = ScanningStatus.ScanningFailure;
            }

        }

        #endregion
    }
}
