using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestCarBot.Migrations
{
    /// <inheritdoc />
    public partial class AddUserState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDataConfirmed",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsPolicyIssued",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                table: "Users");

            migrationBuilder.AddColumn<bool>(
                name: "IsDataConfirmed",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPolicyIssued",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
