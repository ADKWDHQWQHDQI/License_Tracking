using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using License_Tracking.Data;
using License_Tracking.Models;
using System.Text.Json;

namespace License_Tracking.Controllers
{
    [Authorize(Roles = "Admin,Management")]
    public class AlertConfigurationController : Controller
    {
        private readonly AppDbContext _context;

        public AlertConfigurationController(AppDbContext context)
        {
            _context = context;
        }

        // GET: AlertConfiguration
        public async Task<IActionResult> Index()
        {
            var configurations = await _context.AlertConfigurations
                .OrderByDescending(ac => ac.IsDefault)
                .ThenBy(ac => ac.ConfigurationName)
                .ToListAsync();

            return View(configurations);
        }

        // GET: AlertConfiguration/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alertConfiguration = await _context.AlertConfigurations
                .Include(ac => ac.Licenses)
                .FirstOrDefaultAsync(m => m.AlertConfigurationId == id);

            if (alertConfiguration == null)
            {
                return NotFound();
            }

            return View(alertConfiguration);
        }

        // GET: AlertConfiguration/Create
        public IActionResult Create()
        {
            var model = new AlertConfiguration
            {
                ConfigurationName = "",
                AlertThresholds = "[90,60,45,30,15,7,3,1]" // Default enhanced thresholds
            };
            return View(model);
        }

        // POST: AlertConfiguration/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ConfigurationName,AlertThresholds,IsDefault,IsActive,Description")] AlertConfiguration alertConfiguration)
        {
            if (ModelState.IsValid)
            {
                // Validate JSON format
                try
                {
                    var thresholds = JsonSerializer.Deserialize<List<int>>(alertConfiguration.AlertThresholds);
                    if (thresholds == null || !thresholds.Any())
                    {
                        ModelState.AddModelError("AlertThresholds", "Alert thresholds must contain at least one value.");
                        return View(alertConfiguration);
                    }

                    // Sort thresholds in descending order
                    alertConfiguration.SetThresholds(thresholds);
                }
                catch (JsonException)
                {
                    ModelState.AddModelError("AlertThresholds", "Invalid JSON format. Use format: [90,60,45,30,15,7,3,1]");
                    return View(alertConfiguration);
                }

                // If this is set as default, remove default from others
                if (alertConfiguration.IsDefault)
                {
                    var existingDefaults = await _context.AlertConfigurations
                        .Where(ac => ac.IsDefault)
                        .ToListAsync();

                    foreach (var existing in existingDefaults)
                    {
                        existing.IsDefault = false;
                        existing.ModifiedDate = DateTime.Now;
                        existing.ModifiedBy = User.Identity?.Name ?? "System";
                    }
                }

                alertConfiguration.CreatedBy = User.Identity?.Name ?? "Admin";
                alertConfiguration.CreatedDate = DateTime.Now;

                _context.Add(alertConfiguration);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Alert configuration created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(alertConfiguration);
        }

        // GET: AlertConfiguration/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alertConfiguration = await _context.AlertConfigurations.FindAsync(id);
            if (alertConfiguration == null)
            {
                return NotFound();
            }
            return View(alertConfiguration);
        }

        // POST: AlertConfiguration/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AlertConfigurationId,ConfigurationName,AlertThresholds,IsDefault,IsActive,Description,CreatedDate,CreatedBy")] AlertConfiguration alertConfiguration)
        {
            if (id != alertConfiguration.AlertConfigurationId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Validate JSON format
                    try
                    {
                        var thresholds = JsonSerializer.Deserialize<List<int>>(alertConfiguration.AlertThresholds);
                        if (thresholds == null || !thresholds.Any())
                        {
                            ModelState.AddModelError("AlertThresholds", "Alert thresholds must contain at least one value.");
                            return View(alertConfiguration);
                        }

                        // Sort thresholds in descending order
                        alertConfiguration.SetThresholds(thresholds);
                    }
                    catch (JsonException)
                    {
                        ModelState.AddModelError("AlertThresholds", "Invalid JSON format. Use format: [90,60,45,30,15,7,3,1]");
                        return View(alertConfiguration);
                    }

                    // If this is set as default, remove default from others
                    if (alertConfiguration.IsDefault)
                    {
                        var existingDefaults = await _context.AlertConfigurations
                            .Where(ac => ac.IsDefault && ac.AlertConfigurationId != id)
                            .ToListAsync();

                        foreach (var existing in existingDefaults)
                        {
                            existing.IsDefault = false;
                            existing.ModifiedDate = DateTime.Now;
                            existing.ModifiedBy = User.Identity?.Name ?? "System";
                        }
                    }

                    alertConfiguration.ModifiedDate = DateTime.Now;
                    alertConfiguration.ModifiedBy = User.Identity?.Name ?? "Admin";

                    _context.Update(alertConfiguration);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Alert configuration updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlertConfigurationExists(alertConfiguration.AlertConfigurationId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(alertConfiguration);
        }

        // GET: AlertConfiguration/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alertConfiguration = await _context.AlertConfigurations
                .Include(ac => ac.Licenses)
                .FirstOrDefaultAsync(m => m.AlertConfigurationId == id);

            if (alertConfiguration == null)
            {
                return NotFound();
            }

            return View(alertConfiguration);
        }

        // POST: AlertConfiguration/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alertConfiguration = await _context.AlertConfigurations
                .Include(ac => ac.Licenses)
                .FirstOrDefaultAsync(ac => ac.AlertConfigurationId == id);

            if (alertConfiguration != null)
            {
                // Check if this configuration is being used by any licenses
                if (alertConfiguration.Licenses.Any())
                {
                    TempData["ErrorMessage"] = $"Cannot delete this alert configuration as it is being used by {alertConfiguration.Licenses.Count} license(s).";
                    return RedirectToAction(nameof(Index));
                }

                // Don't allow deletion of the default configuration if it's the only one
                if (alertConfiguration.IsDefault)
                {
                    var totalConfigs = await _context.AlertConfigurations.CountAsync();
                    if (totalConfigs == 1)
                    {
                        TempData["ErrorMessage"] = "Cannot delete the only alert configuration. Create another one first.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                _context.AlertConfigurations.Remove(alertConfiguration);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Alert configuration deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Set as Default
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetAsDefault(int id)
        {
            var alertConfiguration = await _context.AlertConfigurations.FindAsync(id);
            if (alertConfiguration == null)
            {
                return NotFound();
            }

            // Remove default from all configurations
            var allConfigs = await _context.AlertConfigurations.ToListAsync();
            foreach (var config in allConfigs)
            {
                config.IsDefault = (config.AlertConfigurationId == id);
                config.ModifiedDate = DateTime.Now;
                config.ModifiedBy = User.Identity?.Name ?? "Admin";
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"'{alertConfiguration.ConfigurationName}' has been set as the default alert configuration.";

            return RedirectToAction(nameof(Index));
        }

        private bool AlertConfigurationExists(int id)
        {
            return _context.AlertConfigurations.Any(e => e.AlertConfigurationId == id);
        }
    }
}
