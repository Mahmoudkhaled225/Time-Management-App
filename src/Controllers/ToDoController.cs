using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using src.B_UnitOfWork;
using src.DTOs.ToDos;
using src.Entities;
using src.Services;

namespace src.Controllers;



public class ToDoController : BaseController
{
    private readonly ILogger _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;

    
    public ToDoController(ILogger<ToDoController> logger, IUnitOfWork unitOfWork, IAuthService authService)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _authService = authService;
    }

    
    [Authorize]
    [HttpPost("add")]
    public async Task<IActionResult> Add([FromBody]AddToDo dto)
    {
        var preToken = Request.Headers["Authorization"];
        var token = preToken.ToString()[7..];
        
        var userId = _authService.GetUserIdFromToken(token);
        if (userId is null)
            return BadRequest("Invalid token");
        
        var user = await _unitOfWork.UserRepository.Get(userId);
        if (user is null)
            return NotFound("User not found");

        var todo = new ToDo
        {
            Title = dto.Title,
            Content = dto.Content,
            UserId = user.Id
        };

        await _unitOfWork.ToDoRepository.Add(todo);
        await _unitOfWork.Save();
        return Ok("ToDo added successfully");
    }
    
    
    [Authorize]
    [HttpPatch("complete/{id}")]
    public async Task<IActionResult> Complete([FromRoute]int id)
    {
        var preToken = Request.Headers["Authorization"];
        var token = preToken.ToString()[7..];
        var userId = _authService.GetUserIdFromToken(token);
        if (userId is null)
            return Unauthorized("Invalid token");

        var todo = await _unitOfWork.ToDoRepository.Get(id);
        if (todo is null)
            return NotFound("ToDo not found");

        if (todo.UserId != userId)
            return Forbid("You are not authorized to complete this task");

        todo.Status = Status.Done;
        _unitOfWork.ToDoRepository.Update(todo);
        await _unitOfWork.Save();
        return Ok("ToDo completed successfully");
    }
    
    
    //GetAllToDosByUserId
    [Authorize]
    [HttpGet("get-all")]
    
    public async Task<IActionResult> GetAll()
    {
        var preToken = Request.Headers["Authorization"];
        var token = preToken.ToString()[7..];
        var userId = _authService.GetUserIdFromToken(token);
        if (userId is null)
            return Unauthorized("Invalid token");

        var todos = await _unitOfWork.ToDoRepository.GetAllByUserId(userId);
        return Ok(todos);
    }
    
}  