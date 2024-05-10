using System.ComponentModel.DataAnnotations;

namespace src.DTOs;

public class ChangePasswordDto
{
    public string OldPassword { get; set; } = string.Empty;
    
    [Compare("ConfirmPassword", ErrorMessage = "New password and confirmation password do not match.")]
    public string NewPassword { get; set; } = string.Empty;
    
    public string ConfirmPassword { get; set; } = string.Empty;
}