using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Phone.Reactive;
using VikingApi.Json;

namespace Fuel.Database.Database
{
    public class FuelDatabase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly FuelDataContext _fuelDb;
        private const string DbConnectionString = "Data Source=isostore:/Database.sdf";

        public const string SimsPropertyName = "Sims";
        private ObservableCollection<Sim> _sims = new ObservableCollection<Sim>();
        public ObservableCollection<Sim> Sims
        {
            get { return _sims; }
            set
            {
                _sims = value;
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged(SimsPropertyName);
            }
        }

        public FuelDatabase()
        {
            _fuelDb = new FuelDataContext(DbConnectionString);
            LoadItems();
        }

        private void LoadItems()
        {
            var simItemsInDb = from Sim sim in _fuelDb.Sims
                                   select sim;
            // Query the database and load all station items.
            if (simItemsInDb.Any())
                Sims = new ObservableCollection<Sim>(simItemsInDb);
        }

        public void DeleteAllSims()
        {
            try
            {
                _fuelDb.Sims.DeleteAllOnSubmit(Sims);
                SubmitChanges();
                Sims.Clear();
            }
            catch (Exception e)
            {
#if(DEBUG)
                Debug.WriteLine(e.Message);
#endif
            }
        }

        public void AddSims(IEnumerable<Sim> sims)
        {
            DeleteAllSims();
            var enumerable = sims as Sim[] ?? sims.ToArray();
            _fuelDb.Sims.InsertAllOnSubmit(enumerable);
            SubmitChanges();
            foreach (var sim in enumerable)
            {
                Sims.Add(sim);
            }
        }

        public void SubmitChanges()
        {
            _fuelDb.SubmitChanges();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
