using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CertPortal.Models.Uploads;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace CertPortal.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UploadsController : BaseController
    {
        private readonly IHostEnvironment _env;

        public UploadsController(IHostEnvironment env)
        {
            _env = env;
        }
        
        [Route("documents")]
        [HttpPost]
        public async Task<string> UploadFile([FromForm] FileUploadViewModel model)
        {
            var doc = Request.Form.Files.First();
            var uniqueFileName = GetUniqueFileName(doc.FileName);
            var dir = Path.Combine(_env.ContentRootPath, "Documents");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            var filePath = Path.Combine(dir, uniqueFileName);
            await doc.CopyToAsync(new FileStream(filePath, FileMode.Create));
            SaveDocumentsPathToDb(model.Description, filePath);
            return uniqueFileName;
        }
        
        private string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return Path.GetFileNameWithoutExtension(fileName)
                   + "_"
                   + Guid.NewGuid().ToString().Substring(0, 4)
                   + Path.GetExtension(fileName);
        }
    
        private void SaveDocumentsPathToDb(string description, string filepath)
        {
            //todo: description and file path to db
            Console.WriteLine(description + " " + filepath);
        }
    }
}