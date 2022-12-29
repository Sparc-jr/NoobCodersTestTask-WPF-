using Nest;
using System.Data.SQLite;
using System.Windows.Controls;

namespace CSVToDBWithElasticIndexing
{
    internal class AppResources
    {
        public static string csvFileName;
        public static string dBaseFileName;
        public static SQLiteConnection dBaseConnection;
        public static ElasticClient elasticSearchClient = ElasticsearchHelper.GetESClient();
        public static string indexName = "posts";  // default name
        public static string elasticCloudID { get; set; }
        public static string elasticUserName { get; set; }
        public static string elasticPassword { get; set; }
        public static int searchResultsCount { get; set; }
    }

    internal class Settings
    {
        private static IniFile iniFile = new IniFile("config.ini");
        private void auto_read()
        {
            if (iniFile.KeyExists("ElasticAuth", "CloudID"))
                AppResources.elasticCloudID = "1";//numericUpDown2.Value = int.Parse(iniFile.ReadINI("SettingForm1", "Height"));
            else AppResources.elasticCloudID = "2";
            //numericUpDown1.Value = this.MinimumSize.Height;

            if (iniFile.KeyExists("ElasticAuth", "username"))
                AppResources.elasticUserName = "user";//numericUpDown1.Value = int.Parse(iniFile.ReadINI("SettingForm1", "Width"));
            else AppResources.elasticUserName = "user2";
            //numericUpDown2.Value = this.MinimumSize.Width;

            if (iniFile.KeyExists("ElasticAuth", "password"))
                AppResources.elasticPassword = "pass";//textBox1.Text = iniFile.ReadINI("Other", "Text");

            if (iniFile.KeyExists("ElasticAuth", "results"))
                AppResources.searchResultsCount = 20;//textBox1.Text = iniFile.ReadINI("Other", "Text");


        }
    }
}
