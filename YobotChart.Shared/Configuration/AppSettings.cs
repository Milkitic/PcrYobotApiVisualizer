using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace YobotChart.Shared.Configuration
{
    public class AppSettings : IDisposable
    {
        public static class Files
        {
            public static string ConfigFile { get; } = Path.Combine(Directories.CurrentDir, "appsettings.yml");
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
                var builder = new YamlDotNet.Serialization.SerializerBuilder();
                //builder.WithTagMapping("tag:yaml.org,2002:test", typeof(Test));
                var converter = builder.Build();
                var content = converter.Serialize(this);
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

        public static void LoadFromDefaultFile()
        {
            if (!File.Exists(Files.ConfigFile))
            {
                CreateNewConfig();
            }
            else
            {
                var content = File.ReadAllText(Files.ConfigFile);
                var builder = new YamlDotNet.Serialization.DeserializerBuilder();
                //builder.WithTagMapping("tag:yaml.org,2002:test", typeof(Test));
                var ymlDeserializer = builder.Build();
                Load(ymlDeserializer.Deserialize<AppSettings>(content));
            }

        }

        public static void Load(AppSettings config)
        {
            Default = config ?? new AppSettings();
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
}