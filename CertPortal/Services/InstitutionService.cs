using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CertPortal.Entities;
using CertPortal.Helpers;
using CertPortal.Models.Institutions;
using Microsoft.EntityFrameworkCore;

namespace CertPortal.Services
{
    public interface IInstitutionService
    {
        IEnumerable<InstitutionResponse> GetAll();
        InstitutionResponse GetById(int id);
        InstitutionResponse Create(CreateRequest model);
        InstitutionResponse Update(int id, UpdateRequest model);
        void Delete(int id);
        IEnumerable<StudentResponse> GetStudents(int institutionId);
        StudentResponse AddStudent(AddStudentRequest model);
        void RemoveStudent(AddStudentRequest model);
        IEnumerable<InstitutionResponse> GetInstructorInstitutions(int userId);
    }
    public class InstitutionService : IInstitutionService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public InstitutionService(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IEnumerable<InstitutionResponse> GetAll()
        {
            IEnumerable<InstitutionResponse> institutionResponses = new List<InstitutionResponse>();
            var institutions = _context.Institutions
                .Include(institution => institution.Students )
                .Include(institution => institution.Certificates );
            foreach (var institution in institutions)
            {
                int studentsCount = institution.Students.Count;
                int certificatesCount = institution.Certificates.Count;
                InstitutionResponse institutionResponse = new InstitutionResponse()
                {
                    Id = institution.Id,
                    Name = institution.Name,
                    Address = institution.Address,
                    Created = institution.Created,
                    Updated = institution.Updated,
                    Description = institution.Description,
                    StudentsCounts = studentsCount.ToString(),
                    CertificatesCounts = certificatesCount.ToString()
                };
                institutionResponses = institutionResponses.Append(institutionResponse);
            }
            return institutionResponses;
        }
        
        public InstitutionResponse GetById(int id)
        {
            var institution = getInstitution(id);
            return _mapper.Map<InstitutionResponse>(institution);
        }
        
        public InstitutionResponse Create(CreateRequest model)
        {
       
            // validate
            // if (_context.Accounts.Any(x => x.Email == model.Email))
            //     throw new AppException($"Email '{model.Email}' is already registered");

            // map model to new account object
            var institution = _mapper.Map<Institution>(model);
            institution.Created = DateTime.UtcNow;

          

            // save account
            _context.Institutions.Add(institution);
            _context.SaveChanges();

            return _mapper.Map<InstitutionResponse>(institution);
        }
        
        public InstitutionResponse Update(int id, UpdateRequest model)
        {
            var institution = getInstitution(id);

            // validate
            // if (account.Email != model.Email && _context.Accounts.Any(x => x.Email == model.Email))
            //     throw new AppException($"Email '{model.Email}' is already taken");

   
            // copy model to account and save
            _mapper.Map(model, institution);
            institution.Updated = DateTime.UtcNow;
            _context.Institutions.Update(institution);
            _context.SaveChanges();

            return _mapper.Map<InstitutionResponse>(institution);
        }
        
        public void Delete(int id)
        {
            var institution = getInstitution(id);
            _context.Institutions.Remove(institution);
            _context.SaveChanges();
        }

        public IEnumerable<StudentResponse> GetStudents(int institutionId)
        {
            var institution = getInstitution(institutionId,true);
            var students = institution.Students ?? new List<Account>();
            
            IEnumerable<StudentResponse> studentResponses = new List<StudentResponse>();
            foreach (var student in students)
            {
                
                StudentResponse studentResponse = new StudentResponse();
                studentResponse.Id = student.Id;
                studentResponse.FirstName = student.FirstName;
                studentResponse.LastName = student.LastName;
                studentResponse.Created = student.Created;
                studentResponse.Updated = student.Updated;

                studentResponses = studentResponses.Append(studentResponse);

            }

            return studentResponses;
        }

        public StudentResponse AddStudent(AddStudentRequest request)
        {
            StudentResponse studentResponse = new StudentResponse();

            Account student = _context.Accounts.Find(request.StudentId);
            Institution institution = _context.Institutions.Find(request.InstitutionId);

            if (student != null && institution != null)
            {
                student.InstitutionId = institution.Id;

                _context.Accounts.Update(student);
                _context.SaveChanges();
            }
            else
            {
                throw new KeyNotFoundException("Institution or Account not found");
            }
            return studentResponse;
        }
        
        public void RemoveStudent(AddStudentRequest request)
        {
            Account student = _context.Accounts.Find(request.StudentId);


            if (student != null)
            {
                student.InstitutionId = null;
                _context.Accounts.Update(student);
                _context.SaveChanges();

            }
            else
            {
                throw new KeyNotFoundException("Institution or Account not found");

            }
        }

        public IEnumerable<InstitutionResponse> GetInstructorInstitutions(int userId)
        {
            IEnumerable<InstitutionResponse> institutionResponses = new List<InstitutionResponse>();
            var account = _context.Accounts.Find(userId);
            
            var accountInstitutions = _context.RoleInstitutions
                .Where(institution => institution.AccountId == userId )
                .Include(institution => institution.Institution )
                .ThenInclude( inst => inst.Students)
                .Include(institution => institution.Institution )
                .ThenInclude( inst => inst.Certificates);

            foreach (var accountInstitution in accountInstitutions)
            {
                int studentsCount = accountInstitution.Institution.Students.Count;
                int certificatesCount = accountInstitution.Institution.Certificates.Count;
                
                InstitutionResponse institutionResponse = new InstitutionResponse()
                {
                    Id = accountInstitution.Institution.Id,
                    Name = accountInstitution.Institution.Name,
                    Address = accountInstitution.Institution.Address,
                    Created = accountInstitution.Institution.Created,
                    Updated = accountInstitution.Institution.Updated,
                    Description = accountInstitution.Institution.Description,
                    StudentsCounts = studentsCount.ToString(),
                    CertificatesCounts = certificatesCount.ToString()

                };
                institutionResponses = institutionResponses.Append(institutionResponse);
            }
            return institutionResponses;
        }


        private Institution getInstitution(int id, bool withStudent = false)
        {
            Institution institution = null;
            if (withStudent)
            {
                institution = _context.Institutions
                    .Where(institution => institution.Id == id)
                    .Include( institution => institution.Students).FirstOrDefault();

            }
            else
            {
                institution = _context.Institutions.Find(id);

            }
            if (institution == null) throw new KeyNotFoundException("Institution not found");
            return institution;
        }
    }
}