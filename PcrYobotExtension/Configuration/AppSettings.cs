using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace PcrYobotExtension.Configuration
{
    public class AppSettings : IDisposable
    {
        public static class Files
        {
            public static string ConfigFile { get; } = Path.Combine(Directories.CurrentDir, "appsettings.json");
        }

        public class Directories
        {
            public static string CurrentDir { get; } = AppDomain.CurrentDomain.BaseDirectory;
        }

        public AppSettings()
        {
            if (Default != null)
            {
                return;
            }

            Default = this;
        }

        [JsonProperty("general")]
        public GeneralSection General { get; set; } = new GeneralSection();

        public void Save()
        {
            lock (FileSaveLock)
            {
                //FileStream.Value.SetLength(0);
                var content = JsonConvert.SerializeObject(this, Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    });
                //byte[] buffer = Encoding.GetBytes(content);
                //FileStream.Value.Write(buffer, 0, buffer.Length);
                File.WriteAllText(Files.ConfigFile, content);
            }
        }

        public void Dispose()
        {
            //foreach (var fs in FileStream.Values) fs?.Dispose();
            //FileStream?.Dispose();
        }

        private static readonly Encoding Encoding = Encoding.UTF8;
        private static readonly object FileSaveLock = new object();

        public static AppSettings Default { get; private set; }

        public static void SaveDefault()
        {
            Default?.Save();
        }

        public static void Load(AppSettings config)
        {
            Default = config ?? new AppSettings();
            //Default.FileStream = File.Open(Domain.ConfigFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        }

        private static void LoadNew()
        {
            File.WriteAllText(Files.ConfigFile, "");
            Load(new AppSettings());
        }

        public static void CreateNewConfig()
        {
            LoadNew();
            SaveDefault();
        }
    }

    public class GeneralSection
    {
        public string Origin { get; set; }
    }
}