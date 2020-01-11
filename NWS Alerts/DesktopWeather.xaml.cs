using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using WindowPlacementHelper;
using static NWS_Alerts.Properties.Settings;
using static NWS_Alerts.WindowHelper;

namespace NWS_Alerts
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DesktopWeather : Window
    {
        #region References

        public static bool ConnectedToInternet = true;
        static DateTime AlertTime = DateTime.Now.AddSeconds(-DateTime.Now.Second);
        public static DispatcherTimer timer;
        static readonly string LocalDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        // Desktop Weather References
        static XmlDocument xmlDoc;
        static XPathNavigator xPathNavigator;
        static XPathNavigator LargeTileInfoNode;
        public static readonly string PreWeatherString = "https://tile-service.weather.microsoft.com/livetile/front/";
        public static bool Updating = false;
        public static string WeatherInstallLocation = string.Empty;
        static readonly string Assets = @"\Assets\AppTiles\";
        public static FileInfo fileInfo;
        public static DateTime FileTime;

        #endregion References

        #region Main Entry Point
        /// <summary>
        /// Reference: 48.85,2.34 - Paris France
        /// Reference: 38.62,-90.19 - Saint Louis, MO
        /// </summary>
        public DesktopWeather()
        {
            InitializeComponent();

            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\PackageRepository\Extensions\ProgIDs\AppXgtz62cfp9761w8h33sbaykyt1vkbm4vj");

            foreach (string vName in key.GetValueNames())
            {
                if (vName.Contains("BingWeather"))
                {
                    if (Directory.Exists(@"C:\Program Files\WindowsApps\" + vName))
                    {
                        WeatherInstallLocation = @"C:\Program Files\WindowsApps\" + vName;
                    }
                    else if (Directory.Exists(@"C:\Program Files (x86)\WindowsApps\" + vName))
                    {
                        WeatherInstallLocation = @"C:\Program Files (x86)\WindowsApps\" + vName;
                    }
                }
                else
                {
                    MessageBox.Show("You need to install and run Bing Weather at least once in order for this program to work correctly.", "NWS Alerts", MessageBoxButton.OK, MessageBoxImage.Information);

                    Application.Current.Shutdown();
                }
            }

            Closing += DesktopWeather_Closing;

            LoadSettings();
        }

        #endregion End Main Entry Point

        #region Net Check

        private void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (CheckForInternet.Connected())
                {
                    NoInternetWarning.Source = null;

                    ConnectedToInternet = true;

                    if (((MainWindow)Application.Current.MainWindow).AlertTextBody.Text == "You may not be connected to the internet...")
                    {
                        ParseWeatherXML.NWSAlertsInfo();
                    }
                }
                else
                {
                    ConnectedToInternet = false;

                    Thread thread = new Thread(EarlyNetCheck)
                    {
                        IsBackground = true
                    };
                    thread.Start();

                    NoInternetWarning.Source = new BitmapImage(new Uri(@"pack://application:,,,/NWS Alerts;component/Resources/NoInternetWarning.png", UriKind.Absolute));

                    EasyLogger.Info("It appears that you do not have an internet connection. We will retry later.");
                }
            }));
        }

        public void EarlyNetCheck()
        {
            Thread.Sleep(6000);

            while (true)
            {
                if (CheckForInternet.Connected())
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        NoInternetWarning.Source = null;

                        ConnectedToInternet = true;
                    }));

                    break;
                }
                else
                {
                    Thread.Sleep(6000);
                }
            }
        }

        #endregion Net Check

        #region Parse Weather

        public void DesktopWeatherXML(string url, string savePath)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {

                try
                {
                    XDocument xml = XDocument.Load(url);

                    xml.Save(savePath);
                }
                catch (Exception ex)
                {
                    ConnectedToInternet = false;

                    EasyLogger.Info("It appears that you do not have an internet connection. We will retry later. The actual error message is: " + Environment.NewLine + Environment.NewLine + "     " + ex.Message + Environment.NewLine);
                }

                if (File.Exists(LocalDirectory + "\\WeatherInfo.xml"))
                {
                    EasyLogger.Info("New weather information downloaded and saved successfully");

                    fileInfo = new FileInfo(LocalDirectory + "\\WeatherInfo.xml");

                    FileTime = fileInfo.LastWriteTime;
                }
                else
                {
                    EasyLogger.Info("It appears you are not connected to the internet. Please check your internet connection and try again.");

                    MessageBox.Show("It appears you are not connected to the internet. Please check your internet connection and try again.", "NWS Alerts", MessageBoxButton.OK, MessageBoxImage.Information);

                    NotifyTray.notifyIcon.Visible = false;

                    Environment.Exit(0);
                }

                SetWeatherUI();

            }));
        }

        public void SetWeatherUI()
        {
            EasyLogger.Info("Updating Desktop Weather info using WeatherInfo.xml");

            using (FileStream fileStream = new FileStream(LocalDirectory + "\\WeatherInfo.xml",
                                      FileMode.Open,
                                      FileAccess.Read,
                                      FileShare.ReadWrite))
            {
                xmlDoc = new XmlDocument();
                xmlDoc.Load(fileStream);

                xPathNavigator = xmlDoc.CreateNavigator();

                // Location
                LargeTileInfoNode = xPathNavigator.SelectSingleNode(@"(//tile/visual/binding[@template='TileLarge'])");

                LocationLabel.Content = LargeTileInfoNode.GetAttribute("DisplayName", "");

                // Background Image
                LargeTileInfoNode = xPathNavigator.SelectSingleNode(@"(//tile/visual/binding[@template='TileLarge']/image)");

                Uri uri = new Uri(WeatherInstallLocation + Assets + LargeTileInfoNode.GetAttribute("src", "").Replace("?a", "").Replace("/", "\\"), UriKind.Absolute);

                windowBackground.Source = new BitmapImage(uri);
                Background.Opacity = OpacitySlider.Value;
                windowBackground.Opacity = OpacitySlider.Value;

                SettingsPanel.Background = new ImageBrush(windowBackground.Source);
                SettingsPanel.Background.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.HighQuality);


                // Current Conditions
                LargeTileInfoNode = xPathNavigator.SelectSingleNode(@"(//tile/visual/binding[@template='TileLarge']/text)");
                CurrentConditions.Content = LargeTileInfoNode.Value;

                // Current Temps
                LargeTileInfoNode = xPathNavigator.SelectSingleNode(@"(//group/subgroup[@hint-weight='100']/text)");

                CurrentTemp.Content = LargeTileInfoNode.Value.Replace("Â", "");


                // Day1 Forecast Day
                LargeTileInfoNode = xPathNavigator.SelectSingleNode(@"(//group/subgroup[@hint-weight='18']/text)[1]");

                day1.Content = LargeTileInfoNode.Value;


                // Day1 Forecast Image
                LargeTileInfoNode = xPathNavigator.SelectSingleNode(@"(//group/subgroup[@hint-weight='18']/image)[1]");
                image1.Source = new BitmapImage(new Uri(WeatherInstallLocation + Assets + LargeTileInfoNode.GetAttribute("src", "").Replace("?a", "").Replace("/", "\\"), UriKind.Absolute));

                // Day1 Forecast High
                LargeTileInfoNode = xPathNavigator.SelectSingleNode(@"(//group/subgroup[@hint-weight='18']/text)[2]");

                high1.Content = LargeTileInfoNode.Value.Replace("Â", "");


                // Day1 Forecast Low
                LargeTileInfoNode = xPathNavigator.SelectSingleNode(@"(//group/subgroup[@hint-weight='18']/text)[3]");

                low1.Content = LargeTileInfoNode.Value.Replace("Â", "");


                // Day2 Forecast Day
                LargeTileInfoNode = xPathNavigator.SelectSingleNode(@"(//group/subgroup[@hint-weight='18']/text)[4]");

                day2.Content = LargeTileInfoNode.Value;


                // Day2 Forecast Image
                LargeTileInfoNode = xPathNavigator.SelectSingleNode(@"(//group/subgroup[@hint-weight='18']/image)[2]");

                image2.Source = new BitmapImage(new Uri(WeatherInstallLocation + Assets + LargeTileInfoNode.GetAttribute("src", "").Replace("?a", "").Replace("/", "\\"), UriKind.Absolute));


                // Day2 Forecast High
                LargeTileInfoNode = xPathNavigator.SelectSingleNode(@"(//group/subgroup[@hint-weight='18']/text)[5]");

                high2.Content = LargeTileInfoNode.Value.Replace("Â", "");


                // Day2 Forecast Low
                LargeTileInfoNode = xPathNavigator.SelectSingleNode(@"(//group/subgroup[@hint-weight='18']/text)[6]");

                low2.Content = LargeTileInfoNode.Value.Replace("Â", "");


                // Day3 Forecast Day
                LargeTileInfoNode = xPathNavigator.SelectSingleNode(@"(//group/subgroup[@hint-weight='18']/text)[7]");

                day3.Content = LargeTileInfoNode.Value;


                // Day3 Forecast Image
                LargeTileInfoNode = xPathNavigator.SelectSingleNode(@"(//group/subgroup[@hint-weight='18']/image)[3]");

                image3.Source = new BitmapImage(new Uri(WeatherInstallLocation + Assets + LargeTileInfoNode.GetAttribute("src", "").Replace("?a", "").Replace("/", "\\"), UriKind.Absolute));


                // Day3 Forecast High
                LargeTileInfoNode = xPathNavigator.SelectSingleNode(@"(//group/subgroup[@hint-weight='18']/text)[8]");

                high3.Content = LargeTileInfoNode.Value.Replace("Â", "");


                // Day3 Forecast Low
                LargeTileInfoNode = xPathNavigator.SelectSingleNode(@"(//group/subgroup[@hint-weight='18']/text)[9]");

                low3.Content = LargeTileInfoNode.Value.Replace("Â", "");


                // Day4 Forecast Day
                LargeTileInfoNode = xPathNavigator.SelectSingleNode(@"(//group/subgroup[@hint-weight='18']/text)[10]");

                day4.Content = LargeTileInfoNode.Value;


                // Day4 Forecast Image
                LargeTileInfoNode = xPathNavigator.SelectSingleNode(@"(//group/subgroup[@hint-weight='18']/image)[4]");

                image4.Source = new BitmapImage(new Uri(WeatherInstallLocation + Assets + LargeTileInfoNode.GetAttribute("src", "").Replace("?a", "").Replace("/", "\\"), UriKind.Absolute));


                // Day4 Forecast High
                LargeTileInfoNode = xPathNavigator.SelectSingleNode(@"(//group/subgroup[@hint-weight='18']/text)[11]");

                high4.Content = LargeTileInfoNode.Value.Replace("Â", "");


                // Day4 Forecast Low
                LargeTileInfoNode = xPathNavigator.SelectSingleNode(@"(//group/subgroup[@hint-weight='18']/text)[12]");

                low4.Content = LargeTileInfoNode.Value.Replace("Â", "");


                // Day5 Forecast Day
                LargeTileInfoNode = xPathNavigator.SelectSingleNode(@"(//group/subgroup[@hint-weight='18']/text)[13]");

                day5.Content = LargeTileInfoNode.Value;


                // Day5 Forecast Image
                LargeTileInfoNode = xPathNavigator.SelectSingleNode(@"(//group/subgroup[@hint-weight='18']/image)[5]");

                image5.Source = new BitmapImage(new Uri(WeatherInstallLocation + Assets + LargeTileInfoNode.GetAttribute("src", "").Replace("?a", "").Replace("/", "\\"), UriKind.Absolute));


                // Day5 Forecast High
                LargeTileInfoNode = xPathNavigator.SelectSingleNode(@"(//group/subgroup[@hint-weight='18']/text)[14]");

                high5.Content = LargeTileInfoNode.Value.Replace("Â", "");


                // Day5 Forecast Low
                LargeTileInfoNode = xPathNavigator.SelectSingleNode(@"(//group/subgroup[@hint-weight='18']/text)[15]");

                low5.Content = LargeTileInfoNode.Value.Replace("Â", "");

                NotifyTray.notifyIcon.Text = (string)LocationLabel.Content + Environment.NewLine + (string)CurrentConditions.Content + " " + (string)CurrentTemp.Content;

                Updating = false;
            }
        }

        #endregion Parse Weather

        #region Time and Date

        public void DateTimeSettings()
        {
            timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                if (Default.ShowDate)
                {
                    ShowDateCheckBox.IsChecked = true;
                    CurrentDate.Content = DateTime.Now.ToString(@"dddd, MMM d");

                    ShowDateCheckBox.Checked += ShowDateCheckBox_Checked;
                    ShowDateCheckBox.Unchecked += ShowDateCheckBox_Unchecked;
                }
                if (Default.ShowTime)
                {
                    ShowTimeCheckBox.IsChecked = true;
                    CurrentTime.Content = DateTime.Now.ToString(@"h:mm tt");

                    ShowTimeCheckBox.Checked += ShowTimeCheckBox_Checked;
                    ShowTimeCheckBox.Unchecked += ShowTimeCheckBox_Unchecked;
                }
                if (UpdateTimer.ReturnDateTime(DateTime.Now.AddMinutes(-30), FileTime) && !Updating && ConnectedToInternet)
                {
                    Updating = true;

                    EasyLogger.Info("Downloading new weather info using values: " + Default.LatValue + "," + Default.LongValue);

                    Thread thread = new Thread(() => DesktopWeatherXML(PreWeatherString + Default.LatValue + "," + Default.LongValue, LocalDirectory + "\\WeatherInfo.xml"))
                    {
                        IsBackground = true
                    };
                    thread.Start();
                }
                if (UpdateTimer.ReturnDateTime(DateTime.Now.AddMinutes(-10), AlertTime))
                {
                    AlertTime = DateTime.Now.AddSeconds(-DateTime.Now.Second);

                    if (CheckForInternet.Connected())
                    {
                        ParseWeatherXML.NWSAlertsInfo();
                    }
                }
            }, Dispatcher);
        }

        #endregion Time and Date

        #region Loading and Rendering

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            WindowPlacement.SetWindowByResolution(window, false);

            SystemEvents.DisplaySettingsChanged += new EventHandler(this.SystemEvens_DisplaySettingsChanged);
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            EnableBlurEffect(window);

            NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
        }

        private void LoadSettings()
        {
            window.Background.Opacity = Default.WindowOpacity;
            windowBackground.Opacity = Default.WindowOpacity;

            LatTextBox.Text = Default.LatValue;
            LongTextBox.Text = Default.LongValue;

            if (Default.UseAccentBorder)
            {
                window.BorderBrush = SystemParameters.WindowGlassBrush;

                BorderAccentCheckBox.IsChecked = true;

                SystemParameters.StaticPropertyChanged += WindowGlassBrush_Changed;
            }
            else
            {
                window.BorderThickness = new Thickness(0, 0, 0, 0);
            }

            OpacitySlider.Value = Default.WindowOpacity;

            if (OpacitySlider.Value < 0.95)
            {
                Overlay.Opacity = 0.1;
            }
            else
            {
                Overlay.Opacity = 0.3;
            }

            VersionLabel.Content = "xCONFLiCTiONx  |  Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString();

            BorderAccentCheckBox.Checked += BorderAccentCheckBox_Checked;
            BorderAccentCheckBox.Unchecked += BorderAccentCheckBox_Unchecked;

            Grid1.Children.Remove(SettingsPanel);

            if (!CheckForInternet.Connected())
            {
                NoInternetWarning.Source = new BitmapImage(new Uri(@"pack://application:,,,/NWS Alerts;component/Resources/NoInternetWarning.png", UriKind.Absolute));

                ConnectedToInternet = false;

                Thread thread = new Thread(EarlyNetCheck)
                {
                    IsBackground = true
                };
                thread.Start();
            }

            if (File.Exists(LocalDirectory + "\\WeatherInfo.xml"))
            {
                fileInfo = new FileInfo(LocalDirectory + "\\WeatherInfo.xml");

                FileTime = fileInfo.LastWriteTime;

                SetWeatherUI();
            }

            DateTimeSettings();

            OpacitySlider.ValueChanged += OpacitySlider_ValueChanged;

            MouseUp += MainWindow_MouseUp;
        }

        private void SystemEvens_DisplaySettingsChanged(object sender, EventArgs e)
        {
            WindowPlacement.SetWindowByResolution(window, false);
        }

        #endregion Loading and Rendering

        #region Event Subs

        private void MainWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SaveSettings();
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            EasyLogger.Info("Setting background opacity to: " + Default.WindowOpacity);

            Default.WindowOpacity = OpacitySlider.Value;

            window.Background.Opacity = OpacitySlider.Value;
            windowBackground.Opacity = OpacitySlider.Value;

            if (OpacitySlider.Value < 0.95)
            {
                Overlay.Opacity = 0.1;
            }
            else
            {
                Overlay.Opacity = 0.3;
            }
        }

        private void BorderAccentCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            window.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            SystemParameters.StaticPropertyChanged -= WindowGlassBrush_Changed;
            window.BorderThickness = new Thickness(0, 0, 0, 0);
            Default.UseAccentBorder = false;
        }

        private void BorderAccentCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            window.BorderBrush = SystemParameters.WindowGlassBrush;
            SystemParameters.StaticPropertyChanged += WindowGlassBrush_Changed;
            window.BorderThickness = new Thickness(1, 1, 1, 1);
            Default.UseAccentBorder = true;
        }

        private void WindowGlassBrush_Changed(object sender, PropertyChangedEventArgs e)
        {
            window.BorderBrush = SystemParameters.WindowGlassBrush;
        }

        private void CloseSettings_Click(object sender, RoutedEventArgs e)
        {
            bool UpdateWeather = false;

            if (LatTextBox.Text != Default.LatValue)
            {
                UpdateWeather = true;
            }

            if (LongTextBox.Text != Default.LongValue)
            {
                UpdateWeather = true;
            }

            SaveSettings();

            if (UpdateWeather)
            {
                EasyLogger.Info("Current location has changed... ");

                Updating = true;

                Thread thread = new Thread(() => DesktopWeatherXML(PreWeatherString + Default.LatValue + "," + Default.LongValue, LocalDirectory + "\\WeatherInfo.xml"))
                {
                    IsBackground = true
                };
                thread.Start();
            }

            Grid1.Children.Remove(SettingsPanel);
        }

        private void CloseApplication_Click(object sender, RoutedEventArgs e)
        {
            CloseApp();
        }

        public static void ShowTimeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            EasyLogger.Info("Setting time to show");

            Default.ShowTime = true;

            if (Default.ShowDate)
            {
                timer.Start();
            }
        }

        public void ShowTimeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            EasyLogger.Info("Setting time not to show");

            CurrentTime.Content = "";

            Default.ShowTime = false;

            if (!Default.ShowDate)
            {
                timer.Stop();
            }
        }

        public static void ShowDateCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            EasyLogger.Info("Setting date to show");

            Default.ShowDate = true;

            if (Default.ShowTime)
            {
                timer.Start();
            }
        }

        private void ShowDateCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            EasyLogger.Info("Setting date not to show");

            CurrentDate.Content = "";

            Default.ShowDate = false;

            if (!Default.ShowTime)
            {
                timer.Stop();
            }
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
            if (e.ChangedButton == MouseButton.Right)
            {
                if (!Grid1.Children.Contains(SettingsPanel))
                {
                    Grid1.Children.Add(SettingsPanel);
                }
            }
        }

        private void NoInternetWarning_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Thread thread = new Thread(EarlyNetCheck)
            {
                IsBackground = true
            };
            thread.Start();
        }

        #endregion Event Subs

        #region Close, Save and Cleanup

        private void CloseApp()
        {
            SaveSettings();

            SystemParameters.StaticPropertyChanged -= WindowGlassBrush_Changed;

            BorderAccentCheckBox.Checked -= BorderAccentCheckBox_Checked;
            BorderAccentCheckBox.Unchecked -= BorderAccentCheckBox_Unchecked;

            OpacitySlider.ValueChanged -= OpacitySlider_ValueChanged;

            NetworkChange.NetworkAddressChanged -= NetworkChange_NetworkAddressChanged;

            MouseUp -= MainWindow_MouseUp;

            NotifyTray.notifyIcon.Visible = false;

            Environment.Exit(0);
        }

        private void SaveSettings()
        {
            WindowPlacement.SetWindowByResolution(window, true);

            Default.LatValue = LatTextBox.Text;
            Default.LongValue = LongTextBox.Text;

            Default.ShowDate = (bool)ShowDateCheckBox.IsChecked;
            Default.ShowTime = (bool)ShowTimeCheckBox.IsChecked;

            Default.UseAccentBorder = (bool)BorderAccentCheckBox.IsChecked;

            Default.WindowOpacity = window.Background.Opacity;

            Default.Save();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            CloseApp();
        }

        private void DesktopWeather_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            CloseApp();
        }

        #endregion Close, Save and Cleanup
    }
}