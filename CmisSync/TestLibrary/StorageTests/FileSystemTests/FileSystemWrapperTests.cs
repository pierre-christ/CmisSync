//-----------------------------------------------------------------------
// <copyright file="FileSystemWrapperTests.cs" company="GRAU DATA AG">
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

namespace TestLibrary.StorageTests.FileSystemTests
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using CmisSync.Lib.Storage.FileSystem;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.ProducerTests.WatcherTests;

    [TestFixture]
    public class FileSystemWrapperTests {
        private static readonly IFileSystemInfoFactory Factory = new FileSystemInfoFactory();
        private DirectoryInfo testFolder;

        [SetUp]
        public void Init() {
            string tempPath = Path.GetTempPath();
            var tempFolder = new DirectoryInfo(tempPath);
            Assert.That(tempFolder.Exists, Is.True);
            this.testFolder = tempFolder.CreateSubdirectory("DSSFileSystemWrapperTest");
        }

        [TearDown]
        public void Cleanup() {
            this.testFolder.Delete(true);
        }

        [Test, Category("Medium")]
        public void FileInfoConstruction() {
            string fileName = "test1";
            string fullPath = Path.Combine(this.testFolder.FullName, fileName);
            IFileInfo fileInfo = Factory.CreateFileInfo(fullPath);
            Assert.That(fileInfo, Is.Not.Null);
        }

        [Test, Category("Medium")]
        public void DirectoryInfoConstruction() {
            string fileName = "test1";
            string fullPath = Path.Combine(this.testFolder.FullName, fileName);
            IDirectoryInfo fileInfo = Factory.CreateDirectoryInfo(fullPath);
            Assert.That(fileInfo, Is.Not.Null);
        }

        [Test, Category("Medium")]
        public void FullPath() {
            string fileName = "test1";
            string fullPath = Path.Combine(this.testFolder.FullName, fileName);
            IFileSystemInfo fileInfo = Factory.CreateFileInfo(fullPath);
            Assert.That(fileInfo.FullName, Is.EqualTo(fullPath));
        }

        [Test, Category("Medium")]
        public void Exists() {
            string fileName = "test1";
            string fullPath = Path.Combine(this.testFolder.FullName, fileName);
            IFileSystemInfo fileInfo = Factory.CreateFileInfo(fullPath);
            Assert.That(fileInfo.Exists, Is.EqualTo(false));
            using (var stream = new FileInfo(fullPath).Create()) {
                fileInfo = Factory.CreateFileInfo(fullPath);
                Assert.That(fileInfo.Exists, Is.EqualTo(true));
            }
        }

        [Test, Category("Medium")]
        public void Refresh() {
            string fileName = "test1";
            string fullPath = Path.Combine(this.testFolder.FullName, fileName);
            IFileSystemInfo fileInfo = Factory.CreateFileInfo(fullPath);

            // trigger lacy loading
            Assert.That(fileInfo.Exists, Is.EqualTo(false));
            var stream = new FileInfo(fullPath).Create();
            stream.Dispose();
            fileInfo.Refresh();
            Assert.That(fileInfo.Exists, Is.EqualTo(true));
        }

        [Test, Category("Medium")]
        public void Name() {
            string fileName = "test1";
            string fullPath = Path.Combine(this.testFolder.FullName, fileName);
            IFileSystemInfo fileInfo = Factory.CreateFileInfo(fullPath);
            Assert.That(fileInfo.Name, Is.EqualTo(fileName));
        }

        [Test, Category("Medium")]
        public void Attributes() {
            string fileName = "test1";
            string fullPath = Path.Combine(this.testFolder.FullName, fileName);
            this.testFolder.CreateSubdirectory(fileName);
            IFileSystemInfo fileInfo = Factory.CreateDirectoryInfo(fullPath);
            Assert.That(fileInfo.Attributes & FileAttributes.Directory, Is.EqualTo(FileAttributes.Directory));
        }

        [Test, Category("Medium")]
        public void Create() {
            string fileName = "test1";
            string fullPath = Path.Combine(this.testFolder.FullName, fileName);
            IDirectoryInfo dirInfo = Factory.CreateDirectoryInfo(fullPath);
            dirInfo.Create();
            Assert.That(dirInfo.Exists, Is.EqualTo(true));
        }

        [Test, Category("Medium")]
        public void Directory() {
            string fileName = "test1";
            string fullPath = Path.Combine(this.testFolder.FullName, fileName);
            IFileInfo fileInfo = Factory.CreateFileInfo(fullPath);
            Assert.That(fileInfo.Directory.FullName, Is.EqualTo(this.testFolder.FullName));
        }

        [Test, Category("Medium")]
        public void Parent() {
            string fileName = "test1";
            string fullPath = Path.Combine(this.testFolder.FullName, fileName);
            IDirectoryInfo dirInfo = Factory.CreateDirectoryInfo(fullPath);
            Assert.That(dirInfo.Parent.FullName, Is.EqualTo(this.testFolder.FullName));
        }

        [Test, Category("Medium")]
        public void GetDirectoriesFor2() {
            string folder1 = "folder1";
            string folder2 = "folder2";
            this.testFolder.CreateSubdirectory(folder1);
            this.testFolder.CreateSubdirectory(folder2);
            IDirectoryInfo dirInfo = Factory.CreateDirectoryInfo(this.testFolder.FullName);
            Assert.That(dirInfo.GetDirectories().Length, Is.EqualTo(2));
            Assert.That(dirInfo.GetDirectories()[0].Name, Is.EqualTo(folder1));
            Assert.That(dirInfo.GetDirectories()[1].Name, Is.EqualTo(folder2));
        }

        [Test, Category("Medium")]
        public void GetDirectoriesFor0() {
            IDirectoryInfo dirInfo = Factory.CreateDirectoryInfo(this.testFolder.FullName);
            Assert.That(dirInfo.GetDirectories().Length, Is.EqualTo(0));
        }

        [Test, Category("Medium")]
        public void GetFilesFor2() {
            string file1 = "file1";
            string file2 = "file2";
            string fullPath1 = Path.Combine(this.testFolder.FullName, file1);
            string fullPath2 = Path.Combine(this.testFolder.FullName, file2);
            var stream = new FileInfo(fullPath1).Create();
            stream.Dispose();
            var stream2 = new FileInfo(fullPath2).Create();
            stream2.Dispose();
            IDirectoryInfo dirInfo = Factory.CreateDirectoryInfo(this.testFolder.FullName);
            Assert.That(dirInfo.GetFiles().Length, Is.EqualTo(2));
            Assert.That(dirInfo.GetFiles()[0].Name, Is.EqualTo(file1));
            Assert.That(dirInfo.GetFiles()[1].Name, Is.EqualTo(file2));
        }

        [Test, Category("Medium")]
        public void GetFilesFor0() {
            IDirectoryInfo dirInfo = Factory.CreateDirectoryInfo(this.testFolder.FullName);
            Assert.That(dirInfo.GetFiles().Length, Is.EqualTo(0));
        }

        [Test, Category("Medium")]
        public void DeleteTrue() {
            string fileName = "test1";
            string fullPath = Path.Combine(this.testFolder.FullName, fileName);
            IDirectoryInfo dirInfo = Factory.CreateDirectoryInfo(fullPath);
            dirInfo.Create();
            Assert.That(dirInfo.Exists, Is.True);
            dirInfo.Delete(true);
            dirInfo.Refresh();
            Assert.That(dirInfo.Exists, Is.False);
        }

        [Test, Category("Medium")]
        public void DeleteFile() {
            string fileName = "toBeDeleted";
            string fullPath = Path.Combine(this.testFolder.FullName, fileName);
            IFileInfo fileInfo = Factory.CreateFileInfo(fullPath);
            using (fileInfo.Open(FileMode.CreateNew)) {
            }

            Assert.That(fileInfo.Exists, Is.True);
            fileInfo.Delete();
            fileInfo.Refresh();
            Assert.That(fileInfo.Exists, Is.False);
        }

        [Test, Category("Medium")]
        public void ReplaceFileContent() {
            string sourceFile = "source";
            string targetFile = "target";
            string backupFile = "source.bak";
            IFileInfo sourceInfo = Factory.CreateFileInfo(Path.Combine(this.testFolder.FullName, sourceFile));
            IFileInfo targetInfo = Factory.CreateFileInfo(Path.Combine(this.testFolder.FullName, targetFile));
            IFileInfo backupInfo = Factory.CreateFileInfo(Path.Combine(this.testFolder.FullName, backupFile));
            using (var stream = sourceInfo.Open(FileMode.CreateNew, FileAccess.Write)) {
                stream.Write(new byte[2], 0, 2);
            }

            sourceInfo.Refresh();
            Assert.That(sourceInfo.Exists, Is.True);
            Assert.That(sourceInfo.Length, Is.EqualTo(2));
            using (var stream = targetInfo.Open(FileMode.CreateNew, FileAccess.Write)) {
                stream.Write(new byte[5], 0, 5);
            }

            targetInfo.Refresh();
            Assert.That(targetInfo.Exists, Is.True);
            Assert.That(targetInfo.Length, Is.EqualTo(5));

            var newFileInfo = sourceInfo.Replace(targetInfo, backupInfo, true);

            sourceInfo.Refresh();
            targetInfo.Refresh();
            backupInfo.Refresh();
            Assert.That(sourceInfo.Exists, Is.False);
            Assert.That(targetInfo.Length, Is.EqualTo(2));
            Assert.That(backupInfo.Exists, Is.True);
            Assert.That(backupInfo.Length, Is.EqualTo(5));
            Assert.That(newFileInfo.FullName, Is.EqualTo(targetInfo.FullName));
        }

        [Test, Category("Medium")]
        public void ReplaceFileContentAndExtendedAttributes() {
            this.SkipIfExtendedAttributesAreNotAvailable();

            string sourceFile = "source";
            string targetFile = "target";
            string backupFile = "source.bak";
            IFileInfo sourceInfo = Factory.CreateFileInfo(Path.Combine(this.testFolder.FullName, sourceFile));
            IFileInfo targetInfo = Factory.CreateFileInfo(Path.Combine(this.testFolder.FullName, targetFile));
            IFileInfo backupInfo = Factory.CreateFileInfo(Path.Combine(this.testFolder.FullName, backupFile));
            using (var stream = sourceInfo.Open(FileMode.CreateNew, FileAccess.Write)) {
                stream.Write(new byte[2], 0, 2);
            }

            sourceInfo.SetExtendedAttribute("test", sourceFile, false);
            sourceInfo.Refresh();
            using (var stream = targetInfo.Open(FileMode.CreateNew, FileAccess.Write)) {
                stream.Write(new byte[5], 0, 5);
            }

            targetInfo.SetExtendedAttribute("test", targetFile, false);
            targetInfo.Refresh();

            var newFileInfo = sourceInfo.Replace(targetInfo, backupInfo, true);
            Assert.That(newFileInfo.GetExtendedAttribute("test"), Is.EqualTo(sourceFile));
            backupInfo.Refresh();
            Assert.That(backupInfo.Exists, Is.True);
            Assert.That(backupInfo.GetExtendedAttribute("test"), Is.EqualTo(targetFile));
        }

        [Test, Category("Medium")]
        public void SetUuidOnFolderToNull() {
            this.SkipIfExtendedAttributesAreNotAvailable();
            var underTest = Factory.CreateDirectoryInfo(Path.Combine(this.testFolder.FullName, "folder"));
            underTest.Create();

            underTest.Uuid = null;

            Assert.That(underTest.Uuid, Is.Null);
        }

        [Test, Category("Medium")]
        public void SetUuidOnFolder() {
            this.SkipIfExtendedAttributesAreNotAvailable();
            var underTest = Factory.CreateDirectoryInfo(Path.Combine(this.testFolder.FullName, "folder"));
            underTest.Create();

            Guid uuid = Guid.NewGuid();
            underTest.Uuid = uuid;

            Assert.That(underTest.Uuid, Is.EqualTo(uuid));
        }

        [Test, Category("Medium")]
        public void GetUuidFromFolder() {
            this.SkipIfExtendedAttributesAreNotAvailable();
            var underTest = Factory.CreateDirectoryInfo(Path.Combine(this.testFolder.FullName, "folder"));
            underTest.Create();

            Guid uuid = Guid.NewGuid();

            underTest.SetExtendedAttribute("DSS-UUID", uuid.ToString(), false);

            Assert.That(underTest.Uuid, Is.EqualTo(uuid));
        }

        [Test, Category("Medium")]
        public void UuidIsNullIfStoredStringIsNotAnUuid() {
            this.SkipIfExtendedAttributesAreNotAvailable();
            var underTest = Factory.CreateDirectoryInfo(Path.Combine(this.testFolder.FullName, "folder"));
            underTest.Create();

            underTest.SetExtendedAttribute("DSS-UUID", "stuff", false);

            Assert.That(underTest.Uuid, Is.Null);
        }

        [Test, Category("Medium")]
        public void UuidIsNullIfNothingIsStored() {
            this.SkipIfExtendedAttributesAreNotAvailable();
            var underTest = Factory.CreateDirectoryInfo(Path.Combine(this.testFolder.FullName, "folder"));
            underTest.Create();

            Assert.That(underTest.Uuid, Is.Null);
        }

        /*
        [Test, Category("Medium")]
        public void GetUuidIsReadMultipleTimesBeforeInsteadOfFailing() {
            this.SkipIfExtendedAttributesAreNotAvailable();
            IFileInfo underTest = Factory.CreateFileInfo(Path.Combine(this.testFolder.FullName, "file"));
            using (underTest.Open(FileMode.CreateNew)) {
            }

            Guid uuid = Guid.NewGuid();

            using (var stream = underTest.Open(FileMode.Open, FileAccess.Write, FileShare.None)) {
                TaskFactory.StartNew(() => SetUuid(underTest, uuid.ToString()));
                Thread.Sleep(100);
            }

            Thread.Sleep(100);

            Assert.That(underTest.Uuid, Is.EqualTo(uuid));
            uuid = Guid.NewGuid();
            underTest.Uuid = uuid;
            Guid? readUuid = null;
            using (var task = TaskFactory.StartNew(() => {
                Thread.Sleep(100);
                readUuid = underTest.Uuid;
            }))
            {
                using (var stream = underTest.Open(FileMode.Open, FileAccess.Write, FileShare.None)) {
                    Thread.Sleep(200);
                }
                task.Wait();
            }

            Assert.That(uuid, Is.EqualTo(readUuid));
        }

        private static void SetUuid(IFileSystemInfo fsObject, string uuid) {
            fsObject.Uuid = Guid.Parse(uuid);
        }

        private static void GetUuid(IFileSystemInfo fsObject, out Guid? uuid) {
            uuid = fsObject.Uuid;
        } */

        [Test, Category("Fast")]
        public void CreatesFirstConflictFile()
        {
            string fileName = "test1.txt";
            string fullPath = Path.Combine(this.testFolder.FullName, fileName);
            var fileInfo = Factory.CreateFileInfo(fullPath);
            using (new FileInfo(fullPath).Create()) {
            }

            fileInfo = Factory.CreateFileInfo(fullPath);

            var conflictFile = Factory.CreateConflictFileInfo(fileInfo);

            Assert.That(conflictFile.Exists, Is.False);
            Assert.That(conflictFile.Directory.FullName, Is.EqualTo(fileInfo.Directory.FullName));
            Assert.That(Path.GetExtension(conflictFile.FullName), Is.EqualTo(Path.GetExtension(fileInfo.FullName)), "The file extension must be kept the same as in the original file");
            Assert.That(conflictFile.Name, Is.Not.EqualTo(fileInfo.Name));
        }

        [Test, Category("Medium")]
        public void IsDirectoryTrue() {
            string fullPath = Path.GetTempPath();
            Assert.That(Factory.IsDirectory(fullPath), Is.True);
        }

        [Test, Category("Medium")]
        public void IsDirectoryFalse() {
            string fullPath = Path.GetTempFileName();
            Assert.That(Factory.IsDirectory(fullPath), Is.False);
        }

        [Test, Category("Medium")]
        public void IsDirectoryNull() {
            string fullPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Assert.That(Factory.IsDirectory(fullPath), Is.Null);
        }

        [Test, Category("Medium")]
        public void IsDirectoryRequirements() {
            string path = Path.GetTempPath();
            Assert.That(Factory.CreateFileInfo(path).Exists, Is.False);
            Assert.That(Factory.CreateDirectoryInfo(path).Exists, Is.True);
        }

        [Test, Category("Medium")]
        public void DirectoryMoveTo() {
            string oldPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            string newPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var folder = Factory.CreateDirectoryInfo(oldPath);
            folder.Create();
            folder.MoveTo(newPath);
            Assert.That(folder.Exists, Is.True);
            Assert.That(folder.FullName.TrimEnd(Path.DirectorySeparatorChar), Is.EqualTo(newPath));
            Assert.That(Factory.CreateDirectoryInfo(oldPath).Exists, Is.False);
        }

        [Test, Category("Medium")]
        public void DirectoryMoveToAlsoMovesExtendedAttributes() {
            string oldPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            string newPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var folder = Factory.CreateDirectoryInfo(oldPath);
            folder.Create();
            BaseWatcherTest.IgnoreIfExtendedAttributesAreNotAvailable(oldPath);
            Assert.That(folder.GetExtendedAttribute("test"), Is.Null);
            folder.SetExtendedAttribute("test", "test", false);
            folder.MoveTo(newPath);
            Assert.That(folder.GetExtendedAttribute("test"), Is.EqualTo("test"));
            Assert.That(Factory.CreateDirectoryInfo(newPath).GetExtendedAttribute("test"), Is.EqualTo("test"));
        }

        [Test, Category("Medium")]
        public void FileMoveTo() {
            string oldPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            string newPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var file = Factory.CreateFileInfo(oldPath);
            using (file.Open(FileMode.CreateNew)) {
            }

            file.MoveTo(newPath);
            Assert.That(file.Exists, Is.True);
            Assert.That(file.FullName, Is.EqualTo(newPath));
            Assert.That(Factory.CreateFileInfo(oldPath).Exists, Is.False);
        }

        [Test, Category("Medium")]
        public void FileMoveToAlsoMovesExtendedAttributes() {
            string oldPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            string newPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var file = Factory.CreateFileInfo(oldPath);
            using (file.Open(FileMode.CreateNew)) {
            }

            BaseWatcherTest.IgnoreIfExtendedAttributesAreNotAvailable(oldPath);
            Assert.That(file.GetExtendedAttribute("test"), Is.Null);
            file.SetExtendedAttribute("test", "test", false);
            file.MoveTo(newPath);
            Assert.That(file.GetExtendedAttribute("test"), Is.EqualTo("test"));
            Assert.That(Factory.CreateFileInfo(newPath).GetExtendedAttribute("test"), Is.EqualTo("test"));
        }

        [Test, Category("Medium")]
        public void SetModificationDate() {
            string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var file = Factory.CreateFileInfo(path);
            using (file.Open(FileMode.CreateNew)) {
            }

            var nearFutureTime = DateTime.UtcNow.AddHours(1);
            file.LastWriteTimeUtc = nearFutureTime;
            Assert.That(file.LastWriteTimeUtc, Is.EqualTo(nearFutureTime).Within(1).Seconds);
        }

        [Test, Category("Medium")]
        public void SetOldModificationDate() {
            string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var file = Factory.CreateFileInfo(path);
            using (file.Open(FileMode.CreateNew)) {
            }

            var veryOldDate = new DateTime(1500, 1, 1);
            file.LastWriteTimeUtc = veryOldDate;
        }

        [Test, Category("Medium")]
        public void SetFutureModificationDate() {
            string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var file = Factory.CreateFileInfo(path);
            using (file.Open(FileMode.CreateNew)) {
            }

            var veryFuturisticDate = new DateTime(6000, 1, 1);
            file.LastWriteTimeUtc = veryFuturisticDate;
        }

        [Test, Category("Fast")]
        public void CreateDownloadCacheIfExtendedAttributesAreAvailable() {
            Guid uuid = Guid.NewGuid();
            var file = Mock.Of<IFileInfo>(
                f =>
                f.Uuid == uuid &&
                f.Exists == true);
            var cacheFile = Factory.CreateDownloadCacheFileInfo(file);
            Assert.That(cacheFile.Name, Is.EqualTo(uuid.ToString() + ".sync"));

            // Ensure that the path does not maps to temp path to avoid problems with extended attribute support
            Assert.That(cacheFile.FullName.Contains(Path.GetTempPath()), Is.False);
        }

        [Test, Category("Fast")]
        public void CreateDownloadCacheIfExtendedAttributesAreNotAvailable() {
            var file = Mock.Of<IFileInfo>(
                f => f.Name == "file" &&
                f.Exists == true &&
                f.FullName == Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "file"));
            var cacheFile = Factory.CreateDownloadCacheFileInfo(file);
            Assert.That(cacheFile.Name, Is.EqualTo(file.Name + ".sync"));
            Assert.That(cacheFile.FullName, Is.EqualTo(file.FullName + ".sync"));
        }

        [Test, Category("Fast")]
        public void CreateDownloadCacheFailsIfOriginalFileCannotBeAccessed() {
            var fileMock = new Mock<IFileInfo>();
            fileMock.Setup(f => f.Exists).Returns(true);
            fileMock.Setup(f => f.Uuid).Throws<ExtendedAttributeException>();
            Assert.Throws<ExtendedAttributeException>(() => Factory.CreateDownloadCacheFileInfo(fileMock.Object));
        }

        [Test, Category("Fast")]
        public void CreateDownloadCacheFailsIfFileDoesNotExists() {
            var file = Mock.Of<IFileInfo>(f => f.Exists == false);
            Assert.Throws<FileNotFoundException>(() => Factory.CreateDownloadCacheFileInfo(file));
        }

        [Test, Category("Fast")]
        public void CreateDownloadCacheCreatesIdenticalFileNames() {
            Guid uuid = Guid.NewGuid();
            var file = Mock.Of<IFileInfo>(
                f =>
                f.Uuid == uuid &&
                f.Exists == true);

            var cacheFile1 = Factory.CreateDownloadCacheFileInfo(file);
            var cacheFile2 = Factory.CreateDownloadCacheFileInfo(file);

            Assert.That(cacheFile1.Name, Is.EqualTo(cacheFile2.Name));
            Assert.That(cacheFile1.FullName, Is.EqualTo(cacheFile2.FullName));
        }

        [Test, Category("Fast")]
        public void CreateDownloadCacheCreatesIdenticalFileNamesIfUuidIsAvailableAndOriginalNamesAreDifferent() {
            Guid uuid = Guid.NewGuid();
            var file1 = Mock.Of<IFileInfo>(
                f =>
                f.Uuid == uuid &&
                f.Name == "name1" &&
                f.Exists == true);

            var file2 = Mock.Of<IFileInfo>(
                f =>
                f.Uuid == uuid &&
                f.Name == "name2" &&
                f.Exists == true);

            var cacheFile1 = Factory.CreateDownloadCacheFileInfo(file1);
            var cacheFile2 = Factory.CreateDownloadCacheFileInfo(file2);

            Assert.That(cacheFile1.Name, Is.EqualTo(cacheFile2.Name));
            Assert.That(cacheFile1.FullName, Is.EqualTo(cacheFile2.FullName));
        }

        [Test, Category("Fast")]
        public void CreateDownloadCacheWithGivenUuid() {
            Guid uuid = Guid.NewGuid();
            var cacheFile = Factory.CreateDownloadCacheFileInfo(uuid);
            Assert.That(cacheFile.Name, Is.EqualTo(uuid.ToString() + ".sync"));
        }

        [Test, Category("Fast")]
        public void CreateDownloadCacheWithEmptyUuidThrowsException() {
            Assert.Throws<ArgumentException>(() => Factory.CreateDownloadCacheFileInfo(Guid.Empty));
        }

        // Test is not implemented yet
        [Ignore]
        [Test, Category("Fast")]
        public void CreateNextConflictFile()
        {
            Assert.Fail("TODO");
            /*
            for (int i = 0; i < 10; i++)
            {
                using (FileStream s = File.Create(conflictFilePath))
                {
                }

                conflictFilePath = Utils.FindNextConflictFreeFilename(path, user);
                Assert.AreNotEqual(path, conflictFilePath, "The conflict file must differ from original file");
                Assert.True(conflictFilePath.Contains(user), "The username should be added to the conflict file name");
                Assert.True(conflictFilePath.EndsWith(Path.GetExtension(path)), "The file extension must be kept the same as in the original file");
                string filename = Path.GetFileName(conflictFilePath);
                string originalFilename = Path.GetFileNameWithoutExtension(path);
                Assert.True(filename.StartsWith(originalFilename), string.Format("The conflict file \"{0}\" must start with \"{1}\"", filename, originalFilename));
                string conflictParent = Directory.GetParent(conflictFilePath).FullName;
                Assert.AreEqual(originalParent, conflictParent, "The conflict file must exists in the same directory like the orignial file");
            } */
        }

        private void SkipIfExtendedAttributesAreNotAvailable() {
            if (!Factory.CreateDirectoryInfo(this.testFolder.FullName).IsExtendedAttributeAvailable()) {
                Assert.Ignore("Extended Attributes are not available => test skipped.");
            }
        }
    }
}