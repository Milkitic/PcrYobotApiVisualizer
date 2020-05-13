using Newtonsoft.Json;

namespace PcrYobotExtension.Models
{
    public class ChallengeModel
    {
        [JsonProperty("boss_num")]
        public int BossNum { get; set; }

        [JsonProperty("challenge_time")]
        public long ChallengeTime { get; set; }

        [JsonProperty("comment")]
        public CommentModel Comment { get; set; }

        [JsonProperty("cycle")]
        public int Cycle { get; set; }

        [JsonProperty("damage")]
        public int Damage { get; set; }

        [JsonProperty("health_ramain")]
        public int HealthRemain { get; set; }

        [JsonProperty("is_continue")]
        public bool IsContinue { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("qqid")]
        public long QqId { get; set; }
    }
}