using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkWeb.Models;
using SocialNetworkWeb.Models.Users;

namespace SocialNetworkWeb.Controllers
{
    public class RegisterController : Controller
    {
        private IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public RegisterController(IMapper mapper, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: Показываем первую страницу регистрации
        [HttpGet]
        public IActionResult Register()
        {
            return RedirectToAction("Index", "Home", new { showRegister = true });
        }

        // POST: Обрабатываем первую страницу и переходим ко второй
        [HttpPost]
        public IActionResult RegisterStep1(RegisterViewModel model)
        {
            if (!string.IsNullOrEmpty(model.FirstName) &&
                !string.IsNullOrEmpty(model.LastName) &&
                model.Year > 1900 && model.Year < 2024 &&
                model.Month >= 1 && model.Month <= 12 &&
                model.Date >= 1 && model.Date <= 31)
            {
                // Сохраняем данные в TempData
                TempData["RegFirstName"] = model.FirstName;
                TempData["RegLastName"] = model.LastName;
                TempData["RegYear"] = model.Year;
                TempData["RegMonth"] = model.Month;
                TempData["RegDate"] = model.Date;
                
                // Перенаправляем на главную с вторым шагом
                return RedirectToAction("Index", "Home", new { step = 2 });
            }
            else
            {
                // Остаемся на первом шаге с ошибками
                ModelState.AddModelError("", "Пожалуйста, заполните все поля корректно");
                var mainModel = new MainViewModel { RegisterView = model };
                return View("~/Views/Home/Index.cshtml", mainModel);
            }
        }

        // POST: Обрабатываем вторую страницу и завершаем регистрацию
[HttpPost]
public async Task<IActionResult> RegisterStep2(RegisterViewModel model)
{
    Console.WriteLine($"RegisterStep2 START: FirstName={model.FirstName}, Email={model.EmailReg}, Login={model.Login}");

    // Временно упрощаем валидацию
    if (string.IsNullOrEmpty(model.EmailReg))
    {
        ModelState.AddModelError("EmailReg", "Email обязателен");
    }
    
    if (string.IsNullOrEmpty(model.Login))
    {
        ModelState.AddModelError("Login", "Никнейм обязателен");
    }
    
    if (string.IsNullOrEmpty(model.PasswordReg))
    {
        ModelState.AddModelError("PasswordReg", "Пароль обязателен");
    }
    else if (model.PasswordReg != model.PasswordConfirm)
    {
        ModelState.AddModelError("PasswordConfirm", "Пароли не совпадают");
    }

    if (ModelState.IsValid)
    {
        try
        {
            Console.WriteLine("Attempting to create user...");
            
            var user = _mapper.Map<User>(model);
           
            var result = await _userManager.CreateAsync(user, model.PasswordReg);
            if (result.Succeeded)
            {
                Console.WriteLine("User created successfully");
                await _signInManager.SignInAsync(user, false);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                Console.WriteLine("User creation failed");
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    Console.WriteLine($"Error: {error.Description}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            ModelState.AddModelError("", "Произошла ошибка при регистрации: " + ex.Message);
        }
    }
    else
    {
        Console.WriteLine("ModelState is invalid");
        foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
        {
            Console.WriteLine($"Validation error: {error.ErrorMessage}");
        }
    }

    // Если есть ошибки, возвращаем на второй шаг
    model.Step = 2;
    
    // Сохраняем данные для восстановления
    TempData["RegFirstName"] = model.FirstName;
    TempData["RegLastName"] = model.LastName;
    TempData["RegYear"] = model.Year;
    TempData["RegMonth"] = model.Month;
    TempData["RegDate"] = model.Date;
    TempData["RegEmail"] = model.EmailReg;
    TempData["RegLogin"] = model.Login;
    
    Console.WriteLine("Redirecting to step 2 with errors");
    return RedirectToAction("Index", "Home", new { step = 2 });
}
    }
}