using CsvHelper;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;

namespace CSVToDBWithElasticIndexing
{
    internal class CSVReader
    {
        public static bool OpenFile(string fileName)
        {
            AppResources.csvFileName = fileName;
            AppResources.dBaseFileName = $"{Path.GetDirectoryName(fileName)}\\{Path.GetFileNameWithoutExtension(fileName)}.db";
            AppResources.indexName = Path.GetFileNameWithoutExtension(fileName).ToLower();
            CSVReader.ReadCSVHeader(fileName);
            CSVReader.ReadCSVTypes(fileName);
            AppResources.dBaseConnection.Dispose();
            if (DBase.CreateDBase(AppResources.dBaseFileName) == false) return false;
            return CSVReader.ReadCSVandSaveToDataBase(fileName, AppResources.dBaseFileName);
        }



        public static bool ReadCSVHeader(string fileCSVPath)
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


        public static void ReadCSVTypes(string fileCSVPath)
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
                    csv.Read();
                    while (csv.Read())
                    {
                        Post nextPost = new Post();
                        for (int i = 0; i < Post.FieldsCount; i++)
                        {
                            var field = csv.GetField(i);
                            nextPost.Fields.Add(Convert.ChangeType(field, Post.typesOfFields[i]));
                        }
                        DBase.AddDataToBase(fileDBasePath, nextPost);
                    }
                    DBase.ReadDBaseHeader(AppResources.dBaseFileName);
                    AppResources.tableIsIndexed = false;
                    Messages.InfoMessage("Файл открыт. Данные успешно экспортированы в БД");
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                AppResources.dBaseConnection.Close();
                return false;

            }
            return true;
        }

    }
}
