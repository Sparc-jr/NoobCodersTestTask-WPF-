using Nest;
using System.Data.SQLite;

namespace CSVToDBWithElasticIndexing
{
    internal class AppResources
    {
        public static string CSVFileName;
        public static string dBaseFileName;
        public static SQLiteConnection dBaseConnection;
        public static ElasticClient elasticSearchClient = ElasticsearchHelper.GetESClient();
        public static string indexName = "posts";  // default name
        

    }
}
