using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PracticeBotResults.Models;
using Microsoft.Identity.Client;
using PracticeBotResults.Utility;

namespace PracticeBotResults.Controllers
{
    public class HomeController : Controller
    {
        private const string AUTHORITY = "https://login.microsoftonline.com/common";
        private string clientId;
        private string redirectUrl;
        private string clientSecret;

        private string[] scope = new[] { "openid", "offline_access" };

        public async  Task<IActionResult> Index()
        {
            TokenCache tokenCache = new InMemoryTokenCacheMSAL().GetMsalCacheInstance();
            ConfidentialClientApplication client = new ConfidentialClientApplication(clientId, redirectUrl, new ClientCredential(clientSecret), tokenCache, null);

            try
            {

                var userUniqueId = (this.Request.Cookies.ContainsKey("UniqueId")) ? this.Request.Cookies["UniqueId"] : "";
                var token = await client.AcquireTokenSilentAsync(scope, client.GetUser(userUniqueId));

                ViewBag["Authenticated"] = true;
                return View(); //todo: query db and get scores
            }
            catch
            {
                ViewBag["Authenticated"] = false;
                return View();
            }
            
        }

        public async Task<IActionResult> Auth([FromQuery]string code)
        {
            TokenCache tokenCache = new InMemoryTokenCacheMSAL().GetMsalCacheInstance();
            ConfidentialClientApplication client = new ConfidentialClientApplication(clientId, redirectUrl, new ClientCredential(clientSecret), tokenCache, null);
            
            if (code != null)
            {
                var token = await client.AcquireTokenByAuthorizationCodeAsync(code, scope);
                this.Response.Cookies.Append("UniqueId", token.UniqueId);
                
                return View(true);
            }
            else
            {
                try
                {
                    var userUniqueId = (this.Request.Cookies.ContainsKey("UniqueId")) ? this.Request.Cookies["UniqueId"] : "";
                    var token = await client.AcquireTokenSilentAsync(scope, client.GetUser(userUniqueId));

                    return View(true);
                }
                catch (Exception e) // lookup exception
                {
                    var uri = await client.GetAuthorizationRequestUrlAsync(scope, null, null);

                    return Redirect(uri.ToString());
                }
            }

            
        }
    }
}
