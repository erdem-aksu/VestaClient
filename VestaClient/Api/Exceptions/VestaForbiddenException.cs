namespace VestaClient.Api.Exceptions
{
    public class VestaForbiddenException : VestaException
    {
        public VestaForbiddenException(string message)
            : base(message)
        {
            HttpStatusCode = 403;
        }
    }
}
