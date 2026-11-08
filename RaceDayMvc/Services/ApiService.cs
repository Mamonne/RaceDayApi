using System.Text;
using System.Text.Json;

namespace RaceDayMvc.Services
{
    // The result of one call to the API
    public class ApiResponse
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Body { get; set; } = "";

        // A friendly message that is safe to show to the user
        public string Message { get; set; } = "";

        // Turns the JSON reply from the API into a C# object
        public T? ReadAs<T>()
        {
            try
            {
                return JsonSerializer.Deserialize<T>(this.Body, ApiService.JsonOptions);
            }
            catch (Exception)
            {
                return default;
            }
        }
    }

    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            this._httpClient = httpClient;
            this._httpContextAccessor = httpContextAccessor;
        }

        public Task<ApiResponse> GetAsync(string url)
        {
            return this.SendAsync(HttpMethod.Get, url, null);
        }

        public Task<ApiResponse> PostAsync(string url, object? body)
        {
            return this.SendAsync(HttpMethod.Post, url, body);
        }

        public Task<ApiResponse> PutAsync(string url, object? body)
        {
            return this.SendAsync(HttpMethod.Put, url, body);
        }

        public Task<ApiResponse> DeleteAsync(string url)
        {
            return this.SendAsync(HttpMethod.Delete, url, null);
        }

        private ISession GetSession()
        {
            return this._httpContextAccessor.HttpContext!.Session;
        }

        private async Task<ApiResponse> SendAsync(HttpMethod method, string url, object? body)
        {
            ApiResponse result = new ApiResponse();

            try
            {
                HttpRequestMessage request = new HttpRequestMessage(method, url);

                if (body != null)
                {
                    string json = JsonSerializer.Serialize(body);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                // Send the saved API session cookie, so the API knows who is logged in
                string? cookie = this.GetSession().GetString("ApiCookie");
                if (string.IsNullOrEmpty(cookie) == false)
                {
                    request.Headers.Add("Cookie", cookie);
                }

                HttpResponseMessage response = await this._httpClient.SendAsync(request);

                result.StatusCode = (int)response.StatusCode;
                result.Success = response.IsSuccessStatusCode;
                result.Body = await response.Content.ReadAsStringAsync();

                // If the API sent back a session cookie, remember it for this user
                if (response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? setCookies))
                {
                    foreach (string setCookie in setCookies)
                    {
                        if (setCookie.StartsWith(".AspNetCore.Session="))
                        {
                            string cookieValue = setCookie.Split(';')[0];
                            this.GetSession().SetString("ApiCookie", cookieValue);
                        }
                    }
                }

                result.Message = this.GetFriendlyMessage(result);
            }
            catch (Exception)
            {
                result.Success = false;
                result.StatusCode = 0;
                result.Message = "The server could not be reached. Please try again later.";
            }

            return result;
        }

        // Never show raw error text to the user
        private string GetFriendlyMessage(ApiResponse result)
        {
            if (result.Success)
            {
                return "";
            }

            if (result.StatusCode == 400)
            {
                // Our API sends short plain-text reasons for 400 errors, such as "That email is already registered."
                // A JSON reply means a badly formed request, so we show a general message instead.
                bool isPlainText = result.Body.Length > 0 && result.Body.StartsWith("{") == false;

                if (isPlainText)
                {
                    return result.Body;
                }

                return "Please check the details you entered and try again.";
            }

            if (result.StatusCode == 401)
            {
                return "Please log in to continue.";
            }

            if (result.StatusCode == 403)
            {
                return "You do not have permission to do that.";
            }

            if (result.StatusCode == 404)
            {
                return "We could not find what you were looking for.";
            }

            return "Something went wrong. Please try again.";
        }
    }
}