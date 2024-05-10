using src.Entities;

namespace src.DTOs.ToDos;

public class ReturnedToDo
{
    public string Title { get; set; }
    
    public Status Status { get; set; }
}