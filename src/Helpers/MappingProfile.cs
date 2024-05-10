using AutoMapper;
using src.DTOs;
using src.DTOs.ToDos;
using src.Entities;
using src.entity;
using Twilio.Http;

namespace src;

public class MappingProfile: Profile
{
    public MappingProfile()
    {
        CreateMap<User, RegisterUser>().ReverseMap();
        CreateMap<User, ReturnedUser>()
            .ForMember(dest => dest.ToDos, opt => opt.MapFrom(src => src.ToDos))
            .ReverseMap();
        
        CreateMap<ToDo, ReturnedToDo>().ReverseMap();

    }
}