using SocialNetworkWeb.Models;
using SocialNetworkWeb.ViewModels;

namespace SocialNetworkWeb.Services
{
    public interface IUserRepository
    {
        Task<ApplicationUser> GetUserByIdAsync(string userId);
        Task<UserProfileViewModel> GetUserProfileAsync(string userId);
        Task<bool> UpdateUserProfileAsync(string userId, UserProfileViewModel model);
        Task<List<UserSearchViewModel>> SearchUsersAsync(string searchTerm, string currentUserId);
        Task<bool> SendFriendRequestAsync(string fromUserId, string toUserId);
        Task<bool> AcceptFriendRequestAsync(string userId, string friendId);
        Task<bool> RemoveFriendAsync(string userId, string friendId);
        Task<List<ApplicationUser>> GetUserFriendsAsync(string userId);
        Task<bool> IsFriendAsync(string userId, string friendId);
        Task<bool> HasPendingRequestAsync(string fromUserId, string toUserId);
    }
}