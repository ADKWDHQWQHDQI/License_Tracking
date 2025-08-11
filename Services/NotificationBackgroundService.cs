using License_Tracking.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace License_Tracking.Services
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationBackgroundService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(1); // Check every hour

        public NotificationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<NotificationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();

                    // Generate renewal alerts (runs every 36 hours internally)
                    await alertService.ProcessAutoRenewalAlertsAsync();

                    // Generate payment alerts (runs every time - has internal logic)
                    await alertService.GeneratePaymentAlertsAsync();

                    _logger.LogInformation("Background notification service completed cycle at {Time}", DateTime.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in background notification service");
                }

                await Task.Delay(_period, stoppingToken);
            }
        }
    }
}
