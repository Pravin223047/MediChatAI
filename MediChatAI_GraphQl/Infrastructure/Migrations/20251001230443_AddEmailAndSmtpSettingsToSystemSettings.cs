using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediChatAI_GraphQl.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailAndSmtpSettingsToSystemSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailBackgroundColor",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmailFacebookUrl",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailFooterText",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmailHeaderImageUrl",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "EmailIncludeFooter",
                table: "SystemSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EmailIncludeSocialLinks",
                table: "SystemSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EmailInstagramUrl",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailLinkedInUrl",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailPrimaryColor",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmailSecondaryColor",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmailTemplateTheme",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmailTextColor",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmailTwitterUrl",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableEmailNotifications",
                table: "SystemSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyAdminOnNewDoctor",
                table: "SystemSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyDoctorOnApproval",
                table: "SystemSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyUserOn2FAChange",
                table: "SystemSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyUserOnLogin",
                table: "SystemSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyUserOnPasswordChange",
                table: "SystemSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SmtpEnableSsl",
                table: "SystemSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SmtpFromEmail",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SmtpFromName",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "SmtpIsConfigured",
                table: "SystemSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SmtpLastTestMessage",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SmtpLastTestSuccessful",
                table: "SystemSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SmtpLastTestedAt",
                table: "SystemSettings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpPassword",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SmtpPort",
                table: "SystemSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SmtpServer",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SmtpUsername",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailBackgroundColor",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "EmailFacebookUrl",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "EmailFooterText",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "EmailHeaderImageUrl",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "EmailIncludeFooter",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "EmailIncludeSocialLinks",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "EmailInstagramUrl",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "EmailLinkedInUrl",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "EmailPrimaryColor",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "EmailSecondaryColor",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "EmailTemplateTheme",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "EmailTextColor",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "EmailTwitterUrl",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "EnableEmailNotifications",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "NotifyAdminOnNewDoctor",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "NotifyDoctorOnApproval",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "NotifyUserOn2FAChange",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "NotifyUserOnLogin",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "NotifyUserOnPasswordChange",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "SmtpEnableSsl",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "SmtpFromEmail",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "SmtpFromName",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "SmtpIsConfigured",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "SmtpLastTestMessage",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "SmtpLastTestSuccessful",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "SmtpLastTestedAt",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "SmtpPassword",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "SmtpPort",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "SmtpServer",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "SmtpUsername",
                table: "SystemSettings");
        }
    }
}
