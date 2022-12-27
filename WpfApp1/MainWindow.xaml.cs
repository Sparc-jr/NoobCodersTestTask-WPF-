using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
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
            AppResources.dBaseConnection = new SQLiteConnection();
            AppResources.dBaseFileName = "sampleDB.db";
            Label1.Content = "Disconnected";
            ElasticsearchHelper.GetESClient();
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
            //var checkedColumnsList = new List<CheckedColumn>();
            //foreach (var field in Post.FieldsToIndex)
            //checkedColumnsList.Add(new (field.isChecked, field.name));
            //HeadersComboBox.ItemsSource = checkedColumnsList;
            HeadersComboBox.ItemsSource = Post.FieldsToIndex.Where(x => x.name.ToLower() != "id"); 
            //was HeadersComboBox.ItemsSource = DataGridSource.Columns.Where(x => x.Header.ToString().ToLower() != "id").Select(x => x.Header);
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
            {
                var searchResult = ElasticsearchHelper.SearchDocument(AppResources.elasticSearchClient, AppResources.indexName, textBox1.Text);
                dataGridSearchResult.ItemsSource = searchResult;
            }
        }
        private void textBox1_Loaded(object sender, RoutedEventArgs e)
        {
            textBox1.SelectAll();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var selected = dataGridSearchResult.SelectedItems
                .OfType<Record>().Select(x => x.Id)
                .ToArray();
            ElasticsearchHelper.DeleteDocument(AppResources.elasticSearchClient, AppResources.indexName, selected);
            DBase.DeleteDBaseRow(selected);
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
            comboBox.SelectedItem = null;
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            AppResources.dBaseConnection.Close();
        }
    }
}
