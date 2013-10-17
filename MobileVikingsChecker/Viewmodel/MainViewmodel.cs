using System.ComponentModel;
using System.Runtime.CompilerServices;
using Tools.Properties;

namespace Fuel.Viewmodel
{
    public class MainViewmodel
    {
        private MainPivotViewmodel _mainViewmodel;
        public MainPivotViewmodel MainPivotViewmodel
        {
            get { return _mainViewmodel; }
            set
            {
                if (value.Equals(_mainViewmodel))
                    return;
                _mainViewmodel = value;
                OnPropertyChanged();
            }
        }
        private UsageViewmodel _usageViewmodel;
        public UsageViewmodel UsageViewmodel
        {
            get { return _usageViewmodel; }
            set
            {
                if (value.Equals(_usageViewmodel))
                    return;
                _usageViewmodel = value;
                OnPropertyChanged();
            }
        }

        private SimDetailsViewmodel _simViewmodel;
        public SimDetailsViewmodel SimViewmodel
        {
            get { return _simViewmodel; }
            set
            {
                if (value.Equals(_simViewmodel))
                    return;
                _simViewmodel = value;
                OnPropertyChanged();
            }
        }

        private ProfileViewmodel _profileViewmodel;
        public ProfileViewmodel ProfileViewmodel
        {
            get { return _profileViewmodel; }
            set
            {
                if (value.Equals(_profileViewmodel))
                    return;
                _profileViewmodel = value;
                OnPropertyChanged();
            }
        }

        public MainViewmodel()
        {
            UsageViewmodel = new UsageViewmodel();
            MainPivotViewmodel = new MainPivotViewmodel();
            SimViewmodel = new SimDetailsViewmodel();
            ProfileViewmodel = new ProfileViewmodel();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
