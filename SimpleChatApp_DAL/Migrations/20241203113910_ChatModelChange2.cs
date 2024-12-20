using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleChatApp_DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChatModelChange2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_Name",
                table: "ChatRooms",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChatRooms_Name",
                table: "ChatRooms");
        }
    }
}
