using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVToDBWithElasticIndexing
{
    public class Post
    {
        private static int fieldsCount;
        private static List<int> fieldsToIndex;
        private List<object> fields;

        public static List<bool> FieldsToIndex;
        public static int FieldsCount;
        public static List<string> namesOfFields;
        public static List<Type> typesOfFields;
        public List<object> Fields { get; set; }

        public Post()
        {
            Fields = new List<object>();
        }
    }

    public class Record
    {
        public long Id { get; set; }
        public object Text { get; set; }
        public Record(long n, object text)
        {
            Id = n;
            Text = text;
        }
    }
}