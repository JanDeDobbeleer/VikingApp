using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VikingApi.Classes;

namespace VikingApi.AppClasses
{
    public class UserBalance
    {
        public string Credit { get; set; }
        public string Remaining { get; set; }
        public string Sms { get; set; }
        public string Data { get; set; }
        public string VikingSms { get; set; }

        //percentages
        public double RemainingPercentage { get; set; }
        public double SmsPercentage { get; set; }
        public double DataPercentage { get; set; }
        public double VikingSmsPercentage { get; set; }

        private Balance _balance;

        public UserBalance(string json)
        {
            Load(json);
            ConvertValues();
        }

        private void Load(string json)
        {
            _balance = JsonConvert.DeserializeObject<Balance>(json);
        }

        public void ConvertValues()
        {
            Credit = string.Format("€{0}", _balance.credits);
            Remaining = ConvertDate(_balance.valid_until);
            Data = string.Format("{0} / {1}", Math.Round(double.Parse(_balance.bundles.First(bundle => bundle.type == "data").value.Split('.')[0]) / 1024d / 1024d, 0).ToString(), Math.Round(double.Parse(_balance.bundles.First(bundle => bundle.type == "data").assigned.Split('.')[0]) / 1024d / 1024d, 0).ToString());
            Sms = string.Format("{0} / {1}",_balance.bundles.First(bundle => bundle.type == "sms").value.Split('.')[0], _balance.bundles.First(bundle => bundle.type == "sms").assigned.Split('.')[0]);
            VikingSms = string.Format("{0} / {1}",_balance.bundles.First(bundle => bundle.type == "sms_super_on_net").value.Split('.')[0], _balance.bundles.First(bundle => bundle.type == "sms_super_on_net").assigned.Split('.')[0]);
            CalculatePercentages();
        }

        private string ConvertDate(string validUntil)
        {
            var expires = Convert.ToDateTime(validUntil);
            var difference = (expires - DateTime.Now);
            if (difference.Days > 0)
            {
                return string.Format("{0} days", difference.Days.ToString());
            }
            if (difference.Hours > 0)
            {
                return string.Format("{0} hours", difference.Hours.ToString());
            }
            if (difference.Minutes > 0)
            {
                return string.Format("{0} minutes", difference.Minutes.ToString());
            }
            return difference.Seconds > 0 ? string.Format("{0} seconds", difference.Seconds.ToString()) : "expired";
        }

        private void CalculatePercentages()
        {
            foreach (var bundle in _balance.bundles)
            {
                switch (bundle.type)
                {
                    case ("sms_super_on_net"):
                        VikingSmsPercentage = Math.Round((double.Parse(bundle.used.Split('.')[0]) / double.Parse(bundle.assigned.Split('.')[0])) * 100d, 0);
                        break;
                    case ("sms"):
                        SmsPercentage = Math.Round((double.Parse(bundle.used.Split('.')[0]) / double.Parse(bundle.assigned.Split('.')[0])) * 100d, 0);
                        break;
                    case ("data"):
                        DataPercentage = Math.Round((double.Parse(bundle.used.Split('.')[0]) / double.Parse(bundle.assigned.Split('.')[0])) * 100d, 0);
                        break;
                }
            }
            var totaldays = (Convert.ToDateTime(_balance.valid_until) - Convert.ToDateTime(_balance.valid_until).AddMonths(-1)).Days;
            RemainingPercentage = ((totaldays - Math.Round((Convert.ToDateTime(_balance.valid_until) - DateTime.Now).TotalDays, 0)) / totaldays) * 100d;
        }
    }
}
