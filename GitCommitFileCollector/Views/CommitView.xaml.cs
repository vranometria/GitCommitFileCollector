using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GitCommitFileCollector.Views
{
    /// <summary>
    /// CommitView.xaml の相互作用ロジック
    /// </summary>
    public partial class CommitView : UserControl
    {
        private Commit? Commit { get; set; }

        public CommitView()
        {
            InitializeComponent();
        }

        public CommitView(Commit commit) : this() 
        {
            Commit = commit;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if(Commit == null) { return; }
            ShaLabel.Content = Commit.Sha;
            CommitCommentLabel.Content = Commit.Message;
            DateLabel.Content = Commit.Committer.When.ToString("yyyy/M/d");
            AuthorLabel.Content = Commit.Author;
        }
    }
}
