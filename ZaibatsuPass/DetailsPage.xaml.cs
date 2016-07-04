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

            /*
            CardTypeTB.Text = inspectedCard.Name;
            CardSerialTB.Text = inspectedCard.SerialNumber;
            CardBalanceTB.Text = inspectedCard.Balance;

            TripList.ItemsSource = inspectedCard.Events;
            */

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("inspectedCard.Events"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("inspectedCard.Name"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("inspectedCard.SerialNumber"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("inspectedCard.Balance"));


            base.OnNavigatedTo(e);
        }
    }
}
