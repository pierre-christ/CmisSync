using System;
using System.Collections.Generic;
using System.IO;

using CmisSync.Lib.Events;
using CmisSync.Lib.Sync.Strategy;
using CmisSync.Lib.Storage;

using DotCMIS.Client;

using NUnit.Framework;

using Moq;

namespace TestLibrary.SyncStrategiesTests
{
    [TestFixture]
    public class CrawlerTest
    {
        private string localPath; 
        private IDirectoryInfo localFolder;
        private DirectoryInfo realLocalFolder;

        [SetUp]
        public void SetUp() {
            localPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            realLocalFolder = new DirectoryInfo(localPath);
            localFolder = new DirectoryInfoWrapper(realLocalFolder);
            localFolder.Create();
        }

        [TearDown]
        public void TearDown() {
            localFolder.Refresh();
            realLocalFolder.Delete();
        }

        [Test, Category("Fast")]
        public void ConstructorWithValidInputTest () {
            var queue = new Mock<ISyncEventQueue>().Object;
            var remoteFolder = new Mock<IFolder>().Object;
            var localFolder = new Mock<IDirectoryInfo>();
            var crawler = new Crawler(queue, remoteFolder, localFolder.Object);
            Assert.AreEqual(Crawler.CRAWLER_PRIORITY, crawler.Priority);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsOnNullQueueTest ()
        {
            var remoteFolder = new Mock<IFolder>().Object;
            var localFolder = new Mock<IDirectoryInfo>();

            new Crawler(null, remoteFolder, localFolder.Object);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsOnNullLocalFolderTest ()
        {
            var queue = new Mock<ISyncEventQueue>().Object;
            var remoteFolder = new Mock<IFolder>().Object;
            new Crawler(queue, remoteFolder, null);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsOnNullRemoteFolderTest ()
        {
            var queue = new Mock<ISyncEventQueue>().Object;
            new Crawler(queue, null, localFolder);
        }

        [Test, Category("Fast")]
        public void IgnoreWrongEventsTest() {
            var queue = new Mock<ISyncEventQueue>().Object;
            var remoteFolder = new Mock<IFolder>().Object;
            var localFolder = new Mock<IDirectoryInfo>();
            var wrongEvent = new Mock<ISyncEvent>().Object;
            var crawler = new Crawler(queue, remoteFolder, localFolder.Object);
            Assert.False(crawler.Handle(wrongEvent));
        }

        [Ignore]
        [Test, Category("Fast")]
        public void CrawlingSpecificFolderTest() {
            Assert.Fail("TODO");
        }

        [Ignore]
        [Test, Category("Fast")]
        public void CrawlingBaseFolderTest() {
            Assert.Fail("TODO");
        }

        [Test, Category("Fast")]
        public void CrawlingEmptyFoldersTest() {
            var queue = new Mock<ISyncEventQueue>();

            var remoteFolder = new Mock<IFolder>();
            var remoteChildren = new Mock<IItemEnumerable<ICmisObject>>();
            var list = new List<ICmisObject>();
            remoteChildren.Setup(r => r.GetEnumerator()).Returns(list.GetEnumerator());
            remoteFolder.Setup(r => r.GetChildren()).Returns(remoteChildren.Object);

            var localFolder = new Mock<IDirectoryInfo>();

            var crawler = new Crawler(queue.Object, remoteFolder.Object, localFolder.Object);
            var startEvent = new StartNextSyncEvent(true);
            Assert.True(crawler.Handle(startEvent));


            queue.Verify(q => q.AddEvent(It.IsAny<FullSyncCompletedEvent>()),Times.Once());
            //this and only this should be added
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()),Times.Once());

        }
    }
}

