//-----------------------------------------------------------------------
// <copyright file="CmisTreeStore.cs" company="GRAU DATA AG">
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

namespace CmisSync.CmisTree
{
    using System;

    using Gtk;

    [CLSCompliant(false)]
    public class CmisTreeStore : TreeStore
    {
        public enum Column : int
        {
            ColumnNode = 0,
            ColumnName = 1,
            ColumnRoot = 2,
            ColumnSelected = 3,
            ColumnSelectedThreeState = 4,
            ColumnStatus = 5,
            NumberColumn = 6,
        };
  

        private object lockCmisStore = new object();

        public CmisTreeStore() : base(typeof(Node), typeof(string), typeof(bool), typeof(bool), typeof(bool), typeof(string))
        {
        }

        public void UpdateCmisTree(RootFolder root)
        {
            lock (this.lockCmisStore)
            {
                TreeIter iter;
                if (this.GetIterFirst(out iter))
                {
                    do
                    {
                        string name = this.GetValue(iter, (int)Column.ColumnName) as string;
                        if (name == null)
                        {
                            Console.WriteLine("UpdateCmisTree GetValue Error");
                            return;
                        }

                        if (name == root.Name)
                        {
                            this.UpdateCmisTreeNode(iter, root);
                            return;
                        }
                    } while (this.IterNext(ref iter));
                }

                iter = this.AppendNode();
                this.UpdateCmisTreeNode(iter, root);
                return;
            }
        }

        private void UpdateCmisTreeNode(TreeIter iter, Node node)
        {
//            Node oldNode = CmisStore.GetValue (iter, (int)Column.ColumnNode) as Node;
//            if (oldNode != node)
//            {
//                CmisStore.SetValue (iter, (int)Column.ColumnNode, node);
//            }
//            string oldName = CmisStore.GetValue (iter, (int)Column.ColumnName) as string;
//            string newName = node.Name;
//            if (oldName != newName)
//            {
//                CmisStore.SetValue (iter, (int)Column.ColumnName, newName);
//            }
//            bool oldRoot = (bool)CmisStore.GetValue (iter, (int)Column.ColumnRoot);
//            bool newRoot = (node.Parent == null);
//            if (oldRoot != newRoot)
//            {
//                CmisStore.SetValue (iter, (int)Column.ColumnRoot, newRoot);
//            }
//            bool oldSelected = (bool)CmisStore.GetValue (iter, (int)Column.ColumnSelected);
//            bool newSelected = (node.Selected != false);
//            if (oldSelected != newSelected)
//            {
//                CmisStore.SetValue (iter, (int)Column.ColumnSelected, newSelected);
//            }
//            bool oldSelectedThreeState = (bool)CmisStore.GetValue (iter, (int)Column.ColumnSelectedThreeState);
//            bool newSelectedThreeState = (node.Selected == null);
//            if (oldSelectedThreeState != newSelectedThreeState)
//            {
//                CmisStore.SetValue (iter, (int)Column.ColumnSelectedThreeState, newSelectedThreeState);
//            }
//            string oldStatus = CmisStore.GetValue (iter, (int)Column.ColumnStatus) as string;
            string newStatus = string.Empty;
            switch (node.Status) {
            case LoadingStatus.START:
                newStatus = Properties_Resources.LoadingStatusSTART;
                break;
            case LoadingStatus.LOADING:
                newStatus = Properties_Resources.LoadingStatusLOADING;
                break;
            case LoadingStatus.ABORTED:
                newStatus = Properties_Resources.LoadingStatusABORTED;
                break;
            default:
                newStatus = string.Empty;
                break;
            }

/*          if (oldStatus != newStatus)
            {
                CmisStore.SetValue (iter, (int)Column.ColumnStatus, newStatus);
            }*/
            this.SetValues(iter, node, node.Name, node.Parent == null, node.Selected != false, node.Selected == null, newStatus);
            foreach (Node child in node.Children)
            {
                TreeIter iterChild;
                this.GetChild(iter, out iterChild, child);
                this.UpdateCmisTreeNode(iterChild, child);
            }

            return;
        }

        private void GetChild(TreeIter iterParent, out TreeIter iterChild, Node child)
        {
            TreeIter iter;
            if (this.IterChildren(out iter, iterParent))
            {
                do
                {
                    string name = this.GetValue(iter, (int)Column.ColumnName) as string;
                    Node node = this.GetValue(iter, (int)Column.ColumnNode) as Node;
                    if (name == child.Name)
                    {
                        if (node != child)
                        {
                            Console.WriteLine("GetChild Error " + name);
                        }

                        iterChild = iter;
                        return;
                    }
                } while (this.IterNext(ref iter));
            }

            iterChild = this.AppendNode(iterParent);
        }
    }
}