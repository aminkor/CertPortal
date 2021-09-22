using AutoMapper;
using CertPortal.Entities;
using CertPortal.Models.Accounts;
using CertPortal.Models.Institutions;
using CreateRequest = CertPortal.Models.Accounts.CreateRequest;
using UpdateRequest = CertPortal.Models.Accounts.UpdateRequest;
using InstitutionCreateRequest = CertPortal.Models.Institutions.CreateRequest;
using InstitutionUpdateRequest = CertPortal.Models.Institutions.UpdateRequest;

namespace CertPortal.Helpers
{
    public class AutoMapperProfile : Profile
    {
        // mappings between model and entity objects
        public AutoMapperProfile()
        {
            CreateMap<Account, AccountResponse>();

            CreateMap<Account, AuthenticateResponse>();

            CreateMap<RegisterRequest, Account>();

            CreateMap<CreateRequest, Account>();

            CreateMap<UpdateRequest, Account>()
                .ForAllMembers(x => x.Condition(
                    (src, dest, prop) =>
                    {
                        // ignore null & empty string properties
                        if (prop == null) return false;
                        if (prop.GetType() == typeof(string) && string.IsNullOrEmpty((string)prop)) return false;

                        // ignore null role
                        if (x.DestinationMember.Name == "UserRole" && src.Role == null) return false;

                        return true;
                    }
                ));
            
            CreateMap<Institution, InstitutionResponse>();

            CreateMap<InstitutionCreateRequest, Institution>();

            CreateMap<InstitutionUpdateRequest, Institution>()
                .ForAllMembers(x => x.Condition(
                    (src, dest, prop) =>
                    {
                        // ignore null & empty string properties
                        if (prop == null) return false;
                        if (prop.GetType() == typeof(string) && string.IsNullOrEmpty((string)prop)) return false;

                     
                        return true;
                    }
                ));
        }
    }
}