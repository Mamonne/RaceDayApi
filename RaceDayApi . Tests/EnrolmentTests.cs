using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDayApi.Tests
{
    public class EnrolmentTests : TestBase
    {
        [Fact]
        public async Task Enrol_AsParticipant_Returns201()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            HttpClient participant = await this.CreateLoggedInClient("part@test.com", "Participant");
            int eventId = await this.CreateEvent(organiser);
            int categoryId = await this.CreateCategory(organiser, eventId);

            HttpResponseMessage response = await this.Enrol(participant, eventId, categoryId);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Enrol_IsRecordedAndVisibleToTheOrganiser()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            HttpClient participant = await this.CreateLoggedInClient("part@test.com", "Participant");
            int eventId = await this.CreateEvent(organiser);
            int categoryId = await this.CreateCategory(organiser, eventId);

            await this.Enrol(participant, eventId, categoryId);

            HttpResponseMessage response = await organiser.GetAsync("/api/events/" + eventId + "/enrolments");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JsonElement list = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(1, list.GetArrayLength());
            Assert.Equal(categoryId, list[0].GetProperty("categoryId").GetInt32());
        }

        [Fact]
        public async Task Enrol_WhenNotLoggedIn_Returns401()
        {
            HttpResponseMessage response = await this.Enrol(this._client, 1, 1);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Enrol_Twice_Returns400()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            HttpClient participant = await this.CreateLoggedInClient("part@test.com", "Participant");
            int eventId = await this.CreateEvent(organiser);
            int categoryId = await this.CreateCategory(organiser, eventId);

            await this.Enrol(participant, eventId, categoryId);
            HttpResponseMessage second = await this.Enrol(participant, eventId, categoryId);

            Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
        }

        [Fact]
        public async Task Enrol_WithInvalidCategory_Returns400()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            HttpClient participant = await this.CreateLoggedInClient("part@test.com", "Participant");
            int eventId = await this.CreateEvent(organiser);

            HttpResponseMessage response = await this.Enrol(participant, eventId, 9999);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Enrol_ForUnknownEvent_Returns404()
        {
            HttpClient participant = await this.CreateLoggedInClient("part@test.com", "Participant");

            HttpResponseMessage response = await this.Enrol(participant, 9999, 1);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Enrol_WhenCategoryIsFull_Returns400()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            HttpClient first = await this.CreateLoggedInClient("part1@test.com", "Participant");
            HttpClient second = await this.CreateLoggedInClient("part2@test.com", "Participant");
            int eventId = await this.CreateEvent(organiser);
            int categoryId = await this.CreateCategory(organiser, eventId, 1);

            HttpResponseMessage firstResponse = await this.Enrol(first, eventId, categoryId);
            Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

            HttpResponseMessage secondResponse = await this.Enrol(second, eventId, categoryId);
            Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);
        }

        [Fact]
        public async Task CancelEnrolment_AsOwner_Returns200()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            HttpClient participant = await this.CreateLoggedInClient("part@test.com", "Participant");
            int eventId = await this.CreateEvent(organiser);
            int categoryId = await this.CreateCategory(organiser, eventId);

            HttpResponseMessage enrolResponse = await this.Enrol(participant, eventId, categoryId);
            JsonElement json = await enrolResponse.Content.ReadFromJsonAsync<JsonElement>();
            int enrolmentId = json.GetProperty("enrolmentId").GetInt32();

            HttpResponseMessage response = await participant.DeleteAsync("/api/enrolments/" + enrolmentId);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CancelEnrolment_OfAnotherParticipant_Returns403()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            HttpClient owner = await this.CreateLoggedInClient("part1@test.com", "Participant");
            HttpClient other = await this.CreateLoggedInClient("part2@test.com", "Participant");
            int eventId = await this.CreateEvent(organiser);
            int categoryId = await this.CreateCategory(organiser, eventId);

            HttpResponseMessage enrolResponse = await this.Enrol(owner, eventId, categoryId);
            JsonElement json = await enrolResponse.Content.ReadFromJsonAsync<JsonElement>();
            int enrolmentId = json.GetProperty("enrolmentId").GetInt32();

            HttpResponseMessage response = await other.DeleteAsync("/api/enrolments/" + enrolmentId);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}