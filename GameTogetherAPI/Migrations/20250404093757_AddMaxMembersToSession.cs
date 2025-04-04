using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameTogetherAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddMaxMembersToSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxMembers",
                table: "Sessions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxMembers",
                table: "Sessions");
        }
    }
}
