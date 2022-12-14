using CsvHelper;
using CSVToDBWithElasticIndexing;
//using Microsoft.Extensions.Hosting;
using Microsoft.SqlServer.Server;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CSVToDBWithElasticIndexing
{
    internal class CSVReader
    {
        public static bool OpenFile(string fileName)
        {
            AppResources.CSVFileName = fileName;
            AppResources.dBaseFileName = $"{Path.GetDirectoryName(fileName)}\\{Path.GetFileNameWithoutExtension(fileName)}.db";
            CSVReader.ReadCSVHeader(fileName, AppResources.dBaseFileName);
            if (DBase.CreateDBASE(AppResources.dBaseFileName) == false) return false;
            return CSVReader.ReadCSVandSaveToDataBase(fileName, AppResources.dBaseFileName);
        }
        
        
        
        public static bool ReadCSVHeader(string fileCSVPath, string fileDBasePath)
        {
            string[] fields = null;
            try
            {
                using (var reader = new StreamReader(fileCSVPath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    bool firstRecord = true;
                    csv.Read();
                    csv.ReadHeader();
                    Post.namesOfFields = csv.HeaderRecord.ToList();
                    Post.FieldsCount = Post.namesOfFields.Count;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static bool ReadCSVandSaveToDataBase(string fileCSVPath, string fileDBasePath)
        {
            string[] fields = null;
            try
            {

                using (var reader = new StreamReader(fileCSVPath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    AppResources.dBaseConnection.Open();
                    bool firstRecord = true;
                    csv.Read();
                    var recordTypes = new List<Type>();
                    var fieldsToIndex = new List<bool>();
                    //var postsTable = new List<Post>();   // подготовка фиксированной коллекции записей для индексации (по ID и первому столбцу)
                    while (csv.Read())
                    {
                        Post nextPost = new Post();
                        for (int i = 0; i < Post.FieldsCount; i++)
                        {
                            var field = csv.GetField(i);
                            nextPost.Fields.Add(field);
                            if (firstRecord) recordTypes.Add(field.GetType());   // TO DO: распознавание типов полей таблицы
                            if (i <= 0) fieldsToIndex.Add(true);
                            else fieldsToIndex.Add(false);
                        }
                        if (firstRecord)
                        {
                            Post.typesOfFields = recordTypes;
                            Post.FieldsToIndex = fieldsToIndex;
                        }
                        firstRecord = false;
                        DBase.AddDataToBase(fileDBasePath, nextPost);
                        //postsTable.Add(nextPost);
                    }
                    ElasticsearchHelper.CreateDocument(AppResources.elasticSearchClient, "posts", ElasticsearchHelper.PrepareDataForIndexing()); // создание индекса в эластике
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                return false;
            }
            finally
            {
                AppResources.dBaseConnection.Close();
            }
            return true;
        }

    }
}
