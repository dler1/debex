using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;
using ClosedXML.Excel;
using Debex.Convert.Environment;
using Debex.Convert.Extensions;
using DocumentFormat.OpenXml.Office2010.Excel;
using LargeXlsx;
using Color = System.Drawing.Color;

namespace Debex.Convert
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            App.Current.DispatcherUnhandledException += OnError;
        }

        protected override async void OnLoadCompleted(NavigationEventArgs e)
        {

#if !DEBUG
            var isValidIp = await IoC.Get<IpChecker>().IsValidIp();
            if (isValidIp)
            {
                return;
            }


            MessageBox.Show("Доступ к приложению не одобрен для вашего устройства. Проверьте, что вы подключены к VPN",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            Application.Current.Shutdown();


           

#endif
        }


        private void OnError(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            MessageBox.Show("Упс. произошло ужасное. подробный лог доступен файле ...");
        }
    }
}
