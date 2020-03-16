﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetsGo.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using LetsGo.Model.Authentication;
using System.ComponentModel;



namespace LetsGo.Model
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public async Task<string> loginUser(string email, string pass)
        {
            //string eMail = email.Text;
            //string pass = password.Text;

            var auth = DependencyService.Get<IFirebaseAuthenticator>();

            string token = await auth.LoginWithEmailPassword(email, pass);
            return token;
            /* //TAKING OUT AND MOVING TO CONTROLLER
            if (token.Contains("There is no user record") || token == "")
            {
                if (token != "")
                    await DisplayAlert("Login Unsuccessful", token, "OK");
                else
                    await DisplayAlert("Login Unsuccessful", "Invalid login credentials", "OK");

            }
            else
            {
                await DisplayAlert("Login Successful", "User has logged in", "OK");
            }
            */

               
        }

        public async Task<string> createAccount(string email, string pass)
        {

            //string eMail = email.Text;
            //string pass = password.Text;

            var auth = DependencyService.Get<IFirebaseAuthenticator>();

            string token = await auth.RegisterWithEmailPassword(email, pass);
            return token;
            /* TAKING OUT AND MOVING TO CONTROLLER
            if (token != "")
            {
                await DisplayAlert("Success", "User account created.", "OK");
            }
            else
            {
                await DisplayAlert("Failure", "User account could not be created", "OK");
            }
            */
            
        }
    }
}