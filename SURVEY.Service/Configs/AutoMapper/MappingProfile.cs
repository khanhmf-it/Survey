using AutoMapper;
using SURVEY.Model.DTOs;
using SURVEY.Model.Models_SURVEY;

namespace SURVEY.Service.Configs.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            CreateMap<employee_evaluation, employee_evaluationDTO>().ReverseMap();
            CreateMap<employee_evaluationDTO, employee_evaluation>().ReverseMap();

            CreateMap<authentication, authenticationDTO>().ReverseMap();
            CreateMap<authenticationDTO, authentication>().ReverseMap();
        }
    }
}
