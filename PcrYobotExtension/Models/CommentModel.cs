using Newtonsoft.Json;

namespace PcrYobotExtension.Models
{
    public class CommentModel
    {
        [JsonProperty("behalf", NullValueHandling = NullValueHandling.Ignore)]
        public string Behalf { get; set; }
    }
}