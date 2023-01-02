﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;


namespace CSVToDBWithElasticIndexing
{
    internal class DBase
    {

        public static bool OpenFile(string fileName)
        {
            AppResources.dBaseFileName = fileName;
            ConnectDBase();
            ReadDBaseHeader();
            MainWindow mainWindow = new ();
            mainWindow.RefreshDataGridView();
            AppResources.TableIsIndexed = false;
            Messages.InfoMessage("Файл открыт");
            return true;
        }
        public static void ReadDBaseHeader()
        {
            Post.Clear();
            var sqlCommand = new SQLiteCommand($"SELECT * FROM {Path.GetFileNameWithoutExtension(AppResources.dBaseFileName)}", AppResources.dBaseConnection);
            var dataReader = sqlCommand.ExecuteReader();
            Post.FieldsCount = dataReader.FieldCount;
            for (var i = 0; i < Post.FieldsCount; i++)
            {
                //Post.namesOfFields.Add(dataReader.GetName(i));
                Post.TypesOfFields.Add(TypesResponser.GetDBaseColumnType(dataReader.GetDataTypeName(i)));
                Post.FieldsToIndex.Add(new(false, dataReader.GetName(i)));
            }
            Post.FieldsToIndex[1].isChecked = true;
        }
        public static bool ExportToDBase(string dBaseName)
        {
            if (!File.Exists(dBaseName))
            {
                SQLiteConnection.CreateFile(dBaseName);
                return CreateNewDBase(dBaseName);
            }
            else
            {
                var dialogResult = Messages.OverWriteExistedDBase(dBaseName);
                if (dialogResult == Messages.DialogResult.Yes)
                {
                    AppResources.dBaseConnection.Dispose();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    File.Delete(dBaseName);
                    SQLiteConnection.CreateFile(dBaseName);
                    return CreateNewDBase(dBaseName);
                }
                else if (dialogResult == Messages.DialogResult.No)
                {
                    return ConnectDBase();
                }
            }
            return false;
        }
        public static bool CreateNewDBase(string dBaseName)
        {
            try
            {
                var sQLCommand = new SQLiteCommand();
                AppResources.dBaseConnection = new SQLiteConnection("Data Source=" + dBaseName + ";Version=3;");
                AppResources.dBaseConnection.Open();
                sQLCommand.Connection = AppResources.dBaseConnection;
                var commandLine = new StringBuilder();
                commandLine.Append("CREATE TABLE IF NOT EXISTS ");
                commandLine.Append(Path.GetFileNameWithoutExtension(dBaseName));
                commandLine.Append(" (");
                commandLine.Append("id INTEGER PRIMARY KEY AUTOINCREMENT, ");
                for (int i = 0; i < Post.FieldsCount; i++)
                {
                    commandLine.Append(Post.FieldsToIndex[i].name);
                    switch (Post.TypesOfFields[i].Name)
                    {
                        case "long": commandLine.Append(" INTEGER"); break;
                        case "DateTime": commandLine.Append(" DATETIME"); break;
                        case "double": commandLine.Append(" float"); break;
                        default: commandLine.Append(" TEXT"); break;
                    }
                    if (i < Post.FieldsCount - 1) commandLine.Append(", ");
                }
                commandLine.Append(')');

                sQLCommand.CommandText = commandLine.ToString();
                sQLCommand.ExecuteNonQuery();
                commandLine.Clear();
                commandLine.Append($"CREATE UNIQUE INDEX record ON {Path.GetFileNameWithoutExtension(dBaseName)}(");
                for (int i = 0; i < Post.FieldsCount; i++)
                {
                    commandLine.Append(Post.FieldsToIndex[i].name);
                    if (i < Post.FieldsCount - 1) commandLine.Append(", ");
                }
                commandLine.Append(')');
                sQLCommand.CommandText = commandLine.ToString();
                sQLCommand.ExecuteNonQuery();
                return true;
            }
            catch (SQLiteException ex)
            {
                Messages.ErrorMessage("Error: " + ex.Message);
                return false;
            }
        }

        public static bool ConnectDBase()
        {
            try
            {
                var sQLCommand = new SQLiteCommand();
                AppResources.dBaseConnection.Dispose();
                AppResources.dBaseConnection = new SQLiteConnection("Data Source=" + AppResources.dBaseFileName + ";Version=3;New=False;Compress=True;");
                AppResources.dBaseConnection.Open();
            }
            catch (SQLiteException ex)
            {
                AppResources.dBaseConnection.Dispose();
                Messages.ErrorMessage("Error: " + ex.Message);
                return false;
            }
            return true;
        }

        public static void AddDataToBase(string dBaseName, Post record)
        {
            if (AppResources.dBaseConnection.State != ConnectionState.Open)
            {
                Messages.ErrorMessage("No connection with database");
                return;
            }
            var sQLCommand = new SQLiteCommand
            {
                Connection = AppResources.dBaseConnection
            };
            var commandLine = new StringBuilder();
            try
            {
                commandLine.Append($"INSERT OR IGNORE INTO {Path.GetFileNameWithoutExtension(dBaseName)} (");
                for (int i = 0; i < Post.FieldsCount; i++)
                {
                    commandLine.Append($"'{Post.FieldsToIndex[i].name}'");
                    if (i < Post.FieldsCount - 1) commandLine.Append(", ");
                }


                commandLine.Append(") Values (");
                for (int i = 0; i < Post.FieldsCount; i++)
                {
                    commandLine.Append($"@{Post.FieldsToIndex[i].name}");
                    if (i < Post.FieldsCount - 1) commandLine.Append(", ");
                }
                commandLine.Append(')');
                for (int i = 0; i < Post.FieldsCount; i++)
                {
                    var sqlParameter = new SQLiteParameter()
                    {
                        ParameterName = $"@{Post.FieldsToIndex[i].name}",
                        Value = record.Fields[i]
                    };
                    sQLCommand.Parameters.Add(sqlParameter);
                }

                sQLCommand.CommandText = commandLine.ToString();
                sQLCommand.CommandType = CommandType.Text;
                sQLCommand.ExecuteNonQuery();

            }
            catch (SQLiteException ex)
            {
                Messages.ErrorMessage("Error: " + ex.Message);
            }
        }
        public static List<Record> PrepareDataForIndexing(params string[] fieldsNames)
        {
            var fields = new StringBuilder();
            fields.AppendJoin(", ", fieldsNames);
            var sQLiteDataAdapter = new SQLiteDataAdapter($"SELECT id, {fields} FROM {Path.GetFileNameWithoutExtension(AppResources.dBaseFileName)}",
                                                                        AppResources.dBaseConnection);
            var dataSet = new DataSet();
            sQLiteDataAdapter.Fill(dataSet);
            var postsTable = new List<Record>();
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                var items = new List<object>();
                //items.Add((long)row["id"]);
                for (int i = 0; i < Post.FieldsCount; i++)
                {
                    if (!Post.FieldsToIndex[i].isChecked)
                    {
                        continue;
                    }
                    items.Add(row[Post.FieldsToIndex[i].name]);
                }
                postsTable.Add(new Record((long)row["id"], items));
            }
            return postsTable;
        }

        public static bool DeleteDBaseRow(params long[] idList)
        {
            try
            {
                var sQLCommand = new SQLiteCommand
                {
                    Connection = AppResources.dBaseConnection
                };
                foreach (long id in idList)
                {
                    sQLCommand.CommandText = $"DELETE FROM {Path.GetFileNameWithoutExtension(AppResources.dBaseFileName)} WHERE id like {id}";
                    sQLCommand.ExecuteNonQuery();
                }
                return true;
            }
            catch (SQLiteException ex)
            {
                Messages.ErrorMessage("Error: " + ex.Message);
                return false;
            }
        }
    }
}
