using src.Entities;

namespace src.DTOs;

public class GenerateAccessTokenDto
{
    public int Id { get; set; }
    
    public Roles Role { get; set; }
}