using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessSimulateImportConditioner
{
    public class TranslationError
    {
        public DateTime Timestamp { get; set; }
        public string JTPath { get; set; }
        public string Description { get; set; }
    }
}
