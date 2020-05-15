using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Win32;

namespace PcrYobotExtension
{
    static class WebBrowserExtension
    {
        private static string _appName = Process.GetCurrentProcess().ProcessName + ".exe";

        public static void SetSilent(this WebBrowser browser, bool silent)
        {
            if (browser == null)
                throw new ArgumentNullException(nameof(browser));

            // get an IWebBrowser2 from the document
            if (!(browser.Document is IOleServiceProvider sp)) return;
            Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
            Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

            sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out var webBrowser);
            if (webBrowser != null)
            {
                webBrowser.GetType().InvokeMember("Silent",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty,
                    null,
                    webBrowser,
                    new object[] { silent }
                );
            }
        }

        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid,
                [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }

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

        public static Dictionary<string, string> GetCookie(this WebBrowser browser)
        {
            var cookieData = FullWebBrowserCookieExtension.GetCookieInternal(browser.Source, false);
            return cookieData.Split(';')
                .Select(k => k.Split('='))
                .ToDictionary(k => k[0]?.Trim(), k => k.Length > 1 ? k[1]?.Trim() : null);
        }
    }
}