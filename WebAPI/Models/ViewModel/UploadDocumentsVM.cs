
namespace WebAPI.Models.ViewModel
{
    public class UploadDocumentsVM
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public IFormFile FilePath { get; set; }
    }
}
