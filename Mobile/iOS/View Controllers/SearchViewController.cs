using System;
using System.Collections.Generic;
using System.Linq;
using Acr.UserDialogs;
using BeerDrinkin.Core.ViewModels;
using BeerDrinkin.Service.DataObjects;
using CoreGraphics;
using Foundation;
using UIKit;

namespace BeerDrinkin.iOS
{
    partial class SearchViewController : BaseViewController
    {
        private readonly SearchViewModel viewModel = new SearchViewModel();

        public SearchViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            DismissKeyboardOnBackgroundTap();

            SetupUI();
            SetupEvents();
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            if (segue.Identifier != "beerDescriptionSegue")
                return;

            // set in Storyboard
            var navctlr = segue.DestinationViewController as BeerDescriptionTableView;
            if (navctlr == null)
                return;

            var rowPath = tableView.IndexPathForSelectedRow;
            var item = viewModel.Beers[rowPath.Row];
            navctlr.EnableCheckIn = true;
            navctlr.SetBeer(item);
        }

        public void SetupUI()
        {
            Title = BeerDrinkin.Core.Helpers.Strings.SearchTitle;
            lblSearchBeerDrinkin.Text = BeerDrinkin.Core.Helpers.Strings.SearchPlaceHolderTitle;
            lblFindBeers.Text = BeerDrinkin.Core.Helpers.Strings.SearchSubPlaceHolderTitle;

            var textfield = searchBar.Subviews[0].Subviews[1] as UITextField;
            if (textfield != null)
                textfield.Font = UIFont.FromName("Avenir-Book", 14);

            View.BringSubviewToFront(scrllPlaceHolder);
        }

        public void SetupEvents()
        {
            searchBar.TextChanged += SearchBarTextChanged;
            
            searchBar.SearchButtonClicked += async delegate
            {
                UserDialogs.Instance.ShowLoading("Searching");
                await viewModel.SearchForBeersCommand(searchBar.Text);
            };

            viewModel.Beers.CollectionChanged += delegate
            {
                var datasource = new SearchDataSource(viewModel.Beers.ToList());
                datasource.DidSelectBeer += delegate
                {
                    PerformSegue("beerDescriptionSegue", this);
                    tableView.DeselectRow(tableView.IndexPathForSelectedRow, true);
                };


                tableView.Source = datasource;
                tableView.ReloadData();

                datasource.CheckInBeer += async (beer, index) =>
                {
                    var result = await viewModel.QuickCheckIn(beer);
                    var cell = tableView.CellAt(index) as SearchBeerTableViewCell;
                    if (cell != null)
                        cell.isCheckedIn = result;
                };

                UserDialogs.Instance.HideLoading();

                View.BringSubviewToFront(tableView);
                searchBar.ResignFirstResponder();
            };
        }

        /// <summary>
        /// Handles showing the placeholder when the text is null or empty. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="uiSearchBarTextChangedEventArgs"></param>
        void SearchBarTextChanged(object sender, UISearchBarTextChangedEventArgs uiSearchBarTextChangedEventArgs)
        {
            if (!string.IsNullOrEmpty(searchBar.Text))
                return;

            //Setup text and image for scaling animation
            scrllPlaceHolder.Alpha = 0.6f;
            var smallTransform = CGAffineTransform.MakeIdentity();
            smallTransform.Scale(0.6f, 0.6f);
            imgSearch.Transform = smallTransform;
            lblFindBeers.Transform = smallTransform;
            lblSearchBeerDrinkin.Transform = smallTransform;

            //Send table view to back and clear it.
            View.SendSubviewToBack(tableView);
            tableView.Source = new SearchDataSource(new List<BeerItem>());
            tableView.ReloadData();

            //Animate the placeholder 
            var normalTransform = CGAffineTransform.MakeIdentity();
            smallTransform.Scale(1f, 1f);
            UIView.Animate(0.3, 0, UIViewAnimationOptions.TransitionCurlUp,
                () =>
                {
                    scrllPlaceHolder.Alpha = 1;

                    imgSearch.Transform = normalTransform;
                    lblFindBeers.Transform = normalTransform;
                    lblSearchBeerDrinkin.Transform = normalTransform;
                }, () =>
                {
                });
        }

    }
}