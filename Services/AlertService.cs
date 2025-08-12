using License_Tracking.Data;
using License_Tracking.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace License_Tracking.Services
{
    public interface IAlertService
    {
        Task GenerateRenewalAlertsAsync();
        Task GeneratePaymentAlertsAsync();
        Task<List<Alert>> GetPendingAlertsAsync();
        Task<List<Alert>> GetAlertsForUserAsync(string userId);
        Task CreateAlertAsync(Alert alert);
        Task MarkAlertAsSentAsync(int alertId);
        Task DismissAlertAsync(int alertId);
        Task<int> GetPendingAlertCountAsync();
        Task ProcessPendingEmailAlertsAsync();
        Task ProcessAutoRenewalAlertsAsync();
        Task<bool> ShouldSendRenewalReminderAsync(int dealId);
    }

    public class AlertService : IAlertService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<AlertService> _logger;

        public AlertService(AppDbContext context, IEmailService emailService, ILogger<AlertService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task GenerateRenewalAlertsAsync()
        {
            var today = DateTime.Today;

            // Get all active licenses that need alerts with their alert configurations
            var licensesNeedingAlerts = await _context.Deals
                .Include(l => l.AlertConfiguration)
                .Where(l => l.LicenseDeliveryStatus == "Active" &&
                           l.AlertsEnabled == true &&
                           l.IsProjectPipeline != true)
                .ToListAsync();

            // Get default alert configuration if none exists
            var defaultAlertConfig = await _context.AlertConfigurations
                .FirstOrDefaultAsync(ac => ac.IsDefault && ac.IsActive);

            if (defaultAlertConfig == null)
            {
                // Create default configuration if it doesn't exist
                defaultAlertConfig = new AlertConfiguration
                {
                    ConfigurationName = "Default Alert Configuration",
                    AlertThresholds = "[90,60,45,30,15,7,3,1]", // Enhanced with 90 and 60 day alerts
                    IsDefault = true,
                    IsActive = true,
                    Description = "Default system alert configuration with multiple thresholds",
                    CreatedBy = "System",
                    CreatedDate = DateTime.Now
                };
                _context.AlertConfigurations.Add(defaultAlertConfig);
                await _context.SaveChangesAsync();
            }

            foreach (var license in licensesNeedingAlerts)
            {
                if (!license.LicenseEndDate.HasValue) continue;

                // Calculate days until expiry
                var daysUntilExpiry = (license.LicenseEndDate.Value.Date - today).Days;

                // Get alert thresholds from license's configuration or default
                var alertConfig = license.AlertConfiguration ?? defaultAlertConfig;
                var alertThresholds = alertConfig.GetThresholds();

                foreach (var threshold in alertThresholds)
                {
                    if (daysUntilExpiry == threshold)
                    {
                        // Check if alert already exists for this threshold
                        var existingAlert = await _context.Alerts
                            .FirstOrDefaultAsync(a => a.DealId == license.DealId &&
                                                    a.AlertType == AlertType.Renewal &&
                                                    a.DaysBeforeExpiry == threshold &&
                                                    a.Status == "Pending");

                        if (existingAlert == null)
                        {
                            var priority = DeterminePriority(daysUntilExpiry);
                            var urgencyText = GetUrgencyText(daysUntilExpiry);

                            var alert = new Alert
                            {
                                DealId = license.DealId,
                                AlertType = AlertType.Renewal,
                                Priority = priority,
                                Title = $"{urgencyText} License Renewal - {license.ProductName}",
                                AlertMessage = $"License for {license.ProductName} (Client: {license.ClientName}) expires on {license.LicenseEndDate:MMM dd, yyyy}. Only {daysUntilExpiry} days remaining! Cost: ${license.OemQuoteAmount:N2}, Revenue: ${license.CustomerInvoiceAmount:N2}",
                                AlertDate = today,
                                Status = "Pending",
                                DaysBeforeExpiry = threshold,
                                CreatedBy = "System",
                                CreatedDate = DateTime.Now
                            };

                            await CreateAlertAsync(alert);
                            _logger.LogInformation($"Created renewal alert for Deal {license.DealId} - {threshold} days before expiry");
                        }
                    }
                }

                // Create overdue alerts for expired licenses
                if (daysUntilExpiry < 0)
                {
                    var existingOverdueAlert = await _context.Alerts
                        .FirstOrDefaultAsync(a => a.DealId == license.DealId &&
                                                a.AlertType == AlertType.Renewal &&
                                                a.Status == "Pending" &&
                                                a.AlertMessage!.Contains("OVERDUE"));

                    if (existingOverdueAlert == null)
                    {
                        var alert = new Alert
                        {
                            DealId = license.DealId,
                            AlertType = AlertType.Renewal,
                            Priority = AlertPriority.Critical,
                            Title = $"OVERDUE: License Expired - {license.ProductName}",
                            AlertMessage = $"CRITICAL: License for {license.ProductName} (Client: {license.ClientName}) expired {Math.Abs(daysUntilExpiry)} days ago on {license.LicenseEndDate:MMM dd, yyyy}. Immediate action required!",
                            AlertDate = today,
                            Status = "Pending",
                            DaysBeforeExpiry = daysUntilExpiry,
                            CreatedBy = "System",
                            CreatedDate = DateTime.Now
                        };

                        await CreateAlertAsync(alert);
                        _logger.LogWarning($"Created OVERDUE alert for Deal {license.DealId} - expired {Math.Abs(daysUntilExpiry)} days ago");
                    }
                }
            }

            // Generate alerts for pipeline projects
            await GeneratePipelineAlertsAsync();
        }

        public async Task GeneratePaymentAlertsAsync()
        {
            var today = DateTime.Today;

            // Get licenses with pending payments from customers
            var licensesWithPendingPayments = await _context.Deals
                .Where(l => l.CustomerPaymentStatus == "Pending" &&
                           l.CustomerInvoiceAmount > 0 &&
                           l.LicenseDeliveryStatus == "Active")
                .ToListAsync();

            foreach (var license in licensesWithPendingPayments)
            {
                var outstandingAmount = license.CustomerInvoiceAmount ?? 0;
                var daysSinceLicense = license.LicenseStartDate.HasValue ? (today - license.LicenseStartDate.Value).Days : 0;

                // Create payment reminder alerts based on days since license date
                var shouldCreateAlert = false;
                var alertPriority = AlertPriority.Low;
                var alertTitle = "";

                if (daysSinceLicense >= 60) // 60+ days
                {
                    shouldCreateAlert = true;
                    alertPriority = AlertPriority.Critical;
                    alertTitle = "URGENT: Payment Overdue";
                }
                else if (daysSinceLicense >= 30) // 30+ days
                {
                    shouldCreateAlert = true;
                    alertPriority = AlertPriority.High;
                    alertTitle = "Payment Follow-up Required";
                }
                else if (daysSinceLicense >= 15) // 15+ days
                {
                    shouldCreateAlert = true;
                    alertPriority = AlertPriority.Medium;
                    alertTitle = "Payment Reminder";
                }

                if (shouldCreateAlert)
                {
                    // Check if recent alert exists (within last 7 days)
                    var recentAlert = await _context.Alerts
                        .FirstOrDefaultAsync(a => a.DealId == license.DealId &&
                                                a.AlertType == AlertType.Payment &&
                                                a.CreatedDate >= today.AddDays(-7) &&
                                                a.Status == "Pending");

                    if (recentAlert == null)
                    {
                        var alert = new Alert
                        {
                            DealId = license.DealId,
                            AlertType = AlertType.Payment,
                            Priority = alertPriority,
                            Title = $"{alertTitle} - {license.ClientName}",
                            AlertMessage = $"Outstanding payment of ${outstandingAmount:N2} for {license.ProductName} from {license.ClientName}. License issued {daysSinceLicense} days ago. Total amount: ${license.CustomerInvoiceAmount:N2}",
                            AlertDate = today,
                            Status = "Pending",
                            CreatedBy = "System",
                            CreatedDate = DateTime.Now
                        };

                        await CreateAlertAsync(alert);
                        _logger.LogInformation($"Created payment alert for Deal {license.DealId} - ${outstandingAmount:N2} outstanding");
                    }
                }
            }
        }

        public async Task ProcessAutoRenewalAlertsAsync()
        {
            var lastRun = DateTime.Now.AddHours(-36); // Check if 36 hours have passed

            // Check when the last auto-renewal process ran
            var lastAutoAlert = await _context.Alerts
                .Where(a => a.CreatedBy == "AutoRenewal" && a.AlertType == AlertType.Renewal)
                .OrderByDescending(a => a.CreatedDate)
                .FirstOrDefaultAsync();

            // Run if no auto alerts exist or if 36 hours have passed
            if (lastAutoAlert == null || lastAutoAlert.CreatedDate <= lastRun)
            {
                await GenerateRenewalAlertsAsync();
                _logger.LogInformation("Auto-renewal alerts processed - 36 hour cycle");
            }
        }

        public async Task<bool> ShouldSendRenewalReminderAsync(int dealId)
        {
            var lastEmailAlert = await _context.Alerts
                .Where(a => a.DealId == dealId &&
                           a.AlertType == AlertType.Renewal &&
                           a.EmailSent)
                .OrderByDescending(a => a.EmailSentDate)
                .FirstOrDefaultAsync();

            // Send email if no email sent or last email was sent more than 36 hours ago
            return lastEmailAlert == null ||
                   (lastEmailAlert.EmailSentDate.HasValue &&
                    lastEmailAlert.EmailSentDate.Value <= DateTime.Now.AddHours(-36));
        }

        private static string GetUrgencyText(int daysUntilExpiry)
        {
            return daysUntilExpiry switch
            {
                <= 1 => "CRITICAL",
                <= 3 => "URGENT",
                <= 7 => "HIGH PRIORITY",
                <= 15 => "IMPORTANT",
                <= 30 => "NOTICE",
                _ => "REMINDER"
            };
        }

        public async Task<List<Alert>> GetPendingAlertsAsync()
        {
            return await _context.Alerts
                .Include(a => a.License)
                .Include(a => a.ProjectPipeline)
                .Where(a => a.Status == "Pending")
                .OrderByDescending(a => a.Priority)
                .ThenBy(a => a.AlertDate)
                .ToListAsync();
        }

        public async Task<List<Alert>> GetAlertsForUserAsync(string userId)
        {
            // For now, return all alerts. In future, implement user-specific filtering
            return await _context.Alerts
                .Include(a => a.License)
                .Include(a => a.ProjectPipeline)
                .Where(a => a.Status == "Pending" || a.Status == "Sent")
                .OrderByDescending(a => a.Priority)
                .ThenBy(a => a.AlertDate)
                .Take(50)
                .ToListAsync();
        }

        public async Task CreateAlertAsync(Alert alert)
        {
            _context.Alerts.Add(alert);
            await _context.SaveChangesAsync();
        }

        public async Task MarkAlertAsSentAsync(int alertId)
        {
            var alert = await _context.Alerts.FindAsync(alertId);
            if (alert != null)
            {
                alert.Status = "Sent";
                alert.SentDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DismissAlertAsync(int alertId)
        {
            var alert = await _context.Alerts.FindAsync(alertId);
            if (alert != null)
            {
                alert.Status = "Dismissed";
                alert.DismissedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetPendingAlertCountAsync()
        {
            return await _context.Alerts
                .Where(a => a.Status == "Pending")
                .CountAsync();
        }

        public async Task ProcessPendingEmailAlertsAsync()
        {
            try
            {
                var pendingAlerts = await _context.Alerts
                    .Include(a => a.License)
                    .Include(a => a.ProjectPipeline)
                    .Where(a => a.Status == "Pending" && !a.EmailSent)
                    .ToListAsync();

                if (!pendingAlerts.Any())
                {
                    _logger.LogInformation("No pending email alerts to process");
                    return;
                }

                _logger.LogInformation($"Processing {pendingAlerts.Count} pending email alerts");

                foreach (var alert in pendingAlerts)
                {
                    try
                    {
                        if (alert.AlertType == AlertType.Renewal)
                        {
                            await _emailService.SendRenewalAlertAsync(alert);
                        }
                        else if (alert.AlertType == AlertType.PipelineReminder)
                        {
                            await _emailService.SendPipelineAlertAsync(alert);
                        }

                        // Mark alert as email sent
                        alert.EmailSent = true;
                        alert.EmailSentDate = DateTime.Now;
                        _context.Alerts.Update(alert);

                        _logger.LogInformation($"Email sent successfully for Alert ID {alert.AlertId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to send email for Alert ID {alert.AlertId}");
                        // Continue with other alerts even if one fails
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Completed processing email alerts");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessPendingEmailAlertsAsync");
                throw;
            }
        }

        #region Private Helper Methods

        private async Task GeneratePipelineAlertsAsync()
        {
            var today = DateTime.Today;

            var pipelineProjects = await _context.ProjectPipelines
                .Where(pp => pp.ProjectStatus == "Pipeline" && pp.AlertsEnabled)
                .ToListAsync();

            foreach (var project in pipelineProjects)
            {
                var alertDate = project.ExpectedLicenseDate.AddDays(-project.AlertDaysBefore);

                var existingAlert = await _context.Alerts
                    .FirstOrDefaultAsync(a => a.ProjectPipelineId == project.ProjectPipelineId &&
                                            a.AlertType == AlertType.PipelineReminder &&
                                            a.AlertDate.Date == alertDate.Date);

                if (existingAlert == null && alertDate <= today.AddDays(1))
                {
                    var daysUntilExpected = (project.ExpectedLicenseDate - today).Days;
                    var priority = DeterminePriority(daysUntilExpected);

                    var alert = new Alert
                    {
                        ProjectPipelineId = project.ProjectPipelineId,
                        AlertType = AlertType.PipelineReminder,
                        Priority = priority,
                        Title = $"Pipeline Project Reminder - {project.ProductName}",
                        AlertMessage = $"Pipeline project for {project.ProductName} ({project.ClientName}) expected license date is {project.ExpectedLicenseDate:yyyy-MM-dd} ({daysUntilExpected} days remaining)",
                        AlertDate = alertDate,
                        Status = "Pending",
                        DaysBeforeExpiry = project.AlertDaysBefore,
                        CreatedBy = "System",
                        CreatedDate = DateTime.Now
                    };

                    await CreateAlertAsync(alert);
                }
            }
        }

        private static AlertPriority DeterminePriority(int daysUntil)
        {
            return daysUntil switch
            {
                <= 7 => AlertPriority.Critical,
                <= 15 => AlertPriority.High,
                <= 30 => AlertPriority.Medium,
                _ => AlertPriority.Low
            };
        }

        #endregion
    }
}
