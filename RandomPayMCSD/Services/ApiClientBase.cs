using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;

namespace RandomPayMCSD.Services
{
    public abstract class ApiClientBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly HttpClient HttpClient;

        protected ApiClientBase(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            HttpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        protected async Task<T?> GetAsync<T>(string url)
        {
            AddBearerToken();
            return await HttpClient.GetFromJsonAsync<T>(url);
        }

        protected async Task PostAsync<T>(string url, T payload)
        {
            AddBearerToken();
            using var response = await HttpClient.PostAsJsonAsync(url, payload);
            response.EnsureSuccessStatusCode();
        }

        protected async Task<TResult?> PostAsync<T, TResult>(string url, T payload)
        {
            AddBearerToken();
            using var response = await HttpClient.PostAsJsonAsync(url, payload);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResult>();
        }

        protected async Task PutAsync<T>(string url, T payload)
        {
            AddBearerToken();
            using var response = await HttpClient.PutAsJsonAsync(url, payload);
            response.EnsureSuccessStatusCode();
        }

        protected async Task DeleteAsync(string url)
        {
            AddBearerToken();
            using var response = await HttpClient.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
        }

        private void AddBearerToken()
        {
            string? token = _httpContextAccessor.HttpContext?.Session.GetString("JWT_TOKEN");
            if (string.IsNullOrWhiteSpace(token))
            {
                HttpClient.DefaultRequestHeaders.Authorization = null;
                return;
            }

            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
