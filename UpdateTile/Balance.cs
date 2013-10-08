using System.Collections.Generic;

namespace UpdateTile
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
        //JSON members
        public string valid_until { get; set; }
        public int voice_super_on_net { get; set; }
        public int sms_super_on_net_max { get; set; }
        public int sms { get; set; }
        public string data { get; set; }
        public string credits { get; set; }
        public List<Bundle> bundles { get; set; }
        public int sms_super_on_net { get; set; }
        public int voice_super_on_net_max { get; set; }
        public bool is_expired { get; set; }
    }
}