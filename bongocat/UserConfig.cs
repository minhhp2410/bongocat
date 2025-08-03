using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace bongocat
{
    internal class UserConfig
    {
        [JsonProperty("folder")]
        public string Folder { get; set; }
        [JsonProperty("bgFomat")]
        public string BgFormat { get; set; }
        [JsonProperty("app")]
        public AppProperty App { get; set; }

        public UserConfig()
        {
            App = new AppProperty();
        }
    }
    internal class AppProperty
    {
        [JsonProperty("appSize")]
        public Size AppSize { get; set; } = new Size(0,0);
        [JsonProperty("appPosition")]
        public Point AppPosition { get; set; } = new Point(0,0);
        [JsonProperty("border")]
        public FormBorderStyle FormBorder { get; set; }
        [JsonProperty("topMost")]
        public bool TopMost { get; set; }
        [JsonProperty("transparentKey")]
        public Color TransparentKey { get; set; } = SystemColors.Control;
    }
}
