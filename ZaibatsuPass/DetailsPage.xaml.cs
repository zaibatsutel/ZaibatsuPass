using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ZaibatsuPass
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DetailsPage : Page,INotifyPropertyChanged 
    {
        public DetailsPage()
        {
            this.InitializeComponent();
        }

        TransitCard.TransitCard inspectedCard { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            inspectedCard = e.Parameter as TransitCard.TransitCard;

            if (inspectedCard == null)
                return;

            // There are certain cards which are "Stub" cards
            // These cards just don't have any information
            if(inspectedCard.isStub)
            {
                infoPivot.Items.Remove(pi_Balance);
                infoPivot.Items.Remove(pi_Events);
                infoPivot.Items.Remove(pi_Extras);
            }
            else
            {
                infoPivot.Items.Remove(pi_stub);
                if (!inspectedCard.hasEvents)
                    infoPivot.Items.Remove(pi_Events);
                if (!inspectedCard.hasExtras)
                    infoPivot.Items.Remove(pi_Extras);
            }

            // As a result of being bound through x:bind, I need this.
            // Blaaah
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("inspectedCard.Events"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("inspectedCard.Name"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("inspectedCard.SerialNumber"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("inspectedCard.Balance"));


            base.OnNavigatedTo(e);
        }
    }
}
