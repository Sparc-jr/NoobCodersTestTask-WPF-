using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
            var cIDBinding = new Binding();
            cIDBinding.Source = resources;
            cIDBinding.Path = new PropertyPath("ElasticCloudID");
            cIDBinding.Mode = BindingMode.TwoWay;
            cIDBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
            cloudIDTextBox.SetBinding(TextBox.TextProperty, cIDBinding);
            var cIDLabel = new Label();
            cIDLabel.Content = $"Настройки авторизации в Elastic\nCloudID:";
            stackPanel.Children.Add(cIDLabel);
            stackPanel.Children.Add(cloudIDTextBox);
            return cloudIDTextBox;
        }

        private static TextBox SetUserNameControls(StackPanel stackPanel, AppResources resources)
        {
            var userNameTextBox = new TextBox();
            userNameTextBox.Width = 150;
            var nameBinding = new Binding();
            nameBinding.Source = resources;
            nameBinding.Path = new PropertyPath("ElasticUserName");
            nameBinding.Mode = BindingMode.TwoWay;
            nameBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
            userNameTextBox.SetBinding(TextBox.TextProperty, nameBinding);
            var userNameLabel = new Label();
            userNameLabel.Width = 150;
            userNameLabel.Content = "имя пользователя:";
            stackPanel.Children.Add(userNameLabel);
            stackPanel.Children.Add(userNameTextBox);
            return userNameTextBox;
        }
        private static TextBox SetPasswordControls(StackPanel stackPanel, AppResources resources)
        {
            var passwordTextBox = new TextBox();
            passwordTextBox.Width = 150;
            var passBinding = new Binding();
            passBinding.Source = resources;
            passBinding.Path = new PropertyPath("ElasticPassword");
            passBinding.Mode = BindingMode.TwoWay;
            passBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
            passwordTextBox.SetBinding(TextBox.TextProperty, passBinding);
            var passwordLabel = new Label();
            passwordLabel.Width = 150;
            passwordLabel.Content = "пароль:";
            stackPanel.Children.Add(passwordLabel);
            stackPanel.Children.Add(passwordTextBox);
            return passwordTextBox;
        }
        private static TextBox SetResultsCountControls(StackPanel stackPanel, AppResources resources)
        {
            var resultsCountTextBox = new TextBox();
            resultsCountTextBox.Width = 50;
            var resultsBinding = new Binding();
            resultsBinding.Source = resources;
            resultsBinding.Path = new PropertyPath("SearchResultsCount");
            resultsBinding.Mode = BindingMode.TwoWay;
            resultsBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
            resultsCountTextBox.SetBinding(TextBox.TextProperty, resultsBinding);
            var resultsLabel = new Label();
            resultsLabel.Content = "Отображаемое количество результатов поиска:";
            stackPanel.Children.Add(resultsLabel);
            stackPanel.Children.Add(resultsCountTextBox);
            return resultsCountTextBox;
        }
        private static void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
