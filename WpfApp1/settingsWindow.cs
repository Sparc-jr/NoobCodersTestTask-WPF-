using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Input;

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
            userNameTextBox.Width = 150;
            var passwordTextBox = new TextBox();
            passwordTextBox.Width = 150;
            var resultsCountTextBox = new TextBox();
            resultsCountTextBox.Width = 50;
            var stackPanel = new StackPanel { Orientation = Orientation.Vertical };
            var cIDBinding = new Binding();
            var nameBinding = new Binding();
            var passBinding = new Binding();
            var resultsBinding = new Binding();
            var saveButton = new Button();
            var closeButton = new Button();
            cIDBinding.Source = resources;
            cIDBinding.Path = new PropertyPath("elasticCloudID");
            cIDBinding.Mode = BindingMode.TwoWay;
            cIDBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
            cloudIDTextBox.SetBinding(TextBox.TextProperty, cIDBinding);
            nameBinding.Source = resources;
            nameBinding.Path = new PropertyPath("elasticUserName");
            nameBinding.Mode = BindingMode.TwoWay;
            nameBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
            userNameTextBox.SetBinding(TextBox.TextProperty, nameBinding);
            passBinding.Source = resources;
            passBinding.Path = new PropertyPath("elasticPassword");
            passBinding.Mode = BindingMode.TwoWay;
            passBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
            passwordTextBox.SetBinding(TextBox.TextProperty, passBinding);
            resultsBinding.Source = resources;
            resultsBinding.Path = new PropertyPath("searchResultsCount");
            resultsBinding.Mode = BindingMode.TwoWay;
            resultsBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
            resultsCountTextBox.SetBinding(TextBox.TextProperty, resultsBinding);
            var cIDLabel = new Label();
            var userNameLabel = new Label();
            userNameLabel.Width = 150;
            var passwordLabel = new Label();
            passwordLabel.Width = 150;
            var resultsLabel = new Label();
            cIDLabel.Content = $"Настройки авторизации в Elastic\nCloudID:";
            userNameLabel.Content = "имя пользователя:";
            passwordLabel.Content = "пароль:";
            resultsLabel.Content = "Отображаемое количество результатов поиска:";
            saveButton.Content = "Сохранить";
            closeButton.Content = "Закрыть";
            stackPanel.Children.Add(cIDLabel);
            stackPanel.Children.Add(cloudIDTextBox);
            stackPanel.Children.Add(userNameLabel);
            stackPanel.Children.Add(userNameTextBox);
            stackPanel.Children.Add(passwordLabel);
            stackPanel.Children.Add(passwordTextBox);
            stackPanel.Children.Add(resultsLabel);
            stackPanel.Children.Add(resultsCountTextBox);
            stackPanel.Children.Add(saveButton);
            stackPanel.Children.Add(closeButton);
            settings.Content = stackPanel;
            resultsCountTextBox.PreviewTextInput += (s, args) =>
            {
                NumberValidationTextBox(s, args);

            };
            closeButton.Click += (s, args) =>
            {
                settings.Close();
            };
            saveButton.Click += (s, args) =>
            {
                cloudIDTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                userNameTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                passwordTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                resultsCountTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                Settings.SaveSettingsToIni();
            };
            settings.ShowDialog();
        }

        private static void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            
            Regex regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
