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
using MahApps.Metro.Controls;

namespace lenticulis_gui.src.Dialogs
{
    /// <summary>
    /// Interaction logic for ProjectPropertiesWindow.xaml
    /// </summary>
    public partial class ProjectPropertiesWindow : MetroWindow
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

            int height = (int)(PropertiesHeight.Value);
            if (height <= 0)
            {
                MessageBox.Show("Výška musí být kladné číslo", "Chyba vytvoření projektu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int width = (int)(PropertiesWidth.Value);
            if (width <= 0)
            {
                MessageBox.Show("Šířka musí být kladné číslo", "Chyba vytvoření projektu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int images = (int)(PropertiesImages.Value);
            if (images <= 0)
            {
                MessageBox.Show("Počet snímků musí být přirozené číslo", "Chyba vytvoření projektu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int layers = (int)(PropertiesLayers.Value);
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
    }
}
