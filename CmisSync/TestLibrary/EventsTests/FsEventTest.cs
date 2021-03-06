//-----------------------------------------------------------------------
// <copyright file="FsEventTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.EventsTests
{
    using System;
    using System.IO;

    using CmisSync.Lib.Events;

    using NUnit.Framework;

    [TestFixture]
    public class FsEventTest
    {
        [Test, Category("Fast")]
        public void Constructor()
        {
            string name = "test";
            string path = Path.Combine(Path.GetTempPath(), name);
            var e = new FSEvent(WatcherChangeTypes.Created, path, false);
            Assert.That(e.Name, Is.EqualTo(name));
            Assert.That(e.LocalPath, Is.EqualTo(path));
        }

        [Test, Category("Fast")]
        public void ConstructorThrowsExceptionInNullPath()
        {
            Assert.Throws<ArgumentNullException>(() => new FSEvent(WatcherChangeTypes.Created, null, false));
        }

        [Test, Category("Medium")]
        public void FsEventStoresDirectoryState()
        {
            var path = Path.Combine(Path.GetTempPath(), "newPath");
            Directory.CreateDirectory(path);
            var e = new FSEvent(WatcherChangeTypes.Created, path, true);

            Assert.That(e.IsDirectory, Is.True, "It is a Directory");
        }

        [Test, Category("Medium")]
        public void FsEventExtractsDirectoryName()
        {
            string name = "newPath";
            var path = Path.Combine(Path.GetTempPath(), name);
            Directory.CreateDirectory(path);
            var e = new FSEvent(WatcherChangeTypes.Created, path, true);

            Assert.That(e.Name, Is.EqualTo(name));
        }
    }
}
