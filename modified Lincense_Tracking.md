# Canarys Business Management System (CBMS) - Implementation Plan

_Bigin.com-Inspired B2B2B CRM for Customer-OEM License Management_

## Overview

Transform the License Tracking system into a comprehensive B2B2B CRM where Canarys acts as a mediator between customers and OEMs, following a structured 4-phase business workflow with modern Bigin.com-inspired interface and user experience.

## Business Workflow (4 Phases)

**Phase 1**: Customer issues PO to Canarys → Canarys releases Invoice to Customer → Customer Payment (with flexible payment terms)

**Phase 2**: Canarys estimates quote from OEM → Canarys sends PO to OEM

**Phase 3**: OEM receives PO from Canarys → OEM issues License to Customer (with start/end dates)

**Phase 4**: OEM releases Invoice to Canarys → Canarys pays OEM (applies to subscriptions and renewals)

## Core Requirements (Bigin.com UX Approach)

1. **Add Customer/Company**: Complete company profiles with contact persons and business details
2. **OEM & Product Reference Data**: Managed as dropdown selections for customer assignments
3. **Deal/Project Management**: 4-Phase workflow tracking from Customer PO to OEM Payment
4. **Multi-Invoice System**: Phase-specific invoice tracking with proper business phase mapping
5. **Pipeline Management**: Future projects with estimated revenue/margin only (no predictions)
6. **BA Target Analytics**: Achievement tracking with actuals vs targets only
7. **Bigin.com UI/UX**: Modern, desktop-first responsive interface with List/Sheet/Kanban views optimized for desktop workflow

## Key Features & Business Requirements

### **1. Add Customer/Company Management (Bigin.com Style)**

**Company Information Fields:**

- **Company Name**: Primary company identifier
- **Industry**: Business sector classification
- **Address**: Complete business address
- **Headquarters**: Main office location
- **Employee Size**: Staff count ranges (1-50, 51-200, 201-1000, 1000+)
- **Annual Revenue**: Company financial scale
- **Contact Number**: Primary business phone
- **Email**: Main company email
- **Website**: Company website URL
- **Company Type**: Prospect/Customer/Partner classification

**Contact Person Fields:**

- **Name**: Full contact name (First Name + Last Name)
- **Email**: Personal/business email
- **Number**: Direct contact phone
- **Designation**: Job title/position
- **Department**: Organizational department
- **Decision Maker Level**: Primary/Secondary/Influencer

**Business Details:**

- **Primary Business**: Core business activities
- **Technology Stack**: Current technology environment
- **Current Vendors**: Existing vendor relationships
- **Payment Terms**: Preferred payment arrangements

### **2. OEM & Product Reference Data Management**

**OEM Registry (Reference Data):**

- OEM Name, Contact Details, Payment Terms
- Performance Rating, Service Level (Gold/Silver/Bronze)
- Managed as dropdown selections for customer assignments

**Product Catalog (Reference Data):**

- Product Name, Category, Description, Unit Price
- License Type (Subscription/Perpetual/Trial)
- Mapped to OEMs via dropdown relationships
- Available for customer selection based on OEM choice

### **3. Deal/Project Management (4-Phase Tracking)**

**Deal/Project Creation Process:**

1. **Select Customer**: Choose from existing companies or add new customer
2. **Select OEM**: Choose OEM partner from dropdown
3. **Select Product**: Product list filtered by selected OEM
4. **Set Quantity**: License quantity and pricing
5. **Deal/Project Name**: Descriptive project identifier

**Phase 1 - Customer Engagement:**

- Customer PO Details, Canarys Invoice Generation, Payment Terms, Payment Status

**Phase 2 - OEM Procurement:**

- OEM Quote Estimation, Canarys PO to OEM, Cost Analysis, Margin Calculation

**Phase 3 - License Delivery:**

- OEM License Issuance, License Start/End Dates, Customer License Details

**Phase 4 - OEM Settlement:**

- OEM Invoice to Canarys, Payment Processing, Settlement Status

### **4. Multi-Invoice System (Phase-Mapped)**

**Invoice Types:**

- **Customer→Canarys**: Phase 1 customer billing
- **Canarys→OEM**: Phase 2 procurement orders
- **OEM→Canarys**: Phase 4 vendor billing

**Invoice Fields:**

- Invoice Number, Date, Due Date, Amount, Tax Amount
- Payment Status (Unpaid/Paid/Overdue/Partial)
- Business Phase Mapping (1, 2, 3, 4)
- Payment Method, Reference, Notes

### **5. Pipeline Management (Future Projects)**

**Pipeline Features:**

- Future deals with estimated revenue only
- Margin calculations based on estimated costs
- Stage tracking: Lead → Quoted → Negotiation → Won → Lost
- **No Forecasting**: Only actual estimates and projections
- Bigin.com-style pipeline views (List/Sheet/Kanban)

## Removed Features (No Forecasting/Predictions)

❌ **Analytics & Reporting Features Moved to Future Phase 6 (HIGH PRIORITY PIPELINE):**

**🚀 Priority Analytics Dashboard Components for Phase 6:**

- **Revenue Analytics Dashboard**: Real-time revenue tracking with margin analysis
- **Customer Performance Analytics**: Engagement metrics and conversion analysis
- **OEM Performance Tracking**: Partnership efficiency and service level monitoring
- **BA Target Achievement Dashboard**: Individual and team performance scorecards
- **Business Intelligence Reports**: Automated reporting with custom KPI tracking
- **Custom Report Generation**: Drag-and-drop report builder with data visualization
- **Interactive Data Visualization**: Charts, graphs, heatmaps with drill-down capabilities
- **KPI Monitoring & Alert Systems**: Real-time performance tracking with notifications

**📊 Analytics Pipeline Foundation (Already Built-in Current Phase):**

- Data collection infrastructure for all customer interactions and deal progression
- Revenue and margin calculation framework across 4-phase workflow
- Customer engagement tracking and activity logging systems
- OEM performance data capture and relationship strength indicators

❌ **Eliminated Features:**

- Market trend analysis and predictions
- Revenue forecasting algorithms
- Predictive analytics for deal closure
- Complex forecasting models
- Future market projections
- Automated prediction systems

## Core System Modules (Bigin.com UX)

1. **Add Customer/Company**: Complete customer management with Bigin.com-style forms
2. **OEM & Product Reference**: Dropdown-driven data management
3. **Deal/Project Management**: 4-phase workflow with Bigin.com interface
4. **Multi-Invoice System**: Phase-specific invoice tracking
5. **Pipeline Dashboard**: Future projects (estimates only, no forecasting)
6. **Activity Management**: CRM-style follow-ups and interaction tracking

## Core System Modules (Current Phase 1-4 + Future Analytics Pipeline)

**Current Implementation (Phases 1-4):**

1. **Add Customer/Company**: Complete CRM with business intelligence foundation
2. **OEM & Product Reference**: Dropdown management with performance tracking setup
3. **Deal/Project Management**: 4-Phase workflow with analytics data collection
4. **Multi-Invoice System**: Phase-specific invoice tracking with revenue analytics preparation
5. **Pipeline Dashboard**: Future projects (estimates only, no forecasting)
6. **Activity Management**: CRM-style follow-ups and interaction tracking

**Future Phase 6: Analytics Dashboard & Business Intelligence Pipeline**

**📊 Comprehensive Analytics Requirements:**

7. **BA Target Analytics Dashboard**:

   - Individual and team target setting with achievement tracking
   - Performance scorecards with KPI monitoring and ranking systems
   - Real-time progress tracking with visual indicators and alerts

8. **Revenue Performance Analytics**:

   - Multi-dimensional revenue analysis (customer/OEM/product breakdowns)
   - Margin tracking and profitability analysis across all deal phases
   - Historical trend analysis with interactive charts and drill-down capabilities

9. **Customer Intelligence Dashboard**:

   - Customer engagement metrics and interaction history analysis
   - Deal velocity tracking and conversion rate optimization insights
   - Customer segmentation and lifetime value calculations

10. **OEM Partnership Analytics**:

    - OEM performance tracking with service level agreement monitoring
    - Deal processing efficiency and payment compliance analytics
    - Partnership strength indicators and relationship health scoring

11. **Business Intelligence & Reporting Engine**:

    - Custom report builder with drag-and-drop functionality
    - Automated monthly/quarterly business performance reports
    - Data visualization suite: charts, graphs, heatmaps, and trend analysis

12. **Real-time Dashboard & KPI Monitoring**:
    - Live business metrics dashboard with role-based customization
    - Automated alert system for performance thresholds and target achievements
    - Mobile-responsive analytics interface with Bigin.com-style UX design

## Example Use Case (Desktop Workflow)

**Desktop Business Scenario:**

- **Customer**: Infosys (Add Customer/Company via comprehensive desktop forms)
- **OEM**: GitHub (Reference Data Selection from advanced dropdown with search)
- **Product**: GitHub Copilot (10 subscriptions managed via desktop data grids)
- **Deal/Project**: "Infosys GitHub Copilot Deployment" (tracked via desktop dashboard)
- **Workflow**: Infosys PO → Canarys Invoice → GitHub procurement → License delivery → GitHub payment
- **Desktop Features**: Multi-tab interface, side-by-side data comparison, advanced filtering, bulk operations

## Desktop-First Responsive Design Strategy

### **Primary Focus: Desktop Workflow Optimization**

- **Screen Resolution Priority**: 1920x1080, 1440x900, 1366x768
- **Secondary Support**: Tablet landscape (1024x768+) and larger screens
- **Layout Approach**: Wide-screen layouts with multi-column data displays
- **Navigation**: Horizontal navigation bars and sidebar layouts optimized for mouse/keyboard interaction
- **Data Density**: Higher information density suitable for business desktop environments

### **Responsive Breakpoints (Desktop-First)**

- **Large Desktop**: ≥1400px (primary optimization target)
- **Standard Desktop**: 1200px-1399px (secondary optimization)
- **Small Desktop/Large Tablet**: 992px-1199px (basic responsiveness)
- **Tablet Landscape**: 768px-991px (minimal responsive support)
- **Mobile/Small Screens**: <768px (not actively supported, basic fallback only)

### **UI/UX Design Principles**

- **Information Architecture**: Dashboard-style layouts with comprehensive data visibility
- **Interaction Patterns**: Hover effects, right-click context menus, keyboard shortcuts
- **Form Design**: Multi-column forms with advanced field grouping
- **Data Tables**: Sortable, filterable tables with inline editing capabilities
- **Charts & Analytics**: Large-format visualizations optimized for detailed analysis

## Development Plan (4 Phases - 12 Weeks) - Desktop-First Bigin.com UX Implementation

### **Phase 1: Foundation & Add Customer/Company (Weeks 1-3)**

**Week 1: Project Setup & Bigin.com UI Foundation**

- ASP.NET Core MVC project with Entity Framework setup
- Authentication system with role-based authorization
- Bigin.com-inspired desktop-first layout with responsive navigation
- Modern UI framework setup (Bootstrap 5.x + custom Bigin.com styles optimized for desktop workflow)

**Week 2: Add Customer/Company Module**

- Company entity with complete business fields
- Contact Persons with decision-maker hierarchy
- Bigin.com-style customer creation forms optimized for desktop data entry
- Customer list view with advanced search, filter, and sorting for large datasets

**Week 3: Customer Management Features**

- Customer profile pages with tabbed interface optimized for desktop viewing
- Contact management within customer profiles with detailed data grids
- Customer-OEM relationship tracking with comprehensive desktop dashboards
- Basic customer activity tracking with timeline views

### **Phase 2: OEM & Product Reference Data (Weeks 4-6)**

**Week 4: OEM Reference Data Management**

- OEM registry with dropdown functionality
- OEM profile management (simplified reference data)
- Performance rating and service level tracking
- OEM-Product relationship setup

**Week 5: Product Catalog Management**

- Product catalog linked to OEMs
- Dynamic OEM-Product dropdown relationships
- Product categorization and pricing management
- Bulk product import and management tools

**Week 6: Reference Data Integration**

- Customer-OEM-Product relationship mapping
- Dynamic dropdown integration in customer forms
- Reference data validation and integrity checks
- Data export and basic reporting for reference tables

### **Phase 3: Deal/Project Management (Weeks 7-9)**

**Week 7: Deal/Project Creation & 4-Phase Setup**

- Deal creation with customer/OEM/product selection
- 4-phase workflow implementation
- Phase 1 & 2: Customer PO and OEM procurement tracking
- Bigin.com-style deal forms and validation (Responsive)

**Week 8: Advanced Deal Features & Multi-Invoice System**

- Phase 3 & 4: License delivery and OEM settlement
- Multi-invoice system with phase mapping
- Invoice generation and payment tracking
- Deal collaboration and assignment features

**Week 9: Deal Management & Bigin.com Desktop Views**

- Deal pipeline with List/Sheet/Kanban views optimized for large screen real estate
- Deal stage management and progression tracking with detailed desktop interfaces
- Activity tracking and deal timeline with comprehensive desktop layouts
- Deal export and advanced reporting with desktop-optimized data visualization

### **Phase 4: Pipeline & Final Integration (Weeks 10-12)**

**Week 10: Pipeline Management**

- Future projects pipeline (estimates only, no forecasting)
- Pipeline views in Bigin.com desktop style (List/Sheet/Kanban) with advanced data grids
- Estimated revenue and margin calculations with detailed desktop forms
- Pipeline export functionality with comprehensive desktop reporting tools

**Week 11: Bigin.com UX Enhancement & Activity Management**

- Complete Bigin.com-inspired desktop interface implementation
- Desktop-responsive optimization with tablet/larger screen adaptation
- Advanced search and filtering capabilities optimized for desktop workflow
- CRM-style activity tracking and follow-ups with desktop-centric design

**Week 12: Testing, Integration & Desktop Optimization**

- Comprehensive testing across all modules with desktop workflow focus
- Performance optimization for desktop browsers and large datasets
- Security hardening and desktop application patterns
- User acceptance testing with desktop-centric feedback incorporation
- Production deployment with desktop-optimized configurations and training documentation

### **Future Phase 6: Analytics Dashboard & Business Intelligence (Comprehensive Pipeline)**

**📊 Analytics Dashboard Requirements for Future Implementation:**

**Week 13-15: Core Analytics Infrastructure**

**1. BA Target Analytics & Performance Tracking:**

- BA monthly/quarterly target setting and achievement tracking
- Performance dashboards with actual vs target metrics (no forecasting)
- Individual BA performance scorecards with KPI tracking
- Team performance comparison and ranking systems
- Achievement badges and performance recognition features

**2. Revenue Analytics Dashboard:**

- Monthly/quarterly revenue tracking by customer, OEM, and product
- Revenue analysis with margin calculations and profitability tracking
- Deal revenue progression through 4-phase workflow
- Customer lifetime value calculations (historical data only)
- Revenue trend analysis without predictive forecasting

**3. Customer Performance Analytics:**

- Customer engagement and interaction tracking
- Customer deal velocity and conversion rate analysis
- Customer satisfaction and retention metrics
- Top performing customers by revenue and deal volume
- Customer segmentation based on industry, size, and spending patterns

**4. OEM Performance Analytics:**

- OEM partnership performance and service level tracking
- OEM deal processing time and efficiency metrics
- OEM payment terms compliance and financial performance
- Product performance analysis by OEM partnerships
- OEM relationship strength indicators and communication tracking

**5. Interactive Business Intelligence Dashboards:**

- Real-time dashboard with key business metrics
- Custom report generation with drag-and-drop interface
- Data visualization: charts, graphs, heatmaps, and trend lines
- Exportable reports in PDF, Excel, and CSV formats
- Role-based dashboard customization for different user types

**6. KPI Monitoring & Alert Systems:**

- Automated alerts for target achievements and misses
- Performance threshold monitoring with notification system
- Deal pipeline health monitoring and bottleneck identification
- Customer payment status and overdue tracking alerts
- OEM performance decline warnings and escalation procedures

**Week 16-18: Advanced Analytics Features**

**7. Advanced Reporting & Data Mining:**

- Multi-dimensional data analysis and cross-referencing
- Historical trend analysis for strategic decision making
- Custom KPI creation and tracking capabilities
- Automated monthly/quarterly business performance reports
- Data export and integration with external BI tools

**8. Bigin.com-Style Analytics UX:**

- Desktop-first responsive analytics dashboard design
- Interactive charts with drill-down capabilities optimized for larger screens
- Real-time data refresh and live dashboard updates
- Intuitive filter and search functionality across all analytics
- Modern dashboard widgets with customizable layouts for desktop workflow

### **Quality Standards (Bigin.com Level):**

- Response time <2 seconds for 10 concurrent users
- Desktop-first responsive design with tablet/larger screen compatibility
- Role-based security: BA, Sales, Finance, Management, Admin
- Bigin.com-inspired UI/UX design with modern desktop-centric aesthetics
- Code coverage >80% with comprehensive testing
- Cross-browser compatibility (Chrome, Firefox, Safari, Edge)
- WCAG 2.1 accessibility compliance with desktop workflow optimization

## Database Schema (Updated for Bigin.com UX & Consistent Naming)

```sql
-- Companies (Add Customer/Company with Comprehensive Fields)
CREATE TABLE Companies (
    CompanyId INT PRIMARY KEY IDENTITY(1,1),
    CompanyName NVARCHAR(200) NOT NULL,
    Industry NVARCHAR(100),
    CompanySize NVARCHAR(50), -- "1-50", "51-200", "201-1000", "1000+"
    AnnualRevenue DECIMAL(18,2),
    Address NVARCHAR(500),
    Headquarters NVARCHAR(200),
    ContactNumber NVARCHAR(20), -- Renamed for consistency
    Email NVARCHAR(200),
    Website NVARCHAR(200),
    CompanyType NVARCHAR(50), -- "Prospect", "Customer", "Partner"
    PaymentTerms NVARCHAR(100), -- "Net 30", "Net 45", "Immediate"
    PrimaryBusiness NVARCHAR(300),
    TechnologyStack NVARCHAR(500),
    CurrentVendors NVARCHAR(500),
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100),
    LastModifiedDate DATETIME2 DEFAULT GETDATE(),
    LastModifiedBy NVARCHAR(100),
    IsActive BIT DEFAULT 1
);

-- ContactPersons (Contact Details with Consistent Naming)
CREATE TABLE ContactPersons (
    ContactId INT PRIMARY KEY IDENTITY(1,1),
    CompanyId INT NOT NULL,
    Name NVARCHAR(200) NOT NULL, -- Combined First + Last Name
    Email NVARCHAR(200),
    Number NVARCHAR(20), -- Renamed for consistency
    Designation NVARCHAR(100),
    Department NVARCHAR(100),
    DecisionMakerLevel NVARCHAR(50), -- "Primary", "Secondary", "Influencer"
    IsPrimaryContact BIT DEFAULT 0,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100),
    IsActive BIT DEFAULT 1,
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId)
);

-- OEMs (Reference Data for Dropdown Selection)
CREATE TABLE OEMs (
    OemId INT PRIMARY KEY IDENTITY(1,1),
    OemName NVARCHAR(200) NOT NULL,
    ContactEmail NVARCHAR(200),
    ContactNumber NVARCHAR(20), -- Renamed for consistency
    PaymentTerms NVARCHAR(100),
    ServiceLevel NVARCHAR(50), -- "Gold", "Silver", "Bronze"
    PerformanceRating DECIMAL(3,2), -- 1.00 to 5.00
    Address NVARCHAR(500),
    Website NVARCHAR(200),
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100),
    IsActive BIT DEFAULT 1
);

-- Products (Reference Data Mapped to OEMs)
CREATE TABLE Products (
    ProductId INT PRIMARY KEY IDENTITY(1,1),
    OemId INT NOT NULL,
    ProductName NVARCHAR(200) NOT NULL,
    ProductCategory NVARCHAR(100), -- "Software", "Cloud", "Security", etc.
    ProductDescription NVARCHAR(1000),
    UnitPrice DECIMAL(18,2),
    LicenseType NVARCHAR(100), -- "Subscription", "Perpetual", "Trial"
    MinimumQuantity INT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100),
    IsActive BIT DEFAULT 1,
    FOREIGN KEY (OemId) REFERENCES OEMs(OemId)
);

-- CustomerOemProducts (Reference Data Relationships)
CREATE TABLE CustomerOemProducts (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CompanyId INT NOT NULL,
    OemId INT NOT NULL,
    ProductId INT NOT NULL,
    RelationshipType NVARCHAR(100), -- "Active", "Prospect", "Past"
    RelationshipStartDate DATE,
    Notes NVARCHAR(500),
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId),
    FOREIGN KEY (OemId) REFERENCES OEMs(OemId),
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    UNIQUE (CompanyId, OemId, ProductId)
);

-- Deals (Deal/Project Management with 4-Phase Workflow)
CREATE TABLE Deals (
    DealId INT PRIMARY KEY IDENTITY(1,1),
    CompanyId INT NOT NULL,
    OemId INT NOT NULL,
    ProductId INT NOT NULL,
    ContactId INT,
    DealName NVARCHAR(200) NOT NULL, -- Deal/Project Name
    DealStage NVARCHAR(50) NOT NULL, -- "Lead", "Quoted", "Negotiation", "Won", "Lost"
    DealType NVARCHAR(50), -- "New", "Renewal", "Upgrade"
    Quantity INT NOT NULL,


    -- Phase 1 - Customer Engagement
    CustomerPoNumber NVARCHAR(100),
    CustomerPoDate DATE,
    CustomerInvoiceNumber NVARCHAR(100),
    CustomerInvoiceAmount DECIMAL(18,2),
    CustomerPaymentStatus NVARCHAR(50), -- "Pending", "Paid", "Overdue"
    CustomerPaymentDate DATE,

    -- Phase 2 - OEM Procurement
    OemQuoteAmount DECIMAL(18,2),
    CanarysPoNumber NVARCHAR(100),
    CanarysPoDate DATE,
    EstimatedMargin DECIMAL(18,2),

    -- Phase 3 - License Delivery
    LicenseStartDate DATE,
    LicenseEndDate DATE,
    LicenseDeliveryStatus NVARCHAR(50), -- "Pending", "Delivered", "Active"

    -- Phase 4 - OEM Settlement
    OemInvoiceNumber NVARCHAR(100),
    OemInvoiceAmount DECIMAL(18,2),
    OemPaymentStatus NVARCHAR(50), -- "Pending", "Paid"
    OemPaymentDate DATE,

    ExpectedCloseDate DATE,
    ActualCloseDate DATE,
    AssignedTo NVARCHAR(100), -- BA/Sales person
    DealProbability DECIMAL(3,2), -- 0.00 to 1.00
    Notes NVARCHAR(1000),
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100),
    LastModifiedDate DATETIME2 DEFAULT GETDATE(),
    LastModifiedBy NVARCHAR(100),
    IsActive BIT DEFAULT 1,

    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId),
    FOREIGN KEY (OemId) REFERENCES OEMs(OemId),
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    FOREIGN KEY (ContactId) REFERENCES ContactPersons(ContactId)
);

-- Invoices (Multi-Invoice System with Phase Mapping)
CREATE TABLE Invoices (
    InvoiceId INT PRIMARY KEY IDENTITY(1,1),
    DealId INT NOT NULL,
    InvoiceType NVARCHAR(50) NOT NULL, -- "Customer_To_Canarys", "Canarys_To_OEM", "OEM_To_Canarys"
    InvoiceNumber NVARCHAR(100) NOT NULL UNIQUE,
    InvoiceDate DATE NOT NULL,
    DueDate DATE,
    Amount DECIMAL(18,2) NOT NULL,
    TaxAmount DECIMAL(18,2) DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL,
    PaymentStatus NVARCHAR(50) DEFAULT 'Unpaid', -- "Unpaid", "Paid", "Overdue", "Partial"
    PaymentDate DATE,
    PaymentMethod NVARCHAR(50),
    PaymentReference NVARCHAR(100),
    BusinessPhase INT NOT NULL, -- 1, 2, 3, 4
    Notes NVARCHAR(500),
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100),
    FOREIGN KEY (DealId) REFERENCES Deals(DealId)
);

-- Future Phase 6: Analytics Dashboard & Business Intelligence Tables (Comprehensive Pipeline)
/*
📊 ANALYTICS DATABASE SCHEMA - FUTURE PHASE 6 IMPLEMENTATION

-- BATargets (BA Analytics Dashboard - Performance Tracking)
CREATE TABLE BATargets (
    TargetId INT PRIMARY KEY IDENTITY(1,1),
    TargetType NVARCHAR(100) NOT NULL, -- "Revenue", "Customer_Acquisition", "Deal_Count", "Margin"
    AssignedTo NVARCHAR(100) NOT NULL, -- BA/Sales person
    TargetValue DECIMAL(18,2) NOT NULL,
    ActualValue DECIMAL(18,2) DEFAULT 0,
    TargetPeriod NVARCHAR(20) NOT NULL, -- "2025-Q3", "2025-08", "2025"
    PeriodType NVARCHAR(20) NOT NULL, -- "Monthly", "Quarterly", "Annual"
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    AchievementPercentage AS (CASE WHEN TargetValue > 0 THEN (ActualValue / TargetValue) * 100 ELSE 0 END),
    Status NVARCHAR(50) DEFAULT 'Active', -- "Active", "Achieved", "Missed", "Inactive"
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100)
);

-- PerformanceMetrics (KPI Dashboard & Analytics Tracking)
CREATE TABLE PerformanceMetrics (
    MetricId INT PRIMARY KEY IDENTITY(1,1),
    MetricName NVARCHAR(100) NOT NULL, -- "Revenue_Growth", "Deal_Velocity", "Customer_Satisfaction"
    MetricCategory NVARCHAR(50) NOT NULL, -- "Revenue", "Customer", "OEM", "Operational"
    MetricValue DECIMAL(18,4) NOT NULL,
    CalculationPeriod NVARCHAR(20) NOT NULL, -- "2025-08", "2025-Q3"
    EntityType NVARCHAR(50), -- "Company", "OEM", "BA", "Overall"
    EntityId INT, -- Reference to specific entity
    BenchmarkValue DECIMAL(18,4), -- Target or benchmark for comparison
    TrendDirection NVARCHAR(10), -- "Up", "Down", "Stable"
    LastCalculated DATETIME2 DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100)
);

-- RevenueAnalytics (Revenue Dashboard & Profitability Tracking)
CREATE TABLE RevenueAnalytics (
    RevenueAnalyticsId INT PRIMARY KEY IDENTITY(1,1),
    CompanyId INT, -- Customer analysis
    OemId INT, -- OEM analysis
    ProductId INT, -- Product analysis
    DealId INT, -- Deal-specific analysis
    AnalyticsPeriod NVARCHAR(20) NOT NULL, -- "2025-08", "2025-Q3"
    TotalRevenue DECIMAL(18,2) NOT NULL,
    TotalCost DECIMAL(18,2) NOT NULL,
    GrossMargin AS (TotalRevenue - TotalCost),
    MarginPercentage AS (CASE WHEN TotalRevenue > 0 THEN ((TotalRevenue - TotalCost) / TotalRevenue) * 100 ELSE 0 END),
    DealCount INT DEFAULT 0,
    AverageDealsSize AS (CASE WHEN DealCount > 0 THEN TotalRevenue / DealCount ELSE 0 END),
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId),
    FOREIGN KEY (OemId) REFERENCES Oems(OemId),
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    FOREIGN KEY (DealId) REFERENCES Deals(DealId)
);

-- CustomerAnalytics (Customer Intelligence Dashboard)
CREATE TABLE CustomerAnalytics (
    CustomerAnalyticsId INT PRIMARY KEY IDENTITY(1,1),
    CompanyId INT NOT NULL,
    AnalyticsPeriod NVARCHAR(20) NOT NULL,
    TotalDeals INT DEFAULT 0,
    TotalRevenue DECIMAL(18,2) DEFAULT 0,
    AverageDealSize AS (CASE WHEN TotalDeals > 0 THEN TotalRevenue / TotalDeals ELSE 0 END),
    DealVelocityDays DECIMAL(10,2), -- Average days from lead to close
    EngagementScore DECIMAL(5,2), -- Customer engagement rating (1-10)
    SatisfactionScore DECIMAL(5,2), -- Customer satisfaction rating (1-10)
    LastInteractionDate DATETIME2,
    NextFollowUpDate DATETIME2,
    LifetimeValue DECIMAL(18,2), -- Historical total value
    RiskScore NVARCHAR(20), -- "Low", "Medium", "High"
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId)
);

-- OEMAnalytics (OEM Partnership Performance Dashboard)
CREATE TABLE OEMAnalytics (
    OEMAnalyticsId INT PRIMARY KEY IDENTITY(1,1),
    OemId INT NOT NULL,
    AnalyticsPeriod NVARCHAR(20) NOT NULL,
    TotalDeals INT DEFAULT 0,
    TotalRevenue DECIMAL(18,2) DEFAULT 0,
    AverageProcessingDays DECIMAL(10,2), -- Deal processing efficiency
    PaymentCompliancePercentage DECIMAL(5,2), -- Payment terms compliance
    ServiceLevelScore DECIMAL(5,2), -- Service quality rating (1-10)
    PartnershipStrength NVARCHAR(20), -- "Strong", "Good", "Weak"
    ResponseTime DECIMAL(10,2), -- Average response time in hours
    DealsWonPercentage DECIMAL(5,2), -- Success rate
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (OemId) REFERENCES Oems(OemId)
);

-- DashboardWidgets (Custom Dashboard Configuration)
CREATE TABLE DashboardWidgets (
    WidgetId INT PRIMARY KEY IDENTITY(1,1),
    UserId NVARCHAR(450) NOT NULL, -- AspNetUsers reference
    WidgetType NVARCHAR(50) NOT NULL, -- "Revenue_Chart", "KPI_Card", "Target_Progress"
    WidgetTitle NVARCHAR(100) NOT NULL,
    ConfigurationJSON NVARCHAR(MAX), -- Widget settings and filters
    Position INT DEFAULT 0, -- Display order
    IsVisible BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    LastModified DATETIME2 DEFAULT GETDATE()
);

-- BusinessReports (Custom Report Generation & Scheduling)
CREATE TABLE BusinessReports (
    ReportId INT PRIMARY KEY IDENTITY(1,1),
    ReportName NVARCHAR(200) NOT NULL,
    ReportType NVARCHAR(50) NOT NULL, -- "Revenue", "Customer", "OEM", "Performance"
    ReportCategory NVARCHAR(50) NOT NULL, -- "Financial", "Operational", "Analytics"
    FilterCriteria NVARCHAR(MAX), -- JSON filters and parameters
    ScheduleType NVARCHAR(20), -- "Manual", "Daily", "Weekly", "Monthly"
    NextRunDate DATETIME2,
    LastRunDate DATETIME2,
    OutputFormat NVARCHAR(20) DEFAULT 'PDF', -- "PDF", "Excel", "CSV"
    CreatedBy NVARCHAR(100),
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1
);
*/

-- Activities (CRM-Style Activity Tracking)
CREATE TABLE Activities (
    ActivityId INT PRIMARY KEY IDENTITY(1,1),
    RelatedEntityType NVARCHAR(50) NOT NULL, -- "Company", "Deal", "Contact"
    RelatedEntityId INT NOT NULL,
    ActivityType NVARCHAR(50) NOT NULL, -- "Call", "Email", "Meeting", "Follow-up"
    Subject NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000),
    ActivityDate DATETIME2 NOT NULL,
    DueDate DATETIME2,
    Status NVARCHAR(50) DEFAULT 'Pending', -- "Pending", "Completed", "Cancelled"
    Priority NVARCHAR(20) DEFAULT 'Medium', -- "Low", "Medium", "High"
    AssignedTo NVARCHAR(100),
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100),
    CompletedDate DATETIME2,
    CompletedBy NVARCHAR(100)
);
*/

## Technical Implementation Stack

### **Desktop-First Development Approach**

**CSS Framework & Styling:**
- Bootstrap 5.x with custom desktop-optimized grid system
- CSS Grid and Flexbox for complex desktop layouts
- Custom CSS variables for consistent desktop theming
- Advanced hover states and desktop interaction patterns
- Print stylesheets for desktop reporting functionality

**JavaScript & Interactions:**
- ES6+ modules for desktop application architecture
- Advanced keyboard navigation and shortcuts
- Mouse event handling (hover, right-click, drag-drop)
- Desktop-optimized data tables with sorting/filtering
- Large-screen chart visualizations and dashboards

**Responsive Strategy:**
- Desktop-first media queries (max-width approach)
- Graceful degradation for smaller screens
- Conditional loading for desktop-specific features
- Performance optimization for desktop workflow patterns

### **Backend:**

- ASP.NET Core 8.0 MVC
- Entity Framework Core
- SQL Server Database
- Azure AD/Identity Framework for Authentication
- Role-based Authorization

### **Frontend:**

- Razor Pages with modern HTML5/CSS3
- Bootstrap 5.x for desktop-first responsive design
- JavaScript ES6+ with AJAX
- Chart.js for data visualization optimized for desktop screens
- Bigin.com-inspired UI components with desktop workflow focus

### **Development Tools:**

- Visual Studio 2022
- Git for version control
- Azure DevOps for project management
- Entity Framework migrations
- NUnit for unit testing

### **Deployment:**

- Azure App Service or IIS
- SQL Server on Azure or on-premises
- SSL certificate and security hardening
- Application monitoring and logging

## Security & Compliance

### **Authentication & Authorization:**

- Multi-factor authentication support
- Role-based access control (RBAC)
- Session management and timeout
- Password policy enforcement
- Audit trail for sensitive operations

### **Data Protection:**

- SQL injection prevention through parameterized queries
- Cross-site scripting (XSS) protection
- Cross-site request forgery (CSRF) tokens
- Data encryption at rest and in transit
- Regular security assessments

### **Business Data Security:**

- Customer data privacy compliance
- Financial data encryption
- Access logging and monitoring
- Backup and disaster recovery procedures
- GDPR compliance considerations

---

## 🎯 **IMPLEMENTATION SUMMARY & ANALYTICS PIPELINE PRIORITY**

**Phase 1-4 Current Implementation (12 Weeks):**
This comprehensive plan delivers a complete Bigin.com-inspired B2B2B CRM system with consistent naming conventions, 4-phase workflow management, enhanced UX design, and professional customer management capabilities. The system focuses on customer relationship management, deal tracking, and business process optimization using real data and structured workflows.

**📊 Phase 6 Analytics Dashboard Pipeline (HIGH PRIORITY - Future 6 Weeks):**
The analytics dashboard represents a critical business intelligence layer that will transform the CRM into a comprehensive performance management system. Key analytics priorities include:

- **Real-time Performance Dashboards** with BA target tracking and achievement monitoring
- **Revenue Analytics Engine** with margin analysis and profitability optimization
- **Customer Intelligence Platform** with engagement metrics and lifetime value tracking
- **OEM Partnership Analytics** with service level monitoring and relationship scoring
- **Custom Business Intelligence** with automated reporting and KPI alert systems
- **Interactive Data Visualization** with drill-down capabilities and mobile responsiveness

**🚀 Analytics Foundation Already Built:**
The current 4-phase implementation includes comprehensive data collection infrastructure, revenue calculation frameworks, and customer interaction tracking systems that will seamlessly integrate with the future analytics dashboard, ensuring zero disruption during Phase 6 implementation.

**Business Impact:** The combined CRM + Analytics platform will provide complete visibility into business performance, enabling data-driven decision making, optimized customer relationships, and strategic partnership management for sustainable growth and competitive advantage.
```
