using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using src.entity;

namespace src.Entities;

public class User : BaseEntity
{

    [MinLength(2)]
    public string UserName { get; set; } = string.Empty;

    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    // [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$")]
    // here don't put on it any validation
    // cause we will hash it
    // but we will validate it in the service
    public string Password { get; set; } = string.Empty;

    [Required]
    public DateTime Dob { get; set; } 

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public int Age => DateTime.Now.Year - Dob.Year;

    // @ character creates verbatim string literals
    [RegularExpression(@"^\+201[0125][0-9]{9}$")]

    public string Phone { get; set; }  

    public bool? IsConfirmed { get; set; } = false;

    public bool? IsDeleted { get; set; } = false;

    
    public string? ImgUrl { get; set; }
    public string? PublicId { get; set; }

    [MinLength(5)]
    [MaxLength(5)]
    public string? UndoIsDeletedCode { get; set; }
    
    public string? RefreshToken { get; set; } = null!;
    
    public DateTime? RefreshTokenExpiryTime { get; set; } = null!;
    public Roles Role { get; set; } = Roles.User;
    
    
    
    // [JsonIgnore]
    public List<ToDo> ToDos { get; set; } = null!;

}

public enum Roles
{
    Admin,
    User,
    Killer
}