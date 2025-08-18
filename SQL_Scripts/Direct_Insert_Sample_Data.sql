-- Direct INSERT statements for sample data
-- Execute this with sqlcmd directly

-- Insert sample companies
INSERT INTO Companies (CompanyName, Industry, CompanySize, Address, City, State, Country, ZipCode, Phone, Email, Website, CompanyType, CreatedDate, IsActive)
VALUES 
('TechCorp Solutions Ltd.', 'Technology', 'Medium', '123 Business Park, Tower A', 'Mumbai', 'Maharashtra', 'India', '400001', '+91-22-12345678', 'contact@techcorp.com', 'www.techcorp.com', 'Customer', GETDATE(), 1),
('Global Enterprises Inc.', 'Manufacturing', 'Large', '456 Industrial Estate', 'Bangalore', 'Karnataka', 'India', '560001', '+91-80-87654321', 'info@global-ent.com', 'www.global-ent.com', 'Customer', GETDATE(), 1),
('StartupX Innovations', 'Software', 'Small', '789 Tech Hub', 'Hyderabad', 'Telangana', 'India', '500001', '+91-40-11223344', 'hello@startupx.com', 'www.startupx.com', 'Prospect', GETDATE(), 1);

-- Insert sample OEMs
INSERT INTO Oems (OemName, ContactPerson, Email, Phone, Address, City, State, Country, ZipCode, PaymentTerms, PerformanceRating, Notes, CreatedDate, IsActive)
VALUES 
('Microsoft Corporation', 'John Smith', 'partner@microsoft.com', '+1-425-882-8080', 'One Microsoft Way', 'Redmond', 'WA', 'USA', '98052', 'Net 30', 5, 'Premium OEM partner for Microsoft products', GETDATE(), 1),
('Adobe Systems Inc.', 'Sarah Johnson', 'partners@adobe.com', '+1-408-536-6000', '345 Park Avenue', 'San Jose', 'CA', 'USA', '95110', 'Net 45', 4, 'Creative software solutions partner', GETDATE(), 1),
('Autodesk Inc.', 'Mike Wilson', 'reseller@autodesk.com', '+1-415-507-5000', '111 McInnis Parkway', 'San Rafael', 'CA', 'USA', '94903', 'Net 30', 4, 'Engineering and design software partner', GETDATE(), 1);

PRINT 'Companies and OEMs inserted successfully';
