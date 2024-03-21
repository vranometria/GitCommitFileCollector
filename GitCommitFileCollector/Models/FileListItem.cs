using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitCommitFileCollector.Models
{
    public class FileListItem
    {
        public string FilePath { get; set; } = string.Empty;

        public bool IsChecked { get; set; }

        public string Sha { get; set; } = string.Empty; 

        public DateTimeOffset DateTimeOffset { get; set; }
    }
}
