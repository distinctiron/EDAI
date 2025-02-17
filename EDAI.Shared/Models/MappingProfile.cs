using AutoMapper;
using EDAI.Shared.Models.DTO;
using EDAI.Shared.Models.Entities;

namespace EDAI.Shared.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Essay, EssayFileDTO>();
        CreateMap<EssayFileDTO,Essay>();
        CreateMap<Assignment, AssignmentDTO>();
        CreateMap<AssignmentDTO, Assignment>();
        CreateMap<Student, StudentDTO>();
        CreateMap<StudentDTO, Student>();
        CreateMap<IndexedContent, IndexedContent>();
        
    }
}