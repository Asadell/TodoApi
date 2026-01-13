using Nedo.AspNet.Request.Validation.Attributes.File;
using Nedo.AspNet.Request.Validation.Attributes.Image;
using Nedo.AspNet.Request.Validation.Attributes.Generic;

namespace TodoApi.Models.Requests;

public class UploadTodoImageRequest
{
    [Required]
    [FileType("jpg", "jpeg", "png", "gif", "webp")]
    [MaxImageSize(10485760)]  // 10MB
    public IFormFile Image { get; set; } = null!;
}