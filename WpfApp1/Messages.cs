using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

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
                
        public static DialogResult overWriteExitedDBase(string dBaseName)
        {
            MessageBoxResult result = MessageBox.Show("ДА - создать базу заново\n Нет - добавить записи в существующую базу" +
                                                "\n Отмена - отменить открытие файла",
                                                $"База c именем {Path.GetFileNameWithoutExtension(dBaseName)} уже существует, заменить?"
                                                , MessageBoxButton.YesNoCancel);
            switch(result)
            {
                case MessageBoxResult.Yes: return DialogResult.Yes;
                case MessageBoxResult.No: return DialogResult.No;
                default: return DialogResult.Cancel;
            }
        }

        public static void ErrorMessage (string errorMessage)
        {
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        public static void InfoMessage(string infoMessage)
        {
            MessageBox.Show(infoMessage,"", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
