using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Tools.Annotations;

namespace VikingApi.Json
{
    public class Usage:INotifyPropertyChanged
    {
        #region Json Properties
        private bool _isData;

        [JsonProperty (PropertyName = "is_data")]
        public bool Isdata
        {
            get { return _isData; }
            set
            {
                if (value == _isData)
                    return;
                _isData = value;
                OnPropertyChanged();
            }
        }

        private string _startTimestamp;

        [JsonProperty(PropertyName = "start_timestamp")]
        public string StartTimestamp
        {
            get { return _startTimestamp; }
            set
            {
                if (value == _startTimestamp)
                    return;
                _startTimestamp = value;
                OnPropertyChanged();
            }
        }

        private string _balance;

        [JsonProperty(PropertyName = "balance")]
        public string Balance
        {
            get { return _balance; }
            set
            {
                if (value == _balance)
                    return;
                _balance = value;
                OnPropertyChanged();
            }
        }

        private int _duration;

        [JsonProperty(PropertyName = "duration_call")]
        public int Duration
        {
            get { return _duration; }
            set
            {
                if (value == _duration)
                    return;
                _duration = value;
                OnPropertyChanged();
            }
        }

        private string _to;

        [JsonProperty(PropertyName = "to")]
        public string To
        {
            get { return _to; }
            set
            {
                if (value == _to)
                    return;
                _to = value;
                OnPropertyChanged();
            }
        }

        private bool _isSms;

        [JsonProperty(PropertyName = "is_sms")]
        public bool IsSms
        {
            get { return _isSms; }
            set
            {
                if (value == _isSms)
                    return;
                _isSms = value;
                OnPropertyChanged();
            }
        }

        private int _timestamp;

        [JsonProperty(PropertyName = "timestamp")]
        public int Timestamp
        {
            get { return _timestamp; }
            set
            {
                if (value == _timestamp)
                    return;
                _timestamp = value;
                OnPropertyChanged();
            }
        }

        private string _price;

        [JsonProperty(PropertyName = "price")]
        public string Price
        {
            get { return _price; }
            set
            {
                if (value == _price)
                    return;
                _price = value;
                OnPropertyChanged();
            }
        }

        private int _durationConnection;

        [JsonProperty(PropertyName = "duration_connection")]
        public int DurationConnection
        {
            get { return _durationConnection; }
            set
            {
                if (value == _durationConnection)
                    return;
                _durationConnection = value;
                OnPropertyChanged();
            }
        }

        private string _durationHuman;

        [JsonProperty(PropertyName = "duration_human")]
        public string DurationHuman
        {
            get { return _durationHuman; }
            set
            {
                if (value == _durationHuman)
                    return;
                _durationHuman = value;
                OnPropertyChanged();
            }
        }

        private string _pricePlan;

        [JsonProperty(PropertyName = "price_plan")]
        public string PricePlan
        {
            get { return _pricePlan; }
            set
            {
                if (value == _pricePlan)
                    return;
                _pricePlan = value;
                OnPropertyChanged();
            }
        }

        private bool _isIncoming;

        [JsonProperty(PropertyName = "is_incoming")]
        public bool IsIncoming
        {
            get { return _isIncoming; }
            set
            {
                if (value == _isIncoming)
                    return;
                _isIncoming = value;
                OnPropertyChanged();
            }
        }

        private bool _isVoice;

        [JsonProperty(PropertyName = "is_voice")]
        public bool IsVoice
        {
            get { return _isVoice; }
            set
            {
                if (value == _isVoice)
                    return;
                _isVoice = value;
                OnPropertyChanged();
            }
        }

        private bool _isMms;

        [JsonProperty(PropertyName = "is_mms")]
        public bool IsMms
        {
            get { return _isMms; }
            set
            {
                if (value == _isMms)
                    return;
                _isMms = value;
                OnPropertyChanged();
            }
        }

        private string _endTimestamp;

        [JsonProperty(PropertyName = "end_timestamp")]
        public string EndTimestamp
        {
            get { return _endTimestamp; }
            set
            {
                if (value == _endTimestamp)
                    return;
                _endTimestamp = value;
                OnPropertyChanged();
            }
        }

        private bool _isSuperOnNet;

        [JsonProperty(PropertyName = "is_super_on_net")]
        public bool IsSuperOnNet
        {
            get { return _isSuperOnNet; }
            set
            {
                if (value == _isSuperOnNet)
                    return;
                _isSuperOnNet = value;
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
