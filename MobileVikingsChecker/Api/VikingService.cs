using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuel.Api
{
    public class VikingService
    {
        #region Constructor
        private static VikingService _sInstance;
        private static readonly object SInstanceSync = new object();

        protected VikingService()
        {
        }

        /// <summary>
        /// Returns an instance (a singleton)
        /// </summary>
        /// <returns>a singleton</returns>
        /// <remarks>
        /// This is an implementation of the singelton design pattern.
        /// </remarks>
        public static VikingService GetInstance()
        {
            // This implementation of the singleton design pattern prevents 
            // unnecessary locks (using the double if-test)
            if (_sInstance == null)
            {
                lock (SInstanceSync)
                {
                    if (_sInstance == null)
                    {
                        _sInstance = new VikingService();
                    }
                }
            }
            return _sInstance;
        }
        #endregion
    }
}
