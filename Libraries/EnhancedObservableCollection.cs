using System;
using System.Linq;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;

namespace GG.Libraries
{
    /// <summary> 
    /// Represents a dynamic data collection that provides notifications when items get added, removed, or when the whole list is refreshed. 
    /// </summary> 
    /// <typeparam name="T"></typeparam> 
    public class EnhancedObservableCollection<T> : System.Collections.ObjectModel.ObservableCollection<T>
    {
        private bool disableNotifications = false;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!disableNotifications)
                base.OnCollectionChanged(e);
        }

        /// <summary>
        /// Disables all change notifications for this collection. You need to re-enable them or they will never fire.
        /// 
        /// Note: Enabling notifications will not fire the supressed notifications.
        /// </summary>
        public void DisableNotifications()
        {
            disableNotifications = true;
        }

        public void EnableNotifications()
        {
            disableNotifications = false;
        }

        /// <summary> 
        /// Adds the elements of the specified collection to the end of the ObservableCollection(Of T). 
        /// </summary> 
        public void AddRange(IEnumerable<T> collection)
        {
            disableNotifications = true;

            foreach (T item in collection)
            {
                Add(item);
            }

            disableNotifications = false;

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary> 
        /// Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection(Of T) class. 
        /// </summary> 
        public EnhancedObservableCollection() : base() { }

        /// <summary> 
        /// Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection(Of T) class that contains elements copied from the specified collection. 
        /// </summary> 
        /// <param name="collection">collection: The collection from which the elements are copied.</param> 
        /// <exception cref="System.ArgumentNullException">The collection parameter cannot be null.</exception> 
        public EnhancedObservableCollection(IEnumerable<T> collection) : base(collection) { }
    }
}