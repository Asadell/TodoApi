using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Models.Requests;
using Nedo.AspNet.Request.Validation.Contracts;
using Nedo.AspNet.Request.Validation.Context;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IRequestValidationEngine _validationEngine;

    public TodoController(AppDbContext context, IRequestValidationEngine validationEngine)
    {
        _context = context;
        _validationEngine = validationEngine;
    }

    // GET: api/todo
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TodoItem>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<TodoItem>>>> GetTodos()
    {
        var todos = await _context.TodoItems
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
        
        return Ok(ApiResponse<IEnumerable<TodoItem>>.Ok(todos, "Todos retrieved successfully"));
    }

    // GET: api/todo/5
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<TodoItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TodoItem>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TodoItem>>> GetTodo(int id)
    {
        var todo = await _context.TodoItems.FindAsync(id);

        if (todo == null)
        {
            return NotFound(ApiResponse<TodoItem>.Fail("Todo not found"));
        }

        return Ok(ApiResponse<TodoItem>.Ok(todo, "Todo retrieved successfully"));
    }

    // POST: api/todo
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TodoItem>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<TodoItem>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<TodoItem>>> CreateTodo(CreateTodoRequest request)
    {
        var validationResult = _validationEngine.Validate(request);
        
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(e => new ValidationErrorDetail(e.Field, e.Message, e.Code))
                .ToList();
            
            return BadRequest(ApiResponse<TodoItem>.Fail("Validation failed", errors));
        }

        var todo = new TodoItem
        {
            Title = request.Title,
            Description = request.Description,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.TodoItems.Add(todo);
        await _context.SaveChangesAsync();

        var response = ApiResponse<TodoItem>.Ok(todo, "Todo created successfully");
        return CreatedAtAction(nameof(GetTodo), new { id = todo.Id }, response);
    }

    // PUT: api/todo/5
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> UpdateTodo(int id, UpdateTodoRequest request)
    {
        var validationResult = _validationEngine.Validate(request);
        
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(e => new ValidationErrorDetail(e.Field, e.Message, e.Code))
                .ToList();
            
            return BadRequest(ApiResponse.Fail("Validation failed", errors));
        }

        var existingTodo = await _context.TodoItems.FindAsync(id);
        if (existingTodo == null)
        {
            return NotFound(ApiResponse.Fail("Todo not found"));
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

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await TodoExists(id))
            {
                return NotFound(ApiResponse.Fail("Todo not found"));
            }
            throw;
        }

        return Ok(ApiResponse.Ok("Todo updated successfully"));
    }

    // DELETE: api/todo/5
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteTodo(int id)
    {
        var todo = await _context.TodoItems.FindAsync(id);
        if (todo == null)
        {
            return NotFound(ApiResponse.Fail("Todo not found"));
        }

        _context.TodoItems.Remove(todo);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse.Ok("Todo deleted successfully"));
    }

    // PATCH: api/todo/5/toggle
    [HttpPatch("{id}/toggle")]
    [ProducesResponseType(typeof(ApiResponse<TodoItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TodoItem>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TodoItem>>> ToggleTodo(int id)
    {
        var todo = await _context.TodoItems.FindAsync(id);
        if (todo == null)
        {
            return NotFound(ApiResponse<TodoItem>.Fail("Todo not found"));
        }

        todo.IsCompleted = !todo.IsCompleted;
        todo.CompletedAt = todo.IsCompleted ? DateTime.UtcNow : null;

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<TodoItem>.Ok(todo, "Todo toggled successfully"));
    }

    // HELPER
    private async Task<bool> TodoExists(int id)
    {
        return await _context.TodoItems.AnyAsync(e => e.Id == id);
    }
}