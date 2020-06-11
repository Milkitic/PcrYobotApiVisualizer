using Microsoft.Win32;
using System.Diagnostics;

namespace YobotChart.Shared
{
    public static class WebBrowserUtils
    {
        private static string _appName = Process.GetCurrentProcess().ProcessName + ".exe";

        public static bool? CheckWebBrowserVersion()
        {
            const string browserEmulationKey =
                @"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";

            // Webpages are displayed in IE9 Standards mode, regardless of the !DOCTYPE directive.
            const int browserEmulationMode = 11001;

            using (RegistryKey registryKeyObj =
                Registry.CurrentUser.OpenSubKey(browserEmulationKey, RegistryKeyPermissionCheck.ReadWriteSubTree) ??
                Registry.CurrentUser.CreateSubKey(browserEmulationKey))
            {
                if (registryKeyObj == null) return null;

                var oldVal = registryKeyObj.GetValue(_appName);
                if (!(oldVal is int i) || i != browserEmulationMode)
                {
                    registryKeyObj.SetValue(_appName, browserEmulationMode, RegistryValueKind.DWord);
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
    }
}
