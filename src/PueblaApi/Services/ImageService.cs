using System.Net;
using System.Text.Encodings.Web;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using PueblaApi.Exceptions;
using PueblaApi.Services.Interfaces;
using PueblaApi.Settings;

namespace PueblaApi.Services;

public class ImageService : IImageService
{
    private Account Account { set; get; }
    private Cloudinary Cloudinary { set; get; }
    private readonly ImageServiceConfiguration _imageServiceConfiguration;
    private ILogger<IImageService> _logger;
    public ImageService(ImageServiceConfiguration imageServiceConfiguration, ILogger<IImageService> logger)
    {
        this._imageServiceConfiguration = imageServiceConfiguration;
        this._logger = logger;
        // Build client (only once).
        if (this.Account == null && this.Cloudinary == null)
        {
            this.Account = new Account(
                "dweyur0zb",
                "118958199649947",
                "R8YxTm0VkaflHI5OrZ7yRP6o_cw");

            this.Cloudinary = new Cloudinary(Account);
            this.Cloudinary.Api.Secure = true;
            this._logger.LogInformation("[CLOUDINARY]: Client created.");
        }
        else
        {
            this._logger.LogWarning("[CLOUDINARY]: Client already created.");
        }
    }
    /// <summary>
    /// Uploads image to Cloudinary and returns public ID of the file.
    /// </summary>
    /// <param name="file">Image to be uploaded</param>
    /// <returns>Public ID of the upload</returns>
    public async Task<string> UploadImage(IFormFile file, string? publicId = null)
    {
        try
        {
            // Run filters to check it is in correct format and size.
            this.FileIsValid(file);
            // Scale every image to MaxWidth x MaxHeight.
            var uploadParams = new ImageUploadParams()
            {
                // File will be assigned a random name (PublicID) by Cloudinary to avoid reading from the database twice
                // in one upload.
                File = new FileDescription(file.Name, file.OpenReadStream()),
                Overwrite = true,
                PublicId = publicId ?? null, // Add public ID when overwriting/updating the image.
                AssetFolder = publicId == null ? this._imageServiceConfiguration.AssetFolderName : null,
                UseAssetFolderAsPublicIdPrefix = true,
                Transformation = new Transformation().Width(this._imageServiceConfiguration.MaxWidth)
                                .Height(this._imageServiceConfiguration.MaxHeight).Crop("scale")
            };

            var uploadResult = await this.Cloudinary.UploadAsync(uploadParams);
            this._logger.LogInformation("[CLOUDINARY]: Image uploaded.");

            return uploadResult.PublicId; // Example: images/svpeupmkzsywpoh7okov
        }
        catch (Exception)
        {
            // Log and rethrow exception to stop script execution.
            this._logger.LogError("[CLOUDINARY]: Failed to upload the image.");
            throw;
        }
    }

    public async Task<string> DeleteImage(string publicId)
    {
        try
        {
            DeletionParams deletionParams = new DeletionParams(publicId)
            {
                Invalidate = true
            };
            DeletionResult deletionResult = await this.Cloudinary.DestroyAsync(deletionParams);
            this._logger.LogInformation("[CLOUDINARY]: Image deleted.");
            return deletionResult.Result;
        }
        catch (ApiException)
        {
            // Don't log and rethrow exception.
            throw;
        }
        catch (Exception)
        {
            // Log and rethrow exception to stop script execution.
            this._logger.LogError("[CLOUDINARY]: Failed to delete the image.");
            throw;
        }
    }
    /// <summary>
    /// Validate file's format and size.
    /// </summary>
    /// <param name="file">File to be validated</param>
    /// <returns>Boolean that indicate the file is valid or an exception</returns>
    private bool FileIsValid(IFormFile file)
    {

        string[] permittedExtensions = { ".png", ".jpg" };
        var ext = Path.GetExtension(WebUtility.HtmlEncode(file.FileName)).ToLowerInvariant();

        if (file.Length > this._imageServiceConfiguration.MaxFileSize || string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
            throw new ApiException("Image not valid (size or format).");

        return true;
    }
}
