using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Projects_Manager.Models
{
    class RepoInfo
    {
        public Repo Repo { get; set; }

        [YamlMember(ScalarStyle = ScalarStyle.Literal)]
        public string Notes { get; set; }

        public bool IsHidden { get; set; }

        public RepoInfo(Repo repo, bool isHidden = false)
        {
            Repo = repo;
            Notes = "";
            IsHidden = isHidden;
        }

        public RepoInfo()
        {
            Repo = null;
            Notes = "";
            IsHidden = false;
        }
    }
}
