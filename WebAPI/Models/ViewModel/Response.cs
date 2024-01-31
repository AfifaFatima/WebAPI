namespace WebAPI.Models.ViewModel
{
    public class Response
    {
        public object Result { get; set; }
        public System.Net.HttpStatusCode HttpResponseCode { get; set; }
        public string CustomResponseCode { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
        public int Count { get; internal set; }
    }
}
