namespace VestaClient.Api
{
    /// <summary>
    ///     Place of every endpoints, headers and other constants and variables.
    /// </summary>
    internal static class VestaApiConstants
    {
        public const string SessionCookieField = "PHPSESSID";

        public const string HeaderUserAgent =
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_12_2) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.95 Safari/537.36";

        public const int DefaultPageSize = 100;

        public const string PathLogin = "login/";

        public const string PathListMail = "list/mail/";
        public const string PathAddMail = "add/mail/";
    }
}