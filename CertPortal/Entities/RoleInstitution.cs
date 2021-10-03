using System;

namespace CertPortal.Entities
{
    public class RoleInstitution
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int InstitutionId { get; set; }
      
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public virtual Account Account { get; set; }
        public virtual Institution Institution { get; set; }
    }
}