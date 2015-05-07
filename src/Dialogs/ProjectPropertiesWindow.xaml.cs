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

            Title = LangProvider.getString("PROPERTIES_WINDOW_TITLE");

            if (ProjectHolder.ValidProject)
            {
                PropertiesProjectName.Text = ProjectHolder.ProjectName;
                PropertiesHeight.Value = ProjectHolder.Height;
                PropertiesWidth.Value = ProjectHolder.Width;
                PropertiesImages.Value = ProjectHolder.ImageCount;
                PropertiesLayers.Value = ProjectHolder.LayerCount;
            }
            else
                PropertiesProjectName.Text = LangProvider.getString("PROP_DEFAULT_PROJECT_NAME");
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
            if (!ProjectHolder.ValidProject)
                ProjectHolder.cleanUp();

            if (PropertiesProjectName.Text == "")
            {
                MessageBox.Show(LangProvider.getString("PROP_ERR_NAME"), LangProvider.getString("PROP_CREATE_ERROR_TITLE"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PropertiesHeight.Value == null || PropertiesWidth.Value == null)
            {
                MessageBox.Show(LangProvider.getString("PROP_ERR_BOTHVALUES"), LangProvider.getString("PROP_CREATE_ERROR_TITLE"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PropertiesImages.Value == null)
            {
                MessageBox.Show(LangProvider.getString("PROP_ERR_FRAME_COUNT"), LangProvider.getString("PROP_CREATE_ERROR_TITLE"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PropertiesLayers.Value == null)
            {
                MessageBox.Show(LangProvider.getString("PROP_ERR_LAYER_COUNT"), LangProvider.getString("PROP_CREATE_ERROR_TITLE"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int height = (int)(PropertiesHeight.Value);
            if (height <= 0)
            {
                MessageBox.Show(LangProvider.getString("PROP_ERR_HEIGHT_NEGATIVE"), LangProvider.getString("PROP_CREATE_ERROR_TITLE"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int width = (int)(PropertiesWidth.Value);
            if (width <= 0)
            {
                MessageBox.Show(LangProvider.getString("PROP_ERR_WIDTH_NEGATIVE"), LangProvider.getString("PROP_CREATE_ERROR_TITLE"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int images = (int)(PropertiesImages.Value);
            if (images <= 0)
            {
                MessageBox.Show(LangProvider.getString("PROP_ERR_FRAMECOUNT_NATURAL"), LangProvider.getString("PROP_CREATE_ERROR_TITLE"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int layers = (int)(PropertiesLayers.Value);
            if (layers <= 0)
            {
                MessageBox.Show(LangProvider.getString("PROP_ERR_LAYERCOUNT_NATURAL"), LangProvider.getString("PROP_CREATE_ERROR_TITLE"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ProjectHolder.ProjectName = PropertiesProjectName.Text;
            ProjectHolder.Height = height;
            ProjectHolder.Width = width;

            MainWindow mw = System.Windows.Application.Current.MainWindow as MainWindow;

            if (ProjectHolder.ValidProject)
            {
                mw.UpdateImageCount(images);
                mw.UpdateLayerCount(layers);
                mw.RefreshCanvasList();
            }
            else
            {
                mw.SetProjectProperties(images, layers);
            }

            ProjectHolder.ValidProject = true;

            this.Close();
        }
    }
}
