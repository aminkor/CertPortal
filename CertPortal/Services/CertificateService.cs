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


    }
    public class CertificateService: ICertificateService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IHostEnvironment _env;
        private static readonly string _serverUrl = "http://localhost/certportal_uploads/";
        private static readonly string _serverDir = "D:\\wamp64\\www\\certportal_uploads";
        public CertificateService(DataContext context, IMapper mapper, IHostEnvironment env)
        {
            _context = context;
            _mapper = mapper;
            _env = env;
        }

        public IEnumerable<CertificateResponse> GetAll()
        {
            var certificates = _context.Certificates;
            return _mapper.Map<IList<CertificateResponse>>(certificates);
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
                var oldFileDir = Path.Combine(_serverDir, certificate.FileName);
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
            _context.Certificates.Remove(certificate);
            _context.SaveChanges();
        }

        public IEnumerable<CertificateResponse> GetUserCertificates(int userId)
        {
            var account = _context.Accounts.Find(userId);
            var accountCertificates = _context.AccountCertificates.Include(certificate => certificate.Certificate );
            IEnumerable<Certificate> certificates = new List<Certificate>();
            foreach (var accountCertificate in accountCertificates)
            {
                certificates = certificates.Append(accountCertificate.Certificate);
            }
            return _mapper.Map<IList<CertificateResponse>>(certificates);
        }

        private Certificate getCertificate(int id)
        {
            var certificate = _context.Certificates.Find(id);
            if (certificate == null) throw new KeyNotFoundException("Institution not found");
            return certificate;
        }
    }
}