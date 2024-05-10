using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using src.B_UnitOfWork;
using src.DTOs;

namespace src.Services;

public class AuthService : IAuthService
{
    private readonly Jwt _jwt;    
    private readonly IUnitOfWork _unitOfWork;
    public AuthService(IOptions<Jwt> jwt, IUnitOfWork unitOfWork)
    {
        this._jwt = jwt.Value;        
        this._unitOfWork = unitOfWork;
    }
    

    private ClaimsPrincipal GetTokenPrincipal(string token)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var validation = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = securityKey,
            ValidateIssuer = true,
            ValidIssuer = _jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwt.Audience,
            RequireExpirationTime = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        return new JwtSecurityTokenHandler().ValidateToken(token, validation, out _);
    }
    public int? GetUserIdFromToken(string token)
    {
        var principal = GetTokenPrincipal(token);
        var userId = principal?.Claims?.FirstOrDefault(c => c.Type == "Id")?.Value;
        return userId != null ? int.Parse(userId) : null;
    }
    public string GenerateTokenString(GenerateAccessTokenDto dto)
    {
        /*List<Claim>*/ var claims = new List<Claim>
        {
            new (ClaimTypes.Role, dto.Role.ToString()),
            new Claim("Id", dto.Id.ToString())
        };
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
        
        /*SecurityTokenDescriptor*/ var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer= _jwt.Issuer,
            Audience= _jwt.Audience,
            Subject = new ClaimsIdentity(claims), 
            Expires = DateTime.UtcNow.AddHours(_jwt.DurationInHours),
            SigningCredentials = signingCredentials
        };
        
        /*JwtSecurityTokenHandler*/var tokenHandler = new JwtSecurityTokenHandler();
        /*SecurityToken*/ var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
        // OR
        // string tokenString = new JwtSecurityTokenHandler().WriteToken(securityToken);
        // return tokenString;
    }
    public string GenerateRefreshTokenString()
    {
        var randomNumber = new byte[64];
        using (var numberGenerator = RandomNumberGenerator.Create())
            numberGenerator.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
    public string GenerateUndoSoftDeleteCode()
    {
        var random = new Random();
        return random.Next(10000, 99999).ToString();
    }
    public async Task<RefreshTokenDto> RegenerateRefreshToken(string accessToken)
    {
        var userId = GetUserIdFromToken(accessToken);
        if (userId == null)
            throw new SecurityTokenException("Invalid token");
        
        // problem here
        var user = await _unitOfWork.UserRepository.Get(userId);
        if (user == null)
            throw new SecurityTokenException("Invalid token");
        
        var generateAccessTokenDto = new GenerateAccessTokenDto
        {
            Id = user.Id,
            Role = user.Role
        };
        
        // Generate a new access token and refresh token
        var newAccessToken = GenerateTokenString(generateAccessTokenDto);
        var newRefreshToken = GenerateRefreshTokenString();
        
        return new RefreshTokenDto { AccessToken = newAccessToken, RefreshToken = newRefreshToken };
    }

    
}