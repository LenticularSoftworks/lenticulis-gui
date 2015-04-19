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
using System.Windows.Shapes;
using lenticulis_gui.src.App;
using lenticulis_gui.src.Containers;

namespace lenticulis_gui.src.Dialogs
{
    /// <summary>
    /// Interaction logic for ProjectPropertiesWindow.xaml
    /// </summary>
    public partial class ProjectPropertiesWindow : Window
    {
        public ProjectPropertiesWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Close window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Check inputs and create new project
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            ProjectHolder.cleanUp();

            if (PropertiesProjectName.Text == "")
            {
                MessageBox.Show("Vyplňte název projektu", "Chyba vytvoření projektu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int height = parseInputInt(PropertiesHeight.Text);
            if (height <= 0)
            {
                MessageBox.Show("Výška musí být kladné číslo", "Chyba vytvoření projektu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int width = parseInputInt(PropertiesWidth.Text);
            if (width <= 0)
            {
                MessageBox.Show("Šířka musí být kladné číslo", "Chyba vytvoření projektu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int images = parseInputInt(PropertiesImages.Text);
            if (images <= 0)
            {
                MessageBox.Show("Počet snímků musí být přirozené číslo", "Chyba vytvoření projektu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int layers = parseInputInt(PropertiesLayers.Text);
            if (layers <= 0)
            {
                MessageBox.Show("Počet vrstev musí být přirozené číslo", "Chyba vytvoření projektu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ProjectHolder.ProjectName = PropertiesProjectName.Text;
            ProjectHolder.Height = height;
            ProjectHolder.Width = width;

            MainWindow mw = System.Windows.Application.Current.MainWindow as MainWindow;
            mw.SetProjectProperties(images, layers);

            this.Close();
        }

        /// <summary>
        /// Try parse integer - Return -1 if failed
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private int parseInputInt(string number)
        {
            int result;

            try
            {
                result = int.Parse(number);
            }
            catch
            {
                result = -1;
            }

            return result;
        }
    }
}
