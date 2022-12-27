using System;

namespace CSVToDBWithElasticIndexing
{
    internal class TypesResponser
    {
        public static Type GetObjectType(Object item)
        {
            var element = (string)item;
            if (Int64.TryParse(element, out _))
            {
                return typeof(long);
            }
            else if (double.TryParse(element, out _))
            {
                return typeof(double);
            }
            else if (DateTime.TryParse(element, out _))
            {
                return typeof(DateTime);
            }
            return typeof(string);
        }
        public static Type GetDBaseColumnType(string item)
        {
            switch (item)
            {
                case "INTEGER": return typeof(long);
                case "float": return typeof(double);
                case "DATETIME": return typeof(DateTime);
                default: return typeof(string);
            }
        }
    }
}
