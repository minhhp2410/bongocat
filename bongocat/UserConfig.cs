using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace bongocat
{
    internal class UserConfig
    {
        [JsonProperty("folder")]
        public string Folder { get; set; }
        [JsonProperty("bgFomat")]
        public string BgFormat { get; set; }
    }
}
