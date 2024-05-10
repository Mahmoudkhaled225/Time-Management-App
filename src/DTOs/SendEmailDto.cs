using src.Entities;

namespace src.DTOs;

public class SendEmailDto
{
    public int Id { get; set; }
    
    public string Email { get; set; } = string.Empty;
    
    public Roles Role { get; set; }
}