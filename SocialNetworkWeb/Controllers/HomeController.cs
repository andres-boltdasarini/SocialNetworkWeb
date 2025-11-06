using System.Diagnostics;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkWeb.Models;
using SocialNetworkWeb.Models.Users;

namespace SocialNetworkWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var model = new HomeViewModel
            {
                LoginView = new LoginViewModel(),
                RegisterView = new RegisterViewModel()
            };
            return View(model);
        }
    }
}
