using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using CertPortal.Entities;
using CertPortal.Helpers;
using CertPortal.Models.Certificates;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CertPortal.Services
{
    public interface ICertificateService
    {
        IEnumerable<CertificateResponse> GetAll();
        CertificateResponse GetById(int id);
        CertificateResponse Create(CreateRequest model);
        CertificateResponse Update(int id, UpdateRequest model);
        void Delete(int id);
        IEnumerable<CertificateResponse> GetUserCertificates(int userId);


        IEnumerable<CertificateResponse> GetInstitutionCertificates(int institutionId);
        IEnumerable<CertificateResponse> GetInstructorCertificates(int instructorId);

    }
    public class CertificateService: ICertificateService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IHostEnvironment _env;
        private static readonly string _serverUrl = "http://cportal.ddns.net:4444/certportal_uploads/";
        private static readonly string _serverDir = "D:\\wamp64\\www\\certportal_uploads";
        private readonly AppSettings _appSettings;

        public CertificateService(DataContext context, IMapper mapper, IHostEnvironment env, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _mapper = mapper;
            _env = env;
            _appSettings = appSettings.Value;
        }

        public IEnumerable<CertificateResponse> GetAll()
        {
            var certificates = _context.Certificates
                .Include(certificate => certificate.Account )
                .Include(certificate => certificate.Institution );
            IEnumerable<CertificateResponse> certificateResponses = new List<CertificateResponse>();
            foreach (var certificate in certificates)
            {
                CertificateResponse certificateResponse = _mapper.Map<CertificateResponse>(certificate);
                certificateResponse.IssuedBy = certificate.Institution != null ? certificate.Institution.Name : "";
                certificateResponse.AssignedTo = certificate.Account != null ? certificate.Account.FullName() : "";
                certificateResponses = certificateResponses.Append(certificateResponse);
            }

            return _mapper.Map<IList<CertificateResponse>>(certificateResponses);
        }

        public CertificateResponse GetById(int id)
        {
            var certificate = getCertificate(id);
            return _mapper.Map<CertificateResponse>(certificate);
        }

        public CertificateResponse Create(CreateRequest model)
        {
            // map model to new account object
            var certificate = _mapper.Map<Certificate>(model);
            certificate.Created = DateTime.UtcNow;

          

            // save account
            _context.Certificates.Add(certificate);
            _context.SaveChanges();

            return _mapper.Map<CertificateResponse>(certificate);
        }

        public CertificateResponse Update(int id, UpdateRequest model)
        {
            var certificate = getCertificate(id);

            // delete old file first
            if (model.Url != null)
            {
                var oldFileDir = Path.Combine(_appSettings.UploadServerDir, certificate.FileName);
                if (File.Exists(oldFileDir))
                {
                    File.Delete(oldFileDir);
                }
            }
        
            // copy model to account and save
            _mapper.Map(model, certificate);
            certificate.Updated = DateTime.UtcNow;
            _context.Certificates.Update(certificate);
            _context.SaveChanges();

            return _mapper.Map<CertificateResponse>(certificate);
        }

        public void Delete(int id)
        {
            var certificate = getCertificate(id);
            
            // delete file first
            var oldFileDir = Path.Combine(_appSettings.UploadServerDir, certificate.FileName);
            if (File.Exists(oldFileDir))
            {
                File.Delete(oldFileDir);
            }
            
            _context.Certificates.Remove(certificate);
            _context.SaveChanges();
        }

        public IEnumerable<CertificateResponse> GetUserCertificates(int userId)
        {
            var account = _context.Accounts
                .Where(account1 => account1.Id == userId )
                .Include(account =>  account.Certificates).FirstOrDefault();
            IEnumerable<Certificate> certificates = new List<Certificate>();

            if (account != null)
            {
                var accountCertificates = account.Certificates;
                foreach (var accountCertificate in accountCertificates)
                {
                    certificates = certificates.Append(accountCertificate);
                }
            }
            return _mapper.Map<IList<CertificateResponse>>(certificates);

        }
        public IEnumerable<CertificateResponse> GetInstitutionCertificates(int institutionId)
        {
            var institution = _context.Institutions
                .Where(institution1 => institution1.Id == institutionId)
                .Include(institution1 => institution1.Certificates)
                .ThenInclude( cert => cert.Account)
                .FirstOrDefault();
            
            IEnumerable<CertificateResponse> certificateResponses = new List<CertificateResponse>();
            if (institution != null)
            {
                foreach (var certificate in institution.Certificates)
                {
                    CertificateResponse certificateResponse = _mapper.Map<CertificateResponse>(certificate);
                    certificateResponse.IssuedBy = institution.Name;
                    certificateResponse.AssignedTo = certificate.Account != null ? certificate.Account.FullName() : "";
                    certificateResponses = certificateResponses.Append(certificateResponse);
                }
            }
         
            return _mapper.Map<IList<CertificateResponse>>(certificateResponses);
        }
        public IEnumerable<CertificateResponse> GetInstructorCertificates(int instructorId)
        {
            IEnumerable<CertificateResponse> certificateResponses = new List<CertificateResponse>();

            var instructor = _context.Accounts.Where(instructor => instructor.Id == instructorId).FirstOrDefault();
            if (instructor != null)
            {
                var roleInstitutions =
                    _context.RoleInstitutions
                        .Where(roleInstitution => roleInstitution.AccountId == instructorId)
                        .Include(roleInstitution => roleInstitution.Institution)
                        .ThenInclude( institution1 => institution1.Certificates )
                        .ThenInclude(certificate => certificate.Account );

                foreach (var roleInstitution in roleInstitutions)
                {
                    foreach (var certificate in roleInstitution.Institution.Certificates)
                    {
                        CertificateResponse certificateResponse = _mapper.Map<CertificateResponse>(certificate);
                        certificateResponse.IssuedBy = roleInstitution.Institution.Name;
                        certificateResponse.AssignedTo = certificate.Account != null ? certificate.Account.FullName() : "";
                        certificateResponses = certificateResponses.Append(certificateResponse);
                    }
             
                }
            }
      
            return _mapper.Map<IList<CertificateResponse>>(certificateResponses);
        }

        private Certificate getCertificate(int id)
        {
            var certificate = _context.Certificates.Find(id);
            if (certificate == null) throw new KeyNotFoundException("Institution not found");
            return certificate;
        }
    }
}