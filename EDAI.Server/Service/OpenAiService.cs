using System.ClientModel;
using System.Text;
using Azure.AI.OpenAI;
using EDAI.Server.Prompts;
using EDAI.Services.Interfaces;
using EDAI.Shared.Models.Entities;
using EDAI.Shared.Models.Enums;
using OpenAI.Chat;
using System.Text.Json;
using EDAI.Shared.Tools;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace EDAI.Services;

public class OpenAiService : IOpenAiService
{
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

        var answerSchema = JsonHelper.getJsonSchema<IEnumerable<BaseComment>>();
        
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
            var comments = JsonConvert.DeserializeObject<IEnumerable<BaseComment>>(completion.Value.Content[0].Text);

            var feedbackComments = new List<FeedbackComment>();
            
            foreach (var comment in comments)
            {
                feedbackComments.Add(new FeedbackComment(comment, commentType));
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
            throw;
        };
    }
    
    public async Task<string> GetFeedbackAsync(string prompt)
    {
        var conversation = messages;
        conversation.Add(new UserChatMessage(prompt));

        var answerSchema = JsonHelper.getJsonSchema<string>();
        
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
            var feedback = JsonConvert.DeserializeObject<string>(completion.Value.Content[0].Text);

            return feedback;

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
    public async Task<float> GetScoreAsync(string prompt)
    {
        var conversation = messages;
        conversation.Add(new UserChatMessage(prompt));

        var answerSchema = JsonHelper.getJsonSchema<float>();
        
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
            var score = JsonConvert.DeserializeObject<float>(completion.Value.Content[0].Text);

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
}