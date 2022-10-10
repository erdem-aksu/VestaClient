using System.Threading.Tasks;

namespace VestaClient.Api
{
    public class Auth
    {
        private VestaApi Api { get; }

        public Auth(VestaApi api)
        {
            Api = api;
        }

        public async Task Login()
        {
            await Api.Login();
        }

        public void Logout()
        {
            Api.Logout();
        }
    }
}