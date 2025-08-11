using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using License_Tracking.Data;
using License_Tracking.Models;
using Microsoft.AspNetCore.Authorization;

namespace License_Tracking.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index(string searchString, int? oemId)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["OemFilter"] = oemId;
            ViewData["OemList"] = new SelectList(await _context.Oems.Where(o => o.IsActive).ToListAsync(), "OemId", "OemName");

            var products = from p in _context.Products.Include(p => p.Oem)
                           select p;

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.ProductName.Contains(searchString) ||
                                             p.ProductCategory!.Contains(searchString));
            }

            if (oemId.HasValue)
            {
                products = products.Where(p => p.OemId == oemId);
            }

            products = products.Where(p => p.IsActive)
                              .OrderBy(p => p.Oem.OemName)
                              .ThenBy(p => p.ProductName);

            return View(await products.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Oem)
                .Include(p => p.Deals)
                    .ThenInclude(d => d.Company)
                .FirstOrDefaultAsync(m => m.ProductId == id && m.IsActive);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public async Task<IActionResult> Create()
        {
            var oems = await _context.Oems.Where(o => o.IsActive).OrderBy(o => o.OemName).ToListAsync();
            ViewData["OemId"] = new SelectList(oems, "OemId", "OemName");

            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OemId,ProductName,ProductCategory,UnitPrice,LicenseType,ProductDescription,MinimumQuantity")] Product product)
        {
            try
            {
                product.CreatedDate = DateTime.Now;
                product.IsActive = true;
                _context.Add(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Product '{product.ProductName}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while saving the product. Please try again.";
            }

            // Reload the OEM list for the dropdown
            var oems = await _context.Oems.Where(o => o.IsActive).OrderBy(o => o.OemName).ToListAsync();
            ViewData["OemId"] = new SelectList(oems, "OemId", "OemName", product.OemId);

            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null || !product.IsActive)
            {
                return NotFound();
            }
            ViewData["OemId"] = new SelectList(await _context.Oems.Where(o => o.IsActive).ToListAsync(), "OemId", "OemName", product.OemId);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,OemId,ProductName,ProductCategory,UnitPrice,LicenseType,CreatedDate,IsActive")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Product updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
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
            ViewData["OemId"] = new SelectList(await _context.Oems.Where(o => o.IsActive).ToListAsync(), "OemId", "OemName", product.OemId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Oem)
                .FirstOrDefaultAsync(m => m.ProductId == id && m.IsActive);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // Soft delete
                product.IsActive = false;
                _context.Update(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product archived successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id && e.IsActive);
        }

        // AJAX method for product search in dropdowns
        [HttpGet]
        public async Task<JsonResult> GetProducts(string term, int? oemId = null)
        {
            var query = _context.Products.Where(p => p.IsActive && p.ProductName.Contains(term));

            if (oemId.HasValue)
            {
                query = query.Where(p => p.OemId == oemId);
            }

            var products = await query
                .Select(p => new
                {
                    id = p.ProductId,
                    text = p.ProductName,
                    price = p.UnitPrice,
                    category = p.ProductCategory
                })
                .Take(10)
                .ToListAsync();

            return Json(products);
        }
    }
}
