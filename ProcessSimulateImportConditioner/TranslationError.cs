using System;

namespace ProcessSimulateImportConditioner
{
    public class TranslationError
    {
        public DateTime Timestamp { get; set; }
        public string JTPath { get; set; }
        public string Description { get; set; }
    }
}
