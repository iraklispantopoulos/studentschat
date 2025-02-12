using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class ChatHistoryUserIdCorrection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatHistory_Users_UserId1",
                table: "ChatHistory");

            migrationBuilder.DropIndex(
                name: "IX_ChatHistory_UserId1",
                table: "ChatHistory");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "ChatHistory");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "ChatHistory",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_ChatHistory_UserId",
                table: "ChatHistory",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatHistory_Users_UserId",
                table: "ChatHistory",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatHistory_Users_UserId",
                table: "ChatHistory");

            migrationBuilder.DropIndex(
                name: "IX_ChatHistory_UserId",
                table: "ChatHistory");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ChatHistory",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "ChatHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ChatHistory_UserId1",
                table: "ChatHistory",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatHistory_Users_UserId1",
                table: "ChatHistory",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
