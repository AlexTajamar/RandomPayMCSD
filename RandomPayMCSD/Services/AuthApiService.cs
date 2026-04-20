using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using RandomPayMCSD.Models;

namespace RandomPayMCSD.Services
{
    public sealed class AuthApiService : ApiClientBase
    {
        public AuthApiService(HttpClient httpClient, IHttpContextAccessor accessor) : base(httpClient, accessor)
        {
        }

        public async Task<(Usuario? usuario, string? token)> LoginAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return (null, null);
            }

            return await ExecuteLoginAsync(email.Trim(), password);
        }

        private async Task<(Usuario? usuario, string? token)> ExecuteLoginAsync(string email, string password)
        {
            try
            {
                using HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/apiRandomPay/Auth/Login", new
                {
                    email,
                    password
                });

                if (!response.IsSuccessStatusCode)
                {
                    return (null, null);
                }

                string raw = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(raw))
                {
                    return (null, null);
                }

                string? token = null;
                Usuario? usuario = null;

                string trimmed = raw.Trim();
                if (trimmed.StartsWith("{") || trimmed.StartsWith("["))
                {
                    using JsonDocument doc = JsonDocument.Parse(trimmed);
                    JsonElement root = doc.RootElement;

                    token = ReadToken(root);
                    JsonElement userNode = ReadUserNode(root);

                    usuario = new Usuario
                    {
                        IDUSUARIO = ReadInt(userNode, "idUsuario", "IDUSUARIO", "id") ?? 0,
                        NOMBRE = ReadString(userNode, "nombre", "NOMBRE", "name") ?? string.Empty,
                        EMAIL = ReadString(userNode, "email", "EMAIL") ?? email,
                        ROL = ReadString(userNode, "rol", "ROL", ClaimTypes.Role) ?? "USER"
                    };
                }
                else
                {
                    token = trimmed.Trim('"');
                }

                if (!string.IsNullOrWhiteSpace(token))
                {
                    usuario = CompleteUserFromJwt(token, usuario, email);
                }

                if (usuario == null)
                {
                    return (null, null);
                }

                if (usuario.IDUSUARIO <= 0 && string.IsNullOrWhiteSpace(token))
                {
                    return (null, null);
                }

                usuario.NOMBRE = string.IsNullOrWhiteSpace(usuario.NOMBRE) ? usuario.EMAIL : usuario.NOMBRE;
                usuario.ROL = string.IsNullOrWhiteSpace(usuario.ROL) ? "USER" : usuario.ROL;

                return (usuario, token);
            }
            catch (HttpRequestException)
            {
                return (null, null);
            }
            catch (TaskCanceledException)
            {
                return (null, null);
            }
            catch
            {
                return (null, null);
            }
        }

        private static string? ReadToken(JsonElement root)
        {
            return ReadString(root, "token", "Token", "jwt", "Jwt", "accessToken", "AccessToken", "response")
                ?? TryReadNestedString(root, "data", "token")
                ?? TryReadNestedString(root, "resultado", "token")
                ?? TryReadNestedString(root, "result", "token");
        }

        private static JsonElement ReadUserNode(JsonElement root)
        {
            if (TryGetProperty(root, out JsonElement value, "usuario", "Usuario", "user", "User"))
            {
                return value;
            }

            if (TryGetProperty(root, out JsonElement data, "data", "Data", "resultado", "Resultado", "result", "Result")
                && data.ValueKind == JsonValueKind.Object)
            {
                if (TryGetProperty(data, out JsonElement nestedUser, "usuario", "Usuario", "user", "User"))
                {
                    return nestedUser;
                }

                return data;
            }

            return root;
        }

        private static Usuario CompleteUserFromJwt(string token, Usuario? current, string fallbackEmail)
        {
            Usuario usuario = current ?? new Usuario();

            try
            {
                JsonElement payload = ReadJwtPayload(token);

                if (usuario.IDUSUARIO <= 0)
                {
                    string? idClaim = ReadString(payload, ClaimTypes.NameIdentifier, "nameid", "sub");
                    if (int.TryParse(idClaim, out int parsedId))
                    {
                        usuario.IDUSUARIO = parsedId;
                    }
                }

                if (string.IsNullOrWhiteSpace(usuario.NOMBRE))
                {
                    usuario.NOMBRE = ReadString(payload, ClaimTypes.Name, "unique_name", "name") ?? string.Empty;
                }

                if (string.IsNullOrWhiteSpace(usuario.EMAIL))
                {
                    usuario.EMAIL = ReadString(payload, ClaimTypes.Email, "email") ?? fallbackEmail;
                }

                if (string.IsNullOrWhiteSpace(usuario.ROL))
                {
                    usuario.ROL = ReadString(payload, ClaimTypes.Role, "role") ?? "USER";
                }
            }
            catch
            {
                usuario.EMAIL = string.IsNullOrWhiteSpace(usuario.EMAIL) ? fallbackEmail : usuario.EMAIL;
                usuario.ROL = string.IsNullOrWhiteSpace(usuario.ROL) ? "USER" : usuario.ROL;
            }

            return usuario;
        }

        private static JsonElement ReadJwtPayload(string token)
        {
            string[] parts = token.Split('.');
            if (parts.Length < 2)
            {
                throw new InvalidOperationException("JWT inválido.");
            }

            string payloadPart = parts[1]
                .Replace('-', '+')
                .Replace('_', '/');

            switch (payloadPart.Length % 4)
            {
                case 2: payloadPart += "=="; break;
                case 3: payloadPart += "="; break;
            }

            byte[] bytes = Convert.FromBase64String(payloadPart);
            string json = Encoding.UTF8.GetString(bytes);

            using JsonDocument doc = JsonDocument.Parse(json);
            return doc.RootElement.Clone();
        }

        private static string? TryReadNestedString(JsonElement root, string parent, string child)
        {
            if (!TryGetProperty(root, out JsonElement parentNode, parent)) return null;
            if (parentNode.ValueKind != JsonValueKind.Object) return null;
            return ReadString(parentNode, child);
        }

        private static string? ReadString(JsonElement element, params string[] names)
        {
            if (!TryGetProperty(element, out JsonElement value, names)) return null;
            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString(),
                JsonValueKind.Number => value.ToString(),
                JsonValueKind.True => bool.TrueString,
                JsonValueKind.False => bool.FalseString,
                _ => null
            };
        }

        private static int? ReadInt(JsonElement element, params string[] names)
        {
            if (!TryGetProperty(element, out JsonElement value, names)) return null;
            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int intValue)) return intValue;
            if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out intValue)) return intValue;
            return null;
        }

        private static bool TryGetProperty(JsonElement element, out JsonElement value, params string[] names)
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                value = default;
                return false;
            }

            foreach (var property in element.EnumerateObject())
            {
                if (names.Any(n => string.Equals(property.Name, n, StringComparison.OrdinalIgnoreCase)))
                {
                    value = property.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }
    }
}
