using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace RandomPayMCSD.Services
{
    public sealed class UsuarioApiService : ApiClientBase
    {
        public UsuarioApiService(HttpClient httpClient, IHttpContextAccessor accessor) : base(httpClient, accessor)
        {
        }

        public Task RegisterAsync(string nombre, string email, string password)
        {
            return SendWithoutResultAsync(HttpMethod.Post, "/apiRandomPay/Users/Register", new
            {
                nombre,
                email,
                password
            });
        }

        public Task ForgotPasswordAsync(string email)
        {
            return SendWithoutResultAsync(HttpMethod.Post, "/apiRandomPay/Users/ForgotPassword", new
            {
                email
            });
        }

        public Task ResetPasswordAsync(string email, string token, string newPassword)
        {
            return SendWithoutResultAsync(HttpMethod.Post, "/apiRandomPay/Users/ResetPassword", new
            {
                email,
                token,
                newPassword
            });
        }

        private async Task SendWithoutResultAsync(HttpMethod method, string endpoint, object payload)
        {
            using var request = new HttpRequestMessage(method, endpoint)
            {
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response;
            try
            {
                response = await HttpClient.SendAsync(request);
            }
            catch (HttpRequestException)
            {
                throw new InvalidOperationException("No se pudo conectar con la API. Verifica ApiSettings:BaseUrl.");
            }
            catch (TaskCanceledException)
            {
                throw new InvalidOperationException("La llamada a la API excedió el tiempo de espera.");
            }

            using (response)
            {
                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    throw new InvalidOperationException(string.IsNullOrWhiteSpace(error)
                        ? $"Error API {(int)response.StatusCode} ({response.StatusCode})"
                        : error);
                }
            }
        }
    }
}
