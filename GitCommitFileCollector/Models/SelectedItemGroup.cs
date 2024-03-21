using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitCommitFileCollector.Models
{
    /// <summary>
    /// 1コミット内の選択したファイル
    /// </summary>
    public class SelectedItemGroup
    {
        public List<string> FilePaths { get; set; } = new List<string>();

        public DateTimeOffset DateTimeOffset { get; set; }

    }
}
