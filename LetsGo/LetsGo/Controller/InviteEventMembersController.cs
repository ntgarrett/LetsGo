﻿using LetsGo.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Xamarin.Forms;

namespace LetsGo.Controller
{
    public partial class InviteEventMembersController
    {
        private EventProfile Event { get; set; }
        private readonly FirebaseDB fb = new FirebaseDB();
        public List<UserProfile> SearchResults { get; set; }
        public List<string> usersToInvite { get; set; }

        public string SearchStr { get; set; }
        public InviteEventMembersController(EventProfile c)
        {
            Event = c;
            SearchResults = new List<UserProfile>();
            usersToInvite = new List<string>();
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        public async void Search_Text(object sender, EventArgs e)
        {

            SearchStr = searchBar.Text.ToLower();
            List<string> leadersFriends = await fb.GetAllFriends(Event.EventOwner);
            List<UserProfile> friendsProfiles = new List<UserProfile>();
            for (int i = 0; i < leadersFriends.Count; i++)
            {
                UserProfile friend = await fb.GetUserObject(leadersFriends[i]);
                friendsProfiles.Add(friend);
            }
            for (int i = 0; i < friendsProfiles.Count; i++)
            {
                if (friendsProfiles[i].Name.ToLower() == SearchStr)
                {
                    friendsProfiles[i].Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(friendsProfiles[i].Name);
                    SearchResults.Add(friendsProfiles[i]);
                }
            }
            search.ItemsSource = SearchResults;

        }

        public async void OnDone_Clicked(object sender, EventArgs e)
        {
            fb.SendEventInvites(Event, usersToInvite);
            await DisplayAlert("Invites Sent!", "You have sent invites to the users selected to join your Event.", "OK");
            await Navigation.PopAsync();

        }

        public void OnInvite_Clicked(object sender, EventArgs e)
        {
            var item = (Xamarin.Forms.Button)sender;
            item.Text = "Invited!";
            item.TextColor = Color.Green;
            UserProfile userToInvite = (UserProfile)item.CommandParameter;
            if (!usersToInvite.Contains(userToInvite.Email))
                usersToInvite.Add(userToInvite.Email);

        }
    }
}
