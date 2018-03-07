using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using Windows.Web.Http;
using Newtonsoft.Json;
using Xamarin.Auth;

namespace GoogleLogin
{
    public class Google
    {
        private const string AuthorizeUrl = "https://accounts.google.com/o/oauth2/auth";
        private const string AccessTokenUrl = "https://accounts.google.com/o/oauth2/token";
        private const string ProfileUrl = "https://www.googleapis.com/plus/v1/people/me";
        private const string Scope = "email profile";

        private string ClientId = "219429978628-tu0vj720uokgleh8epdmpipogo6tqlaj.apps.googleusercontent.com";
        private const string ClientSecret = "cZWRiRVbOhD_9qPtvBdi62Ps";
        private const string CallbackUri = "http://www.ibvsolutions.com";

        public async Task<GoogleUser> AuthenticateAsync()
        {

            //https://developers.google.com/api-client-library/dotnet/get_started

            //Get authorize token
            string googleUrl = AuthorizeUrl + "?client_id=" + Uri.EscapeDataString(ClientId);
            googleUrl += "&redirect_uri=" + Uri.EscapeDataString(CallbackUri);
            googleUrl += "&response_type=code";
            googleUrl += "&scope=" + Uri.EscapeDataString(Scope);

            Uri startUri = new Uri(googleUrl);
            WebAuthenticationResult result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, startUri, new Uri(CallbackUri));
            string code = result.ResponseStatus != WebAuthenticationStatus.Success ? null : result.ResponseData.Substring(result.ResponseData.IndexOf('=') + 1);
            if (code != null) code = Uri.UnescapeDataString(code);
            if (code == null) return null;

            //Fetch the oAuth2 token
            var httpClient = new HttpClient();
            var content = new HttpFormUrlEncodedContent(new Dictionary<string, string>
            {
                {"code", code},
                {"client_id", ClientId},
                {"client_secret", ClientSecret},
                {"redirect_uri", CallbackUri},
                {"grant_type", "authorization_code"},
            });

            HttpResponseMessage accessTokenResponse = await httpClient.PostAsync(new Uri(AccessTokenUrl), content);
            Dictionary<string, string> responseDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(accessTokenResponse.Content.ToString());
            Account account= new Account(null, responseDict);

            //Get user info
            try
            {
                var request = new OAuth2Request("GET", new Uri(ProfileUrl), null, account);
                var response = await request.GetResponseAsync();
                if (response == null) return null;

                var userJson = response.GetResponseText();
                var user = JsonConvert.DeserializeObject<GoogleUser>(userJson);
                return user;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public class GoogleUser
        {
            public string displayName { get; set; }
            public List<GoogleEmail> emails { get; set; }
            public string etag { get; set; }
            public string id { get; set; }
            public GoogleImage image { get; set; }
            public bool isPlusUser { get; set; }
            public string kind { get; set; }
            public string language { get; set; }
            public GoogleName name { get; set; }
            public string objectType { get; set; }
            public bool verified { get; set; }
        }

        public class GoogleEmail
        {
            public string type { get; set; }
            public string value { get; set; }
        }

        public class GoogleName
        {
            public string familyName { get; set; }
            public string givenName { get; set; }
        }

        public class GoogleImage
        {
            public bool isDefault { get; set; }
            public string url { get; set; }
        }
    }
}
