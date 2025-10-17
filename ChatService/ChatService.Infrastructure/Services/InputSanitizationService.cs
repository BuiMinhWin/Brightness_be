using ChatService.Application.Interfaces;
using System.Text;
using System.Text.RegularExpressions;

namespace ChatService.Infrastructure.Services
{
    public class InputSanitizationService : IInputSanitizationService
    {
        private const int MaxInputLength = 10000;

        public string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Truncate to max length
            if (input.Length > MaxInputLength)
                input = input.Substring(0, MaxInputLength);

            // Remove control characters (ASCII 0-31) except \t, \n, \r
            input = RemoveControlCharacters(input);

            // Remove null bytes and zero-width characters
            input = input.Replace("\0", "")
                        .Replace("\u200B", "") // Zero-width space
                        .Replace("\u200C", "") // Zero-width non-joiner
                        .Replace("\u200D", "") // Zero-width joiner
                        .Replace("\uFEFF", ""); // Zero-width no-break space

            // Strip HTML/XML tags
            input = Regex.Replace(input, @"<[^>]*>", string.Empty);

            // Collapse excessive whitespace
            input = CollapseWhitespace(input);

            // Normalize Unicode to prevent homograph attacks
            input = input.Normalize(NormalizationForm.FormC);

            return input.Trim();
        }

        private string RemoveControlCharacters(string input)
        {
            var sb = new StringBuilder(input.Length);
            foreach (char c in input)
            {
                // Keep tab, newline, carriage return, and all printable characters
                if (c == '\t' || c == '\n' || c == '\r' || c >= 32)
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private string CollapseWhitespace(string input)
        {
            // Replace multiple spaces with max 2 spaces
            input = Regex.Replace(input, @" {3,}", "  ");
            
            // Replace multiple newlines with max 2 newlines
            input = Regex.Replace(input, @"\n{3,}", "\n\n");
            
            return input;
        }
    }
}

