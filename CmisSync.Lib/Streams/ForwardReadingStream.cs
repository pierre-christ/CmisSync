//-----------------------------------------------------------------------
// <copyright file="ForwardReadingStream.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Streams
{
    using System;
    using System.IO;

    /// <summary>
    /// Forward reading stream.
    /// </summary>
    public class ForwardReadingStream : StreamWrapper
    {
        /// <summary>
        /// The position.
        /// </summary>
        private long pos = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Streams.ForwardReadingStream"/> class.
        /// </summary>
        /// <param name='nonSeekableStream'>
        /// Non seekable stream.
        /// </param>
        public ForwardReadingStream(Stream nonSeekableStream) : base(nonSeekableStream)
        {
        }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>
        /// The position in the stream.
        /// </value>
        public override long Position
        {
            get
            {
                return this.pos;
            }

            set 
            {
                base.Position = value;
                this.pos = value;
            }
        }

        /// <summary>
        /// Read the specified buffer, offset and count.
        /// </summary>
        /// <param name='buffer'>
        /// Buffer.
        /// </param>
        /// <param name='offset'>
        /// Offset.
        /// </param>
        /// <param name='count'>
        /// Count.
        /// </param>
        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = base.Read(buffer, offset, count);
            this.pos += read;
            return read;
        }
    }
}
