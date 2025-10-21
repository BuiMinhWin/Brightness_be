using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using BillingAPI.Application.DTOs;
using BillingAPI.Application.Interfaces;
using BillingAPI.Domain.Entities;
using BillingAPI.Infrastructure.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BillingAPI.Infrastructure.Services
{
    public class InvoiceEmailService : IInvoiceEmailService
    {
        private readonly EmailOptions _options;
        private readonly ILogger<InvoiceEmailService> _logger;
        private readonly Lazy<string?> _templateContent;

        public InvoiceEmailService(IOptions<EmailOptions> options, ILogger<InvoiceEmailService> logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _templateContent = new Lazy<string?>(LoadTemplateContent, true);
        }

        public async Task SendInvoicePaidEmailAsync(Invoice invoice, SubscriptionPlan plan, UserProfileDTO user)
        {
            if (invoice is null) throw new ArgumentNullException(nameof(invoice));
            if (plan is null) throw new ArgumentNullException(nameof(plan));
            if (user is null) throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                _logger.LogWarning("Cannot send invoice email because user {UserId} has no email address.", user.Id);
                return;
            }

            if (string.IsNullOrWhiteSpace(_options.Host) || string.IsNullOrWhiteSpace(_options.FromEmail))
            {
                _logger.LogWarning("Email configuration is incomplete. Host or FromEmail missing. Skipping invoice email for {InvoiceId}.", invoice.Id);
                return;
            }

            using var message = BuildMailMessage(invoice, plan, user);

            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl
            };

            if (!string.IsNullOrWhiteSpace(_options.User))
            {
                client.Credentials = new NetworkCredential(_options.User, _options.Password);
            }

            try
            {
                await client.SendMailAsync(message);
                _logger.LogInformation("Invoice {InvoiceId} email sent successfully to {Email}.", invoice.Id, user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send invoice email for {InvoiceId} to {Email}.", invoice.Id, user.Email);
            }
        }

        private MailMessage BuildMailMessage(Invoice invoice, SubscriptionPlan plan, UserProfileDTO user)
        {
            var toAddress = new MailAddress(user.Email, string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName);
            var fromAddress = new MailAddress(_options.FromEmail!, string.IsNullOrWhiteSpace(_options.FromName) ? _options.FromEmail : _options.FromName);

            var subject = $"[Brightness] Xác nhận thanh toán {invoice.InvoiceCode}";
            var body = BuildBody(invoice, plan, user);

            var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            return message;
        }

        private string BuildBody(Invoice invoice, SubscriptionPlan plan, UserProfileDTO user)
        {
            var template = _templateContent.Value;
            if (!string.IsNullOrWhiteSpace(template))
            {
                return RenderTemplate(template, invoice, plan, user);
            }

            return BuildFallbackBody(invoice, plan, user);
        }

        private string RenderTemplate(string template, Invoice invoice, SubscriptionPlan plan, UserProfileDTO user)
        {
            var customerName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName;
            var amountFormatted = string.Format(CultureInfo.InvariantCulture, "{0:N0} {1}", invoice.Amount, invoice.Currency);
            var paidAt = invoice.PaidAt?.ToLocalTime().ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture) ?? DateTime.UtcNow.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture);

            var planDescription = string.IsNullOrWhiteSpace(plan.Description)
                ? "Gói dịch vụ Brightness."
                : WebUtility.HtmlEncode(plan.Description).Replace("\n", "<br />", StringComparison.Ordinal);

            var notes = string.IsNullOrWhiteSpace(invoice.Notes)
                ? "Cảm ơn bạn đã tin tưởng và đồng hành cùng Brightness."
                : WebUtility.HtmlEncode(invoice.Notes).Replace("\n", "<br />", StringComparison.Ordinal);

            var supportEmail = _options.FromEmail ?? _options.User ?? "support@brightness.local";
            var companyAddress = "Địa chỉ công ty Brightness";

            var replacements = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["{{CUSTOMER_NAME}}"] = WebUtility.HtmlEncode(customerName),
                ["{{CUSTOMER_EMAIL}}"] = WebUtility.HtmlEncode(user.Email ?? string.Empty),
                ["{{INVOICE_CODE}}"] = WebUtility.HtmlEncode(invoice.InvoiceCode),
                ["{{PLAN_NAME}}"] = WebUtility.HtmlEncode(plan.Name),
                ["{{PLAN_DESCRIPTION}}"] = planDescription,
                ["{{AMOUNT}}"] = WebUtility.HtmlEncode(amountFormatted),
                ["{{PAID_AT}}"] = WebUtility.HtmlEncode(paidAt),
                ["{{PLAN_NOTES}}"] = notes,
                ["{{SUPPORT_EMAIL}}"] = WebUtility.HtmlEncode(supportEmail),
                ["{{COMPANY_ADDRESS}}"] = WebUtility.HtmlEncode(companyAddress),
                ["{{CURRENT_YEAR}}"] = DateTime.UtcNow.Year.ToString(CultureInfo.InvariantCulture)
            };

            var rendered = template;
            foreach (var (placeholder, value) in replacements)
            {
                rendered = rendered.Replace(placeholder, value, StringComparison.Ordinal);
            }

            return rendered;
        }

        private static string BuildFallbackBody(Invoice invoice, SubscriptionPlan plan, UserProfileDTO user)
        {
            var amountFormatted = string.Format(CultureInfo.InvariantCulture, "{0:N0} {1}", invoice.Amount, invoice.Currency);
            var paidAt = invoice.PaidAt?.ToLocalTime().ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture) ?? DateTime.UtcNow.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture);

            var sb = new StringBuilder();
            sb.Append("<html><body>");
            sb.AppendFormat("<p>Xin chào {0},</p>", string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName);
            sb.Append("<p>Cảm ơn bạn đã tin tưởng dịch vụ của chúng tôi. Hóa đơn của bạn đã được thanh toán thành công.</p>");
            sb.Append("<table style='border-collapse:collapse;margin-top:16px;'>");
            sb.Append("<tr><td style='padding:4px 12px;font-weight:bold;'>Mã hóa đơn:</td><td>" + invoice.InvoiceCode + "</td></tr>");
            sb.Append("<tr><td style='padding:4px 12px;font-weight:bold;'>Gói dịch vụ:</td><td>" + plan.Name + "</td></tr>");
            sb.Append("<tr><td style='padding:4px 12px;font-weight:bold;'>Tổng tiền:</td><td>" + amountFormatted + "</td></tr>");
            sb.Append("<tr><td style='padding:4px 12px;font-weight:bold;'>Thời gian thanh toán:</td><td>" + paidAt + "</td></tr>");
            sb.Append("</table>");

            if (!string.IsNullOrWhiteSpace(plan.Description))
            {
                sb.Append("<p style='margin-top:16px;'><strong>Quyền lợi gói:</strong><br/>");
                sb.Append(plan.Description.Replace("\n", "<br/>", StringComparison.Ordinal));
                sb.Append("</p>");
            }

            sb.Append("<p>Nếu bạn cần hỗ trợ thêm, vui lòng liên hệ với Brightness.</p>");
            sb.Append("<p>Trân trọng,<br/>Brightness Team</p>");
            sb.Append("</body></html>");

            return sb.ToString();
        }

        private string? LoadTemplateContent()
        {
            try
            {
                var baseDirectory = AppContext.BaseDirectory;
                var candidatePaths = new[]
                {
                    Path.Combine(baseDirectory, "Email", "Templates", "invoice.html"),
                    Path.Combine(baseDirectory, "Templates", "invoice.html"),
                    Path.Combine(baseDirectory, "invoice.html")
                };

                foreach (var path in candidatePaths)
                {
                    if (File.Exists(path))
                    {
                        _logger.LogInformation("Loaded invoice email template from {Path}", path);
                        return File.ReadAllText(path);
                    }
                }

                _logger.LogWarning("Invoice email template not found in output directory. Falling back to inline template.");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load invoice email template. Falling back to inline template.");
                return null;
            }
        }
    }
}
