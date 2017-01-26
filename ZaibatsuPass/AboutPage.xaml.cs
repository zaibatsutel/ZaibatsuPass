using System;
using System.Collections.Generic;
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
    public sealed partial class AboutPage : Page
    {

        public string VersionInfo
        {
            get
            {
                var ver = Windows.ApplicationModel.Package.Current.Id.Version;


                return String.Format("{0} v{1}.{2}.{3}.{4}",
                    Windows.ApplicationModel.Package.Current.DisplayName,
                    ver.Major,
                    ver.Minor,
                    ver.Build,
                    ver.Revision);
            }
        }

        public AboutPage()
        {
            this.InitializeComponent();
        }
    }
}
