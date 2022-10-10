namespace VestaClient.Api.Responses
{
    public class AjaxResponse<T>
    {
        public T Data { get; set; }

        public bool Success { get; set; }
    }
}
