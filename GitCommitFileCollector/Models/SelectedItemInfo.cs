using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitCommitFileCollector.Models
{
    /// <summary>
    /// 収集対象情報クラス
    /// </summary>
    public class SelectedItemInfo
   {

        public Dictionary<string, SelectedItemGroup> Commits { get; set; } = new Dictionary<string, SelectedItemGroup>();
    }
}
