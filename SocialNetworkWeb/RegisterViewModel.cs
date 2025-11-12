using System.ComponentModel.DataAnnotations;

public class RegisterViewModel
{
    // Шаг 1: Основная информация
    [Required(ErrorMessage = "Имя обязательно")]
    [Display(Name = "Имя")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Фамилия обязательна")]
    [Display(Name = "Фамилия")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Дата рождения обязательна")]
    [Display(Name = "Год")]
    [Range(1900, 2024, ErrorMessage = "Введите корректный год")]
    public int Year { get; set; }

    [Required(ErrorMessage = "Дата рождения обязательна")]
    [Display(Name = "День")]
    [Range(1, 31, ErrorMessage = "Введите корректный день")]
    public int Date { get; set; }

    [Required(ErrorMessage = "Дата рождения обязательна")]
    [Display(Name = "Месяц")]
    [Range(1, 12, ErrorMessage = "Введите корректный месяц")]
    public int Month { get; set; }

    // Шаг 2: Остальная информация
    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Некорректный email")]
    [Display(Name = "Email")]
    public string EmailReg { get; set; }

    [Required(ErrorMessage = "Никнейм обязателен")]
    [Display(Name = "Никнейм")]
    public string Login { get; set; }

    [Required(ErrorMessage = "Пароль обязателен")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    [StringLength(100, ErrorMessage = "Пароль должен быть от {2} до {1} символов", MinimumLength = 5)]
    public string PasswordReg { get; set; }

    [Required(ErrorMessage = "Подтверждение пароля обязательно")]
    [Compare("PasswordReg", ErrorMessage = "Пароли не совпадают")]
    [DataType(DataType.Password)]
    [Display(Name = "Подтвердить пароль")]
    public string PasswordConfirm { get; set; }

    // Для отслеживания шага
    public int Step { get; set; } = 1;
}