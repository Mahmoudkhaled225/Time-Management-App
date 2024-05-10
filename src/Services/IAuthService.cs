using System.Security.Claims;
using src.DTOs;

namespace src.Services;

public interface IAuthService 
{
    int? GetUserIdFromToken(string token);

    string GenerateTokenString(GenerateAccessTokenDto user);
    string GenerateRefreshTokenString();
    string GenerateUndoSoftDeleteCode();
    Task<RefreshTokenDto> RegenerateRefreshToken(string modelAccessToken);
    

}