using Newtonsoft.Json;

namespace Projects_Manager.Models
{
    public partial class Repo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("private")]
        public bool Private { get; set; }

        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("has_projects")]
        public bool HasProjects { get; set; }

        [JsonProperty("open_issues_count")]
        public long OpenIssuesCount { get; set; }
    }
}
