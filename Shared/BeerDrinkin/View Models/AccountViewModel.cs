﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using BeerDrinkin.Service.DataObjects;

namespace BeerDrinkin.Core.ViewModels
{
    public class AccountViewModel : BaseViewModel
    {
        HeaderInfo headerInfo;
        HeaderInfo HeaderInfo
        {
            get
            {
                return headerInfo;
            }
            set
            {    
                SetProperty(ref headerInfo, value);
                headerInfo = value;
            }
        }

        List<string> beerPhotosUrls;
        public List<string> BeerPhotosUrls
        {
            get
            {                 
                return beerPhotosUrls;
            }
            set
            {
                beerPhotosUrls = value;
                SetProperty(ref beerPhotosUrls, value);
            }
        }

        bool busy;

        public AccountViewModel()
        {
            PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                if(e.PropertyName == "HeaderInfo")
                {
                    RefreshProperties();
                }
                System.Diagnostics.Debug.WriteLine(e.PropertyName);
            };
        }

        void RefreshProperties()
        {
            var user = ClientManager.Instance.BeerDrinkinClient.CurrentAccount;

            RatingsCount = headerInfo.Ratings.ToString();
            BeerCount = headerInfo.CheckIns.ToString();
            PhotoCount = headerInfo.Photos.ToString();
            FirstName = user.FirstName;
            AvararUrl = user.AvatarUrl;
            FullName = string.Format("{0} {1}", user.FirstName, user.LastName);
        }

        public async Task FetchData(bool forceRemoteRefresh = false)
        {   
            //Are we already fetching data? 
            if (busy == true)
                return;
            busy = true;

            try
            {
                HeaderInfo = await GetRemoteHeaderInfo();
                BeerPhotosUrls = await GetRemoteBeerPhotosUrls();

            }
            catch(Exception ex)
            {
                Xamarin.Insights.Report(ex);
            }
            finally
            {
                busy = false;
            }
        }

        async Task<List<string>> GetRemoteBeerPhotosUrls()
        {    
            //Fetch base64 strings for images for the currently signed in user. 
            var response = await ClientManager.Instance.BeerDrinkinClient.GetPhotosForUser();

            if (response.Error == null)
            {               
                return response.Result;
            }
            return new List<string>();
        }

        async Task<HeaderInfo> GetRemoteHeaderInfo()
        {
            HeaderInfo header;

            var result = await ClientManager.Instance.BeerDrinkinClient.GetUsersHeaderInfoAsync(ClientManager.Instance.BeerDrinkinClient.GetUserId);
            header = result.Result;
            //Store it for next time

            return header;
        }

        #region Properties 

        string ratingsCount;
        public string RatingsCount
        {
            get
            {
                return ratingsCount;
            }
            set
            {
                SetProperty(ref ratingsCount, value);
                ratingsCount = value;
            }
        }

        string photosCount;
        public string PhotoCount
        {
            get
            {
                return photosCount;
            }
            set
            {
                SetProperty(ref photosCount, value);
                photosCount = value;
            }
        }

        string beerCount;
        public string BeerCount
        {
            get
            {
                return beerCount;
            }
            set
            {                
                SetProperty(ref beerCount, value);
                beerCount = value;
            }
        }

        string firstName;
        public string FirstName
        {
            get
            {
                return firstName;
            }
            set
            {                
                SetProperty(ref firstName, value);
                firstName = value;
            }
        }

        string fullName;
        public string FullName
        {
            get
            {
                return fullName;
            }
            set
            {                
                SetProperty(ref fullName, value);
                fullName = value;
            }
        }

        string avatarUrl = string.Empty;
        public string AvararUrl
        {
            get
            {
                return avatarUrl;
            }
            set
            {                
                SetProperty(ref avatarUrl, value);
                avatarUrl = value;
            }
        }


        #endregion

    }
}

