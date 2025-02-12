using Microsoft.VisualBasic;
using Shared;

namespace UserChat
{
    public class Conversation
    {
        private readonly ResponsePrompt _unrecognizedPromptLiteral = new ResponsePrompt()
        {
            ClearTextFormat = "Συγγνώμη, αλλά δεν καταλαβα τι μου είπες.Μπορείς να το επαναλάβεις; Αν γίνεται αργά και καθάρα.",
            SsmlFormat = "<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xml:lang=\"el-GR\"><voice name=\"Microsoft Stefanos\"><prosody rate=\"medium\" pitch=\"high\">Συγγνώμη, αλλά δεν κατάλαβα τι μου είπες.<break time=\"500ms\"/> Μπορείς να το επαναλάβεις;<break time=\"400ms\"/> Αν γίνεται, αργά και καθαρά.<break time=\"300ms\"/></prosody></voice></speak>"
        };
        private readonly string _unrecogisedLiteral = "unrecognized";
        private readonly string _doneLiteral = "done";
        private readonly int _summaryWordsLimit = 100;
        private readonly IChatGPTClient _chatClient;
        private List<ConversationRecord> _history = new List<ConversationRecord>();
        private readonly string _conversationStarter;
        private string _keyPoints = string.Empty;        
        private string _recentSummary = string.Empty;
        private readonly int _memorySize;
        private readonly int _duration;
        private readonly string _topic;
        private readonly DateTime _startTime;
        /// <summary>
        /// handles a conversation type of chat between a user and an agent(ChatGPT)
        /// </summary>
        /// <param name="chatClient">the chatgpt client</param>
        /// <param name=""></param>
        /// <param name="memorySize">the memorySize of the conversation measured in responses.For example a value of 3, means that the agent, will have always have last 3 responses between the user and the agent. The previous ones, will not be used.The lower the limit, the faster the agent will respond.</param>        
        /// <param name="duration">the number of seconds the conversation must be, before it is marked as finished.0 means idefinetely,or until the user express desire to end.</param>
        public Conversation(string topic,IChatGPTClient chatClient, int memorySize,string conversationStarter, int duration = 0)
        {
            _chatClient = chatClient;
            _memorySize = memorySize;
            _duration = duration;
            _topic = topic;
            _startTime = DateTime.UtcNow;
            _conversationStarter = conversationStarter;
        }
        public bool ConversationLimitReached()
        {
            return _duration > 0 && DateTime.UtcNow.Subtract(_startTime).TotalSeconds > _duration;
        }        
        
        public async Task<ResponsePrompt> GetConversationResponse(string userPrompt)
        {
            //todo check if the conversation is finished or the user is engaging in the conversation!!

            var _userChatHistory = string.Empty;
            var _agentChatHistory = string.Empty;
            string conversation = string.Empty;
            List<ConversationRecord> recentHistory = new List<ConversationRecord>();
            if (_history.Count >= _memorySize)
            {
                if (string.IsNullOrEmpty(_keyPoints))
                    _keyPoints = await Summarize(_history);
                else
                    conversation = $"This is the recent summary of the conversation so far:\n{(await Summarize(_history))}";
                _history.Clear();
            }
            else
            {
                if (_history.Count > 0)
                {
                    recentHistory = _history.OrderByDescending(x => x.Date).Take(_memorySize).OrderBy(p => p.Date).ToList();
                    conversation = $"This is the most recent part of the conversation so far:\n{BuildConversation(recentHistory)}";
                }
            }
            var agentInstructions = $"In this session you are conversing with a user about {_topic}.The conversation starte by promping the user with '{_conversationStarter}'.If the user is asking a question or making a state that you dont understand, or does not fall into the conversation context, return an appropriate response.Moreover dont just repeat the user prompt if no question is being asked, but instead ask for more information and always.Lastly if the user seems not wanting anymore help or is not engaging to the conversation,or your response is signaling the end of the conversation have the word 'conv-done' in the response .\n";
            if (!string.IsNullOrEmpty(_keyPoints))
                agentInstructions += $"These are the key points of the conversation:{_keyPoints}\n";
            if (!string.IsNullOrEmpty(conversation))
                agentInstructions += conversation;
            var response = await _chatClient.GetSsmlResponse(userPrompt, agentInstructions);
            if (response.ClearTextFormat.Trim().Contains("conv-done"))
                return null!;
            if (response.ClearTextFormat.Trim() == _unrecogisedLiteral)

                return _unrecognizedPromptLiteral;
            else
            {
                _history.Add(new ConversationRecord
                {
                    Date = DateTime.UtcNow,
                    UserPrompt = new Prompt { Text = userPrompt },
                    AgentResponse = new Prompt { Text = response.ClearTextFormat }
                });
                return new ResponsePrompt
                {
                    SsmlFormat = response.SsmlFormat,
                    ClearTextFormat = response.ClearTextFormat
                };
            }
        }
        private string BuildConversation(List<ConversationRecord> conversationRecords)
        {
            if (conversationRecords.Count == 0)
            {
                return string.Empty;
            }
            var conversation = "";
            foreach (var record in conversationRecords)
            {
                conversation += ($"User: {record.UserPrompt.Text}\nAgent: {record.AgentResponse.Text}\n");
            }
            return conversation;
        }
        private async Task<string> Summarize(List<ConversationRecord> conversationRecords)
        {
            string instructions = $"i want you to summarize the conversation history in a way that is easy to understand.Limit your response to {_summaryWordsLimit} words at most.Use as less words as possible. The conversation history is as follows:\n{BuildConversation(conversationRecords)}";
            return (await _chatClient.GetResponse("", instructions));
        }
        private class ConversationRecord
        {
            public DateTime Date { get; set; }
            public Prompt UserPrompt { get; set; }
            public Prompt AgentResponse { get; set; }
        }
    }
}