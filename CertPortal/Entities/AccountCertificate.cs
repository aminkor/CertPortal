namespace CertPortal.Entities
{
    public partial class AccountCertificate
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int CertificateId { get; set; }
        public virtual Account Account { get; set; }
        public virtual Certificate Certificate { get; set; }

    }
}