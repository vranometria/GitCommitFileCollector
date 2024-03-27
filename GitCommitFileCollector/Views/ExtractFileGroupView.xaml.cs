using GitCommitFileCollector.Models;
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
    /// ExtractFileGroupView.xaml の相互作用ロジック
    /// </summary>
    public partial class ExtractFileGroupView : UserControl
    {
        public ExtractFileGroupView()
        {
            InitializeComponent();
        }

        public ExtractFileGroupView(ExtractFileGroup g): this()
        {
            DataContext = g;
            g.FilePaths.ForEach(f =>
            {
                var label = new Label { Content = f, Width = double.NaN };
                FileList.Children.Add( label );
            });
        }


    }
}
