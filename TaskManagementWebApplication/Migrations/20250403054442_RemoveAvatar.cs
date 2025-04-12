using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagementWebApplication.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAvatar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Avatar",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Avatar",
                table: "Users",
                type: "varbinary(max)",
                nullable: true);
        }
    }
}
