using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Facebook;
using Windows.Security.Authentication.Web;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace TheShoppingList.Social
{
    public class FacebookClient : Facebook.FacebookClient
    {
        public bool IsLoggedIn { get; set; }

        public string ID { get; set; }

        public string Secret { get; set; }

        public string CallbackUrl { get; set; }


        public FacebookClient()
        {
            ID = "441221599270207";
            CallbackUrl = "https://guarded-shelf-4296.herokuapp.com/";
            AccessToken = Windows.Storage.ApplicationData.Current.RoamingSettings.Values["Token"] as string;
            if (string.IsNullOrEmpty(AccessToken) || AccessToken.Length <=25)
                IsLoggedIn = false;
            else
                IsLoggedIn = true;
        }

        public async Task<bool> Login()
        {

            if (string.IsNullOrEmpty(ID))
            {
                new MessageDialog("Please enter a Client ID.").ShowAsync();
            }
            else if (string.IsNullOrEmpty(CallbackUrl))
            {
                new MessageDialog("Please enter a Callback URL.").ShowAsync();
            }

            try
            {
                String FacebookURL = "https://www.facebook.com/dialog/oauth?client_id=" + Uri.EscapeDataString(ID) +
                                     "&redirect_uri=" + Uri.EscapeDataString(CallbackUrl) +
                                     "&scope=read_stream,publish_stream&display=popup&response_type=token";

                var StartUri = new Uri(FacebookURL);
                var EndUri = new Uri(CallbackUrl);

                //new MessageDialog("Navigating to: " + FacebookURL).ShowAsync();

                WebAuthenticationResult WebAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(
                    WebAuthenticationOptions.None,
                    StartUri,
                    EndUri);
                if (WebAuthenticationResult.ResponseData.Contains("denied") == true)
                    return false;
                if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success)
                {
                    IsLoggedIn = true;
                    var tmp = WebAuthenticationResult.ResponseData.ToString();
                    int start = tmp.IndexOf("=", System.StringComparison.Ordinal) + 1;
                    for (int i = start; i < tmp.Length; i++)
                    {
                        if (tmp[i] == '&')
                            break;
                        AccessToken += tmp[i];
                    }


                    if (AccessToken.Length <= 25)
                        return true;
                    Windows.Storage.ApplicationData.Current.RoamingSettings.Values["Token"] = AccessToken;
                    return true;
                }
                if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
                {
                    await
                        new MessageDialog("HTTP Error returned by AuthenticateAsync() : " +
                                          WebAuthenticationResult.ResponseErrorDetail.ToString()).ShowAsync();
                }
                else
                {
                    await
                        new MessageDialog("Error returned by AuthenticateAsync() : " +
                                          WebAuthenticationResult.ResponseStatus.ToString()).ShowAsync();
                }
                return false;
            }
            catch (Exception exception)
            {
                return false;
                //
                // Bad Parameter, SSL/TLS Errors and Network Unavailable errors are to be handled here.
                //
                //DebugPrint(Error.ToString());
            }
        }

        public string GetUrlImage(string username)
        {
            return "https://graph.facebook.com/" + username + "/picture?type=large";

        }

        public async Task<bool> Logout()
        {
            Uri LogoutUri =
                    GetLogoutUrl(
                        new
                        {
                            access_token = AccessToken,
                            next = "https://www.facebook.com/connect/login_success.html"
                        });
            var StartUri = new Uri(LogoutUri.ToString());
            var EndUri = new Uri(CallbackUrl);


            WebAuthenticationResult WebAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(
                WebAuthenticationOptions.None,
                StartUri,
                EndUri);
            if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success)
            {
                IsLoggedIn = false;
                Windows.Storage.ApplicationData.Current.RoamingSettings.Values["Token"] = null;
                return true;
            }
            new MessageDialog("Logout Failed").ShowAsync();
            return false;

        }

        public async Task<bool> PostMessage(string username, PostDetails details)
        {
            try
            {
                var args = new Dictionary<string, object>();
                args["name"] = details.Name ?? string.Empty;
                args["name"] = details.Name ?? string.Empty;
                args["link"] = details.Link ?? string.Empty;
                args["caption"] = details.Caption ?? string.Empty;
                args["description"] = details.Description ?? string.Empty;
                args["picture"] = details.PictureUrl ?? string.Empty;
                args["message"] = details.Message ?? string.Empty;
                args["actions"] = "";

                await PostTaskAsync("/" + username + "/feed", args);
                await new MessageDialog("Status set successfully").ShowAsync();
                return true;
            }
            catch (Exception exception)
            {
                if (exception.Message.Contains("duplicate"))
                    new MessageDialog("You already posted this status!").ShowAsync();
                else if (exception.Message.Contains("could no resolve"))
                    new MessageDialog("Please connect to the internet before posting a shopping list!").ShowAsync();
                return false;
            }
        }

        public async Task<UserDetails> GetUserDetails(string userName)
        {
            dynamic user = await GetTaskAsync(userName);
            var userDetails = new UserDetails();
            if (user.name != null)
                userDetails.Name = (string)user.name;
            if (user.first_name != null)
                userDetails.FirstName = (string)user.first_name;
            if (user.last_name != null)
                userDetails.LastName = (string)user.last_name;
            if (user.link != null)
                userDetails.ProfileUrl = (string)user.link;
            if (user.username != null)
                userDetails.Username = (string)user.username;
            if (user.gender != null)
                userDetails.Gender = (string)user.gender;
            if (user.id != null)
                userDetails.Id = (string)user.id;
            return userDetails;
        }
    }
}
