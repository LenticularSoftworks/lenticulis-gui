using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using lenticulis_gui.src.Containers;

namespace lenticulis_gui.src.App
{
    /// <summary>
    /// Static class holding all possible metainformation about this project
    /// </summary>
    public static class ProjectHolder
    {
        /// <summary>
        /// Flag for "there is some project present in program!"
        /// </summary>
        public static bool ValidProject = false;

        /// <summary>
        /// Name of project - can be set in properties
        /// </summary>
        public static string ProjectName { get; set; }
        /// <summary>
        /// Name of file containing saved project work, if any
        /// </summary>
        public static string ProjectFileName { get; set; }

        /// <summary>
        /// Count of images, "keyframes" in this project
        /// </summary>
        public static int ImageCount { get; set; }
        /// <summary>
        /// Count of layers in this project
        /// </summary>
        public static int LayerCount { get; set; }

        /// <summary>
        /// Canvas width
        /// </summary>
        public static int Width { get; set; }

        /// <summary>
        /// Canvas height
        /// </summary>
        public static int Height { get; set; }

        /// <summary>
        /// All layers within this project
        /// </summary>
        public static List<Layer> Layers = new List<Layer>();

        /// <summary>
        /// DPI
        /// </summary>
        public static int Dpi { get; set; }

        /// <summary>
        /// LPI
        /// </summary>
        public static int Lpi { get; set; }

        //3D parameters

        /// <summary>
        /// View distance [in]
        /// </summary>
        public static double ViewDistance {get; set; }

        /// <summary>
        /// View angle [°]
        /// </summary>
        public static double ViewAngle { get; set; }

        /// <summary>
        /// Foreground [in]
        /// </summary>
        public static double Foreground { get; set; }

        /// <summary>
        /// Background [in]
        /// </summary>
        public static double Background { get; set; }

        /// <summary>
        /// List of undo / redo actions
        /// </summary>
        public static HistoryList HistoryList { get; set; }

        /// <summary>
        /// Force cleanup of whole project - clears all layers, its objects, and restores default settings
        /// This should be done after program startup, and before loading project, or creating new project
        /// </summary>
        public static void CleanUp()
        {
            MainWindow mw = System.Windows.Application.Current.MainWindow as MainWindow;
            if (mw == null)
                return;

            ValidProject = false;
            ViewAngle = 0.0;
            ViewDistance = 0.0;
            Foreground = 0.0;
            Background = 0.0;
            HistoryList = null;

            mw.ClearTimeline();
        }
    }
}
