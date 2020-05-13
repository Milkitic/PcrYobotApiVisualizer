using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
