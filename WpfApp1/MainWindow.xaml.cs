using CsvHelper;
using Microsoft.Win32;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Data;
using System.Data.SQLite;
using System.IO;
using CSVToDBWithElasticIndexing;
using Microsoft.SqlServer.Server;
using System.Drawing;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.SqlClient;

namespace CSVToDBWithElasticIndexing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        List<CheckBox> checkBoxColumnToIndex = new List<CheckBox>();
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
        }
        internal void ClearDataGridSearchResult()
        {
                    dataGridSearchResult.ItemsSource = null;
        }
        internal void RefreshDataGridView()
        {
            SQLiteDataAdapter sQLiteDataAdapter = new SQLiteDataAdapter($"SELECT * FROM {Path.GetFileNameWithoutExtension(AppResources.dBaseFileName)}", AppResources.dBaseConnection);
            DataSet dataSet = new DataSet();
            sQLiteDataAdapter.Fill(dataSet);
            DataGridSource.ItemsSource = dataSet.Tables[0].DefaultView;
        }
// на данный момент индексация осуществляется по умолчанию по первому столбцу           
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

        private void Window_Closed(object sender, EventArgs e)
        {
            AppResources.dBaseConnection.Close();
        }
    }
}
