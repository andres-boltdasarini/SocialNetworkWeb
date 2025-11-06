using AutoMapper;
using SocialNetworkWeb.Models.Users;

namespace SocialNetworkWeb
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<RegisterViewModel, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Login))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.EmailReg))
                .ForMember(dest => dest.BirthDate, 
                    opt => opt.MapFrom(src => new DateTime(src.Year, src.Month, src.Date, 0, 0, 0, DateTimeKind.Utc)))
                .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => string.Empty)); // Явно устанавливаем пустую строку
        }
    }
}