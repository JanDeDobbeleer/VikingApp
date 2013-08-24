using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VikingApi.ApiTools;

namespace VikingApi.Classes
{
    public class Bundle
    {
        public string valid_until { get; set; }
        public string valid_from { get; set; }
        public string value { get; set; }
        public string assigned { get; set; }
        public string used { get; set; }
        public string type { get; set; }
    }

    public class Balance
    {
        public string valid_until { get; set; }
        public int voice_super_on_net { get; set; }
        public int sms_super_on_net_max { get; set; }
        public int sms { get; set; }
        public int data { get; set; }
        public string credits { get; set; }
        public List<Bundle> bundles { get; set; }
        public int sms_super_on_net { get; set; }
        public int voice_super_on_net_max { get; set; }
        public bool is_expired { get; set; }

        public static void ConvertBalance(string json)
        {
            ApiTools.ApiTools.SaveSetting(new KeyValuePair() { name = "balance", content = JsonConvert.DeserializeObject<Balance>(json) });
        }
    }
}
