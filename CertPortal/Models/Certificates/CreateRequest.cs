namespace CertPortal.Models.Certificates
{
    public class CreateRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string FileName { get; set; }
        public int? InstitutionId { get; set; }
        public int? AccountId { get; set; }

    }
}