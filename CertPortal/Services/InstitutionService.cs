using System;
using System.Collections.Generic;
using AutoMapper;
using CertPortal.Entities;
using CertPortal.Helpers;
using CertPortal.Models.Institutions;

namespace CertPortal.Services
{
    public interface IInstitutionService
    {
        IEnumerable<InstitutionResponse> GetAll();
        InstitutionResponse GetById(int id);
        InstitutionResponse Create(CreateRequest model);
        InstitutionResponse Update(int id, UpdateRequest model);
        void Delete(int id);


    }
    public class InstitutionService : IInstitutionService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public InstitutionService(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IEnumerable<InstitutionResponse> GetAll()
        {
            var institutions = _context.Institutions;
            return _mapper.Map<IList<InstitutionResponse>>(institutions);
        }
        
        public InstitutionResponse GetById(int id)
        {
            var institution = getInstitution(id);
            return _mapper.Map<InstitutionResponse>(institution);
        }
        
        public InstitutionResponse Create(CreateRequest model)
        {
       
            // validate
            // if (_context.Accounts.Any(x => x.Email == model.Email))
            //     throw new AppException($"Email '{model.Email}' is already registered");

            // map model to new account object
            var institution = _mapper.Map<Institution>(model);
            institution.Created = DateTime.UtcNow;

          

            // save account
            _context.Institutions.Add(institution);
            _context.SaveChanges();

            return _mapper.Map<InstitutionResponse>(institution);
        }
        
        public InstitutionResponse Update(int id, UpdateRequest model)
        {
            var institution = getInstitution(id);

            // validate
            // if (account.Email != model.Email && _context.Accounts.Any(x => x.Email == model.Email))
            //     throw new AppException($"Email '{model.Email}' is already taken");

   
            // copy model to account and save
            _mapper.Map(model, institution);
            institution.Updated = DateTime.UtcNow;
            _context.Institutions.Update(institution);
            _context.SaveChanges();

            return _mapper.Map<InstitutionResponse>(institution);
        }
        
        public void Delete(int id)
        {
            var institution = getInstitution(id);
            _context.Institutions.Remove(institution);
            _context.SaveChanges();
        }

        
        private Institution getInstitution(int id)
        {
            var institution = _context.Institutions.Find(id);
            if (institution == null) throw new KeyNotFoundException("Institution not found");
            return institution;
        }
    }
}