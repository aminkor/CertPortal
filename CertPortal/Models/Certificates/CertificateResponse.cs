using System;

namespace CertPortal.Models.Certificates
{
    public class CertificateResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
    }
}