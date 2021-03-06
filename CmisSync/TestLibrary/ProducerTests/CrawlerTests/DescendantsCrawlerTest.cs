//-----------------------------------------------------------------------
// <copyright file="DescendantsCrawlerTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.ProducerTests.CrawlerTests {
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Filter;
    using CmisSync.Lib.PathMatcher;
    using CmisSync.Lib.Producer.Crawler;
    using CmisSync.Lib.Producer.Watcher;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.SelectiveIgnore;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DBreeze;

    using DotCMIS.Client;

    using Moq;

    using Newtonsoft.Json;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class DescendantsCrawlerTest : IDisposable {
        private readonly string remoteRootId = "rootId";
        private readonly string remoteRootPath = "/";
        private readonly Guid rootGuid = Guid.NewGuid();
        private Mock<ISyncEventQueue> queue;
        private IMetaDataStorage storage;
        private Mock<IFolder> remoteFolder;
        private Mock<IDirectoryInfo> localFolder;
        private Mock<IFileSystemInfoFactory> fsFactory;
        private string localRootPath;
        private MappedObject mappedRootObject;
        private IPathMatcher matcher;
        private DBreezeEngine storageEngine;
        private DateTime lastLocalWriteTime = DateTime.Now;
        private IFilterAggregator filter;
        private Mock<IActivityListener> listener;

        [TestFixtureSetUp]
        public void InitCustomSerializator() {
            // Use Newtonsoft.Json as Serializator
            DBreeze.Utils.CustomSerializator.Serializator = JsonConvert.SerializeObject;
            DBreeze.Utils.CustomSerializator.Deserializator = JsonConvert.DeserializeObject;
        }

        [SetUp]
        public void CreateMockObjects() {
            this.storageEngine = new DBreezeEngine(new DBreezeConfiguration { Storage = DBreezeConfiguration.eStorage.MEMORY });
            this.localRootPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            this.matcher = new PathMatcher(this.localRootPath, this.remoteRootPath);
            this.queue = new Mock<ISyncEventQueue>();
            this.remoteFolder = MockOfIFolderUtil.CreateRemoteFolderMock(this.remoteRootId, this.remoteRootPath, this.remoteRootPath);
            this.remoteFolder.SetupDescendants();
            this.localFolder = new Mock<IDirectoryInfo>();
            this.localFolder.Setup(f => f.FullName).Returns(this.localRootPath);
            this.localFolder.Setup(f => f.Exists).Returns(true);
            this.localFolder.Setup(f => f.IsExtendedAttributeAvailable()).Returns(true);
            this.localFolder.SetupGuid(this.rootGuid);
            this.localFolder.Setup(f => f.LastWriteTimeUtc).Returns(this.lastLocalWriteTime);
            this.fsFactory = new Mock<IFileSystemInfoFactory>();
            this.fsFactory.AddIDirectoryInfo(this.localFolder.Object);
            this.mappedRootObject = new MappedObject(
                this.remoteRootPath,
                this.remoteRootId,
                MappedObjectType.Folder,
                null,
                "changeToken") {
                Guid = this.rootGuid,
                LastLocalWriteTimeUtc = this.lastLocalWriteTime
            };
            this.storage = new MetaDataStorage(this.storageEngine, this.matcher);
            this.storage.SaveMappedObject(this.mappedRootObject);
            this.filter = MockOfIFilterAggregatorUtil.CreateFilterAggregator().Object;
            this.listener = new Mock<IActivityListener>();
        }

        [TearDown]
        public void TearDownStorage() {
            this.storageEngine.Dispose();
            this.storageEngine = null;
        }

        [Test, Category("Fast")]
        public void ConstructorThrowsExceptionIfLocalFolderIsNull() {
            Assert.Throws<ArgumentNullException>(() => new DescendantsCrawler(Mock.Of<ISyncEventQueue>(), Mock.Of<IFolder>(), null, Mock.Of<IMetaDataStorage>(), this.filter, this.listener.Object, Mock.Of<IIgnoredEntitiesStorage>()));
        }

        [Test, Category("Fast")]
        public void ConstructorThrowsExceptionIfRemoteFolderIsNull() {
            Assert.Throws<ArgumentNullException>(() => new DescendantsCrawler(Mock.Of<ISyncEventQueue>(), null, Mock.Of<IDirectoryInfo>(), Mock.Of<IMetaDataStorage>(), this.filter, this.listener.Object, Mock.Of<IIgnoredEntitiesStorage>()));
        }

        [Test, Category("Fast")]
        public void ConstructorThrowsExceptionIfQueueIsNull() {
            Assert.Throws<ArgumentNullException>(() => new DescendantsCrawler(null, Mock.Of<IFolder>(), Mock.Of<IDirectoryInfo>(), Mock.Of<IMetaDataStorage>(), this.filter, this.listener.Object, Mock.Of<IIgnoredEntitiesStorage>()));
        }

        [Test, Category("Fast")]
        public void ConstructorThrowsExceptionIfStorageIsNull() {
            Assert.Throws<ArgumentNullException>(() => new DescendantsCrawler(Mock.Of<ISyncEventQueue>(), Mock.Of<IFolder>(), Mock.Of<IDirectoryInfo>(), null, this.filter, this.listener.Object, Mock.Of<IIgnoredEntitiesStorage>()));
        }

        [Test, Category("Fast")]
        public void ConstructorThrowsExceptionIfListenerIsNull() {
            Assert.Throws<ArgumentNullException>(() => new DescendantsCrawler(Mock.Of<ISyncEventQueue>(), Mock.Of<IFolder>(), Mock.Of<IDirectoryInfo>(), Mock.Of<IMetaDataStorage>(), this.filter, null, Mock.Of<IIgnoredEntitiesStorage>()));
        }

        [Test, Category("Fast")]
        public void ConstructorWorksWithoutFsInfoFactory() {
            var localFolder = Mock.Of<IDirectoryInfo>(p => p.FullName == this.localRootPath);
            var remoteFolder = Mock.Of<IFolder>(p => p.Path == this.remoteRootPath);
            new DescendantsCrawler(Mock.Of<ISyncEventQueue>(), remoteFolder, localFolder, Mock.Of<IMetaDataStorage>(), this.filter, this.listener.Object, Mock.Of<IIgnoredEntitiesStorage>());
        }

        [Test, Category("Fast")]
        public void ConstructorTakesFsInfoFactory() {
            this.CreateCrawler();
        }

        [Test, Category("Fast")]
        public void PriorityIsNormal() {
            var crawler = this.CreateCrawler();
            Assert.That(crawler.Priority == EventHandlerPriorities.NORMAL);
        }

        [Test, Category("Fast")]
        public void IgnoresNonFittingEvents()
        {
            var crawler = this.CreateCrawler();
            Assert.That(crawler.Handle(Mock.Of<ISyncEvent>()), Is.False);
            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
            this.listener.Verify(l => l.ActivityStarted(), Times.Never);
        }

        [Test, Category("Fast")]
        public void HandlesStartNextSyncEventAndReportsOnQueueIfDone() {
            var crawler = this.CreateCrawler();
            var startEvent = new StartNextSyncEvent();
            Assert.That(crawler.Handle(startEvent), Is.True);
            this.queue.Verify(q => q.AddEvent(It.Is<FullSyncCompletedEvent>(e => e.StartEvent.Equals(startEvent))), Times.Once());
            this.VerifyThatListenerHasBeenUsed();
        }

        [Test, Category("Fast")]
        public void OneRemoteFolderAdded() {
            IFolder newRemoteFolder = MockOfIFolderUtil.CreateRemoteFolderMock("id", "name", "/name", this.remoteRootId).Object;
            this.remoteFolder.SetupDescendants(newRemoteFolder);
            var crawler = this.CreateCrawler();

            Assert.That(crawler.Handle(new StartNextSyncEvent()), Is.True);
            this.queue.Verify(q => q.AddEvent(It.Is<FolderEvent>(e => e.RemoteFolder.Equals(newRemoteFolder))), Times.Once());
            this.VerifyThatListenerHasBeenUsed();
        }

        [Test, Category("Fast")]
        public void OneRemoteFileAdded() {
            IDocument newRemoteDocument = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, "id", "name", this.remoteRootId).Object;
            this.remoteFolder.SetupDescendants(newRemoteDocument);
            var crawler = this.CreateCrawler();

            Assert.That(crawler.Handle(new StartNextSyncEvent()), Is.True);
            this.queue.Verify(q => q.AddEvent(It.Is<FileEvent>(e => e.RemoteFile.Equals(newRemoteDocument))), Times.Once());
            this.VerifyThatCountOfAddedEventsIsLimitedTo(Times.Once());
            this.VerifyThatListenerHasBeenUsed();
        }

        [Test, Category("Fast")]
        public void OneRemoteFileRemoved() {
            Guid fileGuid = Guid.NewGuid();
            DateTime modificationDate = DateTime.UtcNow;
            MappedObject obj = new MappedObject("name", "id", MappedObjectType.File, this.remoteRootId, "changeToken", 0) {
                Guid = fileGuid,
                LastLocalWriteTimeUtc = modificationDate
            };
            var oldLocalFile = this.fsFactory.AddFile(Path.Combine(this.localRootPath, "name"));
            oldLocalFile.SetupGuid(fileGuid);
            oldLocalFile.SetupLastWriteTimeUtc(modificationDate);
            this.localFolder.SetupFiles(oldLocalFile.Object);
            this.storage.SaveMappedObject(obj);

            var underTest = this.CreateCrawler();

            Assert.That(underTest.Handle(new StartNextSyncEvent()), Is.True);
            this.queue.Verify(q => q.AddEvent(It.Is<FileEvent>(e => e.Remote == MetaDataChangeType.DELETED)), Times.Once());
            this.VerifyThatCountOfAddedEventsIsLimitedTo(Times.Once());
            this.VerifyThatListenerHasBeenUsed();
        }

        [Test, Category("Fast")]
        public void SimpleRemoteFolderHierarchyAdded() {
            var newRemoteSubFolder = MockOfIFolderUtil.CreateRemoteFolderMock("remoteSubFolder", "sub", "/name/sub", "remoteFolder");
            var newRemoteFolder = MockOfIFolderUtil.CreateRemoteFolderMock("remoteFolder", "name", "/name", this.remoteRootId);
            newRemoteFolder.SetupDescendants(newRemoteSubFolder.Object);
            this.remoteFolder.SetupDescendants(newRemoteFolder.Object);
            var crawler = this.CreateCrawler();

            Assert.That(crawler.Handle(new StartNextSyncEvent()), Is.True);
            this.queue.Verify(q => q.AddEvent(It.Is<FolderEvent>(e => e.RemoteFolder.Equals(newRemoteFolder.Object))), Times.Once());
            this.queue.Verify(q => q.AddEvent(It.Is<FolderEvent>(e => e.RemoteFolder.Equals(newRemoteSubFolder.Object))), Times.Once());
            this.VerifyThatCountOfAddedEventsIsLimitedTo(Times.Exactly(2));
            this.VerifyThatListenerHasBeenUsed();
        }

        [Test, Category("Fast")]
        public void OneLocalFolderAdded() {
            var newFolderMock = this.fsFactory.AddDirectory(Path.Combine(this.localRootPath, "newFolder"));
            this.localFolder.SetupDirectories(newFolderMock.Object);
            var crawler = this.CreateCrawler();

            Assert.That(crawler.Handle(new StartNextSyncEvent()), Is.True);
            this.queue.Verify(q => q.AddEvent(It.Is<FolderEvent>(e => e.LocalFolder.Equals(newFolderMock.Object))), Times.Once());
            this.VerifyThatListenerHasBeenUsed();
        }

        [Test, Category("Fast")]
        public void OneLocalFileAdded() {
            var newFileMock = this.fsFactory.AddFile(Path.Combine(this.localRootPath, "newFile"));
            this.localFolder.SetupFiles(newFileMock.Object);
            var crawler = this.CreateCrawler();

            Assert.That(crawler.Handle(new StartNextSyncEvent()), Is.True);
            this.queue.Verify(q => q.AddEvent(It.Is<FileEvent>(e => e.LocalFile.Equals(newFileMock.Object))), Times.Once());
            this.VerifyThatCountOfAddedEventsIsLimitedTo(Times.Once());
            this.VerifyThatListenerHasBeenUsed();
        }

        [Test, Category("Fast")]
        public void OneLocalFileRemoved() {
            var oldLocalFile = this.fsFactory.AddFile(Path.Combine(this.localRootPath, "oldFile"));
            oldLocalFile.Setup(f => f.Exists).Returns(false);
            var remoteFile = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, "oldFileId", "oldFile", this.remoteRootId, changeToken: "oldChange");
            var storedFile = new MappedObject("oldFile", "oldFileId", MappedObjectType.File, this.remoteRootId, "oldChange") {
                Guid = Guid.NewGuid()
            };
            this.storage.SaveMappedObject(storedFile);

            this.remoteFolder.SetupDescendants(remoteFile.Object);
            var crawler = this.CreateCrawler();

            Assert.That(crawler.Handle(new StartNextSyncEvent()), Is.True);
            this.queue.Verify(q => q.AddEvent(It.Is<FileEvent>(e => e.LocalFile != null && e.LocalFile.Equals(oldLocalFile.Object) && e.Local.Equals(MetaDataChangeType.DELETED))), Times.Once());
            this.VerifyThatCountOfAddedEventsIsLimitedTo(Times.Once());
            this.VerifyThatListenerHasBeenUsed();
        }

        [Test, Category("Fast")]
        public void OneLocalFolderRemoved() {
            var oldLocalFolder = this.fsFactory.AddDirectory(Path.Combine(this.localRootPath, "oldFolder"));
            oldLocalFolder.Setup(f => f.Exists).Returns(false);
            var remoteSubFolder = MockOfIFolderUtil.CreateRemoteFolderMock("oldFolderId", "oldFolder", this.remoteRootPath + "oldFolder", this.remoteRootId, "oldChange");
            var storedFolder = new MappedObject("oldFolder", "oldFolderId", MappedObjectType.Folder, this.remoteRootId, "oldChange") {
                Guid = Guid.NewGuid()
            };
            this.storage.SaveMappedObject(storedFolder);
            this.remoteFolder.SetupDescendants(remoteSubFolder.Object);
            var crawler = this.CreateCrawler();

            Assert.That(crawler.Handle(new StartNextSyncEvent()), Is.True);
            this.queue.Verify(q => q.AddEvent(It.Is<FolderEvent>(e => e.LocalFolder != null && e.LocalFolder.Equals(oldLocalFolder.Object))), Times.Once());
            this.VerifyThatCountOfAddedEventsIsLimitedTo(Times.Once());
            this.VerifyThatListenerHasBeenUsed();
        }

        [Test, Category("Fast")]
        public void OneLocalFolderRenamed() {
            var uuid = Guid.NewGuid();
            var oldLocalFolder = this.fsFactory.AddDirectory(Path.Combine(this.localRootPath, "oldFolder"));
            var newLocalFolder = this.fsFactory.AddDirectory(Path.Combine(this.localRootPath, "newFolder"));
            oldLocalFolder.Setup(f => f.Exists).Returns(false);
            newLocalFolder.SetupGuid(uuid);
            var remoteSubFolder = MockOfIFolderUtil.CreateRemoteFolderMock("oldFolderId", "oldFolder", this.remoteRootPath + "oldFolder", this.remoteRootId, "oldChange");
            var storedFolder = new MappedObject("oldFolder", "oldFolderId", MappedObjectType.Folder, this.remoteRootId, "oldChange") {
                Guid = uuid
            };
            this.storage.SaveMappedObject(storedFolder);
            this.remoteFolder.SetupDescendants(remoteSubFolder.Object);

            this.localFolder.SetupDirectories(newLocalFolder.Object);
            var crawler = this.CreateCrawler();

            Assert.That(crawler.Handle(new StartNextSyncEvent()), Is.True);
            this.queue.Verify(
                q =>
                q.AddEvent(
                It.Is<FolderEvent>(
                e =>
                e.LocalFolder.Equals(newLocalFolder.Object) &&
                e.Local.Equals(MetaDataChangeType.CHANGED))),
                Times.Once());
            this.VerifyThatCountOfAddedEventsIsLimitedTo(Times.Once());
            this.VerifyThatListenerHasBeenUsed();
        }

        [Test, Category("Fast")]
        public void OneLocalFileCopied() {
            var uuid = Guid.NewGuid();
            var oldFile = this.fsFactory.AddFile(Path.Combine(this.localRootPath, "old"), uuid);
            oldFile.Setup(f => f.LastWriteTimeUtc).Returns(this.lastLocalWriteTime);
            var newFile = this.fsFactory.AddFile(Path.Combine(this.localRootPath, "new"), uuid);

            var remoteSubFolder = MockOfIDocumentUtil.CreateRemoteDocumentMock("streamId", "oldId", oldFile.Object.Name, this.remoteFolder.Object, changeToken: "oldChange");
            var storedFolder = new MappedObject(oldFile.Object.Name, "oldId", MappedObjectType.File, this.remoteRootId, "oldChange") {
                Guid = uuid,
                LastLocalWriteTimeUtc = this.lastLocalWriteTime
            };
            this.storage.SaveMappedObject(storedFolder);
            this.remoteFolder.SetupDescendants(remoteSubFolder.Object);
            this.localFolder.SetupFiles(newFile.Object, oldFile.Object);

            var crawler = this.CreateCrawler();

            Assert.That(crawler.Handle(new StartNextSyncEvent()), Is.True);
            this.queue.Verify(
                q =>
                q.AddEvent(
                It.Is<FileEvent>(
                e =>
                e.LocalFile.Equals(newFile.Object) &&
                e.Local.Equals(MetaDataChangeType.CREATED))),
                Times.Once());
            this.VerifyThatCountOfAddedEventsIsLimitedTo(Times.Once());
        }

        [Test, Category("Fast")]
        public void NoChangeOnExistingFileAndFolderCreatesNoEventsInQueue() {
            DateTime changeTime = DateTime.UtcNow;
            string changeToken = "token";
            string fileName = "name";
            string fileId = "fileId";
            string folderName = "folder";
            string folderId = "folderId";
            Guid folderGuid = Guid.NewGuid();
            Guid fileGuid = Guid.NewGuid();
            var remoteFile = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, fileId, fileName, this.remoteRootId, changeToken: changeToken);
            var existingRemoteFolder = MockOfIFolderUtil.CreateRemoteFolderMock(folderId, folderName, this.remoteRootPath + folderName, this.remoteRootId, changeToken);
            var file = this.fsFactory.AddFile(Path.Combine(this.localRootPath, fileName), fileGuid);
            file.Setup(f => f.LastWriteTimeUtc).Returns(changeTime);
            var storedFile = new MappedObject(fileName, fileId, MappedObjectType.File, this.remoteRootId, changeToken) { Guid = fileGuid, LastLocalWriteTimeUtc = changeTime };
            this.storage.SaveMappedObject(storedFile);
            var folder = this.fsFactory.AddDirectory(Path.Combine(this.localRootPath, folderName), folderGuid);
            folder.Setup(f => f.LastWriteTimeUtc).Returns(changeTime);
            var storedFolder = new MappedObject(folderName, folderId, MappedObjectType.Folder, this.remoteRootId, changeToken) { Guid = folderGuid, LastLocalWriteTimeUtc = changeTime };
            this.storage.SaveMappedObject(storedFolder);
            this.remoteFolder.SetupDescendants(remoteFile.Object, existingRemoteFolder.Object);
            this.localFolder.SetupFilesAndDirectories(file.Object, folder.Object);

            var crawler = this.CreateCrawler();

            Assert.That(crawler.Handle(new StartNextSyncEvent()), Is.True);
            this.queue.Verify(q => q.AddEvent(It.Is<AbstractFolderEvent>(e => e.Local == MetaDataChangeType.NONE && e.Remote == MetaDataChangeType.NONE)), Times.Never());
            this.VerifyThatCountOfAddedEventsIsLimitedTo(Times.Never());
            this.VerifyThatListenerHasBeenUsed();
        }

        [Test, Category("Fast")]
        public void OneRemoteFolderRenamed() {
            DateTime modification = DateTime.UtcNow;
            string oldFolderName = "folderName";
            string newFolderName = "newfolderName";
            string folderId = "folderId";
            var localGuid = Guid.NewGuid();
            var oldLocalFolder = this.fsFactory.AddDirectory(Path.Combine(this.localRootPath, oldFolderName));
            oldLocalFolder.Setup(f => f.LastWriteTimeUtc).Returns(modification);
            oldLocalFolder.SetupGuid(localGuid);
            var storedFolder = new MappedObject(oldFolderName, folderId, MappedObjectType.Folder, this.remoteRootId, "changeToken") {
                Guid = localGuid,
                LastLocalWriteTimeUtc = modification
            };
            this.localFolder.SetupDirectories(oldLocalFolder.Object);
            this.storage.SaveMappedObject(storedFolder);
            var renamedRemoteFolder = MockOfIFolderUtil.CreateRemoteFolderMock(folderId, newFolderName, this.remoteRootPath + newFolderName, this.remoteRootId, "newChangeToken");
            this.remoteFolder.SetupDescendants(renamedRemoteFolder.Object);
            Assert.That(this.CreateCrawler().Handle(new StartNextSyncEvent()), Is.True);
            this.queue.Verify(
                q =>
                q.AddEvent(
                It.Is<FolderEvent>(
                e =>
                e.Remote == MetaDataChangeType.CHANGED &&
                e.Local == MetaDataChangeType.NONE &&
                e.RemoteFolder.Equals(renamedRemoteFolder.Object))),
                Times.Once());
            this.VerifyThatCountOfAddedEventsIsLimitedTo(Times.Once());
            this.VerifyThatListenerHasBeenUsed();
        }

        [Test, Category("Fast")]
        public void OneRemoteFileRenamedAndContentHashIsNotAvailable() {
            string oldFileName = "fileName";
            string newFileName = "newfileName";
            string fileId = "fileId";
            var localGuid = Guid.NewGuid();
            DateTime modificationDate = DateTime.UtcNow;
            var oldLocalFile = this.fsFactory.AddFile(Path.Combine(this.localRootPath, oldFileName));
            oldLocalFile.SetupGuid(localGuid);
            oldLocalFile.SetupLastWriteTimeUtc(modificationDate);
            var storedFile = new MappedObject(oldFileName, fileId, MappedObjectType.File, this.remoteRootId, "changeToken") {
                Guid = localGuid,
                LastLocalWriteTimeUtc = modificationDate
            };
            this.localFolder.SetupFiles(oldLocalFile.Object);
            this.storage.SaveMappedObject(storedFile);
            var renamedRemoteFile = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, fileId, newFileName, this.remoteRootId, changeToken: "newChangeToken");
            this.remoteFolder.SetupDescendants(renamedRemoteFile.Object);
            Assert.That(this.CreateCrawler().Handle(new StartNextSyncEvent()), Is.True);
            this.queue.Verify(
                q =>
                q.AddEvent(
                It.Is<FileEvent>(
                e =>
                e.Remote == MetaDataChangeType.CHANGED &&
                e.RemoteContent == ContentChangeType.CHANGED &&
                e.Local == MetaDataChangeType.NONE &&
                e.RemoteFile.Equals(renamedRemoteFile.Object))),
                Times.Once());
            this.VerifyThatCountOfAddedEventsIsLimitedTo(Times.Once());
            this.VerifyThatListenerHasBeenUsed();
        }

        [Test, Category("Fast")]
        public void OneRemoteFileRenamedAndContentHashIsAvailable() {
            byte[] checksum = new byte[20];
            string type = "SHA-1";
            string oldFileName = "fileName";
            string newFileName = "newfileName";
            string fileId = "fileId";
            DateTime lastModification = DateTime.UtcNow;
            var localGuid = Guid.NewGuid();
            var oldLocalFile = this.fsFactory.AddFile(Path.Combine(this.localRootPath, oldFileName));
            oldLocalFile.SetupLastWriteTimeUtc(lastModification);
            oldLocalFile.SetupGuid(localGuid);
            var storedFile = new MappedObject(oldFileName, fileId, MappedObjectType.File, this.remoteRootId, "changeToken") {
                Guid = localGuid,
                ChecksumAlgorithmName = type,
                LastChecksum = checksum,
                LastLocalWriteTimeUtc = lastModification
            };
            this.localFolder.SetupFiles(oldLocalFile.Object);
            this.storage.SaveMappedObject(storedFile);
            var renamedRemoteFile = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, fileId, newFileName, this.remoteRootId, changeToken: "newChangeToken");
            renamedRemoteFile.SetupContentStreamHash(checksum, type);
            this.remoteFolder.SetupDescendants(renamedRemoteFile.Object);
            Assert.That(this.CreateCrawler().Handle(new StartNextSyncEvent()), Is.True);
            this.queue.Verify(
                q =>
                q.AddEvent(
                It.Is<FileEvent>(
                e =>
                e.Remote == MetaDataChangeType.CHANGED &&
                e.RemoteContent == ContentChangeType.NONE &&
                e.Local == MetaDataChangeType.NONE &&
                e.RemoteFile.Equals(renamedRemoteFile.Object))),
                Times.Once());
            this.VerifyThatCountOfAddedEventsIsLimitedTo(Times.Once());
            this.VerifyThatListenerHasBeenUsed();
        }

        [Test, Category("Fast")]
        public void OneRemoteFolderMoved() {
            string oldFolderName = "folderName";
            string folderId = "folderId";
            var localGuid = Guid.NewGuid();
            DateTime modification = DateTime.UtcNow;
            var localTargetGuid = Guid.NewGuid();
            var oldLocalFolder = this.fsFactory.AddDirectory(Path.Combine(this.localRootPath, oldFolderName));
            oldLocalFolder.SetupLastWriteTimeUtc(modification);
            oldLocalFolder.SetupGuid(localGuid);
            var localTargetFolder = this.fsFactory.AddDirectory(Path.Combine(this.localRootPath, "target"));
            localTargetFolder.SetupLastWriteTimeUtc(modification);
            localTargetFolder.SetupGuid(localTargetGuid);
            var storedFolder = new MappedObject(oldFolderName, folderId, MappedObjectType.Folder, this.remoteRootId, "changeToken") {
                Guid = localGuid,
                LastLocalWriteTimeUtc = modification
            };
            var storedTargetFolder = new MappedObject("target", "targetId", MappedObjectType.Folder, this.remoteRootId, "changeToken") {
                Guid = localTargetGuid,
                LastLocalWriteTimeUtc = modification
            };
            this.localFolder.SetupDirectories(oldLocalFolder.Object, localTargetFolder.Object);
            this.storage.SaveMappedObject(storedFolder);
            this.storage.SaveMappedObject(storedTargetFolder);
            var targetFolder = MockOfIFolderUtil.CreateRemoteFolderMock("targetId", "target", this.remoteRootPath + "target", this.remoteRootId, "changeToken");
            var renamedRemoteFolder = MockOfIFolderUtil.CreateRemoteFolderMock(folderId, oldFolderName, this.remoteRootPath + oldFolderName, "targetId", "newChangeToken");
            targetFolder.SetupDescendants(renamedRemoteFolder.Object);
            this.remoteFolder.SetupDescendants(targetFolder.Object);
            Assert.That(this.CreateCrawler().Handle(new StartNextSyncEvent()), Is.True);
            this.queue.Verify(
                q =>
                q.AddEvent(
                It.Is<FolderEvent>(
                e =>
                e.Remote == MetaDataChangeType.MOVED &&
                e.Local == MetaDataChangeType.NONE &&
                e.RemoteFolder.Equals(renamedRemoteFolder.Object))),
                Times.Once());
            this.VerifyThatCountOfAddedEventsIsLimitedTo(Times.Once());
            this.VerifyThatListenerHasBeenUsed();
        }

        [Test, Category("Fast")]
        public void OneRemoteFolderRemoved() {
            Guid uuid = Guid.NewGuid();
            DateTime modification = DateTime.UtcNow;
            var oldLocalFolder = this.fsFactory.AddDirectory(Path.Combine(this.localRootPath, "folderName"));
            oldLocalFolder.SetupGuid(uuid);
            oldLocalFolder.SetupLastWriteTimeUtc(modification);
            var storedFolder = new MappedObject("folderName", "folderId", MappedObjectType.Folder, this.remoteRootId, "changeToken") {
                Guid = uuid,
                LastLocalWriteTimeUtc = modification
            };
            this.storage.SaveMappedObject(storedFolder);
            this.localFolder.SetupDirectories(oldLocalFolder.Object);
            var crawler = this.CreateCrawler();

            Assert.That(crawler.Handle(new StartNextSyncEvent()), Is.True);
            this.queue.Verify(q => q.AddEvent(It.Is<FolderEvent>(e => e.Remote == MetaDataChangeType.DELETED && e.LocalFolder.Equals(oldLocalFolder.Object))), Times.Once());
            this.VerifyThatCountOfAddedEventsIsLimitedTo(Times.Once());
            this.VerifyThatListenerHasBeenUsed();
        }

        [Test, Category("Fast")]
        public void OneRemoteAndTheSameLocalFolderRemoved() {
            string folderName = "folderName";
            var oldLocalFolder = this.fsFactory.AddDirectory(Path.Combine(this.localRootPath, folderName));
            var storedFolder = new MappedObject(folderName, "folderId", MappedObjectType.Folder, this.remoteRootId, "changeToken");
            this.storage.SaveMappedObject(storedFolder);
            var crawler = this.CreateCrawler();

            Assert.That(crawler.Handle(new StartNextSyncEvent()), Is.True);
            this.queue.Verify(q => q.AddEvent(It.Is<FolderEvent>(e => e.Remote == MetaDataChangeType.DELETED && e.Local == MetaDataChangeType.DELETED && e.LocalFolder.Equals(oldLocalFolder.Object))), Times.Once());
            this.queue.Verify(q => q.AddEvent(It.Is<FolderEvent>(e => e.Remote == MetaDataChangeType.NONE && e.Local == MetaDataChangeType.DELETED && e.LocalFolder.Equals(oldLocalFolder.Object))), Times.Never());
            this.queue.Verify(q => q.AddEvent(It.Is<FolderEvent>(e => e.Remote == MetaDataChangeType.DELETED && e.Local == MetaDataChangeType.NONE && e.LocalFolder.Equals(oldLocalFolder.Object))), Times.Never());
            this.VerifyThatCountOfAddedEventsIsLimitedTo(Times.Once());
            this.VerifyThatListenerHasBeenUsed();
        }

        #region boilerplatecode
        public void Dispose() {
            if (this.storageEngine != null) {
                this.storageEngine.Dispose();
                this.storageEngine = null;
            }
        }
        #endregion

        private DescendantsCrawler CreateCrawler() {
            var generator = new CrawlEventGenerator(this.storage, this.fsFactory.Object);
            var treeBuilder = new DescendantsTreeBuilder(this.storage, this.remoteFolder.Object, this.localFolder.Object, this.filter, Mock.Of<IIgnoredEntitiesStorage>());
            var notifier = new CrawlEventNotifier(this.queue.Object);
            return new DescendantsCrawler(this.queue.Object, treeBuilder, generator, notifier, this.listener.Object);
        }

        private void VerifyThatListenerHasBeenUsed() {
            this.listener.Verify(l => l.ActivityStarted(), Times.AtLeastOnce());
            this.listener.Verify(l => l.ActivityStopped(), Times.AtLeastOnce());
        }

        private void VerifyThatCountOfAddedEventsIsLimitedTo(Times times) {
            this.queue.Verify(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()), times);
        }
    }
}