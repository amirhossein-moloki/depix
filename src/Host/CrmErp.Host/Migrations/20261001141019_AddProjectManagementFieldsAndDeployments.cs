using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmErp.Host.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectManagementFieldsAndDeployments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deployment_projects_ProjectId",
                table: "Deployment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Deployment",
                table: "Deployment");

            migrationBuilder.RenameTable(
                name: "Deployment",
                newName: "deployments");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "proposals",
                newName: "total_amount");

            migrationBuilder.RenameColumn(
                name: "Discount",
                table: "proposals",
                newName: "subtotal_amount");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "proposal_items",
                newName: "unit_price_amount");

            migrationBuilder.RenameColumn(
                name: "DeliveryDate",
                table: "projects",
                newName: "PlannedDeliveryDate");

            migrationBuilder.RenameIndex(
                name: "IX_Deployment_ProjectId",
                table: "deployments",
                newName: "IX_deployments_ProjectId");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "repositories",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "repositories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "proposals",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "proposals",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "proposals",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "proposals",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "proposals",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "proposals",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "proposals",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "proposals",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "proposals",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "discount_amount",
                table: "proposals",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "discount_currency",
                table: "proposals",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "subtotal_currency",
                table: "proposals",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "total_currency",
                table: "proposals",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "proposal_items",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "proposal_items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "discount_amount",
                table: "proposal_items",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "discount_currency",
                table: "proposal_items",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "total_amount",
                table: "proposal_items",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "total_currency",
                table: "proposal_items",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "unit_price_currency",
                table: "proposal_items",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateOnly>(
                name: "ActualDeliveryDate",
                table: "projects",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "projects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ContactId",
                table: "projects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "projects",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "projects",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OpportunityId",
                table: "projects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProposalId",
                table: "projects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedTo",
                table: "customers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerNumber",
                table: "customers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "customers",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PrimaryContactId",
                table: "customers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "customers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                table: "deployments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "SslStatus",
                table: "deployments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Server",
                table: "deployments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Provider",
                table: "deployments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Domain",
                table: "deployments",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Environment",
                table: "deployments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "deployments",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_deployments",
                table: "deployments",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_proposals_CompanyId",
                table: "proposals",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_proposals_CustomerId",
                table: "proposals",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_proposals_Status",
                table: "proposals",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_projects_CompanyId",
                table: "projects",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_projects_CreatedAt",
                table: "projects",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_projects_CustomerId",
                table: "projects",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_projects_OpportunityId",
                table: "projects",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_projects_PlannedDeliveryDate",
                table: "projects",
                column: "PlannedDeliveryDate");

            migrationBuilder.CreateIndex(
                name: "IX_projects_ProposalId",
                table: "projects",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_customers_AssignedTo",
                table: "customers",
                column: "AssignedTo");

            migrationBuilder.CreateIndex(
                name: "IX_customers_CustomerNumber",
                table: "customers",
                column: "CustomerNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customers_PrimaryContactId",
                table: "customers",
                column: "PrimaryContactId");

            migrationBuilder.CreateIndex(
                name: "IX_customers_Status",
                table: "customers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_deployments_Domain",
                table: "deployments",
                column: "Domain");

            migrationBuilder.CreateIndex(
                name: "IX_deployments_Environment",
                table: "deployments",
                column: "Environment");

            migrationBuilder.AddForeignKey(
                name: "FK_deployments_projects_ProjectId",
                table: "deployments",
                column: "ProjectId",
                principalTable: "projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_deployments_projects_ProjectId",
                table: "deployments");

            migrationBuilder.DropIndex(
                name: "IX_proposals_CompanyId",
                table: "proposals");

            migrationBuilder.DropIndex(
                name: "IX_proposals_CustomerId",
                table: "proposals");

            migrationBuilder.DropIndex(
                name: "IX_proposals_Status",
                table: "proposals");

            migrationBuilder.DropIndex(
                name: "IX_projects_CompanyId",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "IX_projects_CreatedAt",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "IX_projects_CustomerId",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "IX_projects_OpportunityId",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "IX_projects_PlannedDeliveryDate",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "IX_projects_ProposalId",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "IX_customers_AssignedTo",
                table: "customers");

            migrationBuilder.DropIndex(
                name: "IX_customers_CustomerNumber",
                table: "customers");

            migrationBuilder.DropIndex(
                name: "IX_customers_PrimaryContactId",
                table: "customers");

            migrationBuilder.DropIndex(
                name: "IX_customers_Status",
                table: "customers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_deployments",
                table: "deployments");

            migrationBuilder.DropIndex(
                name: "IX_deployments_Domain",
                table: "deployments");

            migrationBuilder.DropIndex(
                name: "IX_deployments_Environment",
                table: "deployments");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "repositories");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "repositories");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "proposals");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "proposals");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "proposals");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "proposals");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "proposals");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "proposals");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "proposals");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "proposals");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "proposals");

            migrationBuilder.DropColumn(
                name: "discount_amount",
                table: "proposals");

            migrationBuilder.DropColumn(
                name: "discount_currency",
                table: "proposals");

            migrationBuilder.DropColumn(
                name: "subtotal_currency",
                table: "proposals");

            migrationBuilder.DropColumn(
                name: "total_currency",
                table: "proposals");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "proposal_items");

            migrationBuilder.DropColumn(
                name: "discount_amount",
                table: "proposal_items");

            migrationBuilder.DropColumn(
                name: "discount_currency",
                table: "proposal_items");

            migrationBuilder.DropColumn(
                name: "total_amount",
                table: "proposal_items");

            migrationBuilder.DropColumn(
                name: "total_currency",
                table: "proposal_items");

            migrationBuilder.DropColumn(
                name: "unit_price_currency",
                table: "proposal_items");

            migrationBuilder.DropColumn(
                name: "ActualDeliveryDate",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "OpportunityId",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "ProposalId",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "AssignedTo",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "CustomerNumber",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "PrimaryContactId",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "Environment",
                table: "deployments");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "deployments");

            migrationBuilder.RenameTable(
                name: "deployments",
                newName: "Deployment");

            migrationBuilder.RenameColumn(
                name: "total_amount",
                table: "proposals",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "subtotal_amount",
                table: "proposals",
                newName: "Discount");

            migrationBuilder.RenameColumn(
                name: "unit_price_amount",
                table: "proposal_items",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "PlannedDeliveryDate",
                table: "projects",
                newName: "DeliveryDate");

            migrationBuilder.RenameIndex(
                name: "IX_deployments_ProjectId",
                table: "Deployment",
                newName: "IX_Deployment_ProjectId");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "proposal_items",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                table: "Deployment",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "SslStatus",
                table: "Deployment",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Server",
                table: "Deployment",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Provider",
                table: "Deployment",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Domain",
                table: "Deployment",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Deployment",
                table: "Deployment",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Deployment_projects_ProjectId",
                table: "Deployment",
                column: "ProjectId",
                principalTable: "projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
