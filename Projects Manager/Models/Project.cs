using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projects_Manager.Models
{
    class Project
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }
    }
}
