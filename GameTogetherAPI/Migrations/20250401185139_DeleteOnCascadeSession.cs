using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameTogetherAPI.Migrations
{
    /// <inheritdoc />
    public partial class DeleteOnCascadeSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Sessions_SessionId",
                table: "Chats");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Sessions_SessionId",
                table: "Chats",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Sessions_SessionId",
                table: "Chats");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Sessions_SessionId",
                table: "Chats",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id");
        }
    }
}
