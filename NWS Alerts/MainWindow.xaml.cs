using DesktopToast;
using NotificationsExtensions;
using NotificationsExtensions.Toasts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using static NWS_Alerts.Properties.Settings;
using Application = System.Windows.Application;
using Label = System.Windows.Controls.Label;
using MessageBox = System.Windows.MessageBox;

namespace NWS_Alerts
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region References
        public DesktopWeather WeatherApp { get; set; }
        public static bool SettingsChanged = false;

        // Windows 10 Toast
        private const string MessageId = "Message";
        public string MessageTitle = "National Weather Service";
        public string MessageBody { get; set; }

        #endregion References

        #region Main Entry Point

        public MainWindow()
        {
            InitializeComponent();

            EasyLogger.BackupLogs(EasyLogger.LogFile);
            EasyLogger.AddListener(EasyLogger.LogFile);

            ((MainWindow)Application.Current.MainWindow).mainWindow.StateChanged += MainWindow_StateChanged;

            HideThisWindow();

            Closing += MainWindow_Closing;

            if (Default.UpgradeRequired)
            {
                Default.Upgrade();

                Default.UpgradeRequired = false;

                Default.Save();

                Default.Reload();

                if (Default.FirstRun)
                {
                    Default.FirstRun = false;

                    SettingsWindow settingsWindow = new SettingsWindow();
                    settingsWindow.ShowDialog();

                    LocationBox locationBox = new LocationBox();
                    locationBox.ShowDialog();

                    MessageBox.Show("Right click on the Desktop App to change the weather location to your preferred location." + Environment.NewLine + Environment.NewLine + "You will need to find your lattitude and longitude in order for me to find your local weather.", "NWS Alerts", MessageBoxButton.OK, MessageBoxImage.Information);

                    Default.Save();

                    Default.Reload();
                }
            }

            NotifyTray.TrayIconCreate();

            if (Default.MuteToast)
            {
                NotifyTray.MuteMenuItem.Checked = true;
                ParseWeatherXML.MuteAlerts = true;
            }

            NotificationActivatorBase.RegisterComType(typeof(NotificationActivator), OnActivated);

            NotificationHelper.RegisterComServer(typeof(NotificationActivator), Assembly.GetExecutingAssembly().Location);

            if (CheckForInternet.Connected())
            {
                ParseWeatherXML.NWSAlertsInfo();
            }
            else
            {
                ((MainWindow)Application.Current.MainWindow).AlertTextBody.Text = "You may not be connected to the internet...";
            }

            WeatherApp = new DesktopWeather();
            WeatherApp.Show();
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            RenderWindow();
        }

        private async void RenderWindow()
        {
            await ((MainWindow)Application.Current.MainWindow).Dispatcher.InvokeAsync(() =>
            {
                if (((MainWindow)Application.Current.MainWindow).mainWindow.WindowState == WindowState.Normal)
                {
                    ((MainWindow)Application.Current.MainWindow).mainWindow.Height = ((MainWindow)Application.Current.MainWindow).AlertTextTitle.ActualHeight + ((MainWindow)Application.Current.MainWindow).AlertTextBody.ActualHeight + 165;

                    Point location = new Point((Screen.PrimaryScreen.WorkingArea.Width - ((MainWindow)Application.Current.MainWindow).mainWindow.Width) / 2,
                                     (Screen.PrimaryScreen.WorkingArea.Height - ((MainWindow)Application.Current.MainWindow).mainWindow.Height) / 2);

                    ((MainWindow)Application.Current.MainWindow).mainWindow.Top = location.Y;
                    ((MainWindow)Application.Current.MainWindow).mainWindow.Left = location.X;
                }
            });
        }

        #endregion Entry Point

        #region Windows 10 Toast

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            NotificationActivatorBase.UnregisterComType();
        }

        private void OnActivated(string arguments, Dictionary<string, string> data)
        {
            var result = "Activated";
            if ((arguments?.StartsWith("action=")).GetValueOrDefault())
            {
                result = arguments.Substring("action=".Length);

                if ((data?.ContainsKey(MessageId)).GetValueOrDefault())
                    Dispatcher.Invoke(() => Message = data[MessageId]);
            }
            Dispatcher.Invoke(() => ActivationResult = result);

            if (result != "Ignored")
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    OpenAlertsWindow();
                });
            }
        }

        #region Property

        public string ToastResult
        {
            get { return (string)GetValue(ToastResultProperty); }
            set { SetValue(ToastResultProperty, value); }
        }
        public static readonly DependencyProperty ToastResultProperty =
            DependencyProperty.Register(
                nameof(ToastResult),
                typeof(string),
                typeof(MainWindow),
                new PropertyMetadata(string.Empty));

        public string ActivationResult
        {
            get { return (string)GetValue(ActivationResultProperty); }
            set { SetValue(ActivationResultProperty, value); }
        }
        public static readonly DependencyProperty ActivationResultProperty =
            DependencyProperty.Register(
                nameof(ActivationResult),
                typeof(string),
                typeof(MainWindow),
                new PropertyMetadata(string.Empty));

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
                nameof(Message),
                typeof(string),
                typeof(MainWindow),
                new PropertyMetadata(string.Empty));

        public bool CanUseInteractiveToast
        {
            get { return (bool)GetValue(CanUseInteractiveToastProperty); }
            set { SetValue(CanUseInteractiveToastProperty, value); }
        }
        public static readonly DependencyProperty CanUseInteractiveToastProperty =
            DependencyProperty.Register(
                nameof(CanUseInteractiveToast),
                typeof(bool),
                typeof(MainWindow),
                new PropertyMetadata(Environment.OSVersion.Version.Major >= 10));

        #endregion

        private void Clear()
        {
            ToastResult = "";
            ActivationResult = "";
            Message = "";
        }

        private async Task<string> ShowInteractiveToastAsync(int expire)
        {
            var request = new ToastRequest
            {
                ToastXml = ComposeInteractiveToast(),
                ShortcutFileName = "NWS Alerts.lnk",
                ShortcutTargetFilePath = Assembly.GetExecutingAssembly().Location,
                ShortcutWorkingFolder = AppDomain.CurrentDomain.BaseDirectory,
                AppId = "NWS Alerts",
                ActivatorId = typeof(NotificationActivator).GUID
            };

            var result = await ToastManager.ShowAsync(request, expire);

            return result.ToString();
        }

        private string ComposeInteractiveToast()
        {
            var toastVisual = new ToastVisual
            {
                BindingGeneric = new ToastBindingGeneric
                {
                    Children =
                    {
                        new AdaptiveText { Text = MessageTitle },
                        new AdaptiveText { Text = MessageBody },
                    },
                    AppLogoOverride = new ToastGenericAppLogo
                    {
                        Source = string.Format("file:///{0}", Path.GetFullPath("DesktopToast.png")),
                        AlternateText = ""
                    }
                }
            };
            var toastAction = new ToastActionsCustom
            {
                Buttons =
                {
                    new ToastButton(content: "More Info", arguments: "action=Replied") { ActivationType = ToastActivationType.Background },
                    new ToastButton(content: "Ignore", arguments: "action=Ignored")
                }
            };

            var toastContent = new ToastContent
            {
                Visual = toastVisual,
                Actions = toastAction,
                Duration = ToastDuration.Long,
                Scenario = ToastScenario.Reminder
            };

            return toastContent.GetContent();
        }

        public async void AlertToast(int expire)
        {
            if (((MainWindow)Application.Current.MainWindow).mainWindow.WindowState != WindowState.Normal)
            {
                Clear();

                ToastResult = await ShowInteractiveToastAsync(expire);
            }
        }

        #endregion Windows 10 Toast

        #region Event Subscriptions

        public static void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && ((MainWindow)Application.Current.MainWindow).WeatherApp != null)
            {
                ((MainWindow)Application.Current.MainWindow).WeatherApp.Activate();
            }
        }

        public static void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                OpenAlertsWindow();
            }
        }

        private void TitleBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left && sender.GetType() != typeof(Label))
            {
                DragMove();
            }
            e.Handled = true;
        }

        private void AlertTextButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ParseWeatherXML.WeatherLink))
            {
                Process.Start(ParseWeatherXML.WeatherLink);
            }
        }

        private void NotifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            OpenAlertsWindow();
        }

        public async static void OpenAlertsWindow()
        {
            await ((MainWindow)Application.Current.MainWindow).Dispatcher.InvokeAsync(() =>
            {
                ((MainWindow)Application.Current.MainWindow).mainWindow.Topmost = true;
                ((MainWindow)Application.Current.MainWindow).mainWindow.WindowState = WindowState.Normal;
                ((MainWindow)Application.Current.MainWindow).mainWindow.Show();

                ((MainWindow)Application.Current.MainWindow).mainWindow.WindowState = WindowState.Minimized;

                ((MainWindow)Application.Current.MainWindow).mainWindow.BringIntoView();

                ((MainWindow)Application.Current.MainWindow).mainWindow.Topmost = false;

                ((MainWindow)Application.Current.MainWindow).mainWindow.Opacity = 1;

                ((MainWindow)Application.Current.MainWindow).mainWindow.WindowState = WindowState.Normal;
            });
        }

        private void HideThisWindow()
        {
            ((MainWindow)Application.Current.MainWindow).mainWindow.Opacity = 0;

            ((MainWindow)Application.Current.MainWindow).mainWindow.WindowState = WindowState.Minimized;
            ((MainWindow)Application.Current.MainWindow).mainWindow.Hide();
        }

        public static void MuteMenuItem_Click(object sender, EventArgs e)
        {
            if (!NotifyTray.MuteMenuItem.Checked)
            {
                NotifyTray.MuteMenuItem.Checked = true;
                ParseWeatherXML.MuteAlerts = true;

                Default.MuteToast = true;
                Default.Save();
            }
            else
            {
                NotifyTray.MuteMenuItem.Checked = false;
                ParseWeatherXML.MuteAlerts = false;

                Default.MuteToast = false;
                Default.Save();
            }
        }

        public static void AlertsWindowMenuItem_Click(object sender, EventArgs e)
        {
            OpenAlertsWindow();
        }

        public static void SettingsMenuItem_Click(object sender, EventArgs e)
        {
            SettingsWindow settings = new SettingsWindow();
            settings.ShowDialog();

            if (SettingsChanged)
            {
                Default.Reload();
                EasyLogger.Info("Settings have changed; Reparsing Weather...");

                ParseWeatherXML.LastUpdate = DateTimeOffset.Now.AddMonths(-1);
                ParseWeatherXML.NWSAlertsInfo();
            }
        }

        public static void AboutMenuItem_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.Show();
        }

        public static void LocationWindowMenuItem_Click(object sender, EventArgs e)
        {
            LocationBox locationBox = new LocationBox();
            locationBox.ShowDialog();

            if (SettingsChanged)
            {
                Default.Reload();
                EasyLogger.Info("Location settings have changed; Reparsing Weather...");

                ParseWeatherXML.LastUpdate = DateTimeOffset.Now.AddMonths(-1);
                ParseWeatherXML.NWSAlertsInfo();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            HideThisWindow();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;

            HideThisWindow();
        }

        public static void CloseMenuItem_Click(object sender, EventArgs e)
        {
            NotifyTray.notifyIcon.Visible = false;

            Application.Current.Shutdown();
        }

        #endregion Event Subscriptions
    }
}
