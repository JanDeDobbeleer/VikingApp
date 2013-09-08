using System.ComponentModel;
using System.Runtime.CompilerServices;
using Tools.Annotations;

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
        private DetailsViewmodel _detailsViewmodel;
        public DetailsViewmodel DetailsViewmodel
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

        public MainViewmodel()
        {
            DetailsViewmodel = new DetailsViewmodel();
            MainPivotViewmodel = new MainPivotViewmodel();
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
