using lenticulis_gui.src.App;
using lenticulis_gui.src.Containers;
using MahApps.Metro.Controls;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace lenticulis_gui.src.Dialogs
{
    /// <summary>
    /// Interaction logic for ProjectPropertiesWindow.xaml
    /// </summary>
    public partial class ProjectPropertiesWindow : MetroWindow
    {
        /// <summary>
        /// milimeters to inches
        /// </summary>
        private float mmToIn = 25.4f;

        /// <summary>
        /// Selected units
        /// </summary>
        private string unitSelect = "px";

        public ProjectPropertiesWindow()
        {
            InitializeComponent();

            Title = LangProvider.getString("PROPERTIES_WINDOW_TITLE");

            // if there's a project opened, prefill boxes with actual data
            if (ProjectHolder.ValidProject)
            {
                PropertiesProjectName.Text = ProjectHolder.ProjectName;
                PropertiesHeight.Text = ProjectHolder.Height.ToString();
                PropertiesWidth.Text = ProjectHolder.Width.ToString();
                PropertiesImages.Value = ProjectHolder.ImageCount;
                PropertiesLayers.Value = ProjectHolder.LayerCount;
                PropertiesDPI.Value = ProjectHolder.Dpi;
                PropertiesLPI.Value = ProjectHolder.Lpi;
                LayerScaleCB.Visibility = Visibility.Visible;

                // do not enable PSD source when not creating new project
                SourcePSDPathEdit.IsEnabled = false;
                SourcePSDBrowseButton.IsEnabled = false;
            }
            else // if not, just use defaults
            {
                PropertiesProjectName.Text = LangProvider.getString("PROP_DEFAULT_PROJECT_NAME");
            }

            SourcePSDPathEdit.TextChanged += delegate(object sender, TextChangedEventArgs e)
            {
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
            LoadingWindow lw = new LoadingWindow("create");

            if (!ProjectHolder.ValidProject)
                lw.Show();

            // project name must be filled
            if (PropertiesProjectName.Text == "")
            {
                MessageBox.Show(LangProvider.getString("PROP_ERR_NAME"), LangProvider.getString("PROP_CREATE_ERROR_TITLE"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // also width and height must be filled
            if (PropertiesHeight.Text == "" || PropertiesWidth.Text == "")
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

            //DPI
            if (PropertiesDPI.Value == null)
            {
                MessageBox.Show(LangProvider.getString("PROP_ERR_DPI"), LangProvider.getString("PROP_CREATE_ERROR_TITLE"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //LPI
            if (PropertiesLPI.Value == null)
            {
                MessageBox.Show(LangProvider.getString("PROP_ERR_LPI"), LangProvider.getString("PROP_CREATE_ERROR_TITLE"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // height must be double larger than zero
            double height = Double.MinValue;
            if (!Double.TryParse(PropertiesHeight.Text, out height) || height <= 0)
            {
                MessageBox.Show(LangProvider.getString("PROP_ERR_HEIGHT_NEGATIVE"), LangProvider.getString("PROP_CREATE_ERROR_TITLE"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // width must be double larger than zero
            double width = Double.MinValue;
            if (!Double.TryParse(PropertiesWidth.Text, out width) || width <= 0)
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

            int dpi = (int)PropertiesDPI.Value;
            if (dpi <= 0)
            {
                MessageBox.Show(LangProvider.getString("PROP_ERR_DPI_NEGATIVE"), LangProvider.getString("PROP_CREATE_ERROR_TITLE"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int lpi = (int)PropertiesLPI.Value;
            if (lpi <= 0)
            {
                MessageBox.Show(LangProvider.getString("PROP_ERR_LPI_NEGATIVE"), LangProvider.getString("PROP_CREATE_ERROR_TITLE"), MessageBoxButton.OK, MessageBoxImage.Warning);
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

            //convert to px
            ConvertToPx(ref width, ref height, dpi, Units.SelectedItem.ToString());

            //old values
            int oldWidth = ProjectHolder.Width;
            int oldHeight = ProjectHolder.Height;

            // set all properties according to actual data in inputs
            ProjectHolder.ProjectName = PropertiesProjectName.Text;
            ProjectHolder.Height = (int)height;
            ProjectHolder.Width = (int)width;
            ProjectHolder.Dpi = dpi;
            ProjectHolder.Lpi = lpi;

            // get main window
            MainWindow mw = System.Windows.Application.Current.MainWindow as MainWindow;

            // if there's some project, the magic is a bit different
            if (ProjectHolder.ValidProject)
            {
                // update image count, layer count, and then refresh canvases
                mw.UpdateImageCount(images);
                mw.UpdateLayerCount(layers);

                //rescale layers if needed
                if(LayerScaleCB.IsChecked == true)
                    mw.RescaleLayers((float)width / (float)oldWidth, (float)height / (float)oldHeight);

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
                        newItem.GetLayerObject().ResourceId = ih.id;

                        // and put it into timeline, etc.
                        mw.AddTimelineItem(newItem, true, false);
                    }
                }
            }

            // in every case, we have valid project now
            ProjectHolder.ValidProject = true;

            mw.PropertyChanged();

            lw.Close();
            this.Close();
        }

        /// <summary>
        /// Convert to image points by print resolution
        /// </summary>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        /// <param name="dpi">dpi</param>
        /// <param name="units">units</param>
        private void ConvertToPx(ref double width, ref double height, int dpi, string units)
        {
            if (units.Equals("px"))
                return;

            double tmpWidth = width;
            double tmpHeight = height;

            //to inches
            if (units.Equals(LengthUnits.mm.ToString()) || units.Equals(LengthUnits.cm.ToString()))
            {
                tmpWidth /= mmToIn;
                tmpHeight /= mmToIn;
                if (units.Equals(LengthUnits.cm.ToString()))
                {
                    tmpWidth *= 10;
                    tmpHeight *= 10;
                }
            }

            //to px
            tmpWidth *= dpi;
            tmpHeight *= dpi;

            width = (int)Math.Round(tmpWidth);
            height = (int)Math.Round(tmpHeight);
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

        /// <summary>
        /// Check recommended frame count
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PropertiesPrint_ValueChanged(object sender, EventArgs e)
        {
            if (PropertiesDPI == null || PropertiesLPI == null || PropertiesImages == null)
                return;

            if (PropertiesDPI.Value == null || PropertiesLPI.Value == null || PropertiesImages.Value == null)
                return;

            int dpi = (int)PropertiesDPI.Value;
            int lpi = (int)PropertiesLPI.Value;
            int frames = (int)PropertiesImages.Value;

            //recommended frame count = DPI / LPI
            int framesRec = dpi / lpi;
            PropertiesRecommended.Content = framesRec;
        }

        /// <summary>
        /// Units combobox loaded method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;

            var values = LengthUnits.GetValues(typeof(LengthUnits));

            //Add units
            cb.Items.Add("px");
            foreach (var value in values)
            {
                cb.Items.Add(value);
            }

            cb.SelectedItem = "px";
        }

        /// <summary>
        /// Units combo box selection changed method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Units_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int dpi;
            double width, height;
            var selectedItem = (sender as ComboBox).SelectedItem;

            //get dpi
            if (PropertiesDPI.Value == null)
                return;
            if ((dpi = (int)PropertiesDPI.Value) <= 0)
                return;

            if (Double.TryParse(PropertiesHeight.Text, out height) && Double.TryParse(PropertiesWidth.Text, out width))
            {
                //to px
                if (!unitSelect.Equals("px"))
                    ConvertToPx(ref width, ref height, dpi, unitSelect);

                //to in 
                double widthTmp = width / (double)dpi;
                double heightTmp = height / (double)dpi;
                if (selectedItem.Equals(LengthUnits.mm)) 
                {
                    widthTmp *= mmToIn;
                    heightTmp *= mmToIn;
                }
                else if (selectedItem.Equals(LengthUnits.cm)) 
                {
                    widthTmp *= mmToIn / 10;
                    heightTmp *= mmToIn / 10;
                }
                else if (selectedItem.Equals("px"))
                {
                    widthTmp = width;
                    heightTmp = height;
                }

                heightTmp = (int)Math.Round(heightTmp * 1000) / 1000.0;
                widthTmp = (int)Math.Round(widthTmp * 1000) / 1000.0;

                //set new values
                PropertiesHeight.Text = heightTmp.ToString();
                PropertiesWidth.Text = widthTmp.ToString();

                unitSelect = selectedItem.ToString();
            }
        }
    }
}
