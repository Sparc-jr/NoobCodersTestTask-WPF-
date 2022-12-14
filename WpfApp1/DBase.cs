using CSVToDBWithElasticIndexing;
using Microsoft.SqlServer.Server;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;


namespace CSVToDBWithElasticIndexing
{
    internal class DBase
    {

        public static bool OpenFile(string fileName)
        {
            AppResources.dBaseFileName = fileName;
            return ConnectDBase(AppResources.dBaseFileName);
        }
        public static bool CreateDBASE(string dBaseName)
        {
            if (!File.Exists(dBaseName))
            {
                SQLiteConnection.CreateFile(dBaseName);
                CreateNewDBase(dBaseName);
            }
            else
            {
                var dialogResult = Messages.overWriteExitedDBase(dBaseName);
                if (dialogResult == Messages.DialogResult.Yes)
                {
                    AppResources.dBaseConnection.Close();
                    File.Delete(dBaseName);
                    SQLiteConnection.CreateFile(dBaseName);
                    return CreateNewDBase(dBaseName);
                }
                else if (dialogResult == Messages.DialogResult.No)
                {
                    AppResources.dBaseConnection.Close();
                    return ConnectDBase(dBaseName);
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
                    commandLine.Append(Post.namesOfFields[i]);
                    /*switch (Post.typesOfFields[i])                                      // TO DO: создание полей в соответствии с опозныннми при парсинге типами
                    {
                        case GetType(String): commandLine.Append(" TEXT"); break;
                        case int: commandLine.Append(" INTEGER"); break;
                        case DateTime: commandLine.Append(" DATETIME"); break;
                        case int: commandLine.Append(" INTEGER"); break;
                        case double: commandLine.Append(" float"); break;
                        case decimal: commandLine.Append(" money"); break;
                        default: commandLine.Append(" TEXT"); break;
                    }*/
                    commandLine.Append(" TEXT");
                    if (i < Post.FieldsCount - 1) commandLine.Append(", ");
                }
                commandLine.Append(")");

                sQLCommand.CommandText = commandLine.ToString();
                sQLCommand.ExecuteNonQuery();
                commandLine.Clear();
                commandLine.Append($"CREATE UNIQUE INDEX record ON {Path.GetFileNameWithoutExtension(dBaseName)}(");
                for (int i = 0; i < Post.FieldsCount; i++)
                {
                    commandLine.Append(Post.namesOfFields[i]);
                    if (i < Post.FieldsCount - 1) commandLine.Append(", ");
                }
                commandLine.Append(")");
                sQLCommand.CommandText = commandLine.ToString();
                sQLCommand.ExecuteNonQuery();
                AppResources.dBaseConnection.Close();
                return true;
            }
            catch (SQLiteException ex)
            {
                Messages.ErrorMessage("Error: " + ex.Message);
                return false;
            }
        }

        public static bool ConnectDBase(string dBaseName)
        {
            try
            {
                var sQLCommand = new SQLiteCommand();
                AppResources.dBaseConnection = new SQLiteConnection("Data Source=" + dBaseName + ";Version=3;New=False;Compress=True;");
            }
            catch (SQLiteException ex)
            {
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
            var sQLCommand = new SQLiteCommand();
            sQLCommand.Connection = AppResources.dBaseConnection;
            var commandLine = new StringBuilder();
            try
            {
                commandLine.Append($"INSERT OR IGNORE INTO {Path.GetFileNameWithoutExtension(dBaseName)} (");
                for (int i = 0; i < Post.FieldsCount; i++)
                {
                    commandLine.Append($"'{Post.namesOfFields[i]}'");
                    if (i < Post.FieldsCount - 1) commandLine.Append(", ");
                }


                commandLine.Append(") Values (");
                for (int i = 0; i < Post.FieldsCount; i++)
                {
                    commandLine.Append($"@{Post.namesOfFields[i]}");
                    if (i < Post.FieldsCount - 1) commandLine.Append(", ");
                }
                commandLine.Append(")");
                for (int i = 0; i < Post.FieldsCount; i++)
                {
                    SQLiteParameter sqlParameter = new SQLiteParameter();
                    sqlParameter.ParameterName = $"@{Post.namesOfFields[i]}";
                    sqlParameter.Value = record.Fields[i];
                    sqlParameter.DbType = DbType.String;    //  TO DO: назначать в зависимости от типа данных поля
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
        public static List<Record> PrepareDataForIndexing()
        {
            SQLiteDataAdapter sQLiteDataAdapter = new SQLiteDataAdapter($"SELECT id, text FROM {Path.GetFileNameWithoutExtension(AppResources.dBaseFileName)}", 
                                                                        AppResources.dBaseConnection);
            DataSet dataSet = new DataSet();
            sQLiteDataAdapter.Fill(dataSet);
            DataTable dataTable = new DataTable();
            dataTable = dataSet.Tables[0];
            var postsTable = new List<Record>();
            foreach (DataRow row in dataTable.Rows)
            {
                postsTable.Add(new Record((long)row["id"], (string)row["text"]));
            }
            return postsTable;
        }
    }
}
