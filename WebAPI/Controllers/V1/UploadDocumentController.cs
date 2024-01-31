using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Database;
using WebAPI.Models.ViewModel;
using WebAPI.Services;

namespace WebAPI.Controllers.V1
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadDocumentController: ControllerBase
    {
        
        private readonly IUploadDocumentService _uploadDocumentService;


        public UploadDocumentController( IUploadDocumentService uploadDocumentService)
        {
            //_context = context;
            _uploadDocumentService = uploadDocumentService;
        }


        [HttpPost, Route("UploadDocumentPDF")]
        public async Task<Response> UploadDocumentPDF(IFormFile file)
        {
            return await _uploadDocumentService.UploadPDF(file);
        }

        [HttpGet("GetDocument")]
        public async Task<Response> GetDocument() 
        {
            return await _uploadDocumentService.GetPDF(); 
        }

        [HttpGet("GetDownloadPDF")]
        public async Task<FileStreamResult> GetDownloadPDF(string fileName,int id)
        {
            return await _uploadDocumentService.DownloadPDF(fileName, id);
        }
    }
}
