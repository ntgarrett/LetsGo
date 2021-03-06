﻿using LetsGo.Model;
using LetsGo.Model.Authentication;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace LetsGo.Controller
{
    public partial class CommunityAnnouncementsController
    {
        readonly FirebaseDB fb = new FirebaseDB();
        private CommunityProfile thisCommunity { get; set; }

        public List<Announcement> AnnouncementList { get; set; }
        public CommunityAnnouncementsController()
        {
            var auth = DependencyService.Get<IFirebaseAuthenticator>();
            thisCommunity = auth.GetCurrentCommunity();
            AnnouncementList = new List<Announcement>();
            SetValues();

            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        public async void SetValues()
        {

            AnnouncementList = await fb.GetCommunityAnnouncements(thisCommunity.CommunityID);
            if (AnnouncementList == null || AnnouncementList.Count == 0)
            {
                AnnouncementList = new List<Announcement>();
                AnnouncementList.Add(new Announcement(thisCommunity.CommunityID, "No Announcements"));
            }
            announcements.ItemsSource = AnnouncementList;
        }
    }
}
