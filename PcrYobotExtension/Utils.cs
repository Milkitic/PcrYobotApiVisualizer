using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PcrYobotExtension
{
    class Utils
    {
        private static Dictionary<long, string> _dicQqNick = new Dictionary<long, string>();

        public static async Task<string> GetQqNickNameAsync(long qqId)
        {
            if (_dicQqNick.ContainsKey(qqId)) return _dicQqNick[qqId];
            var sb = new HttpClient();
            var result = await sb.GetByteArrayAsync("https://r.qzone.qq.com/fcg-bin/cgi_get_portrait.fcg?uins=" + qqId);
            var responseString = Encoding.GetEncoding("gb2312").GetString(result, 0, result.Length - 1);
            var str1 = ",\"";
            var str2 = "\",";
            var index1 = responseString.IndexOf(str1, StringComparison.Ordinal);
            var index2 = responseString.IndexOf(str2, index1, StringComparison.Ordinal);
            string nick = responseString.Substring(index1 + str1.Length, index2 - index1 - str1.Length);
            _dicQqNick.Add(qqId, nick);
            return nick;
        }

        public static async Task<List<Cookie>> GetCookie(string url)
        {
            url = url.Replace("/c/#", "/#");
            var cookies = new CookieContainer();
            var handler = new HttpClientHandler { CookieContainer = cookies };

            var client = new HttpClient(handler);
            var response = await client.GetAsync(url);
            var o = await response.Content.ReadAsStringAsync();
            Uri uri = new Uri(url);
            var responseCookies = cookies.GetCookies(uri).Cast<Cookie>().ToList();
            return responseCookies;
        }
    }
}
