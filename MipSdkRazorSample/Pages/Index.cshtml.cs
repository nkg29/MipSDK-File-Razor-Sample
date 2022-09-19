using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Web;
using System.Net;
using Microsoft.Graph;
using MipSdkRazorSample.Services;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;
using NuGet.Protocol;
using Microsoft.AspNetCore.Components.Forms;
using MipSdkRazorSample.FileUploadService;
using Microsoft.EntityFrameworkCore;
using MipSdkRazorSample.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.InkML;
using System.Linq.Expressions;

namespace MipSdkRazorSample.Pages
{

    [AuthorizeForScopes(ScopeKeySection = "MicrosoftGraph:Scopes")]
    public class IndexModel : PageModel
    {
        private readonly MipSdkRazorSample.Data.MipSdkRazorSampleContext _context;
        private readonly GraphServiceClient _graphServiceClient;
        private readonly ILogger<IndexModel> _logger;
        private readonly IMipService _mipApi;
        private readonly IExcelService _excelService;
        private IWebHostEnvironment _environment;
        private readonly IFileUploadService _fileUploadService;
        private readonly string _userId;
        public string[] files;

        [BindProperty]
        public string ErrorMsg { get; set; }

        [BindProperty]
        public string FilePath { get; set; }

        public IndexModel(ILogger<IndexModel> logger, GraphServiceClient graphServiceClient, MipSdkRazorSample.Data.MipSdkRazorSampleContext context, IWebHostEnvironment environment, IFileUploadService fileUploadService)
        {
            _context = context;
            _logger = logger;
            _excelService = _context.GetService<IExcelService>();
            _mipApi = _context.GetService<IMipService>();
            _graphServiceClient = graphServiceClient;
            _environment = environment;
            _fileUploadService = fileUploadService;
        }

        [HttpPost]
        public async Task<IActionResult> OnPostAsync(IFormFile file, string labelId)
        {
            try
            {
                if (file == null) ErrorMsg = "No file added";
                else
                {
                    FilePath = await _fileUploadService.UploadFileAsync(file);
                    //string labelId = "defa4170-0d19-0005-0000-bc88714345d2"; //_context.DataPolicy.First(d => d.PolicyName == "Download Policy").MinLabelIdForAction;
                    using (var fileStream = new FileStream(FilePath, FileMode.Create))
                    {
                        MemoryStream? mipStream = _mipApi.ApplyMipLabel(fileStream, labelId);
                        mipStream.Position = 0;
                        return File(mipStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.FileName);
                    }
                }
                return this.RedirectToPage();
            }
            catch (Exception ex)
            {
                ErrorMsg = $"Error: " + ex.Message;
                return this.RedirectToPage();
            }
            
        }

        public IList<DataPolicy> DataPolicy { get; set; }
        public IList<MipLabel> MipLabels { get; set; }

        [BindProperty]
        public MipLabel MipLabel { get; set; }
        public async Task OnGetAsync()
        {
            MipLabels = _mipApi.GetMipLabels(_userId);
            DataPolicy = await _context.DataPolicy.ToListAsync();
            Options = MipLabels.Select(a =>
                                          new SelectListItem
                                          {
                                              Value = a.Id,
                                              Text = a.Name
                                          }).ToList();
        }

        public List<SelectListItem> Options { get; set; }

        //public FileResult OnPostExport()
        //{

        //    string labelId = _context.DataPolicy.First(d => d.PolicyName == "Download Policy").MinLabelIdForAction;
        //    var file = Path.Combine(_environment.ContentRootPath, "uploads", "test");
        //    using (var fileStream = new FileStream(file, FileMode.Create))
        //    {
        //        MemoryStream? mipStream = _mipApi.ApplyMipLabel(fileStream, labelId);
        //        mipStream.Position = 0;
        //        return File(mipStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "EmployeeData.xlsx");
        //    }

        //    //var excelStream = _excelService.GenerateEmployeeExport(_context.Employees.ToList());
        //    //MemoryStream? mipStream = _mipApi.ApplyMipLabel(fileStream, labelId);
        //    //mipStream.Position = 0;

        //    //return File(mipStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "EmployeeData.xlsx");

        //}
    }
}