using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net;
using System.Reflection.Metadata;
using WebAPI.Middleware;
using WebAPI.Models.Database;
using WebAPI.Models.Generic;
using WebAPI.Models.ViewModel;

namespace WebAPI.Services
{
    public interface IUploadDocumentService
    {
        Task<Response> UploadPDF(IFormFile file);
        Task<Response> GetPDF();
        Task<FileStreamResult> DownloadPDF(string fileName, int id);
    }
    public class UploadDocumentService : IUploadDocumentService
    {
        private readonly ApplicationContext _context;
        private readonly IMapper _mapper;
        private readonly AppSettings _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public string GetCurrentUser()
        {
            return "SystemAdmin";
        }

        public UploadDocumentService(ApplicationContext context, IOptions<AppSettings> config, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = config.Value;
            _context = context;
            _webHostEnvironment = webHostEnvironment;

        }

        public async Task<Response> UploadPDF(IFormFile file)

        {
            var resp = new Response();
            var relativepath = GetMediaRelativePath();
            var ppath = GetMediaPhysicalPath(relativepath);
            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "wwwroot\\images\\"));
            var pdf = new UploadDocument();
            try
            {
                if (file == null || file.Length == 0)
                {
                    resp.Success = false;
                    resp.HttpResponseCode = System.Net.HttpStatusCode.NotFound;
                    resp.CustomResponseCode = "404 NotFound ";
                    resp.Message = "File Null or empty ";
                    resp.Result = null;
                    return resp;
                }
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(ppath));
                }
                var localPath = Path.Combine(path, file.FileName);
                using (var fileStream = new FileStream(localPath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                pdf.FilePath = GetFullPath(file.FileName);

                pdf.FileName = file.FileName;

                pdf.CreatedOn = DateTime.UtcNow;
                pdf.CreatedBy = GetCurrentUser();
                pdf.UpdatedOn = DateTime.UtcNow;
                pdf.UpdatedBy = GetCurrentUser();
                pdf.IsActive = true;

                _context.UploadDocuments.Add(pdf);
                _context.SaveChanges();

                resp.Success = true;
                resp.HttpResponseCode = System.Net.HttpStatusCode.OK;
                resp.CustomResponseCode = "200 OK";
                resp.Message = "File upload Successfully";
                resp.Result = pdf.FileName;
                return resp;

            }
            catch (Exception ex)
            {
                LogTraceFactory.LogMessage(ex.ToString());
                resp.HttpResponseCode = System.Net.HttpStatusCode.BadRequest;
                resp.CustomResponseCode = "400 BadRequest";
                resp.Success = false;
                resp.Message = "Exception" + ex + "Path : " + path;
                resp.Result = null;
                return resp;
            }
        }

        private string GetMediaPhysicalPath(string mediaUploadValidatorName)
        {
            return Path.Combine(_webHostEnvironment.ContentRootPath, GetMediaRelativePath());
        }

        private string GetMediaRelativePath()
        {
            return Path.Combine("wwwroot\\images\\");
        }

        private string GetFullPath(string fileName)
        {
            return Path.Combine(_configuration.ServerAPIPath ?? string.Empty, fileName);
        }

        public async Task<Response> GetPDF()
        {
            var resp = new Response();
            try
            {
                var result = await _context.UploadDocuments.ToListAsync();
                if (result.Any())
                {
                    resp.Success = true;
                    resp.HttpResponseCode = HttpStatusCode.OK;
                    resp.CustomResponseCode = "200 OK";
                    resp.Message = "List Loaded Successfully";
                    resp.Result = result;
                    resp.Count = result.Count;
                    return resp;
                }

                // If no records found, set result as an empty list
                resp.Success = true;
                resp.HttpResponseCode = HttpStatusCode.OK;
                resp.CustomResponseCode = "200 OK";
                resp.Message = "Histroy Not Found";
                resp.Result = new List<UploadDocument>(); // This ensures an empty array in the JSON response
                resp.Count = 0;
                return resp;
            }
            catch (Exception ex)
            {
                LogTraceFactory.LogMessage(ex.ToString());
                resp.HttpResponseCode = System.Net.HttpStatusCode.BadRequest;
                resp.CustomResponseCode = "400 BadRequest";
                resp.Success = false;
                resp.Message = "Exception" + ex;
                resp.Result = null;
                return resp;
            }
        }
        public async Task<FileStreamResult> DownloadPDF(string fileName, int id)
        {
            var resp = new Response();
            try
            {
                var basePath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "wwwroot\\images\\"));

                var result = _context.UploadDocuments.FirstOrDefault(pd => pd.FileName == fileName && pd.Id == id);
                if (result != null)
                {
                    var filePath = Path.Combine(basePath, result.FilePath); // Assuming you have the file path stored in result.FilePath
                    if (File.Exists(filePath))
                    {
                        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                        var contentType = "application/octet-stream"; // Set the appropriate content type based on the file type

                        return new FileStreamResult(fileStream, contentType)
                        {
                            FileDownloadName = fileName // Set the file name for download
                        };
                        
                    }
                    else
                    {
                        // Handle the case when the file is not found
                        return new FileStreamResult(Stream.Null, "application/octet-stream")
                        {
                            FileDownloadName = fileName // Set the file name for download (it could be a generic name)
                        };
                    }
                }

                // Handle the case when the history is not found
                return new FileStreamResult(Stream.Null, "application/octet-stream")
                {
                    FileDownloadName = fileName // Set the file name for download (it could be a generic name)
                };
            }
            catch (Exception ex)
            {
                // Handle exceptions and log as needed
                LogTraceFactory.LogMessage(ex.ToString());

                // Return an error response or throw an exception based on your requirement
                throw new Exception("Error occurred while processing the request.", ex);
            }
        }

    }
}
