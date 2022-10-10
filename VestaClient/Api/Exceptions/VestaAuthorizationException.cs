namespace VestaClient.Api.Exceptions
{
    public class VestaAuthorizationException : VestaException
    {
        public VestaAuthorizationException(string message)
            : base(message)
        {
            HttpStatusCode = 401;
        }
    }
}
