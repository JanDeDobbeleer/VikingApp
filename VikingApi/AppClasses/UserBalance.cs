using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Tools.Properties;
using VikingApi.Classes;

namespace VikingApi.AppClasses
{
    public class UserBalance:INotifyPropertyChanged
    {
        private Balance _balance;
        public event PropertyChangedEventHandler PropertyChanged;

        #region properties
        private string _credit;
        public string Credit
        {
            get { return _credit; }
            set
            {
                if (value == _credit)
                    return;
                _credit = value;
                OnPropertyChanged();
            }
        }
        private string _remaining ;
        public string Remaining
        {
            get { return _remaining; }
            set
            {
                if (value == _remaining)
                    return;
                _remaining = value;
                OnPropertyChanged();
            }
        }
        private string _sms;
        public string Sms
        {
            get { return _sms; }
            set
            {
                if (value == _sms)
                    return;
                _sms = value;
                OnPropertyChanged();
            }
        }
        private string _data;
        public string Data
        {
            get { return _data; }
            set
            {
                if (value == _data)
                    return;
                _data = value;
                OnPropertyChanged();
            }
        }
        private string _vikingSms;
        public string VikingSms
        {
            get { return _vikingSms; }
            set
            {
                if (value == _vikingSms)
                    return;
                _vikingSms = value;
                OnPropertyChanged();
            }
        }
        private string _vikingMinutes;
        public string VikingMinutes
        {
            get { return _vikingMinutes; }
            set
            {
                if (value == _vikingMinutes)
                    return;
                _vikingMinutes = value;
                OnPropertyChanged();
            }
        }

        //percentages
        private double _remainingPercentage;
        public double RemainingPercentage
        {
            get { return _remainingPercentage; }
            set
            {
                if (value.Equals(_remainingPercentage))
                    return;
                _remainingPercentage = value;
                OnPropertyChanged();
            }
        }
        private double _smsPercentage;
        public double SmsPercentage
        {
            get { return _smsPercentage; }
            set
            {
                if (value.Equals(_smsPercentage))
                    return;
                _smsPercentage = value;
                OnPropertyChanged();
            }
        }
        private double _dataPercentage;
        public double DataPercentage
        {
            get { return _dataPercentage; }
            set
            {
                if (value.Equals(_dataPercentage))
                    return;
                _dataPercentage = value;
                OnPropertyChanged();
            }
        }
        private double _vikingSmsPercentage;
        public double VikingSmsPercentage
        {
            get { return _vikingSmsPercentage; }
            set
            {
                if (value.Equals(_vikingSmsPercentage))
                    return;
                _vikingSmsPercentage = value;
                OnPropertyChanged();
            }
        }
        private double _vikingMinutesPercentage;
        public double VikingMinutesPercentage
        {
            get { return _vikingMinutesPercentage; }
            set
            {
                if (value.Equals(_vikingMinutesPercentage))
                    return;
                _vikingMinutesPercentage = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public void Load(string json)
        {
            _balance = JsonConvert.DeserializeObject<Balance>(json);
            ConvertValues();
        }

        private void ConvertValues()
        {
            if (_balance.is_expired)
            {
                SetExpiredValues();
                return;
            }
            Credit = string.Format("€{0}", _balance.credits);
            Remaining = ConvertDate(_balance.valid_until);
            Data = string.Format("{0} / {1}", Math.Round(_balance.bundles.Where(x => x.type == "data").Sum(x => double.Parse(x.value.Split('.')[0])) / 1024d / 1024d, 0), Math.Round(_balance.bundles.Where(x => x.type == "data").Sum(x => double.Parse(x.assigned.Split('.')[0])) / 1024d / 1024d, 0));
            Sms = string.Format("{0} / {1}", _balance.bundles.Where(x => x.type == "sms").Sum(x => double.Parse(x.value.Split('.')[0])), _balance.bundles.Where(x => x.type == "sms").Sum(x => double.Parse(x.assigned.Split('.')[0])));
            VikingSms = string.Format("{0} / {1}", _balance.bundles.First(bundle => bundle.type == "sms_super_on_net").value.Split('.')[0], _balance.bundles.First(bundle => bundle.type == "sms_super_on_net").assigned.Split('.')[0]);
            int minutes = int.Parse(_balance.bundles.First(bundle => bundle.type == "voice_super_on_net").value.Split('.')[0])/60;
            int seconds = int.Parse(_balance.bundles.First(bundle => bundle.type == "voice_super_on_net").value.Split('.')[0]) % 60;
            VikingMinutes = string.Format("{0}m {1}s", minutes, seconds);
            CalculatePercentages();
        }

        private void SetExpiredValues()
        {
            Credit = string.Format("€{0}", (_balance.credits != null) ? _balance.credits : 0.ToString());
            Remaining = ConvertDate(_balance.valid_until);
            Data = string.Format("{0} / {1}",0,0);
            Sms = string.Format("{0} / {1}", 0,0);
            VikingSms = string.Format("{0} / {1}", 0,_balance.sms_super_on_net_max);
            VikingMinutes = string.Format("{0}m {1}s", ((_balance.voice_super_on_net_max) / 60), 0);
            VikingMinutesPercentage = 0;
            VikingSmsPercentage = 0;
            DataPercentage = 0;
            SmsPercentage = 0;
            RemainingPercentage = 0;
        }

        private string ConvertDate(string validUntil)
        {
            DateTime expires = Convert.ToDateTime(validUntil);
            TimeSpan difference = (expires - DateTime.Now);
            if (difference.Days > 0)
            {
                return string.Format("{0} day{1}", difference.Days, (difference.Days == 1)?"":"s");
            }
            if (difference.Hours > 0)
            {
                return string.Format("{0} hour{1}", difference.Hours, (difference.Hours == 1) ? "" : "s");
            }
            if (difference.Minutes > 0)
            {
                return string.Format("{0} minute{1}", difference.Minutes, (difference.Minutes == 1) ? "" : "s");
            }
            return difference.Seconds > 0 ? string.Format("{0} second{1}", difference.Seconds, (difference.Seconds == 1) ? "" : "s") : "0 days";
        }

        private void CalculatePercentages()
        {
            VikingMinutesPercentage = Calculatepercentage("voice_super_on_net");
            VikingSmsPercentage = Calculatepercentage("sms_super_on_net");
            DataPercentage = Calculatepercentage("data");
            SmsPercentage = Calculatepercentage("sms");
            int totaldays = (Convert.ToDateTime(_balance.valid_until) - Convert.ToDateTime(_balance.valid_until).AddMonths(-1)).Days;
            RemainingPercentage = 100 - ((totaldays - Math.Round((Convert.ToDateTime(_balance.valid_until) - DateTime.Now).TotalDays, 0))/totaldays)*100d;
        }

        private double Calculatepercentage(string bundle)
        {
            return 100 - Math.Round((_balance.bundles.Where(x => x.type == bundle).Sum(x => double.Parse(x.used.Split('.')[0])) / _balance.bundles.Where(x => x.type == bundle).Sum(x => double.Parse(x.assigned.Split('.')[0]))) * 100d, 2);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) 
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}