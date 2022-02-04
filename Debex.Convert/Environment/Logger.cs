using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debex.Convert.Environment
{
    public class Logger
    {
        private bool enabled = false;
        public void Log(string message)
        {
            if (enabled)
                Debug.WriteLine(message);
        }

        public void LogIf(string message, bool shouldLog)
        {
            if (shouldLog) Log(message);
        }
    }
}
