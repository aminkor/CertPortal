using System;

namespace CertPortal.Entities
{
    public class Certificate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string FileName { get; set; }

        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public int? InstitutionId { get; set; }
        public int? AccountId { get; set; }

        public virtual Institution Institution { get; set; }
        public virtual Account Account { get; set; }


    }
}