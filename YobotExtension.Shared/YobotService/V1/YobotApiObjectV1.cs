using Newtonsoft.Json;

namespace YobotExtension.Shared.YobotService.V1
{
    public class YobotApiObjectV1
    {
        [JsonProperty("api_version")]
        public string ApiVersion { get; set; }

        [JsonProperty("challenges")]
        public ChallengeObject[] Challenges { get; set; }

        [JsonProperty("code")]
        public long Code { get; set; }

        [JsonProperty("groupinfo")]
        public GroupInfoObject[] Groupinfo { get; set; }

        [JsonProperty("members")]
        public MemberObject[] Members { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
