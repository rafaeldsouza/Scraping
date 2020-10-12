using System;
using System.Collections.Generic;
using System.Text;

namespace Scraping.Core.Entity
{
    public class GitFile
    {
        public int TotalRows { get; set; }
        public decimal TotalBytes { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Extension { get; set; }
    }
}
