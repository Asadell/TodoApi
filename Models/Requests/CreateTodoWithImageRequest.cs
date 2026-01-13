using Nedo.AspNet.Request.Validation.Attributes.Generic;
using Nedo.AspNet.Request.Validation.Attributes.File;
using Nedo.AspNet.Request.Validation.Attributes.Image;

namespace TodoApi.Models.Requests;

public class CreateTodoWithImageRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    // Image validation
    [FileType("jpg", "jpeg", "png", "gif")]
    [FileNameLength(100)]
    [FileNamePattern(@"^[a-zA-Z0-9_\\-\\.]+$")]  // Alphanumeric, dash, underscore, dot only
    [MaxImageSize(5242880)]  // 5MB in bytes
    [ImageFileNamePattern(@"\\.(jpg|jpeg|png|gif)$")]
    public string? ImageFileName { get; set; }

    // For actual file upload (not validated by attributes, handled separately)
    public IFormFile? ImageFile { get; set; }
}