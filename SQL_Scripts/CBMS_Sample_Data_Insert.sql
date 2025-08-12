-- =============================================
-- CBMS Sample Data Insert Script
-- Created: August 11, 2025
-- Purpose: Insert duplicate/sample data for Pipeline Projects and Current Deals
-- =============================================

-- First, let's insert some sample Companies if they don't exist
INSERT INTO Companies (CompanyName, Industry, CompanySize, AnnualRevenue, Address, Headquarters, ContactNumber, Email, Website, CompanyType, PaymentTerms, PrimaryBusiness, TechnologyStack, CurrentVendors, CreatedDate, CreatedBy, LastModifiedDate, LastModifiedBy, IsActive)
VALUES 
-- Technology Companies
('Infosys Limited', 'Information Technology', '1000+', 16280000000, 'Electronics City, Hosur Road, Bangalore 560100', 'Bangalore, India', '+91-80-2852-0261', 'contact@infosys.com', 'https://www.infosys.com', 'Customer', 'Net 45', 'IT Services and Consulting', 'Java, .NET, Cloud, AI/ML', 'Microsoft, Oracle, SAP', GETDATE(), 'system', GETDATE(), 'system', 1),
('Tata Consultancy Services', 'Information Technology', '1000+', 25700000000, 'Nirmal Building, Nariman Point, Mumbai 400021', 'Mumbai, India', '+91-22-6778-9595', 'corporate@tcs.com', 'https://www.tcs.com', 'Customer', 'Net 30', 'IT Services and Digital Transformation', '.NET, Java, Cloud, DevOps', 'Microsoft, AWS, Google', GETDATE(), 'system', GETDATE(), 'system', 1),
('Wipro Technologies', 'Information Technology', '1000+', 10400000000, 'Doddakannelli, Sarjapur Road, Bangalore 560035', 'Bangalore, India', '+91-80-2844-0011', 'info@wipro.com', 'https://www.wipro.com', 'Customer', 'Net 45', 'IT Services and Business Process Services', 'Java, Python, Cloud, Analytics', 'Microsoft, Oracle, Adobe', GETDATE(), 'system', GETDATE(), 'system', 1),
('Tech Mahindra', 'Information Technology', '1000+', 6000000000, 'Gateway Building, Apollo Bunder, Mumbai 400001', 'Pune, India', '+91-20-3019-6000', 'info@techmahindra.com', 'https://www.techmahindra.com', 'Customer', 'Net 30', 'Digital Transformation and Technology Services', '.NET, Java, Cloud, 5G', 'Microsoft, Cisco, VMware', GETDATE(), 'system', GETDATE(), 'system', 1),
('HCL Technologies', 'Information Technology', '1000+', 12100000000, 'Hamilton Building, DLF Cyber City, Gurgaon 122002', 'Noida, India', '+91-120-254-6000', 'info@hcl.com', 'https://www.hcl.com', 'Customer', 'Net 45', 'IT Services and Product Engineering', 'Java, .NET, Cloud, IoT', 'Microsoft, AWS, Oracle', GETDATE(), 'system', GETDATE(), 'system', 1),

-- Banking & Finance
('HDFC Bank', 'Banking & Finance', '1000+', 2100000000, 'HDFC Bank House, Senapati Bapat Marg, Mumbai 400013', 'Mumbai, India', '+91-22-6160-6161', 'customer.care@hdfcbank.com', 'https://www.hdfcbank.com', 'Customer', 'Net 30', 'Banking and Financial Services', 'Core Banking, Digital Banking', 'Oracle, IBM, Microsoft', GETDATE(), 'system', GETDATE(), 'system', 1),
('ICICI Bank', 'Banking & Finance', '1000+', 1900000000, 'ICICI Bank Towers, Bandra-Kurla Complex, Mumbai 400051', 'Mumbai, India', '+91-22-2653-1414', 'customercare@icicibank.com', 'https://www.icicibank.com', 'Customer', 'Net 45', 'Banking and Financial Services', 'Digital Banking, Mobile Banking', 'Microsoft, Oracle, SAP', GETDATE(), 'system', GETDATE(), 'system', 1),

-- Manufacturing
('Tata Motors', 'Automotive', '1000+', 3100000000, 'Bombay House, 24 Homi Mody Street, Mumbai 400001', 'Mumbai, India', '+91-22-6665-8282', 'investors@tatamotors.com', 'https://www.tatamotors.com', 'Customer', 'Net 60', 'Automotive Manufacturing', 'PLM, ERP, IoT, Analytics', 'Siemens, SAP, Microsoft', GETDATE(), 'system', GETDATE(), 'system', 1),
('Mahindra Group', 'Automotive', '1000+', 2000000000, 'Mahindra Towers, Dr. G.M. Bhosale Marg, Mumbai 400018', 'Mumbai, India', '+91-22-2490-1441', 'group.communications@mahindra.com', 'https://www.mahindra.com', 'Customer', 'Net 45', 'Automotive and Farm Equipment', 'ERP, PLM, CRM, Analytics', 'SAP, Microsoft, Oracle', GETDATE(), 'system', GETDATE(), 'system', 1),

-- Prospects
('Reliance Industries', 'Oil & Gas', '1000+', 8700000000, 'Maker Chambers IV, 3rd Floor, Nariman Point, Mumbai 400021', 'Mumbai, India', '+91-22-3555-5000', 'investor.relations@ril.com', 'https://www.ril.com', 'Prospect', 'Net 30', 'Petrochemicals, Oil & Gas, Retail', 'ERP, Analytics, IoT', 'SAP, Oracle, IBM', GETDATE(), 'system', GETDATE(), 'system', 1);

-- Insert OEMs if they don't exist
INSERT INTO Oems (OemName, ContactEmail, ContactNumber, PaymentTerms, ServiceLevel, PerformanceRating, Address, Website, CreatedDate, CreatedBy, IsActive)
VALUES 
('Microsoft Corporation', 'partner@microsoft.com', '+1-425-882-8080', 'Net 30', 'Gold', 4.8, 'One Microsoft Way, Redmond, WA 98052, USA', 'https://www.microsoft.com', GETDATE(), 'system', 1),
('Adobe Inc.', 'partners@adobe.com', '+1-408-536-6000', 'Net 45', 'Gold', 4.7, '345 Park Avenue, San Jose, CA 95110, USA', 'https://www.adobe.com', GETDATE(), 'system', 1),
('Oracle Corporation', 'partners@oracle.com', '+1-650-506-7000', 'Net 60', 'Silver', 4.5, '2300 Oracle Way, Austin, TX 78741, USA', 'https://www.oracle.com', GETDATE(), 'system', 1),
('VMware Inc.', 'partners@vmware.com', '+1-650-427-5000', 'Net 30', 'Gold', 4.6, '3401 Hillview Ave, Palo Alto, CA 94304, USA', 'https://www.vmware.com', GETDATE(), 'system', 1),
('Salesforce Inc.', 'partners@salesforce.com', '+1-415-901-7000', 'Net 45', 'Gold', 4.9, 'Salesforce Tower, 415 Mission St, San Francisco, CA 94105, USA', 'https://www.salesforce.com', GETDATE(), 'system', 1),
('Atlassian Corporation', 'partners@atlassian.com', '+61-2-9262-2444', 'Net 30', 'Silver', 4.4, 'Level 6, 341 George St, Sydney NSW 2000, Australia', 'https://www.atlassian.com', GETDATE(), 'system', 1),
('GitHub Inc.', 'enterprise@github.com', '+1-877-448-4820', 'Net 30', 'Gold', 4.7, '88 Colin P Kelly Jr St, San Francisco, CA 94107, USA', 'https://github.com', GETDATE(), 'system', 1),
('Slack Technologies', 'enterprise@slack.com', '+1-855-685-7525', 'Net 45', 'Silver', 4.3, '500 Howard St, San Francisco, CA 94105, USA', 'https://slack.com', GETDATE(), 'system', 1);

-- Insert Products if they don't exist
INSERT INTO Products (OemId, ProductName, ProductCategory, ProductDescription, UnitPrice, LicenseType, MinimumQuantity, CreatedDate, CreatedBy, IsActive)
VALUES 
-- Microsoft Products
(1, 'Microsoft 365 Business Premium', 'Productivity Suite', 'Complete productivity and collaboration suite with advanced security', 22.00, 'Subscription', 1, GETDATE(), 'system', 1),
(1, 'Microsoft Office 365 E3', 'Productivity Suite', 'Enterprise-grade productivity suite with advanced compliance and analytics', 32.00, 'Subscription', 1, GETDATE(), 'system', 1),
(1, 'Azure AD Premium P1', 'Identity Management', 'Cloud-based identity and access management service', 6.00, 'Subscription', 1, GETDATE(), 'system', 1),
(1, 'Microsoft Teams Phone', 'Communication', 'Cloud-based phone system integrated with Microsoft Teams', 8.00, 'Subscription', 1, GETDATE(), 'system', 1),
(1, 'Power BI Pro', 'Business Intelligence', 'Self-service business intelligence and analytics platform', 10.00, 'Subscription', 1, GETDATE(), 'system', 1),

-- Adobe Products
(2, 'Adobe Creative Cloud for Enterprise', 'Creative Software', 'Complete creative suite for design, video, and web development', 79.99, 'Subscription', 1, GETDATE(), 'system', 1),
(2, 'Adobe Acrobat Pro DC', 'Document Management', 'Professional PDF creation, editing, and collaboration tool', 14.99, 'Subscription', 1, GETDATE(), 'system', 1),
(2, 'Adobe Analytics', 'Web Analytics', 'Advanced web analytics and customer intelligence platform', 999.00, 'Subscription', 1, GETDATE(), 'system', 1),

-- Oracle Products
(3, 'Oracle Database Enterprise Edition', 'Database', 'High-performance, scalable, and secure database platform', 47500.00, 'Perpetual', 1, GETDATE(), 'system', 1),
(3, 'Oracle ERP Cloud', 'Enterprise Resource Planning', 'Complete cloud-based ERP solution for modern businesses', 150.00, 'Subscription', 1, GETDATE(), 'system', 1),

-- VMware Products
(4, 'VMware vSphere Enterprise Plus', 'Virtualization', 'Advanced server virtualization platform with enterprise features', 5899.00, 'Perpetual', 1, GETDATE(), 'system', 1),
(4, 'VMware Workspace ONE', 'Endpoint Management', 'Digital workspace platform for managing any device, any app, any cloud', 8.33, 'Subscription', 1, GETDATE(), 'system', 1),

-- Salesforce Products
(5, 'Salesforce Enterprise Edition', 'CRM', 'Advanced CRM platform with customization and integration capabilities', 165.00, 'Subscription', 1, GETDATE(), 'system', 1),
(5, 'Salesforce Marketing Cloud', 'Marketing Automation', 'Comprehensive digital marketing platform', 1250.00, 'Subscription', 1, GETDATE(), 'system', 1),

-- Atlassian Products
(6, 'Jira Software Server', 'Project Management', 'Issue tracking and project management for agile teams', 42000.00, 'Perpetual', 1, GETDATE(), 'system', 1),
(6, 'Confluence Server', 'Collaboration', 'Team collaboration and knowledge management platform', 27000.00, 'Perpetual', 1, GETDATE(), 'system', 1),

-- GitHub Products
(7, 'GitHub Enterprise Server', 'Version Control', 'Self-hosted Git repository management with enterprise features', 21.00, 'Subscription', 1, GETDATE(), 'system', 1),
(7, 'GitHub Copilot Business', 'AI Coding Assistant', 'AI-powered code completion and programming assistant', 19.00, 'Subscription', 1, GETDATE(), 'system', 1),

-- Slack Products
(8, 'Slack Enterprise Grid', 'Communication', 'Enterprise-grade team communication and collaboration platform', 15.00, 'Subscription', 1, GETDATE(), 'system', 1);

-- =============================================
-- PIPELINE PROJECTS (Future Deals)
-- =============================================

INSERT INTO ProjectPipelines (
    ProductName, OemName, ClientName, ClientContactEmail, ClientContactPhone,
    ExpectedLicenseDate, ExpectedExpiryDate, 
    CustomerPoNumber, CustomerPoItemDescription, ExpectedCustomerPoAmount,
    OemPoNumber, OemPoItemDescription, ExpectedOemPoAmount,
    ExpectedAmountToReceive, ExpectedAmountToPay,
    ProjectedMarginInput, MarginNotes, AlertDaysBefore, AlertsEnabled,
    ProjectStatus, SuccessProbability, ShipToAddress, BillToAddress, Remarks,
    -- Week 10 Enhanced Fields
    EstimatedRevenue, EstimatedCost, PipelineStage, StageConfidenceLevel, ExpectedCloseDate,
    -- Additional Fields
    ExpectedInvoiceNumber, PaymentStatus, AmountReceived, AmountPaid,
    OemType, CustomerType, CustomerIndustry, CustomerEmployeeCount, CustomerWebsite,
    OemRelationshipType, LastCustomerContact, CustomerNotes,
    CreatedBy, CreatedDate, LastUpdated
) VALUES

-- High-Value Pipeline Projects
('Microsoft 365 Enterprise', 'Microsoft Corporation', 'Reliance Industries', 'it.procurement@ril.com', '+91-22-3555-5100', 
'2025-10-15', '2026-10-14',
'REL/PO/2025/1001', 'Microsoft 365 E3 licenses for 5000 users', 1920000.00,
'MS/PO/2025/REL001', 'M365 E3 Enterprise Licensing', 1600000.00,
1920000.00, 1600000.00, 320000.00, 'Excellent margin opportunity with large enterprise client', 30, 1,
'Pipeline', 75, 'Reliance Corporate Park, Thane', 'Finance Dept, Maker Chambers IV, Mumbai', 'Strategic enterprise deal - high priority',
-- Week 10 Enhanced Fields
1920000.00, 1600000.00, 'Proposal', 4, '2025-09-30',
-- Additional Fields  
'', 'Pending', 0, 0,
'Strategic Partner', 'Enterprise', 'Oil & Gas', 50000, 'https://www.ril.com',
'Gold Partner', '2025-08-05', 'CEO level engagement, very positive response',
'sandeep.kumar', GETDATE(), GETDATE()),

('Adobe Creative Cloud Enterprise', 'Adobe Inc.', 'Tata Motors', 'design.team@tatamotors.com', '+91-22-6665-8300',
'2025-11-01', '2026-10-31',
'TATA/CC/2025/0045', 'Adobe Creative Suite for Marketing and Design Teams', 960000.00,
'ADOBE/ENT/2025/TATA', 'Creative Cloud Enterprise Licensing', 800000.00,
960000.00, 800000.00, 160000.00, 'Design team expansion project', 45, 1,
'Pipeline', 60, 'Tata Motors Pune Plant', 'Tata Motors Head Office, Mumbai', 'Design team modernization initiative',
-- Week 10 Enhanced Fields
960000.00, 800000.00, 'Qualified', 3, '2025-10-15',
-- Additional Fields
'', 'Pending', 0, 0,
'Preferred Vendor', 'Enterprise', 'Automotive', 80000, 'https://www.tatamotors.com',
'Silver Partner', '2025-07-20', 'Design head very interested, budget approved in principle',
'priya.sharma', GETDATE(), GETDATE()),

('Salesforce Enterprise CRM', 'Salesforce Inc.', 'HDFC Bank', 'crm.implementation@hdfcbank.com', '+91-22-6160-6200',
'2025-12-01', '2027-11-30',
'HDFC/CRM/2025/SF001', 'Salesforce CRM for Retail Banking Division', 2400000.00,
'SF/BANK/2025/HDFC', 'Salesforce Enterprise + Marketing Cloud', 2000000.00,
2400000.00, 2000000.00, 400000.00, 'Banking sector CRM transformation', 60, 1,
'Pipeline', 85, 'HDFC Towers, BKC Mumbai', 'HDFC Bank House, Mumbai', 'Digital transformation priority project',
-- Week 10 Enhanced Fields
2400000.00, 2000000.00, 'Negotiation', 5, '2025-11-15',
-- Additional Fields
'', 'Pending', 0, 0,
'Strategic Partner', 'Enterprise', 'Banking & Finance', 120000, 'https://www.hdfcbank.com',
'Gold Partner', '2025-08-10', 'CTO committed, final pricing discussions ongoing',
'rajesh.gupta', GETDATE(), GETDATE()),

-- Medium-Value Pipeline Projects
('VMware vSphere Enterprise', 'VMware Inc.', 'Tech Mahindra', 'infrastructure@techmahindra.com', '+91-20-3019-6100',
'2025-09-30', '2028-09-29',
'TM/INFRA/2025/VM001', 'VMware virtualization infrastructure upgrade', 1180000.00,
'VM/ENT/2025/TECHMAHINDRA', 'vSphere Enterprise Plus Licensing', 1000000.00,
1180000.00, 1000000.00, 180000.00, 'Infrastructure modernization project', 30, 1,
'Pipeline', 70, 'Tech Mahindra Pune Campus', 'Tech Mahindra Mumbai Office', 'Datacenter consolidation initiative',
-- Week 10 Enhanced Fields
1180000.00, 1000000.00, 'Proposal', 4, '2025-09-15',
-- Additional Fields
'', 'Pending', 0, 0,
'Certified Partner', 'Enterprise', 'Information Technology', 145000, 'https://www.techmahindra.com',
'Gold Partner', '2025-07-30', 'Infrastructure head very positive, technical evaluation complete',
'amit.patel', GETDATE(), GETDATE()),

('GitHub Enterprise Server', 'GitHub Inc.', 'Wipro Technologies', 'devops@wipro.com', '+91-80-2844-0100',
'2025-10-01', '2026-09-30',
'WIPRO/DEV/2025/GH001', 'GitHub Enterprise for Development Teams', 570000.00,
'GH/ENT/2025/WIPRO', 'GitHub Enterprise Server Licensing', 480000.00,
570000.00, 480000.00, 90000.00, 'DevOps transformation initiative', 45, 1,
'Pipeline', 65, 'Wipro Bangalore Campus', 'Wipro Head Office, Bangalore', 'Part of digital transformation roadmap',
-- Week 10 Enhanced Fields
570000.00, 480000.00, 'Qualified', 3, '2025-09-20',
-- Additional Fields
'', 'Pending', 0, 0,
'Technology Partner', 'Enterprise', 'Information Technology', 230000, 'https://www.wipro.com',
'Silver Partner', '2025-08-01', 'Development head engaged, POC successful',
'neha.singh', GETDATE(), GETDATE()),

('Oracle Database Enterprise', 'Oracle Corporation', 'ICICI Bank', 'database.admin@icicibank.com', '+91-22-2653-1500',
'2025-11-15', '2028-11-14',
'ICICI/DB/2025/ORA001', 'Oracle Database upgrade for core banking', 2850000.00,
'ORA/BANK/2025/ICICI', 'Oracle Database Enterprise Edition', 2375000.00,
2850000.00, 2375000.00, 475000.00, 'Critical database infrastructure upgrade', 60, 1,
'Pipeline', 80, 'ICICI Towers BKC', 'ICICI Bank Head Office, Mumbai', 'Core banking system enhancement',
-- Week 10 Enhanced Fields
2850000.00, 2375000.00, 'Negotiation', 4, '2025-10-30',
-- Additional Fields
'', 'Pending', 0, 0,
'Strategic Partner', 'Enterprise', 'Banking & Finance', 100000, 'https://www.icicibank.com',
'Gold Partner', '2025-08-08', 'Database team committed, compliance review in progress',
'vikram.mehta', GETDATE(), GETDATE()),

-- Early Stage Pipeline Projects
('Atlassian Jira Enterprise', 'Atlassian Corporation', 'HCL Technologies', 'project.management@hcl.com', '+91-120-254-6100',
'2025-12-15', '2027-12-14',
'HCL/PM/2025/ATL001', 'Jira and Confluence for Project Management', 810000.00,
'ATL/ENT/2025/HCL', 'Atlassian Enterprise Bundle', 675000.00,
810000.00, 675000.00, 135000.00, 'Project management tool standardization', 45, 1,
'Pipeline', 55, 'HCL Noida Campus', 'HCL Head Office, Noida', 'Agile transformation support tool',
-- Week 10 Enhanced Fields
810000.00, 675000.00, 'Lead', 2, '2025-11-30',
-- Additional Fields
'', 'Pending', 0, 0,
'Technology Partner', 'Enterprise', 'Information Technology', 210000, 'https://www.hcl.com',
'Silver Partner', '2025-07-25', 'Initial discussions, requirements gathering phase',
'anita.krishnan', GETDATE(), GETDATE()),

('Slack Enterprise Grid', 'Slack Technologies', 'Mahindra Group', 'communication@mahindra.com', '+91-22-2490-1500',
'2025-10-30', '2026-10-29',
'MAH/COMM/2025/SL001', 'Slack Enterprise for Team Communication', 540000.00,
'SL/ENT/2025/MAHINDRA', 'Slack Enterprise Grid Licensing', 450000.00,
540000.00, 450000.00, 90000.00, 'Modern communication platform deployment', 30, 1,
'Pipeline', 50, 'Mahindra Towers Mumbai', 'Mahindra Group Mumbai Office', 'Employee communication modernization',
-- Week 10 Enhanced Fields
540000.00, 450000.00, 'Lead', 2, '2025-10-10',
-- Additional Fields
'', 'Pending', 0, 0,
'New Partner', 'Enterprise', 'Automotive', 260000, 'https://www.mahindra.com',
'Bronze Partner', '2025-08-02', 'HR and IT teams evaluating, early stage discussions',
'ravi.kumar', GETDATE(), GETDATE()),

('Power BI Premium', 'Microsoft Corporation', 'Infosys Limited', 'analytics@infosys.com', '+91-80-2852-0300',
'2025-09-15', '2026-09-14',
'INFY/BI/2025/MS001', 'Power BI Premium for Business Analytics', 360000.00,
'MS/BI/2025/INFOSYS', 'Power BI Premium Licensing', 300000.00,
360000.00, 300000.00, 60000.00, 'Business intelligence enhancement project', 30, 1,
'Pipeline', 70, 'Infosys Bangalore Campus', 'Infosys Head Office, Bangalore', 'Data analytics capability expansion',
-- Week 10 Enhanced Fields
360000.00, 300000.00, 'Qualified', 3, '2025-09-01',
-- Additional Fields
'', 'Pending', 0, 0,
'Strategic Partner', 'Enterprise', 'Information Technology', 280000, 'https://www.infosys.com',
'Gold Partner', '2025-07-28', 'Analytics team engaged, pilot project planned',
'sarah.thomas', GETDATE(), GETDATE()),

('Adobe Analytics Premium', 'Adobe Inc.', 'Tata Consultancy Services', 'digital.analytics@tcs.com', '+91-22-6778-9600',
'2025-11-30', '2026-11-29',
'TCS/ANALYTICS/2025/AD001', 'Adobe Analytics for Digital Marketing', 1200000.00,
'ADOBE/ANALYTICS/2025/TCS', 'Adobe Analytics Premium Package', 1000000.00,
1200000.00, 1000000.00, 200000.00, 'Digital marketing analytics platform', 45, 1,
'Pipeline', 60, 'TCS Mumbai Office', 'TCS Head Office, Mumbai', 'Digital transformation analytics initiative',
-- Week 10 Enhanced Fields
1200000.00, 1000000.00, 'Proposal', 3, '2025-11-10',
-- Additional Fields
'', 'Pending', 0, 0,
'Preferred Vendor', 'Enterprise', 'Information Technology', 500000, 'https://www.tcs.com',
'Gold Partner', '2025-08-03', 'Marketing head interested, budget discussion ongoing',
'monica.desai', GETDATE(), GETDATE());

-- =============================================
-- CURRENT DEALS (Active Projects)
-- =============================================

INSERT INTO Deals (
    CompanyId, OemId, ProductId, ContactId,
    DealName, DealStage, CurrentPhase, DealType, Quantity,
    -- Phase 1 - Customer Engagement
    CustomerPoNumber, CustomerPoDate, CustomerInvoiceNumber, CustomerInvoiceAmount, CustomerPaymentStatus, CustomerPaymentDate,
    -- Phase 2 - OEM Procurement  
    OemQuoteAmount, CanarysPoNumber, CanarysPoDate, EstimatedMargin,
    -- Phase 3 - License Delivery
    LicenseStartDate, LicenseEndDate, LicenseDeliveryStatus,
    -- Phase 4 - OEM Settlement
    OemInvoiceNumber, OemInvoiceAmount, OemPaymentStatus, OemPaymentDate,
    -- Deal Management
    Priority, AssignedTo, DealProbability, ExpectedCloseDate, ActualCloseDate, Notes, AlertsEnabled, IsProjectPipeline, IsActive,
    CreatedDate, CreatedBy, LastModifiedDate, LastModifiedBy
) VALUES

-- Won Deals (Phase 3-4)
(1, 1, 2, NULL, 'Infosys Office 365 Deployment', 'Won', 'Phase 3', 'New', 2500,
-- Phase 1
'INFY/2025/O365/001', '2025-07-15', 'CBMS-CUST-202507-0001', 2000000.00, 'Completed', '2025-08-10',
-- Phase 2
1666667.00, 'CBMS-OEM-2025/MS001', '2025-07-20', 333333.00,
-- Phase 3
'2025-08-15', '2026-08-14', 'Delivered',
-- Phase 4
'MS-INV-2025-INFY-001', 1666667.00, 'Paid', '2025-08-20',
-- Deal Management
'High', 'Rajesh Gupta', 0.95, '2025-08-15', '2025-08-15', 'Large enterprise deployment for Infosys', 1, 0, 1,
'2025-07-10', 'rajesh.gupta', '2025-08-20', 'system'),

(3, 2, 6, NULL, 'Wipro Creative Cloud License', 'Won', 'Phase 4', 'New', 150,
-- Phase 1
'WIPRO/CC/2025/001', '2025-06-20', 'CBMS-CUST-202506-0002', 359970.00, 'Completed', '2025-07-25',
-- Phase 2
299975.00, 'CBMS-OEM-2025/AD001', '2025-06-25', 59995.00,
-- Phase 3
'2025-07-30', '2026-07-29', 'Delivered',
-- Phase 4
'AD-INV-2025-WIPRO-001', 299975.00, 'Pending', NULL,
-- Deal Management
'Medium', 'Neha Singh', 0.85, '2025-07-30', '2025-07-30', 'Creative Cloud for design team', 1, 0, 1,
'2025-06-15', 'neha.singh', '2025-07-30', 'system'),

-- Deals in Phase 2-3
(2, 1, 1, NULL, 'TCS Microsoft 365 Enterprise', 'Won', 'Phase 2', 'Upgrade', 3500,
-- Phase 1
'TCS/M365/2025/002', '2025-08-01', 'CBMS-CUST-202508-0003', 2310000.00, 'Completed', '2025-08-25',
-- Phase 2
1925000.00, 'CBMS-OEM-2025/MS002', '2025-08-05', 385000.00,
-- Phase 3
'2025-09-01', '2026-08-31', 'Pending',
-- Phase 4
NULL, NULL, 'Pending', NULL,
-- Deal Management
'High', 'Monica Desai', 0.90, '2025-09-01', '2025-09-01', 'Microsoft 365 Enterprise upgrade for TCS', 1, 0, 1,
'2025-07-25', 'monica.desai', '2025-08-25', 'system'),

(6, 5, 13, NULL, 'HDFC Salesforce CRM Implementation', 'Won', 'Phase 2', 'New', 1000,
-- Phase 1
'HDFC/SF/2025/001', '2025-08-05', 'CBMS-CUST-202508-0004', 4950000.00, 'Completed', '2025-08-30',
-- Phase 2
4125000.00, 'CBMS-OEM-2025/SF001', '2025-08-10', 825000.00,
-- Phase 3
'2025-09-15', '2027-09-14', 'Pending',
-- Phase 4
NULL, NULL, 'Pending', NULL,
-- Deal Management
'High', 'Vikram Mehta', 0.95, '2025-09-15', '2025-09-15', 'Salesforce CRM implementation for HDFC Bank', 1, 0, 1,
'2025-07-30', 'vikram.mehta', '2025-08-30', 'system'),

-- Negotiation Phase Deals
(4, 4, 11, NULL, 'Tech Mahindra VMware Infrastructure', 'Negotiation', 'Phase 1', 'New', 200,
-- Phase 1
'TM/VM/2025/001', '2025-08-20', NULL, 3539800.00, 'Pending', NULL,
-- Phase 2
2949833.00, NULL, NULL, 589967.00,
-- Phase 3
'2025-10-01', '2028-09-30', 'Pending',
-- Phase 4
NULL, NULL, 'Pending', NULL,
-- Deal Management
'High', 'Amit Patel', 0.75, '2025-09-30', NULL, 'VMware infrastructure modernization for Tech Mahindra', 1, 0, 1,
'2025-08-15', 'amit.patel', '2025-08-20', 'system'),

(5, 7, 17, NULL, 'HCL GitHub Enterprise Deployment', 'Negotiation', 'Phase 1', 'New', 500,
-- Phase 1
'HCL/GH/2025/001', '2025-08-25', NULL, 315000.00, 'Pending', NULL,
-- Phase 2
262500.00, NULL, NULL, 52500.00,
-- Phase 3
'2025-10-15', '2026-10-14', 'Pending',
-- Phase 4
NULL, NULL, 'Pending', NULL,
-- Deal Management
'Medium', 'Anita Krishnan', 0.65, '2025-10-15', NULL, 'GitHub Enterprise deployment for HCL', 1, 0, 1,
'2025-08-20', 'anita.krishnan', '2025-08-25', 'system'),

-- Early Stage Deals
(7, 6, 15, NULL, 'ICICI Atlassian Project Management', 'Qualified', 'Pre-Phase', 'New', 300,
-- Phase 1
NULL, NULL, NULL, 378000.00, 'Pending', NULL,
-- Phase 2
315000.00, NULL, NULL, 63000.00,
-- Phase 3
'2025-11-01', '2027-10-31', 'Pending',
-- Phase 4
NULL, NULL, 'Pending', NULL,
-- Deal Management
'Medium', 'Sarah Thomas', 0.60, '2025-10-30', NULL, 'Atlassian project management tools for ICICI', 1, 0, 1,
'2025-08-10', 'sarah.thomas', '2025-08-10', 'system'),

(8, 1, 5, NULL, 'Tata Motors Power BI Analytics', 'Lead', 'Pre-Phase', 'New', 100,
-- Phase 1
NULL, NULL, NULL, 120000.00, 'Pending', NULL,
-- Phase 2
100000.00, NULL, NULL, 20000.00,
-- Phase 3
'2025-10-01', '2026-09-30', 'Pending',
-- Phase 4
NULL, NULL, 'Pending', NULL,
-- Deal Management
'Low', 'Ravi Kumar', 0.40, '2025-09-30', NULL, 'Power BI analytics solution for Tata Motors', 1, 0, 1,
'2025-08-05', 'ravi.kumar', '2025-08-05', 'system'),

(9, 8, 19, NULL, 'Mahindra Slack Communication Platform', 'Lead', 'Pre-Phase', 'New', 1500,
-- Phase 1
NULL, NULL, NULL, 675000.00, 'Pending', NULL,
-- Phase 2
562500.00, NULL, NULL, 112500.00,
-- Phase 3
'2025-11-15', '2026-11-14', 'Pending',
-- Phase 4
NULL, NULL, 'Pending', NULL,
-- Deal Management
'Medium', 'Monica Desai', 0.50, '2025-11-01', NULL, 'Slack communication platform for Mahindra Group', 1, 0, 1,
'2025-08-01', 'monica.desai', '2025-08-01', 'system'),

(10, 3, 10, NULL, 'Reliance Oracle ERP Implementation', 'Qualified', 'Pre-Phase', 'New', 2000,
-- Phase 1
NULL, NULL, NULL, 9000000.00, 'Pending', NULL,
-- Phase 2
7500000.00, NULL, NULL, 1500000.00,
-- Phase 3
'2025-12-01', '2027-11-30', 'Pending',
-- Phase 4
NULL, NULL, 'Pending', NULL,
-- Deal Management
'High', 'Rajesh Gupta', 0.70, '2025-11-30', NULL, 'Oracle ERP implementation for Reliance Industries', 1, 0, 1,
'2025-08-08', 'rajesh.gupta', '2025-08-08', 'system');

-- =============================================
-- CBMS INVOICES (Phase-Mapped Invoices)
-- =============================================

-- Insert CBMS Invoices for Phase 1 (Customer→Canarys) and Phase 4 (OEM→Canarys)
INSERT INTO CbmsInvoices (
    DealId, InvoiceType, InvoiceNumber, InvoiceDate, DueDate, Amount, TaxAmount, TotalAmount,
    PaymentStatus, PaymentDate, BusinessPhase, CreatedDate, CreatedBy
) VALUES

-- Phase 1 Invoices (Customer→Canarys)
(1, 'Customer_To_Canarys', 'CBMS-CUST-202507-0001', '2025-07-16', '2025-08-15', 2000000.00, 360000.00, 2360000.00, 'Completed', '2025-08-10', 1, '2025-07-16', 'system'),
(2, 'Customer_To_Canarys', 'CBMS-CUST-202506-0002', '2025-06-21', '2025-07-21', 359970.00, 64794.60, 424764.60, 'Completed', '2025-07-25', 1, '2025-06-21', 'system'),
(3, 'Customer_To_Canarys', 'CBMS-CUST-202508-0003', '2025-08-02', '2025-09-01', 2310000.00, 415800.00, 2725800.00, 'Completed', '2025-08-25', 1, '2025-08-02', 'system'),
(4, 'Customer_To_Canarys', 'CBMS-CUST-202508-0004', '2025-08-06', '2025-09-05', 4950000.00, 891000.00, 5841000.00, 'Completed', '2025-08-30', 1, '2025-08-06', 'system'),

-- Phase 4 Invoices (OEM→Canarys) 
(1, 'OEM_To_Canarys', 'CBMS-OEM-202508-0001', '2025-08-15', '2025-09-14', 1666667.00, 300000.06, 1966667.06, 'Completed', '2025-08-20', 4, '2025-08-15', 'system'),
(2, 'OEM_To_Canarys', 'CBMS-OEM-202508-0003', '2025-07-30', '2025-08-29', 299975.00, 53995.50, 353970.50, 'Pending', NULL, 4, '2025-07-30', 'system');

-- =============================================
-- REGULAR INVOICES (Invoice Management System)
-- =============================================

-- Insert regular invoices for the Invoice Management system
INSERT INTO Invoices (
    LicenseId, InvoiceNumber, InvoiceType, Amount, DueDate, InvoiceDate, 
    PaymentDate, PaymentStatus, AmountReceived, Notes, CreatedDate
) VALUES

-- Customer Invoices (Phase 1)
(1, 'CBMS-CUST-202507-0001', 0, 2000000.00, '2025-08-15', '2025-07-16', '2025-08-10', 'Completed', 2000000.00, 'Infosys Office 365 - Large enterprise deployment', '2025-07-16'),
(2, 'CBMS-CUST-202506-0002', 0, 359970.00, '2025-07-21', '2025-06-21', '2025-07-25', 'Completed', 359970.00, 'Wipro Creative Cloud - Design team licenses', '2025-06-21'),
(3, 'CBMS-CUST-202508-0003', 0, 2310000.00, '2025-09-01', '2025-08-02', '2025-08-25', 'Completed', 2310000.00, 'TCS Microsoft 365 Enterprise - Upgrade project', '2025-08-02'),
(4, 'CBMS-CUST-202508-0004', 0, 4950000.00, '2025-09-05', '2025-08-06', '2025-08-30', 'Completed', 4950000.00, 'HDFC Salesforce CRM - Banking implementation', '2025-08-06'),
(5, 'CBMS-CUST-202508-0005', 0, 3539800.00, '2025-09-20', '2025-08-21', NULL, 'Pending', 0.00, 'Tech Mahindra VMware - Infrastructure modernization', '2025-08-21'),
(6, 'CBMS-CUST-202508-0006', 0, 315000.00, '2025-09-25', '2025-08-26', NULL, 'Pending', 0.00, 'HCL GitHub Enterprise - Development platform', '2025-08-26'),

-- OEM Invoices (Phase 4)
(1, 'CBMS-OEM-202508-0001', 1, 1666667.00, '2025-09-14', '2025-08-15', '2025-08-20', 'Completed', 1666667.00, 'Microsoft payment for Infosys O365 licenses', '2025-08-15'),
(2, 'CBMS-OEM-202507-0002', 1, 299975.00, '2025-08-29', '2025-07-30', NULL, 'Pending', 0.00, 'Adobe payment for Wipro Creative Cloud licenses', '2025-07-30'),
(3, 'CBMS-OEM-202508-0003', 1, 1925000.00, '2025-09-10', '2025-08-06', NULL, 'Pending', 0.00, 'Microsoft payment for TCS M365 Enterprise', '2025-08-06'),
(4, 'CBMS-OEM-202508-0004', 1, 4125000.00, '2025-10-05', '2025-09-16', NULL, 'Pending', 0.00, 'Salesforce payment for HDFC CRM implementation', '2025-09-16');

PRINT 'Sample data insertion completed successfully!';
PRINT 'Pipeline Projects: 10 records inserted';
PRINT 'Current Deals: 10 records inserted';  
PRINT 'CBMS Invoices: 6 records inserted';
PRINT 'Regular Invoices: 10 records inserted';
PRINT 'Companies: 10 records inserted';
PRINT 'OEMs: 8 records inserted';
PRINT 'Products: 17 records inserted';
