using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDayApi.Tests
{
    public class WeatherTests : TestBase
    {
        private object MakeWeatherBody(DateTime forecastDate, decimal temperature = 21.5m, string condition = "Sunny")
        {
            return new
            {
                forecastDate = forecastDate,
                temperatureC = temperature,
                weatherCondition = condition,
                windSpeedKmh = 12.5,
                chanceOfRainPercent = 10
            };
        }

        [Fact]
        public async Task GetWeather_WhenNoForecastAdded_Returns404()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            int eventId = await this.CreateEvent(organiser);

            HttpResponseMessage response = await this._client.GetAsync("/api/events/" + eventId + "/weather");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetWeather_ForUnknownEvent_Returns404()
        {
            HttpResponseMessage response = await this._client.GetAsync("/api/events/9999/weather");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddWeather_AsOwner_Returns201()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            int eventId = await this.CreateEvent(organiser);

            HttpResponseMessage response = await organiser.PostAsJsonAsync(
                "/api/events/" + eventId + "/weather", this.MakeWeatherBody(DateTime.Today.AddDays(30)));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task AddWeather_ThenGetWeather_Returns200_WithTheForecast()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            int eventId = await this.CreateEvent(organiser);

            await organiser.PostAsJsonAsync(
                "/api/events/" + eventId + "/weather", this.MakeWeatherBody(DateTime.Today.AddDays(30)));

            HttpResponseMessage response = await this._client.GetAsync("/api/events/" + eventId + "/weather");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JsonElement json = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("Sunny", json.GetProperty("weatherCondition").GetString());
        }

        [Fact]
        public async Task AddWeather_SameDateAgain_UpdatesInsteadOfDuplicating()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            int eventId = await this.CreateEvent(organiser);
            DateTime forecastDate = DateTime.Today.AddDays(30);

            HttpResponseMessage first = await organiser.PostAsJsonAsync(
                "/api/events/" + eventId + "/weather", this.MakeWeatherBody(forecastDate, 21.5m));
            Assert.Equal(HttpStatusCode.Created, first.StatusCode);

            HttpResponseMessage second = await organiser.PostAsJsonAsync(
                "/api/events/" + eventId + "/weather", this.MakeWeatherBody(forecastDate, 24m));
            Assert.Equal(HttpStatusCode.OK, second.StatusCode);

            HttpResponseMessage get = await this._client.GetAsync("/api/events/" + eventId + "/weather");
            JsonElement json = await get.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(24m, json.GetProperty("temperatureC").GetDecimal());
        }

        [Fact]
        public async Task GetWeather_ReturnsTheLatestForecast()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            int eventId = await this.CreateEvent(organiser);

            await organiser.PostAsJsonAsync(
                "/api/events/" + eventId + "/weather", this.MakeWeatherBody(DateTime.Today.AddDays(10), 20m, "Sunny"));
            await organiser.PostAsJsonAsync(
                "/api/events/" + eventId + "/weather", this.MakeWeatherBody(DateTime.Today.AddDays(20), 15m, "Rainy"));

            HttpResponseMessage response = await this._client.GetAsync("/api/events/" + eventId + "/weather");
            JsonElement json = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.Equal("Rainy", json.GetProperty("weatherCondition").GetString());
        }

        [Fact]
        public async Task AddWeather_WhenNotLoggedIn_Returns401()
        {
            HttpResponseMessage response = await this._client.PostAsJsonAsync(
                "/api/events/1/weather", this.MakeWeatherBody(DateTime.Today.AddDays(30)));

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AddWeather_AsParticipant_Returns403()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            HttpClient participant = await this.CreateLoggedInClient("part@test.com", "Participant");
            int eventId = await this.CreateEvent(organiser);

            HttpResponseMessage response = await participant.PostAsJsonAsync(
                "/api/events/" + eventId + "/weather", this.MakeWeatherBody(DateTime.Today.AddDays(30)));

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task AddWeather_AsDifferentOrganiser_Returns403()
        {
            HttpClient owner = await this.CreateLoggedInClient("owner@test.com", "Organiser");
            HttpClient other = await this.CreateLoggedInClient("other@test.com", "Organiser");
            int eventId = await this.CreateEvent(owner);

            HttpResponseMessage response = await other.PostAsJsonAsync(
                "/api/events/" + eventId + "/weather", this.MakeWeatherBody(DateTime.Today.AddDays(30)));

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task AddWeather_ForUnknownEvent_Returns404()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");

            HttpResponseMessage response = await organiser.PostAsJsonAsync(
                "/api/events/9999/weather", this.MakeWeatherBody(DateTime.Today.AddDays(30)));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddWeather_WithChanceOfRainOver100_Returns400()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            int eventId = await this.CreateEvent(organiser);

            var body = new
            {
                forecastDate = DateTime.Today.AddDays(30),
                temperatureC = 20,
                weatherCondition = "Sunny",
                windSpeedKmh = 10,
                chanceOfRainPercent = 150
            };
            HttpResponseMessage response = await organiser.PostAsJsonAsync(
                "/api/events/" + eventId + "/weather", body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task AddWeather_WithNegativeWindSpeed_Returns400()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            int eventId = await this.CreateEvent(organiser);

            var body = new
            {
                forecastDate = DateTime.Today.AddDays(30),
                temperatureC = 20,
                weatherCondition = "Sunny",
                windSpeedKmh = -5,
                chanceOfRainPercent = 10
            };
            HttpResponseMessage response = await organiser.PostAsJsonAsync(
                "/api/events/" + eventId + "/weather", body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}