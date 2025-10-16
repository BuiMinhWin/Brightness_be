using ChatService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace ChatService.Infrastructure.Services
{
    public class ResponseValidationService : IResponseValidationService
    {
        private readonly ILogger<ResponseValidationService> _logger;
        private const int MaxResponseLength = 5000;

        private static readonly string[] SqlKeywords = {
            "SELECT", "INSERT", "UPDATE", "DELETE", "DROP", "ALTER", "EXEC", "EXECUTE",
            "CREATE TABLE", "TRUNCATE", "GRANT", "REVOKE"
        };

        private static readonly string[] SystemCommands = {
            "bash", "sh ", "/bin/", "cmd.exe", "powershell", "eval(", "exec(",
            "system(", "popen(", "subprocess"
        };

        private static readonly string[] ScriptPatterns = {
            "<script", "javascript:", "onerror=", "onclick=", "onload=",
            "<iframe", "<object", "<embed"
        };

        private static readonly string[] PathTraversalPatterns = {
            "../", "..\\", "/etc/", "C:\\Windows", "/bin/", "/usr/"
        };

        public ResponseValidationService(ILogger<ResponseValidationService> logger)
        {
            _logger = logger;
        }

        public string ValidateAndSanitizeResponse(string response)
        {
            if (string.IsNullOrEmpty(response))
                return string.Empty;

            // Check length
            if (response.Length > MaxResponseLength)
            {
                _logger.LogWarning("Response length exceeds maximum ({Length} > {Max}), truncating", 
                    response.Length, MaxResponseLength);
                response = response.Substring(0, MaxResponseLength) + "...";
            }

            var originalResponse = response;
            bool isModified = false;

            // Check for SQL keywords
            foreach (var keyword in SqlKeywords)
            {
                if (response.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("SQL keyword detected in AI response: {Keyword}", keyword);
                    // Remove the offensive pattern
                    response = Regex.Replace(response, Regex.Escape(keyword), "[REDACTED]", RegexOptions.IgnoreCase);
                    isModified = true;
                }
            }

            // Check for system commands
            foreach (var command in SystemCommands)
            {
                if (response.Contains(command, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("System command detected in AI response: {Command}", command);
                    response = Regex.Replace(response, Regex.Escape(command), "[REDACTED]", RegexOptions.IgnoreCase);
                    isModified = true;
                }
            }

            // Check for script tags
            foreach (var pattern in ScriptPatterns)
            {
                if (response.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Script pattern detected in AI response: {Pattern}", pattern);
                    response = Regex.Replace(response, Regex.Escape(pattern), "[REDACTED]", RegexOptions.IgnoreCase);
                    isModified = true;
                }
            }

            // Check for path traversal
            foreach (var pattern in PathTraversalPatterns)
            {
                if (response.Contains(pattern))
                {
                    _logger.LogWarning("Path traversal pattern detected in AI response: {Pattern}", pattern);
                    response = response.Replace(pattern, "[REDACTED]");
                    isModified = true;
                }
            }

            // If heavily modified, return generic safe response
            if (isModified && response.Length < originalResponse.Length * 0.5)
            {
                _logger.LogError("AI response contained too many unsafe patterns, returning generic response");
                return "I apologize, but I couldn't generate a safe response. Please try rephrasing your question about fitness and health.";
            }

            return response;
        }
    }
}

