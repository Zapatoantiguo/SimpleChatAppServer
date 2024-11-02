using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleChatApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class ModelsCorrecting1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_AspNetUsers_UserId",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "ChatRoomName",
                table: "ChatRooms",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "ChatRoomDescription",
                table: "ChatRooms",
                newName: "Description");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_AspNetUsers_UserId",
                table: "Messages",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_AspNetUsers_UserId",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "ChatRooms",
                newName: "ChatRoomName");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "ChatRooms",
                newName: "ChatRoomDescription");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_AspNetUsers_UserId",
                table: "Messages",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
