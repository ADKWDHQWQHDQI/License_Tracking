using Microsoft.EntityFrameworkCore;
using License_Tracking.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace License_Tracking.Data
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Existing DbSets (Legacy - being phased out)
        public DbSet<CustomerPo> CustomerPOs { get; set; }
        public DbSet<OemPo> OemPOs { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<AlertConfiguration> AlertConfigurations { get; set; }
        public DbSet<Renewal> Renewals { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<Report> Reports { get; set; }

        // New DbSets for enhanced functionality
        public DbSet<ProjectPipeline> ProjectPipelines { get; set; }

        // CBMS (B2B2B CRM) DbSets - Updated for new requirements
        public DbSet<Company> Companies { get; set; }
        public DbSet<ContactPerson> ContactPersons { get; set; } // Updated from Contact
        public DbSet<Oem> Oems { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<CustomerOemProduct> CustomerOemProducts { get; set; }
        public DbSet<Deal> Deals { get; set; }
        public DbSet<CbmsInvoice> CbmsInvoices { get; set; }
        public DbSet<BATarget> BATargets { get; set; } // Updated from Target
        public DbSet<Activity> Activities { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure decimal precision for related entities
            builder.Entity<CustomerPo>()
                .Property(cp => cp.PoAmount)
                .HasPrecision(18, 2);

            builder.Entity<OemPo>()
                .Property(op => op.PoAmount)
                .HasPrecision(18, 2);

            // Configure decimal precision for Invoice
            builder.Entity<Invoice>()
                .Property(i => i.Amount)
                .HasPrecision(18, 2);

            builder.Entity<Invoice>()
                .Property(i => i.AmountReceived)
                .HasPrecision(18, 2);

            builder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            builder.Entity<Renewal>()
                .Property(r => r.RenewalAmount)
                .HasPrecision(18, 2);

            // Configure decimal precision for ProjectPipeline
            builder.Entity<ProjectPipeline>()
                .Property(pp => pp.ExpectedCustomerPoAmount)
                .HasPrecision(18, 2);

            builder.Entity<ProjectPipeline>()
                .Property(pp => pp.ExpectedOemPoAmount)
                .HasPrecision(18, 2);

            builder.Entity<ProjectPipeline>()
                .Property(pp => pp.ExpectedAmountToReceive)
                .HasPrecision(18, 2);

            builder.Entity<ProjectPipeline>()
                .Property(pp => pp.ExpectedAmountToPay)
                .HasPrecision(18, 2);

            builder.Entity<ProjectPipeline>()
                .Property(pp => pp.ProjectedMarginInput)
                .HasPrecision(18, 2);
            // Configure decimal precision for PurchaseOrder
            builder.Entity<PurchaseOrder>()
                .Property(po => po.OemPoAmount)
                .HasPrecision(18, 2);

            builder.Entity<PurchaseOrder>()
                .Property(po => po.AmountPaid)
                .HasPrecision(18, 2);

            builder.Entity<ProjectPipeline>()
                .Ignore(pp => pp.ProjectedMargin)
                .Ignore(pp => pp.ProjectedProfitMargin)
                .Ignore(pp => pp.ProjectedRevenue);

            builder.Entity<Alert>()
                .Ignore(a => a.IsOverdue)
                .Ignore(a => a.DaysUntilAlert);

            // Configure AlertConfiguration decimal precision
            builder.Entity<AlertConfiguration>()
                .HasKey(ac => ac.AlertConfigurationId);

            builder.Entity<AlertConfiguration>()
                .Property(ac => ac.AlertThresholds)
                .HasMaxLength(1000);

            // Configure relationships for existing entities
            builder.Entity<CustomerPo>()
                .HasOne(cp => cp.Deal)
                .WithMany()
                .HasForeignKey(cp => cp.DealId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OemPo>()
                .HasOne(op => op.Deal)
                .WithMany()
                .HasForeignKey(op => op.DealId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Invoice>()
                .HasOne(i => i.Deal)
                .WithMany()
                .HasForeignKey(i => i.DealId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Renewal>()
                .HasOne(r => r.Deal)
                .WithMany()
                .HasForeignKey(r => r.DealId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Payment>()
                .HasOne(p => p.Invoice)
                .WithMany()
                .HasForeignKey(p => p.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PurchaseOrder>()
                .HasOne(p => p.Deal)
                .WithMany()
                .HasForeignKey(p => p.DealId);

            // Configure relationships for new entities
            builder.Entity<Alert>()
                .HasOne(a => a.Deal)
                .WithMany()
                .HasForeignKey(a => a.DealId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Alert>()
                .HasOne(a => a.ProjectPipeline)
                .WithMany()
                .HasForeignKey(a => a.ProjectPipelineId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ProjectPipeline>()
                .HasOne(pp => pp.ConvertedToDeal)
                .WithMany()
                .HasForeignKey(pp => pp.ConvertedToLicenseId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure indexes for better performance  
            builder.Entity<Deal>()
                .HasIndex(d => d.LicenseEndDate)
                .HasDatabaseName("IX_Deal_LicenseEndDate");

            builder.Entity<Deal>()
                .HasIndex(d => d.DealStage)
                .HasDatabaseName("IX_Deal_DealStage");

            builder.Entity<ProjectPipeline>()
                .HasIndex(pp => pp.ExpectedLicenseDate)
                .HasDatabaseName("IX_ProjectPipeline_ExpectedLicenseDate");

            builder.Entity<ProjectPipeline>()
                .HasIndex(pp => pp.ProjectStatus)
                .HasDatabaseName("IX_ProjectPipeline_ProjectStatus");

            builder.Entity<Alert>()
                .HasIndex(a => a.AlertDate)
                .HasDatabaseName("IX_Alert_AlertDate");

            builder.Entity<Alert>()
                .HasIndex(a => a.Status)
                .HasDatabaseName("IX_Alert_Status");


            builder.Entity<Invoice>()
                .HasIndex(i => i.InvoiceType)
                .HasDatabaseName("IX_Invoice_InvoiceType");

            builder.Entity<Invoice>()
                .HasIndex(i => i.PaymentStatus)
                .HasDatabaseName("IX_Invoice_PaymentStatus");

            // CBMS (B2B2B CRM) Model Configurations

            // Configure CustomerOemProduct unique constraint
            builder.Entity<CustomerOemProduct>()
                .HasIndex(cop => new { cop.CompanyId, cop.OemId, cop.ProductId })
                .IsUnique()
                .HasDatabaseName("IX_CustomerOemProduct_Unique");

            // Configure Deal relationships
            builder.Entity<Deal>()
                .HasOne(d => d.Company)
                .WithMany(c => c.Deals)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Deal>()
                .HasOne(d => d.Oem)
                .WithMany(o => o.Deals)
                .HasForeignKey(d => d.OemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Deal>()
                .HasOne(d => d.Product)
                .WithMany(p => p.Deals)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ContactPerson relationship
            builder.Entity<ContactPerson>()
                .HasOne(c => c.Company)
                .WithMany(co => co.ContactPersons)
                .HasForeignKey(c => c.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Product relationship
            builder.Entity<Product>()
                .HasOne(p => p.Oem)
                .WithMany(o => o.Products)
                .HasForeignKey(p => p.OemId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure decimal precision for CBMS entities
            builder.Entity<Oem>()
                .Property(o => o.PerformanceRating)
                .HasPrecision(3, 2);

            builder.Entity<Product>()
                .Property(p => p.UnitPrice)
                .HasPrecision(18, 2);

            builder.Entity<Deal>()
                .Property(d => d.CustomerInvoiceAmount)
                .HasPrecision(18, 2);

            builder.Entity<Deal>()
                .Property(d => d.OemQuoteAmount)
                .HasPrecision(18, 2);

            builder.Entity<Deal>()
                .Property(d => d.EstimatedMargin)
                .HasPrecision(18, 2);

            builder.Entity<Deal>()
                .Property(d => d.OemInvoiceAmount)
                .HasPrecision(18, 2);

            builder.Entity<Deal>()
                .Property(d => d.DealProbability)
                .HasPrecision(3, 2);

            builder.Entity<CbmsInvoice>()
                .Property(i => i.Amount)
                .HasPrecision(18, 2);

            // Configure CustomerOemProduct relationships
            builder.Entity<CustomerOemProduct>()
                .HasOne(cop => cop.Company)
                .WithMany(c => c.CustomerOemProducts)
                .HasForeignKey(cop => cop.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CustomerOemProduct>()
                .HasOne(cop => cop.Oem)
                .WithMany(o => o.CustomerOemProducts)
                .HasForeignKey(cop => cop.OemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CustomerOemProduct>()
                .HasOne(cop => cop.Product)
                .WithMany(p => p.CustomerOemProducts)
                .HasForeignKey(cop => cop.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure unique constraint for CustomerOemProducts
            builder.Entity<CustomerOemProduct>()
                .HasIndex(cop => new { cop.CompanyId, cop.OemId, cop.ProductId })
                .IsUnique()
                .HasDatabaseName("IX_CustomerOemProduct_Unique");

            // Configure CbmsInvoice relationship and ensure unique invoice numbers
            builder.Entity<CbmsInvoice>()
                .HasOne(i => i.Deal)
                .WithMany(d => d.Invoices)
                .HasForeignKey(i => i.DealId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CbmsInvoice>()
                .HasIndex(i => i.InvoiceNumber)
                .IsUnique()
                .HasDatabaseName("IX_CbmsInvoice_InvoiceNumber");

            // Configure Deal relationships with ContactPerson
            builder.Entity<Deal>()
                .HasOne(d => d.ContactPerson)
                .WithMany(c => c.Deals)
                .HasForeignKey(d => d.ContactId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure indexes for performance
            builder.Entity<Deal>()
                .HasIndex(d => d.DealStage)
                .HasDatabaseName("IX_Deal_DealStage");

            builder.Entity<Deal>()
                .HasIndex(d => d.AssignedTo)
                .HasDatabaseName("IX_Deal_AssignedTo");

            builder.Entity<Company>()
                .HasIndex(c => c.CompanyName)
                .HasDatabaseName("IX_Company_CompanyName");

            builder.Entity<ContactPerson>()
                .HasIndex(c => c.Email)
                .HasDatabaseName("IX_ContactPerson_Email");

            // Configure Activity relationships to prevent cascade cycles
            builder.Entity<Activity>()
                .HasOne<Deal>()
                .WithMany()
                .HasForeignKey(a => a.RelatedEntityId)
                .HasPrincipalKey(d => d.DealId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Activity>()
                .HasOne<Company>()
                .WithMany()
                .HasForeignKey(a => a.RelatedEntityId)
                .HasPrincipalKey(c => c.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Activity>()
                .HasOne<ContactPerson>()
                .WithMany()
                .HasForeignKey(a => a.RelatedEntityId)
                .HasPrincipalKey(cp => cp.ContactId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Activity>()
                .HasIndex(a => new { a.RelatedEntityType, a.RelatedEntityId })
                .HasDatabaseName("IX_Activity_RelatedEntity");

            builder.Entity<Activity>()
                .HasIndex(a => a.DueDate)
                .HasDatabaseName("IX_Activity_DueDate");

            builder.Entity<BATarget>()
                .HasIndex(t => new { t.AssignedTo, t.TargetPeriod })
                .HasDatabaseName("IX_BATarget_AssignedTo_Period");
        }
    }
}