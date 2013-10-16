using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Tools.Properties;

namespace VikingApi.Json
{
    public class Referral:INotifyPropertyChanged
    {
        #region Json properties
        private string _status;

        [JsonProperty(PropertyName = "status")]
        public string Status
        {
            get { return _status; }
            set
            {
                if (value == _status)
                    return;
                _status = value;
                OnPropertyChanged();
            }
        }

        private string _methodStr;

        [JsonProperty(PropertyName = "method_str")]
        public string MethodStr
        {
            get { return _methodStr; }
            set
            {
                if (value == _methodStr)
                    return;
                _methodStr = value;
                OnPropertyChanged();
            }
        }

        private int _amount;

        [JsonProperty(PropertyName = "amount")]
        public int Amount
        {
            get { return _amount; }
            set
            {
                if (value == _amount)
                    return;
                _amount = value;
                OnPropertyChanged();
            }
        }

        private string _date;

        [JsonProperty(PropertyName = "date")]
        public string Date
        {
            get { return _date; }
            set
            {
                if (value == _date)
                    return;
                _date = value;
                OnPropertyChanged();
            }
        }

        private string _method;

        [JsonProperty(PropertyName = "method")]
        public string Method
        {
            get { return _method; }
            set
            {
                if (value == _method)
                    return;
                _method = value;
                OnPropertyChanged();
            }
        }

        private string _name;

        [JsonProperty(PropertyName = "name")]
        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name)
                    return;
                _name = value;
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
