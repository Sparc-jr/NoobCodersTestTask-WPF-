using System;
using System.Collections.Generic;

namespace CSVToDBWithElasticIndexing
{
    public class Post
    {
        private static int fieldsCount;
        private static List<FieldsToIndexSelection> fieldsToIndex;
        private static List<Type> typesOfFields;
        private List<object> fields;
        public static int FieldsCount { get; set; }
        public static List<FieldsToIndexSelection> FieldsToIndex { get; set; }
        public static List<Type> TypesOfFields { get; set; }
        public List<object> Fields { get; set; }

        public Post()
        {
            Fields = new List<object>();
        }

        public static void Clear()
        {
            FieldsCount = 0;
            FieldsToIndex.Clear();
            TypesOfFields.Clear();
        }
    }

    public class FieldsToIndexSelection
    {
        public FieldsToIndexSelection(bool isChecked, string name)
        {
            this.isChecked = isChecked;
            this.name = name;
        }

        public Boolean isChecked { get; set; }
        public String name { get; set; }
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