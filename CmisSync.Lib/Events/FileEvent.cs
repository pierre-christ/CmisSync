//-----------------------------------------------------------------------
// <copyright file="FileEvent.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Events
{
    using System;
    using System.IO;

    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    /// <summary>
    /// File event.
    /// </summary>
    public class FileEvent : AbstractFolderEvent, IFilterableNameEvent, IFilterableRemotePathEvent, IFilterableRemoteObjectEvent, IFilterableLocalPathEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.FileEvent"/> class.
        /// </summary>
        /// <param name='localFile'>
        /// Local file.
        /// </param>
        /// <param name='remoteFile'>
        /// Remote file.
        /// </param>
        public FileEvent(IFileInfo localFile = null, IDocument remoteFile = null)
        {
            if (localFile == null && remoteFile == null)
            {
                throw new ArgumentNullException("Given local or remote file must not be null");
            }

            this.LocalFile = localFile;
            this.RemoteFile = remoteFile;
            this.LocalContent = ContentChangeType.NONE;
            this.RemoteContent = ContentChangeType.NONE;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name {
            get {
                return this.LocalFile != null ? this.LocalFile.Name : this.RemoteFile.Name;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this event contains a directory.
        /// </summary>
        /// <returns><c>true</c> if this instance is directory; otherwise, <c>false</c>.</returns>
        public bool IsDirectory {
            get {
                return false;
            }
        }

        /// <summary>
        /// Gets the remote path.
        /// </summary>
        /// <value>The path.</value>
        public override string RemotePath {
            get {
                if (this.RemoteFile == null) {
                    return null;
                }

                try {
                    if (this.RemoteFile.Paths == null) {
                        return null;
                    }
                } catch (CmisRuntimeException) {
                    return null;
                }

                return this.RemoteFile.Paths.Count > 0 ? this.RemoteFile.Paths[0] : null;
            }
        }

        /// <summary>
        /// Gets the remote object.
        /// </summary>
        /// <value>The remote object.</value>
        public ICmisObject RemoteObject {
            get {
                return this.RemoteFile;
            }
        }

        /// <summary>
        /// Gets or sets the content of the local.
        /// </summary>
        /// <value>
        /// The content of the local.
        /// </value>
        public ContentChangeType LocalContent { get; set; }

        /// <summary>
        /// Gets or sets the content of the remote.
        /// </summary>
        /// <value>
        /// The content of the remote.
        /// </value>
        public ContentChangeType RemoteContent { get; set; }

        /// <summary>
        /// Gets or sets the local file.
        /// </summary>
        /// <value>
        /// The local file.
        /// </value>
        public IFileInfo LocalFile { get; set; }

        /// <summary>
        /// Gets or sets the remote file.
        /// </summary>
        /// <value>
        /// The remote file.
        /// </value>
        public IDocument RemoteFile { get; set; }

        /// <summary>
        /// Gets the local path.
        /// </summary>
        /// <value>The local path.</value>
        public string LocalPath {
            get {
                return this.LocalFile != null ? this.LocalFile.FullName : null;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.FileEvent"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.FileEvent"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(
                "[FileEvent: Local={0}, LocalContent={1} on {2}, Remote={3}, RemoteContent={4}]",
                this.Local,
                this.LocalContent,
                this.Name,
                this.Remote,
                this.RemoteContent);
        }
    }
}
