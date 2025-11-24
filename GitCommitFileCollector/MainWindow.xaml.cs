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

        private readonly Dictionary<string, Commit> CommitsLookup = new();

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

            ShowAllBranches();

            ShowAllCommits();
        }

        private void StopButtons()
        {
            this.IsEnabled = false;
            Loading.Visibility = Visibility.Visible;
        }

        private void StartButtons()
        {
            this.IsEnabled = true;
            Loading.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// 参照先ディレクトリを初期設定
        /// </summary>
        private void InitTargeDirectory()
        {
            while (AppDataManager.NotSelectedDirectory || !Directory.Exists(AppDataManager.TargetDirectory))
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
        /// ブランチの一覧を表示する
        /// </summary>
        private void ShowAllBranches()
        {
            if (Repository == null) { return; }
            StopButtons();

            BranchSelector.Items.Clear();
            int currentIndex = 0;
            string currentBranch = Repository.Head.FriendlyName;
            Repository.Branches.ToList().ForEach(branch =>
            {
                BranchSelector.Items.Add(branch.FriendlyName);
                if (branch.FriendlyName == currentBranch)
                {
                    currentIndex = BranchSelector.Items.Count - 1;
                }
            });
            BranchSelector.SelectedIndex = currentIndex;
            StartButtons();
        }

        /// <summary>
        /// コミット情報を全て列挙する
        /// </summary>
        private void ShowAllCommits()
        {
            if (Repository == null) { return; }

            StopButtons();

            Commands.Checkout(Repository, Repository.Head.Tip);
            CommitViewArea.Items.Clear();
            Repository.Commits.ToList().ForEach(commit =>
            {
                CommitView commitView = new(commit);
                CommitViewArea.Items.Add(commitView);
                CommitsLookup[commit.Sha] = commit;
            });

            StartButtons();
        }

        /// <summary>
        /// 選択したコミットで変更されたファイル一覧を表示する
        /// </summary>
        /// <param name="commit"></param>
        private void ShowCommitFiles(Commit commit)
        {
            List<string> filePathes = new List<string>();
            if (ExtractFileGroups.ContainsKey(commit.Sha))
            {
                var group = ExtractFileGroups[commit.Sha];
                filePathes = group.FilePaths;
            }

            CommitFileList.ItemsSource = Utils.GetListFiles(Repository, commit).Select(file =>
                new FileListItem()
                {
                    FilePath = file,
                    IsChecked = filePathes.Contains(file),
                    Sha = commit.Sha,
                    DateTimeOffset = commit.Committer.When,
                }
            );
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
            if (Repository == null) { return; }

            StopButtons();

            Commit? commit = (CommitViewArea.SelectedItem as CommitView)?.Commit;
            if (commit == null) { return; }

            CurrentClickedCommit = commit;

            ShowCommitFiles(commit);
            StartButtons();
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

        private void FilterCommits()
        {
            string keyword = KeywordTextBox.Text.Trim();
            string commiter = CommiterFilterTextBox.Text.Trim();


            if (string.IsNullOrEmpty(keyword) && string.IsNullOrEmpty(commiter))
            {
                ShowAllCommits();
                return;
            }

            CommitViewArea.Items.Clear();

            var commits = Repository?.Commits.ToList();

            if (commits == null) { return; }

            if (!string.IsNullOrEmpty(commiter))
            {
                commits = commits.Where(commit => commit.Committer.Name.Contains(commiter)).ToList();
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                commits = commits.Where(commit => commit.Message.Contains(keyword)).ToList();
            }

            commits.Select(commit => new CommitView(commit)).ToList().ForEach(commitView =>
            {
                CommitViewArea.Items.Add(commitView);
            });

        }

        private void AllCheck_Checked(object sender, RoutedEventArgs e) => ChangeAllFileListCheckState(true);

        private void AllCheck_Unchecked(object sender, RoutedEventArgs e) => ChangeAllFileListCheckState(false);

        private void CommitFileSelector_Checked(object sender, RoutedEventArgs e)
        {
            if (CurrentClickedCommit == null) { return; }

            CheckBox checkbox = (CheckBox)sender;
            string sha = CurrentClickedCommit.Sha;
            ExtractFileGroup group = ExtractFileGroups.ContainsKey(sha) ? ExtractFileGroups[sha] : new ExtractFileGroup(sha);
            string path = ((FileListItem)checkbox.DataContext).FilePath;
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

        /// <summary>
        /// キーワード変更イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeywordTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FilterCommits();
            }
        }

        /// <summary>
        /// コミッターフィルター変更イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommiterFilterTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FilterCommits();
            }
        }

        /// <summary>
        /// コミッターフィルター条件変更イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommiterFilterTextBox_TextChanged(object sender, TextChangedEventArgs e) { }

        /// <summary>
        /// ブランチ変更イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BranchSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string? branchName = BranchSelector.SelectedItem as string;
            if (branchName == null || Repository == null) { return; }
            StopButtons();

            ExtractFileGroupArea.Children.Clear();

            ExtractFileGroups.Clear();

            var branch = Repository.Branches[branchName];
            Commands.Checkout(Repository, branch);
            ShowAllCommits();

            ShowCommitFiles(Repository.Head.Tip);

            FilterCommits();

            StartButtons();
        }

    }
}
