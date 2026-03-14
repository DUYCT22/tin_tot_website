namespace Tin_Tot_Website.Services
{
    public interface IRecaptchaValidationService
    {
        bool IsEnabled { get; }
        Task<bool> ValidateAsync(string? token, string? remoteIp);
    }
}
