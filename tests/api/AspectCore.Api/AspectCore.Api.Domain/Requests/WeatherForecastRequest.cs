using System.Text.Json.Serialization;

namespace AspectCore.Api.Domain.Requests
{
    public sealed class WeatherForecastRequest
    {
        [JsonPropertyName("city")]
        public string City { get; private set; }
        [JsonPropertyName("date")]
        public DateTime Date { get; private set; }

        public WeatherForecastRequest(string city, DateTime date)
        {
            City = city;
            Date = date;
        }
    }
}
