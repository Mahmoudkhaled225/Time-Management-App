using System.ComponentModel.DataAnnotations;
using src.Entities;

namespace src.DTOs;


public class RegisterUser
{
    [MinLength(2)]
    [Required]

    public string UserName { get; set; }  = string.Empty;
 
    [EmailAddress]
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public DateTime Dob { get; set; } 

    public IFormFile? Image { get; set; }
    
    public string Phone { get; set; }
    
    public Roles Role { get; set; } = Roles.User;
}