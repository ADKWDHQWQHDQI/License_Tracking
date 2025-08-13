using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace License_Tracking.Migrations
{
    /// <inheritdoc />
    public partial class FixActivityForeignKeyConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Companies_EntityId",
                table: "Activities");

            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Companies_RelatedEntityId",
                table: "Activities");

            migrationBuilder.DropForeignKey(
                name: "FK_Activities_ContactPersons_EntityId",
                table: "Activities");

            migrationBuilder.DropForeignKey(
                name: "FK_Activities_ContactPersons_RelatedEntityId",
                table: "Activities");

            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Deals_EntityId",
                table: "Activities");

            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Deals_RelatedEntityId",
                table: "Activities");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Invoices_InvoiceId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Activities_EntityId",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_RelatedEntityId",
                table: "Activities");

            migrationBuilder.CreateIndex(
                name: "IX_CbmsInvoice_InvoiceType",
                table: "CbmsInvoices",
                column: "InvoiceType");

            migrationBuilder.CreateIndex(
                name: "IX_CbmsInvoice_PaymentStatus",
                table: "CbmsInvoices",
                column: "PaymentStatus");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_CbmsInvoices_InvoiceId",
                table: "Payments",
                column: "InvoiceId",
                principalTable: "CbmsInvoices",
                principalColumn: "InvoiceId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_CbmsInvoices_InvoiceId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_CbmsInvoice_InvoiceType",
                table: "CbmsInvoices");

            migrationBuilder.DropIndex(
                name: "IX_CbmsInvoice_PaymentStatus",
                table: "CbmsInvoices");

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    InvoiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LicenseId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountReceived = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InvoiceType = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VendorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.InvoiceId);
                    table.ForeignKey(
                        name: "FK_Invoices_Deals_LicenseId",
                        column: x => x.LicenseId,
                        principalTable: "Deals",
                        principalColumn: "DealId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_EntityId",
                table: "Activities",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_RelatedEntityId",
                table: "Activities",
                column: "RelatedEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_InvoiceType",
                table: "Invoices",
                column: "InvoiceType");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_PaymentStatus",
                table: "Invoices",
                column: "PaymentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_LicenseId",
                table: "Invoices",
                column: "LicenseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Companies_EntityId",
                table: "Activities",
                column: "EntityId",
                principalTable: "Companies",
                principalColumn: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Companies_RelatedEntityId",
                table: "Activities",
                column: "RelatedEntityId",
                principalTable: "Companies",
                principalColumn: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_ContactPersons_EntityId",
                table: "Activities",
                column: "EntityId",
                principalTable: "ContactPersons",
                principalColumn: "ContactId");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_ContactPersons_RelatedEntityId",
                table: "Activities",
                column: "RelatedEntityId",
                principalTable: "ContactPersons",
                principalColumn: "ContactId");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Deals_EntityId",
                table: "Activities",
                column: "EntityId",
                principalTable: "Deals",
                principalColumn: "DealId");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Deals_RelatedEntityId",
                table: "Activities",
                column: "RelatedEntityId",
                principalTable: "Deals",
                principalColumn: "DealId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Invoices_InvoiceId",
                table: "Payments",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "InvoiceId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
