using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PcrYobotExtension.Models
{
    public class ChallengeModel
    {
        [JsonProperty("boss_num")]
        public int BossNum { get; set; }

        [JsonProperty("challenge_time")]
        [JsonConverter(typeof(MyJavaScriptDateTimeConverter))]
        public DateTime ChallengeTime { get; set; }

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
        
        public ChallengeModel Clone()
        {
            return (ChallengeModel)MemberwiseClone();
        }
    }

    public class MyJavaScriptDateTimeConverter : DateTimeConverterBase
    {
        private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue($"{((DateTime)value - _epoch).TotalSeconds}");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.Value == null)
            {
                return null;
            }

            var addMilliseconds = _epoch.AddSeconds((long)reader.Value);
            return addMilliseconds;
        }
    }
}