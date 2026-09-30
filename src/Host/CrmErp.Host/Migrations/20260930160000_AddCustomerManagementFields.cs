using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmErp.Host.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerManagementFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerNumber",
                table: "customers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "customers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "ACTIVE");

            migrationBuilder.AddColumn<Guid>(
                name: "PrimaryContactId",
                table: "customers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedTo",
                table: "customers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "customers",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_customers_CustomerNumber",
                table: "customers",
                column: "CustomerNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customers_Status",
                table: "customers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_customers_PrimaryContactId",
                table: "customers",
                column: "PrimaryContactId");

            migrationBuilder.CreateIndex(
                name: "IX_customers_AssignedTo",
                table: "customers",
                column: "AssignedTo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_customers_CustomerNumber",
                table: "customers");

            migrationBuilder.DropIndex(
                name: "IX_customers_Status",
                table: "customers");

            migrationBuilder.DropIndex(
                name: "IX_customers_PrimaryContactId",
                table: "customers");

            migrationBuilder.DropIndex(
                name: "IX_customers_AssignedTo",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "CustomerNumber",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "PrimaryContactId",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "AssignedTo",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "customers");
        }
    }
}
