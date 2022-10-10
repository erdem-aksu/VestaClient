using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MimeMapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using VestaClient.Api;
using VestaClient.Api.Exceptions;
using VestaClient.Api.Session;
using VestaClient.Api.SessionHandlers;
using VestaClient.Serialization;
using VestaClient.Extensions;

namespace VestaClient.Http
{
    internal class VestaHttpClient
    {
        private UserSessionData _user;

        private readonly StateData _stateData;

        private readonly bool _autoLogin;

        private readonly ISessionHandler _sessionHandler;

        private readonly CookieContainer _cookieContainer;

        private readonly Uri _baseAddress;

        private static readonly IDictionary<string, string> RequestHeaders = new Dictionary<string, string>()
        {
            {"User-Agent", VestaApiConstants.HeaderUserAgent},
            {"Accept", "application/json, text/javascript, */*; q=0.01"},
            {"Accept-Language", "en-US,en;q=0.5"},
            {"X-Requested-With", "XMLHttpRequest"}
        };

        private static readonly DefaultContractResolver ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        };

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            ContractResolver = ContractResolver,
        };

        public VestaHttpClient(string uri, UserSessionData user, bool autoLogin = false,
            string stateDataPath = null)
        {
            if (!uri.EndsWith("/"))
            {
                uri += "/";
            }

            _cookieContainer = new CookieContainer();
            _baseAddress = new Uri(uri);

            _user = user;
            _stateData = new StateData
            {
                User = _user
            };

            if (autoLogin && !string.IsNullOrEmpty(stateDataPath) && Directory.Exists(stateDataPath))
            {
                _autoLogin = true;
                _sessionHandler = new FileSessionHandler(this,
                    Path.Combine(stateDataPath, _baseAddress.Host.ToFileName() + ".bin"));
            }
        }

        public async Task<HttpResponseMessage> GetAsync(string requestUri, object options = null,
            bool shouldLogin = true)
        {
            if (shouldLogin)
            {
                await Login();
            }

            requestUri = BuildPath(requestUri, options);

            var message = new HttpRequestMessage(HttpMethod.Get, requestUri);

            return await SendAsync(message);
        }

        public async Task<HttpResponseMessage> PostAsync(string requestUri, object value, object options = null,
            bool shouldLogin = true)
        {
            if (shouldLogin)
            {
                await Login();
            }

            requestUri = BuildPath(requestUri, options);

            var message = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = GetFormUrlEncodedContent(value),
            };

            return await SendAsync(message);
        }

        public async Task<HttpResponseMessage> PutAsync(string requestUri, object value, object options = null,
            bool shouldLogin = true)
        {
            if (shouldLogin)
            {
                await Login();
            }

            requestUri = BuildPath(requestUri, options);

            var message = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = GetStringContent(value),
            };

            message.Headers.Add("X-HTTP-Method-Override", "PUT");

            return await SendAsync(message);
        }

        public async Task<HttpResponseMessage> UploadAsync(string requestUri, byte[] bytes, string fileName,
            string name = "file", bool shouldLogin = true, bool isApi = true, object formData = null)
        {
            if (shouldLogin)
            {
                await Login();
            }

            requestUri = BuildPath(requestUri, null);

            var message = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = GetUploadContent(bytes, fileName, name, formData)
            };

            return await SendAsync(message);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string requestUri, object options = null,
            bool shouldLogin = true, bool isApi = true)
        {
            if (shouldLogin)
            {
                await Login();
            }

            requestUri = BuildPath(requestUri, options);

            var message = new HttpRequestMessage(HttpMethod.Delete, requestUri);

            return await SendAsync(message);
        }

        public async Task Login()
        {
            if (_autoLogin)
            {
                _sessionHandler.Load();
            }

            if (_stateData.IsLoggedIn) return;

            if (string.IsNullOrEmpty(_user.Username) || string.IsNullOrEmpty(_user.Password))
            {
                throw new ArgumentNullException(_user.Username);
            }

            var requestUri = BuildPath(VestaApiConstants.PathLogin);

            var csrfToken = await GetCsrfToken(requestUri, false);

            var message = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = GetFormUrlEncodedContent(new
                {
                    token = csrfToken,
                    user = _user.Username,
                    password = _user.Password
                })
            };

            using (var loginRes = await SendAsync(message))
            {
                var sessionCookie = _cookieContainer.GetCookies(loginRes.RequestMessage.RequestUri)
                    .Cast<Cookie>()
                    .FirstOrDefault(cookie => cookie.Name.StartsWith(VestaApiConstants.SessionCookieField));

                if (!(loginRes.IsSuccessStatusCode || loginRes.StatusCode == HttpStatusCode.Redirect) ||
                    sessionCookie == null)
                {
                    throw new VestaAuthorizationException("Invalid credentials.");
                }
            }

            _stateData.IsLoggedIn = true;

            if (_autoLogin)
            {
                _sessionHandler.Save();
            }
        }

        public void Logout()
        {
            _stateData.CsrfToken = string.Empty;
            _stateData.IsLoggedIn = false;

            foreach (Cookie co in _cookieContainer.GetCookies(_baseAddress))
            {
                co.Expires = DateTime.Now.Subtract(TimeSpan.FromDays(1));
            }

            _stateData.Cookies = _cookieContainer;
            _stateData.RawCookies = _cookieContainer.GetCookies(_baseAddress).Cast<Cookie>().ToList();

            if (_autoLogin)
            {
                _sessionHandler.Save();
            }
        }

        public Stream GetStateDataAsStream()
        {
            return SerializationHelper.SerializeToStream(_stateData);
        }

        public void LoadStateDataFromStream(Stream stream)
        {
            var data = SerializationHelper.DeserializeFromStream<StateData>(stream);

            if (_user.Username != data.User.Username || _user.Password != data.User.Password)
            {
                return;
            }

            _user = data.User;

            foreach (var cookie in data.RawCookies)
            {
                _cookieContainer.Add(_baseAddress, cookie);
            }

            _stateData.IsLoggedIn = data.IsLoggedIn;
            _stateData.CsrfToken = data.CsrfToken;
        }

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage message)
        {
            using (var handler = new HttpClientHandler
            {
                CookieContainer = _cookieContainer,
                AutomaticDecompression = DecompressionMethods.GZip,
                AllowAutoRedirect = false,
                UseCookies = true,
                ServerCertificateCustomValidationCallback = (requestMessage, certificate2, arg3, arg4) => true
            })
            using (var client = new HttpClient(handler) {BaseAddress = _baseAddress})
            {
                SetHeaders(message);

                var response = await client.SendAsync(message);

                if (response.Headers.Contains("Set-Cookie"))
                {
                    var cookieHeaders = response.Headers.GetValues("Set-Cookie").ToList();

                    foreach (var header in cookieHeaders)
                    {
                        handler.CookieContainer.SetCookies(_baseAddress, header);
                    }

                    _stateData.Cookies = handler.CookieContainer;
                    _stateData.RawCookies = _stateData.Cookies.GetCookies(_baseAddress)
                        .Cast<Cookie>()
                        .ToList();
                }

                return response;
            }
        }

        public async Task<string> GetCsrfToken(string uri, bool shouldLogin = true)
        {
            var response = await GetAsync(uri, null, shouldLogin);

            var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var matches = Regex.Matches(html,
                "<input type=\"hidden\" name=\"token\" value=\"((.(?<!(\"\\}|\"\\,)))*)\"");

            if (matches.Count > 0)
            {
                return matches[0].Groups[1].Value;
            }

            throw new Exception("Csrf token not found.");
        }

        public static FormUrlEncodedContent GetFormUrlEncodedContent(object obj)
        {
            var data = obj.ToKeyValueString();

            return new FormUrlEncodedContent(data);
        }

        private static StringContent GetStringContent(object obj)
        {
            return new StringContent(JsonConvert.SerializeObject(obj, JsonSerializerSettings), Encoding.UTF8,
                "application/json");
        }

        private static string BuildPath(string basePath, object options = null)
        {
            if (options == null) return basePath;

            var optionsPairs = options.ToKeyValueString();

            return optionsPairs.Aggregate(basePath, (current, pair) => current.AddQueryParam(pair.Key, pair.Value));
        }

        private static MultipartFormDataContent GetUploadContent(byte[] bytes, string fileName, string name,
            object formData = null)
        {
            var content = new ByteArrayContent(bytes);
            content.Headers.ContentType = new MediaTypeHeaderValue(MimeUtility.GetMimeMapping(fileName));

            var multipartFormData = new MultipartFormDataContent {{content, name, fileName}};

            if (formData != null)
            {
                foreach (var property in formData.GetType().GetProperties())
                {
                    multipartFormData.Add(new StringContent(property.GetValue(formData).ToString()), property.Name);
                }
            }

            return multipartFormData;
        }

        private static StreamContent GetStreamUploadContent(byte[] bytes, string fileName, string name)
        {
            var content = new StreamContent(new MemoryStream(bytes));
            content.Headers.ContentType = new MediaTypeHeaderValue(MimeUtility.GetMimeMapping(fileName));
            content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };

            return content;
        }

        private void SetHeaders(HttpRequestMessage request)
        {
            foreach (var header in RequestHeaders)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            request.Headers.Add("Referer", _baseAddress.Host);

            if (!string.IsNullOrEmpty(_stateData.CsrfToken))
            {
                request.Headers.Add("X-WP-Nonce", _stateData.CsrfToken);
            }
        }
    }
}