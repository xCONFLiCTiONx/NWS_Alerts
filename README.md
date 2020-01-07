# NWS Alerts  

**I'm done maintaining this applicatoin for now. There are a lot of problems with it right now. I may fix them later but I'm entirely too busy for this for now.**  

National Weather Service Alerts For Windows Desktop
  
Desktop Weather app with a secondary interface that sits in the notification area and alerts on dangerous weather.
  
![Screenshot1](https://github.com/xCONFLiCTiONx/NWS_Alerts/raw/master/Screenshot.jpg)  
  
Alerts are fetched every 10 minutes unless upgraded due to weather conditions which is Storm: upgraded to fetch every 5 minutes; Tornado: upgraded to fetch every 2 minutes.  
Desktop Weather app gets weather updates once every 30 minutes to keep load down. Anything less is unrealistic.  
  
NWS Alerts Iterates through counties affected and matches to your setting of which counties to show alerts for. The most recent alert containing one of the counties you list will be the info used. If you have no counties, you will recieve the lastest update for all counties in the state you select.
  
Donations go a long way https://www.patreon.com/xCONFLiCTiONx  
  
Right click the desktop weather app to change settings for the desktop weather app and right click the tray icon for NWS Alert options.  

**UPDATES**  
- I've removed the assets and made the program just use the assets from BingWeather so you will have to have BingWeather App for Windows 10 installed.  
    
**How To:**  
You will need to get your Lattitude and Longitude manually. Not difficult to do but an easy way is:  
  
- Go to "%USERPROFILE%\AppData\Local\Packages\"  
- Find the BingWeather AppData Folder which for me atm is: "Microsoft.BingWeather_8wekyb3d8bbwe"  
- Then continue in to "\RoamingState\Cache\cachePersonalDataCache\PersonalData_weather" and open that file (PersonalData_weather)  
- Find "DefaultWeatherLocation" and somewhere following that string will have your default coordinates that the Weather App uses itself.  
- There are easier ways if you simply just open your browser and search but this method will give you values that will "MOSTLY" match what the live tile displays in the start menu.  
  
  
Disclaimer: Do not rely on this app to alert you in time of a real threat such as Tornados. Always take proper precautions of bad weather.  
  
Credits:  
Microsoft Visual Studio and Assets used are property of Microsoft  
  
Credits for pretty much everything go to Microsoft and National Weather Service. I'm just the newbie coder learning new tricks and make new toys. :)  
NOAA National Environmental Satellite, Data, and Information Service (NESDIS)  
Referral:  
NOAA Environmental Visualization Laboratory 

For the working toast notification:
https://github.com/emoacht/DesktopToast
