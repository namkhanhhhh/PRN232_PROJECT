using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace ProductManagementWebClient.Helpers
{
    public class ApiHelper
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _baseUrl;

        public ApiHelper(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpContextAccessor = httpContextAccessor;
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7009";

            Console.WriteLine($"ApiHelper initialized with base URL: {_baseUrl}");
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                AddAuthorizationHeader();
                var fullUrl = $"{_baseUrl}/{endpoint}";
                Console.WriteLine($"GET Request to: {fullUrl}");

                var response = await _httpClient.GetAsync(fullUrl);
                Console.WriteLine($"Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response Content: {content}");

                    if (string.IsNullOrEmpty(content))
                    {
                        Console.WriteLine("Response content is empty but status is success");
                        return default;
                    }

                    return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error Response: {errorContent}");
                    throw new HttpRequestException($"API call failed with status {response.StatusCode}: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                AddAuthorizationHeader();
                var fullUrl = $"{_baseUrl}/{endpoint}";
                Console.WriteLine($"POST Request to: {fullUrl}");

                var json = JsonSerializer.Serialize(data);
                Console.WriteLine($"Request Body: {json}");
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(fullUrl, content);
                Console.WriteLine($"Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response Content: {responseContent}");

                    if (string.IsNullOrEmpty(responseContent))
                    {
                        Console.WriteLine("Response content is empty but status is success");
                        return default;
                    }

                    return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error Response: {errorContent}");

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new UnauthorizedAccessException("JWT token is invalid or expired");
                    }

                    throw new HttpRequestException($"API call failed with status {response.StatusCode}: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in PostAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<T?> PutAsync<T>(string endpoint, object data)
        {
            try
            {
                AddAuthorizationHeader();
                var fullUrl = $"{_baseUrl}/{endpoint}";
                Console.WriteLine($"PUT Request to: {fullUrl}");

                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(fullUrl, content);
                Console.WriteLine($"Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response Content: {responseContent}");

                    if (string.IsNullOrEmpty(responseContent))
                    {
                        return default;
                    }

                    return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error Response: {errorContent}");
                    throw new HttpRequestException($"API call failed with status {response.StatusCode}: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in PutAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                AddAuthorizationHeader();
                var fullUrl = $"{_baseUrl}/{endpoint}";
                Console.WriteLine($"DELETE Request to: {fullUrl}");

                var response = await _httpClient.DeleteAsync(fullUrl);
                Console.WriteLine($"Response Status: {response.StatusCode}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in DeleteAsync: {ex.Message}");
                throw;
            }
        }

        private void AddAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("Token");

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                Console.WriteLine($"JWT Token set: {token.Substring(0, Math.Min(50, token.Length))}...");
            }
            else
            {
                Console.WriteLine("No JWT Token found in session with key 'Token'!");

                // Try alternative key names
                var jwtToken = _httpContextAccessor.HttpContext?.Session.GetString("JWTToken");
                var accessToken = _httpContextAccessor.HttpContext?.Session.GetString("AccessToken");

                Console.WriteLine($"JWTToken key: {(string.IsNullOrEmpty(jwtToken) ? "Empty" : "Has value")}");
                Console.WriteLine($"AccessToken key: {(string.IsNullOrEmpty(accessToken) ? "Empty" : "Has value")}");

                // List all session keys for debugging
                var sessionKeys = new List<string>();
                foreach (var key in _httpContextAccessor.HttpContext?.Session.Keys ?? new List<string>())
                {
                    sessionKeys.Add(key);
                }
                Console.WriteLine($"Available session keys: {string.Join(", ", sessionKeys)}");
            }
        }

        public void SetAuthorizationHeader(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                Console.WriteLine($"Authorization header set manually: {token.Substring(0, Math.Min(50, token.Length))}...");
            }
        }

        public void ClearAuthorizationHeader()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            Console.WriteLine("Authorization header cleared");
        }

        // Add method to get HttpClient for multipart requests
        public HttpClient GetHttpClient()
        {
            AddAuthorizationHeader();
            return _httpClient;
        }
    }
}
