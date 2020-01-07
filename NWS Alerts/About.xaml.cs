using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace NWS_Alerts
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();

            VersionText.Content = "NWS Alerts   -   xCONFLiCTiONx   -   Version: " + Assembly.GetEntryAssembly().GetName().Version;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
