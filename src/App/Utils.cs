using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace lenticulis_gui.src.App
{
    static class Utils
    {
        // List of accepted image file extensions (lower case without preceding dot)
        public static string[] Extensions = { "jpg", "jpeg", "png", "gif", "tif", "tiff", "psd", "bmp" };

        /// <summary>
        /// Builds C-compliant string (zero terminated)
        /// </summary>
        /// <param name="input">Input string to be formatted</param>
        /// <returns>C-compliant zero terminated char array</returns>
        public static char[] getCString(String input)
        {
            return (input + '\0').ToCharArray();
        }

        /// <summary>
        /// Converts color from ImageMagick format to ours
        /// </summary>
        /// <param name="number">input color-representing integer</param>
        /// <returns>converted ARGB color integer</returns>
        public static int convertColor(uint number)
        {
            // alpha channel is inverted
            uint alpha = 255 - ((number & 0xFF000000) >> 24);
            // remove old alpha and subtitute it with inverted one
            int c = (int)((number & 0x00FFFFFF) | (alpha << 24));
            return c;
        }

        /// <summary>
        /// Converts icon resource to imagesource usable in code for WPF components
        /// </summary>
        /// <param name="resourceName">name of icon resource</param>
        /// <returns>imagesource object from that icon</returns>
        public static ImageSource iconResourceToImageSource(String resourceName)
        {
            System.Drawing.Icon ico = ((System.Drawing.Icon)Properties.Resources.ResourceManager.GetObject(resourceName));
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(ico.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        /// <summary>
        /// Finds a Child of a given item in the visual tree.
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter.
        /// If not matching item can be found,
        /// a null parent is being returned.</returns>
        public static T FindChild<T>(DependencyObject parent, string childName)
           where T : DependencyObject
        {
            // confirm parent and childName are valid.
            if (parent == null)
                return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // if the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // if the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // if the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        /// <summary>
        /// Verifies, whether the supplied extension is accepted by us as valid image extension
        /// </summary>
        /// <param name="extension">string representation of extension</param>
        /// <returns>is valid image extension?</returns>
        public static bool IsAcceptedImageExtension(String extension)
        {
            // no extension is not valid
            if (extension == null || extension.Length == 0)
                return false;

            // prepare string to have no dot at the start, and be lower case
            if (extension[0] == '.')
                extension = extension.Substring(1);
            extension = extension.ToLower();

            for (int i = 0; i < Extensions.Length; i++)
            {
                if (extension.Equals(Extensions[i]))
                    return true;
            }
            return false;
        }
    }
}
