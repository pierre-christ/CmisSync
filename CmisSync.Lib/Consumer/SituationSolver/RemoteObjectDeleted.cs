//-----------------------------------------------------------------------
// <copyright file="RemoteObjectDeleted.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Consumer.SituationSolver {
    using System;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Filter;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    /// <summary>
    /// Remote object has been deleted. => Delete the corresponding local object as well.
    /// </summary>
    public class RemoteObjectDeleted : AbstractEnhancedSolver {
        private IFilterAggregator filters;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Consumer.SituationSolver.RemoteObjectDeleted"/> class.
        /// </summary>
        /// <param name="session">Cmis session.</param>
        /// <param name="storage">Meta data storage.</param>
        public RemoteObjectDeleted(ISession session, IMetaDataStorage storage, IFilterAggregator filters) : base(session, storage) {
            if (filters == null) {
                throw new ArgumentNullException("Given filter aggregator is null");
            }

            this.filters = filters;
        }

        /// <summary>
        /// Deletes the given localFileInfo on file system and removes the stored object from storage.
        /// </summary>
        /// <param name="localFileInfo">Local file info.</param>
        /// <param name="remoteId">Remote identifier.</param>
        /// <param name="localContent">Hint if the local content has been changed.</param>
        /// <param name="remoteContent">Information if the remote content has been changed.</param>
        public override void Solve(
            IFileSystemInfo localFileInfo,
            IObjectId remoteId,
            ContentChangeType localContent = ContentChangeType.NONE,
            ContentChangeType remoteContent = ContentChangeType.NONE)
        {
            if (localFileInfo is IDirectoryInfo) {
                if (!this.DeleteLocalObjectIfHasBeenSyncedBefore(localFileInfo)) {
                    this.Storage.RemoveObject(this.Storage.GetObjectByLocalPath(localFileInfo));
                    throw new IOException(string.Format("Not all local objects under {0} have been synced yet", localFileInfo.FullName));
                } else {
                    this.Storage.RemoveObject(this.Storage.GetObjectByLocalPath(localFileInfo));
                }
            } else if (localFileInfo is IFileInfo) {
                var file = localFileInfo as IFileInfo;
                file.Refresh();
                var mappedFile = this.Storage.GetObjectByLocalPath(file);
                if (file.Exists) {
                    if (mappedFile != null && file.LastWriteTimeUtc.Equals(mappedFile.LastLocalWriteTimeUtc)) {
                        file.Delete();
                        OperationsLogger.Info(string.Format("Deleted local file {0} because the mapped remote object {0} has been deleted", file.FullName, mappedFile.RemoteObjectId));
                    } else {
                        file.Uuid = null;
                        if (mappedFile == null) {
                            return;
                        }

                        OperationsLogger.Info(string.Format("Deletion of local file {0} skipped because of not yet uploaded changes", file.FullName));
                    }
                } else {
                    if (mappedFile == null) {
                        return;
                    } else {
                        OperationsLogger.Info(string.Format("Deletion of local file {0} skipped because it has already been deleted", file.FullName));
                    }
                }

                this.Storage.RemoveObject(this.Storage.GetObjectByLocalPath(localFileInfo));
                file.Refresh();
                if (file.Exists) {
                    throw new IOException(string.Format("Deletion of local file {0} skipped because of not yet uploaded changes", localFileInfo.FullName));
                }
            }
        }

        private bool DeleteLocalObjectIfHasBeenSyncedBefore(IFileSystemInfo fsInfo) {
            bool delete = true;
            string reason;
            Guid? uuid = fsInfo.Uuid;
            IMappedObject obj = null;
            if (uuid != null) {
                obj = this.Storage.GetObjectByGuid((Guid)uuid);
            } else {
                obj = this.Storage.GetObjectByLocalPath(fsInfo);
            }

            if (fsInfo is IFileInfo) {
                if (obj != null && fsInfo.LastWriteTimeUtc.Equals(obj.LastLocalWriteTimeUtc)) {
                    (fsInfo as IFileInfo).Delete();
                    OperationsLogger.Info(string.Format("Deleted local file {0} because the mapped remote object {0} has been deleted", fsInfo.FullName, obj.RemoteObjectId));
                } else {
                    fsInfo.Uuid = null;
                    return false;
                }
            } else if (fsInfo is IDirectoryInfo) {
                var dir = fsInfo as IDirectoryInfo;
                if (obj != null && obj.Ignored) {
                    dir.Delete(true);
                    return true;
                }

                if (!this.filters.FolderNamesFilter.CheckFolderName(dir.Name, out reason)) {
                    foreach (var folder in dir.GetDirectories()) {
                        if (!this.DeleteLocalObjectIfHasBeenSyncedBefore(folder)) {
                            delete = false;
                        }
                    }

                    foreach (var file in dir.GetFiles()) {
                        if (!this.filters.FileNamesFilter.CheckFile(file.Name, out reason)) {
                            if (!this.DeleteLocalObjectIfHasBeenSyncedBefore(file)) {
                                delete = false;
                            }
                        } else {
                            file.Delete();
                            OperationsLogger.Info(string.Format("Deleted local ignored file {0} because the mapped remote parent object {0} has been deleted", fsInfo.FullName, obj.RemoteObjectId));
                        }
                    }

                    if (delete) {
                        try {
                            (fsInfo as IDirectoryInfo).Delete(false);
                            OperationsLogger.Info(string.Format("Deleted local folder {0} because the mapped remote folder has been deleted", fsInfo.FullName));
                        } catch (IOException) {
                            fsInfo.Uuid = null;
                            return false;
                        }
                    } else {
                        fsInfo.Uuid = null;
                    }
                } else {
                    try {
                        (fsInfo as IDirectoryInfo).Delete(true);
                        OperationsLogger.Info(string.Format("Deleted locally ignored folder {0} because the parent mapped remote folder has been deleted", fsInfo.FullName));
                    } catch (IOException e) {
                        OperationsLogger.Info(string.Format("Deletion of locally ignored folder {0} failed", fsInfo.FullName), e);
                        return false;
                    }
                }
            }

            return delete;
        }
    }
}