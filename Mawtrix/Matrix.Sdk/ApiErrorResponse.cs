using Newtonsoft.Json;

namespace Mawtrix.Matrix.Sdk
{
    public class ApiErrorResponse
    {
        [JsonProperty("retry_after_ms")]
        public int retryAfterMs;
    }
}

