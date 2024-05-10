using src.DTOs;

namespace src;

public interface IRefreshTokenProvider
{ 
    string GetRefreshToken();
}

public class RefreshTokenStringProvider : IRefreshTokenProvider
{
    private readonly string _refreshToken;

    public RefreshTokenStringProvider(string refreshToken)
    {
        _refreshToken = refreshToken;
    }

    public string GetRefreshToken()
    {
        return _refreshToken;
    }
}

public class RefreshTokenDtoProvider : IRefreshTokenProvider
{
    private readonly RefreshTokenDto _refreshTokenDto;

    public RefreshTokenDtoProvider(RefreshTokenDto refreshTokenDto)
    {
        _refreshTokenDto = refreshTokenDto;
    }

    public string GetRefreshToken()
    {
        return _refreshTokenDto.RefreshToken;
    }
}