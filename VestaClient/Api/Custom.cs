using System;
using System.Threading.Tasks;
using VestaClient.Api.Exceptions;
using VestaClient.Api.Responses;

namespace VestaClient.Api
{
    public class Custom
    {
        private VestaApi Api { get; }
        
        private string BaseAddress { get; }

        public Custom(VestaApi api, string baseAddress)
        {
            Api = api;
            BaseAddress = baseAddress;
        }

        public async Task Post(string url, object value, bool shouldLogin = true, object options = null)
        {
            ValidateUrl(url);

            await Api.PostAsync(url, value, shouldLogin, options);
        }

        public async Task<T> Post<T>(string url, object value, bool shouldLogin = true, object options = null)
        {
            ValidateUrl(url);

            return await Api.PostAsync<T>(url, value, shouldLogin, options);
        }

        public async Task Delete(string url, object options = null, bool shouldLogin = true)
        {
            ValidateUrl(url);

            await Api.DeleteAsync(url, options, shouldLogin, false);
        }

        public async Task<T> Get<T>(string url, object options = null, bool shouldLogin = true)
        {
            ValidateUrl(url);

            return await Api.GetAsync<T>(url, options, shouldLogin);
        }

        private void ValidateUrl(string url)
        {
            if (new Uri(url).Host != new Uri(BaseAddress).Host)
            {
                throw new VestaException("Host is different from base address.");
            }
        }
    }
}