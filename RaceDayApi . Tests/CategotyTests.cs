using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDayApi.Tests
{
    public class CategoryTests : TestBase
    {
        [Fact]
        public async Task GetCategories_ForExistingEvent_Returns200_WithTheCategory()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            int eventId = await this.CreateEvent(organiser);
            await this.CreateCategory(organiser, eventId);

            HttpResponseMessage response = await this._client.GetAsync("/api/events/" + eventId + "/categories");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JsonElement list = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(1, list.GetArrayLength());
        }

        [Fact]
        public async Task GetCategories_ForUnknownEvent_Returns404()
        {
            HttpResponseMessage response = await this._client.GetAsync("/api/events/9999/categories");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateCategory_AsOwner_Returns201()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            int eventId = await this.CreateEvent(organiser);

            HttpResponseMessage response = await organiser.PostAsJsonAsync(
                "/api/events/" + eventId + "/categories", this.MakeCategoryBody());

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task CreateCategory_WhenNotLoggedIn_Returns401()
        {
            HttpResponseMessage response = await this._client.PostAsJsonAsync(
                "/api/events/1/categories", this.MakeCategoryBody());

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateCategory_AsDifferentOrganiser_Returns403()
        {
            HttpClient owner = await this.CreateLoggedInClient("owner@test.com", "Organiser");
            HttpClient other = await this.CreateLoggedInClient("other@test.com", "Organiser");
            int eventId = await this.CreateEvent(owner);

            HttpResponseMessage response = await other.PostAsJsonAsync(
                "/api/events/" + eventId + "/categories", this.MakeCategoryBody());

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CreateCategory_ForUnknownEvent_Returns404()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");

            HttpResponseMessage response = await organiser.PostAsJsonAsync(
                "/api/events/9999/categories", this.MakeCategoryBody());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateCategory_WithZeroDistance_Returns400()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            int eventId = await this.CreateEvent(organiser);

            var body = new { categoryName = "Bad Category", distance = 0, entryFee = 100, maxParticipants = 10 };
            HttpResponseMessage response = await organiser.PostAsJsonAsync(
                "/api/events/" + eventId + "/categories", body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateCategory_WithEmptyName_Returns400()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            int eventId = await this.CreateEvent(organiser);

            var body = new { categoryName = "", distance = 10, entryFee = 100, maxParticipants = 10 };
            HttpResponseMessage response = await organiser.PostAsJsonAsync(
                "/api/events/" + eventId + "/categories", body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateCategory_AsOwner_Returns200()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            int eventId = await this.CreateEvent(organiser);
            int categoryId = await this.CreateCategory(organiser, eventId);

            var body = new { categoryName = "Renamed Category", distance = 21, entryFee = 200, maxParticipants = 50 };
            HttpResponseMessage response = await organiser.PutAsJsonAsync("/api/categories/" + categoryId, body);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task UpdateCategory_AsDifferentOrganiser_Returns403()
        {
            HttpClient owner = await this.CreateLoggedInClient("owner@test.com", "Organiser");
            HttpClient other = await this.CreateLoggedInClient("other@test.com", "Organiser");
            int eventId = await this.CreateEvent(owner);
            int categoryId = await this.CreateCategory(owner, eventId);

            HttpResponseMessage response = await other.PutAsJsonAsync(
                "/api/categories/" + categoryId, this.MakeCategoryBody());

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UpdateCategory_WithUnknownId_Returns404()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");

            HttpResponseMessage response = await organiser.PutAsJsonAsync(
                "/api/categories/9999", this.MakeCategoryBody());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteCategory_AsOwner_Returns200()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            int eventId = await this.CreateEvent(organiser);
            int categoryId = await this.CreateCategory(organiser, eventId);

            HttpResponseMessage response = await organiser.DeleteAsync("/api/categories/" + categoryId);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task DeleteCategory_AsDifferentOrganiser_Returns403()
        {
            HttpClient owner = await this.CreateLoggedInClient("owner@test.com", "Organiser");
            HttpClient other = await this.CreateLoggedInClient("other@test.com", "Organiser");
            int eventId = await this.CreateEvent(owner);
            int categoryId = await this.CreateCategory(owner, eventId);

            HttpResponseMessage response = await other.DeleteAsync("/api/categories/" + categoryId);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task DeleteCategory_WhenItHasEnrolments_Returns400()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            HttpClient participant = await this.CreateLoggedInClient("part@test.com", "Participant");
            int eventId = await this.CreateEvent(organiser);
            int categoryId = await this.CreateCategory(organiser, eventId);

            await this.Enrol(participant, eventId, categoryId);

            HttpResponseMessage response = await organiser.DeleteAsync("/api/categories/" + categoryId);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}