For reference to setup UWP references:
https://docs.microsoft.com/en-us/windows/apps/desktop/modernize/desktop-to-uwp-enhance

C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.18362.0\Facade\windows.winmd
C:\Program Files (x86)\Windows Kits\10\References\10.0.18362.0\Windows.Foundation.UniversalApiContract\8.0.0.0\Windows.Foundation.UniversalApiContract.winmd
C:\Program Files (x86)\Windows Kits\10\References\10.0.18362.0\Windows.Foundation.FoundationContract\3.0.0.0\Windows.Foundation.FoundationContract.winmd

Make sure to set copy local to false for all UWP references


DesktopToast\ToastManager.cs @ private static async Task<ToastResult> ShowBaseAsync(XmlDocument document, string appId)
// Create a timeout to prevent history piling up
toast.ExpirationTime = DateTimeOffset.Now.AddSeconds(30);