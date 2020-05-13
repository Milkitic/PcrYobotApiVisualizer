using Newtonsoft.Json;

namespace PcrYobotExtension.Models
{
    public class YobotApiModel
    {
        [JsonProperty("challenges")]
        public ChallengeModel[] Challenges { get; set; }

        [JsonProperty("code")]
        public int Code { get; set; }
    }
}