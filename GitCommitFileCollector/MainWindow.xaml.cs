﻿using System;
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
using GitCommitFileCollector.Models;

namespace GitCommitFileCollector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppDataManager AppDataManager { get; set; }

        private Repository? Repository { get; set; }

        private SelectedItemInfo SelectedItem { get; set; } = new SelectedItemInfo();

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

        private void CommitViewArea_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Commit? commit = (CommitViewArea.SelectedItem as CommitView)?.Commit;
            if (commit == null) { return; }

            CommitFileList.ItemsSource = Utils.GetListFiles(commit).Select( file => 
                new FileListItem()
                {
                    FilePath = file,
                    IsChecked = false,
                    Sha = commit.Sha,
                    DateTimeOffset = commit.Committer.When,
                }
            );
        }

        private void AllCheck_Checked(object sender, RoutedEventArgs e) => ChangeAllFileListCheckState(true);

        private void AllCheck_Unchecked(object sender, RoutedEventArgs e) => ChangeAllFileListCheckState(false);

        private void CommitFileSelector_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CommitFileSelector_Unchecked(object sender, RoutedEventArgs e)
        {

        }
    }
}
