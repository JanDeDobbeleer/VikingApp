using System;
using System.Linq;
using Newtonsoft.Json;
using VikingApi.Classes;

namespace VikingApi.AppClasses
{
    public class UserBalance
    {
        private Balance _balance;

        public UserBalance(string json)
        {
            Load(json);
            ConvertValues();
        }

        public string Credit { get; set; }
        public string Remaining { get; set; }
        public string Sms { get; set; }
        public string Data { get; set; }
        public string VikingSms { get; set; }
        public string VikingMinutes { get; set; }

        //percentages
        public double RemainingPercentage { get; set; }
        public double SmsPercentage { get; set; }
        public double DataPercentage { get; set; }
        public double VikingSmsPercentage { get; set; }
        public double VikingMinutesPercentage { get; set; }

        private void Load(string json)
        {
            _balance = JsonConvert.DeserializeObject<Balance>(json);
        }

        public void ConvertValues()
        {
            Credit = string.Format("€{0}", _balance.credits);
            Remaining = ConvertDate(_balance.valid_until);
            Data = string.Format("{0} / {1}", Math.Round(double.Parse(_balance.bundles.First(bundle => bundle.type == "data").value.Split('.')[0])/1024d/1024d, 0), Math.Round(double.Parse(_balance.bundles.First(bundle => bundle.type == "data").assigned.Split('.')[0])/1024d/1024d, 0));
            Sms = string.Format("{0} / {1}", _balance.bundles.First(bundle => bundle.type == "sms").value.Split('.')[0], _balance.bundles.First(bundle => bundle.type == "sms").assigned.Split('.')[0]);
            VikingSms = string.Format("{0} / {1}", _balance.bundles.First(bundle => bundle.type == "sms_super_on_net").value.Split('.')[0], _balance.bundles.First(bundle => bundle.type == "sms_super_on_net").assigned.Split('.')[0]);
            int minutes = int.Parse(_balance.bundles.First(bundle => bundle.type == "voice_super_on_net").value.Split('.')[0])/60;
            int seconds = int.Parse(_balance.bundles.First(bundle => bundle.type == "voice_super_on_net").assigned.Split('.')[0])%60;
            VikingMinutes = string.Format("{0}m {1}s", minutes, seconds);
            CalculatePercentages();
        }

        private string ConvertDate(string validUntil)
        {
            DateTime expires = Convert.ToDateTime(validUntil);
            TimeSpan difference = (expires - DateTime.Now);
            if (difference.Days > 0)
            {
                return string.Format("{0} days", difference.Days);
            }
            if (difference.Hours > 0)
            {
                return string.Format("{0} hours", difference.Hours);
            }
            if (difference.Minutes > 0)
            {
                return string.Format("{0} minutes", difference.Minutes);
            }
            return difference.Seconds > 0 ? string.Format("{0} seconds", difference.Seconds) : "bundle expired";
        }

        private void CalculatePercentages()
        {
            foreach (Bundle bundle in _balance.bundles)
            {
                switch (bundle.type)
                {
                    case ("sms_super_on_net"):
                        VikingSmsPercentage = 100 - Math.Round((double.Parse(bundle.used.Split('.')[0])/double.Parse(bundle.assigned.Split('.')[0]))*100d, 0);
                        break;
                    case ("sms"):
                        SmsPercentage = 100 - Math.Round((double.Parse(bundle.used.Split('.')[0])/double.Parse(bundle.assigned.Split('.')[0]))*100d, 0);
                        break;
                    case ("data"):
                        DataPercentage = 100 - Math.Round((double.Parse(bundle.used.Split('.')[0])/double.Parse(bundle.assigned.Split('.')[0]))*100d, 0);
                        break;
                    case ("voice_super_on_net"):
                        VikingMinutesPercentage = 100 - Math.Round((double.Parse(bundle.used.Split('.')[0])/double.Parse(bundle.assigned.Split('.')[0]))*100d, 0);
                        break;
                }
            }
            int totaldays = (Convert.ToDateTime(_balance.valid_until) - Convert.ToDateTime(_balance.valid_until).AddMonths(-1)).Days;
            RemainingPercentage = 100 - ((totaldays - Math.Round((Convert.ToDateTime(_balance.valid_until) - DateTime.Now).TotalDays, 0))/totaldays)*100d;
        }
    }
}