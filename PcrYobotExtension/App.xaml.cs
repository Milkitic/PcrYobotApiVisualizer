﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using PcrYobotExtension.Configuration;
using PcrYobotExtension.Utils;

namespace PcrYobotExtension
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            CheckBrowserVersion();

            if (!LoadConfig())
            {
                Environment.Exit(0);
                return;
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            MessageBox.Show(e.Exception?.Message, "Msgbox", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var stop = e.IsTerminating ? "，即将终止" : "";

            if (e.ExceptionObject is Exception ex)
            {
                Logger.Error(ex, $"程序出现未处理的异常{stop}");
            }
            else
            {
                Logger.Error("程序出现未处理的异常" + stop + "：{0}", e.ExceptionObject);
            }
        }

        private static void CheckBrowserVersion()
        {
            if (WebBrowserExtension.CheckWebBrowserVersion() == false)
            {
                Thread.Sleep(2000);
                Process.Start(Process.GetCurrentProcess().MainModule.FileName);
                Environment.Exit(0);
            }
        }

        private static bool LoadConfig()
        {
            var file = AppSettings.Files.ConfigFile;
            if (!File.Exists(file))
            {
                AppSettings.CreateNewConfig();
            }
            else
            {
                try
                {
                    var content = File.ReadAllText(file);
                    AppSettings.Load(JsonConvert.DeserializeObject<AppSettings>(content,
                            new JsonSerializerSettings
                            {
                                TypeNameHandling = TypeNameHandling.Auto
                            }
                        )
                    );
                }
                catch (JsonException ex)
                {
                    var result = MessageBox.Show("载入配置文件时失败，用默认配置覆盖继续打开吗？" + Environment.NewLine + ex.Message,
                        "YobotExtension", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        AppSettings.CreateNewConfig();
                    }
                    else
                        return false;
                }
            }

            return true;
        }
    }
}
