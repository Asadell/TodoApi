using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sindika.AspNet.Response;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Models.Requests;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoController : ControllerBase
{
    private readonly AppDbContext _context;

    public TodoController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/todo
    [HttpGet]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<TodoItem>, object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTodos()
    {
        var todos = await _context.TodoItems
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return Ok(ResponseHelper.Success<IEnumerable<TodoItem>>(todos, "Todos retrieved successfully"));
    }

    // GET: api/todo/5
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BaseResponse<TodoItem, object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object, object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTodo(int id)
    {
        var todo = await _context.TodoItems.FindAsync(id);

        if (todo == null)
        {
            return NotFound(ResponseHelper.Error<object, object>(
                null, "TODO-ERR-001", "Todo not found"));
        }

        return Ok(ResponseHelper.Success<TodoItem>(todo, "Todo retrieved successfully"));
    }

    // POST: api/todo
    [HttpPost]
    [ProducesResponseType(typeof(BaseResponse<TodoItem, object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseResponse<object, object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTodo([FromBody] CreateTodoRequest request)
    {

        var todo = new TodoItem
        {
            Title = request.Title,
            Description = request.Description,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.TodoItems.Add(todo);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetTodo),
            new { id = todo.Id },
            ResponseHelper.Success<TodoItem>(todo, "Todo created successfully"));
    }

    // PUT: api/todo/5
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BaseResponse<object, object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object, object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<object, object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTodo(int id, [FromBody] UpdateTodoRequest request)
    {
        var existingTodo = await _context.TodoItems.FindAsync(id);
        if (existingTodo == null)
        {
            return NotFound(ResponseHelper.Error<object, object>(
                null, "TODO-ERR-002", "Todo not found"));
        }

        existingTodo.Title = request.Title;
        existingTodo.Description = request.Description;
        existingTodo.IsCompleted = request.IsCompleted;

        if (request.IsCompleted && existingTodo.CompletedAt == null)
        {
            existingTodo.CompletedAt = DateTime.UtcNow;
        }
        else if (!request.IsCompleted)
        {
            existingTodo.CompletedAt = null;
        }

        await _context.SaveChangesAsync();

        return Ok(ResponseHelper.Success<object>(null, "Todo updated successfully"));
    }

    // DELETE: api/todo/5
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(BaseResponse<object, object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object, object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTodo(int id)
    {
        var todo = await _context.TodoItems.FindAsync(id);
        if (todo == null)
        {
            return NotFound(ResponseHelper.Error<object, object>(
                null, "TODO-ERR-004", "Todo not found"));
        }

        _context.TodoItems.Remove(todo);
        await _context.SaveChangesAsync();

        return Ok(ResponseHelper.Success<object>(null, "Todo deleted successfully"));
    }

    // PATCH: api/todo/5/toggle
    [HttpPatch("{id}/toggle")]
    [ProducesResponseType(typeof(BaseResponse<TodoItem, object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object, object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleTodo(int id)
    {
        var todo = await _context.TodoItems.FindAsync(id);
        if (todo == null)
        {
            return NotFound(ResponseHelper.Error<object, object>(
                null, "TODO-ERR-005", "Todo not found"));
        }

        todo.IsCompleted = !todo.IsCompleted;
        todo.CompletedAt = todo.IsCompleted ? DateTime.UtcNow : null;

        await _context.SaveChangesAsync();

        return Ok(ResponseHelper.Success<TodoItem>(todo, "Todo toggled successfully"));
    }
}