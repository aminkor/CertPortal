using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CertPortal.Entities;
using CertPortal.Helpers;
using CertPortal.Models.Certificates;
using CertPortal.Models.Institutions;
using CertPortal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
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
        private static readonly string _serverUrl = "http://cportal.ddns.net:4444/certportal_uploads/";
        private static readonly string _serverDir = "D:\\wamp64\\www\\certportal_uploads";
        private readonly AppSettings _appSettings;
        private readonly DataContext _context;

        public CertificatesController(ICertificateService certificateService, IMapper mapper, IHostEnvironment env, IOptions<AppSettings> appSettings, DataContext context)
        {
            _certificateService = certificateService;
            _mapper = mapper;
            _env = env;
            _context = context;
            _appSettings = appSettings.Value;
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
        
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<CertificateResponse>> Create([FromForm] CreateRequest model)
        {
            // return unauthorized if the role is non admin, and trying to create a certificate not associated to its institutions
            List<UserRole> authorizedRoles = new List<UserRole> { UserRole.Admin , UserRole.Instructor};
            if (authorizedRoles.Contains(Account.UserRole) == false)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }
            else if (Account.UserRole == UserRole.Instructor)
            {
                // currently must create cert directly to institution
                if (model.InstitutionId == null || model.InstitutionId == 0)
                    return Unauthorized(new { message = "Unauthorized" });

                // limits certificate creation to its institution only
                if (_context.RoleInstitutions.Any(role =>
                    role.AccountId == Account.Id && role.InstitutionId == model.InstitutionId) == false)
                    return Unauthorized(new { message = "Unauthorized" });

            }
         
            
            if (Request.Form.Files.Count > 0)
            {
                var doc = Request.Form.Files.First();
                var uniqueFileName = GetUniqueFileName(doc.FileName);
                string storageUrl = _certificateService.UploadFile(doc, uniqueFileName);
                // var filePath = Path.Combine(_appSettings.UploadServerDir, uniqueFileName);
                // using (var stream = new FileStream(filePath, FileMode.Create))
                // {
                //     await doc.CopyToAsync(stream);
                // }
                // await doc.CopyToAsync(new FileStream(filePath, FileMode.Create));
                
                model.FileName = uniqueFileName;
                model.Url = storageUrl + "/" + uniqueFileName;
            }
          
            var certificate = _certificateService.Create(model);
            return Ok(certificate);
        }
        
        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<CertificateResponse>> Update(int id,[FromForm]  UpdateRequest model)
        {
            // return unauthorized if the role is non admin, and trying to create a certificate not associated to its institutions
            List<UserRole> authorizedRoles = new List<UserRole> { UserRole.Admin , UserRole.Instructor};
            if (authorizedRoles.Contains(Account.UserRole) == false)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }
            else if (Account.UserRole == UserRole.Instructor)
            {
                // disallow institution unassigment if role is instructor. exclude student assignment/unassignment
                var cert = _certificateService.GetById(id);
                List<string> authorizedActions = new List<string> { "instructor-assign-student", "instructor-unassign-student"  };
                if (authorizedActions.Contains(model.ActionType) == false)
                    return Unauthorized(new { message = "Unauthorized" });

            }
            
            if (Request.Form.Files.Count > 0)
            {
                var doc = Request.Form.Files.First();
                var uniqueFileName = GetUniqueFileName(doc.FileName);
                var filePath = Path.Combine(_appSettings.UploadServerDir, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await doc.CopyToAsync(stream);
                }
                model.FileName = uniqueFileName;
                model.Url = _appSettings.UploadServerUrl + uniqueFileName;
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
        
        [Authorize]
        [HttpGet("institutions/{institutionId:int}")]
        public ActionResult<IEnumerable<CertificateResponse>> GetInstitutionCertificates(int institutionId)
        {
            var certificates = _certificateService.GetInstitutionCertificates(institutionId);
            return Ok(certificates);
        }
        
        [Authorize]
        [HttpGet("instructors/{instructorId:int}")]
        public ActionResult<IEnumerable<CertificateResponse>> GetInstructorCertificates(int instructorId)
        {
            var certificates = _certificateService.GetInstructorCertificates(instructorId);
            return Ok(certificates);
        }

        [Authorize]
        [HttpPost("GenerateCert")]
        public ActionResult GenerateCertificates(GenerateRequest model)
     
        {
            model.ParseDates();
            _certificateService.GenerateCertificates(model);
            return Ok();
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