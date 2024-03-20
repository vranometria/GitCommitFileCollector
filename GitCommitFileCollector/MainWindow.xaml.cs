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
using Forms = System.Windows.Forms;

namespace GitCommitFileCollector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitTargeDirectory();


        }

        private void InitTargeDirectory()
        {
            var manager = AppDataManager.Instance;

            while (manager.NotSelectedDirectory)
            {
                var fbd = new Forms.FolderBrowserDialog();
                if (fbd.ShowDialog() == Forms.DialogResult.OK)
                {
                    manager.SetTargetDirectory(fbd.SelectedPath);
                }
            }
        }
    }
}
