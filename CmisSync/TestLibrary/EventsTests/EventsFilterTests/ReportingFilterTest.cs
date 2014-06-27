//-----------------------------------------------------------------------
// <copyright file="ReportingFilterTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.EventsTests.EventsFilterTests
{
    using System;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Events.Filter;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ReportingFilterTest
    {
        private IgnoredFoldersFilter ignoreFoldersFilter;
        private IgnoredFileNamesFilter ignoreFileNamesFilter;
        private IgnoredFolderNameFilter ignoreFolderNamesFilter;
        private InvalidFolderNameFilter invalidFolderNameFilter;

        private Mock<ISyncEventQueue> queue;

        [SetUp]
        public void SetUpFilter() {
            this.ignoreFoldersFilter = Mock.Of<IgnoredFoldersFilter>();
            this.ignoreFileNamesFilter = Mock.Of<IgnoredFileNamesFilter>();
            this.ignoreFolderNamesFilter = Mock.Of<IgnoredFolderNameFilter>();
            this.invalidFolderNameFilter = Mock.Of<InvalidFolderNameFilter>();
            this.queue = new Mock<ISyncEventQueue>();
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void ConstructorTakesAllFilter() {
            new ReportingFilter(
                this.queue.Object,
                this.ignoreFoldersFilter,
                this.ignoreFileNamesFilter,
                this.ignoreFolderNamesFilter,
                this.invalidFolderNameFilter);
        }

        [Test, Category("Fast"), Category("EventFilter")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfQueueIsNull() {
            new ReportingFilter(
                null,
                this.ignoreFoldersFilter,
                this.ignoreFileNamesFilter,
                this.ignoreFolderNamesFilter,
                this.invalidFolderNameFilter);
        }

        [Test, Category("Fast"), Category("EventFilter")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfIgnoreFoldersFilterIsNull() {
            new ReportingFilter(
                this.queue.Object,
                null,
                this.ignoreFileNamesFilter,
                this.ignoreFolderNamesFilter,
                this.invalidFolderNameFilter);
        }

        [Test, Category("Fast"), Category("EventFilter")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfIgnoreFileNamesFilterIsNull() {
            new ReportingFilter(
                this.queue.Object,
                this.ignoreFoldersFilter,
                null,
                this.ignoreFolderNamesFilter,
                this.invalidFolderNameFilter);
        }

        [Test, Category("Fast"), Category("EventFilter")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfIgnoreFolderNameFilterIsNull() {
            new ReportingFilter(
                this.queue.Object,
                this.ignoreFoldersFilter,
                this.ignoreFileNamesFilter,
                null,
                this.invalidFolderNameFilter);
        }

        [Test, Category("Fast"), Category("EventFilter")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfInvalidFolderNameFilterIsNull() {
            new ReportingFilter(
                this.queue.Object,
                this.ignoreFoldersFilter,
                this.ignoreFileNamesFilter,
                this.ignoreFolderNamesFilter,
                null);
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void HandleFilterableFileNameEvent() {
            var filter = this.CreateFilter();
            var filterableNameEvent = Mock.Of<IFilterableNameEvent>(e => e.Name == "name");
            Mock<IgnoredFileNamesFilter> fileNameFilter = Mock.Get<IgnoredFileNamesFilter>(this.ignoreFileNamesFilter);
            Assert.That(filter.Handle(filterableNameEvent), Is.False);
            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never);
            string reason = null;
            fileNameFilter.Verify(f => f.CheckFile("name", out reason), Times.Once());
            Assert.That(string.IsNullOrEmpty(reason));
        }

        private ReportingFilter CreateFilter() {
            return new ReportingFilter(
                this.queue.Object,
                this.ignoreFoldersFilter,
                this.ignoreFileNamesFilter,
                this.ignoreFolderNamesFilter,
                this.invalidFolderNameFilter);
        }
    }
}