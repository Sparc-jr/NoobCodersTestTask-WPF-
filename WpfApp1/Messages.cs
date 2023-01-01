using System.IO;
using System.Windows;

namespace CSVToDBWithElasticIndexing
{
    internal class Messages
    {
        public enum DialogResult
        {
            Yes,
            No,
            OK,
            Cancel,
        }

        public static DialogResult OverWriteExistedDBase(string dBaseName)
        {
            MessageBoxResult result = MessageBox.Show("ДА - создать базу заново\n Нет - добавить записи в существующую базу" +
                                                "\n Отмена - отменить открытие файла",
                                                $"База c именем {Path.GetFileNameWithoutExtension(dBaseName)} уже существует, заменить?"
                                                , MessageBoxButton.YesNoCancel);
            return result switch
            {
                MessageBoxResult.Yes => DialogResult.Yes,
                MessageBoxResult.No => DialogResult.No,
                _ => DialogResult.Cancel,
            };
        }

        public static void ErrorMessage(string errorMessage)
        {
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        public static void InfoMessage(string infoMessage)
        {
            MessageBox.Show(infoMessage, "", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
