using VestaClient.Api;

namespace VestaClient
{
    public class VestaClient
    {
        private VestaApi Api { get; }

        public Auth Auth { get; }
        
        public Mail Mail { get; }

        public Custom Custom { get; }
        
        public VestaClient(string uri, string username, string password, bool autoLogin = false,
            string sessionDataPath = null)
        {
            Api = new VestaApi(uri, username, password, autoLogin, sessionDataPath);

            Auth = new Auth(Api);
            Mail = new Mail(Api);
            Custom = new Custom(Api, uri);
        }
    }
}