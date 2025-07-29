using System.ClientModel;
using System.ClientModel.Primitives;
using System.Globalization;
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
        //"14iJ5AHR1VzKxf8yHNmWWqEFGPKz41zIo06oG816TufVbeKNyDwKJQQJ99AKACfhMk5XJ3w3AAABACOGRcGt"; 
        //"9qI35RSMYA99C35ZuyV3zmJ7KQw3OYZgNDTvh8BQg8StXOvABkV8JQQJ99BEACfhMk5XJ3w3AAABACOGG3J8";
        //Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY");
        Environment.GetEnvironmentVariable("OPENAI_API_KEY")
        ?? throw new InvalidOperationException("OPENAI_API_KEY is not set.");
    
    private static AzureOpenAIClient _openAiClient = new(
        //new Uri("https://edai-llm.openai.azure.com"), 
        new Uri("https://edai-v2.openai.azure.com/"),
        
        new ApiKeyCredential(keyFromEnvironment),
        new AzureOpenAIClientOptions
        {
            Transport = new HttpClientPipelineTransport(
                new HttpClient
                {
                    Timeout = TimeSpan.FromMinutes(10)
                }
                ),
        }
        );
    
    private static List<ChatMessage> messages = new List<ChatMessage>();

    private static ChatClient _client = _openAiClient.GetChatClient("gpt-4o-edai");

    public void InitiateScoreConversation(string essayText, string assignmentDescription, string referencetext = null)
    {
        messages.Add(new SystemChatMessage(TextEvaluatingPrompts.SystemRole.Prompt));
        messages.Add(new UserChatMessage(TextEvaluatingPrompts.ProvideAssignmentContextPrompt(assignmentDescription,referencetext)));
        messages.Add(new UserChatMessage(TextEvaluatingPrompts.ProvideEssay(essayText)));
    }

    public void InitiateStudentSummaryConversation()
    {
        messages.Add(TextEvaluatingPrompts.SystemRoleStudent.Prompt);
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

    public async Task<AreaScoreDTO> AssessArea(CommentType area)
    {
        var prompts = AssignCommentPrompts(area);

        var conversation = CopyConversation(messages);
        
        var score = new AreaScoreDTO();

        score.Comments = await GetAiObjectResponseAsync<CommentsDTO>(prompts.CommentPrompt, conversation);
        Console.WriteLine($"{DateTime.Now.ToString(CultureInfo.CurrentCulture)}: Received Comments");
        score.Recommendation = await GetAiObjectResponseAsync<FeedbackDTO>(prompts.RecommendationPrompt, conversation);
        Console.WriteLine($"{DateTime.Now.ToString(CultureInfo.CurrentCulture)}: Received Recommendation");
        score.Score = await GetAiObjectResponseAsync<ScoreDTO>(prompts.ScorePrompt, conversation);
        Console.WriteLine($"{DateTime.Now.ToString(CultureInfo.CurrentCulture)}: Received Score");

        return score;
    }

    public async Task<OverallScoreDTO> AssessOverall(FeedbackType feedbackType)
    {
        var prompts = AssignFeedbackPrompts(feedbackType);

        var conversation = CopyConversation(messages);

        var score = new OverallScoreDTO();

        score.Feedback = await GetAiObjectResponseAsync<FeedbackDTO>(prompts.FeedbackPrompt, conversation);
        Console.WriteLine($"{DateTime.Now.ToString(CultureInfo.CurrentCulture)}: Received Feedback");
        score.Recommendation = await GetAiObjectResponseAsync<FeedbackDTO>(prompts.RecommendationPrompt, conversation);
        Console.WriteLine($"{DateTime.Now.ToString(CultureInfo.CurrentCulture)}: Received Recommendation");
        score.Score = await GetAiObjectResponseAsync<ScoreDTO>(prompts.ScorePrompt, conversation);
        Console.WriteLine($"{DateTime.Now.ToString(CultureInfo.CurrentCulture)}: Received Score");

        return score;
    }

    public async Task<GenerateScoreDTO> AssessEssayAsync()
    {
        
        Console.WriteLine($"{DateTime.Now.ToString(CultureInfo.CurrentCulture)}: Starting to Asses Argumentation");
        var argumentationArea = await AssessArea(CommentType.Logic);
        Console.WriteLine($"{DateTime.Now.ToString(CultureInfo.CurrentCulture)}: Received Argumentation. Starting to Asses Eloquence");
        var eloquenceArea = await AssessArea(CommentType.Eloquence);
        Console.WriteLine($"{DateTime.Now.ToString(CultureInfo.CurrentCulture)}: Received Eloquence. Starting to Asses Grammar");
        var grammarArea = await AssessArea(CommentType.Grammar);
        Console.WriteLine($"{DateTime.Now.ToString(CultureInfo.CurrentCulture)}: Received Grammar. Starting to Asses Structure");
        var structureArea = await AssessOverall(FeedbackType.Structure);
        Console.WriteLine($"{DateTime.Now.ToString(CultureInfo.CurrentCulture)}: Received Structure. Starting to Asses Assignment Answer");
        var answerArea = await AssessOverall(FeedbackType.AssignmentAnswer);
         Console.WriteLine($"{DateTime.Now.ToString(CultureInfo.CurrentCulture)}: Received Assignment Answer");

        var score = new GenerateScoreDTO
        {
            GrammarComments = grammarArea.Comments.Comments,
            GrammarRecommendation = grammarArea.Recommendation.Feedback,
            GrammarScore = grammarArea.Score.Score,
            EloquenceComments = eloquenceArea.Comments.Comments,
            EloquenceRecommendation = eloquenceArea.Recommendation.Feedback,
            EloquenceScore = eloquenceArea.Score.Score,
            ArgumentationComments = argumentationArea.Comments.Comments,
            ArgumentationRecommendation = argumentationArea.Recommendation.Feedback,
            ArgumentationScore = argumentationArea.Score.Score,
            OverallStructure = structureArea.Feedback.Feedback,
            OverallStructureRecommendation = structureArea.Recommendation.Feedback,
            OverallStructureScore = structureArea.Score.Score,
            AssignmentAnswer = answerArea.Feedback.Feedback,
            AssignmentAnswerRecommendation = answerArea.Recommendation.Feedback,
            AssignmentAnswerScore = answerArea.Score.Score,
            OverallScore = (grammarArea.Score.Score + eloquenceArea.Score.Score + argumentationArea.Score.Score 
                           + structureArea.Score.Score + answerArea.Score.Score) / 5.0f

        };
        
        /*
        
        var testFilePath = Path.Combine(AppContext.BaseDirectory, "TestData", "Response", "poorResponse.json");
        var json = File.ReadAllText(testFilePath);
        var opts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var score = JsonSerializer.Deserialize<GenerateScoreDTO>(json, opts); */

        return score;
        
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

    public async Task<StudentSummaryDTO> GenerateStudentSummary(IEnumerable<Score> scores)
    {
        var input = JsonConvert.SerializeObject(scores);
        
        var studentSummaryDto =
            await GetAiObjectResponseAsync<StudentSummaryDTO>(input,messages);

        return studentSummaryDto;
    }

    private List<ChatMessage> CopyConversation(List<ChatMessage> existingMessages)
    {
        var conversation = new List<ChatMessage>();

        foreach (var message in existingMessages)
        {
            conversation.Add(message);
        }
        
        return conversation;
    }

    public T DeserializeChat<T>(string json)
    {
        try
        {
            var score = JsonConvert.DeserializeObject<T>(json);

            return score;

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

    private AreaPrompts AssignCommentPrompts(CommentType area)
    {
        var returnValue = new AreaPrompts();
        
        switch (area)
        {
            case CommentType.Grammar:
                returnValue.CommentPrompt = TextEvaluatingPrompts.ProvideGrammarComments.Prompt;
                returnValue.RecommendationPrompt = TextEvaluatingPrompts.GiveGrammarRecommendation.Prompt;
                returnValue.ScorePrompt = TextEvaluatingPrompts.ScoreGrammar.Prompt;
                break;
            case CommentType.Eloquence:
                returnValue.CommentPrompt = TextEvaluatingPrompts.ProvideEloquenceComments.Prompt;
                returnValue.RecommendationPrompt = TextEvaluatingPrompts.GiveEloquenceRecommendation.Prompt;
                returnValue.ScorePrompt = TextEvaluatingPrompts.ScoreEloquence.Prompt;
                break;
            case CommentType.Logic:
                returnValue.CommentPrompt = TextEvaluatingPrompts.ProvideArgumentComments.Prompt;
                returnValue.RecommendationPrompt = TextEvaluatingPrompts.GiveArgumentationRecommendation.Prompt;
                returnValue.ScorePrompt = TextEvaluatingPrompts.ScoreArgumentation.Prompt;
                break;
            default:
                throw new Exception("Invalid Area Type");
        }

        return returnValue;
    }
    
    private FeedbackPrompts AssignFeedbackPrompts(FeedbackType area)
    {
        var returnValue = new FeedbackPrompts();
        
        switch (area)
        {
            case FeedbackType.Structure:
                returnValue.FeedbackPrompt = TextEvaluatingPrompts.ProvideGrammarComments.Prompt;
                returnValue.RecommendationPrompt = TextEvaluatingPrompts.GiveGrammarRecommendation.Prompt;
                returnValue.ScorePrompt = TextEvaluatingPrompts.ScoreGrammar.Prompt;
                break;
            case FeedbackType.AssignmentAnswer:
                returnValue.FeedbackPrompt = TextEvaluatingPrompts.ProvideEloquenceComments.Prompt;
                returnValue.RecommendationPrompt = TextEvaluatingPrompts.GiveEloquenceRecommendation.Prompt;
                returnValue.ScorePrompt = TextEvaluatingPrompts.ScoreEloquence.Prompt;
                break;
            default:
                throw new Exception("Invalid Area Type");
        }

        return returnValue;
    }

    private ChatCompletionOptions GetChatOptions<T>()
    {
        var answerSchema = JsonHelper.getJsonSchema<T>();
        
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

        return options;
    }
    
    private async Task<T> GetAiObjectResponseAsync<T>(string prompt, List<ChatMessage> conversation)
    {
        conversation.Add(new UserChatMessage(prompt));

        var commentsOptions = GetChatOptions<T>();

        var commentsCompletion = await _client.CompleteChatAsync(conversation, commentsOptions);
        var commentResponse = commentsCompletion.Value.Content[0].Text;
        conversation.Add(new AssistantChatMessage(commentResponse));

        return DeserializeChat<T>(commentResponse);
    }
}