using System;

namespace CertPortal.Models.Institutions
{
    public class InstitutionResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public string StudentsCounts { get; set; }
        public string CertificatesCounts { get; set; }

    }
}