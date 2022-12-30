using Elasticsearch.Net;
using Nest;
using System.Collections.Generic;
using System.Linq;

namespace CSVToDBWithElasticIndexing
{
    public class ElasticsearchHelper
    {
        public static ElasticClient GetESClient()
        {
            try
            {
                var credentials = new BasicAuthenticationCredentials(AppResources.ElasticUserName, AppResources.ElasticPassword);
                var connectionPool = new CloudConnectionPool(AppResources.ElasticCloudID, credentials);
                var connectionSettings = new ConnectionSettings(connectionPool)
                    .EnableApiVersioningHeader()
                    .DefaultIndex(AppResources.indexName)
                    .ThrowExceptions()
                    .EnableDebugMode();
                var elasticClient = new ElasticClient(connectionSettings);
                return elasticClient;
            }
            catch
            {
                Messages.ErrorMessage("Не удалось подключиться к Elastic Cloud, проверьте настройки!");
            }
            return null;
        }
        public static List<Post> PrepareDataForIndexing()
        {
            MainWindow mainWindow = new MainWindow();
            var postsTable = new List<Post>();
            for (int i = 0; i < mainWindow.DataGridSource.Items.Count - 1; i++)
            {
                postsTable.Add(new Post());
            }

            return postsTable;
        }
        public static void CreateDocument(ElasticClient elasticClient, string indexName, List<Post> posts)
        {
            MainWindow mainWindow = new MainWindow();
            elasticClient.DeleteIndex(indexName);
            var postsToIndex = DBase.PrepareDataForIndexing(Post.FieldsToIndex.Where(x => x.isChecked).Select(x => x.name).ToArray());
            var response = elasticClient.IndexMany(postsToIndex, AppResources.indexName);
            if (response.IsValid) Messages.InfoMessage("Данные успешно импортированы. Индекс создан");
            else Messages.ErrorMessage(response.ToString());
            AppResources.tableIsIndexed = true;
        }

        public static List<Record> SearchDocument(ElasticClient elasticClient, string indexName, string stringToSearch)
        {
            var searchResponse = elasticClient.Search<Record>(s => s
                .Size(AppResources.SearchResultsCount)
                .Index(indexName)
                .Query(q => q
                    .Term(t => t.ItemsToIndex, stringToSearch)
            )
                );
            return searchResponse.Documents.ToList();
        }

        public static void DeleteDocument(ElasticClient elasticClient, string indexName, params long[] idList)
        {
            foreach (long id in idList)
            {
                var deleteResponse = elasticClient.Delete<Record>(id, id => id.Index(indexName));
                if (deleteResponse.IsValid) Messages.InfoMessage($"Запись №{id} успешно удалена");
                else Messages.ErrorMessage(deleteResponse.ToString());
            }
        }
    }

}