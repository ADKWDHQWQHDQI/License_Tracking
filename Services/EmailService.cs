using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using License_Tracking.Models;

namespace License_Tracking.Services
{
    public interface IEmailService
    {
        Task SendRenewalAlertAsync(Alert alert);
        Task SendPipelineAlertAsync(Alert alert);
        Task SendPaymentReminderAsync(string recipientEmail, string recipientName, Invoice invoice);
        Task SendBulkAlertsAsync(List<Alert> alerts);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendRenewalAlertAsync(Alert alert)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("License Tracking System", _configuration["Email:From"]));

                // Determine recipients based on alert priority and type
                var recipients = GetRecipientsForAlert(alert);
                foreach (var recipient in recipients)
                {
                    message.To.Add(new MailboxAddress("", recipient));
                }

                message.Subject = $"🔔 {alert.Title}";

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = GenerateRenewalEmailBody(alert);
                bodyBuilder.TextBody = GenerateRenewalEmailTextBody(alert);
                message.Body = bodyBuilder.ToMessageBody();

                await SendEmailAsync(message);

                // Update alert status
                alert.EmailSent = true;
                alert.EmailSentDate = DateTime.Now;
                alert.EmailRecipients = string.Join("; ", recipients);

                _logger.LogInformation($"Renewal alert email sent for Alert ID {alert.AlertId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send renewal alert email for Alert ID {alert.AlertId}");
                throw;
            }
        }

        public async Task SendPipelineAlertAsync(Alert alert)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("License Tracking System", _configuration["Email:From"]));

                var recipients = GetRecipientsForAlert(alert);
                foreach (var recipient in recipients)
                {
                    message.To.Add(new MailboxAddress("", recipient));
                }

                message.Subject = $"📋 {alert.Title}";

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = GeneratePipelineEmailBody(alert);
                bodyBuilder.TextBody = GeneratePipelineEmailTextBody(alert);
                message.Body = bodyBuilder.ToMessageBody();

                await SendEmailAsync(message);

                alert.EmailSent = true;
                alert.EmailSentDate = DateTime.Now;
                alert.EmailRecipients = string.Join("; ", recipients);

                _logger.LogInformation($"Pipeline alert email sent for Alert ID {alert.AlertId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send pipeline alert email for Alert ID {alert.AlertId}");
                throw;
            }
        }

        public async Task SendPaymentReminderAsync(string recipientEmail, string recipientName, Invoice invoice)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("License Tracking System", _configuration["Email:From"]));
                message.To.Add(new MailboxAddress(recipientName, recipientEmail));
                message.Subject = $"💰 Payment Reminder - Invoice {invoice.InvoiceNumber}";

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = GeneratePaymentReminderEmailBody(invoice);
                bodyBuilder.TextBody = GeneratePaymentReminderTextBody(invoice);
                message.Body = bodyBuilder.ToMessageBody();

                await SendEmailAsync(message);

                _logger.LogInformation($"Payment reminder email sent for Invoice {invoice.InvoiceNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send payment reminder email for Invoice {invoice.InvoiceNumber}");
                throw;
            }
        }

        public async Task SendBulkAlertsAsync(List<Alert> alerts)
        {
            var tasks = new List<Task>();

            foreach (var alert in alerts)
            {
                if (alert.AlertType == AlertType.Renewal)
                {
                    tasks.Add(SendRenewalAlertAsync(alert));
                }
                else if (alert.AlertType == AlertType.PipelineReminder)
                {
                    tasks.Add(SendPipelineAlertAsync(alert));
                }
            }

            await Task.WhenAll(tasks);
            _logger.LogInformation($"Bulk alert emails sent for {alerts.Count} alerts");
        }

        private async Task SendEmailAsync(MimeMessage message)
        {
            try
            {
                _logger.LogInformation($"Attempting to send email to: {string.Join(", ", message.To.Select(t => t.ToString()))}");
                _logger.LogInformation($"SMTP Server: {_configuration["Email:SmtpServer"]}, Port: {_configuration["Email:Port"]}");

                using var client = new SmtpClient();

                // Enable SMTP logging for debugging
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                await client.ConnectAsync(
                    _configuration["Email:SmtpServer"] ?? "smtp.gmail.com",
                    int.Parse(_configuration["Email:Port"] ?? "587"),
                    SecureSocketOptions.StartTls
                );

                _logger.LogInformation("SMTP connection established");

                await client.AuthenticateAsync(
                    _configuration["Email:Username"],
                    _configuration["Email:Password"]
                );

                _logger.LogInformation("SMTP authentication successful");

                await client.SendAsync(message);
                _logger.LogInformation("Email sent successfully");

                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email via SMTP");
                throw;
            }
        }

        private List<string> GetRecipientsForAlert(Alert alert)
        {
            var recipients = new List<string>();

            // Use your actual email address for testing
            string adminEmail = _configuration["Email:From"] ?? "adnkuruva@gmail.com";

            // For now, send all alerts to admin email for testing
            // Later you can configure different recipients based on roles
            recipients.Add(adminEmail);

            // Add any specifically assigned recipients
            if (!string.IsNullOrEmpty(alert.AssignedTo) && IsValidEmail(alert.AssignedTo))
            {
                recipients.Add(alert.AssignedTo);
            }

            // Remove duplicates and return
            return recipients.Distinct().ToList();
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateRenewalEmailBody(Alert alert)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; border-radius: 10px 10px 0 0;'>
                            <h1 style='margin: 0; font-size: 24px;'>🔔 License Renewal Alert</h1>
                            <p style='margin: 10px 0 0 0; opacity: 0.9;'>Immediate Attention Required</p>
                        </div>
                        
                        <div style='background: white; padding: 30px; border: 1px solid #e1e5e9; border-radius: 0 0 10px 10px;'>
                            <div style='background: #fff3cd; border: 1px solid #ffeaa7; border-radius: 5px; padding: 15px; margin-bottom: 20px;'>
                                <h3 style='color: #856404; margin: 0 0 10px 0;'>⚠️ Priority: {alert.Priority}</h3>
                                <p style='margin: 0; color: #856404;'>{alert.AlertMessage}</p>
                            </div>
                            
                            <h3 style='color: #495057;'>License Details:</h3>
                            <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #dee2e6; font-weight: bold;'>Product:</td>
                                    <td style='padding: 8px; border-bottom: 1px solid #dee2e6;'>{alert.License?.ProductName ?? "N/A"}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #dee2e6; font-weight: bold;'>Client:</td>
                                    <td style='padding: 8px; border-bottom: 1px solid #dee2e6;'>{alert.License?.ClientName ?? "N/A"}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #dee2e6; font-weight: bold;'>Expiry Date:</td>
                                    <td style='padding: 8px; border-bottom: 1px solid #dee2e6;'>{alert.License?.ExpiryDate.ToString("MMMM dd, yyyy") ?? "N/A"}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #dee2e6; font-weight: bold;'>Days Remaining:</td>
                                    <td style='padding: 8px; border-bottom: 1px solid #dee2e6; color: #dc3545; font-weight: bold;'>{alert.DaysBeforeExpiry} days</td>
                                </tr>
                            </table>
                            
                            <div style='background: #d1ecf1; border: 1px solid #bee5eb; border-radius: 5px; padding: 15px; margin-bottom: 20px;'>
                                <h4 style='color: #0c5460; margin: 0 0 10px 0;'>📋 Next Steps:</h4>
                                <ul style='color: #0c5460; margin: 0; padding-left: 20px;'>
                                    <li>Contact the client to discuss renewal terms</li>
                                    <li>Update renewal status in the system</li>
                                    <li>Prepare renewal documentation</li>
                                    <li>Schedule follow-up reminders if needed</li>
                                </ul>
                            </div>
                            
                            <div style='text-align: center; margin-top: 30px;'>
                                <a href='#' style='background: #007bff; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                                    View License Details
                                </a>
                            </div>
                        </div>
                        
                        <div style='text-align: center; margin-top: 20px; color: #6c757d; font-size: 12px;'>
                            <p>This is an automated message from License Tracking System</p>
                            <p>Alert generated on {alert.CreatedDate:MMMM dd, yyyy} at {alert.CreatedDate:HH:mm}</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GenerateRenewalEmailTextBody(Alert alert)
        {
            return $@"
LICENSE RENEWAL ALERT - IMMEDIATE ATTENTION REQUIRED

Priority: {alert.Priority}
{alert.AlertMessage}

License Details:
- Product: {alert.License?.ProductName ?? "N/A"}
- Client: {alert.License?.ClientName ?? "N/A"}
- Expiry Date: {alert.License?.ExpiryDate.ToString("MMMM dd, yyyy") ?? "N/A"}
- Days Remaining: {alert.DaysBeforeExpiry} days

Next Steps:
1. Contact the client to discuss renewal terms
2. Update renewal status in the system
3. Prepare renewal documentation
4. Schedule follow-up reminders if needed

Alert generated on {alert.CreatedDate:MMMM dd, yyyy} at {alert.CreatedDate:HH:mm}

This is an automated message from License Tracking System.
            ";
        }

        private string GeneratePipelineEmailBody(Alert alert)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <div style='background: linear-gradient(135deg, #28a745 0%, #20c997 100%); color: white; padding: 30px; border-radius: 10px 10px 0 0;'>
                            <h1 style='margin: 0; font-size: 24px;'>📋 Pipeline Project Alert</h1>
                            <p style='margin: 10px 0 0 0; opacity: 0.9;'>Project Follow-up Required</p>
                        </div>
                        
                        <div style='background: white; padding: 30px; border: 1px solid #e1e5e9; border-radius: 0 0 10px 10px;'>
                            <div style='background: #d4edda; border: 1px solid #c3e6cb; border-radius: 5px; padding: 15px; margin-bottom: 20px;'>
                                <h3 style='color: #155724; margin: 0 0 10px 0;'>📊 Priority: {alert.Priority}</h3>
                                <p style='margin: 0; color: #155724;'>{alert.AlertMessage}</p>
                            </div>
                            
                            <h3 style='color: #495057;'>Project Details:</h3>
                            <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #dee2e6; font-weight: bold;'>Product:</td>
                                    <td style='padding: 8px; border-bottom: 1px solid #dee2e6;'>{alert.ProjectPipeline?.ProductName ?? "N/A"}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #dee2e6; font-weight: bold;'>Client:</td>
                                    <td style='padding: 8px; border-bottom: 1px solid #dee2e6;'>{alert.ProjectPipeline?.ClientName ?? "N/A"}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #dee2e6; font-weight: bold;'>Expected Date:</td>
                                    <td style='padding: 8px; border-bottom: 1px solid #dee2e6;'>{alert.ProjectPipeline?.ExpectedLicenseDate.ToString("MMMM dd, yyyy") ?? "N/A"}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #dee2e6; font-weight: bold;'>Status:</td>
                                    <td style='padding: 8px; border-bottom: 1px solid #dee2e6;'>{alert.ProjectPipeline?.ProjectStatus ?? "N/A"}</td>
                                </tr>
                            </table>
                            
                            <div style='text-align: center; margin-top: 30px;'>
                                <a href='#' style='background: #28a745; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                                    View Project Details
                                </a>
                            </div>
                        </div>
                        
                        <div style='text-align: center; margin-top: 20px; color: #6c757d; font-size: 12px;'>
                            <p>This is an automated message from License Tracking System</p>
                            <p>Alert generated on {alert.CreatedDate:MMMM dd, yyyy} at {alert.CreatedDate:HH:mm}</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GeneratePipelineEmailTextBody(Alert alert)
        {
            return $@"
PIPELINE PROJECT ALERT - FOLLOW-UP REQUIRED

Priority: {alert.Priority}
{alert.AlertMessage}

Project Details:
- Product: {alert.ProjectPipeline?.ProductName ?? "N/A"}
- Client: {alert.ProjectPipeline?.ClientName ?? "N/A"}
- Expected Date: {alert.ProjectPipeline?.ExpectedLicenseDate.ToString("MMMM dd, yyyy") ?? "N/A"}
- Status: {alert.ProjectPipeline?.ProjectStatus ?? "N/A"}

Alert generated on {alert.CreatedDate:MMMM dd, yyyy} at {alert.CreatedDate:HH:mm}

This is an automated message from License Tracking System.
            ";
        }

        private string GeneratePaymentReminderEmailBody(Invoice invoice)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <div style='background: linear-gradient(135deg, #ffc107 0%, #fd7e14 100%); color: white; padding: 30px; border-radius: 10px 10px 0 0;'>
                            <h1 style='margin: 0; font-size: 24px;'>💰 Payment Reminder</h1>
                            <p style='margin: 10px 0 0 0; opacity: 0.9;'>Payment Due</p>
                        </div>
                        
                        <div style='background: white; padding: 30px; border: 1px solid #e1e5e9; border-radius: 0 0 10px 10px;'>
                            <h3 style='color: #495057;'>Invoice Details:</h3>
                            <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #dee2e6; font-weight: bold;'>Invoice Number:</td>
                                    <td style='padding: 8px; border-bottom: 1px solid #dee2e6;'>{invoice.InvoiceNumber}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #dee2e6; font-weight: bold;'>Amount:</td>
                                    <td style='padding: 8px; border-bottom: 1px solid #dee2e6; font-weight: bold; color: #dc3545;'>{invoice.Amount:C}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #dee2e6; font-weight: bold;'>Due Date:</td>
                                    <td style='padding: 8px; border-bottom: 1px solid #dee2e6;'>{invoice.DueDate:MMMM dd, yyyy}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #dee2e6; font-weight: bold;'>Status:</td>
                                    <td style='padding: 8px; border-bottom: 1px solid #dee2e6;'>{invoice.PaymentStatus}</td>
                                </tr>
                            </table>
                            
                            <div style='text-align: center; margin-top: 30px;'>
                                <a href='#' style='background: #ffc107; color: #212529; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                                    View Invoice Details
                                </a>
                            </div>
                        </div>
                        
                        <div style='text-align: center; margin-top: 20px; color: #6c757d; font-size: 12px;'>
                            <p>This is an automated payment reminder from License Tracking System</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GeneratePaymentReminderTextBody(Invoice invoice)
        {
            return $@"
PAYMENT REMINDER

Invoice Details:
- Invoice Number: {invoice.InvoiceNumber}
- Amount: {invoice.Amount:C}
- Due Date: {invoice.DueDate:MMMM dd, yyyy}
- Status: {invoice.PaymentStatus}

This is an automated payment reminder from License Tracking System.
            ";
        }
    }
}
