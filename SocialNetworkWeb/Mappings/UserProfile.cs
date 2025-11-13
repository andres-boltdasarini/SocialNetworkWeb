using AutoMapper;
using SocialNetworkWeb.Models;
using SocialNetworkWeb.ViewModels;

namespace SocialNetworkWeb.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<ApplicationUser, UserProfileViewModel>()
                .ReverseMap();

            CreateMap<ApplicationUser, UserSearchViewModel>()
                .ForMember(dest => dest.IsFriend, opt => opt.Ignore())
                .ForMember(dest => dest.FriendRequestSent, opt => opt.Ignore());
        }
    }
}