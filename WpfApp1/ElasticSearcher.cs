﻿//using Elastic.Apm.Api;
using Elasticsearch.Net;
//using Microsoft.Extensions.Hosting;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;

namespace CSVToDBWithElasticIndexing
{
    public class ElasticsearchHelper
    {
        static string cloudID = "NoobCodersTask:dXMtY2VudHJhbDEuZ2NwLmNsb3VkLmVzLmlvOjQ0MyQ4NjQyZWI4MDg4YTg0ZDUxYjhiMDdiMGE4NDFkMTdiYyRjY2I1NTFlZDY3MWI0MTRhYTc4ZDUyZjIxYThlMGI5Nw==";
        public static ElasticClient GetESClient()
        {

            var credentials = new BasicAuthenticationCredentials("elastic", "ZZwhmkol45v3JtMEZusfiLpe");
            var connectionPool = new CloudConnectionPool(cloudID, credentials);
            var connectionSettings = new ConnectionSettings(connectionPool)
                .EnableApiVersioningHeader()
                .DefaultIndex(AppResources.indexName)
                .ThrowExceptions()
                .EnableDebugMode();
            var elasticClient = new ElasticClient(connectionSettings);
            return elasticClient;
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
            elasticClient.DeleteIndex(indexName);
            var postsToIndex = DBase.PrepareDataForIndexing(Post.namesOfFields.ToArray());
            var response = elasticClient.IndexMany(postsToIndex, AppResources.indexName);
            if (response.IsValid) Messages.InfoMessage("Данные успешно импортированы. Индекс создан");
            else Messages.ErrorMessage(response.ToString());
        }

        public static List<Record> SearchDocument(ElasticClient elasticClient, string indexName, string stringToSearch)
        {
            var searchResponse = elasticClient.Search<Record>(s => s
                .Size(20)
                .Index(indexName)
                .Query(q => q
                    .Term(t => t.Text, stringToSearch)
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