//-----------------------------------------------------------------------
// <copyright file="GenericSyncEventHandlerTest.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class GenericSyncEventHandlerTest
    {
        private int priority = 666;

        [Test, Category("Fast")]
        public void ConstructorTakesInstanceName()
        {
            string name = "InstanceName";
            var handler = new GenericSyncEventHandler<ISyncEvent>(
                delegate { return false; },
            name);
            Assert.That(handler.ToString().Contains(name));
        }

        [Test, Category("Fast")]
        public void ConstructorWorksWithNullName()
        {
            var handler = new GenericSyncEventHandler<ISyncEvent>(
                delegate { return false; },
            null);
            handler.ToString();
        }

        [Test, Category("Fast")]
        public void ToStringReturnsAcceptedType()
        {
            var handler = new GenericSyncEventHandler<ISyncEvent>(delegate { return false; });
            Assert.That(handler.ToString().Contains("ISyncEvent"));
        }

        [Test, Category("Fast")]
        public void PriorityTest()
        {
            var handler = new GenericSyncEventHandler<ISyncEvent>(
                this.priority,
                delegate(ISyncEvent e) {
                return false;
            });
            Assert.AreEqual(this.priority, handler.Priority);
        }

        [Test, Category("Fast")]
        public void IgnoresUnexpectedEvents()
        {
            bool eventPassed = false;
            var handler = new GenericSyncEventHandler<FSEvent>(
                this.priority,
                delegate(ISyncEvent e) {
                eventPassed = true;
                return true;
            });
            var wrongEvent = new Mock<ISyncEvent>(MockBehavior.Strict);
            Assert.IsFalse(handler.Handle(wrongEvent.Object));
            Assert.IsFalse(eventPassed);
        }

        [Test, Category("Fast")]
        public void HandleExpectedEvents()
        {
            bool eventPassed = false;
            var handler = new GenericSyncEventHandler<ConfigChangedEvent>(
                this.priority,
                delegate(ISyncEvent e) {
                eventPassed = true;
                return true;
            });
            var correctEvent = new Mock<ConfigChangedEvent>();
            Assert.IsTrue(handler.Handle(correctEvent.Object));
            Assert.IsTrue(eventPassed);
        }
    }
}
