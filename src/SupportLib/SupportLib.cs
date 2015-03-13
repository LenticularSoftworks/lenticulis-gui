﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace lenticulis_gui.src.SupportLib
{
    /// <summary>
    /// Static unsafe class holding all DLL imports from main support library
    /// </summary>
    static unsafe class SupportLib
    {
        /// <summary>
        /// Filename of imported main support library
        /// </summary>
        const String LENT_SUPPORT_DLL_NAME = "lenticulis-support.dll";

        /// <summary>
        /// Registers image in memory, and returns ID
        /// </summary>
        /// <param name="filename">Path to image to be loaded</param>
        /// <returns>ID of loaded image</returns>
        [DllImport(SupportLib.LENT_SUPPORT_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int registerImage(char[] filename);

        /// <summary>
        /// Registers image in memory, returns ID and fills output parameters with valid data
        /// </summary>
        /// <param name="filename">Path to image to be loaded</param>
        /// <param name="format">Image format (output param)</param>
        /// <param name="colorSpace">Image color space (output param)</param>
        /// <param name="width">Image width in pixels (output param)</param>
        /// <param name="height">Image height in pixels (output param)</param>
        /// <param name="mipmapData">Built mipmap, as array of integers; its size is determined using image dimensions and fixed mipmap dimension size</param>
        /// <returns>ID of loaded image</returns>
        [DllImport(SupportLib.LENT_SUPPORT_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int registerImageP(char[] filename, out char*[] format, out IntPtr colorSpace, out UIntPtr width, out UIntPtr height, out void* mipmapData);

        /// <summary>
        /// Retrieves image ID using path
        /// </summary>
        /// <param name="filename">Path of image</param>
        /// <returns>ID of image</returns>
        [DllImport(SupportLib.LENT_SUPPORT_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int getImageId(char[] filename);

        /// <summary>
        /// Retrieves image filename using its ID
        /// </summary>
        /// <param name="imageId">image ID</param>
        /// <returns>Image filename</returns>
        [DllImport(SupportLib.LENT_SUPPORT_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern char[] getImageFileName(int imageId);

        /// <summary>
        /// Retrieves image format using its ID
        /// </summary>
        /// <param name="imageId">image ID</param>
        /// <returns>Image format</returns>
        [DllImport(SupportLib.LENT_SUPPORT_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern char[] getImageFormat(int imageId);

        /// <summary>
        /// Retrieves image color space using its ID
        /// </summary>
        /// <param name="imageId">image ID</param>
        /// <returns>Image color space</returns>
        [DllImport(SupportLib.LENT_SUPPORT_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int getImageColorSpace(int imageId);

        /// <summary>
        /// Retrieves image width in pixels using its ID
        /// </summary>
        /// <param name="imageId">image ID</param>
        /// <returns>Image width in pixels</returns>
        [DllImport(SupportLib.LENT_SUPPORT_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint getImageWidth(int imageId);

        /// <summary>
        /// Retrieves image height in pixels using its ID
        /// </summary>
        /// <param name="imageId">image ID</param>
        /// <returns>Image height in pixels</returns>
        [DllImport(SupportLib.LENT_SUPPORT_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint getImageHeight(int imageId);

        /// <summary>
        /// Retrieves image mipmap data pointer using its ID
        /// </summary>
        /// <param name="imageId">image ID</param>
        /// <returns>Pointer to mipmap data</returns>
        [DllImport(SupportLib.LENT_SUPPORT_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void* getImageMipmap(int imageId);
    }
}
