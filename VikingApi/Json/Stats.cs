using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Tools.Properties;

namespace VikingApi.Json
{
    public class Stats : INotifyPropertyChanged
    {
        #region Json properties
        private int _usedPoints;

        [JsonProperty(PropertyName = "used_points")]
        public int UsedPoints
        {
            get { return _usedPoints; }
            set
            {
                if (value == _usedPoints)
                    return;
                _usedPoints = value;
                OnPropertyChanged();
            }
        }

        private int _unusedPoints;

        [JsonProperty(PropertyName = "unused_points")]
        public int UnusedPoints
        {
            get { return _unusedPoints; }
            set
            {
                if (value == _unusedPoints)
                    return;
                _unusedPoints = value;
                OnPropertyChanged();
            }
        }

        private int _waitingPoints;

        [JsonProperty(PropertyName = "waiting_points")]
        public int WaitingPoints
        {
            get { return _waitingPoints; }
            set
            {
                if (value == _waitingPoints)
                    return;
                _waitingPoints = value;
                OnPropertyChanged();
            }
        }

        private int _topupsused;

        [JsonProperty(PropertyName = "topups_used")]
        public int Topupsused
        {
            get { return _topupsused; }
            set
            {
                if (value == _topupsused)
                    return;
                _topupsused = value;
                OnPropertyChanged();
            }
        }

        private int _earnedPoints;

        [JsonProperty(PropertyName = "earned_points")]
        public int EarnedPoints
        {
            get { return _earnedPoints; }
            set
            {
                if (value == _earnedPoints)
                    return;
                _earnedPoints = value;
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
