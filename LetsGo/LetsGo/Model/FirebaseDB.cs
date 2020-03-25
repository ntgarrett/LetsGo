﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using System.Linq;
using LetsGo.Model.Authentication;
using System.Net.Mail;
using System.Text;

namespace LetsGo.Model
{
    public class FirebaseDB
    {
        private readonly FirebaseClient firebase = new FirebaseClient("https://letsgo-f4d0d.firebaseio.com/");

        public async Task<bool> LoginUser(string email, string password)
        {
            List<UserProfile> users = await GetAllUsers();
            var CurrentUser = users.Where(a => a.Email == email).FirstOrDefault();
            if (CurrentUser == null)
            {
                return false;
            }
            else if (email == CurrentUser.Email && EncryptDecrypt(password,5) == CurrentUser.Password)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        
        public bool SignOutUser()
        {
            return true;
        }

        public async Task<bool> InitializeUser(string uName, DateTime uDOB, string uEmail, string password, bool publicAcct)
        {
            List<UserProfile> users = await GetAllUsers();

            var found = users.Where(a => a.Email == uEmail).FirstOrDefault();
            if (found != null)
            {
                return false;
            }
            string encodedPass = EncryptDecrypt(password, 5);
            UserProfile newUser = new UserProfile(uName.ToLower(), uDOB, uEmail.ToLower(), encodedPass, publicAcct);
            await firebase
              .Child("userprofiles")
              .PostAsync(newUser);
            return true;
        }

        public async Task<bool> InitializeEvent(string eName, string edetails, DateTime eDate, TimeSpan eStart, TimeSpan eEnd, string eMail , bool publicAcct)
        {
            EventProfile newEvent = new EventProfile(eName.ToLower(), edetails.ToLower(), eDate, eStart, eEnd, eMail, publicAcct);
            await firebase
              .Child("Events")
              .PostAsync(newEvent);
            return true;
        }

        public async Task<bool> SendPasswordRecoverEmail(string email)
        {

            try
            {

                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress("letsgo.noreply441@gmail.com");
                mail.To.Add(email);
                mail.Subject = "LetsGo: Password Recovery";
                mail.Body = "You are receiving this email to reset your LetsGo password. Your new temporary" +
                            " password is: " + "'LetsGoResetPassword441'. Use this new password to login. You may reset your password once logged in" +
                            " with this temporary password.";

                SmtpServer.Port = 587;
                SmtpServer.Host = "smtp.gmail.com";
                SmtpServer.EnableSsl = true;
                SmtpServer.UseDefaultCredentials = false;
                SmtpServer.Credentials = new System.Net.NetworkCredential("letsgo.noreply441@gmail.com" , "LetsGo123");

                SmtpServer.Send(mail);

                List<UserProfile> users = await GetAllUsers();
                UserProfile CurrentUser = users.Where(a => a.Email == email).FirstOrDefault();

                var userToUpdate = (await firebase
                    .Child("userprofiles")
                    .OnceAsync<UserProfile>()).Where(a => a.Object.Email == email).FirstOrDefault();

                await firebase.Child("userprofiles").Child(userToUpdate.Key)
                    .PutAsync(new UserProfile() { Name = CurrentUser.Name, Email = CurrentUser.Email, DateOfBirth = CurrentUser.DateOfBirth, Password = EncryptDecrypt("LetsGoResetPassword441", 5), Location = CurrentUser.Location, Interests = CurrentUser.Interests, PublicAcct = CurrentUser.PublicAcct });
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public async Task<List<UserProfile>> GetAllUsers()
        {
            var users = (await firebase
                    .Child("userprofiles")
                    .OnceAsync<UserProfile>()).Select(item => new UserProfile
                    {
                        Name = item.Object.Name,
                        Email = item.Object.Email,
                        DateOfBirth = item.Object.DateOfBirth,
                        Password = item.Object.Password,
                        Location = item.Object.Location,
                        Interests = item.Object.Interests,
                        PublicAcct = item.Object.PublicAcct
                    }).ToList();

            return users;
        }

        public async Task<bool> UpdateUserProfile(string uName, string location, bool publicAccount, List<string> interestList)
        {

            string CurrentUserEmail = GetCurrentUser();
            var userToUpdate = (await firebase
                .Child("userprofiles")
                .OnceAsync<UserProfile>()).Where(a => a.Object.Email == CurrentUserEmail).FirstOrDefault();

            List<UserProfile> users = await GetAllUsers();



            var CurrentUser = users.Where(a => a.Email == CurrentUserEmail).FirstOrDefault();
            DateTime date = CurrentUser.DateOfBirth;
            string email = CurrentUser.Email;
            List<string> interests;

            if (CurrentUser.Email != email)
                return false;

            interests = new List<string>();
            for (int i = 0; i < interestList.Count; i++)
            {
                if (!interests.Contains(interestList.ElementAt(i).ToLower()))
                    interests.Add(interestList.ElementAt(i).ToLower());
            }



            await firebase
            .Child("userprofiles")
            .Child(userToUpdate.Key)
            .PutAsync(new UserProfile() { Name = uName.ToLower(), Email = CurrentUser.Email, DateOfBirth = CurrentUser.DateOfBirth, Password = CurrentUser.Password, Location = location.ToLower(), Interests = interests, PublicAcct = publicAccount });
            return true;
        }

        public void SetCurrentUser(string email)
        {
            var auth = DependencyService.Get<IFirebaseAuthenticator>();
            auth.SetCurrentUser(email);
        }

        public string GetCurrentUser()
        {
            var auth = DependencyService.Get<IFirebaseAuthenticator>();
            return auth.GetCurrentUser();
        }


        private async Task<UserProfile> User()
        {
            string CurrentUserEmail = GetCurrentUser();
            var userToUpdate = (await firebase
                .Child("userprofiles")
                .OnceAsync<UserProfile>()).Where(a => a.Object.Email == CurrentUserEmail).FirstOrDefault();

            List<UserProfile> users = await GetAllUsers();

            var CurrentUser = users.Where(a => a.Email == CurrentUserEmail).FirstOrDefault();
            return CurrentUser;
        }

        public async Task<bool> HasPublicAccount()
        {
            UserProfile current = await User();

            return current.PublicAcct;
        }

        public async Task<string> GetUsersLocation()
        {
            UserProfile current = await User();

            return current.Location;
        }

        public async Task<string> GetUsersName()
        {
            UserProfile current = await User();

            return current.Name;
        }

        public async Task<List<string>> GetUsersInterests()
        {
            UserProfile current = await User();

            return current.Interests;

        }

        public async Task<bool> DeleteUserAccount()
        {
            string current = GetCurrentUser();
            var userToDelete = (await firebase
                .Child("userprofiles")
                .OnceAsync<UserProfile>()).Where(a => a.Object.Email == current).FirstOrDefault();

            await firebase.Child("userprofiles").Child(userToDelete.Key).DeleteAsync();
            return true;
        }

        public async Task<bool> ChangePassword(string oldPassword, string NewPassword)
        {
            List<UserProfile> users = await GetAllUsers();
            string CurrentUserEmail = GetCurrentUser();
            var CurrentUser = users.Where(a => a.Email == CurrentUserEmail).FirstOrDefault();

            if (oldPassword != EncryptDecrypt(CurrentUser.Password, 5))
                return false;
            var userToUpdate = (await firebase
                .Child("userprofiles")
                .OnceAsync<UserProfile>()).Where(a => a.Object.Email == CurrentUserEmail).FirstOrDefault();

            await firebase
                        .Child("userprofiles")
                        .Child(userToUpdate.Key)
                        .PutAsync(new UserProfile() { Name = CurrentUser.Name, Email = CurrentUser.Email, DateOfBirth = CurrentUser.DateOfBirth, 
                            Password = EncryptDecrypt(NewPassword, 5), Location = CurrentUser.Location, Interests = CurrentUser.Interests, PublicAcct = CurrentUser.PublicAcct });
            return true;
        }

        private string EncryptDecrypt(string szPlainText, int szEncryptionKey)
        {
            StringBuilder szInputStringBuild = new StringBuilder(szPlainText);
            StringBuilder szOutStringBuild = new StringBuilder(szPlainText.Length);
            char Textch;
            for (int iCount = 0; iCount < szPlainText.Length; iCount++)
            {
                Textch = szInputStringBuild[iCount];
                Textch = (char)(Textch ^ szEncryptionKey);
                szOutStringBuild.Append(Textch);
            }
            return szOutStringBuild.ToString();
        }

    }
}
