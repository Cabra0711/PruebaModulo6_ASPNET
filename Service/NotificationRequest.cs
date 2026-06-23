using System.Text.Json.Serialization;

namespace RentingBooking.Service;

public class NotificationRequest
{
    [JsonPropertyName("event")]
    public string Event { get; set; } = string.Empty;

    [JsonPropertyName("to")]
    public string To { get; set; } = string.Empty;

    [JsonPropertyName("userName")]
    public string UserName { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public Dictionary<string, object?> Data { get; set; } = new();
}
