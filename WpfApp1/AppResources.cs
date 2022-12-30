using Nest;
using System;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;

namespace CSVToDBWithElasticIndexing
{
    internal class AppResources
    {
        public static string csvFileName;
        public static string dBaseFileName;
        public static SQLiteConnection dBaseConnection;
        public static bool tableIsIndexed { get; set; }

        public static string indexName = "posts";  // default name
        public static string elasticCloudID { get; set; }
        public static string elasticUserName { get; set; }
        public static string elasticPassword { get; set; }
        public static int searchResultsCount { get; set; }
        public static ElasticClient elasticSearchClient { get; set; } //= ElasticsearchHelper.GetESClient();

    }

    internal static class Settings
    {
        private static IniFile iniFile = new IniFile("config.ini");
        internal static void auto_read()
        {
            if (iniFile.KeyExists("results", "AppSettings"))
                AppResources.searchResultsCount = int.Parse(iniFile.Read("AppSettings", "results"));
            else AppResources.searchResultsCount = 20;
            
            if (iniFile.KeyExists("CloudID", "ElasticAuth") && iniFile.KeyExists("username", "ElasticAuth")
                && iniFile.KeyExists("password", "ElasticAuth"))
            {
                AppResources.elasticCloudID = iniFile.Read("ElasticAuth", "CloudID");
                AppResources.elasticUserName = iniFile.Read("ElasticAuth", "username");
                AppResources.elasticPassword = iniFile.Read("ElasticAuth", "password");
            }
            else
            {
                Messages.ErrorMessage("Введите настройки авторизации в elastic");
                settingsWindow.ShowSettings();
            }
        }
        internal static void SaveSettingsToIni()
        {
            iniFile.Write("ElasticAuth", "CloudID", AppResources.elasticCloudID);
            iniFile.Write("ElasticAuth", "username", AppResources.elasticUserName);
            iniFile.Write("ElasticAuth", "password", AppResources.elasticPassword);
            iniFile.Write("AppSettings", "results", AppResources.searchResultsCount.ToString());
            Messages.InfoMessage("Настройки сохранены");
        }
    }
}
