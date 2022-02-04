using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CertPortal.Entities;
using CertPortal.Helpers;
using CertPortal.Models.Certificates;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using Microsoft.Extensions.Configuration;

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

        void GenerateCertificates(GenerateRequest request);
        string UploadFile(IFormFile doc, string uniqueFileName);
    }
    public class CertificateService: ICertificateService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IHostEnvironment _env;
        private static readonly string _serverUrl = "http://cportal.ddns.net:4444/certportal_uploads/";
        private static readonly string _serverDir = "D:\\wamp64\\www\\certportal_uploads";
        private readonly AppSettings _appSettings;
        private BlobServiceClient blobServiceClient;
        private readonly IConfiguration Configuration;

        public CertificateService(DataContext context, IMapper mapper, IHostEnvironment env, IOptions<AppSettings> appSettings, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _env = env;
            Configuration = configuration;
            _appSettings = appSettings.Value;
            blobServiceClient = new BlobServiceClient(Configuration.GetConnectionString("AZURE_STORAGE_CONNECTION_STRING"));
            
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
            BlobContainerClient blobClient = blobServiceClient.GetBlobContainerClient("uploads");

            BlobItem blob = GetBlobReferenceForCertificate(certificate, blobClient).Result;
            blobClient.DeleteBlob(blob.Name);
            // var oldFileDir = Path.Combine(_appSettings.UploadServerDir, certificate.FileName);
            // if (File.Exists(oldFileDir))
            // {
            //     File.Delete(oldFileDir);
            // }
            
            _context.Certificates.Remove(certificate);
            _context.SaveChanges();
        }

        private async Task<BlobItem> GetBlobReferenceForCertificate(Certificate certificate, BlobContainerClient blobClient)
        {
            BlobItem item = null;
            try
            {
                // Call the listing operation and return pages of the specified size.
                var resultSegment = blobClient.GetBlobsAsync(prefix:certificate.FileName)
                    .AsPages(default, 100);
                
                
                // Enumerate the blobs returned for each page.
                await foreach (Azure.Page<BlobItem> blobPage in resultSegment)
                {
                    int iterator = 0;
                    foreach (BlobItem blobItem in blobPage.Values)
                    {
                        if (iterator == 0)
                        {
                            item = blobItem;

                        }
                        Console.WriteLine("Blob name: {0}", blobItem.Name);
                        iterator += 1;
                    }

                    Console.WriteLine();
                }
            }
            catch (RequestFailedException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }

            return item;
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

        public void GenerateCertificates(GenerateRequest request)
        {
            // user send template id
            // using the id, and input of first name last name, course, issue date, expiration date
            // create a pdf and associate it to the student
            List<Account> studentsToBeAssigned = new List<Account>();
            if (request.StudentId != null)
            {
               var student = _context.Accounts
                    .Where(account1 => account1.Id == request.StudentId )
                    .Include(account =>  account.Certificates).FirstOrDefault();
               studentsToBeAssigned.Add(student);
            }
            else if (request.InstitutionId != null)
            {
                var institution = getInstitution(request.InstitutionId,true);
                var students = institution.Students ?? new List<Account>();
                studentsToBeAssigned = students.ToList();
            }
            else if (request.StudentIds != null && request.StudentIds.Count > 0)
            {
                var students = _context.Accounts.Where(account => request.StudentIds.Contains(account.Id));
                studentsToBeAssigned = students.ToList();

            }
            
            CreatePdfCertificateTemplate1(studentsToBeAssigned, request);
        }

        public string UploadFile(IFormFile doc, string uniqueFileName)
        {
            BlobContainerClient blobClient = blobServiceClient.GetBlobContainerClient("uploads");

            using (var ms = new MemoryStream())
            {
                doc.CopyTo(ms); 
                ms.Position = 0;
                blobClient.UploadBlobAsync(uniqueFileName,ms).Wait();
                // var fileBytes = ms.ToArray();
                // string s = Convert.ToBase64String(fileBytes);
                // act on the Base64 data
            }

            return blobClient.Uri.AbsoluteUri;
        }

        private void CreatePdfCertificateTemplate1(List<Account> studentsToBeAssigned, GenerateRequest request)
        {
            foreach (var student in studentsToBeAssigned)
            {
                CreateRequest model = new CreateRequest();
                
                var fileName = student.FullName() + " Template " + request.TemplateId + ".pdf";
                var uniqueFileName = GetUniqueFileName(fileName);
                BlobContainerClient blobClient = blobServiceClient.GetBlobContainerClient("uploads");
                var filePath = Path.Combine(_appSettings.UploadServerDir, uniqueFileName);

                // create cert here
                switch (request.TemplateId)
                {
                    case 1:
                        TemplateOneGen(filePath, student, request, uniqueFileName, blobClient);
                        break;
                    case 2:
                        TemplateTwoGen(filePath, student, request, uniqueFileName, blobClient);
                        break;
                    default:
                        break;
                }
                // TemplateOneGen(filePath, student, request);
         
                model.Name = fileName;
                model.InstitutionId = student.InstitutionId;
                model.AccountId = student.Id;
                model.Description = fileName;
                model.FileName = uniqueFileName;
                model.Url = blobClient.Uri.AbsoluteUri + "/" + uniqueFileName;

                Create(model);
            }
       
            
        }

        private void TemplateOneGen(string filePath, Account account, GenerateRequest request, string uniqueFileName, BlobContainerClient blobClient)
        {
            PdfDocument doc = new PdfDocument();
            doc.PageSettings.Orientation = PdfPageOrientation.Landscape;
            doc.PageSettings.Margins.All = 0;
            PdfPage page = doc.Pages.Add();
            
            // download image first
            WebClient myWebClient = new WebClient(); 
            byte[] bytes = myWebClient.DownloadData("https://certportal.blob.core.windows.net/images/peoplelab-temp-1.png"); 
            Stream imageStream = new MemoryStream(bytes); 

            PdfBitmap image = new PdfBitmap(imageStream); 
            PdfGraphicsState state = page.Graphics.Save();
            
            page.Graphics.DrawImage(image, new PointF(0,0), new SizeF(page.GetClientSize().Width, page.GetClientSize().Height));

            page.Graphics.Restore(state);
            
            PdfStringFormat format = new PdfStringFormat();
            //Set the text alignment.
            format.Alignment = PdfTextAlignment.Center;
            // global font
            PdfFont font = new PdfStandardFont(PdfFontFamily.TimesRoman, 20, PdfFontStyle.Bold);
            

            var firstName = account.FirstName;
            var lastName = account.LastName;
            var course = request.CourseName;
            var issuedDate = request.IssuedDateInput.Date.ToShortDateString();
            var expirationDate = request.ExpiryDateInput.Date.ToShortDateString();
            
            // first name 
            
            // last name
            page.Graphics.DrawString(firstName + " " + lastName, font, PdfBrushes.Black, new PointF(420, 280),format);

            // course
            page.Graphics.DrawString(course, font, PdfBrushes.Black, new PointF(420, 380),format);

            // issued date
            page.Graphics.DrawString(issuedDate, font, PdfBrushes.Black, new PointF(220, 450),format);

            // expiration date
            page.Graphics.DrawString(expirationDate, font, PdfBrushes.Black, new PointF(640, 450),format);
            
            // pdfGrid.Draw(page, new Syncfusion.Drawing.PointF(10, 10));
            // save file here
            //Saving the PDF to the MemoryStream
            MemoryStream stream = new MemoryStream();
 
            doc.Save(stream);   
            //Set the position as '0'
            stream.Position = 0;

            blobClient.UploadBlobAsync(uniqueFileName,stream).Wait();
            // Console.WriteLine(blobClient);
            stream.Close();
            // FileStream fileStream = new FileStream( filePath, FileMode.CreateNew, FileAccess.ReadWrite);
            // doc.Save(fileStream);
            // doc.Close();
            // fileStream.Close();

        }

        private void TemplateTwoGen(string filePath, Account account, GenerateRequest request, string uniqueFileName, BlobContainerClient blobClient)
        {
            PdfDocument doc = new PdfDocument();
            doc.PageSettings.Orientation = PdfPageOrientation.Landscape;
            doc.PageSettings.Margins.All = 0;
            PdfPage page = doc.Pages.Add();
            
               // download image first
            WebClient myWebClient = new WebClient(); 
            byte[] bytes = myWebClient.DownloadData("https://certportal.blob.core.windows.net/images/sample-temp-2.png"); 
            Stream imageStream = new MemoryStream(bytes); 

            PdfBitmap image = new PdfBitmap(imageStream); 
            PdfGraphicsState state = page.Graphics.Save();
            
            page.Graphics.DrawImage(image, new PointF(0,0), new SizeF(page.GetClientSize().Width, page.GetClientSize().Height));

            page.Graphics.Restore(state);
            
            PdfStringFormat format = new PdfStringFormat();
            //Set the text alignment.
            format.Alignment = PdfTextAlignment.Center;
            // global font
            PdfFont font = new PdfStandardFont(PdfFontFamily.TimesRoman, 20, PdfFontStyle.Bold);
            

            var organization = request.Organization;
            var firstName = account.FirstName;
            var lastName = account.LastName;
            var course = request.CourseName;
            var issuedDate = request.IssuedDateInput.Date.ToShortDateString();
            var expirationDate = request.ExpiryDateInput.Date.ToShortDateString();
            
            // organization
            page.Graphics.DrawString(organization, font, PdfBrushes.Black, new PointF(420, 85),format);

            //  name
            page.Graphics.DrawString(firstName + " " + lastName, font, PdfBrushes.Black, new PointF(420, 280),format);

            // course
            page.Graphics.DrawString(course, font, PdfBrushes.Black, new PointF(420, 380),format);

            // issued date
            page.Graphics.DrawString(issuedDate, font, PdfBrushes.Black, new PointF(220, 450),format);

            // expiration date
            page.Graphics.DrawString(expirationDate, font, PdfBrushes.Black, new PointF(640, 450),format);
            
            // pdfGrid.Draw(page, new Syncfusion.Drawing.PointF(10, 10));
            MemoryStream stream = new MemoryStream();
 
            doc.Save(stream);   
            //Set the position as '0'
            stream.Position = 0;

            blobClient.UploadBlobAsync(uniqueFileName,stream).Wait();
            // Console.WriteLine(blobClient);
            stream.Close();
            // FileStream fileStream = new FileStream( filePath, FileMode.CreateNew, FileAccess.ReadWrite);
            // doc.Save(fileStream);
            // doc.Close();
            // fileStream.Close();

        }


        private Certificate getCertificate(int id)
        {
            var certificate = _context.Certificates.Find(id);
            if (certificate == null) throw new KeyNotFoundException("Institution not found");
            return certificate;
        }
        
        private Institution getInstitution(int? id, bool withStudent = false)
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