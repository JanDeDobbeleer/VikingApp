using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VikingApi.Json
{
    public class Usage
    {
        public bool is_data { get; set; }
        public string start_timestamp { get; set; }
        public string balance { get; set; }
        public int duration_call { get; set; }
        public string to { get; set; }
        public bool is_sms { get; set; }
        public int timestamp { get; set; }
        public string price { get; set; }
        public int duration_connection { get; set; }
        public string duration_human { get; set; }
        public string price_plan { get; set; }
        public bool is_incoming { get; set; }
        public bool is_voice { get; set; }
        public bool is_mms { get; set; }
        public string end_timestamp { get; set; }
    }
}
