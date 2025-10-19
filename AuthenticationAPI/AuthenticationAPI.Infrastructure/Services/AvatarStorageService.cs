using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AuthenticationAPI.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthenticationAPI.Infrastructure.Services
{
    public class AvatarStorageService : IAvatarStorageService
    {
        private const int DefaultMaxFileSizeBytes = 2 * 1024 * 1024; // 2 MB
        private static readonly Regex DataUrlRegex = new(@"^data:(?<type>[\w/\-\+\.]+);base64,(?<data>.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AvatarStorageService> _logger;
        private readonly string _avatarsRoot;
        private readonly string _publicBasePath;
        private readonly int _maxFileSizeBytes;

        public AvatarStorageService(
            IWebHostEnvironment environment,
            IConfiguration configuration,
            ILogger<AvatarStorageService> logger)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var rootOverride = configuration["AvatarStorage:RootPath"];
            var webRoot = !string.IsNullOrWhiteSpace(rootOverride)
                ? rootOverride
                : environment.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");

            _avatarsRoot = Path.Combine(webRoot, "uploads", "avatars");
            Directory.CreateDirectory(_avatarsRoot);

            _maxFileSizeBytes = configuration.GetValue<int?>("AvatarStorage:MaxFileSizeBytes") ?? DefaultMaxFileSizeBytes;

            var configuredBasePath = configuration["AvatarStorage:BasePath"];
            if (string.IsNullOrWhiteSpace(configuredBasePath))
            {
                configuredBasePath = "/uploads/avatars";
            }

            configuredBasePath = configuredBasePath.TrimEnd('/');
            if (!configuredBasePath.StartsWith("http", StringComparison.OrdinalIgnoreCase) &&
                !configuredBasePath.StartsWith("/"))
            {
                configuredBasePath = "/" + configuredBasePath;
            }

            _publicBasePath = configuredBasePath;
        }

        public async Task<string> SaveAvatarAsync(Guid userId, string dataUrl)
        {
            if (string.IsNullOrWhiteSpace(dataUrl))
            {
                throw new ArgumentException("Avatar data cannot be empty.", nameof(dataUrl));
            }

            try
            {
                var (contentType, base64Data) = ParseDataUrl(dataUrl);
                var extension = GetExtensionFromContentType(contentType);
                var imageBytes = Convert.FromBase64String(base64Data);

                if (imageBytes.Length > _maxFileSizeBytes)
                {
                    throw new InvalidOperationException($"Avatar exceeds the maximum allowed size of {_maxFileSizeBytes} bytes.");
                }

                var fileName = $"{userId:N}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
                var destinationPath = Path.Combine(_avatarsRoot, fileName);

                await File.WriteAllBytesAsync(destinationPath, imageBytes);
                _logger.LogInformation("Saved avatar for user {UserId} to {Destination}", userId, destinationPath);

                return BuildPublicPath(fileName);
            }
            catch (FormatException ex)
            {
                _logger.LogWarning(ex, "Invalid avatar payload provided for user {UserId}", userId);
                throw new InvalidOperationException("Avatar data is not a valid base64 image string.", ex);
            }
        }

        public Task DeleteAvatarAsync(string? existingPath)
        {
            if (string.IsNullOrWhiteSpace(existingPath))
            {
                return Task.CompletedTask;
            }

            if (existingPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                // External URLs are not managed by this service.
                return Task.CompletedTask;
            }

            try
            {
                var trimmed = existingPath.TrimStart('/');
                var webRoot = _environment.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
                var fullPath = Path.Combine(webRoot, trimmed.Replace('/', Path.DirectorySeparatorChar));

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("Removed avatar file {FilePath}", fullPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to remove avatar at path {ExistingPath}", existingPath);
            }

            return Task.CompletedTask;
        }

        private static (string contentType, string base64Data) ParseDataUrl(string dataUrl)
        {
            var match = DataUrlRegex.Match(dataUrl.Trim());
            if (match.Success)
            {
                var contentType = match.Groups["type"].Value;
                var base64 = match.Groups["data"].Value;
                return (contentType, SanitizeBase64(base64));
            }

            // If the string is raw base64 without data URL prefix, assume PNG.
            return ("image/png", SanitizeBase64(dataUrl));
        }

        private static string GetExtensionFromContentType(string contentType)
        {
            return contentType.ToLowerInvariant() switch
            {
                "image/png" => ".png",
                "image/jpeg" or "image/jpg" => ".jpg",
                "image/gif" => ".gif",
                "image/webp" => ".webp",
                _ => ".png"
            };
        }

        private string BuildPublicPath(string fileName)
        {
            if (_publicBasePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return $"{_publicBasePath}/{fileName}";
            }

            return $"{_publicBasePath}/{fileName}".Replace("//", "/");
        }

        private static string SanitizeBase64(string base64Data)
        {
            return base64Data.Replace(" ", string.Empty)
                             .Replace("\r", string.Empty)
                             .Replace("\n", string.Empty);
        }
    }
}
