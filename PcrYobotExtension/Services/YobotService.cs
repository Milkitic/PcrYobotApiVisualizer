using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PcrYobotExtension.Services
{
    public class YobotService
    {
        public long GroupId { get; }
        public string Domain { get; } = "http://101.132.134.254:9222";
        public Dictionary<string, string> Cookie { get; set; }

        public YobotService(long groupId, string domain)
        {
            GroupId = groupId;
            Domain = domain;
        }

        // /yobot/login/ POST qqid=&pwd= [BROWSER]
        // /yobot/logout/ GET [BROWSER]
        // /yobot/user/ GET (QQ ID & GROUP ID)
        // /yobot/clan/{groupId}/api/ POST {action: "get_data", csrf_token: "oFrl0Gi0FiA6t0Xa"} (Group name)

        public async Task<string> GetApiInfo()
        {
            var request = (HttpWebRequest)WebRequest.Create($"{Domain}/yobot/clan/{GroupId}/statistics/api/");
            request.Method = "GET";
            SetCookie(request);

            string responseFromServer;
            using (var response = await request.GetResponseAsync())
            using (var dataStream = response.GetResponseStream())
            using (var reader = new StreamReader(dataStream))
            {
                responseFromServer = reader.ReadToEnd();
            }
            return responseFromServer;
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
    }
}
