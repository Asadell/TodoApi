using Nedo.AspNet.Request.Validation.Attributes.Generic;

namespace TodoApi.Models.Requests;

public class CreateTodoRequest
{
    [Required]
    [MaxLength(10)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }
}