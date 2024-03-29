﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using CertPortal.Entities;
using CertPortal.Helpers;
using CertPortal.Models.Accounts;
using CertPortal.Services;

namespace CertPortal.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountsController : BaseController
    {
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;
        private readonly DataContext _context;


        public AccountsController(
            IAccountService accountService,
            IMapper mapper, DataContext context)
        {
            _accountService = accountService;
            _mapper = mapper;
            _context = context;
        }

        [HttpPost("authenticate")]
        public ActionResult<AuthenticateResponse> Authenticate(AuthenticateRequest model)
        {
            var response = _accountService.Authenticate(model, ipAddress());
            setTokenCookie(response.RefreshToken);
            return Ok(response);
        }

        [HttpPost("refresh-token")]
        public ActionResult<AuthenticateResponse> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var response = _accountService.RefreshToken(refreshToken, ipAddress());
            setTokenCookie(response.RefreshToken);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("revoke-token")]
        public IActionResult RevokeToken(RevokeTokenRequest model)
        {
            // accept token from request body or cookie
            var token = model.Token ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Token is required" });

            // users can revoke their own tokens and admins can revoke any tokens
            if (!Account.OwnsToken(token) && Account.UserRole != UserRole.Admin)
                return Unauthorized(new { message = "Unauthorized" });

            _accountService.RevokeToken(token, ipAddress());
            return Ok(new { message = "Token revoked" });
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterRequest model)
        {
            _accountService.Register(model, Request.Headers["origin"]);
            return Ok(new { message = "Registration successful, please check your email for verification instructions" });
        }

        [HttpPost("verify-email")]
        public IActionResult VerifyEmail(VerifyEmailRequest model)
        {
            _accountService.VerifyEmail(model.Token);
            return Ok(new { message = "Verification successful, you can now login" });
        }

        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword(ForgotPasswordRequest model)
        {
            _accountService.ForgotPassword(model, Request.Headers["origin"]);
            return Ok(new { message = "Please check your email for password reset instructions" });
        }

        [HttpPost("validate-reset-token")]
        public IActionResult ValidateResetToken(ValidateResetTokenRequest model)
        {
            _accountService.ValidateResetToken(model);
            return Ok(new { message = "Token is valid" });
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword(ResetPasswordRequest model)
        {
            _accountService.ResetPassword(model);
            return Ok(new { message = "Password reset successful, you can now login" });
        }

        [Authorize(UserRole.Admin)]
        [HttpGet]
        public ActionResult<IEnumerable<AccountResponse>> GetAll([FromQuery] string filter = null)
        {
            var accounts = _accountService.GetAll(filter);
            return Ok(accounts);
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public ActionResult<AccountResponse> GetById(int id)
        {
            // users can get their own account and admins can get any account
            if (id != Account.Id && Account.UserRole != UserRole.Admin)
                return Unauthorized(new { message = "Unauthorized" });

            var account = _accountService.GetById(id);
            return Ok(account);
        }

        [Authorize]
        [HttpPost]
        public ActionResult<AccountResponse> Create(CreateRequest model)
        {
            List<UserRole> authorizedRoles = new List<UserRole> { UserRole.Admin , UserRole.Instructor};
            if (authorizedRoles.Contains(Account.UserRole) == false)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            if (Account.UserRole.ToString() == "Instructor")
            {
                // limits user creation to its institution only
                if (_context.RoleInstitutions.Any(role =>
                    role.AccountId == Account.Id && role.InstitutionId == model.InstitutionId) == false)
                    return Unauthorized(new { message = "Unauthorized" });
            }

            
            
            var account = _accountService.Create(model);
            return Ok(account);
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public ActionResult<AccountResponse> Update(int id, UpdateRequest model)
        {
            // users can update their own account and admins can update any account
            List<UserRole> authorizedRoles = new List<UserRole> { UserRole.Admin , UserRole.Instructor};
            if (id != Account.Id  && authorizedRoles.Contains(Account.UserRole) == false)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }
            // else if (id != Account.Id && Account.UserRole != UserRole.Admin)
            // {
            //     return Unauthorized(new { message = "Unauthorized" });
            //
            // }
            // limits user creation to its institution only
            // if (_context.RoleInstitutions.Any(role =>
            //     role.AccountId == Account.Id && role.InstitutionId == model.InstitutionId) == false)
            //     return Unauthorized(new { message = "Unauthorized" });
            
           
            // only admins can update role
            if (Account.UserRole != UserRole.Admin)
                model.UserRole = null;

            var account = _accountService.Update(id, model);
            return Ok(account);
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            // users can delete their own account and admins can delete any account
            if (id != Account.Id && Account.UserRole != UserRole.Admin)
                return Unauthorized(new { message = "Unauthorized" });

            _accountService.Delete(id);
            return Ok(new { message = "Account deleted successfully" });
        }
        
        [Authorize]
        [HttpGet("Students/{instutitionId:int}")]
        public IActionResult GetStudents(int instutitionId, [FromQuery] int forCert = 0)
        {
            // users can delete their own account and admins can delete any account
            // TODO return unauthorized if non admin trying to get students list, or the user no institution role on that
            // resource
            // if (id != Account.Id && Account.UserRole != UserRole.Admin)
            //     return Unauthorized(new { message = "Unauthorized" });

            var students = _accountService.GetStudents(instutitionId, forCert);
            return Ok(students);
        }
        
        [Authorize]
        [HttpGet("Roles/{accountId:int}")]
        public IActionResult GetUserRoles(int accountId)
        {
            // users can delete their own account and admins can delete any account
            // TODO return unauthorized if non admin trying to get students list, or the user no institution role on that
            // resource
            // if (id != Account.Id && Account.UserRole != UserRole.Admin)
            //     return Unauthorized(new { message = "Unauthorized" });

            var userRoles = _accountService.GetUserRoles(accountId);
            return Ok(userRoles);
        }
        
        [Authorize]
        [HttpPut("Roles/Institutions")]
        public IActionResult UpdateRoleInstitutions(RoleInstitutionUpdateRequest request)
        {
            // users can delete their own account and admins can delete any account
            // TODO return unauthorized if non admin trying to get students list, or the user no institution role on that
            // resource
            // if (id != Account.Id && Account.UserRole != UserRole.Admin)
            //     return Unauthorized(new { message = "Unauthorized" });

            _accountService.UpdateRoleInstitutions(request);
            return Ok(new { message = "Institution Updated Succesfully" });
        }
        
        [Authorize]
        [HttpGet("instructors/{instructorId:int}")]
        public IActionResult GetInstructorStudents(int instructorId)
        {
            // users can delete their own account and admins can delete any account
            // TODO return unauthorized if non admin trying to get students list, or the user no institution role on that
            // resource
            // if (id != Account.Id && Account.UserRole != UserRole.Admin)
            //     return Unauthorized(new { message = "Unauthorized" });

            var students = _accountService.GetInstructorStudents(instructorId);
            return Ok(students);
        }

        // helper methods

        private void setTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        private string ipAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
    }
}
