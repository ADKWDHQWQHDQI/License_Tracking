using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace License_Tracking.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCustomerOemProductModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedDate",
                table: "CustomerOemProducts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "CustomerOemProducts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CustomerOemProducts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "CustomerOemProducts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "CustomerOemProducts",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedDate",
                table: "CustomerOemProducts");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CustomerOemProducts");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CustomerOemProducts");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "CustomerOemProducts");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "CustomerOemProducts");
        }
    }
}
