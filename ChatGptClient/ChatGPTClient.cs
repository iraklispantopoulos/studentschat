using OpenAI.Chat;
using Shared;

namespace ChatGptClient
{
    public class ChatGPTClient : IChatGPTClient
    {
        private string _apiKey;
        private string _model;
        public ChatGPTClient(string apiKey, string model)
        {
            _apiKey = apiKey;
            _model = model;
        }
        public async Task<string> GetResponse(string userPrompt, string agentinstructions)
        {            
            OpenAI.Chat.ChatClient client = new(model: _model, apiKey: _apiKey);            

            var messages = new List<ChatMessage>
                {
                    ChatMessage.CreateSystemMessage(agentinstructions),
                    ChatMessage.CreateUserMessage(userPrompt)
                };

            ChatCompletion completion = await client.CompleteChatAsync(messages);
            var response = completion.Content[0].Text;
            return response;
        }
        public async Task<ResponsePrompt> GetSsmlResponse(string prompt, string userChatHistory, string agentChatHistory)
        {
            var instructions = "i want you to be interactive.i want you to respond in two parts, in the same prompt response. The first part should be in ssml format.However the actual content should not be more than 2 lines." +
                "I want you to generate the responses in a way to make them sound as natural as possible.Moreover never use low rate. Lets say you had to provide the following response: Γειά σου! Είμαι τόσο χαρούμενος που σε βλέπω!Σήμερα είναι μια υπέροχη μέρα, ας κάνουμε κάτι διασκεδαστικό!Χαμογέλα, γιατί όλα πάνε καλά! " +
                "I would expect something like the following " +
                "<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='el-GR'> " +
                    "<voice name='Microsoft Stefanos'> " +
                        "<prosody rate='medium' pitch='high'> " +
                            "<p>Γειά σου! Είμαι τόσο χαρούμενος που σε βλέπω!</p> " +
                        "</prosody> " +
                        "<prosody rate='medium' pitch='high'> " +
                            "<p>Σήμερα είναι μια υπέροχη μέρα, ας κάνουμε κάτι διασκεδαστικό!</p> " +
                        "</prosody>" +
                        "<break time='300ms'/> " +
                        "<prosody rate=\"\"medium\"\" pitch=\"\"high\"\"> " +
                            "<emphasis level=\"\"strong\"\">Χαμογέλα, γιατί όλα πάνε καλά!</emphasis> " +
                        "</prosody>" +
                   "</voice> " +
                "</speak>";
            instructions += ($"\n. The second part should be the actual content prior to the ssml reformat.The two parts should be separated by 5 * characters, so i can separate the parts afterwards");
            OpenAI.Chat.ChatClient client = new(model: _model, apiKey: _apiKey);
            if (!string.IsNullOrEmpty(userChatHistory))
            {
                instructions += ($"\n. Επίσης να θυμάσαι ότι μέχρι στιγμής σου έχω πεί τα παρακάτω.\n{userChatHistory}");
            }
            if (!string.IsNullOrEmpty(agentChatHistory))
            {
                instructions += ($"\n. Επίσης να θυμάσαι ότι μέχρι στιγμής μου έχεις απαντήσει τα παρακάτω.\n{agentChatHistory}");
            }

            var messages = new List<ChatMessage>
                {
                    ChatMessage.CreateSystemMessage(instructions),
                    ChatMessage.CreateUserMessage(prompt)
                };

            ChatCompletion completion = await client.CompleteChatAsync(messages);
            //split the response into the two parts and return the response
            var response = completion.Content[0].Text;
            var parts = response.Split("*****");
            return new ResponsePrompt
            {
                SsmlFormat = parts[0],
                ClearTextFormat = parts[1]
            };
        }
        public async Task<ResponsePrompt> GetSsmlResponse(string prompt, string agentInstructions)
        {
            var instructions = "i want you to be interactive.i want you to respond in two parts, in the same prompt response. The first part should be in ssml format.However the actual content should not be more than 2 lines." +
                "I want you to generate the responses in a way to make them sound as natural as possible.Moreover never use low rate. Lets say you had to provide the following response: Γειά σου! Είμαι τόσο χαρούμενος που σε βλέπω!Σήμερα είναι μια υπέροχη μέρα, ας κάνουμε κάτι διασκεδαστικό!Χαμογέλα, γιατί όλα πάνε καλά! " +
                "I would expect something like the following " +
                "<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='el-GR'> " +
                    "<voice name='Microsoft Stefanos'> " +
                        "<prosody rate='medium' pitch='high'> " +
                            "<p>Γειά σου! Είμαι τόσο χαρούμενος που σε βλέπω!</p> " +
                        "</prosody> " +
                        "<prosody rate='medium' pitch='high'> " +
                            "<p>Σήμερα είναι μια υπέροχη μέρα, ας κάνουμε κάτι διασκεδαστικό!</p> " +
                        "</prosody>" +
                        "<break time='300ms'/> " +
                        "<prosody rate=\"\"medium\"\" pitch=\"\"high\"\"> " +
                            "<emphasis level=\"\"strong\"\">Χαμογέλα, γιατί όλα πάνε καλά!</emphasis> " +
                        "</prosody>" +
                   "</voice> " +
                "</speak>";
            instructions += ($"\n. The second part should be the actual content prior to the ssml reformat.The two parts should be separated by 5 * characters, so i can separate the parts afterwards");
            OpenAI.Chat.ChatClient client = new(model: _model, apiKey: _apiKey);
            
            if (!string.IsNullOrEmpty(agentInstructions))
            {
                instructions += agentInstructions;
            }

            var messages = new List<ChatMessage>
                {
                    ChatMessage.CreateSystemMessage(instructions),
                    ChatMessage.CreateUserMessage(prompt)
                };

            ChatCompletion completion = await client.CompleteChatAsync(messages);
            //split the response into the two parts and return the response
            var response = completion.Content[0].Text;
            var parts = response.Split("*****");
            return new ResponsePrompt
            {
                SsmlFormat = parts[0],
                ClearTextFormat = parts[1]
            };
        }
    }
}
