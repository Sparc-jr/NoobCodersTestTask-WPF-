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
            MakeValidDBaseName(fileName);
            Post.Clear();
            CSVReader.ReadCSVHeader(fileName);
            CSVReader.ReadCSVTypes(fileName);
            AppResources.DBaseConnection.Dispose();
            if (DBase.ExportToDBase(AppResources.DBaseFileName) == false) return false;
            return CSVReader.ReadCSVandSaveToDataBase(fileName, AppResources.DBaseFileName);
        }

        private static void MakeValidDBaseName(string fileName)
        {
            AppResources.DBaseFileName = $"{Path.GetDirectoryName(fileName)}\\{Path.GetFileNameWithoutExtension(fileName.Replace(' ', '_').Replace('-', '_'))}.db";
            if (int.TryParse(AppResources.DBaseFileName[0].ToString(), out _)) AppResources.DBaseFileName = '_' + AppResources.DBaseFileName;
        }

        public static bool ReadCSVHeader(string fileCSVPath)
        {
            try
            {
                using (var reader = new StreamReader(fileCSVPath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Read();
                    csv.ReadHeader();
                    var fieldsNames = csv.HeaderRecord.ToList();
                    foreach (var field in fieldsNames)
                    {
                        Post.FieldsToIndex.Add(new FieldsToIndexSelection(false, field.Replace(' ','_').ToLower()));
                    }
                    Post.FieldsCount = Post.FieldsToIndex.Count;
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
                    recordTypes.Add(TypesResponser.GetObjectType(field.Trim()));
                }
                Post.TypesOfFields = recordTypes;
            }
        }

        public static bool ReadCSVandSaveToDataBase(string fileCSVPath, string fileDBasePath)
        {
            try
            {

                using (var reader = new StreamReader(fileCSVPath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Read();
                    while (csv.Read())
                    {
                        var nextPost = new Post();
                        for (int i = 0; i < Post.FieldsCount; i++)
                        {
                            var field = csv.GetField(i);
                            nextPost.Fields.Add(Convert.ChangeType(field.Trim(), Post.TypesOfFields[i]));
                        }
                        DBase.AddDataToBase(fileDBasePath, nextPost);
                    }
                    DBase.ReadDBaseHeader();
                    AppResources.TableIsIndexed = false;
                    Messages.InfoMessage("Файл открыт. Данные успешно экспортированы в БД");
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                AppResources.DBaseConnection.Close();
                return false;

            }
            return true;
        }

    }
}
