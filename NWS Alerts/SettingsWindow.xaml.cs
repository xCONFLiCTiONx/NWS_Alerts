using System.Collections;
using System.Windows;
using System.Windows.Input;
using static NWS_Alerts.Properties.Settings;

namespace NWS_Alerts
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        #region Main
        public SettingsWindow()
        {
            InitializeComponent();

            foreach (var item in Default.IgnoreAlerts)
            {
                ListIgnoreAlerts.Items.Add(item);
            }

            foreach (var item in Default.ShowCounties)
            {
                ListShowCounties.Items.Add(item);
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion Main


        #region Buttons Remove

        private void ListRemoveIgnoreAlerts_Click(object sender, RoutedEventArgs e)
        {
            if (ListIgnoreAlerts.SelectedItem != null)
            {
                foreach (var item in Default.IgnoreAlerts)
                {
                    if (item == ListIgnoreAlerts.SelectedItem.ToString())
                    {
                        Default.IgnoreAlerts.Remove(item);
                        break;
                    }
                }
                Default.Save();
                Default.Reload();

                MainWindow.SettingsChanged = true;

                ListIgnoreAlerts.Items.Clear();

                foreach (var item in Default.IgnoreAlerts)
                {
                    ListIgnoreAlerts.Items.Add(item);
                }
            }
        }

        private void ListRemoveShowCounties_Click(object sender, RoutedEventArgs e)
        {
            if (ListShowCounties.SelectedItem != null)
            {
                foreach (var item in Default.ShowCounties)
                {
                    if (item == ListShowCounties.SelectedItem.ToString())
                    {
                        Default.ShowCounties.Remove(item);
                        break;
                    }
                }
                Default.Save();
                Default.Reload();

                MainWindow.SettingsChanged = true;

                ListShowCounties.Items.Clear();

                foreach (var item in Default.ShowCounties)
                {
                    ListShowCounties.Items.Add(item);
                }
            }
        }

        #endregion Buttons Remove


        #region Buttons Add

        private void ListAddIgnoreAlerts_Click(object sender, RoutedEventArgs e)
        {
            AddToAlertsIgnore();
        }

        private void ListAddShowCounties_Click(object sender, RoutedEventArgs e)
        {
            AddToCountiesShow();
        }

        #endregion Buttons Add


        #region TextBox Entries

        private void TextBoxAddIgnoreAlerts_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddToAlertsIgnore();
            }
        }

        private void TextBoxAddShowCounties_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddToCountiesShow();
            }
        }

        #endregion TextBox Entries


        #region Methods

        private void AddToCountiesShow()
        {
            bool ItemExists = false;
            if (TextBoxAddShowCounties.Text != "")
            {
                foreach (string item in Default.ShowCounties)
                {
                    if (item.ToString() == TextBoxAddShowCounties.Text)
                    {
                        ItemExists = true;
                        break;
                    }
                }
                if (!ItemExists)
                {
                    Default.ShowCounties.Add(TextBoxAddShowCounties.Text);

                    ArrayList.Adapter(Default.ShowCounties).Sort();

                    Default.Save();
                    Default.Reload();

                    MainWindow.SettingsChanged = true;

                    ListShowCounties.Items.Clear();

                    foreach (var item in Default.ShowCounties)
                    {
                        ListShowCounties.Items.Add(item);
                    }
                }

                TextBoxAddShowCounties.Clear();
            }
        }

        private void AddToAlertsIgnore()
        {
            bool ItemExists = false;
            if (TextBoxAddIgnoreAlerts.Text != "")
            {
                foreach (var item in Default.IgnoreAlerts)
                {
                    if (item.ToString() == TextBoxAddIgnoreAlerts.Text)
                    {
                        ItemExists = true;
                        break;
                    }
                }
                if (!ItemExists)
                {
                    Default.IgnoreAlerts.Add(TextBoxAddIgnoreAlerts.Text);

                    ArrayList.Adapter(Default.IgnoreAlerts).Sort();

                    Default.Save();
                    Default.Reload();

                    MainWindow.SettingsChanged = true;

                    ListIgnoreAlerts.Items.Clear();

                    foreach (var item in Default.IgnoreAlerts)
                    {
                        ListIgnoreAlerts.Items.Add(item);
                    }
                }

                TextBoxAddIgnoreAlerts.Clear();
            }
        }

        #endregion Methods
    }
}
