using src.DTOs.ToDos;

namespace src.DTOs;

public class ReturnedUser
{
    public int Id { get; set; }
    
    public string UserName { get; set; } 
    
    public string Email { get; set; } 
    
    //age
    
    public int Age { get; set; }
    
    public string Phone { get; set; }
    
    public bool? IsConfirmed { get; set; }
    
    public bool? IsDeleted { get; set; }
    
    public string? ImgUrl { get; set; }
    
    public string? PublicId { get; set; }
    
    public string? UndoIsDeletedCode { get; set; }
    
    public string? RefreshToken { get; set; }
    
    public DateTime? RefreshTokenExpiryTime { get; set; }
    
    public string Role { get; set; }
    
    public List<ReturnedToDo> ToDos { get; set; }
    
    
}