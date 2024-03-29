namespace CertPortal.Helpers
{
    public class AppSettings
    {
        public string AZURE_STORAGE_CONNECTION_STRING { get; set; }
        public string Secret { get; set; }

        // refresh token time to live (in days), inactive tokens are
        // automatically deleted from the database after this time
        public int RefreshTokenTTL { get; set; }

        public string EmailFrom { get; set; }
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUser { get; set; }
        public string SmtpPass { get; set; }
        public string UploadServerUrl { get; set; }
        public string UploadServerDir { get; set; }

    }
}