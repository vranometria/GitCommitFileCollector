using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using LibGit2Sharp;
using Forms = System.Windows.Forms;
using GitCommitFileCollector.Views;
using GitCommitFileCollector.Models;
using System.Diagnostics;

namespace GitCommitFileCollector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppDataManager AppDataManager { get; set; }

        private Repository? Repository { get; set; }

        private Commit? CurrentClickedCommit { get; set; } 

        /// <summary>
        /// 抽出対象として選択したファイルのコミット単位のグループ
        /// </summary>
        public Dictionary<string, ExtractFileGroup> ExtractFileGroups { get; set; } = new Dictionary<string, ExtractFileGroup>();

        public MainWindow()
        {
            AppDataManager = AppDataManager.Instance;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitTargeDirectory();

            ShowAllCommits();
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
        /// コミット情報を全て列挙する
        /// </summary>
        private void ShowAllCommits()
        {
            if(Repository == null) { return; }

            Commands.Checkout(Repository, Repository.Head.Tip);
            CommitViewArea.Items.Clear();
            Repository.Commits.Select(commit => new CommitView(commit)).ToList().ForEach( commitView => {
                CommitViewArea.Items.Add(commitView);
            });
        }

        /// <summary>
        /// 選択ファイル一覧のチェック状態を一括変更する
        /// </summary>
        /// <param name="state"></param>
        private void ChangeAllFileListCheckState(bool state)
        {
            var items = CommitFileList.ItemsSource as IEnumerable<FileListItem>;
            if (items == null) { return; }

            List<FileListItem> source = new List<FileListItem>();
            items.ToList().ForEach(item =>
            {
                item.IsChecked = state;
                source.Add(item);
            });

            CommitFileList.ItemsSource = source;
        }

        /// <summary>
        /// 選択コミット変更時イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommitViewArea_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(Repository == null) { return; }

            Commit? commit = (CommitViewArea.SelectedItem as CommitView)?.Commit;
            if (commit == null) { return; }

            CurrentClickedCommit = commit;

            List<string> filePathes = new List<string>();
            if (ExtractFileGroups.ContainsKey(commit.Sha)) 
            {
                var group = ExtractFileGroups[commit.Sha];
                filePathes = group.FilePaths;
            }


            CommitFileList.ItemsSource = Utils.GetListFiles(Repository, commit).Select( file => 
                new FileListItem()
                {
                    FilePath = file,
                    IsChecked = filePathes.Contains(file),
                    Sha = commit.Sha,
                    DateTimeOffset = commit.Committer.When,
                }
            );
        }

        private void ShowExtractFileGroup() 
        {
            ExtractFileGroupArea.Children.Clear();
            ExtractFileGroups.Keys.ToList().ForEach(sha =>
            {
                var g = ExtractFileGroups[sha];
                ExtractFileGroupArea.Children.Add(new ExtractFileGroupView(g));
            });
        }

        private void AllCheck_Checked(object sender, RoutedEventArgs e) => ChangeAllFileListCheckState(true);

        private void AllCheck_Unchecked(object sender, RoutedEventArgs e) => ChangeAllFileListCheckState(false);

        private void CommitFileSelector_Checked(object sender, RoutedEventArgs e)
        {
            if (CurrentClickedCommit == null) { return; }

            CheckBox checkbox = (CheckBox) sender;
            string sha = CurrentClickedCommit.Sha;
            ExtractFileGroup group = ExtractFileGroups.ContainsKey(sha) ? ExtractFileGroups[sha] : new ExtractFileGroup(sha);
            string path = ((FileListItem) checkbox.DataContext).FilePath;
            group.Add(path);
            ExtractFileGroups[sha] = group;

            ShowExtractFileGroup();
        }

        private void CommitFileSelector_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CurrentClickedCommit == null) { return; }

            string sha = CurrentClickedCommit.Sha;
            ExtractFileGroup group = ExtractFileGroups[sha];

            CheckBox checkbox = (CheckBox)sender;
            string path = ((FileListItem)checkbox.DataContext).FilePath;
            group.Remove(path);
            if (group.IsEmpty)
            {
                ExtractFileGroups.Remove(sha);
            }
            else
            {
                ExtractFileGroups[sha] = group;
            }

            ShowExtractFileGroup();
        }

        private void CollectButton_Click(object sender, RoutedEventArgs e)
        {
            //選択ファイルが0件の場合は処理しない
            if (ExtractFileGroups.Count == 0) { return; }

            string now = DateTime.Now.ToString("yyyyMMddHHmmss");

            ExtractFileGroups.Keys.ToList().ForEach(sha =>
            {
                var group = ExtractFileGroups[sha];
                var commit = Repository.Lookup<Commit>(sha);
                Commands.Checkout(Repository, commit);
                group.FilePaths.ForEach(path =>
                {
                    string directoryPath = Path.Combine(now, $"{Path.GetDirectoryName(path)}");
                    if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }

                    string source = Path.Combine(AppDataManager.TargetDirectory, path);
                    string fileName = Path.GetFileName(path);
                    File.Copy(source, Path.Combine(directoryPath, fileName));
                });
            });

            //終了を知らせるために出力フォルダを開く
            Process.Start("explorer", now);
        }

        private void KeywordTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string keyword = KeywordTextBox.Text;
                if (string.IsNullOrEmpty(keyword)) 
                {
                    ShowAllCommits();
                    return; 
                }

                CommitViewArea.Items.Clear();
                Repository?.Commits.Where(commit => commit.Message.Contains(keyword)).Select(commit => new CommitView(commit)).ToList().ForEach(commitView =>
                {
                    CommitViewArea.Items.Add(commitView);
                });
            }
        }
    }
}
