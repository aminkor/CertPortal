using System.Collections.Generic;
using CertPortal.Contracts;
using CertPortal.Models;

namespace CertPortal.IServices
{
    public interface IUserService
    {
        User Authenticate(string username, string password);
        IEnumerable<User> GetAll();
        User GetById(int id);
        User Create(User user, string password);
        void Update(User user, string password = null);
        void Delete(int id);
    }
}