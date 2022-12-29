using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;

namespace CSVToDBWithElasticIndexing
{
    internal class settingsWindow
    {
        internal static void ShowSettings()
        {
            var resources = new AppResources();
            var settings = new Window();
            settings.Title = "settings";
            settings.Width = 350;
            settings.Height = 250;
            settings.ResizeMode = ResizeMode.NoResize;
            settings.WindowStyle = WindowStyle.None;
            var cloudIDTextBox = new TextBox();
            var userNameTextBox = new TextBox();
            var passwordTextBox = new TextBox();
            var resultsCountTextBox = new TextBox();
            var stackPanel = new StackPanel { Orientation = Orientation.Vertical };
            var cIDBinding = new Binding();
            var nameBinding = new Binding();
            var passBinding = new Binding();
            var resultsBinding = new Binding();
            var saveButton = new Button();
            var cancelButton = new Button();
            cIDBinding.Source = resources;
            cIDBinding.Path = new PropertyPath("elasticCloudID");
            cloudIDTextBox.SetBinding(TextBox.TextProperty, cIDBinding);
            nameBinding.Source = resources;
            nameBinding.Path = new PropertyPath("elasticUserName");
            userNameTextBox.SetBinding(TextBox.TextProperty, nameBinding);
            passBinding.Source = resources;
            passBinding.Path = new PropertyPath("elasticPassword");
            passwordTextBox.SetBinding(TextBox.TextProperty, passBinding);
            resultsBinding.Source = resources;
            resultsBinding.Path = new PropertyPath("elasticPassword");
            resultsCountTextBox.SetBinding(TextBox.TextProperty, resultsBinding);
            var cIDLabel = new Label();
            var userNameLabel = new Label();
            var passwordLabel = new Label();
            var resultsLabel = new Label();
            cIDLabel.Content = $"Настройки авторизации в Elastic\nCloudID:";
            userNameLabel.Content = "имя пользователя:";
            passwordLabel.Content = "пароль:";
            resultsLabel.Content = "Отображаемое количество результатов поиска:";
            saveButton.Content = "Сохранить";
            cancelButton.Content = "Отменить";
            stackPanel.Children.Add(cIDLabel);
            stackPanel.Children.Add(cloudIDTextBox);
            stackPanel.Children.Add(userNameLabel);
            stackPanel.Children.Add(userNameTextBox);
            stackPanel.Children.Add(passwordLabel);
            stackPanel.Children.Add(passwordTextBox);
            stackPanel.Children.Add(resultsLabel);
            stackPanel.Children.Add(resultsCountTextBox);
            stackPanel.Children.Add(saveButton);
            stackPanel.Children.Add(cancelButton);
            settings.Content = stackPanel;
            settings.ShowDialog();


        }
    }
}
