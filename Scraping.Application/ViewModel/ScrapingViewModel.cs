using System;
using System.Collections.Generic;
using System.Text;

namespace Scraping.Application.ViewModel
{
    public class ScrapingViewModel
    {
        public int TotalRows { get; set; }
        public int TotalFiles { get; set; }
        public decimal TotalBytes { get; set; }
        public string Extension { get; set; }
    }
}
