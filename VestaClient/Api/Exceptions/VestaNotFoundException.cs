namespace VestaClient.Api.Exceptions
{
    public class VestaNotFoundException : VestaException
    {
        public VestaNotFoundException(string message)
            : base(message)
        {
            HttpStatusCode = 404;
        }
    }
}
