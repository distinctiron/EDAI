using AutoMapper;
using EDAI.Server.Data;
using EDAI.Server.Hubs;
using EDAI.Services.Interfaces;
using EDAI.Shared.Factories;
using EDAI.Shared.Models.DTO.OpenAI;
using EDAI.Shared.Models.Entities;
using EDAI.Shared.Models.Enums;
using EDAI.Shared.Tools;
using Microsoft.AspNetCore.SignalR;

namespace EDAI.Server.Jobs;

public class GenerateStudentSummaryService(IOpenAiService openAiService, IHubContext<MessageHub> messageHub, IMapper mapper, IServiceProvider serviceProvider) : IGenerateStudentSummaryService
{
    public async Task GenerateStudentSummaryScore(int studentId, string connectionString)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<EdaiContext>();
            
            var scores = context.Scores.Where(s => s.Essay.StudentId == studentId);
            openAiService.InitiateStudentSummaryConversation();
            var studentSummary = await openAiService.GenerateStudentSummary(scores);

            var entity = mapper.Map<StudentSummary>(studentSummary);
            entity.StudentId = studentId;

            context.StudentSummaries.Add(entity);

            context.SaveChanges();
        
            //await messageHub.Clients.Client(connectionString).SendAsync("SummaryGenerated", entity.StudentSummaryId.ToString());
        
            await messageHub.Clients.All.SendAsync("SummaryGenerated", entity.StudentSummaryId.ToString());
        }
        
    }
}