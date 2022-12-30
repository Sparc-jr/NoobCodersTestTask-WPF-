using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CSVToDBWithElasticIndexing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            var resources = new AppResources();
            var buttonEnabledBinding = new Binding();

            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Settings.auto_read();
            AppResources.tableIsIndexed = false;
            AppResources.dBaseConnection = new SQLiteConnection();
            AppResources.dBaseFileName = "sampleDB.db";            
            Label1.Content = "Disconnected";
            AppResources.elasticSearchClient = ElasticsearchHelper.GetESClient();
        }

        private void Button_OpenCSV_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV files(*.csv)|*.csv";
            if (openFileDialog.ShowDialog() == false)
                return;
            if (CSVReader.OpenFile(openFileDialog.FileName))
            {
                Label1.Content = "Connected";
                RefreshDataGridView();
            }
            ClearDataGridSearchResult();
            RefreshComboBox();
        }

        private void Button_OpenDB_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Database files(*.db)|*.db";
            if (openFileDialog.ShowDialog() == false)
                return;
            Label1.Content = DBase.OpenFile(openFileDialog.FileName) ? "Connected" : "Disconnected";
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
                this.isChecked = isChecked;
                this.name = name;
            }

            public Boolean isChecked { get; set; }
            public String name { get; set; }
        }



        internal void RefreshDataGridView()
        {
            SQLiteDataAdapter sQLiteDataAdapter = new SQLiteDataAdapter($"SELECT * FROM {Path.GetFileNameWithoutExtension(AppResources.dBaseFileName)}", AppResources.dBaseConnection);
            DataSet dataSet = new DataSet();
            sQLiteDataAdapter.Fill(dataSet);
            DataGridSource.ItemsSource = dataSet.Tables[0].DefaultView;
        }
        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            if (AppResources.tableIsIndexed == false)
            {
                Messages.InfoMessage("Сначала проиндексируйте таблицу");
            }
            else                    
            {
                var searchResult = ElasticsearchHelper.SearchDocument(AppResources.elasticSearchClient, AppResources.indexName, textBox1.Text);
                DataSet dataSet = new DataSet();
                dataSet.Tables.Add(new DataTable());
                dataSet.Tables[0].Columns.Add("id");
                for (int i = 1; i < Post.FieldsCount; i++)
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
                        row[dataSet.Tables[0].Columns[i + 1]] = result.ItemsToIndex[i];
                    }
                    row.EndEdit();
                    dataSet.Tables[0].Rows.Add(row);
                }
                dataGridSearchResult.ItemsSource = dataSet.Tables[0].DefaultView;
            }
        }
        private void textBox1_Loaded(object sender, RoutedEventArgs e)
        {
            textBox1.SelectAll();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var selectedRecords = dataGridSearchResult.SelectedItems
                .OfType<DataRowView>().Select(x => Int64.Parse((string)x.Row[0]))
                .ToArray();
            ElasticsearchHelper.DeleteDocument(AppResources.elasticSearchClient, AppResources.indexName, selectedRecords);
            DBase.DeleteDBaseRow(selectedRecords);
            RefreshDataGridView();
            searchButton_Click(sender, e);            
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
            comboBox.SelectedItem = "Выберите";
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            AppResources.dBaseConnection.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void IndexingButton_Click(object sender, RoutedEventArgs e)
        {
            ElasticsearchHelper.CreateDocument(AppResources.elasticSearchClient, AppResources.indexName, ElasticsearchHelper.PrepareDataForIndexing());
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            settingsWindow.ShowSettings();
        }
    }
}
