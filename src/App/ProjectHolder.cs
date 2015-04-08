using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using lenticulis_gui.src.Containers;

namespace lenticulis_gui.src.App
{
    public static class ProjectHolder
    {
        public static String ProjectName { get; set; }
        public static String ProjectFileName { get; set; }

        public static int ImageCount { get; set; }
        public static int LayerCount { get; set; }

        public static List<Layer> layers = new List<Layer>();
    }
}
