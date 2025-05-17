using AutoMapper;
using SGS.Application.DTOs;
using SGS.Domain.Entities;
using SGS.Domain.Enums;
using Document = SGS.Domain.Entities.Document;

namespace SGS.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Document, DocumentDto>();
        CreateMap<Assignment, AssignmentDto>();
        CreateMap<Course, CourseDto>();
        CreateMap<AnalysisResults, AnalysisResultDto>();
        CreateMap<GradingCriteria, GradingCriteriaDto>();

        CreateMap<UploadDocumentCommand, Document>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FilePath, opt => opt.Ignore())
            .ForMember(dest => dest.UploadedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));
        
        CreateMap<UserDto, User>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => Enum.Parse<UserRole>(src.Role)));

        CreateMap<CreateAssignmentCommand, Assignment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<CreateCourseCommand, Course>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<CreateGradingCriteriaCommand, GradingCriteria>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}