using Newtonsoft.Json.Linq;
using PcrYobotExtension.Configuration;
using PcrYobotExtension.Utils;
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
using YobotExtension.Shared;

namespace PcrYobotExtension.Services
{
    public class YobotService : IYobotServiceV1<>
    {
        public string Version => "yobot[v3.6.3-beta.4]";

        private WebBrowser _hiddenWebBrowser;
        private ManualResetEventSlim mre = new ManualResetEventSlim(false);

        public event Func<Task<bool>> InitRequested;
        public event Func<Task<bool>> OriginChangeRequested;

        public string QqId { get; private set; }
        public string GroupId { get; private set; }
        public string Origin { get; set; }
        public string Host { get; set; }
        public Dictionary<string, string> Cookie { get; set; }

        public YobotService(WebBrowser wb)
        {
            _hiddenWebBrowser = wb;
            _hiddenWebBrowser.Navigated += HiddenWebBrowser_Navigated;
            _hiddenWebBrowser.LoadCompleted += _hiddenWebBrowser_LoadCompleted;

            Origin = AppSettings.Default.General.Origin;
        }

        public UriType InitUriType { get; } = UriType.Login;

        public bool IsLogin { get; private set; }

        public async Task<bool> LoginAsync(string loginUrl)
        {
            var uri = new Uri(loginUrl);
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
            mre.Reset();
            _hiddenWebBrowser.Navigate(loginUrl);
            var waitResult = await Task.Run(() => mre.Wait(3000));
            if (!waitResult)
            {
                throw new Exception("登陆失败");
                return false;
            }

            return true;
        }

        private async Task ValidateVersion()
        {
            var o = new HttpClient();
            var result = await o.GetAsync(Origin + "/yobot/about/");
            var str = await result.Content.ReadAsStringAsync();
            var ver1Index = str.IndexOf("版本：", StringComparison.Ordinal);
            var verIndex2 = str.IndexOf("</p>", ver1Index, StringComparison.Ordinal);
            var subStr = str.Substring(ver1Index, verIndex2 - ver1Index).Split('\n')[1].Trim();
            if (!subStr.StartsWith(this.Version))
            {
                Console.WriteLine("yobot版本：" + subStr + "，当前版本支持：" + this.Version + "。可能存在兼容问题");
            }
        }

        //public async Task InitAsync(string qqId, string password)
        //{
        //    var uri = new Uri(loginUrl);
        //    Host = uri.Host;
        //    Origin = uri.Scheme + "://" + uri.Authority;

        //    mre.Reset();
        //    _hiddenWebBrowser.Navigate(loginUrl);
        //    await Task.Run(() => mre.Wait(5000));
        //}

        // /yobot/login/ POST qqid=&pwd= [BROWSER]
        // /yobot/logout/ GET [BROWSER]
        // /yobot/user/ GET (QQ ID & GROUP ID)
        // /yobot/clan/{groupId}/api/ POST {action: "get_data", csrf_token: "oFrl0Gi0FiA6t0Xa"} (Group name)

        public async Task<string> GetApiInfo()
        {
            return await InnerGetApiInfo(false);
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
                    if (sw.ElapsedMilliseconds > 3000)
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
            await CheckInit();

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

        private async Task CheckInit()
        {
            if (!IsLogin)
            {
                try
                {
                    await LoginAsync($"{Origin}/yobot/login/");
                }
                catch (Exception ex)
                {
                    if (InitRequested != null)
                    {
                        var result = await InitRequested.Invoke();
                        if (result == false)
                            throw new Exception("获取API信息失败：" + ex.Message);
                    }
                    else
                    {
                        throw new Exception("获取API信息失败：" + ex.Message);
                    }
                }
            }
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

        private void _hiddenWebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            var url = _hiddenWebBrowser.Source.ToString();
            if (url.EndsWith("/yobot/user/"))
            {
                if (!IsLogin)
                {
                    dynamic doc = _hiddenWebBrowser.Document;
                    var text = doc.DocumentElement.OuterHtml;
                    var o = new HtmlAgilityPack.HtmlDocument();
                    o.LoadHtml(text);
                    var docNode = o.DocumentNode;
                    var elRows = docNode.Descendants("el-row").ToList();

                    if (elRows.Count > 1)
                    {
                        var elRowChildNodes = elRows[0].ChildNodes.Where(k => k.Name == "a").ToArray();
                        if (elRowChildNodes.Length > 1)
                        {
                            var firstA = elRowChildNodes[0];
                            var href = firstA.GetAttributeValue("href", null);
                            if (href != null)
                            {
                                QqId = href.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                            }

                            elRowChildNodes = elRows[1].ChildNodes.Where(k => k.Name == "a").ToArray();

                            if (elRowChildNodes.Length > 1)
                            {
                                var firstB = elRowChildNodes.Last();
                                href = firstB.GetAttributeValue("href", null);
                                if (href != null)
                                {
                                    GroupId = href.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                                }

                                _hiddenWebBrowser.Navigate($"{Origin}/yobot/clan/{GroupId}");
                            }
                        }

                    }
                }
            }
            else if (url.Contains("/yobot/clan/"))
            {
                var cookie = _hiddenWebBrowser.GetCookie();
                Cookie = cookie;
                IsLogin = true;
                mre.Set();

                _hiddenWebBrowser.Navigate($"{Origin}/yobot/user/");
            }
        }
    }
}
