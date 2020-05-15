using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PcrYobotExtension.Services
{
    public static class QQService
    {
        private static readonly Dictionary<long, string> CacheNickName = new Dictionary<long, string>();

        public static async Task<string> GetQqNickNameAsync(long qqId)
        {
            if (CacheNickName.ContainsKey(qqId)) return CacheNickName[qqId];
            var sb = new HttpClient();
            var result = await sb.GetByteArrayAsync("https://r.qzone.qq.com/fcg-bin/cgi_get_portrait.fcg?uins=" + qqId);
            var responseString = Encoding.GetEncoding("gb2312").GetString(result, 0, result.Length - 1);
            var str1 = ",\"";
            var str2 = "\",";
            var index1 = responseString.IndexOf(str1, StringComparison.Ordinal);
            var index2 = responseString.IndexOf(str2, index1, StringComparison.Ordinal);
            string nick = responseString.Substring(index1 + str1.Length, index2 - index1 - str1.Length);
            CacheNickName.Add(qqId, nick);
            return nick;
        }
    }
}
