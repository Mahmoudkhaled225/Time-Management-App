using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using src.B_UnitOfWork;
using src.DTOs;
using src.Entities;
using src.Services;

namespace src.Controllers;

public class AuthController : BaseController
{   
    private readonly IMapper _mapper;
    private readonly ILogger _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISmsService _smsService;
    private readonly IAuthService _authService;
    private readonly IEmailService _emailService;
    private readonly IUploadImgService _uploadImgService;

     
    public AuthController(IMapper mapper, ILogger<AuthController> logger, IUnitOfWork unitOfWork, ISmsService smsService, IAuthService authService, IEmailService emailService, IUploadImgService uploadImgService)
    {
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _smsService = smsService;
        _authService = authService;
        _emailService = emailService;
        _uploadImgService = uploadImgService;
    }

    /// /////////////////   Helper Methods    /////////////////////////

    private User CreateUser(RegisterUser model, string? imgUrl, string? publicId)
    {
        return new User
        {
            UserName = model.UserName,
            Email = model.Email,
            Password = Hashing.HashPass(model.Password),
            Dob = model.Dob,
            ImgUrl = imgUrl,
            PublicId = publicId,
            Phone = model.Phone
        };
    }
    private async Task<bool> SendVerificationEmail(SendEmailDto dto)
    {
        var token = _authService.GenerateTokenString(new GenerateAccessTokenDto { Id = dto.Id, Role = dto.Role });
        var link = Url.Action("VerifyEmail", "Auth", new { token }, Request.Scheme);
        var body = $"Please click on the link to verify your email {link} Verify Email</a>";
        return await _emailService.SendEmail(dto.Email, "Email Verification", body);
    }
    private async Task<(string? imgUrl, string? publicId)> UploadUserImage(IFormFile? image)
    {
        if (image == null)
            return (null, null);
        var uploadResult = await _uploadImgService.UploadImg(image);
        return uploadResult != null ? (uploadResult.Url.ToString(), uploadResult.PublicId) : (null, null);
    }
    // private int? GetUserIdFromToken(string token)
    // {
    //     var principal = _authService.GetTokenPrincipal(token);
    //     var userId = principal?.Claims?.FirstOrDefault(c => c.Type == "Id")?.Value;
    //     return userId != null ? int.Parse(userId) : null;
    // }
    private void VerifyUserEmail(User user)
    {
        user.IsConfirmed = true;
        _unitOfWork.UserRepository.Update(user); 
    }
    private void UpdateUserRefreshToken(User user, IRefreshTokenProvider refreshTokenProvider)
    {
        user.RefreshToken = refreshTokenProvider.GetRefreshToken();
        user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
        _unitOfWork.UserRepository.Update(user);
    }
    private async Task<User?> ValidateUser(LoginUser user)
    {
        var checkUser = await _unitOfWork.UserRepository.FindUserByEmailAsync(user.Email);
        if (checkUser is null)
        {
            _logger.LogInformation($" Register first User with email {user.Email} not found");
            return null;
        }

        if (checkUser.IsConfirmed is false)
        {
            _logger.LogInformation($" Verification and isConfirmed User with email {user.Email} has not verified their email");
            return null;
        }

        if (checkUser.IsDeleted.GetValueOrDefault())
        {
            _logger.LogInformation($" SoftDelete User with email {user.Email} is deleted");
            return null;
        }

        if (Hashing.VerifyPassword(user.Password, checkUser.Password) is false)
        {
            _logger.LogInformation($"Invalid password for user with email {user.Email}");
            return null;
        }

        _logger.LogInformation($"User with email {user.Email} validated successfully");
        return checkUser;
    }
    private LogInResponse LogInTokensLogic(User user)
    {
        GenerateAccessTokenDto tokenDto = new()
        {
            Id = user.Id, 
            Role = user.Role
        };
        var accessToken = _authService.GenerateTokenString(tokenDto);
        var refreshToken = _authService.GenerateRefreshTokenString();
        
        return new LogInResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }
    private void SoftDeleteUser(User user)
    {
        user.IsDeleted = true;
        user.UndoIsDeletedCode = _authService.GenerateUndoSoftDeleteCode();
    }
    private void  UndoSoftDeleteUser(User user)
    {
        user.IsDeleted = false;
        user.UndoIsDeletedCode = null;
    }
    
    
    /// ////////////////////////////   APIs   ///////////////////////////////     
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterUser([FromForm] RegisterUser model)
    {
        var checkUser = await _unitOfWork.UserRepository.FindUserByEmailAsync(model.Email);
        if (checkUser != null)
            return BadRequest("User already exists, please login or use another email");
        
        var (imgUrl, publicId) = await UploadUserImage(model.Image);
        
        var user = CreateUser(model, imgUrl, publicId);
        
        await _unitOfWork.UserRepository.Add(user);
        
        var checkSave = await _unitOfWork.Save();
        if (checkSave != 1)
            return BadRequest("An error occurred while registering the user please try again");
        
        var sendEmailDto = new SendEmailDto
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role
        };
        
        var check = await SendVerificationEmail(sendEmailDto);
        if (!check)
            return BadRequest("An error occurred while sending the email please try again");
        return Ok("Check your email for verification");
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpGet("VerifyEmail/{token}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromRoute]string token)
    {
        var userId = _authService.GetUserIdFromToken(token);
        if (userId is null)
            return BadRequest("Invalid token");

        var user = await _unitOfWork.UserRepository.Get(userId);
        if (user is null)
            return NotFound("No user found with this email. Please register first.");

        VerifyUserEmail(user);
        var check = await _unitOfWork.Save();
        if (check != 1)
            return BadRequest("An error occurred while verifying the email. Please try again.");

        return Ok("Email verified successfully. Now you can login.");
    }


    [HttpPost("Login")]
    [ProducesResponseType(typeof(LogInResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Login([FromBody] LoginUser user)
    {
        var checkUser = await ValidateUser(user);
        if (checkUser is null)
            return BadRequest("Invalid email or password");
        
        var tokens = LogInTokensLogic(checkUser);
        
        UpdateUserRefreshToken(checkUser, new RefreshTokenStringProvider(tokens.RefreshToken));

        var check = await _unitOfWork.Save();
        if (check != 1)
            return BadRequest("An error occurred while updating the user please try again");
        return Ok(new { tokens });
    }



    [HttpPost("RefreshToken")]
    [ProducesResponseType(typeof(LogInResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto model)
    {
        var loginResult = await _authService.RegenerateRefreshToken(model.AccessToken);
        var userId = _authService.GetUserIdFromToken(model.AccessToken);
        if (userId is null)
            return BadRequest("Invalid token");
        
        var user = await _unitOfWork.UserRepository.Get(userId);
        if (user is null)
            return NotFound("no user found with this email please register first");
        
        UpdateUserRefreshToken(user, new RefreshTokenDtoProvider(loginResult));        
        var check = await _unitOfWork.Save();
        if (check != 1)
            return BadRequest("An error occurred while updating the user please try again");
        return Ok(new { loginResult });
    }
    
    [Authorize]
    [HttpGet("softDelete/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SoftDeleteUser([FromRoute] int id)
    {
        var user = await _unitOfWork.UserRepository.Get(id);        
        if (user is null)
            return NotFound("No user found with this id");
        
        SoftDeleteUser(user);
        
        _unitOfWork.UserRepository.Update(user);
        var check = await _unitOfWork.Save();
        if (check != 1)
            return BadRequest("An error occurred while soft deleting the user please try again");
        return Ok("User Soft deleted successfully");
    }
    
    //Undo send undo soft delete code to user phone 
    [HttpGet("activate/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendSoftDeleteCode([FromRoute] int id)
    {
        var user = await _unitOfWork.UserRepository.Get(id);        
        if (user is null ) 
            return BadRequest("No user found with this id");
        if (user.IsDeleted is false)
            return BadRequest("User is not soft deleted so login normally");
        
        var check = await _smsService.Send(user.Phone, user.UndoIsDeletedCode);
        if (check is null)
            return BadRequest("An error occurred while sending the code please try again");
        return Ok("Code sent successfully");
    }
    
    //Undo soft delete user
    [HttpGet("activate/{id}/{code}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UndoSoftDeleteUser([FromRoute] int id, [FromRoute] string code)
    {
        var user = await _unitOfWork.UserRepository.Get(id);
        if (user is null)
            return BadRequest("No user found with this id");
        if (user.IsDeleted is false)
            return BadRequest("User is not soft deleted");
        if (user.UndoIsDeletedCode != code)
            return BadRequest("Invalid code");
        
        UndoSoftDeleteUser(user);
        
        _unitOfWork.UserRepository.Update(user);
        var check = await _unitOfWork.Save();
        if (check != 1)
            return BadRequest("An error occurred while undoing the soft delete please try again");
        return Ok("User is active now , login now ");
    }
    
    [Authorize]
    [HttpDelete("delete/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteUser([FromRoute] int id)
    {
        var user = await _unitOfWork.UserRepository.Get(id);
        if (user is null)
            return BadRequest("No user found with this id");
        
        _unitOfWork.UserRepository.Delete(user);
        var check = await _unitOfWork.Save();
        if (check != 1)
            return BadRequest("An error occurred while deleting the user please try again");
        return Ok("User deleted successfully");
    }


    [Authorize]
    [HttpPatch("changepassword")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
    {

        var preToken = Request.Headers["Authorization"];
        //lets remove the part "Bearer " from the token
        var token = preToken.ToString()[7..];
        
        var userId = _authService.GetUserIdFromToken(token);
        if (userId is null)
            return BadRequest("Invalid token");
        
        var user = await _unitOfWork.UserRepository.Get(userId);
        if (user is null)
            return BadRequest("No user found with this id");
        
        if (Hashing.VerifyPassword(model.OldPassword, user.Password) is false)
            return BadRequest("Invalid old password");
        
        user.Password = Hashing.HashPass(model.NewPassword);
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        _unitOfWork.UserRepository.Update(user);
        var check = await _unitOfWork.Save();
        if (check != 1)
            return BadRequest("An error occurred while deleting the user please try again");
        return Ok("Password changed successfully");
    }
    
    [Authorize]
    [HttpGet("profile")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Profile()
    {
        var preToken = Request.Headers["Authorization"];
        //lets remove the part "Bearer " from the token
        var token = preToken.ToString()[7..];
        
        var userId = _authService.GetUserIdFromToken(token);
        if (userId is null)
            return BadRequest("Invalid token");

        var user = await _unitOfWork.UserRepository.GetWithToDos(userId);
        if (user is null)
            return NotFound("No user found with this id");
        
        return Ok(user);
    }
    
}


