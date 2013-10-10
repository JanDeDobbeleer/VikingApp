using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using UpdateLiveTile.Properties;

namespace UpdateLiveTile.Classes
{
    public class UserBalance : INotifyPropertyChanged
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
        private int _remaining;
        public int Remaining
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
        #endregion

        public void Load(string json)
        {
            _balance = JsonConvert.DeserializeObject<Balance>(json);
            ConvertValues();
        }

        private void ConvertValues()
        {
            Credit = string.Format("€{0}", _balance.credits);
            Remaining = ConvertDate(_balance.valid_until);
            Data = Math.Round(_balance.bundles.Where(x => x.type == "data").Sum(x => double.Parse(x.value.Split('.')[0])) / 1024d / 1024d, 0).ToString();
            Sms = _balance.bundles.Where(x => x.type == "sms").Sum(x => double.Parse(x.value.Split('.')[0])).ToString();
            VikingSms = _balance.bundles.First(bundle => bundle.type == "sms_super_on_net").value.Split('.')[0];
            var minutes = int.Parse(_balance.bundles.First(bundle => bundle.type == "voice_super_on_net").value.Split('.')[0]) / 60;
            var seconds = int.Parse(_balance.bundles.First(bundle => bundle.type == "voice_super_on_net").value.Split('.')[0]) % 60;
            VikingMinutes = string.Format("{0}m {1}s", minutes, seconds);
        }

        private int ConvertDate(string validUntil)
        {
            var expires = Convert.ToDateTime(validUntil);
            var difference = (expires - DateTime.Now);
            if (difference.Days > 0)
            {
                return difference.Days;
            }
            return (difference.Seconds > 0) ? 1 : 0;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}