using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Tools.Properties;

namespace VikingApi.Json
{
    public class SimCard:INotifyPropertyChanged
    {
        #region Json properties
        private string _msisdn;

        [JsonProperty(PropertyName = "msisdn")]
        public string Msisdn
        {
            get { return _msisdn; }
            set
            {
                if (value == _msisdn)
                    return;
                _msisdn = value;
                OnPropertyChanged();
            }
        }
        private string _cardNumber;

        [JsonProperty(PropertyName = "cardnumber")]
        public string CardNumber
        {
            get { return _cardNumber; }
            set
            {
                if (value == _cardNumber)
                    return;
                _cardNumber = value;
                OnPropertyChanged();
            }
        }

        private string _pin2;

        [JsonProperty(PropertyName = "pin2")]
        public string Pin2
        {
            get { return _pin2; }
            set
            {
                if (value == _pin2)
                    return;
                _pin2 = value;
                OnPropertyChanged();
            }
        }

        private string _puk1;

        [JsonProperty(PropertyName = "puk1")]
        public string Puk1
        {
            get { return _puk1; }
            set
            {
                if (value == _puk1)
                    return;
                _puk1 = value;
                OnPropertyChanged();
            }
        }

        private string _pin1;

        [JsonProperty(PropertyName = "pin1")]
        public string Pin1
        {
            get { return _pin1; }
            set
            {
                if (value == _pin1)
                    return;
                _pin1 = value;
                OnPropertyChanged();
            }
        }

        private string _puk2;

        [JsonProperty(PropertyName = "puk2")]
        public string Puk2
        {
            get { return _puk2; }
            set
            {
                if (value == _puk2)
                    return;
                _puk2 = value;
                OnPropertyChanged();
            }
        }

        private string _imsi;

        [JsonProperty(PropertyName = "imsi")]
        public string Imsi
        {
            get { return _imsi; }
            set
            {
                if (value == _imsi)
                    return;
                _imsi = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
