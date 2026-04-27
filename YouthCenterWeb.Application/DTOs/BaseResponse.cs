using System.Text.Json.Serialization;

namespace YouthCenterWeb.Models
{
    public class BaseResponse<T>
    {
        [JsonPropertyName("result")]
        public int Result { get; set; }

        [JsonPropertyName("dataCount")]
        public int DataCount { get; set; }

        [JsonPropertyName("data")]
        public T? Data { get; set; }

        [JsonPropertyName("alert")]
        public Alert? Alert { get; set; }

        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }

    public class Alert
    {
        // Add your Alert properties here (e.g., Title, Message)
        public string? MessageAr { get; set; }
        public string? MessageEn { get; set; }
    }
}