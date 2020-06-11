using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace YobotChart.AutoUpdate
{
    public class GiteeUpdater
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private const int Timeout = 10000;
        private const int RetryCount = 3;
        private static readonly HttpClient HttpClient;

        public GiteeRelease NewRelease { get; private set; }
        public bool IsRunningChecking { get; private set; }
        public string CurrentVersion { get; } = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString().TrimEnd('.', '0');

        public async Task<bool?> CheckUpdateAsync()
        {
            IsRunningChecking = true;

            try
            {
                string json = "";
                while (json == "")
                {
                    json = await HttpGetAsync("https://gitee.com/api/v5/repos/Milkitic/YobotExtensionRelease/releases");
                }

                if (json.Contains("API rate limit"))
                {
                    Logger.Error("Error while checking for updates: Gitee API rate limit exceeded.");
                    return null;
                }

                List<GiteeRelease> releases;
                try
                {
                    releases = JsonConvert.DeserializeObject<List<GiteeRelease>>(json);
                }
                catch (JsonReaderException ex)
                {
                    Logger.Error(ex, json);
                    NewRelease = null;
                    return false;
                }

                var latest = releases.OrderByDescending(k => k.CreatedAt)
                    .FirstOrDefault(k => !k.PreRelease);
                if (latest == null)
                {
                    NewRelease = null;
                    return false;
                }

                var latestVer = latest.TagName.TrimStart('v').TrimEnd('.', '0');

                latest.NewVerString = "v" + latestVer;
                latest.NowVerString = "v" + CurrentVersion;
                var g = new YamlDotNet.Serialization.Deserializer();
                var body = latest.Body;
                body = body.Replace("```yaml", "").Replace("```", "");
                var giteeBody = g.Deserialize<GiteeBody>(body);

                latest.GithubReleaseFile = giteeBody.ReleaseFile;
                latest.GithubReleasePage = giteeBody.ReleasePage;
                latest.Notes = string.Join("\r\n", giteeBody.Notes);

                var latestVerObj = new Version(latestVer);
                var nowVerObj = new Version(CurrentVersion);

                Logger.Info("Current version: {nowVer}; Got version info: {latestVer}", nowVerObj, latestVerObj);

                if (latestVerObj <= nowVerObj)
                {
                    NewRelease = null;
                    return false;
                }

                NewRelease = latest;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while checking for updates.");
                return null;
            }

            IsRunningChecking = false;
            return true;
        }

        static GiteeUpdater()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpClient =
                new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip })
                {
                    Timeout = TimeSpan.FromMilliseconds(Timeout)
                };
            HttpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36");
        }

        private static async Task<string> HttpGetAsync(string url)
        {
            for (int i = 0; i < RetryCount; i++)
            {
                try
                {
                    var message = new HttpRequestMessage(HttpMethod.Get, url);
                    var cts = new CancellationTokenSource(Timeout);
                    var response = await HttpClient.SendAsync(message, cts.Token).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, $"请求失败 {i + 1}次");
                    if (i == RetryCount - 1)
                        throw;
                }
            }

            return null;
        }
    }
}