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
using lenticulis_gui.src.SupportLib;
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

                // do not enable PSD source when not creating new project
                SourcePSDPathEdit.IsEnabled = false;
                SourcePSDBrowseButton.IsEnabled = false;
            }
            else // if not, just use defaults
            {
                PropertiesProjectName.Text = LangProvider.getString("PROP_DEFAULT_PROJECT_NAME");
            }

            SourcePSDPathEdit.TextChanged += delegate(object sender, TextChangedEventArgs e) {
                if (SourcePSDPathEdit.Text.Length > 0)
                    PropertiesLayers.IsEnabled = false;
                else
                    PropertiesLayers.IsEnabled = true;
            };
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

            // if creating project, and using PSD as source..
            if (!ProjectHolder.ValidProject && SourcePSDPathEdit.Text.Length > 0)
            {
                StringBuilder sb = new StringBuilder(1024);
                // retrieve layer count
                int count = SupportLib.SupportLib.getLayerInfo(Utils.getCString(SourcePSDPathEdit.Text), sb);
                // count lower or equal zero means error - the file does not exist or has corrupted format
                if (count <= 0)
                {
                    MessageBox.Show(LangProvider.getString("PSD_SOURCE_INVALID"), LangProvider.getString("PROP_CREATE_ERROR_TITLE"), MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                layers = count;
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

                // if there was PSD specified; we can now be sure, the PSD is valid
                if (SourcePSDPathEdit.Text.Length > 0)
                {
                    String filepath = SourcePSDPathEdit.Text;
                    ImageHolder ih;

                    // extract bare file name
                    String[] expl = filepath.Split('\\');
                    String fileBareName = expl[expl.Length - 1];

                    // for each layer, load PSD layer, and put it into project
                    for (int i = 1; i <= layers; i++)
                    {
                        // load PSD with current layer
                        ih = ImageHolder.loadImage(filepath, true, i);

                        // create timeline item
                        TimelineItem newItem = new TimelineItem(layers - i, 0, images, fileBareName);
                        newItem.getLayerObject().ResourceId = ih.id;

                        // and put it into timeline, etc.
                        mw.AddTimelineItem(newItem);
                    }
                }
            }

            // in every case, we have valid project now
            ProjectHolder.ValidProject = true;

            this.Close();
        }

        /// <summary>
        /// When clicked on source PSD browse button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SourcePSDBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // open dialog to search for needed PSD
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = LangProvider.getString("PSD_FORMAT_NAME") + " (.psd)|*.psd";

            Nullable<bool> dres = dialog.ShowDialog();
            if (dres == true)
                SourcePSDPathEdit.Text = dialog.FileName;
        }
    }
}
