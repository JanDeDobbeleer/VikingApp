using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tools.Properties;

namespace VikingApi.Json
{
    public class Links:INotifyPropertyChanged
    {
        #region Json properties
        private string _alias;

        [JsonProperty(PropertyName = "alias")] 
        public string Alias
        {
            get { return _alias; }
            set
            {
                if (value == _alias)
                    return;
                _alias = value;
                OnPropertyChanged();
            }
        }

        private string _link;

        [JsonProperty(PropertyName = "link")] 
        public string Link
        {
            get { return _link; }
            set
            {
                if (value == _link)
                    return;
                _link = value;
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
