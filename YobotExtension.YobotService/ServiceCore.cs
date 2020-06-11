using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using YobotExtension.Shared.Configuration;
using YobotExtension.Shared.Win32;
using YobotExtension.Shared.YobotService;
using YobotExtension.Shared.YobotService.V1;

namespace YobotExtension.YobotService
{
    public class ServiceCore : IYobotServiceV1
    {
        public string Version { get; } = "3.6.3.4";
        public string ApiVersion => "1";

        private WebBrowser _hiddenWebBrowser;
        private ManualResetEventSlim _mre = new ManualResetEventSlim(false);

        public event Func<Task<string>> InitRequested;
        public event Func<Task<bool>> OriginChangeRequested;

        public string QqId { get; private set; }
        public string GroupId { get; private set; }
        public string Origin { get; set; }
        public string Host { get; set; }
        public Dictionary<string, string> Cookie { get; set; }
        public UriType InitUriType { get; } = UriType.Login;
        public bool IsLogin { get; private set; }

        public ServiceCore(WebBrowser wb)
        {
            _hiddenWebBrowser = wb;
            _hiddenWebBrowser.Navigated += HiddenWebBrowser_Navigated;
            _hiddenWebBrowser.LoadCompleted += HiddenWebBrowser_LoadCompleted;

            Origin = AppSettings.Default.General.Origin;
        }
        private int _timeoutMilliseconds = 3000;

        public async Task<bool> LoginAsync(string loginUrl)
        {
            Uri uri;
            try
            {
                uri = new Uri(loginUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            var origin = uri.Scheme + "://" + uri.Authority;

            if (origin != Origin && !string.IsNullOrWhiteSpace(Origin) && OriginChangeRequested != null)
            {
                var result = await OriginChangeRequested?.Invoke();
                if (result)
                {
                    AppSettings.Default.General.Origin = origin;
                    AppSettings.SaveDefault();
                }
            }
            else
            {
                AppSettings.Default.General.Origin = origin;
                AppSettings.SaveDefault();
            }

            Host = uri.Host;
            Origin = origin;

            await ValidateVersion();
            _mre.Reset();
            _hiddenWebBrowser.Navigate(loginUrl);
            var waitResult = await Task.Run(() => _mre.Wait(_timeoutMilliseconds));
            return waitResult;
        }

        public async Task<string> ValidateVersion()
        {
            var o = new HttpClient();
            var result = await o.GetAsync(Origin + "/yobot/about/");
            var str = await result.Content.ReadAsStringAsync();
            var ver1Index = str.IndexOf("版本：", StringComparison.Ordinal);
            if (ver1Index == -1) return null;
            var verIndex2 = str.IndexOf("</p>", ver1Index, StringComparison.Ordinal);
            if (verIndex2 == -1) return null;
            var subStr = str.Substring(ver1Index, verIndex2 - ver1Index).Split('\n')[1].Trim();

            var braceIndex1 = subStr.IndexOf('[');
            if (braceIndex1 == -1) return null;
            var braceIndex2 = subStr.IndexOf(']', braceIndex1);
            if (braceIndex2 == -1) return null;
            var version = subStr.Substring(braceIndex1 + 1, braceIndex2 - braceIndex1 - 1);
            version = version.Replace("-beta", "").Replace("-alpha", ".0").Trim('v');
            if (version != this.Version)
            {
                Console.WriteLine("yobot版本：" + version + "，当前版本支持：" + this.Version + "。可能存在兼容问题");
            }

            return version;
        }

        // /yobot/login/ POST qqid=&pwd= [BROWSER]
        // /yobot/logout/ GET [BROWSER]
        // /yobot/user/ GET (QQ ID & GROUP ID)
        // /yobot/clan/{groupId}/api/ POST {action: "get_data", csrf_token: "oFrl0Gi0FiA6t0Xa"} (Group name)

        public async Task<YobotApiObjectV1> GetApiInfo()
        {
            var str = await InnerGetApiInfo(false);
            return JsonConvert.DeserializeObject<YobotApiObjectV1>(str);
        }

        public async Task<bool> LogoutAsync()
        {
            _hiddenWebBrowser.LoadCompleted += LoadCompleted;
            _hiddenWebBrowser.Navigate($"{Origin}/yobot/logout/");
            bool success = false;
            await Task.Run(() =>
            {
                var sw = Stopwatch.StartNew();
                while (!success)
                {
                    if (sw.ElapsedMilliseconds > _timeoutMilliseconds)
                    {
                        _hiddenWebBrowser.LoadCompleted -= LoadCompleted;
                        break;
                    }

                    Thread.Sleep(10);
                }
            });

            Cookie = null;
            IsLogin = false;
            return success;

            void LoadCompleted(object sender, NavigationEventArgs e)
            {
                var url = _hiddenWebBrowser.Source.ToString();
                if (url.Contains("/yobot/login/"))
                {
                    _hiddenWebBrowser.LoadCompleted -= LoadCompleted;
                    success = true;
                }
            }
        }

        private async Task<string> InnerGetApiInfo(bool loop)
        {
            await CheckIsLogin();

            var request = (HttpWebRequest)WebRequest.Create($"{Origin}/yobot/clan/{GroupId}/statistics/api/");
            request.Method = "GET";
            SetCookie(request);

            string responseFromServer;
            using (var response = await request.GetResponseAsync())
            using (var dataStream = response.GetResponseStream())
            using (var reader = new StreamReader(dataStream))
            {
                responseFromServer = reader.ReadToEnd();
            }

            var jObj = JObject.Parse(responseFromServer);
            if (jObj.First is JProperty jp && jp.Name == "code")
            {
                var code = jp.Value.Value<long>();
                if (code != 0)
                {
                    if (!loop) return await InnerGetApiInfo(true);
                    if (jObj.Last is JProperty jp2 && jp2.Name == "message")
                        throw new Exception($"[{code}] {jp2.Value.Value<string>()}");
                }
            }

            return responseFromServer;
        }

        private async Task CheckIsLogin()
        {
            if (IsLogin) return;

            var result = await LoginAsync($"{Origin}/yobot/login/");
            if (result) return;

            if (InitRequested == null) throw new Exception("登录失败，请检查链接是否已过期");

            var newUri = await InitRequested.Invoke();
            result = await LoginAsync(newUri);
            if (!result) throw new Exception("登录失败，请检查链接是否已过期");
        }

        private void SetCookie(HttpWebRequest request)
        {
            request.CookieContainer = new CookieContainer();
            if (Cookie == null) return;
            foreach (var kvp in Cookie)
            {
                var cookie = new Cookie(kvp.Key, kvp.Value, "/", request.Host?.Split(':')[0]);
                request.CookieContainer.Add(cookie);
            }
        }

        private void HiddenWebBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            _hiddenWebBrowser.SetSilent(true); // make it silent
        }

        private void HiddenWebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            var url = _hiddenWebBrowser.Source.ToString();
            // ↓ 爬取个人信息、公会信息
            if (url.EndsWith("/yobot/user/") && !IsLogin)
            {
                var outerHtml = _hiddenWebBrowser.GetDocumentOuterHtml();

                var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(outerHtml);

                var docNode = htmlDoc.DocumentNode;

                var elRows = docNode.Descendants("el-row").ToList();
                if (elRows.Count <= 1) return;

                var elRowChildNodes = elRows[0].ChildNodes.Where(k => k.Name == "a").ToArray();
                // 功能按钮列表
                if (elRowChildNodes.Length <= 1) return;

                var btnMyCenter = elRowChildNodes.First(); // 个人中心按钮
                var href = btnMyCenter.GetAttributeValue("href", null);
                if (href != null)
                {
                    QqId = href.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                    if (!long.TryParse(QqId, out _))
                    {
                        Console.WriteLine("错误：非法的QQ号，可能页面已发生改变");
                    }
                }

                elRowChildNodes = elRows[1].ChildNodes.Where(k => k.Name == "a").ToArray();
                // 公会（可能多个）按钮
                if (elRowChildNodes.Length <= 1) return;

                var btnClan = elRowChildNodes.Last(); // 选择最后一个
                href = btnClan.GetAttributeValue("href", null);
                if (href != null)
                {
                    GroupId = href.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries)
                        .LastOrDefault();
                    if (!long.TryParse(GroupId, out _))
                    {
                        Console.WriteLine("错误：非法的QQ群号，可能页面已发生改变");
                    }
                }

                _hiddenWebBrowser.Navigate($"{Origin}/yobot/clan/{GroupId}");
                // ↓ 如果跳转至公会信息，则上面爬取的内容无误
            }
            else if (url.Contains("/yobot/clan/") && !IsLogin)
            {
                var cookie = _hiddenWebBrowser.GetCookie();
                Cookie = cookie;
                IsLogin = true;
                _mre.Set();

                _hiddenWebBrowser.Navigate($"{Origin}/yobot/user/");
            }
        }
    }
}
