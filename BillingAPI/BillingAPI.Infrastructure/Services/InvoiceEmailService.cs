using System;
using System.Globalization;
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

        public InvoiceEmailService(IOptions<EmailOptions> options, ILogger<InvoiceEmailService> logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

            var subject = $"Invoice {invoice.InvoiceCode} payment received";
            var body = BuildBody(invoice, plan, user);

            var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            return message;
        }

        private static string BuildBody(Invoice invoice, SubscriptionPlan plan, UserProfileDTO user)
        {
            var amountFormatted = string.Format(CultureInfo.InvariantCulture, "{0:N0} {1}", invoice.Amount, invoice.Currency);
            var paidAt = invoice.PaidAt?.ToLocalTime().ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture) ?? DateTime.UtcNow.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture);

            var sb = new StringBuilder();
            sb.Append("<html><body>");
            sb.AppendFormat("<p>Xin chào {0},</p>", string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName);
            sb.Append("<p>C?m on b?n dã thanh toán. Hóa don c?a b?n dã du?c xác nh?n thành công.</p>");
            sb.Append("<table style='border-collapse:collapse;margin-top:16px;'>");
            sb.Append("<tr><td style='padding:4px 12px;font-weight:bold;'>Mã hóa don:</td><td>" + invoice.InvoiceCode + "</td></tr>");
            sb.Append("<tr><td style='padding:4px 12px;font-weight:bold;'>Gói d?ch v?:</td><td>" + plan.Name + "</td></tr>");
            sb.Append("<tr><td style='padding:4px 12px;font-weight:bold;'>Giá tr?:</td><td>" + amountFormatted + "</td></tr>");
            sb.Append("<tr><td style='padding:4px 12px;font-weight:bold;'>Th?i gian thanh toán:</td><td>" + paidAt + "</td></tr>");
            sb.Append("</table>");

            if (!string.IsNullOrWhiteSpace(plan.Description))
            {
                sb.Append("<p style='margin-top:16px;'><strong>Quy?n l?i gói:</strong><br/>");
                sb.Append(plan.Description.Replace("\n", "<br/>", StringComparison.Ordinal));
                sb.Append("</p>");
            }

            sb.Append("<p>N?u b?n c?n h? tr? thêm, vui lòng liên h? d?i ngu Brightness.</p>");
            sb.Append("<p>Trân tr?ng,<br/>Brightness Team</p>");
            sb.Append("</body></html>");

            return sb.ToString();
        }
    }
}
