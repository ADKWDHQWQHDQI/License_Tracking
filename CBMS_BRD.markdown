# Business Requirements Document (BRD) for Canarys Business Management System (CBMS)

## 1. Document Control

| Version | Author           | Description                                                            |
| ------- | ---------------- | ---------------------------------------------------------------------- |
| 1.0     | Business Analyst | Initial Draft Document                                                 |
| 2.0     | Business Analyst | Updated to reflect new deal-centric requirements and CRM functionality |

## 2. Purpose

The Canarys Business Management System (CBMS) is a web-based platform designed to manage the lifecycle of deals involving software licenses and subscriptions. It supports Canarys Automations' role as an intermediary between customers and Original Equipment Manufacturers (OEMs), tracking the process from customer purchase orders to OEM payments. CBMS incorporates CRM-like features, including company and contact management, deal pipeline tracking, and analytics, with a modern UI/UX inspired by Bigin.com.

## 3. Scope

The application will:

- Manage customer companies and their contacts.
- Track deals through a four-phase workflow:
  1. Customer issues Purchase Order (PO) to Canarys, Canarys releases invoice, customer pays.
  2. Canarys estimates quote from OEM, sends PO to OEM.
  3. OEM issues license to customer with start/end dates.
  4. OEM invoices Canarys, Canarys pays OEM.
- Support pipeline management for future deals with estimated revenue and margins.
- Provide analytics for actual vs. target performance (no forecasting).
- Offer a desktop-first responsive UI with List, Sheet, and Kanban views optimized for desktop workflow.

## 4. Stakeholders

| Stakeholder       | Role                                      |
| ----------------- | ----------------------------------------- |
| Sales Team        | Manages customer relationships and deals  |
| Finance Team      | Tracks invoices, payments, and margins    |
| Operations Team   | Handles procurement and OEM relationships |
| Management        | Reviews performance and analytics         |
| IT Administrator  | Manages system users and roles            |
| Business Analysts | Tracks targets and pipeline performance   |

## 5. Functional Requirements

### 5.1 Company and Contact Management Module

**Features**:

- Add, update, view, delete company and contact records.
- Classify companies as prospects, customers, or partners.
- Link multiple contacts to a company.

**Fields (Company)**:

- Company Name
- Industry
- Address (City, State, Country, ZipCode)
- Employee Size
- Headquarters
- Contact Number
- Email
- Website
- Company Type (Prospect/Customer/Partner)

**Fields (Contact)**:

- Name (First and Last)
- Email
- Phone
- Designation
- Department
- Decision Maker Level
- Is Primary Contact
- Notes

### 5.2 OEM and Product Management Module

**Features**:

- Register and manage OEMs with contact details and performance ratings.
- Maintain a product catalog linked to OEMs with pricing.

**Fields (OEM)**:

- OEM Name
- Contact Email
- Contact Phone
- Address
- Service Level (Gold/Silver/Bronze)
- Payment Terms
- Performance Rating
- Notes

**Fields (Product)**:

- Product Name
- Category
- Description
- Version
- License Type (Subscription/Perpetual/Trial)
- Unit Price (USD)
- OEM (dropdown)

### 5.3 Deal Management Module

**Features**:

- Create, update, view, delete deals.
- Track deals through the four-phase workflow.
- Manage financial details, including costs, revenues, and margins.

**Fields (Deal)**:

- Resource (BA/Sales person)
- CRM Update
- Company Name (dropdown)
- Project ID
- OEM (dropdown)
- PO Type (New License/Renewal/Upgrade)
- Item Description
- No of Licenses
- License Type
- Services
- Start Date
- End Date
- PO Amount
- Client PO Number
- Date of PO Received
- OEM (USD to INR)
- Actual Market Price (USD)
- Actual Market Price (INR)
- Quote received from OEM (INR)
- Partner Discount
- Partner Discount %
- Customer Discount %
- Customer Price (INR)
- Canarys Margin (INR)
- Canarys Margin %
- PO raised to OEM ($)
- PO No for OEM
- PO Date to OEM
- Notes

### 5.4 Pipeline Management Module

**Features**:

- Manage future deals with estimated revenue and margins.
- Track pipeline stages and convert to active deals.

**Fields (Pipeline)**:

- Company Name (dropdown)
- OEM (dropdown)
- Product (dropdown)
- Quantity
- Estimated Revenue
- Estimated Margin
- Success Probability
- Expected Close Date
- Pipeline Stage (Lead/Qualified/Proposal/Negotiation)

### 5.5 Billing & Invoicing Module

**Features**:

- Generate and track invoices for customers (Phase 1) and OEMs (Phase 4).
- Monitor payment statuses and aging summaries.

**Fields (Invoice)**:

- Invoice ID
- Deal ID
- Invoice Type (Customer_To_Canarys / OEM_To_Canarys)
- Invoice Number
- Invoice Date
- Amount
- Payment Status
- Payment Date
- Payment Received
- Payment Received Date

### 5.6 Analytics & Reporting Module

**Features**:

- Provide Bigin.com-inspired views: List, Sheet, Kanban, Dashboard.
- Track BA performance against targets (revenue, margins, deal count).
- Generate exportable reports (Excel/PDF).

**Analytics**:

- OEM Analytics: Cost analysis, payment terms
- Customer Analytics: Revenue, payment behavior
- Deal Analytics: Margin analysis, success rates
- BA Performance: Achievement tracking

### 5.7 Notification & Alerts Module

**Features**:

- Send email and in-app notifications for renewals, payments, expiries, and approvals.

### 5.8 User Management & Roles Module

**Roles**:

- Admin (full access)
- Sales (deals, customers)
- Finance (billing, payments)
- Operations (procurement, OEMs)
- Management (analytics, reports)
- BA (pipeline, targets)

## 6. Integration Between Modules

| Module                | Integration                                         |
| --------------------- | --------------------------------------------------- |
| Company Management    | Links to deals and pipeline                         |
| Deal Management       | Integrates with companies, OEMs, products, invoices |
| Pipeline Management   | Mirrors deal structure with estimated data          |
| Billing & Invoicing   | Ties to deals and payment statuses                  |
| Analytics & Reporting | Aggregates data from all modules                    |
| Notification Module   | Integrates with deal and renewal data               |

## 7. Non-Functional Requirements

- **Scalability**: Support growing data volume.
- **Security**: Role-based access, data encryption.
- **Availability**: 99.9% uptime.
- **Performance**: <2s for alerts/reporting.
- **Desktop Responsiveness**: Optimized for desktop workflow with tablet/larger screen compatibility.

## 8. Assumptions

- Data entry via manual input or bulk upload.
- Email service available for notifications.
- Unique deal identification.

## 9. Future Enhancements

- API integration with OEMs/CRMs.
- Automated invoicing.
- Enhanced graphical dashboards.

## 10. Appendix

### Sample Data Fields for Deal Entry

| Field Name           | Sample Data    |
| -------------------- | -------------- |
| Company Name         | Infosys        |
| OEM Name             | Git            |
| Product Name         | GitHub Copilot |
| No of Licenses       | 10             |
| Start Date           | 2025-11-01     |
| End Date             | 2026-10-31     |
| Client PO Number     | PO12345        |
| PO Amount            | ₹1,00,000      |
| PO No for OEM        | OEM45678       |
| PO raised to OEM ($) | $80,000        |
| Invoice Number       | INV-78901      |
| Amount Received      | ₹1,00,000      |
| OEM Invoice          | OEM-INV-321    |
| Amount Paid          | ₹80,000        |
| Canarys Margin (INR) | ₹20,000        |
