using System;
using System.IO;
using System.Collections.Generic;

using CmisSync.Lib.Sync.Strategy;
using CmisSync.Lib.Storage;
using CmisSync.Lib.Data;

using DotCMIS.Client;
using DotCMIS.Exceptions;

using NUnit.Framework;

using Moq;

using TestLibrary.TestUtils;
using CmisSync.Lib.Events;

namespace TestLibrary.SyncStrategiesTests.SituationDetectionTests
{
    [TestFixture]
    public class RemoteSituationDetectionTest
    {
        private Mock<IMetaDataStorage> StorageMock;
        private string RemoteChangeToken = "changeToken";
        private readonly IObjectId ObjectId = Mock.Of<IObjectId>(ob => ob.Id == "objectId");
        private readonly string RemotePath = "/object/path";


        [SetUp]
        public void SetUp() {
            this.StorageMock = new Mock<IMetaDataStorage>();

        }

        [Test, Category("Fast")]
        public void ConstructorWithSession() {
            new RemoteSituationDetection();
        }

        [Test, Category("Fast")]
        public void NoChangeDetectionForFile()
        {
            var lastModificationDate = DateTime.Now;
            var remoteObject = new Mock<IDocument>();
            var fileEvent = new FileEvent(remoteFile: remoteObject.Object);
            fileEvent.Remote = MetaDataChangeType.NONE;

            var detector = new RemoteSituationDetection();

            Assert.AreEqual(SituationType.NOCHANGE, detector.Analyse(StorageMock.Object, fileEvent));
        }

        [Test, Category("Fast")]
        public void NoChangeDetectionForFileOnAddedEvent()
        {
            var lastModificationDate = DateTime.Now;
            var remoteObject = new Mock<IDocument>();
            var remotePaths = new List<string>();
            remotePaths.Add(RemotePath);
            remoteObject.Setup (remote => remote.ChangeToken).Returns(RemoteChangeToken);
            remoteObject.Setup (remote => remote.Id ).Returns(ObjectId.Id);
            remoteObject.Setup (remote => remote.LastModificationDate).Returns(lastModificationDate);
            remoteObject.Setup (remote => remote.Paths).Returns(remotePaths);
            var file = Mock.Of<IMappedObject>( f =>
                                              f.LastRemoteWriteTimeUtc == lastModificationDate &&
                                              f.RemoteObjectId == ObjectId.Id &&
                                              f.LastChangeToken == RemoteChangeToken &&
                                              f.Type == MappedObjectType.File);
            StorageMock.AddMappedFile(file);
            var fileEvent = new FileEvent(remoteFile: remoteObject.Object) {Remote = MetaDataChangeType.CREATED};

            var detector = new RemoteSituationDetection();

            Assert.AreEqual(SituationType.NOCHANGE, detector.Analyse(StorageMock.Object, fileEvent));
        }

        [Test, Category("Fast")]
        public void NoChangeDetectedForFolder()
        {
            var remoteObject = new Mock<IFolder>();
            var folderEvent = new FolderEvent(remoteFolder: remoteObject.Object);
            folderEvent.Remote = MetaDataChangeType.NONE;

            var detector = new RemoteSituationDetection();

            Assert.AreEqual(SituationType.NOCHANGE, detector.Analyse(StorageMock.Object, folderEvent));
        }

        [Test, Category("Fast")]
        public void FileAddedDetection()
        {
            var remoteObject = new Mock<IDocument>();

            var fileEvent = new FileEvent(remoteFile: remoteObject.Object);
            fileEvent.Remote = MetaDataChangeType.CREATED;

            var detector = new RemoteSituationDetection();

            Assert.AreEqual(SituationType.ADDED, detector.Analyse(StorageMock.Object, fileEvent));
        }

        [Test, Category("Fast")]
        public void FolderAddedDetection()
        {
            var remoteObject = new Mock<IFolder>();
            var folderEvent = new FolderEvent(remoteFolder: remoteObject.Object);
            folderEvent.Remote = MetaDataChangeType.CREATED;

            var detector = new RemoteSituationDetection();

            Assert.AreEqual(SituationType.ADDED, detector.Analyse(StorageMock.Object, folderEvent));
        }


        [Test, Category("Fast")]
        public void FileRemovedDetection()
        {
            var remoteObject = new Mock<IDocument>();

            var fileEvent = new FileEvent(remoteFile: remoteObject.Object);
            fileEvent.Remote = MetaDataChangeType.DELETED;

            var detector = new RemoteSituationDetection();

            Assert.AreEqual(SituationType.REMOVED, detector.Analyse(StorageMock.Object, fileEvent));
        }

        [Test, Category("Fast")]
        public void FolderRemovedDetection()
        {
            var remoteObject = new Mock<IFolder>();
            var folderEvent = new FolderEvent(remoteFolder: remoteObject.Object);
            folderEvent.Remote = MetaDataChangeType.DELETED;

            var detector = new RemoteSituationDetection();

            Assert.AreEqual(SituationType.REMOVED, detector.Analyse(StorageMock.Object, folderEvent));
        }

        [Test, Category("Fast")]
        public void FolderMovedDetectionOnFolderMovedEvent()
        {
            var remoteObject = new Mock<IFolder>();
            var folderEvent = new FolderMovedEvent(null, null, null, remoteObject.Object) { Remote = MetaDataChangeType.MOVED };

            var detector = new RemoteSituationDetection();

            Assert.AreEqual(SituationType.MOVED, detector.Analyse(StorageMock.Object, folderEvent));
        }

        [Test, Category("Fast")]
        public void FolderMovedDetectionOnChangeEvent()
        {
            string folderName = "old";
            string oldLocalPath = Path.Combine(Path.GetTempPath(), folderName);
            string remoteId = "remoteId";
            string oldParentId = "oldParentId";
            string newParentId = "newParentId";
            var remoteFolder = new Mock<IFolder>();
            remoteFolder.Setup(f => f.Name).Returns(folderName);
            remoteFolder.Setup(f => f.Path).Returns("/new/" + folderName);
            remoteFolder.Setup(f => f.Id).Returns(remoteId);
            remoteFolder.Setup(f => f.ParentId).Returns(newParentId);
            var mappedParentFolder = Mock.Of<IMappedObject>(p =>
                                                            p.RemoteObjectId == oldParentId &&
                                                            p.Type == MappedObjectType.Folder);
            var mappedFolder = StorageMock.AddLocalFolder(oldLocalPath, remoteId);
            mappedFolder.Setup( f => f.Name).Returns(folderName);
            mappedFolder.Setup( f => f.ParentId).Returns(mappedParentFolder.RemoteObjectId);
            var folderEvent = new FolderEvent(remoteFolder: remoteFolder.Object) { Remote = MetaDataChangeType.CHANGED };

            var detector = new RemoteSituationDetection();

            Assert.AreEqual( SituationType.MOVED, detector.Analyse(StorageMock.Object, folderEvent));
        }

        [Test, Category("Fast")]
        public void FolderRenameDetectionOnChangeEvent()
        {
            string remoteId = "remoteId";
            string oldName = "old";
            string newName = "new";
            var remoteFolder = new Mock<IFolder>();
            remoteFolder.Setup(f => f.Name).Returns(newName);
            remoteFolder.Setup(f => f.Id).Returns(remoteId);
            var mappedFolder = Mock.Of<IMappedObject>(f =>
                                                      f.RemoteObjectId == remoteId &&
                                                      f.Name == oldName &&
                                                      f.Type == MappedObjectType.Folder);
            StorageMock.AddMappedFolder(mappedFolder);
            var folderEvent = new FolderEvent(remoteFolder: remoteFolder.Object) { Remote = MetaDataChangeType.CHANGED };

            var detector = new RemoteSituationDetection();

            Assert.AreEqual(SituationType.RENAMED, detector.Analyse(StorageMock.Object, folderEvent));
        }
    }
}

