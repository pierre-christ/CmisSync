//-----------------------------------------------------------------------
// <copyright file="RemoteObjectFetcher.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Accumulator {
    using System;
    using System.IO;
    using System.Threading;

    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    using log4net;

    /// <summary>
    /// Remote object fetcher.
    /// </summary>
    public class RemoteObjectFetcher : SyncEventHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(RemoteObjectFetcher));
        private IMetaDataStorage storage;
        private ISession session;
        private IOperationContext operationContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteObjectFetcher"/> class.
        /// Fetches remote CMIS Objects and adds them to the handled events.
        /// </summary>
        /// <param name="session">Session to be used.</param>
        /// <param name="storage">Storage to look for mapped objects.</param>
        public RemoteObjectFetcher(ISession session, IMetaDataStorage storage) {
            if (session == null) {
                throw new ArgumentNullException("Session instance is needed , but was null");
            }

            if (storage == null) {
                throw new ArgumentNullException("MetaDataStorage instance is needed, but was null");
            }

            this.session = session;
            this.storage = storage;
            this.operationContext = OperationContextFactory.CreateNonCachingPathIncludingContext(this.session);
        }

        /// <summary>
        /// Handles the specified e.
        /// </summary>
        /// <param name="e">sync events</param>
        /// <returns>Always returns <c>false</c></returns>
        public override bool Handle(ISyncEvent e) {
            if (!(e is FileEvent || e is FolderEvent || e is CrawlRequestEvent)) {
                return false;
            }

            ICmisObject remote = this.GetRemoteObject(e);

            // already there, no need to GetObject
            if (remote != null) {
                return false;
            }

            string id;
            if (e is AbstractFolderEvent && (e as AbstractFolderEvent).Local == MetaDataChangeType.DELETED) {
                id = this.FetchIdFromStorage(e);
            } else {
                id = this.FetchIdFromExtendedAttribute(e);
            }

            if (id != null) {
                Logger.Debug("Fetching remote Object with id " + id);
                try {
                    remote = this.session.GetObject(id, this.operationContext);
                    Logger.Debug("Fetched object " + remote);
                } catch (CmisObjectNotFoundException) {
                    Logger.Debug("Was already deleted on server, could not fetch");
                    return false;
                }

                this.SetRemoteObject(e, remote);
            }

            return false;
        }

        private ICmisObject GetRemoteObject(ISyncEvent e) {
            if (e is FileEvent) {
                return (e as FileEvent).RemoteFile;
            }

            if (e is CrawlRequestEvent) {
                return (e as CrawlRequestEvent).RemoteFolder;
            }

            return (e as FolderEvent).RemoteFolder;
        }

        private void SetRemoteObject(ISyncEvent e, ICmisObject remote) {
            if (e is FileEvent) {
                (e as FileEvent).RemoteFile = remote as IDocument;
            } else if (e is CrawlRequestEvent) {
                (e as CrawlRequestEvent).RemoteFolder = remote as IFolder;
            } else {
                (e as FolderEvent).RemoteFolder = remote as IFolder;
            }
        }

        private string FetchIdFromStorage(ISyncEvent e) {
            IFileSystemInfo path = null;
            if (e is FileEvent) {
                path = (e as FileEvent).LocalFile;
            } else if (e is FolderEvent) {
                path = (e as FolderEvent).LocalFolder;
            }

            if (path != null) {
                IMappedObject savedObject = this.storage.GetObjectByLocalPath(path);
                if (savedObject != null) {
                    return savedObject.RemoteObjectId;
                }
            }

            return null;
        }

        private string FetchIdFromExtendedAttribute(ISyncEvent e) {
            IFileSystemInfo path = null;
            if (e is FileEvent) {
                path = (e as FileEvent).LocalFile;
            } else if (e is CrawlRequestEvent) {
                path = (e as CrawlRequestEvent).LocalFolder;
            } else if (e is FolderEvent) {
                path = (e as FolderEvent).LocalFolder;
            }

            if (path != null && path.Exists) {
                Guid? uuid = null;
                try {
                    uuid = path.Uuid;
                } catch (ExtendedAttributeException ex) {
                    Logger.Debug("Could not read extended attributes from path, do not fetch", ex);
                }

                if (uuid != null) {
                    var mappedObject = this.storage.GetObjectByGuid((Guid)uuid);
                    if (mappedObject != null) {
                        return mappedObject.RemoteObjectId;
                    } else {
                        Logger.Debug("Uuid found in Extended Attribute but not in DataBase, do not fetch");
                    }
                }
            }

            return null;
        }
    }
}
