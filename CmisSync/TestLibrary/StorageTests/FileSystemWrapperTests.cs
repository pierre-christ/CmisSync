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
using System;
using System.IO;

using CmisSync.Lib.Storage;

using NUnit.Framework;

using Moq;

namespace TestLibrary.StorageTests {

    [TestFixture]
    public class FileSystemWrapperTests {
        private static readonly IFileSystemInfoFactory factory = new FileSystemInfoFactory();
        DirectoryInfo testFolder;

        [SetUp]
        public void Init() {
            string tempPath = Path.GetTempPath();
            var tempFolder = new DirectoryInfo(tempPath);
            Assert.That(tempFolder.Exists, Is.True);
            testFolder = tempFolder.CreateSubdirectory("DSSFileSystemWrapperTest");
        }

        [TearDown]
        public void Cleanup() {
            testFolder.Delete(true);
        }

        [Test, Category("Medium")]
        public void FileInfoConstruction() {
            string fileName = "test1";
            string fullPath = Path.Combine(testFolder.FullName, fileName);
            IFileInfo fileInfo = factory.CreateFileInfo(fullPath);
            Assert.That(fileInfo, Is.Not.Null);
        }

        [Test, Category("Medium")]
        public void DirectoryInfoConstruction() {
            string fileName = "test1";
            string fullPath = Path.Combine(testFolder.FullName, fileName);
            IDirectoryInfo fileInfo = factory.CreateDirectoryInfo(fullPath);
            Assert.That(fileInfo, Is.Not.Null);
        }

        [Test, Category("Medium")]
        public void FullPath() {
            string fileName = "test1";
            string fullPath = Path.Combine(testFolder.FullName, fileName);
            IFileSystemInfo fileInfo = factory.CreateFileInfo(fullPath);
            Assert.That(fileInfo.FullName, Is.EqualTo(fullPath));
        }

        [Test, Category("Medium")]
        public void Exists() {
            string fileName = "test1";
            string fullPath = Path.Combine(testFolder.FullName, fileName);
            IFileSystemInfo fileInfo = factory.CreateFileInfo(fullPath);
            Assert.That(fileInfo.Exists, Is.EqualTo(false));
            new FileInfo(fullPath).Create();
            fileInfo = factory.CreateFileInfo(fullPath);
            Assert.That(fileInfo.Exists, Is.EqualTo(true));
        }

        [Test, Category("Medium")]
        public void Refresh() {
            string fileName = "test1";
            string fullPath = Path.Combine(testFolder.FullName, fileName);
            IFileSystemInfo fileInfo = factory.CreateFileInfo(fullPath);
            //trigger lacy loading
            Assert.That(fileInfo.Exists, Is.EqualTo(false));
            new FileInfo(fullPath).Create();
            fileInfo.Refresh();
            Assert.That(fileInfo.Exists, Is.EqualTo(true));
        }

        [Test, Category("Medium")]
        public void Name() {
            string fileName = "test1";
            string fullPath = Path.Combine(testFolder.FullName, fileName);
            IFileSystemInfo fileInfo = factory.CreateFileInfo(fullPath);
            Assert.That(fileInfo.Name, Is.EqualTo(fileName));
        }

        [Test, Category("Medium")]
        public void Attributes() {
            string fileName = "test1";
            string fullPath = Path.Combine(testFolder.FullName, fileName);
            testFolder.CreateSubdirectory(fileName);
            IFileSystemInfo fileInfo = factory.CreateDirectoryInfo(fullPath);
            Assert.That(fileInfo.Attributes, Is.EqualTo(FileAttributes.Directory));
        }

        [Test, Category("Medium")]
        public void Create() {
            string fileName = "test1";
            string fullPath = Path.Combine(testFolder.FullName, fileName);
            IDirectoryInfo dirInfo = factory.CreateDirectoryInfo(fullPath);
            dirInfo.Create();
            Assert.That(dirInfo.Exists, Is.EqualTo(true));
        }

        [Test, Category("Medium")]
        public void Directory() {
            string fileName = "test1";
            string fullPath = Path.Combine(testFolder.FullName, fileName);
            IFileInfo fileInfo = factory.CreateFileInfo(fullPath);
            Assert.That(fileInfo.Directory.FullName, Is.EqualTo(testFolder.FullName));
        }

        [Test, Category("Medium")]
        public void Parent() {
            string fileName = "test1";
            string fullPath = Path.Combine(testFolder.FullName, fileName);
            IDirectoryInfo dirInfo = factory.CreateDirectoryInfo(fullPath);
            Assert.That(dirInfo.Parent.FullName, Is.EqualTo(testFolder.FullName));
        }

        [Test, Category("Medium")]
        public void GetDirectoriesFor2() {
            string folder1 = "folder1";
            string folder2 = "folder2";
            testFolder.CreateSubdirectory(folder1);
            testFolder.CreateSubdirectory(folder2);
            IDirectoryInfo dirInfo = factory.CreateDirectoryInfo(testFolder.FullName);
            Assert.That(dirInfo.GetDirectories().Length, Is.EqualTo(2));
            Assert.That(dirInfo.GetDirectories()[0].Name, Is.EqualTo(folder1));
            Assert.That(dirInfo.GetDirectories()[1].Name, Is.EqualTo(folder2));
        }

        [Test, Category("Medium")]
        public void GetDirectoriesFor0() {
            IDirectoryInfo dirInfo = factory.CreateDirectoryInfo(testFolder.FullName);
            Assert.That(dirInfo.GetDirectories().Length, Is.EqualTo(0));
        }

        [Test, Category("Medium")]
        public void GetFilesFor2() {
            string file1 = "file1";
            string file2 = "file2";
            string fullPath1 = Path.Combine(testFolder.FullName, file1);
            string fullPath2 = Path.Combine(testFolder.FullName, file2);
            new FileInfo(fullPath1).Create();
            new FileInfo(fullPath2).Create();
            IDirectoryInfo dirInfo = factory.CreateDirectoryInfo(testFolder.FullName);
            Assert.That(dirInfo.GetFiles().Length, Is.EqualTo(2));
            Assert.That(dirInfo.GetFiles()[0].Name, Is.EqualTo(file1));
            Assert.That(dirInfo.GetFiles()[1].Name, Is.EqualTo(file2));
        }

        [Test, Category("Medium")]
        public void GetFilesFor0() {
            IDirectoryInfo dirInfo = factory.CreateDirectoryInfo(testFolder.FullName);
            Assert.That(dirInfo.GetFiles().Length, Is.EqualTo(0));
        }

        [Test, Category("Medium")]
        public void DeleteTrue() {
            string fileName = "test1";
            string fullPath = Path.Combine(testFolder.FullName, fileName);
            IDirectoryInfo dirInfo = factory.CreateDirectoryInfo(fullPath);
            dirInfo.Create();
            Assert.That(dirInfo.Exists, Is.EqualTo(true));
            dirInfo.Delete(true);
            dirInfo.Refresh();
            Assert.That(dirInfo.Exists, Is.EqualTo(false));

        }

    }
}
