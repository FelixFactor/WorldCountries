﻿namespace CountriesAPP.ViewModels
{
    using Interfaces;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;

    public class CountryMainViewModel
    {
        //observable collection to the XAML
        private readonly ObservableCollection<ITab> countries;
        //observable collection to take all kind of tabs
        public ICollection<ITab> Countries { get; }

        public CountryMainViewModel()
        {
            countries = new ObservableCollection<ITab>();
            //checks if the collection changed and acts accordingly
            countries.CollectionChanged += Countries_CollectionChanged;
            Countries = countries;
        }

        /// <summary>
        /// listens to the objects added and removed and subscrives/unsubscrives to the OnTabCloseRequest
        /// </summary>
        /// <remarks>
        /// similar to SQL deleted and updated tables,
        /// NotifyCollectionChangedEventArgs keeps a collection of the items
        /// e.NewItems and e.OldItems,
        /// since there will be only one tab added and removed at one time
        /// [0] is the item to add or remove from the collection
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Countries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ITab tab;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    tab = (ITab)e.NewItems[0];
                    tab.CloseRequest += OnTabCloseRequested;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    tab = (ITab)e.OldItems[0];
                    tab.CloseRequest -= OnTabCloseRequested;
                    break;
            }
        }

        /// <summary>
        /// removes the tab from the observation collection, and from the UI view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTabCloseRequested(object sender, EventArgs e)
        {
            Countries.Remove((ITab)sender);
        }

        /// <summary>
        /// Adds a new tab programatically to the observable collection
        /// </summary>
        /// <param name="tab"></param>
        public void AddTab(CountryTab tab)
        {
            Countries.Add(tab);
        }
    }
}
