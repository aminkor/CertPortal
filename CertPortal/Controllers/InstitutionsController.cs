using System.Collections.Generic;
using AutoMapper;
using CertPortal.Entities;
using CertPortal.Models.Institutions;
using CertPortal.Services;
using Microsoft.AspNetCore.Mvc;

namespace CertPortal.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InstitutionsController : BaseController
    {
        private readonly IInstitutionService _institutionService;
        private readonly IMapper _mapper;

        public InstitutionsController(IInstitutionService institutionService, IMapper mapper)
        {
            _institutionService = institutionService;
            _mapper = mapper;
        }
        
        [Authorize(UserRole.Admin)]
        [HttpGet]
        public ActionResult<IEnumerable<InstitutionResponse>> GetAll()
        {
            var institutions = _institutionService.GetAll();
            return Ok(institutions);
        }
        
        [Authorize]
        [HttpGet("{id:int}")]
        public ActionResult<InstitutionResponse> GetById(int id)
        {
            // users can get their own account and admins can get any account
            // TODO return unauthorized if not admin and getting non linked instititutions
            // if (id != Account.Id && Account.UserRole != UserRole.Admin)
            //     return Unauthorized(new { message = "Unauthorized" });

            var institution = _institutionService.GetById(id);
            return Ok(institution);
        }
        
        [Authorize(UserRole.Admin)]
        [HttpPost]
        public ActionResult<InstitutionResponse> Create(CreateRequest model)
        {
            var institution = _institutionService.Create(model);
            return Ok(institution);
        }
        
        [Authorize]
        [HttpPut("{id:int}")]
        public ActionResult<InstitutionResponse> Update(int id, UpdateRequest model)
        {
            // users can update their own account and admins can update any account
            // TODO return unauthorized if not admin and updating non linked instititutions
            // if (id != Account.Id && Account.UserRole != UserRole.Admin)
            //     return Unauthorized(new { message = "Unauthorized" });

        
            var institution = _institutionService.Update(id, model);
            return Ok(institution);
        }
        
        [Authorize]
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            // users can delete their own account and admins can delete any account
            // TODO return unauthorized if trying to delete non linked institutions
            // if (id != Account.Id && Account.UserRole != UserRole.Admin)
            //     return Unauthorized(new { message = "Unauthorized" });

            _institutionService.Delete(id);
            return Ok(new { message = "Institution deleted successfully" });
        }
        
        // institutions-students
        
        [Authorize]
        [HttpGet("Students/{instutitionId:int}")]
        public IActionResult GetStudents(int instutitionId)
        {
            // users can delete their own account and admins can delete any account
            // TODO return unauthorized if non admin trying to get students list, or the user no institution role on that
            // resource
            // if (id != Account.Id && Account.UserRole != UserRole.Admin)
            //     return Unauthorized(new { message = "Unauthorized" });

            var students = _institutionService.GetStudents(instutitionId);
            return Ok(students);
        }
        
        [Authorize]
        [HttpPost("Students")]
        public ActionResult<InstitutionResponse> AddStudent(AddStudentRequest model)
        {
            // authorization here
            var institution = _institutionService.AddStudent(model);
            return Ok(institution);
        }
        
        [Authorize]
        [HttpPost("Students/Remove")]
        public ActionResult<InstitutionResponse> RemoveStudent(AddStudentRequest model)
        {
            // authorization here
            _institutionService.RemoveStudent(model);
            return Ok(new { message = "Institution deleted successfully" });
        }
    }
}