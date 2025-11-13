namespace SocialNetworkWeb.ViewModels
{
    public class SearchViewModel
    {
        public string SearchTerm { get; set; }
        public List<UserSearchViewModel> Users { get; set; } = new List<UserSearchViewModel>();
    }

    public class UserSearchViewModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string ProfilePicture { get; set; }
        public bool IsFriend { get; set; }
        public bool FriendRequestSent { get; set; }
    }
}