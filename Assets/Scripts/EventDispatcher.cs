using System;
using System.Collections.Generic;
using Interfaces;
using Unity.VisualScripting;
using UnityEngine;

namespace DefaultNamespace
{
    public class EventDispatcher<TContext>
    {
        protected List<IEventObserver<TContext>> observers = new ();

        /// <summary>
        /// Dispatch an event to all observers.
        /// </summary>
        /// <param name="context">Event data to dispatch</param>
        public void BroadcastEvent(TContext context)
        {
            foreach (IEventObserver<TContext> observer in observers)
            {
                observer.OnEvent(context);
            }
        }

        /// <summary>
        /// Subscribe an object to the event dispatching list.
        /// </summary>
        /// <param name="observer"></param>
        public void Subscribe(IEventObserver<TContext> observer)
        {
            if (observers.Contains(observer))
            {
                Debug.LogWarning("Attempted to subscribe an object that was already subscribed to the dispatcher list.");
                return;
            }
            observers.Add(observer);
        }
        
        /// <summary>
        /// Remove an object from the event dispatching list.
        /// </summary>
        /// <param name="observer"></param>
        public void Unsubscribe(IEventObserver<TContext> observer)
        {
            if (!observers.Contains(observer))
            {
                Debug.LogWarning("Attempted to unsubscribe an object that never subscribed to the dispatcher list before.");
                return;
            }
            observers.Remove(observer);
        }
    }

    public class EventDispatcher : EventDispatcher<Null>
    {
        /// <summary>
        /// Dispatch an event to all observers.
        /// </summary>
        public void BroadcastEvent()
        {
            foreach (IEventObserver<Null> observer in observers)
            {
                ((IEventObserver)observer).OnEvent();
            }
        }
    }
}