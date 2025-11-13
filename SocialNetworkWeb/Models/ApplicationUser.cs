using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SocialNetworkWeb.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [StringLength(500)]  // Убираем [Required]
        public string Bio { get; set; } = string.Empty;  // Добавляем значение по умолчанию

        // public string ProfilePicture { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Friendship> Friends { get; set; }
        public virtual ICollection<Friendship> FriendOf { get; set; }
    }

    public class Friendship
    {
        public int Id { get; set; }
        
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        
        public string FriendId { get; set; }
        public virtual ApplicationUser Friend { get; set; }
        
        public DateTime FriendsSince { get; set; } = DateTime.UtcNow;
        public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;
    }

    public enum FriendshipStatus
    {
        Pending,
        Accepted,
        Rejected,
        Blocked
    }
}