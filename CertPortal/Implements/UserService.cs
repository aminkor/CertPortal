using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using AttendanceTracker.Helpers;
using CertPortal.Contracts;
using CertPortal.Helpers;
using CertPortal.IServices;
using CertPortal.Models;
using CertPortal.Models.Repository;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CertPortal.Implements
{
    public class UserService : IUserService
    {
        
        private readonly IDataRepository<User> _usersRepo;
        private readonly AppSettings _appSettings;

        public UserService(IOptions<AppSettings> appSettings, IDataRepository<User> usersRepo)
        {
            _usersRepo = usersRepo;
            _appSettings = appSettings.Value;
        }

        public User Authenticate(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return null;

            var user = _usersRepo.GetAll().SingleOrDefault(x => x.Email == email);

            // check if username exists
            if (user == null)
                return null;

            // check if password is correct
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            // authentication successful
            return user;
        }

        public IEnumerable<User> GetAll()
        {
            return _usersRepo.GetAll();
        }

        public User GetById(int id)
        {
            return _usersRepo.GetAll().Where(user => user.Id == id).FirstOrDefault();
        }
        
        public User Create(User user, string password)
        {
            // validation
            if (string.IsNullOrWhiteSpace(password))
                throw new AppException("Password is required");

            if (_usersRepo.GetAll().Where((x => x.Email == user.Email)).FirstOrDefault() != null)
                throw new AppException("Username \"" + user.Username + "\" is already taken");

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _usersRepo.InsertOnCommit(user);
            _usersRepo.CommitChanges();

            return user;
        }
        
        public void Update(User userParam, string password = null)
        {
            var user = _usersRepo.GetAll().FirstOrDefault(user => user.Id == userParam.Id);

            if (user == null)
                throw new AppException("User not found");

            // update username if it has changed
            if (!string.IsNullOrWhiteSpace(userParam.Email) && userParam.Email != user.Email)
            {
                // throw error if the new username is already taken
                if (_usersRepo.GetAll().Where(x => x.Email == userParam.Email).FirstOrDefault() != null)
                    throw new AppException("Username " + userParam.Username + " is already taken");

                user.Email = userParam.Email;
            }

            // update user properties if provided
            if (!string.IsNullOrWhiteSpace(userParam.FirstName))
                user.FirstName = userParam.FirstName;

            if (!string.IsNullOrWhiteSpace(userParam.LastName))
                user.LastName = userParam.LastName;

            // update password if provided
            if (!string.IsNullOrWhiteSpace(password))
            {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash(password, out passwordHash, out passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            _usersRepo.UpdateOnCommit(user);
            _usersRepo.CommitChanges();
        }
        
        public void Delete(int id)
        {
            var user = _usersRepo.GetAll().Where(user => user.Id == id).FirstOrDefault();
            if (user != null)
            {
                _usersRepo.DeleteOnCommit(user);
                _usersRepo.CommitChanges();
            }
        }
        
        // helper methods

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }


    }
}