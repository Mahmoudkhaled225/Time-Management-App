using Newtonsoft.Json;

namespace src.Entities;

public class ToDo : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    
    // also Description
    public string? Content { get; set; } = string.Empty;
    
    public Status Status { get; set; } = Status.Pending;
    
    // one to many relationship with user
    public int UserId { get; set; }
    
    // [JsonIgnore] 
    public User User { get; set; } = null!;
}


public enum Status
{
    Pending,
    Done
}