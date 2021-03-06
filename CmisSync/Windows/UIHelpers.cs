﻿//-----------------------------------------------------------------------
// <copyright file="UIHelpers.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General private License for more details.
//
//   You should have received a copy of the GNU General private License
//   along with this program. If not, see http://www.gnu.org/licenses/.
//
// </copyright>
//-----------------------------------------------------------------------
//   CmisSync, a collaboration and sharing tool.
//   Copyright (C) 2010  Hylke Bons <hylkebons@gmail.com>
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General Public License for more details.
//
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace CmisSync {
    using System;
    using System.IO;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using Drawing = System.Drawing;

    using CmisSync.Lib.Config;

    /// <summary>
    /// Convenient methods for retrieving images from files.
    /// </summary>
    public static class UIHelpers {
        
        /// <summary>
        /// Get the image frame associated with given identifier.
        /// </summary>
        public static BitmapFrame GetImageSource (string name) {
            return GetImageSource (name, "png");
        }

        private static string FindImagePathname(string folder, string filename) {
            ClientBrand brand = new ClientBrand();
            foreach (string path in brand.PathList) {
                if (Path.GetFileName(path) == filename) {
                    string pathname = Path.Combine(folder, path.Substring(1));
                    if (File.Exists(pathname)) {
                        return pathname;
                    } else {
                        return null;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get the image frame associated with given identifier and file type.
        /// </summary>
        /// <param name="type">Filename extension, for instance "png" or "ico".</param>
        public static BitmapFrame GetImageSource(string name, string type) {
            string filename = name + "." + type;

            string brandFolder = Path.Combine(ConfigManager.CurrentConfig.GetConfigPath(), Program.Controller.BrandConfigFolder);
            string pathname = FindImagePathname(brandFolder, filename);
            if (!string.IsNullOrEmpty(pathname)) {
                try {
                    using (FileStream stream = File.Open(pathname, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                        return BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }
                } catch (Exception) {
                }
            }

            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream image_stream = assembly.GetManifestResourceStream("CmisSync.Pixmaps." + filename);
            return BitmapFrame.Create(image_stream);
        }

        /// <summary>
        /// Get the image associated with given identifier.
        /// </summary>
        public static Drawing.Bitmap GetBitmap (string name) {
            string filename = name + ".png";

            string brandFolder = Path.Combine(ConfigManager.CurrentConfig.GetConfigPath(), Program.Controller.BrandConfigFolder);
            string pathname = FindImagePathname(brandFolder, filename);
            if (!string.IsNullOrEmpty(pathname)) {
                try {
                    using (FileStream stream = File.Open(pathname, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                        return (Drawing.Bitmap)Drawing.Bitmap.FromStream(stream);
                    }
                } catch (Exception) {
                }
            }

            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream image_stream = assembly.GetManifestResourceStream ("CmisSync.Pixmaps." + filename);
            return (Drawing.Bitmap) Drawing.Bitmap.FromStream (image_stream);
        }

        /// <summary>
        /// Get the icon associated with given identifier.
        /// </summary>
        public static Drawing.Icon GetIcon(string name) {
            return Drawing.Icon.FromHandle(GetBitmap(name).GetHicon());
        }
    }
}