using Elastic.Apm.Api;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System;

namespace CSVToDBWithElasticIndexing
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            ShutdownMode = ShutdownMode.OnLastWindowClose;
        }

        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string errorMessage = string.Format("An unhandled exception occurred: {0}", e.Exception.Message);
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            File.AppendAllText("error.log", $"[{DateTime.Now}]: {e.Exception.Source}\n {errorMessage}\n");
            e.Handled = true;
        }
    }
}
