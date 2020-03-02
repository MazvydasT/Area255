using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

using FolderSelect;

namespace ProcessSimulateImportConditioner
{
    /// <summary>
    /// Interaction logic for InputItemView.xaml
    /// </summary>
    public partial class InputItemView : UserControl
    {
        public InputItemView()
        {
            InitializeComponent();
        }

        private void browseCustomOutputDir_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowserDialog = new FolderSelectDialog();

            while (true)
            {
                var openFolderResult = folderBrowserDialog.ShowDialog();
                if (!openFolderResult) return;

                if (!Utils.DirectoryIsEmpty(folderBrowserDialog.FileName))
                {
                    var messageBoxResult = MessageBox.Show("Selected directory contains data. OK to overwrite?", "Overwrite warning", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                    if (messageBoxResult == MessageBoxResult.Cancel) return;
                    if (messageBoxResult == MessageBoxResult.Yes) break;
                }
            }

            ((Input)DataContext).OutputDirectory = folderBrowserDialog.FileName;
        }

        private void customOutputDirectory_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((Input)DataContext).OutputDirectory = ((TextBox)sender).Text;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ((Input)this.DataContext).Delete();
        }
    }
}
