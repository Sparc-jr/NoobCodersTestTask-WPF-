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
        private static int searchResultsCount;
        public static string ElasticCloudID { get; set; }
        public static string ElasticUserName { get; set; }
        public static string ElasticPassword { get; set; }
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
            SearchResultsCount = 0;
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
