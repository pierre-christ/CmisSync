//-----------------------------------------------------------------------
// <copyright file="RenamedFileSystemEventHandler.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Producer.Watcher
{
    using System;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.FileSystem;

    using log4net;

    /// <summary>
    /// Renamed file system event handler.
    /// </summary>
    public class RenamedFileSystemEventHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(RenamedFileSystemEventHandler));

        private ISyncEventQueue queue;
        private IFileSystemInfoFactory fsFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenamedFileSystemEventHandler"/> class.
        /// </summary>
        /// <param name="queue">Sync event queue to report the events to.</param>
        /// <param name="fsFactory">File system factory.</param>
        public RenamedFileSystemEventHandler(ISyncEventQueue queue, IFileSystemInfoFactory fsFactory = null)
        {
            if (queue == null) {
                throw new ArgumentNullException("Given queue is null");
            }

            this.queue = queue;
            this.fsFactory = fsFactory ?? new FileSystemInfoFactory();
        }

        /// <summary>
        /// Takes rename file system events and transforms them to rename events on queue.
        /// </summary>
        /// <param name="source">source object</param>
        /// <param name="e">Rename event from file system watcher</param>
        public virtual void Handle(object source, RenamedEventArgs e) {
            try {
                bool? isDirectory = this.fsFactory.IsDirectory(e.FullPath);

                if (isDirectory == null) {
                    this.queue.AddEvent(new StartNextSyncEvent(true));
                    return;
                }

                this.queue.AddEvent(new FSMovedEvent(e.OldFullPath, e.FullPath, (bool)isDirectory));
            } catch (Exception ex) {
                Logger.Warn(string.Format("Processing RenamedEventArgs {0} produces Exception => force crawl sync", e.ToString()), ex);
                this.queue.AddEvent(new StartNextSyncEvent(true));
            }
        }
    }
}