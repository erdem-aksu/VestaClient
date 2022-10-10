namespace VestaClient.Api.Exceptions
{
    public class VestaBadRequestException : VestaException
    {
        public VestaBadRequestException(string message)
            : base(message)
        {
            HttpStatusCode = 400;
        }
    }
}
