# NWS Alerts  

National Weather Service Alerts For Windows Desktop

A Widget like WPF Desktop Application with Windows 10 Toast Notifications alerting of dangerous weather.  

Works on Windows 10/11  

## Download

[https://github.com/xCONFLiCTiONx/NWS_Alerts/releases](https://github.com/xCONFLiCTiONx/NWS_Alerts/releases)  

![GitHub all releases](https://img.shields.io/github/downloads/xCONFLiCTiONx/NWS_Alerts/total)  [![GitHub stars](https://img.shields.io/github/stars/xCONFLiCTiONx/XtendedMenu)](https://github.com/xCONFLiCTiONx/NWS_Alerts/stargazers)

### More info:

Alerts are fetched every 10 minutes from [https://alerts.weather.gov](https://alerts.weather.gov).
The Desktop Weather App parses weather updates once every 30 minutes.
  
NWS Alerts Iterates through counties affected and matches to your setting of which counties to show alerts for. The most recent alert containing one of the counties you list will be the info used. If you have no counties, you will receive the latest update for all counties in the state you select.

![Screenshot1](https://github.com/xCONFLiCTiONx/NWS_Alerts/raw/main/Screenshot.jpg)  

### Instructions

The Microsoft BingWeather App must be installed for this application to work.  
To get your coordinates for the Desktop Weather App:  

* [Google Maps](https://support.google.com/maps/answer/18539?co=GENIE.Platform%3DDesktop&hl=en)

## Known Issues:
The widget is slow to load. Just be patient. I have to right click on the system tray icon to get it to respond. This could be fixed but I don't have the time. If anyone wnats to fix it and send a pull request, go for it!

## Disclaimer

Do not rely on this app to alert you in time of a real threat such as Tornados. Always take proper precautions of bad weather.  

### Prerequisites

Windows 10
.NET Framework 4.7.2

## Built With

* [EasyLogger](https://github.com/xCONFLiCTiONx/Logger) | [Nuget](https://www.nuget.org/packages/xCONFLiCTiONx.Logger/) - The Logger used
* [NotificationsExtensions.Win10](https://www.nuget.org/packages/NotificationsExtensions.Win10/14332.0.2/) - Windows 10 Toast Notifications
* [DesktopToast](https://github.com/emoacht/DesktopToast) - Windows 10 Toast Notifications

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

NOAA National Environmental Satellite, Data, and Information Service (NESDIS)  
Referral:  
NOAA Environmental Visualization Laboratory 
