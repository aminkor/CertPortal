using System.Collections.Generic;
using CertPortal.Entities;
using CertPortal.Models.Institutions;

namespace CertPortal.Models.Accounts
{
    public class UserRoleResponse
    {
        public int AccountId { get; set; }
        public string RoleName { get; set; }
        public List<InstitutionResponse> Institutions { get; set; }

        public UserRoleResponse()
        {
            Institutions = new List<InstitutionResponse>();
        }
    }
}