﻿//-----------------------------------------------------------------------
// <copyright file="SimpleFileDownloader.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.FileTransmission
{
    using System;
    using System.IO;
    using System.Security.Cryptography;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Streams;

    using DotCMIS.Client;

    /// <summary>
    /// Simple file downloader.
    /// </summary>
    public class SimpleFileDownloader : IFileDownloader
    {
        private bool disposed = false;

        private object disposeLock = new object();

        /// <summary>
        /// Downloads the file and returns the SHA-1 hash of the content of the saved file
        /// </summary>
        /// <param name="remoteDocument">Remote document.</param>
        /// <param name="localFileStream">Local taget file stream.</param>
        /// <param name="status">Transmission status.</param>
        /// <param name="hashAlg">Hash algoritm, which should be used to calculate hash of the uploaded stream content</param>
        /// <exception cref="IOException">On any disc or network io exception</exception>
        /// <exception cref="DisposeException">If the remote object has been disposed before the dowload is finished</exception>
        /// <exception cref="AbortException">If download is aborted</exception>
        /// <exception cref="CmisException">On exceptions thrown by the CMIS Server/Client</exception>
        public void DownloadFile(IDocument remoteDocument, Stream localFileStream, FileTransmissionEvent status, HashAlgorithm hashAlg)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;

            if (localFileStream.Length > 0) {
                localFileStream.Seek(0, SeekOrigin.Begin);
                while ((len = localFileStream.Read(buffer, 0, buffer.Length)) > 0) {
                    hashAlg.TransformBlock(buffer, 0, len, buffer, 0);
                }
            }

            long offset = localFileStream.Position;
            long? fileLength = remoteDocument.ContentStreamLength;
            if (fileLength <= offset) {
                hashAlg.TransformFinalBlock(new byte[0], 0, 0);
                return;
            }

            DotCMIS.Data.IContentStream contentStream = null;
            if (offset > 0) {
                long remainingBytes = (long)fileLength - offset;
                status.ReportProgress(new TransmissionProgressEventArgs {
                    Length = remoteDocument.ContentStreamLength,
                    ActualPosition = offset
                });
                contentStream = remoteDocument.GetContentStream(remoteDocument.ContentStreamId, offset, remainingBytes);
            } else {
                contentStream = remoteDocument.GetContentStream();
            }

            using (ProgressStream progressStream = new ProgressStream(localFileStream, status))
            using (CryptoStream hashstream = new CryptoStream(progressStream, hashAlg, CryptoStreamMode.Write))
            using (Stream remoteStream = contentStream != null ? contentStream.Stream : new MemoryStream(0))
            {
                status.ReportProgress(new TransmissionProgressEventArgs {
                    Length = remoteDocument.ContentStreamLength,
                    ActualPosition = offset
                });
                while ((len = remoteStream.Read(buffer, 0, buffer.Length)) > 0) {
                    lock(this.disposeLock)
                    {
                        if(this.disposed) {
                            status.ReportProgress(new TransmissionProgressEventArgs { Aborted = true });
                            throw new ObjectDisposedException(status.Path);
                        }

                        hashstream.Write(buffer, 0, len);
                        hashstream.Flush();
                    }
                }
            }
        }

        /// <summary>
        /// Releases all resource used by the <see cref="CmisSync.Lib.FileTransmission.SimpleFileDownloader"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="CmisSync.Lib.FileTransmission.SimpleFileDownloader"/>. The <see cref="Dispose"/> method leaves the
        /// <see cref="CmisSync.Lib.FileTransmission.SimpleFileDownloader"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the
        /// <see cref="CmisSync.Lib.FileTransmission.SimpleFileDownloader"/> so the garbage collector can reclaim the memory
        /// that the <see cref="CmisSync.Lib.FileTransmission.SimpleFileDownloader"/> was occupying.</remarks>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// If disposing equals true, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources
        /// can be disposed.
        /// If disposing equals false, the method has been called by the
        /// runtime from inside the finalizer and you should not reference
        /// other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing">If set to <c>true</c> disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            lock(this.disposeLock)
            {
                // Check to see if Dispose has already been called.
                if(!this.disposed)
                {
                    // Note disposing has been done.
                    this.disposed = true;
                }
            }
        }
    }
}
