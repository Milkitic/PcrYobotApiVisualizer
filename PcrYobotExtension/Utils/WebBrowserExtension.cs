using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Windows.Controls;
using Microsoft.Win32;

namespace PcrYobotExtension.Utils
{
    public static class WebBrowserExtension
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
            var cookieData = GetCookieInternal(browser.Source, false);
            return cookieData.Split(';')
                .Select(k => k.Split('='))
                .ToDictionary(k => k[0]?.Trim(), k => k.Length > 1 ? k[1]?.Trim() : null);
        }

        [SecurityCritical]
        public static string GetCookieInternal(Uri uri, bool throwIfNoCookie)
        {
            uint pchCookieData = 0;
            string url = UriToString(uri);
            uint flag = (uint)NativeMethods.InternetFlags.INTERNET_COOKIE_HTTPONLY;

            //Gets the size of the string builder   
            if (NativeMethods.InternetGetCookieEx(url, null, null, ref pchCookieData, flag, IntPtr.Zero))
            {
                pchCookieData++;
                StringBuilder cookieData = new StringBuilder((int)pchCookieData);

                //Read the cookie   
                if (NativeMethods.InternetGetCookieEx(url, null, cookieData, ref pchCookieData, flag, IntPtr.Zero))
                {
                    DemandWebPermission(uri);
                    return cookieData.ToString();
                }
            }

            int lastErrorCode = Marshal.GetLastWin32Error();

            if (throwIfNoCookie || (lastErrorCode != (int)NativeMethods.ErrorFlags.ERROR_NO_MORE_ITEMS))
            {
                throw new Win32Exception(lastErrorCode);
            }

            return null;
        }

        private static void DemandWebPermission(Uri uri)
        {
            string uriString = UriToString(uri);

            if (uri.IsFile)
            {
                string localPath = uri.LocalPath;
                new FileIOPermission(FileIOPermissionAccess.Read, localPath).Demand();
            }
            else
            {
                new WebPermission(NetworkAccess.Connect, uriString).Demand();
            }
        }

        private static string UriToString(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            UriComponents components = (uri.IsAbsoluteUri ? UriComponents.AbsoluteUri : UriComponents.SerializationInfoString);
            return new StringBuilder(uri.GetComponents(components, UriFormat.SafeUnescaped), 2083).ToString();
        }

        internal sealed class NativeMethods
        {
            #region enums

            public enum ErrorFlags
            {
                ERROR_INSUFFICIENT_BUFFER = 122,
                ERROR_INVALID_PARAMETER = 87,
                ERROR_NO_MORE_ITEMS = 259
            }

            public enum InternetFlags
            {
                INTERNET_COOKIE_HTTPONLY = 8192, //Requires IE 8 or higher   
                INTERNET_COOKIE_THIRD_PARTY = 131072,
                INTERNET_FLAG_RESTRICTED_ZONE = 16
            }

            #endregion

            #region DLL Imports

            [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("wininet.dll", EntryPoint = "InternetGetCookieExW", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
            internal static extern bool InternetGetCookieEx([In] string Url, [In] string cookieName, [Out] StringBuilder cookieData, [In, Out] ref uint pchCookieData, uint flags, IntPtr reserved);

            #endregion
        }
    }
}