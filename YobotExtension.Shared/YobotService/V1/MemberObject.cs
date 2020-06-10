using Newtonsoft.Json;

namespace YobotExtension.Shared.YobotService.V1
{
    public class MemberObject
    {
        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("qqid")]
        public long Qqid { get; set; }
    }
}