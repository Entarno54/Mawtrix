using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Mawtrix.Matrix.Sdk.Core.Infrastructure.Dto.Event
{
    public record ImageMessageEvent(string body, string url)
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public MessageType msgtype = MessageType.Image;
    }
}
