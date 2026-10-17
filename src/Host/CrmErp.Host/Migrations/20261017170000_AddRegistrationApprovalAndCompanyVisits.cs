using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmErp.Host.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261017170000_AddRegistrationApprovalAndCompanyVisits")]
public partial class AddRegistrationApprovalAndCompanyVisits : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
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
            name: "user_approvals",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                ReviewedBy = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_user_approvals", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_user_approvals_UserId",
            table: "user_approvals",
            column: "UserId",
            unique: true);

        migrationBuilder.CreateTable(
            name: "company_visits",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                SellerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                VisitedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                Notes = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                Outcome = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                FollowUpAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                Latitude = table.Column<double>(type: "double precision", nullable: true),
                Longitude = table.Column<double>(type: "double precision", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_company_visits", x => x.Id);
                table.ForeignKey(
                    name: "FK_company_visits_companies_CompanyId",
                    column: x => x.CompanyId,
                    principalTable: "companies",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_company_visits_CompanyId_VisitedAt",
            table: "company_visits",
            columns: new[] { "CompanyId", "VisitedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_company_visits_SellerUserId_VisitedAt",
            table: "company_visits",
            columns: new[] { "SellerUserId", "VisitedAt" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "company_visits");
        migrationBuilder.DropTable(name: "user_approvals");
        migrationBuilder.DropColumn(name: "Latitude", table: "companies");
        migrationBuilder.DropColumn(name: "Longitude", table: "companies");
    }
}
