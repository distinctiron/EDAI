using System.ClientModel;
using System.Diagnostics;
using System.Text;
using Azure.AI.OpenAI;
using EDAI.Server.Prompts;
using EDAI.Services.Interfaces;
using EDAI.Shared.Models.Entities;
using EDAI.Shared.Models.Enums;
using OpenAI.Chat;
using System.Text.Json;
using AutoMapper;
using EDAI.Shared.Models.DTO.OpenAI;
using EDAI.Shared.Tools;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using ScoreDTO = EDAI.Shared.Models.DTO.OpenAI.ScoreDTO;

namespace EDAI.Services;

public class OpenAiService : IOpenAiService
{
    public OpenAiService(IMapper mapper)
    {
        _mapper = mapper;
    }

    private IMapper _mapper;
    
    private static string keyFromEnvironment =
        "14iJ5AHR1VzKxf8yHNmWWqEFGPKz41zIo06oG816TufVbeKNyDwKJQQJ99AKACfhMk5XJ3w3AAABACOGRcGt"; 
        
        //Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY");
    
    private static AzureOpenAIClient _openAiClient = new(
        new Uri("https://edai-llm.openai.azure.com"),
        new ApiKeyCredential(keyFromEnvironment)
        );

    private static IEnumerable<IndexedContent> _indexedContents;
    
    private static List<ChatMessage> messages = new List<ChatMessage>();

    private static ChatClient _client = _openAiClient.GetChatClient("gpt-4o-mini");

    public void SetIndexedContents(IEnumerable<IndexedContent> indexedContents, string assignmentDescription, string referencetext = null)
    {
        _indexedContents = indexedContents;
        messages.Add(new SystemChatMessage(TextEvaluatingPrompts.SystemRole.Prompt));
        messages.Add(new UserChatMessage(TextEvaluatingPrompts.ProvideAssignmentContextPrompt(assignmentDescription,referencetext)));
    }

    public void AddChatAssistantMessage(string answer)
    {
        messages.Add(new AssistantChatMessage(answer));
    }

    public async Task<IEnumerable<FeedbackComment>> GetCommentsAsync(CommentType commentType, string prompt)
    {
        var conversation = messages;
        conversation.Add(new UserChatMessage(prompt));

        var answerSchema = JsonHelper.getJsonSchema<CommentsDTO>();
        
        var responseSchema = JsonSerializer.Serialize(answerSchema, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        
        var options = new ChatCompletionOptions
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName:"essay_feedback",
                jsonSchema: BinaryData.FromBytes(Encoding.UTF8.GetBytes($$$"""
                                                                           {{{responseSchema}}}
                                                                           """).ToArray()
                ),jsonSchemaIsStrict:true),
            Temperature = 0.1f
        };

        
        
        try
        {
            var completion = await _client.CompleteChatAsync(conversation, options);
            AddChatAssistantMessage(completion.Value.Content[0].Text);
            
            var comments = JsonConvert.DeserializeObject<CommentsDTO>(completion.Value.Content[0].Text);

            var feedbackComments = new List<FeedbackComment>();
            
            foreach (var comment in comments.Comments)
            {
                var entityComment = _mapper.Map<BaseComment>(comment);
                feedbackComments.Add(new FeedbackComment(entityComment, commentType));
            }

            return feedbackComments;
        }
        catch (JsonReaderException e)
        {
            Console.WriteLine($"Error in reading Json: {e}");
            throw;
        }
        catch (JsonSerializationException e)
        {
            Console.WriteLine($"Error in serializing Json: {e}");
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw e;
        };
    }
    
    public async Task<string> GetFeedbackAsync(string prompt)
    {
        var conversation = messages;
        conversation.Add(new UserChatMessage(prompt));

        var answerSchema = JsonHelper.getJsonSchema<FeedbackDTO>();
        
        var responseSchema = JsonSerializer.Serialize(answerSchema, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        
        var options = new ChatCompletionOptions
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName:"essay_feedback",
                jsonSchema: BinaryData.FromBytes(Encoding.UTF8.GetBytes($$$"""
                                                                           {{{responseSchema}}}
                                                                           """).ToArray()
                ),jsonSchemaIsStrict:true),
            Temperature = 0.1f
        };

        var completion = await _client.CompleteChatAsync(conversation, options);
        AddChatAssistantMessage(completion.Value.Content[0].Text);
        
        try
        {
            var feedback = JsonConvert.DeserializeObject<FeedbackDTO>(completion.Value.Content[0].Text);

            return feedback.Feedback;

        }
        catch (JsonReaderException e)
        {
            Console.WriteLine($"Error in reading Json: {e}");
            throw;
        }
        catch (JsonSerializationException e)
        {
            Console.WriteLine($"Error in serializing Json: {e}");
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        };
    }

    public async Task<GenerateScoreDTO> AssessEssayAsync()
    {
        messages.Add(new UserChatMessage(TextEvaluatingPrompts.ProvideGrammarComments.Prompt));
        messages.Add(new UserChatMessage(TextEvaluatingPrompts.GiveGrammarRecommendation.Prompt));
        messages.Add(new UserChatMessage(TextEvaluatingPrompts.ScoreGrammar.Prompt));
        messages.Add(new UserChatMessage(TextEvaluatingPrompts.ProvideEloquenceComments.Prompt));
        messages.Add(new UserChatMessage(TextEvaluatingPrompts.GiveEloquenceRecommendation.Prompt));
        messages.Add(new UserChatMessage(TextEvaluatingPrompts.ScoreEloquence.Prompt));
        messages.Add(new UserChatMessage(TextEvaluatingPrompts.ProvideArgumentComments.Prompt));
        messages.Add(new UserChatMessage(TextEvaluatingPrompts.GiveArgumentationRecommendation.Prompt));
        messages.Add(new UserChatMessage(TextEvaluatingPrompts.ScoreArgumentation.Prompt));
        messages.Add(new UserChatMessage(TextEvaluatingPrompts.ProvideStructureFeedback.Prompt));
        messages.Add(new UserChatMessage(TextEvaluatingPrompts.GiveEssayStructureRecommendation.Prompt));
        messages.Add(new UserChatMessage(TextEvaluatingPrompts.ScoreEssayStructure.Prompt));
        messages.Add(new UserChatMessage(TextEvaluatingPrompts.ProvideAssignmentAnswerFeedback.Prompt));
        messages.Add(new UserChatMessage(TextEvaluatingPrompts.GiveAssignmentAnswerRecommendation.Prompt));
        messages.Add(new UserChatMessage(TextEvaluatingPrompts.ScoreAssignmentAnswer.Prompt));
        
        var answerSchema = JsonHelper.getJsonSchema<GenerateScoreDTO>();
        
        var responseSchema = JsonSerializer.Serialize(answerSchema, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        
        var options = new ChatCompletionOptions
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName:"essay_feedback",
                jsonSchema: BinaryData.FromBytes(Encoding.UTF8.GetBytes($$$"""
                                                                           {{{responseSchema}}}
                                                                           """).ToArray()
                ),jsonSchemaIsStrict:true),
            Temperature = 0.1f
        };

        
        
        try
        {
            var completion = await _client.CompleteChatAsync(messages, options);
            AddChatAssistantMessage(completion.Value.Content[0].Text);
            
            var generateScoreDto = JsonConvert.DeserializeObject<GenerateScoreDTO>(completion.Value.Content[0].Text);

            return generateScoreDto;
        }
        catch (JsonReaderException e)
        {
            Console.WriteLine($"Error in reading Json: {e}");
            throw;
        }
        catch (JsonSerializationException e)
        {
            Console.WriteLine($"Error in serializing Json: {e}");
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw e;
        };
    }
    public async Task<float> GetScoreAsync(string prompt)
    {
        var conversation = messages;
        conversation.Add(new UserChatMessage(prompt));

        var answerSchema = JsonHelper.getJsonSchema<ScoreDTO>();
        
        var responseSchema = JsonSerializer.Serialize(answerSchema, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        
        var options = new ChatCompletionOptions
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName:"essay_feedback",
                jsonSchema: BinaryData.FromBytes(Encoding.UTF8.GetBytes($$$"""
                                                                           {{{responseSchema}}}
                                                                           """).ToArray()
                ),jsonSchemaIsStrict:true),
            Temperature = 0.1f
        };

        var completion = await _client.CompleteChatAsync(conversation, options);
        AddChatAssistantMessage(completion.Value.Content[0].Text);
        
        try
        {
            var score = JsonConvert.DeserializeObject<ScoreDTO>(completion.Value.Content[0].Text);

            return score.Score;

        }
        catch (JsonReaderException e)
        {
            Console.WriteLine($"Error in reading Json: {e}");
            throw;
        }
        catch (JsonSerializationException e)
        {
            Console.WriteLine($"Error in serializing Json: {e}");
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        };
    }
}