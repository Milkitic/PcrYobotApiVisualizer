using System.Linq;
using Newtonsoft.Json;

namespace YobotChart.Shared.YobotService.V1
{
    public class YobotApiObjectV1 : IYobotApiObject
    {
        [JsonProperty("api_version")]
        public string ApiVersion { get; set; }

        [JsonProperty("challenges")]
        public ChallengeObject[] Challenges { get; set; }

        [JsonProperty("code")]
        public long Code { get; set; }

        [JsonProperty("groupinfo")]
        public GroupInfoObject[] GroupInfo { get; set; }

        [JsonProperty("members")]
        public MemberObject[] Members { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        IChallengeObject[] IYobotApiObject.Challenges
        {
            get => Challenges.Cast<IChallengeObject>().ToArray();
            set => Challenges = value.Cast<ChallengeObject>().ToArray();
        }
    }
}
