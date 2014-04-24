//-----------------------------------------------------------------------
// <copyright file="FullSyncCompletedEventTest.cs" company="GRAU DATA AG">
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

using CmisSync.Lib.Events;

using NUnit.Framework;

using Moq;
namespace TestLibrary.EventsTests
{ 
    [TestFixture]
    public class FullSyncCompletedEventTest
    {
        [Test, Category("Fast")]
        public void ContructorTest() {
            var start = new Mock<StartNextSyncEvent>(false).Object;
            var complete = new FullSyncCompletedEvent(start);
            Assert.AreEqual(start, complete.StartEvent);
        }

        [Test, Category("Fast")]
        public void ConstructorFailsOnNullParameterTest()
        {
            try{
                new FullSyncCompletedEvent(null);
                Assert.Fail();
            }catch(ArgumentNullException) {}
        }

        [Test, Category("Fast")]
        public void ParamTest () {
            string key = "key";
            string value = "value";
            string result;
            var start = new StartNextSyncEvent(false);
            start.SetParam(key, value);
            var complete = new FullSyncCompletedEvent(start);
            Assert.IsTrue(complete.StartEvent.TryGetParam(key, out result));
            Assert.AreEqual(value, result);
        }
    }
}

