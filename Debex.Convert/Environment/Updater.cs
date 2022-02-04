using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoUpdaterDotNET;

namespace Debex.Convert.Enviroment
{
    public class Updater
    {
        private const bool ShouldCheckUpdates = false;
        private const string UpdatePathXml = "localhost/update.xml";
        private const int MajorVersion = 9;
        private const int MinorVersion = 44;
      
        public void CheckUpdates()
        {
            if (!ShouldCheckUpdates) return;
            
            AutoUpdater.InstalledVersion = new Version(MajorVersion, MinorVersion);
            AutoUpdater.ShowSkipButton = false;
            AutoUpdater.ShowRemindLaterButton = false;
            AutoUpdater.UpdateMode = Mode.Forced;
            AutoUpdater.RunUpdateAsAdmin = false;
            AutoUpdater.AppTitle = "Debex.Convert";
            AutoUpdater.Start(UpdatePathXml);
        }
    }
}
