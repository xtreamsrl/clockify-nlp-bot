using Newtonsoft.Json;

namespace Bot.Common.ChannelData
{
    public abstract class ChannelData<T>
    {
        protected ChannelData(string method, T parameters)
        {
            Method = method;
            Parameters = parameters;
        }

        [JsonProperty("method")] private string Method { get; }
        
        [JsonProperty("parameters")] private T Parameters { get; }
        
    }
}