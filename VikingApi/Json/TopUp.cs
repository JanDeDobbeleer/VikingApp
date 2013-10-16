using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Tools.Properties;

namespace VikingApi.Json
{
    public class TopUp:INotifyPropertyChanged
    {
        #region static properties
        public static string Msisdn = "msisdn"; //number
        public static string FromDate = "from_date"; //results after given date (format: `YYYY-MM-DDTHH:MM:SS`). Default: today
        public static string UntilDate = "until_date"; //results before given date (format: `YYYY-MM-DDTHH:MM:SS`). Default: tomorrow
        public static string PageSize = "page_size"; //how many results are on 1 page. Default: 25
        public static string Page = "page"; //page number of results. Default: 1
        #endregion

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
        
        private string _amount;

        [JsonProperty(PropertyName = "amount")]
        public string Amount
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
        
        private string _amountExVat;

        [JsonProperty(PropertyName = "amount_ex_vat")]
        public string AmountExVat
        {
            get { return _amountExVat; }
            set
            {
                if (value == _amountExVat)
                    return;
                _amountExVat = value;
                OnPropertyChanged();
            }
        }

        private string _executedOn;

        [JsonProperty(PropertyName = "executed_on")]
        public string ExecutedOn
        {
            get { return _executedOn; }
            set
            {
                if (value == _executedOn)
                    return;
                _executedOn = value;
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

        private string _paymentReceivedOn;

        [JsonProperty(PropertyName = "payment_received_on")]
        public string PaymentReceivedOn
        {
            get { return _paymentReceivedOn; }
            set
            {
                if (value == _paymentReceivedOn)
                    return;
                _paymentReceivedOn = value;
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
