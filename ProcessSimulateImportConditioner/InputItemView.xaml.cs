using FolderSelect;
using System.Windows;
using System.Windows.Controls;

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

        private void BrowseCustomOutputDir_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowserDialog = new FolderSelectDialog
            {
                Title = "Choose output directory"
            };

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

                else break;
            }

            ((Input)DataContext).OutputDirectory = folderBrowserDialog.FileName;
        }

        private void CustomOutputDirectory_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((Input)DataContext).OutputDirectory = ((TextBox)sender).Text;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ((Input)this.DataContext).Delete();
        }
    }
}
