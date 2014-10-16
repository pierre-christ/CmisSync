//-----------------------------------------------------------------------
// <copyright file="SelectiveIgnoreFilter.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.SelectiveIgnore
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Filter;
    using CmisSync.Lib.Queueing;

    using DotCMIS.Client;

    /// <summary>
    /// Selective ignore filter.
    /// All file/folder events for affecting files/folders which are inside an ignored folder are filtered out.
    /// </summary>
    public class SelectiveIgnoreFilter : SyncEventHandler
    {
        private ObservableCollection<IIgnoredEntity> ignores;
        private ISession session;

        public SelectiveIgnoreFilter(ObservableCollection<IIgnoredEntity> ignores, ISession session) {
            if (ignores == null) {
                throw new ArgumentNullException("The collection of ignored entities is null");
            }

            if (session == null) {
                throw new ArgumentNullException("The given session is null");
            }

            this.ignores = ignores;
            this.session = session;
        }

        public override bool Handle(ISyncEvent e)
        {
            if (e is IFilterableRemoteObjectEvent) {
                var ev = e as IFilterableRemoteObjectEvent;
                if (ev.RemoteObject is IFolder) {
                    var folder = ev.RemoteObject as IFolder;
                    var parent = this.session.GetObject(folder.ParentId);
                    while (parent != null && parent is IFolder) {
                        parent = this.session.GetObject((parent as IFolder).ParentId);
                    }
                }
            }

            if (e is IFilterableLocalPathEvent) {
                var path = (e as IFilterableLocalPathEvent).LocalPath;
                foreach(var ignore in this.ignores) {
                    if (path.StartsWith(ignore.LocalPath)) {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}