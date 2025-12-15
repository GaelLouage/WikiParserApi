using Infra.Helpers;
using Infra.Models;
using Infra.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Infra.Extensions;

namespace WikiParserApi.Controllers
{
    [EnableRateLimiting("fixed")]
    [ApiController]
    [Route("api/rest_v1/[controller]")]
    public class LoginController : ControllerBase
    {

        private readonly IJwtTokenService _jwtTokenService;

        public LoginController(IJwtTokenService jwtTokenService)
        {
            _jwtTokenService = jwtTokenService;
        }

       [HttpPost("login")]
       public IActionResult Login([FromBody]UserEntity user)
       {
            if (string.IsNullOrEmpty(user.UserName) || 
                string.IsNullOrEmpty(user.Email) ||
                string.IsNullOrEmpty(user.Password))
            {
               
                return  BadRequest("User data required");

            }

            var isEmailValid = EmailHelpers.IsValidEmail(user.Email);
            if (isEmailValid is false)
            {
                return BadRequest($"Invalid email: {user.Email}");
            }
            //dummy test
            var hashedUserPassword = user.Password.HashPassword();
            //compare password
            var verifyPassword = user.Password.VerifyPassword(hashedUserPassword);
            if (verifyPassword is false)
            {
                return NotFound("Wrong username of password.");
            }
            //TODO: db connection (mongodb)


            var jwToken = _jwtTokenService.GenerateJWTToken(user);
            return Ok(jwToken);
       }
       
    }
}
