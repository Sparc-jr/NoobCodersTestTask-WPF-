using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace CSVToDBWithElasticIndexing
{
    internal class AppResources
    {
        public static string CSVFileName;
        public static string dBaseFileName;
        public static SQLiteConnection dBaseConnection;
        public static ElasticClient elasticSearchClient = ElasticsearchHelper.GetESClient();
    }
}
