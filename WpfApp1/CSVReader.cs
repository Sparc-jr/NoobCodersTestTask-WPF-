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
            AppResources.indexName = Path.GetFileNameWithoutExtension(fileName).ToLower();
            CSVReader.ReadCSVHeader(fileName, AppResources.dBaseFileName);
            CSVReader.ReadCSVTypes(fileName, AppResources.dBaseFileName);
            if (DBase.CreateDBase(AppResources.dBaseFileName) == false) return false;
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


        public static void ReadCSVTypes(string fileCSVPath, string fileDBasePath)
        {
            using (var reader = new StreamReader(fileCSVPath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Read();
                csv.Read();
                var recordTypes = new List<Type>();
                for (int i = 0; i < Post.FieldsCount; i++)
                {
                    var field = csv.GetField(i);
                    recordTypes.Add(TypesResponser.GetObjectType(field));
                }
                Post.typesOfFields = recordTypes;
            }
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
                    //var recordTypes = new List<Type>();
                    var fieldsToIndex = new List<bool>();
                    //var postsTable = new List<Post>();   // подготовка фиксированной коллекции записей для индексации (по ID и первому столбцу)
                    while (csv.Read())
                    {
                        Post nextPost = new Post();
                        for (int i = 0; i < Post.FieldsCount; i++)
                        {
                            var field = csv.GetField(i);
                            //if (firstRecord) recordTypes.Add(TypesResponser.GetObjectType(field));   // TO DO: распознавание типов полей таблицы
                            if (i <= 0) fieldsToIndex.Add(true);
                            else fieldsToIndex.Add(false);
                            nextPost.Fields.Add(Convert.ChangeType(field, Post.typesOfFields[i]));
                        }
                        if (firstRecord)
                        {
                            //Post.typesOfFields = recordTypes;
                            Post.FieldsToIndex = fieldsToIndex;
                        }
                        firstRecord = false;
                        DBase.AddDataToBase(fileDBasePath, nextPost);
                        //postsTable.Add(nextPost);
                    }
                    DBase.ReadDBaseHeader(AppResources.dBaseFileName);
                    ElasticsearchHelper.CreateDocument(AppResources.elasticSearchClient, AppResources.indexName, ElasticsearchHelper.PrepareDataForIndexing()); // создание индекса в эластике
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
