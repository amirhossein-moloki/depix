using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmErp.Host.Migrations
{
    /// <inheritdoc />
    public partial class AddUserApprovalAndSalesVisitAndGeoLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedBy",
                table: "users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "companies",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "companies",
                type: "double precision",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "sales_visits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    SalespersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    VisitedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReportText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Result = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FollowUpAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    LocationSource = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sales_visits", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_sales_visits_CompanyId",
                table: "sales_visits",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_sales_visits_SalespersonId",
                table: "sales_visits",
                column: "SalespersonId");

            migrationBuilder.CreateIndex(
                name: "IX_sales_visits_VisitedAt",
                table: "sales_visits",
                column: "VisitedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sales_visits");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "users");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "users");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "companies");
        }
    }
}
