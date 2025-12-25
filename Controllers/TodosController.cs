using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly AppDbContext _context;

    public TodosController(AppDbContext context)
    {
        _context = context;
    }

    // get api/todos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodos()
    {
        return await _context.TodoItems.ToListAsync();
    }

    // get api/todo/1
    [HttpGet("{id}")]
    public async Task<ActionResult<TodoItem>> GetTodo(int id)
    {
        var todo = await _context.TodoItems.FindAsync(id);

        if (todo == null)
        {
            return NotFound(new { message = "Todo not found"});
        }

        return todo;
    }

    // post api/todo
    [HttpPost]
    public async Task<ActionResult<TodoItem>> CreateTodo(TodoItem todo)
    {
        if (string.IsNullOrWhiteSpace(todo.Title))
        {
            return BadRequest(new { message = "Title is required"});
        }

        _context.TodoItems.Add(todo);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTodo), new { id = todo.Id }, todo);
    }

    // put api/todo/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTodo(int id, TodoItem todo)
    {
        if (id != todo.Id)
        {
            return BadRequest(new { message = "ID mismatch"});
        }

        var existingTodo = await _context.TodoItems.FindAsync(id);
        if (existingTodo == null)
        {
            return NotFound(new { message = "Todo not found" });
        }

        existingTodo.Title = todo.Title;
        existingTodo.Description = todo.Description;
        existingTodo.IsCompleted = todo.IsCompleted;

        if (todo.IsCompleted && existingTodo.CompleteAt == null)
        {
            existingTodo.CompleteAt = DateTime.UtcNow;
        } else if (!todo.IsCompleted)
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
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // delete api/todo/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTodo(int id)
    {
        var todo = await _context.TodoItems.FindAsync(id);
        if (todo==null)
        {
            return NotFound(new { message = "Todo not found" });
        }

        _context.TodoItems.Remove(todo);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool TodoExists(int id)
    {
        return _context.TodoItems.Any(e => e.Id == id);
    }
}