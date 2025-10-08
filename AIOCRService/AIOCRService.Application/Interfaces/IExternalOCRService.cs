namespace AIOCRService.Application.Interfaces
{
    public interface IExternalOCRService
    {
        Task<string> ExtractTextFromImageAsync(string imageBase64, string contentType);
        Task<string> ExtractTextFromImageUrlAsync(string imageUrl);
    }
}

