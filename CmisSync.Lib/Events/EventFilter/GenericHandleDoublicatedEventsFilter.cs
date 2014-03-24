using System;

using CmisSync.Lib.Events;

namespace CmisSync.Lib.Events.Filter
{
    public class GenericHandleDoublicatedEventsFilter<Filter, Reset> : SyncEventHandler
        where Filter: ISyncEvent
        where Reset : ISyncEvent
    {
        private bool firstOccurence = true;

        public override bool Handle (ISyncEvent e)
        {
            if(e is Filter)
            {
                if(firstOccurence)
                {
                    firstOccurence = false;
                    return false;
                }
                else
                {
                    return true;
                }
            }
            if(e is Reset)
            {
                firstOccurence = true;
            }
            return false;
        }

        public override int Priority {
            get {
                return EventHandlerPriorities.GetPriority(typeof(GenericHandleDoublicatedEventsFilter<,>));
            }
        }
    }
}

