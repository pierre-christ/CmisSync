//-----------------------------------------------------------------------
// <copyright file="DelayRetryAndNextSyncEventHandler.cs" company="GRAU DATA AG">
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
    using System.Collections.Generic;

    using CmisSync.Lib.Events;

    public class DelayRetryAndNextSyncEventHandler : ReportingSyncEventHandler
    {
        private bool triggerSyncWhenQueueEmpty = false;
        private bool triggerFullSync = false;
        private List<AbstractFolderEvent> retryEvents = new List<AbstractFolderEvent>();

        public DelayRetryAndNextSyncEventHandler(ISyncEventQueue queue) : base(queue)
        {
        }

        public override bool Handle(ISyncEvent e) {
            bool hasBeenHandled = false;

            var startNextSyncEvent = e as StartNextSyncEvent;
            if(startNextSyncEvent != null) {
                triggerSyncWhenQueueEmpty = true;
                triggerFullSync = startNextSyncEvent.FullSyncRequested;
                hasBeenHandled = true;
            }

            var fileOrFolderEvent = e as AbstractFolderEvent;
            if(fileOrFolderEvent != null && fileOrFolderEvent.RetryCount > 0) {
                retryEvents.Add(fileOrFolderEvent);
                hasBeenHandled = true;
            }

            if(Queue.IsEmpty && triggerSyncWhenQueueEmpty) {
                if(triggerFullSync) {
                    this.retryEvents.Clear();
                }

                foreach(var storedRetryEvent in retryEvents) {
                    Queue.AddEvent(storedRetryEvent);
                }

                Queue.AddEvent(new StartNextSyncEvent(triggerFullSync));
                triggerSyncWhenQueueEmpty = false;
            }

            return hasBeenHandled;
        }
    }
}

