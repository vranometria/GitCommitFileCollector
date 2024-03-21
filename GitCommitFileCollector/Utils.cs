using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitCommitFileCollector
{
    public static class Utils
    {
        public static List<string> GetListFiles(Commit commit) => ListFiles(commit.Tree, "");

        private static List<string> ListFiles(Tree tree, string pathPrefix)
        {
            List<string> files = new();
            foreach (var entry in tree)
            {
                if (entry.TargetType == TreeEntryTargetType.Blob)
                {
                    files.Add($"{pathPrefix}{entry.Name}");
                }
                else if(entry.TargetType == TreeEntryTargetType.Tree)
                {
                    var t = ListFiles((Tree)entry.Target, $"{pathPrefix}{entry.Name}\\");
                    files.AddRange(t);
                }
            }
            return files;
        }


    }
}
