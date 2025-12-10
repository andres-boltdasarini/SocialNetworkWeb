// IUserRepository.cs
using System.Security.Claims;  // ← ДОБАВЬТЕ ЭТО!
using SocialNetworkWeb.Models;
using SocialNetworkWeb.ViewModels;

namespace SocialNetworkWeb.Services
{
    public interface IUserRepository
    {
        // Существующие методы
        Task<IEnumerable<ApplicationUser>> SearchUsersAsync(string searchTerm, string currentUserId);
        Task<bool> SendFriendRequestAsync(string fromUserId, string toUserId);
        Task<bool> RemoveFriendAsync(string currentUserId, string friendId);
        Task<bool> AcceptFriendRequestAsync(string currentUserId, string friendId);
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
        
        // Для поиска с информацией о дружбе
        Task<List<UserViewModel>> SearchUsersWithFriendshipInfoAsync(
            string searchTerm, 
            string currentUserId);
        
        // Новые методы для замены DbContext в контроллере
        
        // Получить текущего пользователя (аналог _userManager.GetUserAsync)
        Task<ApplicationUser?> GetCurrentUserAsync(ClaimsPrincipal user);  // ← Теперь будет работать
        
        // Сохранить пользователя с поддержкой изменения email
        Task<bool> SaveUserWithEmailUpdateAsync(ApplicationUser user, string newEmail);
        
        // Обновить аутентификацию
        Task RefreshSignInAsync(ApplicationUser user);
        
        // Получить пользователя по ID (из UserManager)
        Task<ApplicationUser?> FindByIdAsync(string userId);
    }
}