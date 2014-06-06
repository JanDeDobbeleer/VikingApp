using System.Data.Linq;
using VikingApi.Json;

namespace Fuel.Database.Database
{
    public class FuelDataContext : DataContext
    {
        public FuelDataContext(string connectionString)
            : base(connectionString)
        {
        }

        public Table<Sim> Sims;
    }
}
