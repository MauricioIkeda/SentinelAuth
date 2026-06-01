using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SentinelAuth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationClientSecrets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowRoleAssignment",
                table: "application_clients",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ClientSecretHash",
                table: "application_clients",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowRoleAssignment",
                table: "application_clients");

            migrationBuilder.DropColumn(
                name: "ClientSecretHash",
                table: "application_clients");
        }
    }
}
