using Microsoft.EntityFrameworkCore;
using AutoMapper;
using SocialNetworkWeb.Data;
using SocialNetworkWeb.Models;
using SocialNetworkWeb.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<List<UserViewModel>> SearchUsersAsync(string searchTerm, string currentUserId)
        {
            IQueryable<ApplicationUser> query = _context.Users;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(u =>
                    u.Id != currentUserId &&
                    (u.FirstName.ToLower().Contains(searchTerm) ||
                     u.LastName.ToLower().Contains(searchTerm) ||
                     u.UserName.ToLower().Contains(searchTerm) ||
                     u.Email.ToLower().Contains(searchTerm)));
            }
            else
            {
                query = query.Where(u => u.Id != currentUserId);
            }

            var users = await query.ToListAsync();
            var result = new List<UserViewModel>();

            foreach (var user in users)
            {
                // Получаем все возможные статусы
                var friendship = await GetFriendshipStatusAsync(currentUserId, user.Id);

                var userViewModel = new UserViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    IsFriend = friendship?.Status == FriendshipStatus.Accepted,
                    FriendRequestSent = friendship?.Status == FriendshipStatus.Pending &&
                                       friendship.UserId == currentUserId,
                    // Новые свойства для лучшего отображения
                    HasPendingRequestFromMe = friendship?.Status == FriendshipStatus.Pending &&
                                             friendship.UserId == currentUserId,
                    HasPendingRequestToMe = friendship?.Status == FriendshipStatus.Pending &&
                                           friendship.FriendId == currentUserId,
                    FriendshipStatus = friendship?.Status
                };
                result.Add(userViewModel);
            }

            return result;
        }

        // Новый метод для получения статуса дружбы
        private async Task<Friendship?> GetFriendshipStatusAsync(string userId1, string userId2)
        {
            return await _context.Friendships
                .FirstOrDefaultAsync(f =>
                    (f.UserId == userId1 && f.FriendId == userId2) ||
                    (f.UserId == userId2 && f.FriendId == userId1));
        }

        public async Task<bool> SendFriendRequestAsync(string fromUserId, string toUserId)
        {
            if (fromUserId == toUserId) return false;

            // Ищем существующую дружбу в ЛЮБОМ направлении
            var existingFriendship = await _context.Friendships
                .FirstOrDefaultAsync(f =>
                    (f.UserId == fromUserId && f.FriendId == toUserId) ||
                    (f.UserId == toUserId && f.FriendId == fromUserId));

            if (existingFriendship != null)
            {
                // Анализируем существующую дружбу
                switch (existingFriendship.Status)
                {
                    case FriendshipStatus.Pending:
                        // Если запрос уже отправлен
                        if (existingFriendship.UserId == fromUserId &&
                            existingFriendship.FriendId == toUserId)
                        {
                            return true; // Запрос уже отправлен
                        }
                        else if (existingFriendship.UserId == toUserId &&
                                 existingFriendship.FriendId == fromUserId)
                        {
                            // Пользователь 2 хочет отправить запрос пользователю 1,
                            // но пользователь 1 уже отправил запрос пользователю 2
                            // В этом случае автоматически принимаем запрос
                            existingFriendship.Status = FriendshipStatus.Accepted;
                            _context.Friendships.Update(existingFriendship);
                            return await _context.SaveChangesAsync() > 0;
                        }
                        break;

                    case FriendshipStatus.Accepted:
                        // Уже друзья
                        return true;

                    case FriendshipStatus.Rejected:
                        // Запрос был отклонен, обновляем статус
                        // Меняем направление если нужно
                        if (existingFriendship.UserId == toUserId &&
                            existingFriendship.FriendId == fromUserId)
                        {
                            // Меняем местами User и Friend
                            var temp = existingFriendship.UserId;
                            existingFriendship.UserId = existingFriendship.FriendId;
                            existingFriendship.FriendId = temp;
                        }
                        existingFriendship.Status = FriendshipStatus.Pending;
                        _context.Friendships.Update(existingFriendship);
                        return await _context.SaveChangesAsync() > 0;

                    case FriendshipStatus.Blocked:
                        // Заблокировано, нельзя отправить запрос
                        return false;
                }

                return false;
            }

            // Создаем новый запрос на дружбу
            var friendship = new Friendship
            {
                UserId = fromUserId,
                FriendId = toUserId,
                Status = FriendshipStatus.Pending,
                FriendsSince = DateTime.UtcNow
            };

            _context.Friendships.Add(friendship);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> AcceptFriendRequestAsync(string userId, string friendId)
        {
            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f => f.UserId == friendId &&
                                         f.FriendId == userId &&
                                         f.Status == FriendshipStatus.Pending);

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
                .Where(f => (f.UserId == userId || f.FriendId == userId) &&
                           f.Status == FriendshipStatus.Accepted)
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
                .AnyAsync(f => f.UserId == fromUserId &&
                              f.FriendId == toUserId &&
                              f.Status == FriendshipStatus.Pending);
        }
    }
}