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
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Settings.ReadConfigIni();
            AppResources.DBaseConnection = new SQLiteConnection();
            AppResources.ElasticSearchClient = ElasticsearchHelper.GetESClient();
            RefreshStatusStringLabels();
            Post.TypesOfFields = new List<Type>();
            Post.FieldsToIndex = new List<FieldsToIndexSelection>();
        }

       private void RefreshStatusStringLabels()
        {
            DBStatusLabel.Content = $"DB {AppResources.DBaseConnection.State}";
            var response = AppResources.ElasticSearchClient.Cluster.Health(new ClusterHealthRequest() { WaitForStatus = WaitForStatus.Red });
            ElasticStatusLabel.Content = $"Elastic status {response.Status}";
        }

        private void ButtonOpenCSV_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files(*.csv)|*.csv"
            };
            if (openFileDialog.ShowDialog() == false)
                return;
            if (CSVReader.OpenFile(openFileDialog.FileName))
            {
                RefreshDataGridView();
            }
            ClearDataGridSearchResult();
            RefreshComboBox();
            RefreshStatusStringLabels();
        }

        private void ButtonOpenDB_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Database files(*.db)|*.db"
            };
            if (openFileDialog.ShowDialog() == false)
                return;
            DBase.OpenFile(openFileDialog.FileName);
            RefreshDataGridView();
            ClearDataGridSearchResult();
            RefreshComboBox();
            RefreshStatusStringLabels();
        }
        internal void ClearDataGridSearchResult()
        {
            dataGridSearchResult.ItemsSource = null;
        }
        private void RefreshComboBox()
        {
            var exludeFields = new List<string>();
            for (int i = 0; i < Post.FieldsCount; i++)
            {
                if (Post.TypesOfFields[i] == typeof(DateTime)) exludeFields.Add(Post.FieldsToIndex[i].name);
            } 
            // removes id and datetime columns from selection of indexing columns
            HeadersComboBox.ItemsSource = Post.FieldsToIndex.Where(x => (x.name.ToLower() != "id" && !exludeFields.Contains(x.name))); 
        }
        internal void RefreshDataGridView()
        {
            var sQLiteDataAdapter = new SQLiteDataAdapter($"SELECT * FROM {Path.GetFileNameWithoutExtension(AppResources.DBaseFileName)}", AppResources.DBaseConnection);
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
                var searchResult = ElasticsearchHelper.SearchDocument(AppResources.ElasticSearchClient, AppResources.IndexName, textBox1.Text.ToLower());
                var dataSet = DBase.GetSearchResultsTable(searchResult);
                dataGridSearchResult.ItemsSource = dataSet.Tables[0].DefaultView;
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var selectedRecords = dataGridSearchResult.SelectedItems
                .OfType<DataRowView>().Select(x => (long)x.Row[0])
                .ToArray();
            ElasticsearchHelper.DeleteDocument(AppResources.ElasticSearchClient, AppResources.IndexName, selectedRecords);
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

        private void IndexingButton_Click(object sender, RoutedEventArgs e)
        {
            if (AppResources.ElasticSearchClient == null)
            {
                Messages.ErrorMessage("Нет подключения к Elastic Cloud, проверьте настройки!");
            }
            else
            {
                ElasticsearchHelper.CreateDocument(AppResources.ElasticSearchClient, AppResources.IndexName);
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
        
        private void Window_Closed(object sender, EventArgs e)
        {
            AppResources.DBaseConnection.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Application.Current.Shutdown();
            Logging.Write("app closed normally");
            return;
        }
    }
}
