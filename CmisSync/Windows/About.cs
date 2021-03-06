//-----------------------------------------------------------------------
// <copyright file="About.cs" company="GRAU DATA AG">
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
//   Copyright (C) 2010  Hylke Bons (hylkebons@gmail.com)
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
//   along with this program. If not, see (http://www.gnu.org/licenses/).


using System;
using System.ComponentModel; 
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xaml;

namespace CmisSync {

    /// <summary>
    /// About dialog.
    /// It shows information such as the CmisSync name and logo, the version, some copyright information.
    /// </summary>
    public class About : Window {

        /// <summary>
        /// Controller.
        /// </summary>
        public AboutController Controller = new AboutController ();

        /// <summary>
        /// Constructor.
        /// </summary>
        public About ()
        {
            Title      = String.Format(Properties_Resources.About, Properties_Resources.ApplicationName);
            ResizeMode = ResizeMode.NoResize;
            Height     = 288;
            Width      = 640;
            Icon = UIHelpers.GetImageSource("app", "ico");
            
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Closing += Close;

            //CreateAbout ();
            LoadAbout();

            CreateLink();

            Controller.ShowWindowEvent += delegate {
               Dispatcher.BeginInvoke ((Action) delegate {
                    Show ();
                    Activate ();
                    BringIntoView ();
                });
            };

            Controller.HideWindowEvent += delegate {
                Dispatcher.BeginInvoke((Action)delegate {
                    Hide ();
                });
            };

            Controller.NewVersionEvent += delegate (string new_version) {
                Dispatcher.BeginInvoke((Action)delegate {
                    this.updates.Content = String.Format(Properties_Resources.NewVersionAvailable, new_version);
                    this.updates.UpdateLayout ();
                });
            };

            Controller.VersionUpToDateEvent += delegate {
                Dispatcher.BeginInvoke ((Action) delegate {
                    this.updates.Content = Properties_Resources.RunningLatestVersion;
                    this.updates.UpdateLayout ();
                });
            };

            Controller.CheckingForNewVersionEvent += delegate {
                Dispatcher.BeginInvoke ((Action) delegate {
                    //this.updates.Content = "Checking for updates...";
                    this.updates.UpdateLayout ();
                });
            };
        }

        private Canvas canvas;
        private Image image;
        private Label version;
        private Label updates;
        private TextBlock credits;

        private void LoadAbout()
        {
            System.Uri resourceLocater = new System.Uri("/DataSpaceSync;component/AboutWPF.xaml", System.UriKind.Relative);
            UserControl aboutWPF = Application.LoadComponent(resourceLocater) as UserControl;

            canvas = aboutWPF.FindName("canvas") as Canvas;
            image = aboutWPF.FindName("image") as Image;
            version = aboutWPF.FindName("version") as Label;
            updates = aboutWPF.FindName("updates") as Label;
            credits = aboutWPF.FindName("credits") as TextBlock;

            image.Source = UIHelpers.GetImageSource("about");
            version.Content = String.Format(Properties_Resources.Version, Controller.RunningVersion, Controller.CreateTime.GetValueOrDefault().ToString("d"));
            updates.Content = "";
            credits.Text = String.Format("Copyright © {0}–{1} {2}\n\n{3} {4}",
                    "2013",
                    DateTime.Now.Year.ToString(),
                    " GRAU DATA AG, Aegif and others.",
                    Properties_Resources.ApplicationName,
                    "is Open Source software. You are free to use, modify, and redistribute it under the GNU General Public License version 3 or later.");

            Content = aboutWPF;
        }

        private void CreateLink()
        {
            Link website_link = new Link(Properties_Resources.Website, Controller.WebsiteLinkAddress);
            Link credits_link = new Link(Properties_Resources.Credits, Controller.CreditsLinkAddress);
            Link report_problem_link = new Link(Properties_Resources.ReportProblem, Controller.ReportProblemLinkAddress);

            canvas.Children.Add(website_link);
            Canvas.SetLeft(website_link, 289);
            Canvas.SetTop(website_link, 222);

            canvas.Children.Add(credits_link);
            Canvas.SetLeft(credits_link, 289 + website_link.ActualWidth + 60);
            Canvas.SetTop(credits_link, 222);

            canvas.Children.Add(report_problem_link);
            Canvas.SetLeft(report_problem_link, 289 + website_link.ActualWidth + credits_link.ActualWidth + 115);
            Canvas.SetTop(report_problem_link, 222);
        }
        
        /// <summary>
        /// Close the dialog.
        /// </summary>
        private void Close (object sender, CancelEventArgs args)
        {
            Controller.WindowClosed ();
            args.Cancel = true;
        }
    }


    /// <summary>
    /// Hyperlink label that opens an URL in the default browser.
    /// </summary>
    public class Link : Label {

        public Link (string title, string address)
        {
            FontSize   = 11;
            Cursor     = Cursors.Hand;
            Foreground = new SolidColorBrush (Color.FromRgb (135, 178, 227));

            TextDecoration underline = new TextDecoration () {
                Pen              = new Pen (new SolidColorBrush (Color.FromRgb (135, 178, 227)), 1),
                PenThicknessUnit = TextDecorationUnit.FontRecommended
            };

            TextDecorationCollection collection = new TextDecorationCollection ();
            collection.Add (underline);

            TextBlock text_block = new TextBlock () {
                Text            = title,
                TextDecorations = collection
            };

            Content = text_block;

            MouseUp += delegate {
                Process.Start (new ProcessStartInfo (address));
            };
        }
    }
}
