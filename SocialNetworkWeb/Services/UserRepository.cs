// UserRepository.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;  // ← ДОБАВЬТЕ ЭТО!
using SocialNetworkWeb.Data;
using SocialNetworkWeb.Models;
using SocialNetworkWeb.ViewModels;

namespace SocialNetworkWeb.Services
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public UserRepository(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // === НОВЫЕ МЕТОДЫ ===
        
        public async Task<ApplicationUser?> GetCurrentUserAsync(ClaimsPrincipal user)
        {
            return await _userManager.GetUserAsync(user);
        }
        
        public async Task<ApplicationUser?> FindByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }
        
        public async Task RefreshSignInAsync(ApplicationUser user)
        {
            await _signInManager.RefreshSignInAsync(user);
        }
        
        public async Task<bool> SaveUserWithEmailUpdateAsync(ApplicationUser user, string newEmail)
        {
            try
            {
                Console.WriteLine($"SaveUserWithEmailUpdateAsync: UserId={user.Id}, NewEmail={newEmail}");
                
                // 1. Сохраняем основные поля
                var updateResult = await _userManager.UpdateAsync(user);
                Console.WriteLine($"UpdateAsync result: {updateResult.Succeeded}");
                
                if (!updateResult.Succeeded)
                {
                    Console.WriteLine($"UpdateAsync errors: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
                    return false;
                }
                
                // 2. Проверяем, нужно ли обновлять email
                var emailChanged = !string.IsNullOrEmpty(newEmail) && 
                                  !string.Equals(user.Email, newEmail, StringComparison.OrdinalIgnoreCase);
                
                if (emailChanged)
                {
                    Console.WriteLine($"Email changed from '{user.Email}' to '{newEmail}'");
                    
                    // Обновляем Email
                    var emailResult = await _userManager.SetEmailAsync(user, newEmail);
                    Console.WriteLine($"SetEmailAsync result: {emailResult.Succeeded}");
                    
                    if (!emailResult.Succeeded)
                    {
                        Console.WriteLine($"SetEmailAsync errors: {string.Join(", ", emailResult.Errors.Select(e => e.Description))}");
                        return false;
                    }
                    
                    // Обновляем UserName
                    var userNameResult = await _userManager.SetUserNameAsync(user, newEmail);
                    Console.WriteLine($"SetUserNameAsync result: {userNameResult.Succeeded}");
                    
                    if (!userNameResult.Succeeded)
                    {
                        Console.WriteLine($"SetUserNameAsync errors (non-critical): {string.Join(", ", userNameResult.Errors.Select(e => e.Description))}");
                    }
                    
                    // Обновляем Security Stamp
                    await _userManager.UpdateSecurityStampAsync(user);
                    
                    // Явное сохранение через DbContext
                    _context.Entry(user).State = EntityState.Modified;
                    var rowsAffected = await _context.SaveChangesAsync();
                    Console.WriteLine($"DbContext saved: {rowsAffected} rows affected");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SaveUserWithEmailUpdateAsync: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        // === ОПЕРАЦИИ С ПОЛЬЗОВАТЕЛЯМИ ===
        
        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<bool> UpdateUserAsync(ApplicationUser user)
        {
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> UpdateEmailAsync(string userId, string newEmail)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null) return false;
            
            var result = await _userManager.SetEmailAsync(user, newEmail);
            return result.Succeeded;
        }

        public async Task<bool> UpdateUserNameAsync(string userId, string newUserName)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null) return false;
            
            var result = await _userManager.SetUserNameAsync(user, newUserName);
            return result.Succeeded;
        }

        public async Task UpdateSecurityStampAsync(string userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user != null)
            {
                await _userManager.UpdateSecurityStampAsync(user);
            }
        }

        // === ПОИСК ПОЛЬЗОВАТЕЛЕЙ ===
        
        public async Task<IEnumerable<ApplicationUser>> SearchUsersAsync(
            string searchTerm, 
            string currentUserId)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<ApplicationUser>();

            return await _context.Users
                .Where(u => u.Id != currentUserId &&
                           ((u.FirstName != null && u.FirstName.Contains(searchTerm)) ||
                            (u.LastName != null && u.LastName.Contains(searchTerm)) ||
                            (u.Email != null && u.Email.Contains(searchTerm))))
                .Take(50)
                .ToListAsync();
        }

        public async Task<List<UserViewModel>> SearchUsersWithFriendshipInfoAsync(
            string searchTerm, 
            string currentUserId)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<UserViewModel>();

            // Получаем пользователей
            var users = await SearchUsersAsync(searchTerm, currentUserId);
            
            // Получаем информацию о друзьях текущего пользователя
            var friendships = await _context.Friendships
                .Where(f => f.UserId == currentUserId || f.FriendId == currentUserId)
                .ToListAsync();
            
            // Преобразуем ApplicationUser в UserViewModel с информацией о дружбе
            var userViewModels = users.Select(u => new UserViewModel
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                UserName = u.UserName,
                
                // Определяем статус дружбы
                IsFriend = friendships.Any(f => 
                    (f.UserId == currentUserId && f.FriendId == u.Id && f.Status == FriendshipStatus.Accepted) ||
                    (f.UserId == u.Id && f.FriendId == currentUserId && f.Status == FriendshipStatus.Accepted)),
                    
                FriendRequestSent = friendships.Any(f => 
                    f.UserId == currentUserId && f.FriendId == u.Id && f.Status == FriendshipStatus.Pending),
                    
                HasPendingRequestFromMe = friendships.Any(f => 
                    f.UserId == currentUserId && f.FriendId == u.Id && f.Status == FriendshipStatus.Pending),
                    
                HasPendingRequestToMe = friendships.Any(f => 
                    f.UserId == u.Id && f.FriendId == currentUserId && f.Status == FriendshipStatus.Pending),
                    
                FriendshipStatus = friendships
                    .FirstOrDefault(f => 
                        (f.UserId == currentUserId && f.FriendId == u.Id) ||
                        (f.UserId == u.Id && f.FriendId == currentUserId))
                    ?.Status
            }).ToList();

            return userViewModels;
        }

        // === МЕТОДЫ ДЛЯ ДРУЗЕЙ ===
        
        public async Task<bool> SendFriendRequestAsync(string fromUserId, string toUserId)
        {
            try
            {
                var existingRequest = await _context.Friendships
                    .FirstOrDefaultAsync(f => 
                        (f.UserId == fromUserId && f.FriendId == toUserId) ||
                        (f.UserId == toUserId && f.FriendId == fromUserId));

                if (existingRequest != null) return false;

                var friendship = new Friendship
                {
                    UserId = fromUserId,
                    FriendId = toUserId,
                    Status = FriendshipStatus.Pending,
                    FriendsSince = DateTime.UtcNow  // Используем FriendsSince вместо CreatedAt
                };

                _context.Friendships.Add(friendship);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendFriendRequestAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemoveFriendAsync(string currentUserId, string friendId)
        {
            try
            {
                var friendship = await _context.Friendships
                    .FirstOrDefaultAsync(f =>
                        (f.UserId == currentUserId && f.FriendId == friendId) ||
                        (f.UserId == friendId && f.FriendId == currentUserId));

                if (friendship == null) return false;

                _context.Friendships.Remove(friendship);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RemoveFriendAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AcceptFriendRequestAsync(string currentUserId, string friendId)
        {
            try
            {
                var friendship = await _context.Friendships
                    .FirstOrDefaultAsync(f =>
                        f.UserId == friendId && 
                        f.FriendId == currentUserId && 
                        f.Status == FriendshipStatus.Pending);

                if (friendship == null) return false;

                friendship.Status = FriendshipStatus.Accepted;
                friendship.FriendsSince = DateTime.UtcNow;  // Обновляем FriendsSince
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AcceptFriendRequestAsync: {ex.Message}");
                return false;
            }
        }
    }
}