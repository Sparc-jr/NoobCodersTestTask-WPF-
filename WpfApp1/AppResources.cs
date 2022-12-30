using Nest;
using System.Data.SQLite;

namespace CSVToDBWithElasticIndexing
{
    internal class AppResources
    {
        public static string csvFileName;
        public static string dBaseFileName;
        public static SQLiteConnection dBaseConnection;
        public static bool tableIsIndexed { get; set; }
        public static string indexName = "posts";  // default name
        private static string elasticCloudID;
        private static string elasticUserName;
        private static string elasticPassword;
        private static int searchResultsCount;
        public static string ElasticCloudID
        {
            get
            {
                return elasticCloudID;
            }
            set
            {
                elasticCloudID = value != string.Empty ? value : "none:NoneCloudIDNoneCloudID=="; //hcnkgYmxvYg
            }
        }
        public static string ElasticUserName
        {
            get
            {
                return elasticUserName;
            }
            set
            {
                elasticUserName = value != string.Empty ? value : "user";
            }
        }

        public static string ElasticPassword
        {
            get
            {
                return elasticPassword;
            }
            set
            {
                elasticPassword = value != string.Empty ? value : "pass";
            }
        }
        public static int SearchResultsCount
        {
            get
            {
                return searchResultsCount;
            }
            set
            {
                searchResultsCount = value > 0 ? value : 1;
            }
        }
        public static ElasticClient elasticSearchClient { get; set; }
        public AppResources()
        {
            //ElasticCloudID = string.Empty;
            //ElasticUserName = string.Empty;
            //ElasticPassword = string.Empty;
            //SearchResultsCount = 0;
        }
    }

    internal static class Settings
    {
        private static IniFile iniFile = new IniFile("config.ini");
        internal static void ReadConfigIni()
        {
            if (iniFile.KeyExists("results", "AppSettings"))
                AppResources.SearchResultsCount = int.Parse(iniFile.Read("AppSettings", "results"));
            else AppResources.SearchResultsCount = 20;

            if (iniFile.KeyExists("CloudID", "ElasticAuth") && iniFile.KeyExists("username", "ElasticAuth")
                && iniFile.KeyExists("password", "ElasticAuth"))
            {
                AppResources.ElasticCloudID = iniFile.Read("ElasticAuth", "CloudID");
                AppResources.ElasticUserName = iniFile.Read("ElasticAuth", "username");
                AppResources.ElasticPassword = iniFile.Read("ElasticAuth", "password");
            }
            else
            {
                Messages.ErrorMessage("Введите настройки авторизации в elastic");
                settingsWindow.ShowSettings();
            }
        }
        internal static void SaveSettingsToIni()
        {
            iniFile.Write("ElasticAuth", "CloudID", AppResources.ElasticCloudID);
            iniFile.Write("ElasticAuth", "username", AppResources.ElasticUserName);
            iniFile.Write("ElasticAuth", "password", AppResources.ElasticPassword);
            iniFile.Write("AppSettings", "results", AppResources.SearchResultsCount.ToString());
            Messages.InfoMessage("Настройки сохранены");
        }
    }
}
