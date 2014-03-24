﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using CmisSync.Lib.Credentials;
using CmisSync.CmisTree;
using System.Collections.ObjectModel;

namespace CmisSync
{
    /// <summary>
    /// Edit folder diaglog
    /// It allows user to edit the selected and ignored folders
    /// </summary>
    class Edit : SetupWindow
    {
        /// <summary>
        /// Controller
        /// </summary>
        public EditController Controller = new EditController();


        /// <summary>
        /// Synchronized folder name
        /// </summary>
        public string FolderName;

        /// <summary>
        /// Ignore folder list
        /// </summary>
        public List<string> Ignores;

        /// <summary>
        /// Credentials
        /// </summary>
        public CmisRepoCredentials Credentials;

        private string remotePath;
        private string localPath;

        public enum EditType {
            EditFolder,
            EditCredentials
        };

        private EditType type;

        /// <summary>
        /// Constructor
        /// </summary>
        public Edit(EditType type, CmisRepoCredentials credentials, string name, string remotePath, List<string> ignores, string localPath)
        {
            FolderName = name;
            this.Credentials = credentials;
            this.remotePath = remotePath;
            this.Ignores = new List<string>(ignores);
            this.localPath = localPath;
            this.type = type;

            CreateTreeView();
            LoadEdit();
            switch (type)
            {
                case EditType.EditFolder:
                    tab.SelectedItem = tabItemSelection;
                    break;
                case EditType.EditCredentials:
                    tab.SelectedItem = tabItemCredentials;
                    break;
                default:
                    break;
            }

            this.Title = Properties_Resources.EditTitle;
            this.Description = "";
            this.ShowAll();

            // Defines how to show the setup window.
            Controller.OpenWindowEvent += delegate
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    Show();
                    Activate();
                    BringIntoView();
                });
            };

            Controller.CloseWindowEvent += delegate
            {
                asyncLoader.Cancel();
            };

            finishButton.Click += delegate
            {
                Ignores = NodeModelUtils.GetIgnoredFolder(repo);
                Credentials.Password = passwordBox.Password;
                Controller.SaveFolder();
                Close();
            };

            cancelButton.Click += delegate
            {
                Close();
            };
        }


        protected override void Close(object sender, CancelEventArgs args)
        {
            Controller.CloseWindow();
        }


        CmisSync.CmisTree.RootFolder repo;
        private AsyncNodeLoader asyncLoader;

        TreeView treeView;
        private TabControl tab;
        private TabItem tabItemSelection;
        private TabItem tabItemCredentials;
        private TextBlock addressLabel;
        private TextBox addressBox;
        private TextBlock userLabel;
        private TextBox userBox;
        private TextBlock passwordLabel;
        private PasswordBox passwordBox;
        private Button finishButton;
        private Button cancelButton;


        private void CreateTreeView()
        {
            System.Uri resourceLocater = new System.Uri("/DataSpaceSync;component/FolderTreeMVC/TreeView.xaml", System.UriKind.Relative);
            treeView = Application.LoadComponent(resourceLocater) as TreeView;

            repo = new CmisSync.CmisTree.RootFolder()
            {
                Name = FolderName,
                Id = Credentials.RepoId,
                Address = Credentials.Address.ToString()
            };
            asyncLoader = new AsyncNodeLoader(repo, Credentials, PredefinedNodeLoader.LoadSubFolderDelegate, PredefinedNodeLoader.CheckSubFolderDelegate);
            IgnoredFolderLoader.AddIgnoredFolderToRootNode(repo, Ignores);
            LocalFolderLoader.AddLocalFolderToRootNode(repo, localPath);

            asyncLoader.Load(repo);

            ObservableCollection<RootFolder> repos = new ObservableCollection<RootFolder>();
            repos.Add(repo);
            repo.Selected = true;

            treeView.DataContext = repos;

            treeView.AddHandler(TreeViewItem.ExpandedEvent, new RoutedEventHandler(delegate(object sender, RoutedEventArgs e)
            {
                TreeViewItem expandedItem = e.OriginalSource as TreeViewItem;
                Node expandedNode = expandedItem.Header as Folder;
                if (expandedNode != null)
                {
                    asyncLoader.Load(expandedNode);
                }
            }));
        }


        private void LoadEdit()
        {
            System.Uri resourceLocater = new System.Uri("/DataSpaceSync;component/EditWPF.xaml", System.UriKind.Relative);
            UserControl editWPF = Application.LoadComponent(resourceLocater) as UserControl;

            tab = editWPF.FindName("tab") as TabControl;
            tabItemSelection = editWPF.FindName("tabItemSelection") as TabItem;
            tabItemCredentials = editWPF.FindName("tabItemCredentials") as TabItem;
            addressLabel = editWPF.FindName("addressLabel") as TextBlock;
            addressBox = editWPF.FindName("addressBox") as TextBox;
            userLabel = editWPF.FindName("userLabel") as TextBlock;
            userBox = editWPF.FindName("userBox") as TextBox;
            passwordLabel = editWPF.FindName("passwordLabel") as TextBlock;
            passwordBox = editWPF.FindName("passwordBox") as PasswordBox;
            finishButton = editWPF.FindName("finishButton") as Button;
            cancelButton = editWPF.FindName("cancelButton") as Button;

            tabItemSelection.Content = treeView;

            addressBox.Text = Credentials.Address.ToString();
            userBox.Text = Credentials.UserName;
            passwordBox.Password = Credentials.Password.ToString();

            ContentCanvas.Children.Add(editWPF);
        }
    }
}
