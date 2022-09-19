using Microsoft.AspNetCore.Http;

namespace MipSdkRazorSample.FileUploadService
{
    public interface IFileUploadService
    {
        Task<string> UploadFileAsync(IFormFile file);
    }
}
