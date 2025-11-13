using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkWeb.Models;
using SocialNetworkWeb.Services;
using SocialNetworkWeb.ViewModels;

namespace SocialNetworkWeb.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(IUserRepository userRepository, UserManager<ApplicationUser> userManager)
        {
            _userRepository = userRepository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = _userManager.GetUserId(User);
            var model = await _userRepository.GetUserProfileAsync(userId);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(UserProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var result = await _userRepository.UpdateUserProfileAsync(userId, model);

                if (result)
                {
                    TempData["SuccessMessage"] = "Профиль успешно обновлен";
                    return RedirectToAction("Profile");
                }

                ModelState.AddModelError("", "Ошибка при обновлении профиля");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string searchTerm)
        {
            var currentUserId = _userManager.GetUserId(User);
            var model = new SearchViewModel
            {
                SearchTerm = searchTerm,
                Users = await _userRepository.SearchUsersAsync(searchTerm, currentUserId)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddFriend(string friendId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var result = await _userRepository.SendFriendRequestAsync(currentUserId, friendId);

            if (result)
            {
                TempData["SuccessMessage"] = "Запрос на дружбу отправлен";
            }
            else
            {
                TempData["ErrorMessage"] = "Не удалось отправить запрос на дружбу";
            }

            return RedirectToAction("Search", new { searchTerm = Request.Query["searchTerm"] });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFriend(string friendId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var result = await _userRepository.RemoveFriendAsync(currentUserId, friendId);

            if (result)
            {
                TempData["SuccessMessage"] = "Пользователь удален из друзей";
            }
            else
            {
                TempData["ErrorMessage"] = "Не удалось удалить пользователя из друзей";
            }

            return RedirectToAction("Search", new { searchTerm = Request.Query["searchTerm"] });
        }
    }
}