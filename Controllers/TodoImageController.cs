using Microsoft.AspNetCore.Mvc;
using Sindika.AspNet.Response;
using TodoApi.Data;
using TodoApi.Models.Requests;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/todo/{todoId}/image")]
public class TodoImageController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public TodoImageController(AppDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    // POST: api/todo/5/image
    [HttpPost]
    [ProducesResponseType(typeof(BaseResponse<object, object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object, object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<object, object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadImage(
        int todoId,
        [FromForm] UploadTodoImageRequest request)
    {
        var todo = await _context.TodoItems.FindAsync(todoId);
        if (todo == null)
        {
            return NotFound(ResponseHelper.Error<object, object>(
                null, "TODO-ERR-001", "Todo not found"));
        }

        var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "todos");
        Directory.CreateDirectory(uploadsPath);

        var fileExtension = Path.GetExtension(request.Image.FileName);
        var fileName = $"{todoId}_{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await request.Image.CopyToAsync(stream);
        }

        var relativePath = $"/uploads/todos/{fileName}";
        todo.ImagePath = relativePath;
        await _context.SaveChangesAsync();

        return Ok(ResponseHelper.Success<object>(
            new { imagePath = relativePath },
            "Image uploaded successfully"));
    }

    // DELETE: api/todo/5/image
    // Delete image for a todo
    [HttpDelete]
    [ProducesResponseType(typeof(BaseResponse<object, object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object, object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImage(int todoId)
    {
        var todo = await _context.TodoItems.FindAsync(todoId);
        if (todo == null)
        {
            return NotFound(ResponseHelper.Error<object, object>(
                null, "TODO-ERR-001", "Todo not found"));
        }

        if (string.IsNullOrEmpty(todo.ImagePath))
        {
            return NotFound(ResponseHelper.Error<object, object>(
                null, "TODO-ERR-006", "No image found for this todo"));
        }

        var fileName = Path.GetFileName(todo.ImagePath);
        var filePath = Path.Combine(_environment.WebRootPath, "uploads", "todos", fileName);
        
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }

        todo.ImagePath = null;
        await _context.SaveChangesAsync();

        return Ok(ResponseHelper.Success<object>(null, "Image deleted successfully"));
    }
}