using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace License_Tracking.Migrations
{
    /// <inheritdoc />
    public partial class Week8_AddDealCollaborationActivitiesOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create the new DealCollaborationActivities table
            migrationBuilder.CreateTable(
                name: "DealCollaborationActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DealId = table.Column<int>(type: "int", nullable: false),
                    ActivityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ActivityTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ActivityDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AssignedTo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PerformedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsInternal = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DealCollaborationActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DealCollaborationActivities_Deals_DealId",
                        column: x => x.DealId,
                        principalTable: "Deals",
                        principalColumn: "DealId",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_DealCollaborationActivity_DealId",
                table: "DealCollaborationActivities",
                column: "DealId");

            migrationBuilder.CreateIndex(
                name: "IX_DealCollaborationActivity_CreatedDate",
                table: "DealCollaborationActivities",
                column: "CreatedDate");

            // Add new columns to CbmsInvoices if they don't exist
            migrationBuilder.AddColumn<decimal>(
                name: "PaymentReceived",
                table: "CbmsInvoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reference",
                table: "CbmsInvoices",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the table
            migrationBuilder.DropTable(
                name: "DealCollaborationActivities");

            // Remove columns from CbmsInvoices
            migrationBuilder.DropColumn(
                name: "PaymentReceived",
                table: "CbmsInvoices");

            migrationBuilder.DropColumn(
                name: "Reference",
                table: "CbmsInvoices");
        }
    }
}
