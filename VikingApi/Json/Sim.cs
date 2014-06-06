using System.ComponentModel;
using System.Data.Linq.Mapping;
using Newtonsoft.Json;

namespace VikingApi.Json
{
    [Table]
    public class Sim : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public const string MsisdnPropertyName = "Msisdn";
        private string _msisdn;
        [Column(IsPrimaryKey = true, IsDbGenerated = false)]
        [JsonProperty(PropertyName = "msisdn")]
        public string Msisdn
        {
            get { return _msisdn; }
            set
            {
                NotifyPropertyChanging(MsisdnPropertyName);
                _msisdn = value;
                NotifyPropertyChanged(MsisdnPropertyName);
            }
        }

        public const string AliasPropertyName = "Alias";
        private string _alias;
        [Column]
        [JsonProperty(PropertyName = "alias")]
        public string Alias
        {
            get { return _alias; }
            set
            {
                NotifyPropertyChanging(AliasPropertyName);
                _alias = value;
                NotifyPropertyChanged(AliasPropertyName);
            }
        }

        #region notify
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangingEventHandler PropertyChanging;

        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }
        #endregion
    }
}