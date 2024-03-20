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
using LibGit2Sharp;
using Forms = System.Windows.Forms;
using GitCommitFileCollector.Views;

namespace GitCommitFileCollector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppDataManager AppDataManager { get; set; }

        private Repository? Repository { get; set; }

        public MainWindow()
        {
            AppDataManager = AppDataManager.Instance;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitTargeDirectory();

            ShowCommits();
        }

        /// <summary>
        /// 参照先ディレクトリを初期設定
        /// </summary>
        private void InitTargeDirectory()
        {
            while (AppDataManager.NotSelectedDirectory)
            {
                var fbd = new Forms.FolderBrowserDialog();
                if (fbd.ShowDialog() == Forms.DialogResult.OK)
                {
                    AppDataManager.SetTargetDirectory(fbd.SelectedPath);
                }
            }
            Repository = new Repository(AppDataManager.TargetDirectory);
        }

        /// <summary>
        /// コミット情報を表示
        /// </summary>
        private void ShowCommits()
        {
            if(Repository == null) { return; }
            Repository.Commits.Select(commit => new CommitView(commit)).ToList().ForEach( commitView => {
                CommitViewArea.Items.Add(commitView);
            });

        }
    }
}
