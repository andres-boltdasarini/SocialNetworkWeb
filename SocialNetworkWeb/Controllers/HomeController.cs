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
[HttpGet]
public IActionResult Index(int? step = 1)
{
    var model = new MainViewModel();
    
    if (step == 2)
    {
        model.RegisterView = new RegisterViewModel
        {
            Step = 2,
            FirstName = TempData["RegFirstName"]?.ToString() ?? "",
            LastName = TempData["RegLastName"]?.ToString() ?? "",
            Year = TempData["RegYear"] != null ? (int)TempData["RegYear"] : 0,
            Month = TempData["RegMonth"] != null ? (int)TempData["RegMonth"] : 0,
            Date = TempData["RegDate"] != null ? (int)TempData["RegDate"] : 0,
            EmailReg = TempData["RegEmail"]?.ToString() ?? "",
            Login = TempData["RegLogin"]?.ToString() ?? ""
        };
        TempData.Keep();
    }
    else
    {
        model.RegisterView = new RegisterViewModel { Step = 1 };
    }
    
    return View(model);
}
}
}