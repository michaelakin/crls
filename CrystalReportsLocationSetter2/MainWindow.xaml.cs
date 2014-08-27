using CrystalReportLocationSetter2;
using CrystalReportsLocationSetter2.ViewModel;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telerik.Windows.Controls;

namespace CrystalReportsLocationSetter2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel _viewModel;

        public MainWindow()
        {
            StyleManager.ApplicationTheme = new Expression_DarkTheme();
            InitializeComponent();
            _viewModel = (MainViewModel)DataContext;
        }

        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            ToolBar toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }
            var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
            if (mainPanelBorder != null)
            {
                mainPanelBorder.Margin = new Thickness();
            }
        }

        private void reportListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string file in files.Where(c => System.IO.Path.GetExtension(c) == ".rpt"))
                {
                    var doc = new CrlsReportDocument();

                    try
                    {
                        //_logger.Log(LogLevel.Trace, String.Format("Loading {0}", file));
                        doc.Load(file);
                        _viewModel.Reports.Add(doc);
                    }
                    catch (Exception ex)
                    {
                        //_logger.LogException(LogLevel.Error, "Error: ", ex);
                    }
                }
            }
        }

        private void reportListBox_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.None;
            }
        }
    }
}
