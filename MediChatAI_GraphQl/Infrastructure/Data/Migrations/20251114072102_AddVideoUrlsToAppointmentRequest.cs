using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediChatAI_GraphQl.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoUrlsToAppointmentRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VideoUrls",
                table: "AppointmentRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoUrls",
                table: "AppointmentRequests");
        }
    }
}
