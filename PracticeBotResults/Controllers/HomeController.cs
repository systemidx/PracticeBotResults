using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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

        private string[] scope = new[] { "openid", "offline_access" };
        private readonly ConfigOptions _config;
        private readonly PracticeBotDbContext _db;

        private const string ObjectIdentifierClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";

        public HomeController(IOptionsSnapshot<ConfigOptions> optionsAccessor, PracticeBotDbContext db)
        {
            _config = optionsAccessor.Value;
            _db = db;
        }

        public async  Task<IActionResult> Index()
        {

            var studentUserId = User.Claims.First(c => c.Type == ObjectIdentifierClaimType).Value;
            var resultsVM = GetResults(studentUserId);
            return View(resultsVM);
            //TokenCache tokenCache = new InMemoryTokenCacheMSAL().GetMsalCacheInstance();
            //ConfidentialClientApplication client = new ConfidentialClientApplication(_config.ClientId, _config.RedirectUrl, new ClientCredential(_config.ClientSecret), tokenCache, null);

            //try
            //{

            //    var userUniqueId = (this.Request.Cookies.ContainsKey("UniqueId")) ? this.Request.Cookies["UniqueId"] : "";
            //    var token = await client.AcquireTokenSilentAsync(scope, client.GetUser(userUniqueId));

            //    ViewBag["Authenticated"] = true;


            //}
            //catch
            //{
            //    ViewBag["Authenticated"] = false;
            //    return View();
            //}

        }

        private IList<ResultsViewModel> GetResults(string studentUserId)
        {
            List<ResultsViewModel> viewModels = _db.Results.Where(r => r.UserId == studentUserId).GroupBy(r => r.CourseName)
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
        

        public async Task<IActionResult> Auth([FromQuery]string code)
        {
            TokenCache tokenCache = new InMemoryTokenCacheMSAL().GetMsalCacheInstance();
            ConfidentialClientApplication client = new ConfidentialClientApplication(_config.ClientId, _config.RedirectUrl, new ClientCredential(_config.ClientSecret), tokenCache, null);
            
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
