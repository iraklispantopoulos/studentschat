using Repository;
using Shared;
using UserChat;

namespace WebApiGateway.Helpers
{
    public class ChatFactory
    {
        private static Dictionary<int,UserChat.Chat> Chats = new Dictionary<int, UserChat.Chat>();
        private readonly IChatGPTClient _chatClient;
        private readonly IUserRepository _userRepository;
        private readonly IChatHistoryRepository _chatHistoryRepository;
        //private readonly ILogger _logger;
        private readonly Shared.PromptConfiguration.Configuration _configuration;
        public ChatFactory(
            IChatGPTClient chatClient, 
            IUserRepository userRepository,
            IChatHistoryRepository chatHistoryRepository,
            Shared.PromptConfiguration.Configuration configuration)
            //ILogger logger)            
        {
            _chatClient = chatClient;
            _userRepository = userRepository;
            _chatHistoryRepository = chatHistoryRepository;
            //_logger = logger;
            _configuration = configuration;
        }
        public UserChat.Chat GetChat(int userId)
        {
            if(Chats.ContainsKey(userId))
            {
                return Chats[userId];
            }
            else
            {
                //create a new chat through the dependency injection and return it
                var chat = new UserChat.Chat(
                    _chatClient, 
                    _userRepository,
                    _chatHistoryRepository, 
                    _configuration,
                    ServiceProviderWrapper.ServiceProvider.GetService<ILogger<Chat>>()!,
                    userId);
                Chats.Add(userId, chat);
                return chat;
            }
        }
    }
}
