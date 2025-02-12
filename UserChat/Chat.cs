using Shared;
using Shared.PromptConfiguration;
using Repository;
using Configuration = Shared.PromptConfiguration;
using Microsoft.Extensions.Logging;
namespace UserChat
{
    public class Chat
    {
        private readonly ResponsePrompt _repeatPromptResponsePrompt = new ResponsePrompt()
        {
            ClearTextFormat = "μπορεις να επαναλάβεις την απάντηση σου σε παρακαλώ;",
            SsmlFormat = "<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xml:lang=\"el-GR\"><voice name=\"Microsoft Stefanos\"><prosody rate=\"medium\" pitch=\"high\">Μπορείς να επαναλάβεις την απάντησή σου, σε παρακαλώ;<break time=\"400ms\"/></prosody></voice></speak>"
        };
        private readonly ResponsePrompt _chatEndResponsePrompt = new ResponsePrompt()
        {
            ClearTextFormat = "Ευχαριστώ πολύ για την συνομιλία.Να έχεις μια όμορφη μέρα.",
            SsmlFormat = "<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xml:lang=\"el-GR\"><voice name=\"Microsoft Stefanos\"><prosody rate=\"medium\" pitch=\"high\">Ευχαριστώ πολύ για την συνομιλία.Να έχεις μια όμορφη μέρα!<break time=\"400ms\"/></prosody></voice></speak>"
        };
        private readonly IChatGPTClient _chatClient;
        private readonly IUserRepository _userRepository;
        private readonly IChatHistoryRepository _chatHistoryRepository;
        private Conversation _currentConversation;
        private readonly Configuration.Configuration _configuration;
        private readonly ILogger<Chat> _logger;
        private Configuration.Prompt _currentPrompt;
        private Unit _currentUnit;
        private readonly int _userId;
        private string _previousUserResponse = "";
        private UserChat.ResponsePrompt _lastAgentResponse = null;
        public Chat(
            IChatGPTClient chatClient,
            IUserRepository userRepository,
            IChatHistoryRepository chatHistoryRepository,
            Configuration.Configuration configuration,
            ILogger<Chat> logger,
            int userId)
        {
            _chatClient = chatClient;
            _userRepository = userRepository;
            _chatHistoryRepository = chatHistoryRepository;
            _configuration = configuration;
            _logger = logger;
            _userId = userId;
        }
        public async Task<Shared.ResponsePrompt> Start(bool continueFromLastPrompt)
        {
            try
            {                
                Func<(Shared.PromptConfiguration.Unit unit, Shared.PromptConfiguration.Prompt prompt)> getStartPrompt = () =>
                {
                    var unit = _configuration.Units.Where(x => x.Type == "start").FirstOrDefault()!;
                    var prompt = unit.Prompts.FirstOrDefault()!;
                    return (unit, prompt);
                };
                Func<(Shared.PromptConfiguration.Unit unit, Shared.PromptConfiguration.Prompt prompt)> getGenericStartPrompt = () =>
                {
                    var unit = _configuration.Units.Where(x => x.Type == "generic").FirstOrDefault()!;
                    var prompt = unit.Prompts.FirstOrDefault()!;
                    return (unit, prompt);
                };
                Unit currentUnit = null!;
                Shared.PromptConfiguration.Prompt currentPrompt = null!;
                if (continueFromLastPrompt)
                {
                    var user = await _userRepository.Get(_userId);
                    if (string.IsNullOrEmpty(user.CurrentPromptId))
                    {
                        var startPrompt = getStartPrompt();
                        currentUnit = startPrompt.unit;
                        currentPrompt = startPrompt.prompt;
                    }
                    else
                    {
                        foreach (var unit in _configuration.Units)
                        {
                            if (unit.Id == user.CurrentUnitId)
                            {
                                var prompt = unit.Prompts.FirstOrDefault(x => x.Id == user.CurrentPromptId);
                                if (prompt != null)
                                {
                                    if (unit.Type != "end")
                                    {
                                        currentUnit = unit;
                                        currentPrompt = prompt;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (currentUnit == null || currentPrompt == null)
                    {
                        var startPrompt = getStartPrompt();
                        currentUnit = startPrompt.unit;
                        currentPrompt = startPrompt.prompt;
                    }
                    this._currentUnit = currentUnit;
                    this._currentPrompt = currentPrompt;
                    if (this._currentPrompt.Type == "conversation")
                    {
                        _currentConversation = new Conversation(this._currentPrompt.Conversation.Topic, _chatClient, this._currentPrompt.Conversation.Summarization.Size, this._currentPrompt.Message.Text, this._currentPrompt.Conversation.MaxDuration);
                    }
                }
                else
                {
                    var startPrompt = getGenericStartPrompt();
                    this._currentPrompt = startPrompt.prompt;
                    this._currentUnit = startPrompt.unit;
                }

                _lastAgentResponse = new UserChat.ResponsePrompt()
                {
                    ClearTextFormat = _currentPrompt.Message.Text,
                    SsmlFormat = _currentPrompt.Message.Ssml,
                    PromptId = _currentPrompt.Id,
                    UnitId = _currentUnit.Id
                };
                return _lastAgentResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting chat");
                throw;
            }
        }
        public Task<ResponsePrompt> GetLastAgentPrompt()
        {
            return Task.FromResult(_lastAgentResponse);
        }
        public async Task<ResponsePrompt> ProcessUserResponse(string userPrompt)
        {
            try
            {
                ResponsePrompt responsePrompt = null!;
                if (string.IsNullOrEmpty(userPrompt))
                {
                    return _repeatPromptResponsePrompt;
                }

                if (_currentPrompt.Type == "question")
                {
                    Shared.PromptConfiguration.Prompt nextPrompt = null!;
                    if (await QuestionHasPossibleAnswers())
                    {
                        var matchedAnswer = await GetUserAnswer(userPrompt);
                        if (matchedAnswer != null)
                            nextPrompt = GetPrompt(matchedAnswer.PromptId);
                        if (nextPrompt == null)
                            nextPrompt = GetPrompt(_currentPrompt.NextPrompt);
                    }
                    else
                        nextPrompt = GetPrompt(_currentPrompt.NextPrompt);
                    if (nextPrompt.Type == "conversation")
                    {
                        _currentConversation = new Conversation(nextPrompt.Conversation.Topic, _chatClient, nextPrompt.Conversation.Summarization.Size, nextPrompt.Message.Text, nextPrompt.Conversation.MaxDuration);
                    }
                    responsePrompt = new ResponsePrompt()
                    {
                        ClearTextFormat = nextPrompt.Message.Text,
                        SsmlFormat = nextPrompt.Message.Ssml,
                        PromptId = nextPrompt.Id,
                        UnitId = _currentUnit.Id
                    };
                    _currentPrompt = nextPrompt;
                }
                else
                {
                    Shared.PromptConfiguration.Prompt nextPrompt = null!;
                    if (!_currentConversation.ConversationLimitReached())
                    {
                        responsePrompt = await _currentConversation.GetConversationResponse(userPrompt);
                    }
                    if (responsePrompt == null)
                    {
                        _currentPrompt = nextPrompt = GetPrompt(_currentPrompt.NextPrompt);
                        _currentConversation = null!;
                        responsePrompt = new ResponsePrompt()
                        {
                            ClearTextFormat = nextPrompt.Message.Text,
                            SsmlFormat = nextPrompt.Message.Ssml,
                            PromptId = nextPrompt.Id,
                            UnitId = _currentUnit.Id
                        };
                    }
                }
                _previousUserResponse = userPrompt;
                _lastAgentResponse = responsePrompt;
                return responsePrompt;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing user response");
                throw;
            }
        }
        private Task<bool> QuestionHasPossibleAnswers()
        {
            return Task.FromResult(_currentPrompt.PossibleAnswers.Count > 0);
        }
        /// <summary>
        /// checks the users answer in the question asked.This method makes sense when the question has been configured to have possible answers to one question
        /// </summary>
        /// <param name="userPrompt">the user's answer</param>
        /// <returns>the answer that matched the user's response. If no match was found, we return null</returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task<Configuration.PossibleAnswer> GetUserAnswer(string userPrompt)
        {
            var possibleAnswersStr = "";
            var goal = "";
            string questionGoal = "";
            int counter = 1;
            _currentPrompt.PossibleAnswers
                            .Select(p => p.UserPrompt)
                            .ToList()
                            .ForEach(userPrompt => possibleAnswersStr += $"{counter++}.'{userPrompt}'\n");
            if (_currentPrompt.Goal != null)
                goal = $"The purpose of the question is {_currentPrompt.Goal}.";
            var agentInstructions = $"I asked the user '{_currentPrompt.Message.Text}' and the user responded '{userPrompt}'.{goal} Return only one character which must be the number of the sentence  that resembles the user response from the following sentences:{possibleAnswersStr}.We dont want a direct match but to see which type of answer the user chose. If you think that none of the sentences resembles the user response, return the reason you did not choose one of the sentences.";

            var response = await _chatClient.GetResponse(userPrompt, agentInstructions);
            if (!int.TryParse(response, out int answerIndex))
                return null!;
            else
            {
                if (answerIndex == 0)
                    return null!;
                return _currentPrompt.PossibleAnswers[answerIndex - 1];
            }
        }

        private Configuration.Prompt GetPrompt(string promptId)
        {
            if (_currentPrompt != null)
            {
                Configuration.Prompt? nextPrompt = null;
                if (!string.IsNullOrEmpty(promptId))
                    nextPrompt = _currentUnit.Prompts.FirstOrDefault(x => x.Id == promptId)!;
                if (nextPrompt == null)
                {
                    var nextUnit = _configuration.Units.FirstOrDefault(x => x.Id == _currentUnit.NextUnit);
                    if (nextUnit != null)
                    {
                        _currentUnit = nextUnit;
                        nextPrompt = nextUnit.Prompts.FirstOrDefault();
                    }
                }
                return nextPrompt!;
            }
            else
            {
                return null;
            }
        }
    }
}