/*using System.Text;
using System.Text.Json;
using Vinov;
using Vinov.Models;

namespace sd
{
    public record ApiSonuc<T>()
    {
        public T? Result { get; set; }
        public bool Succeded { get; set; }
        public Data? Data { get; set; }
        public string? ErrorMessage { get; set; }
    };


    public class VakifBankApi
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly string token;

        public VakifBankApi()
        {
            _config = VinovFonksiyonlar.Ayarlar;
            _httpClient = new HttpClient();
            token = GetToken().Result;
        }

        public async Task<string> GetToken()
        {
            var tokenUrl = _config["Ayarlar:ApiAdresi"] + "/auth/oauth/v2/token";
            var client_id = _config["Ayarlar:client_id"];
            var client_secret = _config["Ayarlar:client_secret"];
            var grant_type = _config["Ayarlar:grant_type"];
            var scope = _config["Ayarlar:scope"];

            var data = new
            {
                client_id,
                client_secret,
                grant_type,
                scope
            };

            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Get, tokenUrl)
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API request failed with status code: {response.StatusCode}, Content: {responseContent}");
            }

            response.EnsureSuccessStatusCode();
            var tokenCevap = await JsonSerializer.DeserializeAsync<TokenCevap>(await response.Content.ReadAsStreamAsync());

            return tokenCevap.access_token;
        }

        public ApiSonuc<T> ApiIstek<T>(HttpMethod method, string EndPoint, string Body)
        {
            try
            {
                var apiUrl = _config["Ayarlar:ApiAdresi"] + EndPoint;

                var request = new HttpRequestMessage(method, apiUrl);
                request.Headers.Add("Authorization", "Bearer " + token);

                var content = new StringContent(Body, Encoding.UTF8, "application/json");
                request.Content = content;

                var response = _httpClient.SendAsync(request).Result;

                if (!response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    responseContent = VinovFonksiyonlar.SondakiVirguluSil(responseContent);

                    HataCevap hataCevap = JsonSerializer.Deserialize<HataCevap>(responseContent);

                    return new ApiSonuc<T>
                    {
                        Succeded = false,
                        ErrorMessage = $"{hataCevap.Header.StatusCode} <br/> {hataCevap.Header.StatusDescription}"
                    };
                }

                var json = response.Content.ReadAsStringAsync().Result;
                var result = JsonSerializer.Deserialize<T>(json);

                //check type of result is HataCevap


                if (result.GetType().GetProperty("Header") != null)
                {
                    var header = result.GetType().GetProperty("Header").GetValue(result, null);
                    var statusCode = header.GetType().GetProperty("StatusCode").GetValue(header, null);

                    if (statusCode.ToString() != "APIGW000000")
                    {
                        var statusDescription = header.GetType().GetProperty("StatusDescription").GetValue(header, null);

                        return new ApiSonuc<T>
                        {
                            Succeded = false,
                            ErrorMessage = statusDescription.ToString()
                        };
                    }
                }

       
                //if (result.GetType().GetProperty("Data") != null)
                //{
                //    data = (Data)result.GetType().GetProperty("Data").GetValue(result, null);

                //    var resultData = data.GetType().GetProperty("Result").GetValue(data, null);

                //    if (resultData.GetType().GetProperty("IsSucceeded").GetValue(resultData, null).ToString() == "False")
                //    {
                //        var errorMessage = resultData.GetType().GetProperty("ErrorMessage").GetValue(resultData, null);

                //        return new ApiSonuc<T>
                //        {
                //            Succeded = false,
                //            ErrorMessage = errorMessage.ToString()
                //        };
                //    }
                //}

                return new ApiSonuc<T>
                {
                    Result = result,
                    Succeded = true
                };
            }
            catch (Exception ex)
            {
                return new ApiSonuc<T>
                {
                    Succeded = false,
                    ErrorMessage = ex.Message
                };
            }
        }

    }
}*/

using System.Text;
using System.Text.Json;
using Vinov;
using Vinov.Models;
using System.Reflection;

namespace sd
{
    public record ApiSonuc<T>()
    {
        public T? Result { get; set; }
        public bool Succeded { get; set; }
        public Data? Data { get; set; }
        public string? ErrorMessage { get; set; }
    };

    public class VakifBankApi
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private string? _token;
        private DateTime _tokenFetchedAt;
        private readonly TimeSpan _tokenLifetime = TimeSpan.FromMinutes(55);

        public VakifBankApi()
        {
            _config = VinovFonksiyonlar.Ayarlar;
            _httpClient = new HttpClient();
        }

        private async Task<string> GetTokenCachedAsync()
        {
            if (_token != null && (DateTime.Now - _tokenFetchedAt) < _tokenLifetime)
            {
                return _token;
            }

            var tokenUrl = _config["Ayarlar:ApiAdresi"] + "/auth/oauth/v2/token";
            var data = new
            {
                client_id = _config["Ayarlar:client_id"],
                client_secret = _config["Ayarlar:client_secret"],
                grant_type = _config["Ayarlar:grant_type"],
                scope = _config["Ayarlar:scope"]
            };

            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Get, tokenUrl)
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API request failed with status code: {response.StatusCode}, Content: {responseContent}");
            }

            var tokenCevap = await JsonSerializer.DeserializeAsync<TokenCevap>(await response.Content.ReadAsStreamAsync());
            _token = tokenCevap.access_token;
            _tokenFetchedAt = DateTime.Now;
            return _token;
        }

        public async Task<ApiSonuc<T>> ApiIstekAsync<T>(HttpMethod method, string EndPoint, string Body)
        {
            try
            {
                var token = await GetTokenCachedAsync();
                var apiUrl = _config["Ayarlar:ApiAdresi"] + EndPoint;

                var request = new HttpRequestMessage(method, apiUrl);
                request.Headers.Add("Authorization", "Bearer " + token);

                var content = new StringContent(Body, Encoding.UTF8, "application/json");
                request.Content = content;

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    responseContent = VinovFonksiyonlar.SondakiVirguluSil(responseContent);

                    var hataCevap = JsonSerializer.Deserialize<HataCevap>(responseContent);

                    return new ApiSonuc<T>
                    {
                        Succeded = false,
                        ErrorMessage = $"{hataCevap.Header.StatusCode} <br/> {hataCevap.Header.StatusDescription}"
                    };
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<T>(json);

                if (result?.GetType().GetProperty("Header") is PropertyInfo headerProp)
                {
                    var header = headerProp.GetValue(result);
                    var statusCode = header?.GetType().GetProperty("StatusCode")?.GetValue(header);

                    if (statusCode?.ToString() != "APIGW000000")
                    {
                        var statusDescription = header?.GetType().GetProperty("StatusDescription")?.GetValue(header);
                        return new ApiSonuc<T>
                        {
                            Succeded = false,
                            ErrorMessage = statusDescription?.ToString()
                        };
                    }
                }

                return new ApiSonuc<T>
                {
                    Result = result,
                    Succeded = true
                };
            }
            catch (Exception ex)
            {
                return new ApiSonuc<T>
                {
                    Succeded = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public ApiSonuc<T> ApiIstek<T>(HttpMethod method, string EndPoint, string Body)
        {
            return ApiIstekAsync<T>(method, EndPoint, Body).GetAwaiter().GetResult();
        }
    }
}
