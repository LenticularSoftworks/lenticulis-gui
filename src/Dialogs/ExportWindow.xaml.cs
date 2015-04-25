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
        }

        /// <summary>
        /// Browses the filesystem for suitable directory to export to
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportBrowseButton_Click(object sender, RoutedEventArgs e)
        {
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

            // extract current extension
            String[] splstr = ExportPatternEdit.Text.Split('.');
            if (splstr != null && splstr.Length > 0)
            {
                String ext = splstr[splstr.Length-1];

                // and if it's equal to removed item extension, replace it with added item extension
                if (ext.Equals(remItem.Content.ToString().ToLower()))
                {
                    ExportPatternEdit.Text = ExportPatternEdit.Text.Substring(0, ExportPatternEdit.Text.Length - ext.Length) + addedItem.Content.ToString().ToLower();
                    return;
                }
            }
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
                MessageBox.Show("Vyplňte cestu, kam má program exportovat návrh", "Špatné parametry", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // if target directory does not exist, offer creating
            if (!Directory.Exists(@ExportPathEdit.Text))
            {
                MessageBoxResult res = MessageBox.Show("Cílová složka neexistuje. Má být programem vytvořena?", "Složka neexistuje", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (res == MessageBoxResult.No)
                    return;
            }

            // export pattern must be filled
            if (ExportPatternEdit.Text.Length == 0)
            {
                MessageBox.Show("Vyplňte vzor názvu výstupního souboru", "Špatné parametry", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // export pattern must contain %i sequence
            if (!ExportPatternEdit.Text.Contains("%i"))
            {
                MessageBox.Show("Použijte ve vzoru zástupné znaky %i pro označení místa, které program nahradí číslem snímku", "Špatné parametry", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Vyplňte kvalitu jako číslo od 1 do 100", "Špatné parametry", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // if the target directory does not exist, create one
            if (!Directory.Exists(@ExportPathEdit.Text))
                Directory.CreateDirectory(@ExportPathEdit.Text);

            // prepare layer object matrix
            LayerObject[][] objMatrix = ImageProcessor.prepareObjectMatrix(ProjectHolder.layers.ToArray());

            // and export it to file sequence
            ImageProcessor.exportObjectMatrix(objMatrix, ExportPathEdit.Text, ExportPatternEdit.Text, quality);

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
