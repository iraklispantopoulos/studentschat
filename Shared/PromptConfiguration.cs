using Newtonsoft.Json;
using System.Collections.Generic;
namespace Shared.PromptConfiguration
{
    public class Configuration
    {
        [JsonProperty("global_memory_size")]
        public int GlobalMemorySize { get; set; }
        [JsonProperty("units")]
        public List<Unit> Units { get; set; }

    }
    public class Unit
    {
        [JsonProperty("id")]        
        public string Id { get; set; }
        [JsonProperty("next_unit")]
        public string NextUnit { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("prompts")]
        public List<Prompt> Prompts { get; set; }
    }
    public class Prompt
    {
        [JsonProperty("Id")]
        public string Id { get; set; }
        [JsonProperty("goal")]
        public string Goal { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("message")]
        public Message Message { get; set; }

        [JsonProperty("possible_answers")]
        public List<PossibleAnswer> PossibleAnswers { get; set; }
        [JsonProperty("requested_information")]
        public List<RequestedInformation> RequestedInformation { get; set; }

        [JsonProperty("next_prompt")]
        public string NextPrompt { get; set; }

        [JsonProperty("conversation")]
        public Conversation Conversation { get; set; }
    }

    public class Message
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("ssml")]
        public string Ssml { get; set; }
    }

    public class PossibleAnswer
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("user_prompt")]
        public string UserPrompt { get; set; }

        [JsonProperty("prompt_id")]
        public string PromptId { get; set; }
    }
    public class Conversation
    {
        [JsonProperty("max_duration")]
        public int MaxDuration { get; set; }

        [JsonProperty("ending_que")]
        public string EndingQue { get; set; }

        [JsonProperty("summarization")]
        public Summarization Summarization { get; set; }
        [JsonProperty("topic")]
        public string Topic { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public class Summarization
    {
        [JsonProperty("frequency")]
        public int Frequency { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }
    }

    public class RequestedInformation
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("name_en")]
        public string NameEn { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }
    }

    public class Directions
    {
        [JsonProperty("description")]
        public string Description { get; set; }
    }    
}
