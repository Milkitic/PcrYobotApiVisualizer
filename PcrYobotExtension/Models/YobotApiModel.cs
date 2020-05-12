using Newtonsoft.Json;

namespace PcrYobotExtension.Models
{
    public class YobotApiModel
    {
        [JsonProperty("challenges")]
        public ChallengeModel[] Challenges { get; set; }

        [JsonProperty("code")]
        public long Code { get; set; }
    }
}