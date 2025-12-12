using Infra.Models;

namespace Infra.Services.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateJWTToken(UserEntity user);
    }
}