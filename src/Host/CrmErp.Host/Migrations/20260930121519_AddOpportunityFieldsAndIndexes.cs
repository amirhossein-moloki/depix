using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmErp.Host.Migrations
{
    /// <inheritdoc />
    public partial class AddOpportunityFieldsAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EstimatedValue",
                table: "opportunities",
                newName: "estimated_value");

            migrationBuilder.AlterColumn<Guid>(
                name: "LeadId",
                table: "opportunities",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedTo",
                table: "opportunities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "opportunities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ContactId",
                table: "opportunities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "opportunities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "opportunities",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "opportunities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "opportunities",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "opportunities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LossReason",
                table: "opportunities",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LostAt",
                table: "opportunities",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "opportunities",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "opportunities",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "opportunities",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "WonAt",
                table: "opportunities",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "value_currency",
                table: "opportunities",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.AddColumn<DateTime>(
                name: "ConvertedAt",
                table: "leads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "leads",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_opportunities_AssignedTo",
                table: "opportunities",
                column: "AssignedTo");

            migrationBuilder.CreateIndex(
                name: "IX_opportunities_CompanyId",
                table: "opportunities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_opportunities_CreatedAt",
                table: "opportunities",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_opportunities_CustomerId",
                table: "opportunities",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_opportunities_ExpectedCloseDate",
                table: "opportunities",
                column: "ExpectedCloseDate");

            migrationBuilder.CreateIndex(
                name: "IX_opportunities_LeadId",
                table: "opportunities",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_opportunities_Status",
                table: "opportunities",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_opportunities_AssignedTo",
                table: "opportunities");

            migrationBuilder.DropIndex(
                name: "IX_opportunities_CompanyId",
                table: "opportunities");

            migrationBuilder.DropIndex(
                name: "IX_opportunities_CreatedAt",
                table: "opportunities");

            migrationBuilder.DropIndex(
                name: "IX_opportunities_CustomerId",
                table: "opportunities");

            migrationBuilder.DropIndex(
                name: "IX_opportunities_ExpectedCloseDate",
                table: "opportunities");

            migrationBuilder.DropIndex(
                name: "IX_opportunities_LeadId",
                table: "opportunities");

            migrationBuilder.DropIndex(
                name: "IX_opportunities_Status",
                table: "opportunities");

            migrationBuilder.DropColumn(
                name: "AssignedTo",
                table: "opportunities");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "opportunities");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "opportunities");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "opportunities");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "opportunities");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "opportunities");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "opportunities");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "opportunities");

            migrationBuilder.DropColumn(
                name: "LossReason",
                table: "opportunities");

            migrationBuilder.DropColumn(
                name: "LostAt",
                table: "opportunities");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "opportunities");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "opportunities");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "opportunities");

            migrationBuilder.DropColumn(
                name: "WonAt",
                table: "opportunities");

            migrationBuilder.DropColumn(
                name: "value_currency",
                table: "opportunities");

            migrationBuilder.DropColumn(
                name: "ConvertedAt",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "leads");

            migrationBuilder.RenameColumn(
                name: "estimated_value",
                table: "opportunities",
                newName: "EstimatedValue");

            migrationBuilder.AlterColumn<Guid>(
                name: "LeadId",
                table: "opportunities",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
