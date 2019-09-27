using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackDot_ConsoleApp
{/// <summary>
/// Class to store the metadata for each search result
/// </summary>
    public class SearchResults
    {
        private string _heading;
        private string _price;
        private string _source;

        public string Heading { get; set; }
        public string Price { get; set; }
        public string Source { get; set; }
    }
}
