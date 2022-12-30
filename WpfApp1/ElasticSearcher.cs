using Elasticsearch.Net;
using Nest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;

namespace CSVToDBWithElasticIndexing
{
    public class ElasticsearchHelper
    {
        public static TextQueryType textQueryType = TextQueryType.PhrasePrefix;

        public static ElasticClient GetESClient()
        {
            try
            {
                //File.AppendAllText("error.log", $"[{DateTime.Now}]: get target window - OK\n"); //remove logging
                var credentials = new BasicAuthenticationCredentials(AppResources.ElasticUserName, AppResources.ElasticPassword);
                //File.AppendAllText("error.log", $"[{DateTime.Now}]: set elastic authentication credentials - OK\n");  //remove logging
                var connectionPool = new CloudConnectionPool(AppResources.ElasticCloudID, credentials);
                //File.AppendAllText("error.log", $"[{DateTime.Now}]: set conection pool - OK\n");  //remove logging
                //var location = typeof(IElasticLowLevelClient).GetTypeInfo().Assembly.Location;
                //File.AppendAllText("error.log", $"[{DateTime.Now}]: get elastic client location - OK\nLocation: {location}\n");  //remove logging
                //var version = FileVersionInfo.GetVersionInfo(location)?.ProductVersion;
                //File.AppendAllText("error.log", $"[{DateTime.Now}]: get elastic client version - OK\nVersion: {version}\n");  //remove logging

                var connectionSettings = new ConnectionSettings(connectionPool)
                    .DisableDirectStreaming()
                    .EnableApiVersioningHeader()
                    .DefaultIndex(AppResources.indexName)
                    .ThrowExceptions()
                    .EnableDebugMode();
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var elasticClient = new ElasticClient(connectionSettings);
                return elasticClient;
            }
            catch (SystemException ex)
            {
                Messages.ErrorMessage($"Не удалось подключиться к Elastic Cloud, проверьте настройки! \n{ex.Source.ToString()}\n{ex.Message}\n");
                File.AppendAllText("error.log", $"[{DateTime.Now}]: {ex.Source.ToString()}\n{ex.Message}\n");
            }
            return null;
        }
        public static List<Post> PrepareDataForIndexing()
        {
            var targetWindow = Application.Current.Windows.Cast<MainWindow>().FirstOrDefault(window => window is MainWindow) as MainWindow;
            var postsTable = new List<Post>();
            for (int i = 0; i < targetWindow.DataGridSource.Items.Count - 1; i++)
            {
                postsTable.Add(new Post());
            }
            return postsTable;
        }
        public static void CreateDocument(ElasticClient elasticClient, string indexName, List<Post> posts)
        {
            try
            {
                MainWindow mainWindow = new MainWindow();
                elasticClient.DeleteIndex(indexName);
                var postsToIndex = DBase.PrepareDataForIndexing(Post.FieldsToIndex.Where(x => x.isChecked).Select(x => x.name).ToArray());
                var response = elasticClient.IndexMany(postsToIndex, AppResources.indexName);
                if (response.IsValid) Messages.InfoMessage("Данные успешно импортированы. Индекс создан");
                else Messages.ErrorMessage(response.ToString());
                AppResources.tableIsIndexed = true;
            }
            catch (SystemException ex)
            {
                Messages.ErrorMessage($"Не удалось создать индекс. \n{ex.Source.ToString()}\n{ex.Message}\n");
            }
        }

        public static List<Record> SearchDocument(ElasticClient elasticClient, string indexName, string stringToSearch)
        {

            var searchResponse = elasticClient.Search<Record>(sd => sd
                .Size(AppResources.SearchResultsCount)
                .Index(indexName)
                .Query(q => q
                    .MultiMatch(query => query
                        .Type(textQueryType)
                        .Fields("*")
                        .Query(stringToSearch)
                        )
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