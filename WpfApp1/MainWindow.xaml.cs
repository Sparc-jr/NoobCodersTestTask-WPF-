using Elasticsearch.Net;
using Microsoft.Win32;
using Nest;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CSVToDBWithElasticIndexing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Settings.ReadConfigIni();
            AppResources.TableIsIndexed = false;
            AppResources.dBaseConnection = new SQLiteConnection();
            AppResources.dBaseFileName = "sampleDB.db";
            DBStatusLabel.Content = "DB Disconnected";
            AppResources.ElasticSearchClient = ElasticsearchHelper.GetESClient();
            var response = AppResources.ElasticSearchClient.ClusterHealth(new ClusterHealthRequest() { WaitForStatus = WaitForStatus.Red });
            ElasticStatusLabel.Content = $"Elastic status {response.Status}";
            Post.TypesOfFields = new List<Type>();
            Post.FieldsToIndex = new List<FieldsToIndexSelection>();
        }

        private void Button_OpenCSV_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files(*.csv)|*.csv"
            };
            if (openFileDialog.ShowDialog() == false)
                return;
            if (CSVReader.OpenFile(openFileDialog.FileName))
            {
                DBStatusLabel.Content = "DB Connected";
                RefreshDataGridView();
            }
            ClearDataGridSearchResult();
            RefreshComboBox();
        }

        private void Button_OpenDB_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Database files(*.db)|*.db"
            };
            if (openFileDialog.ShowDialog() == false)
                return;
            DBStatusLabel.Content = DBase.OpenFile(openFileDialog.FileName) ? "DB Connected" : "DB Disconnected";
            RefreshDataGridView();
            ClearDataGridSearchResult();
            RefreshComboBox();
        }
        internal void ClearDataGridSearchResult()
        {
            dataGridSearchResult.ItemsSource = null;
        }
        private void RefreshComboBox()
        {
            HeadersComboBox.ItemsSource = Post.FieldsToIndex.Where(x => x.name.ToLower() != "id");
        }

        public class CheckedColumn
        {
            public CheckedColumn(bool isChecked, string name)
            {
                this.IsChecked = isChecked;
                this.Name = name;
            }

            public Boolean IsChecked { get; set; }
            public String Name { get; set; }
        }



        internal void RefreshDataGridView()
        {
            var sQLiteDataAdapter = new SQLiteDataAdapter($"SELECT * FROM {Path.GetFileNameWithoutExtension(AppResources.dBaseFileName)}", AppResources.dBaseConnection);
            var dataSet = new DataSet();
            sQLiteDataAdapter.Fill(dataSet);
            DataGridSource.ItemsSource = dataSet.Tables[0].DefaultView;
        }
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (AppResources.TableIsIndexed == false)
            {
                Messages.InfoMessage("Сначала проиндексируйте таблицу");
            }
            else
            {
                var searchResult = ElasticsearchHelper.SearchDocument(AppResources.ElasticSearchClient, AppResources.indexName, textBox1.Text.ToLower());
                var dataSet = new DataSet();
                dataSet.Tables.Add(new DataTable());
                dataSet.Tables[0].Columns.Add("id");
                for (int i = 0; i < Post.FieldsCount; i++)
                {
                    if (!Post.FieldsToIndex[i].isChecked)
                    {
                        continue;
                    }
                    dataSet.Tables[0].Columns.Add(Post.FieldsToIndex[i].name);
                }
                var columnsNumber = 0;
                if (searchResult.Count > 0)
                {
                    columnsNumber = searchResult[0].ItemsToIndex.Count;
                }
                foreach (var result in searchResult)
                {
                    DataRow row = dataSet.Tables[0].NewRow();
                    row.BeginEdit();
                    row["id"] = result.Id;
                    for (int i = 0; i < columnsNumber; i++)
                    {
                        row[dataSet.Tables[0].Columns[i+1]] = result.ItemsToIndex[i];
                    }
                    row.EndEdit();
                    dataSet.Tables[0].Rows.Add(row);
                }
                dataGridSearchResult.ItemsSource = dataSet.Tables[0].DefaultView;
            }
        }
        private void TextBox1_Loaded(object sender, RoutedEventArgs e)
        {
            textBox1.SelectAll();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var selectedRecords = dataGridSearchResult.SelectedItems
                .OfType<DataRowView>().Select(x => Int64.Parse((string)x.Row[0]))
                .ToArray();
            ElasticsearchHelper.DeleteDocument(AppResources.ElasticSearchClient, AppResources.indexName, selectedRecords);
            DBase.DeleteDBaseRow(selectedRecords);
            RefreshDataGridView();
            SearchButton_Click(sender, e);
        }

        private void OnComboBoxCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            var selectedFields = new List<string>();
            foreach (var item in HeadersComboBox.Items)
            {
                if (HeadersComboBox.SelectedItem == item) selectedFields.Add(item as string);
            }
        }

        private void OnComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            comboBox.SelectedItem = null;
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            AppResources.dBaseConnection.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Application.Current.Shutdown();
            Logging.Write("app closed");
            return;
        }

        private void IndexingButton_Click(object sender, RoutedEventArgs e)
        {
            if (AppResources.ElasticSearchClient == null)
            {
                Messages.ErrorMessage("Нет подключения к Elastic Cloud, проверьте настройки!");
            }
            else
            {
                ElasticsearchHelper.CreateDocument(AppResources.ElasticSearchClient, AppResources.indexName);
            }

        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow.ShowSettings();
        }

        private void StrongSearchCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ElasticsearchHelper.textQueryType = TextQueryType.Phrase;
        }

        private void StrongSearchCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ElasticsearchHelper.textQueryType = TextQueryType.PhrasePrefix;
        }
    }
}
