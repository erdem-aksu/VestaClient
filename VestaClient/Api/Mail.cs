using System;
using System.Threading.Tasks;
using VestaClient.Api.Exceptions;
using VestaClient.Api.Responses;

namespace VestaClient.Api
{
    public class Mail
    {
        private VestaApi Api { get; }

        public Mail(VestaApi api)
        {
            Api = api;
        }

        public async Task CreateMailDomain(string domain)
        {
            await Api.PostAsync(VestaApiConstants.PathAddMail, new
            {
                token = await Api.GetCsrfToken(VestaApiConstants.PathAddMail),
                ok = "Add",
                v_domain = domain,
                v_antispam = "on",
                v_antivirus = "on"
            });
        }

        public async Task CreateMailAddress(string domain, string email, string password)
        {
            await Api.PostAsync(VestaApiConstants.PathAddMail, new
            {
                token = await Api.GetCsrfToken(VestaApiConstants.PathAddMail),
                ok_acc = "add",
                v_domain = domain,
                v_account = email,
                v_password = password
            }, true, new
            {
                domain = domain
            });
        }
    }
}