// Services/IUserRepository.cs
using SocialNetworkWeb.Models;
using SocialNetworkWeb.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialNetworkWeb.Services
{
    public interface IUserRepository
    {
        Task<ApplicationUser> GetUserByIdAsync(string userId);
        Task<UserProfileViewModel> GetUserProfileAsync(string userId);
        Task<bool> UpdateUserProfileAsync(string userId, UserProfileViewModel model);
        Task<List<UserViewModel>> SearchUsersAsync(string searchTerm, string currentUserId); // Изменено на UserViewModel
        Task<bool> SendFriendRequestAsync(string fromUserId, string toUserId);
        Task<bool> AcceptFriendRequestAsync(string userId, string friendId);
        Task<bool> RemoveFriendAsync(string userId, string friendId);
        Task<List<ApplicationUser>> GetUserFriendsAsync(string userId);
        Task<bool> IsFriendAsync(string userId, string friendId);
        Task<bool> HasPendingRequestAsync(string fromUserId, string toUserId);
    }
}