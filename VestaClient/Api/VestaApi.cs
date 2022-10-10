using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using VestaClient.Api.Exceptions;
using VestaClient.Api.Responses;
using VestaClient.Api.Session;
using VestaClient.Http;
using VestaClient.Utility;

namespace VestaClient.Api
{
    public class VestaApi
    {
        private readonly bool _autoLogin;

        private static readonly DefaultContractResolver ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        };

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            ContractResolver = ContractResolver
        };

        private VestaHttpClient Client { get; }

        private UserSessionData User { get; }

        public VestaApi(string uri, string username, string password, bool autoLogin = false,
            string sessionDataPath = null)
        {
            _autoLogin = autoLogin;

            User = new UserSessionData()
            {
                Username = username,
                Password = password
            };

            Client = new VestaHttpClient(uri, User, _autoLogin, sessionDataPath);
        }


        public async Task<T> GetAsync<T>(string path, object options = null, bool shouldLogin = true)
        {
            using (var response = await Client.GetAsync(path, options, shouldLogin)
                .ConfigureAwait(false))
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return default;
                }

                if (!response.IsSuccessStatusCode)
                {
                    await ValidateResponse(response).ConfigureAwait(false);
                }

                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var res = JsonConvert.DeserializeObject<T>(content, JsonSerializerSettings);

                return res;
            }
        }

        public async Task<string> GetStringAsync(string path, object options = null, bool shouldLogin = true)
        {
            using (var response = await Client.GetAsync(path, options, shouldLogin)
                .ConfigureAwait(false))
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return default;
                }

                if (!response.IsSuccessStatusCode)
                {
                    await ValidateResponse(response).ConfigureAwait(false);
                }

                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                return content;
            }
        }

        public async Task PostAsync(string path, object value, bool shouldLogin = true, object options = null)
        {
            await PostAsyncInternal(path, value, shouldLogin, options).ConfigureAwait(false);
        }

        public async Task<T> PostAsync<T>(string path, object value, bool shouldLogin = true,
            object options = null)
        {
            var content = await PostAsyncInternal(path, value, shouldLogin, options)
                .ConfigureAwait(false);

            var res = JsonConvert.DeserializeObject<T>(content, JsonSerializerSettings);

            return res;
        }

        public async Task<T> PutAsync<T>(string path, object value, bool shouldLogin = true,
            object options = null)
        {
            using (var response = await Client.PutAsync(path, value, options, shouldLogin)
                .ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode)
                {
                    await ValidateResponse(response).ConfigureAwait(false);
                }

                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var res = JsonConvert.DeserializeObject<T>(content, JsonSerializerSettings);

                return res;
            }
        }

        private async Task<string> PostAsyncInternal(string path, object value, bool shouldLogin = true,
            object options = null)
        {
            using (var response = await Client.PostAsync(path, value, options, shouldLogin)
                .ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode)
                {
                    await ValidateResponse(response).ConfigureAwait(false);
                }

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        public async Task<string> UploadAsync(string path, byte[] bytes, string filename, string name = "file",
            object formData = null)
        {
            using (var response = await Client.UploadAsync(path, bytes, filename, name, true, true, formData)
                .ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode)
                {
                    await ValidateResponse(response).ConfigureAwait(false);
                }

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        public async Task<T> UploadAsync<T>(string path, byte[] bytes, string filename, string name = "file",
            object formData = null)
        {
            var content = await UploadAsync(path, bytes, filename, name, formData).ConfigureAwait(false);

            return JsonConvert.DeserializeObject<T>(content, JsonSerializerSettings);
        }

        public async Task DeleteAsync(string path, object options = null, bool shouldLogin = true,
            bool isApi = true)
        {
            using (var response = await Client.DeleteAsync(path, options, shouldLogin, isApi).ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode)
                {
                    await ValidateResponse(response).ConfigureAwait(false);
                }
            }
        }

        public async Task Login()
        {
            await Client.Login().ConfigureAwait(false);
        }

        public async Task<string> GetCsrfToken(string uri, bool shouldLogin = true)
        {
            return await Client.GetCsrfToken(uri, shouldLogin);
        }

        public void Logout()
        {
            Client.Logout();
        }

        private async Task ValidateResponse(HttpResponseMessage response)
        {
            if (_autoLogin && (response.StatusCode == HttpStatusCode.Unauthorized ||
                               response.StatusCode == HttpStatusCode.Forbidden))
            {
                Client.Logout();
            }

            throw await CreateException(response).ConfigureAwait(false);
        }

        private static async Task<Exception> CreateException(HttpResponseMessage response)
        {
            var url = response.RequestMessage.RequestUri.ToString();
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            ErrorResponse jsonError = null;
            try
            {
                jsonError = JsonConvert.DeserializeObject<ErrorResponse>(content, JsonSerializerSettings);
            }
            catch (Exception)
            {
                // ignored
            }

            var message = jsonError?.Message ?? response.ReasonPhrase;
            var status = (int) response.StatusCode;
            switch (status)
            {
                case 400:
                    return VestaException.Create<VestaBadRequestException>(message, url, content, jsonError);
                case 401:
                    return VestaException.Create<VestaAuthorizationException>(message, url, content, jsonError);
                case 403:
                    return VestaException.Create<VestaForbiddenException>(message, url, content, jsonError);
                case 404:
                    return VestaException.Create<VestaNotFoundException>(message, url, content, jsonError);
                case 408:
                    return VestaException.Create<VestaTimeoutException>(message, url, content, jsonError);
                case 429:
                    return VestaException.Create<VestaRateLimitExceededException>(message, url, content,
                        jsonError);
                case 500:
                case 502:
                case 599:
                    return VestaException.Create<VestaServerErrorException>(message, url, content, jsonError,
                        status);
                default:
                    return new VestaException(message)
                    {
                        RequestUrl = url,
                        ResponseContent = content,
                        ErrorResponse = jsonError,
                        HttpStatusCode = status
                    };
            }
        }
    }
}