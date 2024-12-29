using System.ClientModel;
using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace EDAI.Services;

public class OpenAiService
{
    private static string keyFromEnvironment =
        ""; 
        
        //Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY");
    
    private static AzureOpenAIClient _openAiClient = new(
        new Uri("https://edai-llm.openai.azure.com"),
        new ApiKeyCredential(keyFromEnvironment)
        );

    private static ChatClient _client = _openAiClient.GetChatClient("gpt-4o-mini");

    public async Task<string> helloWordAsync()
    {
        var completion = await _client.CompleteChatAsync("Tell me a joke about a frog");

        return completion.Value.Content[0].Text;
    }

    public string helloWorld()
    {
        var completion = _client.CompleteChat("Tell me a joke");

        return completion.Value.Content[0].Text;
    }
}