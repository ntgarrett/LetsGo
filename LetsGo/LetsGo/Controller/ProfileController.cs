﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Xamarin.Forms;
using LetsGo.Model;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace LetsGo.Controller
{
    public partial class ProfileController : ContentPage, INotifyPropertyChanged
    {
        private ProfilePage profile = new ProfilePage();

        private FirebaseDB fb = new FirebaseDB();
        private string _name { get; set; }
        private string _location { get; set; }
        private List<string> _interests { get; set; }

        private Image _img { get; set; }

        private UserProfile user { get; set; }
        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

        public ProfileController()
        {
            SetValues();
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            
            name.BindingContext = this;
            location.BindingContext = this;
            
        }

        protected override void OnAppearing()
        {
            SetValues();
            base.OnAppearing();
        }

        public ProfileController(UserProfile profile)
        {
            user = profile;
            SetValues();
            InitializeComponent();
            ((Xamarin.Forms.NavigationPage)Xamarin.Forms.Application.Current.MainPage).BarBackgroundColor = Color.FromHex("#51aec2");

            name.BindingContext = this;
            location.BindingContext = this;

        }

        public List<string> Interests { get; set; }

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

        public Image ProfilePicture
        {
            get
            {
                return _img;
            }
            set
            {
                _img = value;
                OnPropertyChanged(nameof(ProfilePicture));
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

        
        private async void SetValues()
        {
            Name = await fb.GetUsersName();
            Name = textInfo.ToTitleCase(Name);
            Location = await fb.GetUsersLocation();
            if (Location != null)
                Location = textInfo.ToTitleCase(Location);
            else
            {
                Location = "No Location Yet...";
            }
            Interests = new List<string>();
            Interests =  await fb.GetUsersInterests();
            
            if (Interests.Count == 0)
            {
                
                Interests.Add("No interests listed yet...");

            }
            interests.ItemsSource = Interests;

            string profilePictureStr = await fb.GetProfilePicture();
            if (profilePictureStr != null)
            { 
                profilePicture.Source = ImageSource.FromUri(new Uri(profilePictureStr));
            }
            else
            {
                profilePicture.Source = ImageSource.FromFile("defaultProfilePic.jpg");
            }
            ProfilePicture = profilePicture;

        }

        public async void Logout_Clicked(object sender, EventArgs e)
        {
            bool done = profile.LogoutUser();
            if (done)
                await Navigation.PopToRootAsync();

        }

        public async void ChangePass_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ChangePasswordController());
        }

        public async void UpdateProfile_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new UpdateProfileController());
        }
    }
}
