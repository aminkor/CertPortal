using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CertPortal.Entities;
using CertPortal.Models.Certificates;
using CertPortal.Models.Institutions;
using CertPortal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using CreateRequest = CertPortal.Models.Certificates.CreateRequest;
using UpdateRequest = CertPortal.Models.Certificates.UpdateRequest;


namespace CertPortal.Controllers
{
      [ApiController]
    [Route("[controller]")]
    public class CertificatesController : BaseController
    {
        private readonly ICertificateService _certificateService;
        private readonly IMapper _mapper;
        private readonly IHostEnvironment _env;
        private static readonly string _serverUrl = "http://localhost/certportal_uploads/";
        private static readonly string _serverDir = "D:\\wamp64\\www\\certportal_uploads";
        public CertificatesController(ICertificateService certificateService, IMapper mapper, IHostEnvironment env)
        {
            _certificateService = certificateService;
            _mapper = mapper;
            _env = env;
        }
        
        [Authorize(UserRole.Admin)]
        [HttpGet]
        public ActionResult<IEnumerable<CertificateResponse>> GetAll()
        {
            var certificates = _certificateService.GetAll();
            return Ok(certificates);
        }
        
        [Authorize]
        [HttpGet("{id:int}")]
        public ActionResult<CertificateResponse> GetById(int id)
        {
            // users can get their own account and admins can get any account
            // TODO return unauthorized if not admin and getting non linked certificate
            // if (id != Account.Id && Account.UserRole != UserRole.Admin)
            //     return Unauthorized(new { message = "Unauthorized" });

            var certificate = _certificateService.GetById(id);
            return Ok(certificate);
        }
        
        [Authorize(UserRole.Admin)]
        [HttpPost]
        public async Task<ActionResult<CertificateResponse>> Create([FromForm] CreateRequest model)
        {
            if (Request.Form.Files.Count > 0)
            {
                var doc = Request.Form.Files.First();
                var uniqueFileName = GetUniqueFileName(doc.FileName);
                var filePath = Path.Combine(_serverDir, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await doc.CopyToAsync(stream);
                }
                // await doc.CopyToAsync(new FileStream(filePath, FileMode.Create));
                
                model.FileName = uniqueFileName;
                model.Url = _serverUrl + uniqueFileName;
            }
          
            var certificate = _certificateService.Create(model);
            return Ok(certificate);
        }
        
        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<CertificateResponse>> Update(int id,[FromForm]  UpdateRequest model)
        {
            // users can update their own account and admins can update any account
            // TODO return unauthorized if not admin and updating non linked certificate
            // if (id != Account.Id && Account.UserRole != UserRole.Admin)
            //     return Unauthorized(new { message = "Unauthorized" });

            if (Request.Form.Files.Count > 0)
            {
                var doc = Request.Form.Files.First();
                var uniqueFileName = GetUniqueFileName(doc.FileName);
                var filePath = Path.Combine(_serverDir, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await doc.CopyToAsync(stream);
                }
                model.FileName = uniqueFileName;
                model.Url = _serverUrl + uniqueFileName;
            }
        
            var certificate = _certificateService.Update(id, model);
            return Ok(certificate);
        }
        
        [Authorize]
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            // users can delete their own account and admins can delete any account
            // TODO return unauthorized if trying to delete non linked certificate
            // if (id != Account.Id && Account.UserRole != UserRole.Admin)
            //     return Unauthorized(new { message = "Unauthorized" });

            _certificateService.Delete(id);
            return Ok(new { message = "Certificate deleted successfully" });
        }
        
        [Authorize]
        [HttpGet("users/{userId:int}")]
        public ActionResult<IEnumerable<CertificateResponse>> GetUserCertificates(int userId)
        {
            var certificates = _certificateService.GetUserCertificates(userId);
            return Ok(certificates);
        }
        
        private string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return Path.GetFileNameWithoutExtension(fileName)
                   + "_"
                   + Guid.NewGuid().ToString().Substring(0, 4)
                   + Path.GetExtension(fileName);
        }
    }
}