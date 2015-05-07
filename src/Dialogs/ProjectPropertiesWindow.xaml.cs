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

            // if there's a project opened, prefill boxes with actual data
            if (ProjectHolder.ValidProject)
            {
                PropertiesProjectName.Text = ProjectHolder.ProjectName;
                PropertiesHeight.Value = ProjectHolder.Height;
                PropertiesWidth.Value = ProjectHolder.Width;
                PropertiesImages.Value = ProjectHolder.ImageCount;
                PropertiesLayers.Value = ProjectHolder.LayerCount;
            }
            else // if not, just use defaults
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
            // project name must be filled
            if (PropertiesProjectName.Text == "")
            {
                MessageBox.Show(LangProvider.getString("PROP_ERR_NAME"), LangProvider.getString("PROP_CREATE_ERROR_TITLE"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // also width and height must be filled
            if (PropertiesHeight.Value == null || PropertiesWidth.Value == null)
            {
                MessageBox.Show(LangProvider.getString("PROP_ERR_BOTHVALUES"), LangProvider.getString("PROP_CREATE_ERROR_TITLE"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // frame count also has to be filled
            if (PropertiesImages.Value == null)
            {
                MessageBox.Show(LangProvider.getString("PROP_ERR_FRAME_COUNT"), LangProvider.getString("PROP_CREATE_ERROR_TITLE"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // layer count as well
            if (PropertiesLayers.Value == null)
            {
                MessageBox.Show(LangProvider.getString("PROP_ERR_LAYER_COUNT"), LangProvider.getString("PROP_CREATE_ERROR_TITLE"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // height must be an integer larger than zero
            int height = (int)(PropertiesHeight.Value);
            if (height <= 0)
            {
                MessageBox.Show(LangProvider.getString("PROP_ERR_HEIGHT_NEGATIVE"), LangProvider.getString("PROP_CREATE_ERROR_TITLE"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // width must be an integer larger than zero
            int width = (int)(PropertiesWidth.Value);
            if (width <= 0)
            {
                MessageBox.Show(LangProvider.getString("PROP_ERR_WIDTH_NEGATIVE"), LangProvider.getString("PROP_CREATE_ERROR_TITLE"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // image count must be an integer larger than zero
            int images = (int)(PropertiesImages.Value);
            if (images <= 0)
            {
                MessageBox.Show(LangProvider.getString("PROP_ERR_FRAMECOUNT_NATURAL"), LangProvider.getString("PROP_CREATE_ERROR_TITLE"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // layer count must be an integer larger than zero
            int layers = (int)(PropertiesLayers.Value);
            if (layers <= 0)
            {
                MessageBox.Show(LangProvider.getString("PROP_ERR_LAYERCOUNT_NATURAL"), LangProvider.getString("PROP_CREATE_ERROR_TITLE"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // set all properties according to actual data in inputs
            ProjectHolder.ProjectName = PropertiesProjectName.Text;
            ProjectHolder.Height = height;
            ProjectHolder.Width = width;

            // get main window
            MainWindow mw = System.Windows.Application.Current.MainWindow as MainWindow;

            // if there's some project, the magic is a bit different
            if (ProjectHolder.ValidProject)
            {
                // update image count, layer count, and then refresh canvases
                mw.UpdateImageCount(images);
                mw.UpdateLayerCount(layers);
                mw.RefreshCanvasList();
            }
            else
            {
                // just hardly set frame and layer count
                mw.SetProjectProperties(images, layers);
            }

            // in every case, we have valid project now
            ProjectHolder.ValidProject = true;

            this.Close();
        }
    }
}
