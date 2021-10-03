using System;
using System.Collections.Generic;

namespace CertPortal.Entities
{
    public class Institution
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public virtual ICollection<InstitutionStudent> InstitutionStudent { get; set; }
    }
}