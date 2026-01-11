using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

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
    public async Task<ActionResult<ApiResponse<IEnumerable<TodoItem>>>> GetTodos()
    {
        var todos = await _context.TodoItems.ToListAsync();
        return Ok(ApiResponse<IEnumerable<TodoItem>>.Success(todos, "Todos retrieved successfully"));
    }

    // GET: api/todo/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TodoItem>>> GetTodo(int id)
    {
        var todo = await _context.TodoItems.FindAsync(id);

        if (todo == null)
        {
            return NotFound(ApiResponse<TodoItem>.Error("Todo not found"));
        }

        return Ok(ApiResponse<TodoItem>.Success(todo, "Todo retrieved successfully"));
    }

    // POST: api/todo
    [HttpPost]
    public async Task<ActionResult<ApiResponse<TodoItem>>> CreateTodo(TodoItem todo)
    {
        if (string.IsNullOrWhiteSpace(todo.Title))
        {
            return BadRequest(ApiResponse<TodoItem>.Error("Title is required"));
        }

        _context.TodoItems.Add(todo);
        await _context.SaveChangesAsync();

        var response = ApiResponse<TodoItem>.Success(todo, "Todo created successfully");
        return CreatedAtAction(nameof(GetTodo), new { id = todo.Id }, response);
    }

    // PUT: api/todo/5
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse>> UpdateTodo(int id, TodoItem todo)
    {
        if (id != todo.Id)
        {
            return BadRequest(ApiResponse.Error("ID mismatch"));
        }

        var existingTodo = await _context.TodoItems.FindAsync(id);
        if (existingTodo == null)
        {
            return NotFound(ApiResponse.Error("Todo not found"));
        }

        existingTodo.Title = todo.Title;
        existingTodo.Description = todo.Description;
        existingTodo.IsCompleted = todo.IsCompleted;
        
        if (todo.IsCompleted && existingTodo.CompleteAt == null)
        {
            existingTodo.CompleteAt = DateTime.UtcNow;
        }
        else if (!todo.IsCompleted)
        {
            existingTodo.CompleteAt = null;
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TodoExists(id))
            {
                return NotFound(ApiResponse.Error("Todo not found"));
            }
            throw;
        }

        return Ok(ApiResponse.Success("Todo updated successfully"));
    }

    // DELETE: api/todo/5
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteTodo(int id)
    {
        var todo = await _context.TodoItems.FindAsync(id);
        if (todo == null)
        {
            return NotFound(ApiResponse.Error("Todo not found"));
        }

        _context.TodoItems.Remove(todo);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse.Success("Todo deleted successfully"));
    }

    // PATCH: api/todo/5/toggle
    [HttpPatch("{id}/toggle")]
    public async Task<ActionResult<ApiResponse<TodoItem>>> ToggleTodo(int id)
    {
        var todo = await _context.TodoItems.FindAsync(id);
        if (todo == null)
        {
            return NotFound(ApiResponse<TodoItem>.Error("Todo not found"));
        }

        todo.IsCompleted = !todo.IsCompleted;
        todo.CompleteAt = todo.IsCompleted ? DateTime.UtcNow : null;

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<TodoItem>.Success(todo, "Todo toggled successfully"));
    }

    private bool TodoExists(int id)
    {
        return _context.TodoItems.Any(e => e.Id == id);
    }
}