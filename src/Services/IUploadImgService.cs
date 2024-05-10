using CloudinaryDotNet.Actions;

namespace src.Services;

public interface IUploadImgService
{
    Task<ImageUploadResult?> UploadImg(IFormFile file);
    Task<DeletionResult?> DeleteImg(string imgId);
    
}