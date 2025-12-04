using System.ComponentModel.DataAnnotations;

namespace SocialNetworkWeb.ViewModels
{
    public class UserProfileViewModel
    {
        public string Id { get; set; }

        // Временно убираем [Required] для тестирования
        [Display(Name = "Имя")]
        public string FirstName { get; set; }

        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "О себе")]
        [StringLength(500)]
        public string Bio { get; set; }

        [Display(Name = "Телефон")]
        public string PhoneNumber { get; set; }

        public string ProfilePicture { get; set; }

        [Display(Name = "Имя пользователя")]
        public string UserName { get; set; }
    }
}