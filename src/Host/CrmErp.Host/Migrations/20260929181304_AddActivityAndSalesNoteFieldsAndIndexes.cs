using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmErp.Host.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityAndSalesNoteFieldsAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "users");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Strategy",
                table: "sales_notes",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Objections",
                table: "sales_notes",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "NeedAnalysis",
                table: "sales_notes",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedBy",
                table: "sales_notes",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "BudgetInformation",
                table: "sales_notes",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "sales_notes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompetitorsMentioned",
                table: "sales_notes",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ContactId",
                table: "sales_notes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "sales_notes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DecisionMakerInfo",
                table: "sales_notes",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "sales_notes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "sales_notes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "sales_notes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "sales_notes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "sales_notes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "sales_notes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "roles",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ContactId",
                table: "leads",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "leads",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DisqualificationReason",
                table: "leads",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedValue",
                table: "leads",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "leads",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "contacts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "contacts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "contacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "contacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "contacts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "contacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Result",
                table: "activities",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "activities",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "activities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ContactId",
                table: "activities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "activities",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "activities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FollowUpAt",
                table: "activities",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FollowUpNotes",
                table: "activities",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "activities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "OccurredAt",
                table: "activities",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "activities",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByIp = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_role_permissions_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_sales_notes_CompanyId",
                table: "sales_notes",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_sales_notes_ContactId",
                table: "sales_notes",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_sales_notes_CreatedAt",
                table: "sales_notes",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_leads_AssignedTo",
                table: "leads",
                column: "AssignedTo");

            migrationBuilder.CreateIndex(
                name: "IX_leads_ContactId",
                table: "leads",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_leads_CreatedAt",
                table: "leads",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_activities_CompanyId",
                table: "activities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_activities_ContactId",
                table: "activities",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_activities_CreatedAt",
                table: "activities",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_activities_OccurredAt",
                table: "activities",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_activities_Type",
                table: "activities",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_permissions_Code",
                table: "permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_TokenHash",
                table: "refresh_tokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_UserId",
                table: "refresh_tokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_RoleId_PermissionId",
                table: "role_permissions",
                columns: new[] { "RoleId", "PermissionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropIndex(
                name: "IX_sales_notes_CompanyId",
                table: "sales_notes");

            migrationBuilder.DropIndex(
                name: "IX_sales_notes_ContactId",
                table: "sales_notes");

            migrationBuilder.DropIndex(
                name: "IX_sales_notes_CreatedAt",
                table: "sales_notes");

            migrationBuilder.DropIndex(
                name: "IX_leads_AssignedTo",
                table: "leads");

            migrationBuilder.DropIndex(
                name: "IX_leads_ContactId",
                table: "leads");

            migrationBuilder.DropIndex(
                name: "IX_leads_CreatedAt",
                table: "leads");

            migrationBuilder.DropIndex(
                name: "IX_activities_CompanyId",
                table: "activities");

            migrationBuilder.DropIndex(
                name: "IX_activities_ContactId",
                table: "activities");

            migrationBuilder.DropIndex(
                name: "IX_activities_CreatedAt",
                table: "activities");

            migrationBuilder.DropIndex(
                name: "IX_activities_OccurredAt",
                table: "activities");

            migrationBuilder.DropIndex(
                name: "IX_activities_Type",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "users");

            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                table: "users");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "users");

            migrationBuilder.DropColumn(
                name: "BudgetInformation",
                table: "sales_notes");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "sales_notes");

            migrationBuilder.DropColumn(
                name: "CompetitorsMentioned",
                table: "sales_notes");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "sales_notes");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "sales_notes");

            migrationBuilder.DropColumn(
                name: "DecisionMakerInfo",
                table: "sales_notes");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "sales_notes");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "sales_notes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "sales_notes");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "sales_notes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "sales_notes");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "sales_notes");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "roles");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "DisqualificationReason",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "EstimatedValue",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "contacts");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "contacts");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "contacts");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "contacts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "contacts");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "contacts");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "FollowUpAt",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "FollowUpNotes",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "OccurredAt",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "activities");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Strategy",
                table: "sales_notes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "Objections",
                table: "sales_notes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "NeedAnalysis",
                table: "sales_notes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedBy",
                table: "sales_notes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Result",
                table: "activities",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "activities",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);
        }
    }
}
