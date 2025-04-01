using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameTogetherAPI.Migrations
{
    /// <inheritdoc />
    public partial class RenameChatTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Sessions_SessionId",
                table: "Chat");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Chat_ChatId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_UserChat_Chat_ChatId",
                table: "UserChat");

            migrationBuilder.DropForeignKey(
                name: "FK_UserChat_Users_UserId",
                table: "UserChat");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserChat",
                table: "UserChat");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat",
                table: "Chat");

            migrationBuilder.RenameTable(
                name: "UserChat",
                newName: "UserChats");

            migrationBuilder.RenameTable(
                name: "Chat",
                newName: "Chats");

            migrationBuilder.RenameIndex(
                name: "IX_UserChat_ChatId",
                table: "UserChats",
                newName: "IX_UserChats_ChatId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_SessionId",
                table: "Chats",
                newName: "IX_Chats_SessionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserChats",
                table: "UserChats",
                columns: new[] { "UserId", "ChatId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chats",
                table: "Chats",
                column: "ChatId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Sessions_SessionId",
                table: "Chats",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Chats_ChatId",
                table: "Messages",
                column: "ChatId",
                principalTable: "Chats",
                principalColumn: "ChatId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserChats_Chats_ChatId",
                table: "UserChats",
                column: "ChatId",
                principalTable: "Chats",
                principalColumn: "ChatId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserChats_Users_UserId",
                table: "UserChats",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Sessions_SessionId",
                table: "Chats");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Chats_ChatId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_UserChats_Chats_ChatId",
                table: "UserChats");

            migrationBuilder.DropForeignKey(
                name: "FK_UserChats_Users_UserId",
                table: "UserChats");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserChats",
                table: "UserChats");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chats",
                table: "Chats");

            migrationBuilder.RenameTable(
                name: "UserChats",
                newName: "UserChat");

            migrationBuilder.RenameTable(
                name: "Chats",
                newName: "Chat");

            migrationBuilder.RenameIndex(
                name: "IX_UserChats_ChatId",
                table: "UserChat",
                newName: "IX_UserChat_ChatId");

            migrationBuilder.RenameIndex(
                name: "IX_Chats_SessionId",
                table: "Chat",
                newName: "IX_Chat_SessionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserChat",
                table: "UserChat",
                columns: new[] { "UserId", "ChatId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat",
                table: "Chat",
                column: "ChatId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Sessions_SessionId",
                table: "Chat",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Chat_ChatId",
                table: "Messages",
                column: "ChatId",
                principalTable: "Chat",
                principalColumn: "ChatId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserChat_Chat_ChatId",
                table: "UserChat",
                column: "ChatId",
                principalTable: "Chat",
                principalColumn: "ChatId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserChat_Users_UserId",
                table: "UserChat",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
