using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace YobotExtension.Services
{
    public static class QQService
    {
        private static ConcurrentDictionary<long, string> CacheNickName { get; set; } =
            new ConcurrentDictionary<long, string>();

        public static async Task<string> GetQqNickNameAsync(long qqId)
        {
            if (CacheNickName.ContainsKey(qqId)) return CacheNickName[qqId];
            var httpClient = new HttpClient();
            var resultByte =
                await httpClient.GetByteArrayAsync($"https://r.qzone.qq.com/fcg-bin/cgi_get_portrait.fcg?uins={qqId}");
            var resultStr = Encoding.GetEncoding("gb2312").GetString(resultByte, 0, resultByte.Length - 1);

            const string flag1 = ",\"";
            const string flag2 = "\",";
            var flagI1 = resultStr.IndexOf(flag1, StringComparison.Ordinal);
            var flagI2 = resultStr.IndexOf(flag2, flagI1, StringComparison.Ordinal);
            string nick = resultStr.Substring(flagI1 + flag1.Length, flagI2 - flagI1 - flag1.Length);

            CacheNickName.TryAdd(qqId, nick);
            return nick;
        }
    }
}
