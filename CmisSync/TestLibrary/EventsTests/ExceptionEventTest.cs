//-----------------------------------------------------------------------
// <copyright file="ExceptionEventTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.EventsTests.ExceptionEventsTests
{
    using System;

    using CmisSync.Lib.Events;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class BaseExceptionEventTest
    {
        [Test, Category("Fast")]
        public void ConstructorFailsWithNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ExceptionEvent(null));
        }

        [Test, Category("Fast")]
        public void ConstructorWithValidInput()
        {
            var exception = new Mock<Exception>().Object;
            var ev = new ExceptionEvent(exception);
            Assert.AreEqual(exception, ev.Exception);
        }

        [Test, Category("Fast")]
        public void ToStringIsImplemented()
        {
            var exception = new Mock<Exception>(string.Empty) { CallBase = true }.Object;
            var ev = new ExceptionEvent(exception);
            Assert.IsNotNull(ev.ToString());
        }
    }

    [TestFixture]
    public class PermissionDeniedEventTest
    {
        [Test, Category("Fast")]
        public void ConstructorWithValidInput()
        {
            var exception = new Mock<DotCMIS.Exceptions.CmisPermissionDeniedException>().Object;
            var ev = new PermissionDeniedEvent(exception);
            Assert.AreEqual(exception, ev.Exception);
        }
    }
}
