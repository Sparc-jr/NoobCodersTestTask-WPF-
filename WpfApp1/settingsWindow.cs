using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace CSVToDBWithElasticIndexing
{
    internal class SettingsWindow
    {
        internal static void ShowSettings()
        {
            var resources = new AppResources();
            var settings = new Window
            {
                Title = "settings",
                Width = 350,
                Height = 250,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.None
            };
            var stackPanel = new StackPanel { Orientation = Orientation.Vertical };
            var cloudIDTextBox = SetCloudIDControls(stackPanel, resources);
            var userNameTextBox = SetUserNameControls(stackPanel, resources);
            var passwordTextBox = SetPasswordControls(stackPanel, resources);
            var resultsCountTextBox = SetResultsCountControls(stackPanel, resources);
            var saveButton = new Button();
            var closeButton = new Button();
            saveButton.Content = "Сохранить";
            closeButton.Content = "Закрыть";
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
                ElasticsearchHelper.GetESClient();
            };
            settings.ShowDialog();
        }

        private static TextBox SetCloudIDControls(StackPanel stackPanel, AppResources resources)
        {
            var cloudIDTextBox = new TextBox();
            var cIDBinding = new Binding
            {
                Source = resources,
                Path = new PropertyPath("ElasticCloudID"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            cloudIDTextBox.SetBinding(TextBox.TextProperty, cIDBinding);
            var cIDLabel = new Label
            {
                Content = $"Настройки авторизации в Elastic.\nCloudID:"
            };
            stackPanel.Children.Add(cIDLabel);
            stackPanel.Children.Add(cloudIDTextBox);
            return cloudIDTextBox;
        }

        private static TextBox SetUserNameControls(StackPanel stackPanel, AppResources resources)
        {
            var userNameTextBox = new TextBox
            {
                Width = 150
            };
            var nameBinding = new Binding
            {
                Source = resources,
                Path = new PropertyPath("ElasticUserName"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            userNameTextBox.SetBinding(TextBox.TextProperty, nameBinding);
            var userNameLabel = new Label
            {
                Width = 150,
                Content = "имя пользователя:"
            };
            stackPanel.Children.Add(userNameLabel);
            stackPanel.Children.Add(userNameTextBox);
            return userNameTextBox;
        }
        private static TextBox SetPasswordControls(StackPanel stackPanel, AppResources resources)
        {
            var passwordTextBox = new TextBox
            {
                Width = 150
            };
            var passBinding = new Binding
            {
                Source = resources,
                Path = new PropertyPath("ElasticPassword"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            passwordTextBox.SetBinding(TextBox.TextProperty, passBinding);
            var passwordLabel = new Label
            {
                Width = 150,
                Content = "пароль:"
            };
            stackPanel.Children.Add(passwordLabel);
            stackPanel.Children.Add(passwordTextBox);
            return passwordTextBox;
        }
        private static TextBox SetResultsCountControls(StackPanel stackPanel, AppResources resources)
        {
            var resultsCountTextBox = new TextBox
            {
                Width = 50
            };
            var resultsBinding = new Binding
            {
                Source = resources,
                Path = new PropertyPath("SearchResultsCount"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            resultsCountTextBox.SetBinding(TextBox.TextProperty, resultsBinding);
            var resultsLabel = new Label
            {
                Content = "Отображаемое количество результатов поиска:"
            };
            stackPanel.Children.Add(resultsLabel);
            stackPanel.Children.Add(resultsCountTextBox);
            return resultsCountTextBox;
        }
        private static void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
