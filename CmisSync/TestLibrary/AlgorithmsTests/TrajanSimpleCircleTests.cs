//-----------------------------------------------------------------------
// <copyright file="TrajanSimpleCircleTests.cs" company="GRAU DATA AG">
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

namespace TestLibrary.AlgorithmsTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CmisSync.Lib.Algorithms;

    using NUnit.Framework;

    [TestFixture]
    public class TrajanSimpleCircleTests
    {
        /// <summary>
        /// A ↔ B
        /// </summary>
        [Test, Category("Fast")]
        public void SimpleCycle()
        {
            var a = new StringTarjanNode("A");
            var b = new StringTarjanNode("B", a);
            a.Neighbors.Add(b);

            var underTest = new Tarjan(a, b);

            Assert.That(underTest.ResultSets.Count, Is.EqualTo(1));
            Assert.That(underTest.ResultSets.First().Contains(a));
            Assert.That(underTest.ResultSets.First().Contains(b));
        }

        /// <summary>
        /// A ↔ B
        /// C ↔ D
        /// </summary>
        [Test, Category("Fast")]
        public void TwoCycles()
        {
            var a = new StringTarjanNode("A");
            var b = new StringTarjanNode("B", a);
            a.Neighbors.Add(b);

            var c = new StringTarjanNode("C");
            var d = new StringTarjanNode("D", c);
            c.Neighbors.Add(d);

            var underTest = new Tarjan(a, b, c, d);

            Assert.That(underTest.ResultSets.Count, Is.EqualTo(2));
        }

        /// <summary>
        /// A → B
        /// ↑    ↓
        /// D ← C
        /// </summary>
        [Test, Category("Fast")]
        public void LargerSimpleCycle()
        {
            var a = new StringTarjanNode("A");
            var b = new StringTarjanNode("B", a);
            var c = new StringTarjanNode("C", b);
            var d = new StringTarjanNode("D", c);
            a.Neighbors.Add(d);

            var underTest = new Tarjan(a, b, c, d);

            Assert.That(underTest.ResultSets.Count, Is.EqualTo(1));
            Assert.That(underTest.ResultSets.First().Contains(a));
            Assert.That(underTest.ResultSets.First().Contains(b));
            Assert.That(underTest.ResultSets.First().Contains(c));
            Assert.That(underTest.ResultSets.First().Contains(d));
        }

        /// <summary>
        /// A → B
        /// ↑    ↓
        /// D ← C → E
        /// </summary>
        [Test, Category("Fast")]
        public void LargerSimpleCycleWithBlindEnd()
        {
            var a = new StringTarjanNode("A");
            var b = new StringTarjanNode("B", a);
            var c = new StringTarjanNode("C", b);
            var d = new StringTarjanNode("D", c);
            a.Neighbors.Add(d);

            var e = new StringTarjanNode("E");
            c.Neighbors.Add(e);

            var underTest = new Tarjan(a, b, c, d, e);

            Assert.That(underTest.ResultSets.Count, Is.EqualTo(2));
            var first = underTest.ResultSets.First();
            var last = underTest.ResultSets.Last();
            var large = first.Count > last.Count ? first : last;
            var small = first.Count > last.Count ? last : first;
            Assert.That(large.Count == 4);
            Assert.That(small.Count == 1);
            Assert.That(large.Contains(a));
            Assert.That(large.Contains(b));
            Assert.That(large.Contains(c));
            Assert.That(large.Contains(d));
            Assert.That(small.Contains(e));
        }

        /// <summary>
        /// A → B
        /// ↑    ↓
        /// D ← C ← E
        /// </summary>
        [Test, Category("Fast")]
        public void LargerSimpleCycleWithBlindEnd2()
        {
            var a = new StringTarjanNode("A");
            var b = new StringTarjanNode("B", a);
            var c = new StringTarjanNode("C", b);
            var d = new StringTarjanNode("D", c);
            a.Neighbors.Add(d);

            var e = new StringTarjanNode("E", c);

            var underTest = new Tarjan(a, b, c, d, e);

            Assert.That(underTest.ResultSets.Count, Is.EqualTo(2));
            var first = underTest.ResultSets.First();
            var last = underTest.ResultSets.Last();
            var large = first.Count > last.Count ? first : last;
            var small = first.Count > last.Count ? last : first;
            Assert.That(large.Count == 4);
            Assert.That(small.Count == 1);
            Assert.That(large.Contains(a));
            Assert.That(large.Contains(b));
            Assert.That(large.Contains(c));
            Assert.That(large.Contains(d));
            Assert.That(small.Contains(e));
        }
    }
}