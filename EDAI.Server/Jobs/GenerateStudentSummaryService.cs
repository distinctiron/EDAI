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

public class GenerateStudentSummaryService(EdaiContext context, IOpenAiService openAiService, IHubContext<MessageHub> messageHub) : IGenerateStudentSummaryService
{
    public async Task GenerateStudentSummaryScore(int studentId)
    {
        var scores = context.Scores.Where(s => s.Essay.StudentId == studentId);
        openAiService.InitiateStudentSummaryConversation();
        var studentSummary = await openAiService.GetStudentSummary(scores);
    }
}