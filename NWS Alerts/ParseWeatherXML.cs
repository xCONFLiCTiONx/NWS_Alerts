using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using static NWS_Alerts.Properties.Settings;

namespace NWS_Alerts
{
    public class ParseWeatherXML
    {
        #region References

        public static DateTimeOffset LastUpdate { get; set; }
        public static bool MuteAlerts = false;
        public static string WeatherLink { get; set; }
        private static WebClient webClient = null;

        #endregion References

        #region NWS Alerts

        public static void NWSAlertsInfo()
        {
            try
            {
                EasyLogger.Info("Parsing NWS Alerts...");

                StringBuilder LocationsListString = new StringBuilder();

                try
                {
                    webClient = new WebClient();

                    webClient.Headers.Add("User-Agent: None");

                    using (XmlReader reader = XmlReader.Create(webClient.OpenRead(Default.AlertFeedUrl)))
                    {
                        SyndicationFeed AtomFeed = SyndicationFeed.Load(reader);
                        reader.Close();

                        bool CountyInfoFound = false;
                        bool ShowToast = false;

                        EasyLogger.Info("Parsing info for counties... ");

                        // Run through each alert
                        foreach (SyndicationItem item in AtomFeed.Items)
                        {
                            // Get Location of the alert
                            foreach (SyndicationElementExtension extension in item.ElementExtensions)
                            {
                                XElement element = extension.GetObject<XElement>();
                                string name = element.Name.ToString();

                                if (name.Contains("areaDesc"))
                                {
                                    LocationsListString.Append(element.Value.ToString().ToLower());
                                }
                            }

                            // Add locations to an array for comparison
                            List<string> LocationList = new List<string>();
                            LocationList.AddRange(LocationsListString.ToString().Split(';').Select(i => i));

                            // Counties
                            if (Default.ShowCounties.Count > 0)
                            {
                                foreach (var county in Default.ShowCounties)
                                {
                                    foreach (string location in LocationList)
                                    {
                                        if (county.ToLower() == location.Trim() && item.LastUpdatedTime > LastUpdate)
                                        {
                                            ShowToast = true;

                                            LastUpdate = item.LastUpdatedTime;

                                            EasyLogger.Info("Last Update Time: " + LastUpdate);
                                            EasyLogger.Info("Found information for: " + county);
                                        }
                                        if (county.ToLower() == location.Trim())
                                        {
                                            CountyInfoFound = true;

                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                CountyInfoFound = true;
                                ShowToast = true;

                                LastUpdate = item.LastUpdatedTime;

                                EasyLogger.Info("Last Update Time: " + LastUpdate);
                            }

                            bool AlertIgnored = false;
                            if (Default.IgnoreAlerts.Count > 0)
                            {
                                foreach (var alert in Default.IgnoreAlerts)
                                {
                                    if (item.Title.Text.Contains(alert))
                                    {
                                        AlertIgnored = true;
                                    }
                                }
                            }

                            if (CountyInfoFound && !AlertIgnored)
                            {
                                if (ShowToast && !MuteAlerts)
                                {
                                    EasyLogger.Info("All conditions have been met and an alert is present. Showing a Toast Notification Alert... ");

                                    ((MainWindow)Application.Current.MainWindow).MessageBody = item.Title.Text;

                                    ((MainWindow)Application.Current.MainWindow).AlertToast(300);
                                }

                                ((MainWindow)Application.Current.MainWindow).AlertTextTitle.Text = item.Title.Text + Environment.NewLine;

                                ((MainWindow)Application.Current.MainWindow).AlertTextBody.Text = item.Summary.Text;

                                WeatherLink = item.Id;

                                if (((MainWindow)Application.Current.MainWindow).mainWindow.WindowState == WindowState.Normal)
                                {
                                    MainWindow.OpenAlertsWindow();
                                }

                                EasyLogger.Info("Alert Feed: " + item.Summary.Text);

                                break;
                            }
                        }

                        if (!CountyInfoFound)
                        {
                            EasyLogger.Info("No updates found for any counties in the list...");

                            if (((MainWindow)Application.Current.MainWindow).AlertTextBody.Text == string.Empty)
                            {
                                ((MainWindow)Application.Current.MainWindow).AlertTextBody.Text = "No alerts have been found for any of the counties you are watching.";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    EasyLogger.Error(ex);
                }

                ((MainWindow)Application.Current.MainWindow).NextUpdateLabel.Content = "-   Next Update: " + DateTime.Now.AddMinutes(15).AddSeconds(-DateTime.Now.Second);
            }
            catch (Exception ex)
            {
                EasyLogger.Error(ex);
            }
        }

        #endregion NWS Alerts
    }
}
