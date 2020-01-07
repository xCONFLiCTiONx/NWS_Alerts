using DesktopToast;
using System;
using System.Runtime.InteropServices;

namespace NWS_Alerts
{
    /// <summary>
	/// Inherited class of notification activator (for Action Center of Windows 10)
	/// </summary>
	/// <remarks>The CLSID of this class must be unique for each application.</remarks>
	[Guid("407B0197-B17F-4C64-8C90-95FC374BF5E0"), ComVisible(true), ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(INotificationActivationCallback))]
    public class NotificationActivator : NotificationActivatorBase
    { }
}
