using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PcrYobotApiVisualizer.Models
{
    public class TestViewModel
    {
    }

    public partial class Welcome
    {
        [JsonProperty("challenges")]
        public Challenge[] Challenges { get; set; }

        [JsonProperty("code")]
        public long Code { get; set; }
    }

    public partial class Challenge
    {
        [JsonProperty("boss_num")]
        public long BossNum { get; set; }

        [JsonProperty("challenge_time")]
        public long ChallengeTime { get; set; }

        [JsonProperty("comment")]
        public Comment Comment { get; set; }

        [JsonProperty("cycle")]
        public long Cycle { get; set; }

        [JsonProperty("damage")]
        public long Damage { get; set; }

        [JsonProperty("health_ramain")]
        public long HealthRamain { get; set; }

        [JsonProperty("is_continue")]
        public bool IsContinue { get; set; }

        [JsonProperty("message")]
        public object Message { get; set; }

        [JsonProperty("qqid")]
        public long Qqid { get; set; }
    }

    public partial class Comment
    {
        [JsonProperty("behalf", NullValueHandling = NullValueHandling.Ignore)]
        public string Behalf { get; set; }
    }

}
