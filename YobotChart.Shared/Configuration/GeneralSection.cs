using System.ComponentModel;
using YamlDotNet.Serialization;

namespace YobotChart.Shared.Configuration
{
    public class GeneralSection
    {
        [YamlMember(Alias = "origin")]
        [Description("服务器的源地址，包含域名/IP、端口、协议。")]
        public string Origin { get; set; }
        [YamlMember(Alias = "ignoredVer")]
        [Description("忽略的版本更新。")]
        public string IgnoredVer { get; set; }
    }
}