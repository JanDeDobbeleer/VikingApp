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
        private UsageViewmodel _detailsViewmodel;
        public UsageViewmodel DetailsViewmodel
        {
            get { return _detailsViewmodel; }
            set
            {
                if (value.Equals(_detailsViewmodel))
                    return;
                _detailsViewmodel = value;
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

        public MainViewmodel()
        {
            DetailsViewmodel = new UsageViewmodel();
            MainPivotViewmodel = new MainPivotViewmodel();
            SimViewmodel = new SimDetailsViewmodel();
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
