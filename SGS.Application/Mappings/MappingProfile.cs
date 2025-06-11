using AutoMapper;
using SGS.Application.DTOs;
using SGS.Domain.Entities;

namespace SGS.Application.Mappings;

// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        ConfigureUserMappings();
        ConfigureRoleMappings();
        ConfigureCourseMappings();
        ConfigureStudentsGroupMappings();
        ConfigureAssignmentMappings();
        ConfigureGradingCriteriaMappings();
        ConfigureGradingMappings();
        ConfigureDocumentMappings();
        ConfigureAnalysisMappings();
    }

    private void ConfigureUserMappings()
    {
        CreateMap<User, UserDto>();
        
        CreateMap<UserDto, User>();
        CreateMap<UpdateUserDto, User>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<Student, UserDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => 
                src.User != null ? src.User.FullName : string.Empty))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => 
                src.User != null ? src.User.Email : string.Empty));
        CreateMap<UserDto, Student>();
    }

    private void ConfigureRoleMappings()
    {
        CreateMap<Teacher, TeacherDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => 
                src.User != null ? src.User.FullName : string.Empty))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => 
                src.User != null ? src.User.Email : string.Empty))
            .ForMember(dest => dest.Courses, opt => opt.MapFrom(src => 
                src.Courses.Select(c => new CourseBasicDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description
                })));
    }

    private void ConfigureCourseMappings()
    {
        CreateMap<Course, CourseBasicDto>();
        CreateMap<CreateCourseCommand, Course>();
        
        CreateMap<UpdateCourseDto, Course>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<CourseBasicDto, Course>()
            .ForMember(dest => dest.Teachers, opt => opt.Ignore())
            .ForMember(dest => dest.StudentsGroups, opt => opt.Ignore())
            .ForMember(dest => dest.Assignments, opt => opt.Ignore());
       
        CreateMap<Course, CourseDto>()
            .ForMember(dest => dest.Teachers, opt => opt.MapFrom(src => 
                src.Teachers.Where(t => t.User != null).Select(t => new TeacherDto
                {
                    Id = t.Id,
                    FullName = t.User.FullName,
                    Email = t.User.Email
                })))
            .ForMember(dest => dest.StudentsGroups, opt => opt.MapFrom(src => 
                src.StudentsGroups.Select(g => new StudentsGroupBasicDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description
                })));
    }

    private void ConfigureStudentsGroupMappings()
    {
        CreateMap<StudentsGroup, StudentsGroupBasicDto>();
        
        CreateMap<UpdateStudentsGroupDto, StudentsGroup>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<GroupDto, StudentsGroup>()
            .ForMember(dest => dest.Students, opt => opt.Ignore())
            .ForMember(dest => dest.Courses, opt => opt.Ignore());
        CreateMap<CreateStudentsGroupDto, StudentsGroup>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Students, opt => opt.Ignore())
            .ForMember(dest => dest.Courses, opt => opt.Ignore());

        CreateMap<StudentsGroup, GroupDto>()
            .ForMember(dest => dest.Students, opt => opt.MapFrom(src => 
                src.Students.Where(s => s.User != null).Select(s => new UserDto
                {
                    Id = s.Id,
                    FullName = s.User.FullName,
                    Email = s.User.Email
                })))
            .ForMember(dest => dest.Courses, opt => opt.MapFrom(src => 
                src.Courses.Select(c => new CourseBasicDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description
                })));
    }

    private void ConfigureAssignmentMappings()
    {
        CreateMap<Assignment, AssignmentDto>();
        CreateMap<CreateAssignmentCommand, Assignment>();
        CreateMap<UpdateAssignmentDto, Assignment>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }

    private void ConfigureGradingCriteriaMappings()
    {
        CreateMap<GradingCriteria, GradingCriteriaDto>();
        CreateMap<CreateGradingCriteriaCommand, GradingCriteria>();
        CreateMap<CreateGradingCriteriaBatchCommand, List<GradingCriteria>>()
            .ConvertUsing((src, dest, ctx) =>
                ctx.Mapper.Map<List<GradingCriteria>>(src.Criteria)
            );
    }

    private void ConfigureGradingMappings()
    {
        CreateMap<GradeDto, Grade>();
        CreateMap<Grade, GradeDto>();
    }

    private void ConfigureDocumentMappings()
    {
        CreateMap<Document, DocumentDto>();
        CreateMap<UploadDocumentCommand, Document>()
            .ForMember(dest => dest.FilePath, opt => opt.Ignore())
            .ForMember(dest => dest.UploadedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
    }

    private void ConfigureAnalysisMappings()
    {
        CreateMap<AnalysisResults, AnalysisResultDto>()
            .ForMember(dest => dest.Grade, opt => opt.MapFrom(src => src.Grade));
    }
}