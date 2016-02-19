using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using MahApps.Metro.Controls;
using lenticulis_gui.src.App;
using lenticulis_gui.src.Containers;
using lenticulis_gui.src.SupportLib;
using lenticulis_gui.src.Dialogs;
using System.Windows.Controls.Primitives;
using System.Diagnostics;

namespace lenticulis_gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        /// <summary>
        /// timeline column dimensions
        /// </summary>
        private const int rowHeight = 30;
        private const int columnMinWidth = 90;

        /// <summary>
        /// timeline item list
        /// </summary>
        public List<TimelineItem> timelineList;

        /// <summary>
        /// canvas list
        /// </summary>
        private List<WorkCanvas> canvasList;

        /// <summary>
        /// drag n drop captured items
        /// </summary>
        private TimelineItem capturedTimelineItem = null;
        private TimelineItem capturedTimelineItemContext = null;
        private WrapPanel capturedResizePanel = null;

        /// <summary>
        /// layer number for layer move up/down
        /// </summary>
        private int layerContext;

        /// <summary>
        /// drag n drop captured coords and dimensions
        /// </summary>
        private double capturedX;
        private double capturedY;
        private int capturedTimelineItemColumn;
        private int capturedTimelineItemLength;

        /// <summary>
        /// Selected tranfromation tool
        /// </summary>
        public static TransformType SelectedTool = TransformType.Translation;

        public MainWindow()
        {
            InitializeComponent();

            GetDrives();

            // Get available languages
            Dictionary<String, String> availableLangs = LangProvider.GetAvailableLangs();
            // and fetch them to menu items as available options
            LangChooserItem.Items.Clear();
            foreach (KeyValuePair<String, String> kvp in availableLangs)
            {
                // create new menu item
                MenuItem mi = new MenuItem();
                mi.Header = kvp.Value;
                // assign click event to change all we want
                mi.Click += new RoutedEventHandler(delegate(object o, RoutedEventArgs args)
                {
                    // change language
                    LangProvider.UseLanguage(kvp.Key);
                    // and update all bindings
                    LangDataSource.UpdateDataSources();
                });
                LangChooserItem.Items.Add(mi);
            }
        }

        /// <summary>
        /// Set image and layer count
        /// </summary>
        /// <param name="imageCount">image count</param>
        /// <param name="layerCount">layer count</param>
        public void SetProjectProperties(int imageCount, int layerCount)
        {
            Timeline.Children.Clear();
            SetImageCount(imageCount);
            AddTimelineHeader();
            AddTimelineLayer(layerCount);
            timelineList = new List<TimelineItem>();

            RefreshCanvasList();
        }

        /// <summary>
        /// Save routine - asks for file, if needed
        /// </summary>
        /// <returns></returns>
        private bool SaveRoutine(bool saveAs = false)
        {
            if (!ProjectHolder.ValidProject)
                return false;

            if (saveAs || ProjectHolder.ProjectFileName == null || ProjectHolder.ProjectFileName == "")
            {
                Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.FileName = "projekt";
                dialog.DefaultExt = ".lcp";
                dialog.Filter = LangProvider.getString("LENTICULIS_PROJECT_FILE") + " (.lcp)|*.lcp";

                Nullable<bool> res = dialog.ShowDialog();
                if (res == true)
                {
                    // save project to newly selected file target
                    ProjectSaver.saveProject(dialog.FileName);
                    return true;
                }
                return false;
            }
            else
            {
                // use previously stored filename
                ProjectSaver.saveProject();
                return true;
            }
        }

        #region Tools & buttons listeners

        /// <summary>
        /// Saving button click event hook - invoke save dialog and proceed saving if confirmed and filled correctly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveRoutine();
        }

        /// <summary>
        /// Clicked on project loading
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectHolder.ValidProject)
            {
                MessageBoxResult res = MessageBox.Show(LangProvider.getString("UNSAVED_WORK_CONFIRM_SAVE"), LangProvider.getString("UNSAVED_WORK"), MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                switch (res)
                {
                    // When clicked "Yes", offer saving, and if saving succeeds, proceed to load; otherwise do nothing
                    case MessageBoxResult.Yes:
                        if (!SaveRoutine())
                            return;
                        break;
                    // When clicked "No", discard any changes
                    case MessageBoxResult.No:
                        break;
                    // When clicked "Cancel", just do nothing
                    case MessageBoxResult.Cancel:
                        return;
                }
            }

            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = LangProvider.getString("LENTICULIS_PROJECT_FILE") + " (.lcp)|*.lcp";

            Nullable<bool> dres = dialog.ShowDialog();
            if (dres == true)
            {
                // load project from selected file
                // Create new loading window
                LoadingWindow lw = new LoadingWindow("project");
                // show it
                lw.Show();
                // and disable this window to disallow all operations
                this.IsEnabled = false;

                ProjectLoader.loadProject(dialog.FileName);

                // after image was loaded, enable main window
                this.IsEnabled = true;
                // and close loading window
                lw.Close();
            }
        }

        /// <summary>
        /// Creates new project
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewProjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectHolder.ValidProject)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show(LangProvider.getString("NEW_PROJECT_CONFIRM_TEXT"), LangProvider.getString("NEW_PROJECT_CONFIRM_TITLE"), MessageBoxButton.YesNo);

                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    SaveRoutine();
                    return;
                }
            }

            ProjectHolder.cleanUp();
            ProjectHolder.ValidProject = false;
            ProjectPropertiesWindow ppw = new ProjectPropertiesWindow();
            ppw.ShowDialog();
        }

        /// <summary>
        /// Opens dialog with project properties
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProjectPropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectHolder.ValidProject)
            {
                ProjectPropertiesWindow ppw = new ProjectPropertiesWindow();
                ppw.ShowDialog();
            }
        }

        /// <summary>
        /// Change canvas view event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoubleCanvas_Checked(object sender, RoutedEventArgs e)
        {
            // no project loaded / created
            if (!ProjectHolder.ValidProject)
                return;

            SetRangeSlider(0, ProjectHolder.ImageCount - 1);
            ShowDoubleCanvas(0, ProjectHolder.ImageCount - 1);
        }

        /// <summary>
        /// Change canvas view event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SingleCanvas_Checked(object sender, RoutedEventArgs e)
        {
            // no project loaded / created
            if (!ProjectHolder.ValidProject)
                return;

            SetSingleSlider(0);
            ShowSingleCanvas(0);
        }

        /// <summary>
        /// Change tool to translation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Translation_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.SelectedTool = TransformType.Translation;
        }

        /// <summary>
        /// Change tool to scale
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Scale_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.SelectedTool = TransformType.Scale;
        }

        /// <summary>
        /// Change tool to rotate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Rotate_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.SelectedTool = TransformType.Rotate;
        }

        /// <summary>
        /// Disable / enable 3D tool panel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TD_Checked(object sender, RoutedEventArgs e)
        {
            if (Panel3D.IsEnabled)
            {
                Panel3D.IsEnabled = false;
                LayerDepth.IsEnabled = false;
            }
            else
            {
                Panel3D.IsEnabled = true;
                LayerDepth.IsEnabled = true;

                //TODO - temp
                Width3D.Text = ProjectHolder.Width / (float)ProjectHolder.Dpi + " in";
            }
        }

        /// <summary>
        /// Clicked on zoom in button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ZoomInButton_Clicked(object sender, RoutedEventArgs e)
        {
            // no project loaded / created
            if (!ProjectHolder.ValidProject)
                return;

            foreach (WorkCanvas wc in canvasList)
            {
                if (wc.CanvasScale < 50.0)
                    wc.CanvasScale *= 1.2;
            }
        }

        /// <summary>
        /// Clicked on zoom out button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ZoomOutButton_Clicked(object sender, RoutedEventArgs e)
        {
            // no project loaded / created
            if (!ProjectHolder.ValidProject)
                return;

            foreach (WorkCanvas wc in canvasList)
            {
                if (wc.CanvasScale > 0.1)
                    wc.CanvasScale /= 1.2;
            }
        }

        /// <summary>
        /// On resize event - window itself
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MetroWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // adjust slider size to match the timeline
            SliderPanel.Margin = new Thickness() { Left = 43 + (Timeline.ActualWidth / Timeline.ColumnDefinitions.Count) / 2, Right = (Timeline.ActualWidth / Timeline.ColumnDefinitions.Count) / 2 };
        }

        /// <summary>
        /// Clicked on export button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportButton_Clicked(object sender, RoutedEventArgs e)
        {
            // no project loaded / created
            if (!ProjectHolder.ValidProject)
                return;

            ExportWindow ew = new ExportWindow();
            ew.ShowDialog();
        }

        /// <summary>
        /// Clicked on "About" menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowAboutWindow_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aw = new AboutWindow();
            aw.ShowDialog();
        }

        /// <summary>
        /// Clicked on GitHub menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenGithub_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/LenticularSoftworks");
        }

        /// <summary>
        /// Clicked on licencing menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenLicence_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.gnu.org/copyleft/gpl.html");
        }

        /// <summary>
        /// Clicked on close menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseProgram_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Clicked "Undo" button (menu item or toolbar)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            //
        }

        /// <summary>
        /// Clicked "Redo" button (menu item or toolbar)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            //
        }

        #endregion Tools & buttons listeners
    }
}
