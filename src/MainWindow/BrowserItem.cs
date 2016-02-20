using lenticulis_gui.src.App;

namespace lenticulis_gui
{
    /// <summary>
    /// File browser Item
    /// </summary>
    public class BrowserItem
    {
        /// <summary>
        /// Item filename (or folder name)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Item file path (or folder path)
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// File extension (if any)
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Is this item a directory?
        /// </summary>
        public bool Dir { get; set; }

        /// <summary>
        /// Used icon identifier
        /// </summary>
        public string Ico { get; set; }

        /// <summary>
        /// Is this item an image?
        /// </summary>
        public bool Image { get; set; }

        /// <summary>
        /// Path to icon resource files
        /// </summary>
        private const string PATH = "/lenticulis-gui;component/res/icon/";

        /// <summary>
        /// The only one constructor
        /// </summary>
        /// <param name="name">name of item</param>
        /// <param name="path">path to item</param>
        /// <param name="extension">item extension (if any)</param>
        /// <param name="dir">is directory?</param>
        public BrowserItem(string name, string path, string extension, bool dir)
        {
            this.Name = name;
            this.Path = path;
            this.Extension = extension.ToLower();
            this.Dir = dir;

            // if it's recognized and supported file extension, this flag would change icon and add
            // the drag'n'drop capability
            Image = Utils.IsAcceptedImageExtension(Extension);
            FindIcon();
        }

        /// <summary>
        /// Finds icon by file extension
        /// </summary>
        private void FindIcon()
        {
            // If file is acceptable format
            if (Image)
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
