//-----------------------------------------------------------------------
// <copyright file="AbstractFolderEvent.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Storage;

    using DotCMIS.Client;

    /// <summary>
    /// Abstract folder event.
    /// </summary>
    public abstract class AbstractFolderEvent : ISyncEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.AbstractFolderEvent"/> class.
        /// </summary>
        public AbstractFolderEvent()
        {
            this.Local = MetaDataChangeType.NONE;
            this.Remote = MetaDataChangeType.NONE;
        }

        /// <summary>
        /// Gets or sets the local change type.
        /// </summary>
        /// <value>The local change type.</value>
        public MetaDataChangeType Local { get; set; }

        /// <summary>
        /// Gets or sets the remote change type.
        /// </summary>
        /// <value>The remote change type.</value>
        public MetaDataChangeType Remote { get; set; }

        /// <summary>
        /// Gets the remote path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public abstract string RemotePath { get; }
    }
}
