using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace License_Tracking.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultRolesAndUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create all roles first, then users, then user-role assignments
            migrationBuilder.Sql(@"
                -- Seed CBMS B2B2B Roles
                IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE NormalizedName = 'ADMIN')
                BEGIN
                    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
                    VALUES ('1a2b3c4d-5e6f-7890-abcd-1234567890ab', 'Admin', 'ADMIN', NEWID())
                END

                IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE NormalizedName = 'SALES')
                BEGIN
                    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
                    VALUES ('2b3c4d5e-6f78-90ab-cdef-234567890abc', 'Sales', 'SALES', NEWID())
                END

                IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE NormalizedName = 'FINANCE')
                BEGIN
                    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
                    VALUES ('3c4d5e6f-7890-abcd-ef12-34567890abcd', 'Finance', 'FINANCE', NEWID())
                END

                IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE NormalizedName = 'OPERATIONS')
                BEGIN
                    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
                    VALUES ('4d5e6f78-90ab-cdef-1234-567890abcdef', 'Operations', 'OPERATIONS', NEWID())
                END

                IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE NormalizedName = 'MANAGEMENT')
                BEGIN
                    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
                    VALUES ('5e6f7890-abcd-ef12-3456-7890abcdef12', 'Management', 'MANAGEMENT', NEWID())
                END

                IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE NormalizedName = 'BA')
                BEGIN
                    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
                    VALUES ('6f789012-cdef-1234-5678-90abcdef1234', 'BA', 'BA', NEWID())
                END
            ");

            // Create all users
            migrationBuilder.Sql(@"
                -- Admin User
                IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE NormalizedEmail = 'ADMIN@CBMS.COM')
                BEGIN
                    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
                    VALUES ('admin-user-guid-1234-567890abcdef', 'admin@cbms.com', 'ADMIN@CBMS.COM', 'admin@cbms.com', 'ADMIN@CBMS.COM', 1, 
                           'AQAAAAIAAYagAAAAEH8VmMqE4H6Y5J2K3L4M5N6O7P8Q9R0S1T2U3V4W5X6Y7Z8A9B0C1D2E3F4G5H6I7J8K9L0M', 
                           NEWID(), NEWID(), 0, 0, 1, 0)
                END

                -- Sales User
                IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE NormalizedEmail = 'SALES@CBMS.COM')
                BEGIN
                    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
                    VALUES ('sales-user-guid-1234-567890abcdef', 'sales@cbms.com', 'SALES@CBMS.COM', 'sales@cbms.com', 'SALES@CBMS.COM', 1, 
                           'AQAAAAIAAYagAAAAEH8VmMqE4H6Y5J2K3L4M5N6O7P8Q9R0S1T2U3V4W5X6Y7Z8A9B0C1D2E3F4G5H6I7J8K9L0M', 
                           NEWID(), NEWID(), 0, 0, 1, 0)
                END

                -- Finance User
                IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE NormalizedEmail = 'FINANCE@CBMS.COM')
                BEGIN
                    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
                    VALUES ('finance-user-guid-1234-567890abcd', 'finance@cbms.com', 'FINANCE@CBMS.COM', 'finance@cbms.com', 'FINANCE@CBMS.COM', 1, 
                           'AQAAAAIAAYagAAAAEH8VmMqE4H6Y5J2K3L4M5N6O7P8Q9R0S1T2U3V4W5X6Y7Z8A9B0C1D2E3F4G5H6I7J8K9L0M', 
                           NEWID(), NEWID(), 0, 0, 1, 0)
                END

                -- Operations User
                IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE NormalizedEmail = 'OPERATIONS@CBMS.COM')
                BEGIN
                    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
                    VALUES ('ops-user-guid-1234-567890abcdef', 'operations@cbms.com', 'OPERATIONS@CBMS.COM', 'operations@cbms.com', 'OPERATIONS@CBMS.COM', 1, 
                           'AQAAAAIAAYagAAAAEH8VmMqE4H6Y5J2K3L4M5N6O7P8Q9R0S1T2U3V4W5X6Y7Z8A9B0C1D2E3F4G5H6I7J8K9L0M', 
                           NEWID(), NEWID(), 0, 0, 1, 0)
                END

                -- Management User
                IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE NormalizedEmail = 'MANAGEMENT@CBMS.COM')
                BEGIN
                    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
                    VALUES ('mgmt-user-guid-1234-567890abcdef', 'management@cbms.com', 'MANAGEMENT@CBMS.COM', 'management@cbms.com', 'MANAGEMENT@CBMS.COM', 1, 
                           'AQAAAAIAAYagAAAAEH8VmMqE4H6Y5J2K3L4M5N6O7P8Q9R0S1T2U3V4W5X6Y7Z8A9B0C1D2E3F4G5H6I7J8K9L0M', 
                           NEWID(), NEWID(), 0, 0, 1, 0)
                END

                -- BA User
                IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE NormalizedEmail = 'BA@CBMS.COM')
                BEGIN
                    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
                    VALUES ('ba-user-guid-1234-567890abcdef', 'ba@cbms.com', 'BA@CBMS.COM', 'ba@cbms.com', 'BA@CBMS.COM', 1, 
                           'AQAAAAIAAYagAAAAEH8VmMqE4H6Y5J2K3L4M5N6O7P8Q9R0S1T2U3V4W5X6Y7Z8A9B0C1D2E3F4G5H6I7J8K9L0M', 
                           NEWID(), NEWID(), 0, 0, 1, 0)
                END
            ");

            // Assign roles to users (only after both roles and users exist)
            migrationBuilder.Sql(@"
                -- Assign roles to users
                IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = 'admin-user-guid-1234-567890abcdef' AND RoleId = '1a2b3c4d-5e6f-7890-abcd-1234567890ab')
                BEGIN
                    INSERT INTO AspNetUserRoles (UserId, RoleId)
                    VALUES ('admin-user-guid-1234-567890abcdef', '1a2b3c4d-5e6f-7890-abcd-1234567890ab')
                END

                IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = 'sales-user-guid-1234-567890abcdef' AND RoleId = '2b3c4d5e-6f78-90ab-cdef-234567890abc')
                BEGIN
                    INSERT INTO AspNetUserRoles (UserId, RoleId)
                    VALUES ('sales-user-guid-1234-567890abcdef', '2b3c4d5e-6f78-90ab-cdef-234567890abc')
                END

                IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = 'finance-user-guid-1234-567890abcd' AND RoleId = '3c4d5e6f-7890-abcd-ef12-34567890abcd')
                BEGIN
                    INSERT INTO AspNetUserRoles (UserId, RoleId)
                    VALUES ('finance-user-guid-1234-567890abcd', '3c4d5e6f-7890-abcd-ef12-34567890abcd')
                END

                IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = 'ops-user-guid-1234-567890abcdef' AND RoleId = '4d5e6f78-90ab-cdef-1234-567890abcdef')
                BEGIN
                    INSERT INTO AspNetUserRoles (UserId, RoleId)
                    VALUES ('ops-user-guid-1234-567890abcdef', '4d5e6f78-90ab-cdef-1234-567890abcdef')
                END

                IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = 'mgmt-user-guid-1234-567890abcdef' AND RoleId = '5e6f7890-abcd-ef12-3456-7890abcdef12')
                BEGIN
                    INSERT INTO AspNetUserRoles (UserId, RoleId)
                    VALUES ('mgmt-user-guid-1234-567890abcdef', '5e6f7890-abcd-ef12-3456-7890abcdef12')
                END

                IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = 'ba-user-guid-1234-567890abcdef' AND RoleId = '6f789012-cdef-1234-5678-90abcdef1234')
                BEGIN
                    INSERT INTO AspNetUserRoles (UserId, RoleId)
                    VALUES ('ba-user-guid-1234-567890abcdef', '6f789012-cdef-1234-5678-90abcdef1234')
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove seeded users and roles
            migrationBuilder.Sql(@"
                -- Remove user role assignments
                DELETE FROM AspNetUserRoles WHERE UserId IN (
                    'admin-user-guid-1234-567890abcdef',
                    'sales-user-guid-1234-567890abcdef', 
                    'finance-user-guid-1234-567890abcd',
                    'ops-user-guid-1234-567890abcdef',
                    'mgmt-user-guid-1234-567890abcdef',
                    'ba-user-guid-1234-567890abcdef'
                )

                -- Remove seeded users
                DELETE FROM AspNetUsers WHERE Id IN (
                    'admin-user-guid-1234-567890abcdef',
                    'sales-user-guid-1234-567890abcdef', 
                    'finance-user-guid-1234-567890abcd',
                    'ops-user-guid-1234-567890abcdef',
                    'mgmt-user-guid-1234-567890abcdef',
                    'ba-user-guid-1234-567890abcdef'
                )

                -- Remove seeded roles
                DELETE FROM AspNetRoles WHERE Id IN (
                    '1a2b3c4d-5e6f-7890-abcd-1234567890ab',
                    '2b3c4d5e-6f78-90ab-cdef-234567890abc',
                    '3c4d5e6f-7890-abcd-ef12-34567890abcd',
                    '4d5e6f78-90ab-cdef-1234-567890abcdef',
                    '5e6f7890-abcd-ef12-3456-7890abcdef12',
                    '6f789012-cdef-1234-5678-90abcdef1234'
                )
            ");
        }
    }
}
