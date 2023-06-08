using CJSBugTracker.Enums;

namespace CJSBugTracker.Service.Interface
{
    public interface IBTFileService
    {
        Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file);

        string ConvertByteArrayToFile(byte[]? fileData, string? extension, DefaultImage defaultImage);
    }
}
