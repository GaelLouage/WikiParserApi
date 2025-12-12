using Infra.Models;
using Infra.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Services.Classes
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration? _configuration;
        public JwtTokenService(IConfiguration? configuration)
        {
            _configuration = configuration;
        }

        public string GenerateJWTToken(UserEntity user)
        {
            var claims = new List<Claim> {
                         new Claim(ClaimTypes.NameIdentifier, user.Email),
                         new Claim(ClaimTypes.UserData, user.Email),
                         new Claim(ClaimTypes.Role, user.RoleType),
                     };
            var jwtToken = new JwtSecurityToken(
                claims: claims,
                issuer: _configuration["ApplicationSettings:JWT_Issuer"],
                audience: _configuration["ApplicationSettings:JWT_Audience"],
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(
                       Encoding.UTF8.GetBytes(_configuration["ApplicationSettings:JWT_Secret"])
                        ),
                    SecurityAlgorithms.HmacSha256Signature)
                );
            var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            return token;
        }
    }
}
