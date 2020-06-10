using Newtonsoft.Json;

namespace YobotExtension.Shared.YobotService.V1
{
    public class GroupInfoObject
    {
        [JsonProperty("battle_id")]
        public long BattleId { get; set; }

        [JsonProperty("game_server")]
        public string GameServer { get; set; }

        [JsonProperty("group_id")]
        public long GroupId { get; set; }

        [JsonProperty("group_name")]
        public string GroupName { get; set; }
    }
}