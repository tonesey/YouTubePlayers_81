using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Centapp.CartoonCommon.Helpers
{
    class FlurryHelper
    {
        public static void LogException(string msg, Exception ex)
        {
            string stacktrace = ex.StackTrace.Substring(0, ex.StackTrace.Length >= 255 ? 255 : ex.StackTrace.Length);
            FlurryWP8SDK.Api.LogError(string.Format("[{0}] {1}", msg, stacktrace), ex);
        }
    }
}
