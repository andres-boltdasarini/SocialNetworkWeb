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
        private readonly UserManager<ApplicationUser> _userManager; // Можно оставить для GetUserId
        // ApplicationDbContext УБРАН!

        public UserController(
            IUserRepository userRepository,
            UserManager<ApplicationUser> userManager)
        {
            _userRepository = userRepository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            // Используем репозиторий вместо UserManager
            var user = await _userRepository.GetCurrentUserAsync(User);
            
            if (user == null)
            {
                return NotFound();
            }

            var model = new UserProfileViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                Bio = user.Bio ?? string.Empty
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(UserProfileViewModel model)
        {
            Console.WriteLine("=== Profile POST ===");

            // Временная отладочная проверка - пропускаем валидацию
            // if (!ModelState.IsValid)
            // {
            //     Console.WriteLine("ModelState not valid");
            //     return View(model);
            // }

            try
            {
                // Получаем пользователя через репозиторий
                var user = await _userRepository.GetCurrentUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                Console.WriteLine($"Before update: FirstName='{user.FirstName}', Email='{user.Email}'");
                Console.WriteLine($"From form: FirstName='{model.FirstName}', Email='{model.Email}'");

                // Обновляем поля
                user.FirstName = model.FirstName?.Trim();
                user.LastName = model.LastName?.Trim();
                user.PhoneNumber = model.PhoneNumber?.Trim();
                user.Bio = model.Bio?.Trim() ?? string.Empty;

                // ВСЁ сохраняем за один вызов через репозиторий
                var saveResult = await _userRepository.SaveUserWithEmailUpdateAsync(user, model.Email?.Trim());
                Console.WriteLine($"SaveUserWithEmailUpdateAsync result: {saveResult}");

                if (!saveResult)
                {
                    ModelState.AddModelError("", "Не удалось обновить профиль");
                    return View(model);
                }

                // Обновляем аутентификацию через репозиторий
                await _userRepository.RefreshSignInAsync(user);

                // Получаем свежие данные через репозиторий
                var freshUser = await _userRepository.FindByIdAsync(user.Id);
                Console.WriteLine($"After update: FirstName='{freshUser?.FirstName}', Email='{freshUser?.Email}'");

                TempData["SuccessMessage"] = "Профиль успешно обновлен";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                ModelState.AddModelError("", $"Ошибка: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Search(string searchTerm)
        {
            var currentUserId = _userManager.GetUserId(User); // Можно оставить это через UserManager
            
            var users = await _userRepository.SearchUsersWithFriendshipInfoAsync(searchTerm, currentUserId);
            
            var model = new SearchViewModel
            {
                SearchTerm = searchTerm,
                Users = users
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddFriend(string friendId)
        {
            var currentUserId = _userManager.GetUserId(User);
            
            if (string.IsNullOrEmpty(currentUserId) || string.IsNullOrEmpty(friendId))
            {
                TempData["ErrorMessage"] = "Неверные параметры запроса";
                return RedirectToAction("Search");
            }
            
            var result = await _userRepository.SendFriendRequestAsync(currentUserId, friendId);

            TempData[result ? "SuccessMessage" : "ErrorMessage"] = 
                result ? "Запрос на дружбу отправлен" : "Не удалось отправить запрос на дружбу";

            return RedirectToAction("Search", new { searchTerm = Request.Query["searchTerm"] });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFriend(string friendId)
        {
            var currentUserId = _userManager.GetUserId(User);
            
            if (string.IsNullOrEmpty(currentUserId) || string.IsNullOrEmpty(friendId))
            {
                TempData["ErrorMessage"] = "Неверные параметры запроса";
                return RedirectToAction("Search");
            }
            
            var result = await _userRepository.RemoveFriendAsync(currentUserId, friendId);

            TempData[result ? "SuccessMessage" : "ErrorMessage"] = 
                result ? "Пользователь удален из друзей" : "Не удалось удалить пользователя из друзей";

            return RedirectToAction("Search", new { searchTerm = Request.Query["searchTerm"] });
        }

        [HttpPost]
        public async Task<IActionResult> AcceptFriendRequest(string friendId)
        {
            var currentUserId = _userManager.GetUserId(User);
            
            if (string.IsNullOrEmpty(currentUserId) || string.IsNullOrEmpty(friendId))
            {
                TempData["ErrorMessage"] = "Неверные параметры запроса";
                return RedirectToAction("Search");
            }
            
            var result = await _userRepository.AcceptFriendRequestAsync(currentUserId, friendId);

            TempData[result ? "SuccessMessage" : "ErrorMessage"] = 
                result ? "Запрос на дружбу принят" : "Не удалось принять запрос на дружбу";

            return RedirectToAction("Search", new { searchTerm = Request.Query["searchTerm"] });
        }
    }
}