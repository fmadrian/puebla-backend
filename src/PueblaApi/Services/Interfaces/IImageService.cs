using System;

namespace PueblaApi.Services.Interfaces;

public interface IImageService
{
    Task<string> UploadImage(IFormFile file, string publicId);
    Task<string> DeleteImage(string imageName);
}
