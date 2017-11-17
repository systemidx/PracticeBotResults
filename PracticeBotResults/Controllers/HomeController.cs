using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PracticeBotResults.Models;
using Microsoft.Identity.Client;
using PracticeBotResults.Utility;
using Microsoft.Extensions.Options;
using PracticeBotResults.ViewModels;

namespace PracticeBotResults.Controllers
{
    public class HomeController : Controller
    {
        private const string AUTHORITY = "https://login.microsoftonline.com/common";

        private string[] scope = new string[] {"User.Read"};
        private readonly ConfigOptions _config;
        private readonly PracticeBotDbContext _db;

        private const string ObjectIdentifierClaimType =
            "http://schemas.microsoft.com/identity/claims/objectidentifier";

        public HomeController(IOptionsSnapshot<ConfigOptions> optionsAccessor, PracticeBotDbContext db)
        {
            _config = optionsAccessor.Value;
            _db = db;
        }

        public async Task<IActionResult> Index(string upn)
        {
            string userUniqueId = this.Request.Cookies.ContainsKey("UniqueId") ? this.Request.Cookies["UniqueId"] : "";
            if (string.IsNullOrEmpty(userUniqueId))
            {
                ViewData["Authenticated"] = false;
                return View();
            }

            TokenCache tokenCache = new MSALSessionCache(userUniqueId, HttpContext).GetMsalCacheInstance();
            ConfidentialClientApplication client = new ConfidentialClientApplication(_config.ClientId,
                _config.RedirectUrl, new ClientCredential(_config.ClientSecret), tokenCache, null);

            IUser user = client.GetUser(userUniqueId);
            if (user == null)
            {
                ViewData["Authenticated"] = false;
                return View();
            }

            AuthenticationResult token = await client.AcquireTokenSilentAsync(scope, user);

            ViewData["Authenticated"] = token != null;
            return View(GetResults(upn).ToList()); //todo: query db and get scores            
        }

        private IList<ResultsViewModel> GetResults(string studentUserId)
        {
            List<ResultsViewModel> viewModels = _db.Results.Where(r => r.UserId == studentUserId)
                .GroupBy(r => r.CourseName)
                .Select(group => new ResultsViewModel
                {
                    CourseName = group.Key,
                    Assessments = group.GroupBy(g => g.AssessmentName).Select(a =>
                        new AssessmentViewModel
                        {
                            AssessmentTitle = a.Key,
                            Correct = a.Where(q => q.IsCorrect).Count(),
                            QuestionsTotal = a.Count()
                        }
                    ).ToList()
                }).ToList();

            return viewModels;
        }


        public async Task<IActionResult> Auth([FromQuery] string code)
        {
            TokenCache tokenCache = new MSALSessionCache(null, HttpContext).GetMsalCacheInstance();
            ConfidentialClientApplication client = new ConfidentialClientApplication(_config.ClientId,
                _config.RedirectUrl, new ClientCredential(_config.ClientSecret), tokenCache, null);

            AuthenticationResult token;
            if (code != null)
            {
                token = await client.AcquireTokenByAuthorizationCodeAsync(code, scope);
                tokenCache = new MSALSessionCache(token.UniqueId, HttpContext).GetMsalCacheInstance();
                client = new ConfidentialClientApplication(_config.ClientId, _config.RedirectUrl,
                    new ClientCredential(_config.ClientSecret), tokenCache, null);
                token = await client.AcquireTokenByAuthorizationCodeAsync(code, scope);
                this.Response.Cookies.Append("UniqueId", token.UniqueId);

                return View(true);
            }

            if (!this.Request.Cookies.ContainsKey("UniqueId"))
            {
                Uri uri = await client.GetAuthorizationRequestUrlAsync(scope, null, null);

                return Redirect(uri.ToString());
            }

            IUser user = client.GetUser(this.Request.Cookies["UniqueId"]);
            if (user == null)
            {
                Uri uri = await client.GetAuthorizationRequestUrlAsync(scope, null, null);

                return Redirect(uri.ToString());
            }

            token = await client.AcquireTokenSilentAsync(scope, user);

            return View(true);
        }
    }
}