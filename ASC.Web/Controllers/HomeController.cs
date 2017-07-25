using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ASC.Web.Configuration;
using Microsoft.AspNetCore.Localization;
using System;
using Microsoft.AspNetCore.Http;

namespace ASC.Web.Controllers
{
    
    public class HomeController : AnonymousController
    {
        private IOptions<ApplicationSettings> _settings;
        public HomeController(IOptions<ApplicationSettings> settings)
        {
            _settings = settings;
        }

        public IActionResult Index()
        {
            // Set Session Test
            // HttpContext.Session.SetSession("Test", _settings.Value);
            // Get Session Test
            // var settings = HttpContext.Session.GetSession<ApplicationSettings>("Test");
            
            // Usage of IOptions
            ViewBag.Title = _settings.Value.ApplicationTitle;
            return View();
        }

        [HttpPost]
        public IActionResult SetCulture(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTime.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "ASC contact page.";
            return View();
        }

        public IActionResult Error(string id)
        {
            if (id == "404")
                return View("NotFound");

            if (id == "401" && User.Identity.IsAuthenticated)
                return View("AccessDenied");
            else
                return RedirectToAction("Login", "Account");

                return View();
        }
    }
}
