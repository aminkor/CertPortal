using System.Collections.Generic;

namespace CertPortal.Models.Accounts
{
    public class RoleInstitutionUpdateRequest
    {
        public int AccountId { get; set; }
        public List<int> InstitutionIds { get; set; }

    }
}