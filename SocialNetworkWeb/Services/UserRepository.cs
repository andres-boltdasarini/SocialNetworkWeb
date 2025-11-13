using Microsoft.EntityFrameworkCore;
using AutoMapper;
using SocialNetworkWeb.Data;
using SocialNetworkWeb.Models;
using SocialNetworkWeb.ViewModels;

namespace SocialNetworkWeb.Services
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public UserRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            return await _context.Users
                .Include(u => u.Friends)
                .ThenInclude(f => f.Friend)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<UserProfileViewModel> GetUserProfileAsync(string userId)
        {
            var user = await GetUserByIdAsync(userId);
            return _mapper.Map<UserProfileViewModel>(user);
        }

        public async Task<bool> UpdateUserProfileAsync(string userId, UserProfileViewModel model)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null) return false;

            _mapper.Map(model, user);
            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<UserSearchViewModel>> SearchUsersAsync(string searchTerm, string currentUserId)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<UserSearchViewModel>();

            var users = await _context.Users
                .Where(u => u.Id != currentUserId &&
                           (u.FirstName.Contains(searchTerm) ||
                            u.LastName.Contains(searchTerm) ||
                            u.UserName.Contains(searchTerm) ||
                            u.Email.Contains(searchTerm)))
                .ToListAsync();

            var result = _mapper.Map<List<UserSearchViewModel>>(users);

            foreach (var user in result)
            {
                user.IsFriend = await IsFriendAsync(currentUserId, user.Id);
                user.FriendRequestSent = await HasPendingRequestAsync(currentUserId, user.Id);
            }

            return result;
        }

        public async Task<bool> SendFriendRequestAsync(string fromUserId, string toUserId)
        {
            if (fromUserId == toUserId) return false;

            var existingFriendship = await _context.Friendships
                .FirstOrDefaultAsync(f => 
                    (f.UserId == fromUserId && f.FriendId == toUserId) ||
                    (f.UserId == toUserId && f.FriendId == fromUserId));

            if (existingFriendship != null) return false;

            var friendship = new Friendship
            {
                UserId = fromUserId,
                FriendId = toUserId,
                Status = FriendshipStatus.Pending
            };

            _context.Friendships.Add(friendship);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> AcceptFriendRequestAsync(string userId, string friendId)
        {
            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f => f.UserId == friendId && f.FriendId == userId && f.Status == FriendshipStatus.Pending);

            if (friendship == null) return false;

            friendship.Status = FriendshipStatus.Accepted;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveFriendAsync(string userId, string friendId)
        {
            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f => 
                    (f.UserId == userId && f.FriendId == friendId) ||
                    (f.UserId == friendId && f.FriendId == userId));

            if (friendship == null) return false;

            _context.Friendships.Remove(friendship);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<ApplicationUser>> GetUserFriendsAsync(string userId)
        {
            var friendships = await _context.Friendships
                .Where(f => (f.UserId == userId || f.FriendId == userId) && f.Status == FriendshipStatus.Accepted)
                .Include(f => f.User)
                .Include(f => f.Friend)
                .ToListAsync();

            return friendships.Select(f => f.UserId == userId ? f.Friend : f.User).ToList();
        }

        public async Task<bool> IsFriendAsync(string userId, string friendId)
        {
            return await _context.Friendships
                .AnyAsync(f => 
                    ((f.UserId == userId && f.FriendId == friendId) ||
                     (f.UserId == friendId && f.FriendId == userId)) &&
                    f.Status == FriendshipStatus.Accepted);
        }

        public async Task<bool> HasPendingRequestAsync(string fromUserId, string toUserId)
        {
            return await _context.Friendships
                .AnyAsync(f => f.UserId == fromUserId && f.FriendId == toUserId && f.Status == FriendshipStatus.Pending);
        }
    }
}