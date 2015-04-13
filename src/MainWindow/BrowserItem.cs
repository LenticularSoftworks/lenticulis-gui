using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using lenticulis_gui.src.App;

namespace lenticulis_gui
{
    /// <summary>
    /// File browser Item
    /// </summary>
    class BrowserItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Extension { get; set; }
        public bool Dir { get; set; }
        public string Ico { get; set; }

        private const string PATH = "/lenticulis-gui;component/res/icon/";

        public BrowserItem(string name, string path, string extension, bool dir)
        {
            this.Name = name;
            this.Path = path;
            this.Extension = extension.ToLower();
            this.Dir = dir;
            FindIcon();
        }

        /// <summary>
        /// Finds icon by file extension
        /// </summary>
        private void FindIcon()
        {
            // If the extension is valid image extension
            if (Utils.IsAcceptedImageExtension(Extension))
            {
                Ico = PATH + "Image.ico";
                return;
            }

            // Other extensions
            switch (Extension)
            {
                case "drive": Ico = PATH + "Disc.ico"; break;
                case "dir": Ico = PATH + "Folder.ico"; break;
                case "parent": Ico = PATH + "Trackback.ico"; break;
                default: Ico = PATH + "Unknown.ico"; break;
            }
        }

        /// <summary>
        /// To string
        /// </summary>
        /// <returns>Name of file</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
