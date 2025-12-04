using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNetworkWeb.Data;
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
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public UserController(
            IUserRepository userRepository,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> TestUpdate([FromBody] SimpleUpdateModel model)
        {
            Console.WriteLine("=== TEST UPDATE ===");

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Content("ERROR: User not found");
                }

                Console.WriteLine($"Old FirstName: {user.FirstName}");
                Console.WriteLine($"New FirstName: {model.FirstName}");

                // Обновляем только FirstName для теста
                user.FirstName = model.FirstName;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return Content($"SUCCESS: Updated FirstName to '{user.FirstName}' at {DateTime.Now:HH:mm:ss}");
                }
                else
                {
                    return Content($"ERROR: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return Content($"EXCEPTION: {ex.Message}");
            }
        }

        public class SimpleUpdateModel
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string Bio { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);

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
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                Console.WriteLine($"Before update: FirstName='{user.FirstName}', Email='{user.Email}'");
                Console.WriteLine($"From form: FirstName='{model.FirstName}', Email='{model.Email}'");

                // 1. Обновляем основные поля
                user.FirstName = model.FirstName?.Trim();
                user.LastName = model.LastName?.Trim();
                user.PhoneNumber = model.PhoneNumber?.Trim();
                user.Bio = model.Bio?.Trim() ?? string.Empty;

                // 2. Сохраняем основные поля
                var updateResult = await _userManager.UpdateAsync(user);
                Console.WriteLine($"UpdateAsync result: {updateResult.Succeeded}");

                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                        Console.WriteLine($"Error: {error.Description}");
                    }
                    return View(model);
                }

                // 3. Проверяем, изменился ли email
                if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Email changed from '{user.Email}' to '{model.Email}'");

                    // 3.1 Обновляем Email
                    var emailResult = await _userManager.SetEmailAsync(user, model.Email?.Trim());
                    Console.WriteLine($"SetEmailAsync result: {emailResult.Succeeded}");

                    if (!emailResult.Succeeded)
                    {
                        foreach (var error in emailResult.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                            Console.WriteLine($"Email error: {error.Description}");
                        }
                        return View(model);
                    }

                    // 3.2 Обновляем UserName
                    var userNameResult = await _userManager.SetUserNameAsync(user, model.Email?.Trim());
                    Console.WriteLine($"SetUserNameAsync result: {userNameResult.Succeeded}");

                    if (!userNameResult.Succeeded)
                    {
                        foreach (var error in userNameResult.Errors)
                        {
                            Console.WriteLine($"UserName error: {error.Description}");
                        }
                    }

                    // 3.3 Обновляем Security Stamp
                    await _userManager.UpdateSecurityStampAsync(user);

                    // 3.4 Явное сохранение через контекст
                    _context.Entry(user).State = EntityState.Modified;
                    var rowsAffected = await _context.SaveChangesAsync();
                    Console.WriteLine($"DbContext saved: {rowsAffected} rows affected");
                }

                // 4. Обновляем аутентификацию
                await _signInManager.RefreshSignInAsync(user);

                // 5. Получаем свежие данные из базы
                var freshUser = await _userManager.FindByIdAsync(user.Id);
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

        [HttpPost]
        public async Task<IActionResult> AcceptFriendRequest(string friendId)
        {
            var currentUserId = _userManager.GetUserId(User);

            Console.WriteLine($"=== AcceptFriendRequest Debug ===");
            Console.WriteLine($"Current User ID: {currentUserId}");
            Console.WriteLine($"Friend ID: {friendId}");

            try
            {
                var result = await _userRepository.AcceptFriendRequestAsync(currentUserId, friendId);

                if (result)
                {
                    Console.WriteLine($"Friend request accepted successfully");
                    TempData["SuccessMessage"] = "Запрос на дружбу принят";
                }
                else
                {
                    Console.WriteLine($"Failed to accept friend request");
                    TempData["ErrorMessage"] = "Не удалось принять запрос на дружбу";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                TempData["ErrorMessage"] = $"Ошибка: {ex.Message}";
            }

            return RedirectToAction("Search", new { searchTerm = Request.Query["searchTerm"] });
        }
    }
}