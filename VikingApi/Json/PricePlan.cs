using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Tools.Properties;

namespace VikingApi.Json
{
    public class Price
    {
        public string amount { get; set; }
        public string type { get; set; }
        public int type_id { get; set; }
    }

    public class Bundle
    {
        public int amount { get; set; }
        public string type { get; set; }
        public int type_id { get; set; }
    }

    public class PricePlan : INotifyPropertyChanged
    {
        #region Json properties
        private List<Price> _prices;

        [JsonProperty(PropertyName = "prices")]
        public List<Price> Prices
        {
            get { return _prices; }
            set
            {
                if (value == _prices)
                    return;
                _prices = value;
                OnPropertyChanged();
            }
        }

        private List<Bundle> _bundles;

        [JsonProperty(PropertyName = "bundles")]
        public List<Bundle> Bundles
        {
            get { return _bundles; }
            set
            {
                if (value == _bundles)
                    return;
                _bundles = value;
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

        private string _topUpAmount;

        [JsonProperty(PropertyName = "top_up_amount")]
        public string TopUpAmount
        {
            get { return _topUpAmount; }
            set
            {
                if (value == _topUpAmount)
                    return;
                _topUpAmount = value;
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
