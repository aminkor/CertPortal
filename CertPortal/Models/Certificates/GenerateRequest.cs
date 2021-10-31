using System;
using System.Collections.Generic;

namespace CertPortal.Models.Certificates
{
    public class GenerateRequest
    {
        public DateTime IssuedDateInput { get; set; }
        public DateTime ExpiryDateInput { get; set; }
        public int? StudentId { get; set; }
        public int? InstitutionId { get; set; }
        public List<int> StudentIds { get; set; }
        public string CourseName { get; set; }
        public string IssuedDate { get; set; }
        public string ExpiryDate { get; set; }
        public int TemplateId { get; set; }
        public string Organization { get; set; }



        public void ParseDates()
        {
            if (IssuedDate != null && ExpiryDate != null)
            {
                IssuedDateInput = DateTime.ParseExact(IssuedDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                ExpiryDateInput = DateTime.ParseExact(ExpiryDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            }
        }
    }
}