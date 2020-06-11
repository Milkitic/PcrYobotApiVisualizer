using YamlDotNet.Serialization;

namespace YobotExtension.AutoUpdate
{
    public class GiteeBody
    {
        [YamlMember(Alias = "notes")]
        public string[] Notes { get; set; }
        [YamlMember(Alias = "releasePage")]
        public string ReleasePage { get; set; }
        [YamlMember(Alias = "releaseFile")]
        public string ReleaseFile { get; set; }
    }
}
