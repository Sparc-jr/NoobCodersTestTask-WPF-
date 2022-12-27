using System;
using System.Collections.Generic;

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
        public List<object> ItemsToIndex { get; set; }
        public Record(long n, List<object> items)
        {
            Id = n;
            ItemsToIndex = items;
        }
    }
}