using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;

namespace GitCommitFileCollector
{
    public static class Utils
    {
        /// <summary>
        /// コミットに含まれるファイル一覧を取得
        /// </summary>
        /// <param name="commit"></param>
        /// <returns></returns>
        public static List<string> GetListFiles(Repository repository, Commit commit)
        {
            TreeChanges changes;
            if (commit.Parents.Count() > 0)
            {
                var parent = commit.Parents.First();
                changes = repository.Diff.Compare<TreeChanges>(parent.Tree, commit.Tree);
            }
            else
            {
                var emptyTree = repository.ObjectDatabase.CreateTree(new TreeDefinition());
                changes = repository.Diff.Compare<TreeChanges>(emptyTree, commit.Tree);
            }

            return changes.Select(c => c.Path).ToList();
        }

    }
}
