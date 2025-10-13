using AutoMapper;
using EDAI.Shared.Models.DTO;
using EDAI.Shared.Models.DTO.OpenAI;
using EDAI.Shared.Models.Entities;
using OpenAI.Chat;
using ScoreDTO = EDAI.Shared.Models.Entities.ScoreDTO;

namespace EDAI.Shared.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Essay, EssayFileDTO>();
        CreateMap<EssayFileDTO,Essay>()
            .ForMember(e => e.EssayId, 
                o => o.Ignore());
        CreateMap<Essay, EssayDTO>();
        CreateMap<EssayDTO, Essay>();
        CreateMap<Assignment, AssignmentDTO>();
        CreateMap<AssignmentDTO, Assignment>();
        CreateMap<Student, StudentDTO>();
        CreateMap<StudentDTO, Student>();
        CreateMap<BaseComment, CommentDTO>();
        CreateMap<CommentDTO, BaseComment>();
        CreateMap<StudentSummaryDTO, StudentSummary>();
        CreateMap<StudentSummary, StudentSummaryDTO>();
        CreateMap<Score, ScoreDTO>();
        CreateMap<ScoreDTO, Score>();
        CreateMap<Student, StudentDTO>();
        CreateMap<StudentDTO, Student>();
        CreateMap<StudentClass, StudentClassDTO>();
        CreateMap<StudentClassDTO, StudentClass>();
        CreateMap<Organisation, OrganisationDto>();
        CreateMap<OrganisationDto, Organisation>();
        CreateMap<FeedbackComment, FeedbackCommentDTO>();
        CreateMap<FeedbackCommentDTO, FeedbackComment>();
    }
}