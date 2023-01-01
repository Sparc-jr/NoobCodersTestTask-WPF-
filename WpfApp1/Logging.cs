using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CSVToDBWithElasticIndexing
{
    internal class Logging
    {
        internal static void Write(string logstring)
        {
            File.AppendAllText("error.log", $"[{DateTime.Now}]: {logstring}\n");
        }

    }
}
