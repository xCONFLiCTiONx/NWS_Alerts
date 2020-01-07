using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using static NWS_Alerts.Properties.Settings;
using MessageBox = System.Windows.Forms.MessageBox;

namespace NWS_Alerts
{
    /// <summary>
    /// Interaction logic for LocationBox.xaml
    /// </summary>
    public partial class LocationBox : Window
    {
        public LocationBox()
        {
            try
            {
                InitializeComponent();

                LocationState.Text = Default.SetState;

                LocationUrl.Text = Default.AlertFeedUrl;

                LocationState.Focus();

                LocationState.SelectAll();

                LocationState.GotKeyboardFocus += LocationState_GotKeyboardFocus;

                LocationUrl.GotKeyboardFocus += LocationState_GotKeyboardFocus;

                LocationState.TextChanged += LocationState_TextChanged;
                LocationUrl.TextChanged += LocationUrl_TextChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
        }

        private void LocationState_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (LocationState.Text.Length > 2)
            {
                MessageBox.Show("Either put the abbreviated state in this box or press the help button and find your url manually and put the link in the Atom Feed field.", "NWS Alerts", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            LocationUrl.TextChanged -= LocationUrl_TextChanged;

            LocationUrl.Text = "https://alerts.weather.gov/cap/" + LocationState.Text.ToLower() + ".php?x=0";

            LocationUrl.TextChanged += LocationUrl_TextChanged;
        }

        private void LocationUrl_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            LocationState.TextChanged -= LocationState_TextChanged;

            LocationState.Clear();

            LocationState.TextChanged += LocationState_TextChanged;
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult result = MessageBox.Show("Either put your state abbreviated in the state box or paste the Atom feed link into the Text Box and press OK." + Environment.NewLine + Environment.NewLine + "Would you like to open your browser to https://alerts.weather.gov/ and find your location?" + Environment.NewLine + Environment.NewLine + "Remember to right click on the Weather Tile and insert your lattitude and longitude also.", "NWS Alerts", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                Process.Start("https://alerts.weather.gov/");
            }
        }

        private void CloseAppButton_Click(object sender, RoutedEventArgs e)
        {
            if (LocationState.Text != string.Empty || LocationUrl.Text != string.Empty)
            {
                SaveAndClose();
            }
        }

        private void LocationState_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (LocationState.Text != string.Empty || LocationUrl.Text != string.Empty)
                {
                    SaveAndClose();
                }
            }
        }

        private void SaveAndClose()
        {
            Default.SetState = LocationState.Text;
            Default.AlertFeedUrl = LocationUrl.Text;

            Default.Save();

            MainWindow.SettingsChanged = true;

            Close();
        }

        private void LocationState_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)sender;
            Dispatcher.BeginInvoke(new Action(() => tb.SelectAll()));
        }

        private void CancelAppButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.SettingsChanged = false;

            Close();
        }
    }
}
