using License_Tracking.Models;

namespace License_Tracking.ViewModels
{
    public class CustomerProfileViewModel
    {
        public Company Company { get; set; } = null!;
        public List<ContactPerson> ContactPersons { get; set; } = new List<ContactPerson>();
        public List<Deal> ActiveDeals { get; set; } = new List<Deal>();
        public List<Deal> CompletedDeals { get; set; } = new List<Deal>();
        public List<CustomerOemProduct> OemRelationships { get; set; } = new List<CustomerOemProduct>();
        public List<Activity> RecentActivities { get; set; } = new List<Activity>();

        // Quick Stats
        public int TotalContacts => ContactPersons?.Count ?? 0;
        public int TotalDeals => (ActiveDeals?.Count ?? 0) + (CompletedDeals?.Count ?? 0);
        public int TotalOemProducts => OemRelationships?.Count ?? 0;
        public decimal TotalRevenue => (ActiveDeals?.Where(d => d.CustomerInvoiceAmount.HasValue).Sum(d => d.CustomerInvoiceAmount.Value) ?? 0) +
                                      (CompletedDeals?.Where(d => d.CustomerInvoiceAmount.HasValue).Sum(d => d.CustomerInvoiceAmount.Value) ?? 0);

        // Contact Analysis
        public ContactPerson? PrimaryContact => ContactPersons?.FirstOrDefault(c => c.IsPrimaryContact);
        public List<ContactPerson> DecisionMakers => ContactPersons?.Where(c => c.DecisionMakerLevel == "Primary").ToList() ?? new List<ContactPerson>();
        public List<ContactPerson> Influencers => ContactPersons?.Where(c => c.DecisionMakerLevel == "Influencer").ToList() ?? new List<ContactPerson>();

        // OEM Relationship Analysis
        public List<IGrouping<Oem?, CustomerOemProduct>> OemGroupedRelationships =>
            OemRelationships?.GroupBy(x => x.Oem).ToList() ?? new List<IGrouping<Oem?, CustomerOemProduct>>();

        // Deal Analysis
        public List<Deal> HighValueDeals => ActiveDeals?.Where(d => d.CustomerInvoiceAmount > 50000).ToList() ?? new List<Deal>();
        public string MostRecentDealStage => ActiveDeals?.OrderByDescending(d => d.CreatedDate).FirstOrDefault()?.DealStage ?? "No Active Deals";
    }

    public class ContactManagementViewModel
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public List<ContactPerson> Contacts { get; set; } = new List<ContactPerson>();
        public ContactPerson NewContact { get; set; } = new ContactPerson();

        // Contact Statistics
        public int TotalContacts => Contacts?.Count ?? 0;
        public int PrimaryContacts => Contacts?.Count(c => c.IsPrimaryContact) ?? 0;
        public int DecisionMakers => Contacts?.Count(c => c.DecisionMakerLevel == "Primary") ?? 0;
        public int Influencers => Contacts?.Count(c => c.DecisionMakerLevel == "Influencer") ?? 0;

        // Department Breakdown
        public List<IGrouping<string?, ContactPerson>> ContactsByDepartment =>
            Contacts?.Where(c => !string.IsNullOrEmpty(c.Department))
                    .GroupBy(c => c.Department)
                    .ToList() ?? new List<IGrouping<string?, ContactPerson>>();
    }

    public class OemRelationshipViewModel
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public List<CustomerOemProduct> Relationships { get; set; } = new List<CustomerOemProduct>();
        public List<Oem> AvailableOems { get; set; } = new List<Oem>();
        public List<Product> AvailableProducts { get; set; } = new List<Product>();

        // Relationship Statistics
        public int TotalOemPartnerships => Relationships?.Select(r => r.OemId).Distinct().Count() ?? 0;
        public int TotalProducts => Relationships?.Count ?? 0;
        public List<IGrouping<Oem?, CustomerOemProduct>> RelationshipsByOem =>
            Relationships?.GroupBy(r => r.Oem).ToList() ?? new List<IGrouping<Oem?, CustomerOemProduct>>();

        // Product Type Analysis
        public int SubscriptionProducts => Relationships?.Count(r => r.Product?.LicenseType == "Subscription") ?? 0;
        public int PerpetualProducts => Relationships?.Count(r => r.Product?.LicenseType == "Perpetual") ?? 0;
        public int TrialProducts => Relationships?.Count(r => r.Product?.LicenseType == "Trial") ?? 0;
    }

    public class ActivityTrackingViewModel
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public List<Activity> Activities { get; set; } = new List<Activity>();
        public Activity NewActivity { get; set; } = new Activity();

        // Activity Statistics
        public int TotalActivities => Activities?.Count ?? 0;
        public int ThisMonthActivities => Activities?.Count(a => a.ActivityDate.Month == DateTime.Now.Month &&
                                                                  a.ActivityDate.Year == DateTime.Now.Year) ?? 0;
        public List<IGrouping<string, Activity>> ActivitiesByType =>
            Activities?.GroupBy(a => a.ActivityType).ToList() ?? new List<IGrouping<string, Activity>>();

        // Recent Activity Summary
        public List<Activity> RecentActivities => Activities?.OrderByDescending(a => a.ActivityDate).Take(10).ToList() ?? new List<Activity>();
        public Activity? LastActivity => Activities?.OrderByDescending(a => a.ActivityDate).FirstOrDefault();
    }
}
