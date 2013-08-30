using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace Tools
{
    public static class Error
    {
        public static bool HandleError(object result, string message)
        {
            if (result != null)
                return false;
            Message.ShowToast(message);
            return true;
        }
    }
}
