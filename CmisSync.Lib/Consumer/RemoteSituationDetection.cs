//-----------------------------------------------------------------------
// <copyright file="RemoteSituationDetection.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Consumer
{
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    using log4net;

    /// <summary>
    /// Remote situation detection.
    /// </summary>
    public class RemoteSituationDetection : ISituationDetection<AbstractFolderEvent>
    {
        /// <summary>
        /// Analyse the specified actual event.
        /// </summary>
        /// <param name="storage">Storage of saved MappedObjects.</param>
        /// <param name="actualEvent">Actual event.</param>
        /// <returns>The detected situation type</returns>
        public SituationType Analyse(IMetaDataStorage storage, AbstractFolderEvent actualEvent)
        {
            if (actualEvent.Remote == MetaDataChangeType.NONE && this.IsRemoteObjectDifferentToLastSync(storage, actualEvent)) {
                actualEvent.Remote = MetaDataChangeType.CHANGED;
            }

            SituationType type = this.DoAnalyse(storage, actualEvent);
            return type;
        }

        private SituationType DoAnalyse(IMetaDataStorage storage, AbstractFolderEvent actualEvent)
        {
            switch (actualEvent.Remote)
            {
            case MetaDataChangeType.CREATED:
                if (this.IsChangeEventAHintForMove(storage, actualEvent)) {
                    return SituationType.MOVED;
                }

                if (this.IsChangeEventAHintForRename(storage, actualEvent)) {
                    return SituationType.RENAMED;
                }

                if (actualEvent is FileEvent) {
                    return this.IsSavedFileEqual(storage, (actualEvent as FileEvent).RemoteFile) ? SituationType.NOCHANGE : SituationType.ADDED;
                } else {
                    return SituationType.ADDED;
                }

            case MetaDataChangeType.DELETED:
                return SituationType.REMOVED;
            case MetaDataChangeType.MOVED:
                return SituationType.MOVED;
            case MetaDataChangeType.CHANGED:
                if (this.IsChangeEventAHintForMove(storage, actualEvent)) {
                    return SituationType.MOVED;
                }

                if (this.IsChangeEventAHintForRename(storage, actualEvent)) {
                    return SituationType.RENAMED;
                }

                return SituationType.CHANGED;
            case MetaDataChangeType.NONE:
            default:
                return SituationType.NOCHANGE;
            }
        }

        private bool IsSavedFileEqual(IMetaDataStorage storage, IDocument doc)
        {
            var mappedFile = storage.GetObjectByRemoteId(doc.Id) as IMappedObject;
            if (mappedFile != null &&
               mappedFile.Type == MappedObjectType.File &&
               mappedFile.LastRemoteWriteTimeUtc == doc.LastModificationDate &&
               mappedFile.Name == doc.Name &&
               mappedFile.LastChangeToken == doc.ChangeToken)
            {
                return true;
            } else {
                return false;
            }
        }

        private bool IsChangeEventAHintForMove(IMetaDataStorage storage, AbstractFolderEvent actualEvent)
        {
            if (actualEvent is FolderEvent)
            {
                var folderEvent = actualEvent as FolderEvent;
                var storedFolder = storage.GetObjectByRemoteId(folderEvent.RemoteFolder.Id);
                if (storedFolder != null) {
                    if (storedFolder.ParentId != folderEvent.RemoteFolder.ParentId) {
                        return true;
                    }
                }
            } else if (actualEvent is FileEvent) {
                var fileEvent = actualEvent as FileEvent;
                if (fileEvent.RemoteFile.Parents == null || fileEvent.RemoteFile.Parents.Count == 0) {
                    return false;
                }

                var storedFile = storage.GetObjectByRemoteId(fileEvent.RemoteFile.Id);
                if (storedFile != null && storedFile.ParentId != fileEvent.RemoteFile.Parents[0].Id) {
                    return true;
                }
            }

            return false;
        }

        private bool IsChangeEventAHintForRename(IMetaDataStorage storage, AbstractFolderEvent actualEvent)
        {
            if (actualEvent is FolderEvent) {
                var folderEvent = actualEvent as FolderEvent;
                var storedFolder = storage.GetObjectByRemoteId(folderEvent.RemoteFolder.Id);
                if (storedFolder != null) {
                    return storedFolder.Name != folderEvent.RemoteFolder.Name;
                }
            }
            else if (actualEvent is FileEvent) {
                var fileEvent = actualEvent as FileEvent;
                var storedFile = storage.GetObjectByRemoteId(fileEvent.RemoteFile.Id);
                if (storedFile != null) {
                    return storedFile.Name != fileEvent.RemoteFile.Name;
                }
            }

            return false;
        }

        private bool IsRemoteObjectDifferentToLastSync(IMetaDataStorage storage, AbstractFolderEvent actualEvent) {
            try {
                if (actualEvent is FileEvent) {
                    var obj = storage.GetObjectByRemoteId((actualEvent as FileEvent).RemoteFile.Id);
                    return obj != null && obj.LastChangeToken != (actualEvent as FileEvent).RemoteFile.ChangeToken;
                } else if (actualEvent is FolderEvent) {
                    var obj = storage.GetObjectByRemoteId((actualEvent as FolderEvent).RemoteFolder.Id);
                    return obj != null && obj.LastChangeToken != (actualEvent as FolderEvent).RemoteFolder.ChangeToken;
                }
            } catch (Exception) {
            }

            return false;
        }
    }
}
