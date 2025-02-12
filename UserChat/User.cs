using Shared;
using Repository;
namespace UserChat
{
    internal class User
    {
        public string Guid { get; private set; }
        public bool ConversationMode = false;
        private List<Prompt> UserPrompts { get; set; }
        private List<Prompt> AgentPrompts { get; set; }
        private readonly IUserRepository _userRepository;        
        public User(string guid,IUserRepository userRepository) {
            Guid = guid;
            UserPrompts = new List<Prompt>();
            AgentPrompts = new List<Prompt>();
            _userRepository = userRepository;
        }
        public void AddUserPrompt(string prompt)
        {
            UserPrompts.Add(new Prompt()
            {
                Date = DateTime.UtcNow,
                Text = prompt
            });
        }
        public void AddAgentPrompt(string prompt)
        {
            AgentPrompts.Add(new Prompt()
            {
                Date = DateTime.UtcNow,
                Text = prompt
            });
        }        
        public List<string> GetUserPrompts()
        {
            return UserPrompts.Select(p=>p.Text).ToList();
        }
        public List<string> GetAgentPrompts()
        {
            return AgentPrompts.Select(p=> p.Text).ToList();
        }
    }
}
