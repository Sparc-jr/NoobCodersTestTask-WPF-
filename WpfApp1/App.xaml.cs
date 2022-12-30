using System;
using System.IO;
using System.Windows;

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
