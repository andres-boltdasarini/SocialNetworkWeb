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
        private readonly ILogger<HomeController> _logger;

        private IMapper _mapper;

        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;



        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("Register")]
        [HttpGet]
        public IActionResult Register()
        {
            return View("Home/Register");
        }

        [Route("RegisterPart2")]
        [HttpGet]
        public IActionResult RegisterPart2(RegisterViewModel model)
        {
            return View("RegisterPart2", model);
        }
    }
}
