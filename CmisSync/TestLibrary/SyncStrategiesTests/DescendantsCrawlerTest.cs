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

namespace TestLibrary.SyncStrategiesTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Sync.Strategy;

    using DBreeze;

    using DotCMIS.Client;

    using Moq;

    using Newtonsoft.Json;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class DescendantsCrawlerTest
    {
        private readonly string remoteRootId = "rootId";
        private readonly string remoteRootPath = "/";
        private Mock<ISyncEventQueue> queue;
        private IMetaDataStorage storage;
        private Mock<IFolder> remoteFolder;
        private Mock<IDirectoryInfo> localFolder;
        private Mock<IFileSystemInfoFactory> fsFactory;
        private string localRootPath;
        private MappedObject mappedRootObject;
        private IPathMatcher matcher;
        private DBreezeEngine storageEngine;

        [TestFixtureSetUp]
        public void InitCustomSerializator()
        {
            // Use Newtonsoft.Json as Serializator
            DBreeze.Utils.CustomSerializator.Serializator = JsonConvert.SerializeObject; 
            DBreeze.Utils.CustomSerializator.Deserializator = JsonConvert.DeserializeObject;
        }

        [SetUp]
        public void CreateMockObjects()
        {
            this.storageEngine = new DBreezeEngine(new DBreezeConfiguration { Storage = DBreezeConfiguration.eStorage.MEMORY });
            this.localRootPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            this.matcher = new PathMatcher(this.localRootPath, this.remoteRootPath);
            this.queue = new Mock<ISyncEventQueue>();
            this.remoteFolder = MockOfIFolderUtil.CreateRemoteFolderMock(this.remoteRootId, this.remoteRootPath, this.remoteRootPath);
            this.remoteFolder.SetupDescendants();
            this.localFolder = new Mock<IDirectoryInfo>();
            this.localFolder.Setup(f => f.FullName).Returns(this.localRootPath);
            this.localFolder.Setup(f => f.Exists).Returns(true);
            this.fsFactory = new Mock<IFileSystemInfoFactory>();
            this.fsFactory.AddIDirectoryInfo(this.localFolder.Object);
            this.mappedRootObject = new MappedObject(
                this.remoteRootPath,
                this.remoteRootId,
                MappedObjectType.Folder,
                null,
                "changeToken");
            this.storage = new MetaDataStorage(this.storageEngine, this.matcher);
            this.storage.SaveMappedObject(this.mappedRootObject);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfLocalFolderIsNull()
        {
            new DescendantsCrawler(Mock.Of<ISyncEventQueue>(), Mock.Of<IFolder>(), null, Mock.Of<IMetaDataStorage>());
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfRemoteFolderIsNull()
        {
            new DescendantsCrawler(Mock.Of<ISyncEventQueue>(), null, Mock.Of<IDirectoryInfo>(), Mock.Of<IMetaDataStorage>());
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfQueueIsNull()
        {
            new DescendantsCrawler(null, Mock.Of<IFolder>(), Mock.Of<IDirectoryInfo>(), Mock.Of<IMetaDataStorage>());
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfStorageIsNull()
        {
            new DescendantsCrawler(Mock.Of<ISyncEventQueue>(), Mock.Of<IFolder>(), Mock.Of<IDirectoryInfo>(), null);
        }

        [Test, Category("Fast")]
        public void ConstructorWorksWithoutFsInfoFactory()
        {
            new DescendantsCrawler(Mock.Of<ISyncEventQueue>(), Mock.Of<IFolder>(), Mock.Of<IDirectoryInfo>(), Mock.Of<IMetaDataStorage>());
        }

        [Test, Category("Fast")]
        public void ConstructorTakesFsInfoFactory()
        {
            this.CreateCrawler();
        }

        [Test, Category("Fast")]
        public void PriorityIsNormal()
        {
            var crawler = this.CreateCrawler();
            Assert.That(crawler.Priority == EventHandlerPriorities.NORMAL);
        }

        [Test, Category("Fast")]
        public void IgnoresNonFittingEvents()
        {
            var crawler = this.CreateCrawler();
            Assert.That(crawler.Handle(Mock.Of<ISyncEvent>()), Is.False);
            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
        }

        [Test, Category("Fast")]
        public void HandlesStartNextSyncEventAndReportsOnQueueIfDone()
        {
            var crawler = this.CreateCrawler();
            var startEvent = new StartNextSyncEvent();
            Assert.That(crawler.Handle(startEvent), Is.True);
            this.queue.Verify(q => q.AddEvent(It.Is<FullSyncCompletedEvent>(e => e.StartEvent.Equals(startEvent))), Times.Once());
        }

        [Test, Category("Fast")]
        public void RecognizesOneNewRemoteFolder()
        {
            IFolder newRemoteFolder = MockOfIFolderUtil.CreateRemoteFolderMock("id", "name", "/name", this.remoteRootId).Object;
            this.remoteFolder.SetupDescendants(newRemoteFolder);
            var crawler = this.CreateCrawler();

            Assert.That(crawler.Handle(new StartNextSyncEvent()), Is.True);
            this.queue.Verify(q => q.AddEvent(It.Is<FolderEvent>(e => e.RemoteFolder.Equals(newRemoteFolder))), Times.Once());
        }

        [Test, Category("Fast")]
        public void RegognizesNewRemoteFolderHierarchie()
        {
            var newRemoteSubFolder = MockOfIFolderUtil.CreateRemoteFolderMock("remoteSubFolder", "sub", "/name/sub", "remoteFolder");
            var newRemoteFolder = MockOfIFolderUtil.CreateRemoteFolderMock("remoteFolder", "name", "/name", this.remoteRootId);
            newRemoteFolder.SetupDescendants(newRemoteSubFolder.Object);
            this.remoteFolder.SetupDescendants(newRemoteFolder.Object);
            var crawler = this.CreateCrawler();

            Assert.That(crawler.Handle(new StartNextSyncEvent()), Is.True);
            this.queue.Verify(q => q.AddEvent(It.Is<FolderEvent>(e => e.RemoteFolder.Equals(newRemoteFolder.Object))), Times.Once());
            this.queue.Verify(q => q.AddEvent(It.Is<FolderEvent>(e => e.RemoteFolder.Equals(newRemoteSubFolder.Object))), Times.Once());
        }

        [Test, Category("Fast")]
        public void RecognizesOneNewLocalFolder()
        {
            var newFolderMock = this.fsFactory.AddDirectory(Path.Combine(this.localRootPath, "newFolder"));
            this.localFolder.SetupDirectories(newFolderMock.Object);
            var crawler = this.CreateCrawler();

            Assert.That(crawler.Handle(new StartNextSyncEvent()), Is.True);
            this.queue.Verify(q => q.AddEvent(It.Is<FolderEvent>(e => e.LocalFolder.Equals(newFolderMock.Object))), Times.Once());
        }

        private DescendantsCrawler CreateCrawler()
        {
            return new DescendantsCrawler(
                this.queue.Object,
                this.remoteFolder.Object,
                this.localFolder.Object,
                this.storage,
                this.fsFactory.Object);
        }
    }
}