//-----------------------------------------------------------------------
// <copyright file="ICountingQueue.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Queueing
{
    using System;

    /// <summary>
    /// I counting queue counts every countable event by its category if its added and substracts it if the event is handled and removed from queue.
    /// This queue also notifies listener about changes on categories or all countable events.
    /// </summary>
    public interface ICountingQueue : IDisposableSyncEventQueue, IObservable<int>, IObservable<Tuple<string, int>>
    {
    }
}