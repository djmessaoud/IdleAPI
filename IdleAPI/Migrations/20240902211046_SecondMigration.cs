using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleAPI.Migrations
{
    /// <inheritdoc />
    public partial class SecondMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "User_id",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Access_token",
                table: "Users",
                column: "Access_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_User_id",
                table: "Users",
                column: "User_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Access_token",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_User_id",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "User_id",
                table: "Users");
        }
    }
}
