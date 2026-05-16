using AutoMapper;
using TaskManagement.DTOs;
using TaskManagement.Models;

namespace TaskManagement.Automapper
{
    public class TaskMappingProfile : Profile
    {
        public TaskMappingProfile()
        {
            CreateMap<TaskCreateDTO, Models.Task>();

            CreateMap<Models.Task, TaskUpdateDTO>()
                    .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category!.Categoryname))
                    .ForMember(dest => dest.AssignedtoName, opt => opt.MapFrom(src => src.AssignedtoNavigation!.Username))
                    .ForMember(dest => dest.CheckerName, opt => opt.MapFrom(src => src.TaskcheckerNavigation!.Username))
                    .ForMember(dest => dest.GithubPrUrl, opt => opt.MapFrom(src => src.githubprurl));

            CreateMap<TaskUpdateDTO, Models.Task>()
                .ForMember(dest => dest.Taskid, opt => opt.Ignore())
                .ForMember(dest => dest.Datetimetaskcreated, opt => opt.Ignore())
                .ForMember(dest => dest.attachmenturl, opt => opt.Ignore())
                .ForMember(dest => dest.githubprurl, opt => opt.MapFrom(src => src.GithubPrUrl));

            CreateMap<Models.Task, TaskDeleteDTO>()
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status!.Statusname))
                .ForMember(dest => dest.Datetaskcreated, opt => opt.MapFrom(src => src.Datetimetaskcreated));

            CreateMap<User, AuthenticatedUserDTO>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.Rolename : "User"));
        }
    }
}