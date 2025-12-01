using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediChatAI_GraphQl.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiModalUrlsToAppointmentRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InsuranceCardUrls",
                table: "AppointmentRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicalDocumentUrls",
                table: "AppointmentRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrls",
                table: "AppointmentRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InsuranceCardUrls",
                table: "AppointmentRequests");

            migrationBuilder.DropColumn(
                name: "MedicalDocumentUrls",
                table: "AppointmentRequests");

            migrationBuilder.DropColumn(
                name: "PhotoUrls",
                table: "AppointmentRequests");
        }
    }
}
