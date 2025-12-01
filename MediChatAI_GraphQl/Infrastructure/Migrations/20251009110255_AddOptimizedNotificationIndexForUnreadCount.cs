using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediChatAI_GraphQl.Migrations
{
    /// <inheritdoc />
    public partial class AddOptimizedNotificationIndexForUnreadCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead_ExpiresAt",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead", "ExpiresAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_IsRead_ExpiresAt",
                table: "Notifications");
        }
    }
}
