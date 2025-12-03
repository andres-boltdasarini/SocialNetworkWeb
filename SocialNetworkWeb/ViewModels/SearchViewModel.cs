// ViewModels/SearchViewModel.cs
using System.Collections.Generic;
using SocialNetworkWeb.Models;

namespace SocialNetworkWeb.ViewModels
{
    public class SearchViewModel
    {
        public string SearchTerm { get; set; }
        public List<UserViewModel> Users { get; set; } = new List<UserViewModel>();
    }

    public class UserViewModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public bool IsFriend { get; set; }
        public bool FriendRequestSent { get; set; }

        // Новые свойства
        public bool HasPendingRequestFromMe { get; set; }
        public bool HasPendingRequestToMe { get; set; }
        public FriendshipStatus? FriendshipStatus { get; set; }
    }
}