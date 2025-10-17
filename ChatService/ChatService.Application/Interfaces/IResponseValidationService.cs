namespace ChatService.Application.Interfaces
{
    public interface IResponseValidationService
    {
        string ValidateAndSanitizeResponse(string response);
    }
}

