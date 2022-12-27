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
//using System.Windows.Shapes;
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
            if (openFileDialog.ShowDialog() == false )
                return;
            if (CSVReader.OpenFile(openFileDialog.FileName))
            {
                Label1.Content = "Connected";
                RefreshDataGridView();
            }


        }
        private void Button_OpenDB_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Database files(*.db)|*.db";
            if (openFileDialog.ShowDialog() == false)
                return;

            Label1.Content = DBase.OpenFile(openFileDialog.FileName) ? "Connected" : "Disconnected";
            RefreshDataGridView();
            
            
            MessageBox.Show("Файл открыт");

        }

        /*private List<Post> PrepareDataForIndexing()
        {
            var postsTable = new List<Post>();
            for (int i = 0; i < DataGridSource.Items.Count - 1; i++)
            {
                postsTable.Add(new Post());
                for (int j = 0; j < DataGridSource.    [i].Cells.Count - 1; j++)
                {
                    postsTable[i].Fields.Add(DataGridSource.Rows[i].Cells[j].Value);
                }
            }

            return postsTable;
        }*/

        internal void RefreshDataGridView()
        {
            SQLiteDataAdapter sQLiteDataAdapter = new SQLiteDataAdapter($"SELECT * FROM {Path.GetFileNameWithoutExtension(AppResources.dBaseFileName)}", AppResources.dBaseConnection);
            DataSet dataSet = new DataSet();
            sQLiteDataAdapter.Fill(dataSet);
            DataGridSource.ItemsSource = dataSet.Tables[0].DefaultView;
            //DrawCheckBoxesInColumns(); //отрисовка чекбоксов в заголовках колонок
        }
        private void DrawCheckBoxesInColumns()       // добавляем чекбоксы в заголовки столбцов для выбора что индексировать
        {                                            // на данный момент индексация осуществляется по умолчанию по первому столбцу           
            foreach (CheckBox chkBox in checkBoxColumnToIndex)
            {
                Canvas.Children.Remove(chkBox);
            }
            checkBoxColumnToIndex.Clear();
            for (int i = 0; i < Post.FieldsCount; i++)
            {
                var ckBox = new CheckBox();
                checkBoxColumnToIndex.Add(ckBox);
                var cellContent = (FrameworkElement)DataGridSource.Columns[i].GetCellContent(DataGridSource.Items[1]);
                //DataGridSource.RowHeaderTemplate = 

                //Rectangle rect = this.DataGridSource.   GetCellDisplayRectangle(i + 1, -1, true);
                //checkBoxColumnToIndex[i].Size = new Size(18, 18);
                //checkBoxColumnToIndex[i].Top = rect.Top + 1;
                //checkBoxColumnToIndex[i].Left = rect.Left + rect.Width - checkBoxColumnToIndex[i].Width - 1;
                /*checkBoxColumnToIndex[i].CheckedChanged += (sender, eventArgs) => {
                    CheckBox senderCheckbox = (CheckBox)sender;
                    ckBox_CheckedChanged(i);
                };*/
                cellContent = checkBoxColumnToIndex[i];
            //this.dataGridView1.Controls.Add(checkBoxColumnToIndex[i]);
            }
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
            /*var idsSelected = new int[selected.Length];
            for (int i = 0; i< selected.Length;i++)
            {
                idsSelected[i] = (int)(selected[i].Id);
            }*/
            DBase.DeleteDBaseRow(selected);
            ElasticsearchHelper.DeleteDocument(AppResources.elasticSearchClient, AppResources.indexName, selected);
            RefreshDataGridView();
            searchButton_Click(sender, e);            
        }
    }
}
