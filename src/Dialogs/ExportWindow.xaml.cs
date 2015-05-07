using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using lenticulis_gui.src.App;
using lenticulis_gui.src.Containers;
using lenticulis_gui.src.SupportLib;

namespace lenticulis_gui.src.Dialogs
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : MetroWindow
    {
        public ExportWindow()
        {
            InitializeComponent();

            Title = LangProvider.getString("EXPORT_WINDOW_TITLE");

            // starting value is "PNG", but quality can be set only when exporting to JPEG
            QualityNumBox.IsEnabled = false;
        }

        /// <summary>
        /// Browses the filesystem for suitable directory to export to
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // opens folder browser, so we can locate the project export directory
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ExportPathEdit.Text = dialog.SelectedPath;
            }
        }

        /// <summary>
        /// Change selection in format combobox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Count == 0 || e.AddedItems.Count == 0)
                return;

            // removed item = old extension, added item = new extension
            ComboBoxItem remItem = e.RemovedItems[0] as ComboBoxItem;
            ComboBoxItem addedItem = e.AddedItems[0] as ComboBoxItem;

            String extNew = addedItem.Content.ToString().ToLower();

            // extract current extension
            String[] splstr = ExportPatternEdit.Text.Split('.');
            if (splstr != null && splstr.Length > 0)
            {
                String ext = splstr[splstr.Length-1];

                // and if it's equal to removed item extension, replace it with added item extension
                if (ext.Equals(remItem.Content.ToString().ToLower()))
                {
                    ExportPatternEdit.Text = ExportPatternEdit.Text.Substring(0, ExportPatternEdit.Text.Length - ext.Length) + extNew;
                }
            }

            if (extNew == "jpeg")
                QualityNumBox.IsEnabled = true;
            else
                QualityNumBox.IsEnabled = false;
        }

        /// <summary>
        /// Clicked OK - validate entered data and export
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // export path must be filled
            if (ExportPathEdit.Text.Length == 0)
            {
                MessageBox.Show(LangProvider.getString("OUT_FILL_DIRECTORY"), LangProvider.getString("WRONG_OUTPUT_PARAMS"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // if target directory does not exist, offer creating
            if (!Directory.Exists(@ExportPathEdit.Text))
            {
                MessageBoxResult res = MessageBox.Show(LangProvider.getString("OUT_DIRECTORY_MISSING_CREATE_CONFIRM"), LangProvider.getString("OUT_DIRECTORY_MISSING_TITLE"), MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (res == MessageBoxResult.No)
                    return;
            }

            // export pattern must be filled
            if (ExportPatternEdit.Text.Length == 0)
            {
                MessageBox.Show(LangProvider.getString("OUT_FILL_PATTERN"), LangProvider.getString("WRONG_OUTPUT_PARAMS"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // export pattern must contain %i sequence
            if (!ExportPatternEdit.Text.Contains("%i"))
            {
                MessageBox.Show(LangProvider.getString("OUT_FILL_PATTERN_LAMBDA"), LangProvider.getString("WRONG_OUTPUT_PARAMS"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // quality must be filled
            byte quality = 0;
            try
            {
                quality = byte.Parse(QualityNumBox.Value.ToString());
            }
            catch (Exception)
            {
                //
            }

            // quality must be between 1 and 100 including
            if (quality <= 0 || quality > 100)
            {
                MessageBox.Show(LangProvider.getString("OUT_FILL_QUALITY"), LangProvider.getString("WRONG_OUTPUT_PARAMS"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // if the target directory does not exist, create one
            if (!Directory.Exists(@ExportPathEdit.Text))
                Directory.CreateDirectory(@ExportPathEdit.Text);

            // load project from selected file
            // Create new loading window
            LoadingWindow lw = new LoadingWindow("export");
            // show it
            lw.Show();
            // and disable this window to disallow all operations
            this.IsEnabled = false;

            // prepare layer object matrix
            LayerObject[][] objMatrix = ImageProcessor.prepareObjectMatrix(ProjectHolder.layers.ToArray());

            // and export it to file sequence
            ImageProcessor.exportObjectMatrix(objMatrix, ExportPathEdit.Text, ExportPatternEdit.Text, quality);

            // after image was loaded, enable main window
            this.IsEnabled = true;
            // and close loading window
            lw.Close();

            this.Close();
        }

        /// <summary>
        /// Cancels exporting, closes window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
