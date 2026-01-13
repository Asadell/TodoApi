using Microsoft.AspNetCore.Http;
using Nedo.AspNet.Request.Validation.Attributes.Image;

namespace TodoApi.Models.Requests;

public class UploadTodoImageRequest
{
    [MaxImageSize(5)]
    public IFormFile Image { get; set; } = null!;
}