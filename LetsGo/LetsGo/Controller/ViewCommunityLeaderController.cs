﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using LetsGo.Model;
using Xamarin.Forms;
using System.ComponentModel;
using Plugin.Media;
using Plugin.Media.Abstractions;

namespace LetsGo.Controller
{
    public partial class ViewCommunityLeaderController
    {
        readonly FirebaseDB fb = new FirebaseDB();
        private CommunityProfile community { get; set; }
        public List<string> Interests { get; set; }
        private string _name { get; set; }
        private string _location { get; set; }
        private string _description { get; set; }
        private string _leader { get; set; }
        private Guid CommunityID { get; set; }
        private string _leaderName { get; set; }
        private Image _img { get; set; }

        public Image CommunityImage
        {
            get
            {
                return _img;
            }
            set
            {
                _img = value;
                OnPropertyChanged(nameof(CommunityImage));
            }
        }
        public string LeaderName
        {
            get
            {
                return _leaderName;
            }
            set
            {
                _leaderName = value;
                OnPropertyChanged(nameof(LeaderName));
            }
        }
        public string Leader
        {
            get
            {
                return _leader;
            }
            set
            {
                _leader = value;
                OnPropertyChanged(nameof(Leader));
            }
        }
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        public string Location
        {
            get
            {
                return _location;
            }
            set
            {
                _location = value;
                OnPropertyChanged(nameof(Location));
            }
        }
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public ViewCommunityLeaderController(CommunityProfile c)
        {
            community = c;
            System.Diagnostics.Debug.WriteLine(c.CommunityID.ToString());
            SetValues(community);
            InitializeComponent();
            ((NavigationPage)Application.Current.MainPage).BarBackgroundColor = Color.LightSteelBlue;
            name.BindingContext = this;
            location.BindingContext = this;
            description.BindingContext = this;
            leader.BindingContext = this;
        }

        public async void SetValues(CommunityProfile community)
        {
            Interests = new List<string>();
            Name = community.Name;
            Location = community.Location;
            Description = community.Description;
            CommunityID = community.CommunityID;
            Leader = community.Leader;
            if (Location == null)
            {
                Location = "No Location Yet...";
            }
            string id = CommunityID.ToString();

            Interests = await fb.GetCommunityInterests(id);
            if (Interests.Count == 0)
            {
                Interests.Add("No interests listed yet...");
            }
            string ln = await fb.GetUsersName(Leader);
            LeaderName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ln);

            //interests.ItemsSource = Interests;




            System.Diagnostics.Debug.WriteLine(id);



            string communityPictureStr = await fb.GetCommunityPicture(id);
            if (communityPictureStr != null)
            {
                imgChosen.Source = ImageSource.FromUri(new Uri(communityPictureStr));
            }
            else
            {
                imgChosen.Source = ImageSource.FromFile("communityimage.jpg");
            }
        }

        public async void OnLeaderName_Clicked(object sender, EventArgs e)
        {
            bool publicLeader = await fb.IsPublicUser(Leader);
            string userEmail = fb.GetCurrentUser();
            bool isFriendOfLeader = await fb.isFriend(Leader);
            // leader is the current user
            if (userEmail == Leader)
            {
                await Navigation.PushAsync(new ProfileController());
            }
            // leader is a friend of current user
            else if (isFriendOfLeader)
            {
                UserProfile friend = await fb.GetUserObject(Leader);
                await Navigation.PushAsync(new FriendProfileController(friend));
            }
            // leader is not a friend of current user and has a public account
            else if (!isFriendOfLeader && publicLeader)
            {
                UserProfile userToVisit = await fb.GetUserObject(Leader);
                await Navigation.PushAsync(new PublicProfileController(userToVisit));
            }
            // leader is not a friend of current user and has a private account
            else
            {
                UserProfile userToVisit = await fb.GetUserObject(Leader);
                await Navigation.PushAsync(new PrivateProfileController(userToVisit));
            }
        }

        MediaFile file;
        public async void Upload_Picture_Clicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();
            try
            {
                file = await Plugin.Media.CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
                {
                    PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium
                });
                if (file == null)
                    return;
                imgChosen.Source = ImageSource.FromStream(() =>
                {
                    var imageStram = file.GetStream();
                    return imageStram;
                });
                string id = CommunityID.ToString();
                string photo = await fb.UploadCommunityPhoto(file.GetStream(), id);
                CommunityImage = imgChosen;

            }
            catch (Exception ex)
            {
                await DisplayAlert("Upload Failed", ex.Message, "OK");
            }

        }
        
    }
}