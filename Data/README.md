# CBMS CRM Data Management

This folder contains SQL scripts for managing temporary data in the CBMS CRM system. All seed data functionality has been removed from the application code and replaced with direct SQL scripts.

## Files

- **`CBMS_TempData.sql`** - Main SQL script with comprehensive sample data
- **`PopulateTempData.ps1`** - PowerShell script to execute the SQL script
- **`populate-temp-data.bat`** - Batch file for easy execution

## Quick Start

### Option 1: Use the Batch File (Easiest)

1. Double-click `populate-temp-data.bat`
2. Follow the prompts
3. The script will populate the database with sample data

### Option 2: Use PowerShell

1. Open PowerShell in this directory
2. Run: `.\PopulateTempData.ps1`
3. Follow the prompts

### Option 3: Use SQL Server Management Studio (SSMS)

1. Open SSMS and connect to your SQL Server
2. Open `CBMS_TempData.sql`
3. Verify the database name at the top of the script
4. Execute the script

## Sample Data Included

The SQL script includes:

- **5 Companies** - Mix of customers and prospects with realistic Indian IT companies
- **6 Contact Persons** - Various roles and decision-maker levels
- **5 OEMs** - Microsoft, Adobe, VMware, Autodesk, Oracle
- **6 Products** - Popular enterprise software products
- **4 Deals** - Covering all 4 phases of the business workflow:
  - Phase 1: Customer PO received
  - Phase 2: OEM procurement in progress
  - Phase 3: License delivery
  - Phase 4: OEM payment completed
- **3 CBMS Invoices** - Customer invoices and OEM payments
- **5 Activities** - CRM tracking activities
- **5 BA Targets** - Business analytics targets (actuals only, no forecasting)

## 4-Phase Business Workflow

The sample deals demonstrate the complete 4-phase workflow:

1. **Phase 1 - Customer Engagement**: Customer PO → Customer Invoice → Customer Payment
2. **Phase 2 - OEM Procurement**: OEM Quote → Canarys PO → Margin Calculation
3. **Phase 3 - License Delivery**: License Start/End Dates → Delivery Status
4. **Phase 4 - OEM Settlement**: OEM Invoice → OEM Payment → Deal Closure

## Database Schema

The scripts work with the latest CBMS CRM database schema including:

- Companies (enhanced with business fields)
- ContactPersons (replacing old Contact model)
- Deals (4-phase workflow tracking)
- CbmsInvoices (multi-invoice system)
- BATargets (business analytics without forecasting)
- Activities (CRM activity tracking)

## Important Notes

- ⚠️ **The script will clear existing data** in the main CBMS tables
- The script resets identity columns to start fresh
- User roles and authentication data are preserved
- The script uses realistic business data for testing
- All monetary amounts are in appropriate ranges for enterprise deals

## Customization

To modify the sample data:

1. Edit `CBMS_TempData.sql` directly
2. Add/remove INSERT statements as needed
3. Maintain referential integrity between tables
4. Re-run the script to apply changes

## No More Seed Data in Code

- ✅ Removed all `SeedTestData()` methods from controllers
- ✅ Removed `CreateTestPipelineProject()` method
- ✅ Removed old seed data files
- ✅ Application now uses pure SQL for data population
- ✅ Build process no longer includes any seed data functionality
