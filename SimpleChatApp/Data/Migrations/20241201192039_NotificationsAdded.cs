using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleChatApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class NotificationsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Nickname",
                table: "Profiles",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "InviteNotifications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TargetId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SourceUserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChatRoomName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InviteNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InviteNotifications_AspNetUsers_TargetId",
                        column: x => x.TargetId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_Nickname",
                table: "Profiles",
                column: "Nickname",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InviteNotifications_TargetId",
                table: "InviteNotifications",
                column: "TargetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InviteNotifications");

            migrationBuilder.DropIndex(
                name: "IX_Profiles_Nickname",
                table: "Profiles");

            migrationBuilder.AlterColumn<string>(
                name: "Nickname",
                table: "Profiles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
