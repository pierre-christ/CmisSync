//-----------------------------------------------------------------------
// <copyright file="DescendantsCrawler.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Sync.Strategy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;
    using DotCMIS.Client;

    using log4net;

    /// <summary>
    /// Decendants crawler.
    /// </summary>
    public class DescendantsCrawler : ReportingSyncEventHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DescendantsCrawler));
        private IFolder remoteFolder;
        private IDirectoryInfo localFolder;
        private IMetaDataStorage storage;
        private IFileSystemInfoFactory fsFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Sync.Strategy.DescendantsCrawler"/> class.
        /// </summary>
        /// <param name="queue">Sync Event Queue.</param>
        /// <param name="remoteFolder">Remote folder.</param>
        /// <param name="localFolder">Local folder.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="fsFactory">File system info factory.</param>
        public DescendantsCrawler(
            ISyncEventQueue queue,
            IFolder remoteFolder,
            IDirectoryInfo localFolder,
            IMetaDataStorage storage,
            IFileSystemInfoFactory fsFactory = null)
            : base(queue)
        {
            if (remoteFolder == null) {
                throw new ArgumentNullException("Given remoteFolder is null");
            }

            if (localFolder == null) {
                throw new ArgumentNullException("Given localFolder is null");
            }

            if (storage == null) {
                throw new ArgumentNullException("Given storage is null");
            }

            this.storage = storage;
            this.remoteFolder = remoteFolder;
            this.localFolder = localFolder;

            if (fsFactory == null) {
                this.fsFactory = new FileSystemInfoFactory();
            } else {
                this.fsFactory = fsFactory;
            }
        }

        /// <summary>
        /// Handles StartNextSync events.
        /// </summary>
        /// <param name="e">The event to handle.</param>
        /// <returns>true if handled</returns>
        public override bool Handle(ISyncEvent e)
        {
            if(e is StartNextSyncEvent) {
                Logger.Debug("Starting DecendantsCrawlSync upon " + e);
                this.CrawlDescendants();
                this.Queue.AddEvent(new FullSyncCompletedEvent(e as StartNextSyncEvent));
                return true;
            }

            return false;
        }

        private static AbstractFolderEvent GetCorrespondingRemoteEvent(Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>> eventMap, IMappedObject storedMappedChild)
        {
            AbstractFolderEvent correspondingRemoteEvent = null;
            Tuple<AbstractFolderEvent, AbstractFolderEvent> tuple;
            if (eventMap.TryGetValue(storedMappedChild.RemoteObjectId, out tuple))
            {
                correspondingRemoteEvent = tuple.Item2;
            }

            return correspondingRemoteEvent;
        }

        private void CrawlDescendants()
        {
            IObjectTree<IMappedObject> storedTree = null;
            IObjectTree<IFileSystemInfo> localTree = null;
            IObjectTree<IFileableCmisObject> remoteTree = null;

            // Request 3 trees in parallel
            Task[] tasks = new Task[3];
            tasks[0] = Task.Factory.StartNew(() => storedTree = this.storage.GetObjectTree());
            tasks[1] = Task.Factory.StartNew(() => localTree = GetLocalDirectoryTree(this.localFolder));
            tasks[2] = Task.Factory.StartNew(() => remoteTree = GetRemoteDirectoryTree(this.remoteFolder, this.remoteFolder.GetDescendants(-1)));

            // Wait until all tasks are finished
            Task.WaitAll(tasks);

            /*
            storedTree = this.storage.GetObjectTree();
            localTree = GetLocalDirectoryTree(this.localFolder);
            var desc = this.remoteFolder.GetDescendants(-1);
            remoteTree = GetRemoteDirectoryTree(this.remoteFolder, desc);*/

            List<IMappedObject> storedObjectsForRemote = storedTree.ToList();
            List<IMappedObject> storedObjectsForLocal = new List<IMappedObject>(storedObjectsForRemote);
            Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>> eventMap = new Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>>();
            this.CreateRemoteEvents(storedObjectsForRemote, remoteTree, eventMap);
            this.CreateLocalEvents(storedObjectsForLocal, localTree, eventMap);

            Dictionary<string, IFileSystemInfo> removedLocalObjects = new Dictionary<string, IFileSystemInfo>();
            Dictionary<string, IFileSystemInfo> removedRemoteObjects = new Dictionary<string, IFileSystemInfo>();

            storedObjectsForLocal.Remove(storedTree.Item);
            storedObjectsForRemote.Remove(storedTree.Item);

            foreach (var localDeleted in storedObjectsForLocal) {
                string path = this.storage.GetLocalPath(localDeleted);
                IFileSystemInfo info = localDeleted.Type == MappedObjectType.File ? (IFileSystemInfo)this.fsFactory.CreateFileInfo(path) : (IFileSystemInfo)this.fsFactory.CreateDirectoryInfo(path);
                removedLocalObjects.Add(localDeleted.RemoteObjectId, info);
            }

            foreach (var remoteDeleted in storedObjectsForRemote) {
                string path = this.storage.GetLocalPath(remoteDeleted);
                IFileSystemInfo info = remoteDeleted.Type == MappedObjectType.File ? (IFileSystemInfo)this.fsFactory.CreateFileInfo(path) : (IFileSystemInfo)this.fsFactory.CreateDirectoryInfo(path);
                removedRemoteObjects.Add(remoteDeleted.RemoteObjectId, info);
            }

            this.MergeAndSendEvents(eventMap);
            
            this.FindReportAndRemoveMutualDeletedObjects(removedRemoteObjects, removedLocalObjects);

            // Send out Events to queue
            this.InformAboutRemoteObjectsDeleted(removedRemoteObjects.Values);
            this.InformAboutLocalObjectsDeleted(removedLocalObjects.Values);
        }

        private void CreateLocalEvents(
            List<IMappedObject> storedObjects,
            IObjectTree<IFileSystemInfo> localTree,
            Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>> eventMap)
        {
            var parent = localTree.Item;
            IMappedObject storedParent = null;
            Guid guid;

            if (this.TryGetExtendedAttribute(parent, out guid)) {
                storedParent = storedObjects.Find(o => o.Guid.Equals(guid));
            }

            foreach (var child in localTree.Children) {
                Guid childGuid;
                IMappedObject storedMappedChild = null;
                if (this.TryGetExtendedAttribute(child.Item, out childGuid)) {
                    storedMappedChild = storedObjects.Find(o => o.Guid == childGuid);
                    if (storedMappedChild != null) {
                        // Moved, Renamed, Updated or Equal
                        AbstractFolderEvent correspondingRemoteEvent = GetCorrespondingRemoteEvent(eventMap, storedMappedChild);

                        if (storedMappedChild.ParentId == storedParent.RemoteObjectId) {
                            // Renamed, Updated or Equal
                            if (child.Item.Name == storedMappedChild.Name) {
                                // Updated or Equal
                                if (child.Item.LastWriteTimeUtc != storedMappedChild.LastLocalWriteTimeUtc) {
                                    // Updated
                                    AbstractFolderEvent updateEvent = FileOrFolderEventFactory.CreateEvent(null, child.Item, localChange: MetaDataChangeType.CHANGED, src: this);
                                    eventMap[storedMappedChild.RemoteObjectId] = new Tuple<AbstractFolderEvent, AbstractFolderEvent>(updateEvent, correspondingRemoteEvent);
                                } else {
                                    // Equal
                                    AbstractFolderEvent equalEvent = FileOrFolderEventFactory.CreateEvent(null, child.Item, src: this);
                                    eventMap[storedMappedChild.RemoteObjectId] = new Tuple<AbstractFolderEvent, AbstractFolderEvent>(equalEvent, correspondingRemoteEvent);
                                }
                            } else {
                                // Renamed
                                AbstractFolderEvent renamedEvent = FileOrFolderEventFactory.CreateEvent(null, child.Item, localChange: MetaDataChangeType.CHANGED, src: this);
                                eventMap[storedMappedChild.RemoteObjectId] = new Tuple<AbstractFolderEvent, AbstractFolderEvent>(renamedEvent, correspondingRemoteEvent);
                            }
                        } else {
                            // Moved
                            IFileSystemInfo oldLocalPath = child.Item is IFileInfo ? (IFileSystemInfo)this.fsFactory.CreateFileInfo(this.storage.GetLocalPath(storedMappedChild)) : (IFileSystemInfo)this.fsFactory.CreateDirectoryInfo(this.storage.GetLocalPath(storedMappedChild));
                            AbstractFolderEvent movedEvent = FileOrFolderEventFactory.CreateEvent(null, child.Item, localChange: MetaDataChangeType.MOVED, oldLocalObject: oldLocalPath, src: this);
                            eventMap[storedMappedChild.RemoteObjectId] = new Tuple<AbstractFolderEvent, AbstractFolderEvent>(movedEvent, correspondingRemoteEvent);
                        }
                    } else {
                        // Added
                        AbstractFolderEvent addEvent = FileOrFolderEventFactory.CreateEvent(null, child.Item, localChange: MetaDataChangeType.CREATED, src: this);
                        Queue.AddEvent(addEvent);
                    }
                } else {
                    // Added
                    AbstractFolderEvent addEvent = FileOrFolderEventFactory.CreateEvent(null, child.Item, localChange: MetaDataChangeType.CREATED, src: this);
                    Queue.AddEvent(addEvent);
                }

                this.CreateLocalEvents(storedObjects, child, eventMap);
                if (storedMappedChild != null) {
                    storedObjects.Remove(storedMappedChild);
                }
            }
        }

        private void CreateRemoteEvents(List<IMappedObject> storedObjects, IObjectTree<IFileableCmisObject> remoteTree, Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>> eventMap)
        {
            var storedParent = storedObjects.Find(o => o.RemoteObjectId == remoteTree.Item.Id);

            foreach (var child in remoteTree.Children) {
                var storedMappedChild = storedObjects.Find(o => o.RemoteObjectId == child.Item.Id);
                if (storedMappedChild != null) {
                    if (storedMappedChild.ParentId == storedParent.RemoteObjectId) {
                        // Renamed or Equal
                        if (storedMappedChild.Name == child.Item.Name) {
                            // Equal or property update
                            if (storedMappedChild.LastChangeToken != child.Item.ChangeToken) {
                                // Update
                                AbstractFolderEvent updateEvent = FileOrFolderEventFactory.CreateEvent(child.Item, null, MetaDataChangeType.CHANGED, src: this);
                                eventMap[child.Item.Id] = new Tuple<AbstractFolderEvent, AbstractFolderEvent>(null, updateEvent);
                            } else {
                                // Equal
                                AbstractFolderEvent noChangeEvent = FileOrFolderEventFactory.CreateEvent(child.Item, null, MetaDataChangeType.NONE, src: this);
                                eventMap[child.Item.Id] = new Tuple<AbstractFolderEvent, AbstractFolderEvent>(null, noChangeEvent);
                            }
                        } else {
                            // Renamed
                            AbstractFolderEvent renameEvent = FileOrFolderEventFactory.CreateEvent(child.Item, null, MetaDataChangeType.CHANGED, src: this);
                            eventMap[child.Item.Id] = new Tuple<AbstractFolderEvent, AbstractFolderEvent>(null, renameEvent);
                        }
                    } else {
                        // Moved
                        AbstractFolderEvent movedEvent = FileOrFolderEventFactory.CreateEvent(child.Item, null, MetaDataChangeType.MOVED, oldRemotePath: this.storage.GetRemotePath(storedMappedChild), src: this);
                        eventMap[child.Item.Id] = new Tuple<AbstractFolderEvent, AbstractFolderEvent>(null, movedEvent);
                    }
                } else {
                    // Added
                    AbstractFolderEvent addEvent = FileOrFolderEventFactory.CreateEvent(child.Item, null, MetaDataChangeType.CREATED, src: this);
                    this.Queue.AddEvent(addEvent);
                }

                this.CreateRemoteEvents(storedObjects, child, eventMap);
                if (storedMappedChild != null) {
                    storedObjects.Remove(storedMappedChild);
                }
            }
        }

        private bool TryGetExtendedAttribute(IFileSystemInfo fsInfo, out Guid guid) {
            string ea = fsInfo.GetExtendedAttribute(MappedObject.ExtendedAttributeKey);
            if (!string.IsNullOrEmpty(ea) &&
                Guid.TryParse(ea, out guid)) {
                return true;
            } else {
                guid = Guid.Empty;
                return false;
            }
        }

        private void MergeAndSendEvents(Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>> eventMap)
        {
            foreach (var entry in eventMap) {
                if (entry.Value == null) {
                    continue;
                } else if (entry.Value.Item1 == null && entry.Value.Item2 == null) {
                    continue;
                } else if (entry.Value.Item1 == null) {
                    this.Queue.AddEvent(entry.Value.Item2);
                } else if (entry.Value.Item2 == null) {
                    this.Queue.AddEvent(entry.Value.Item1);
                } else {
                    var localEvent = entry.Value.Item1;
                    var remoteEvent = entry.Value.Item2;
                    var newEvent = FileOrFolderEventFactory.CreateEvent(
                        remoteEvent is FileEvent ? (IFileableCmisObject)(remoteEvent as FileEvent).RemoteFile : (IFileableCmisObject)(remoteEvent as FolderEvent).RemoteFolder,
                        localEvent is FileEvent ? (IFileSystemInfo)(localEvent as FileEvent).LocalFile : (IFileSystemInfo)(localEvent as FolderEvent).LocalFolder,
                        remoteEvent.Remote,
                        localEvent.Local,
                        remoteEvent.Remote == MetaDataChangeType.MOVED ? (remoteEvent is FileMovedEvent ? (remoteEvent as FileMovedEvent).OldRemoteFilePath : (remoteEvent as FolderMovedEvent).OldRemoteFolderPath) : null,
                        localEvent.Local == MetaDataChangeType.MOVED ? (localEvent is FileMovedEvent ? (IFileSystemInfo)(localEvent as FileMovedEvent).OldLocalFile : (IFileSystemInfo)(localEvent as FolderMovedEvent).OldLocalFolder) : null,
                        this);
                    this.Queue.AddEvent(newEvent);
                }
            }
        }

        private void InformAboutRemoteObjectsAdded(IList<IFileableCmisObject> objects) {
            foreach (var addedRemotely in objects) {
                if (addedRemotely is IFolder) {
                    this.Queue.AddEvent(new FolderEvent(null, addedRemotely as IFolder, this) { Remote = MetaDataChangeType.CREATED });
                } else if (addedRemotely is IDocument) {
                    this.Queue.AddEvent(new FileEvent(null, addedRemotely as IDocument) { Remote = MetaDataChangeType.CREATED });
                }
            }
        }

        private void InformAboutLocalObjectsAdded(IList<IFileSystemInfo> objects) {
            foreach (var addedLocally in objects) {
                if (addedLocally is IDirectoryInfo) {
                    this.Queue.AddEvent(new FolderEvent(addedLocally as IDirectoryInfo, null, this) { Local = MetaDataChangeType.CREATED });
                } else if (addedLocally is IFileInfo) {
                    this.Queue.AddEvent(new FileEvent(addedLocally as IFileInfo, null) { Local = MetaDataChangeType.CREATED });
                }
            }
        }

        private void InformAboutLocalObjectsDeleted(IEnumerable<IFileSystemInfo> objects) {
            foreach (var deleted in objects) {
                if (deleted is IDirectoryInfo) {
                    this.Queue.AddEvent(new FolderEvent(deleted as IDirectoryInfo, null, this) { Local = MetaDataChangeType.DELETED });
                } else if (deleted is IFileInfo) {
                    this.Queue.AddEvent(new FileEvent(deleted as IFileInfo) { Local = MetaDataChangeType.DELETED });
                }
            }
        }

        private void InformAboutRemoteObjectsDeleted(IEnumerable<IFileSystemInfo> objects) {
            foreach (var deleted in objects) {
                AbstractFolderEvent deletedEvent = FileOrFolderEventFactory.CreateEvent(null, deleted, MetaDataChangeType.DELETED, src: this);
                this.Queue.AddEvent(deletedEvent);
            }
        }
        
        private void FindReportAndRemoveMutualDeletedObjects(IDictionary<string, IFileSystemInfo> removedRemoteObjects, IDictionary<string, IFileSystemInfo> removedLocalObjects) {
            IEnumerable<string> intersect = removedRemoteObjects.Keys.Intersect(removedLocalObjects.Keys);
            IList<string> mutualIds = new List<string>();
            foreach (var id in intersect) {
                AbstractFolderEvent deletedEvent = FileOrFolderEventFactory.CreateEvent(null, removedLocalObjects[id], MetaDataChangeType.DELETED, MetaDataChangeType.DELETED, src: this);
                mutualIds.Add(id);
                this.Queue.AddEvent(deletedEvent);
            }

            foreach(var id in mutualIds) {
                removedLocalObjects.Remove(id);
                removedRemoteObjects.Remove(id);
            }
        }

        public static IObjectTree<IFileSystemInfo> GetLocalDirectoryTree(IDirectoryInfo parent) {
            var children = new List<IObjectTree<IFileSystemInfo>>();
            foreach (var child in parent.GetDirectories()) {
                children.Add(GetLocalDirectoryTree(child));
            }

            foreach (var file in parent.GetFiles()) {
                children.Add(new ObjectTree<IFileSystemInfo> {
                    Item = file,
                    Children = new List<IObjectTree<IFileSystemInfo>>()
                });
            }

            IObjectTree<IFileSystemInfo> tree = new ObjectTree<IFileSystemInfo> {
                Item = parent,
                Children = children
            };
            return tree;
        }

        public static IObjectTree<IFileableCmisObject> GetRemoteDirectoryTree(IFolder parent, IList<ITree<IFileableCmisObject>> descendants)
        {
            IList<IObjectTree<IFileableCmisObject>> children = new List<IObjectTree<IFileableCmisObject>>();
            if (descendants != null) {
                foreach (var child in descendants) {
                    if(child.Item is IFolder) {
                        children.Add(GetRemoteDirectoryTree(child.Item as IFolder, child.Children));
                    } else if(child.Item is IDocument) {
                        children.Add(new ObjectTree<IFileableCmisObject> {
                            Item = child.Item,
                            Children = new List<IObjectTree<IFileableCmisObject>>()
                        });
                    }
                }
            }

            var tree = new ObjectTree<IFileableCmisObject> {
                Item = parent,
                Children = children
            };

            return tree;
        }
    }
}