using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediChatAI_GraphQl.Migrations
{
    /// <inheritdoc />
    public partial class AddAppearanceSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ThemeAccentColor",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThemeBackgroundColor",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThemeBorderRadius",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ThemeCompactMode",
                table: "SystemSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ThemeEnableAnimations",
                table: "SystemSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ThemeFontSize",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ThemeHighContrast",
                table: "SystemSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ThemeMode",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThemePreset",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThemePrimaryColor",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThemeSidebarStyle",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThemeTextColor",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThemeAccentColor",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "ThemeBackgroundColor",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "ThemeBorderRadius",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "ThemeCompactMode",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "ThemeEnableAnimations",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "ThemeFontSize",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "ThemeHighContrast",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "ThemeMode",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "ThemePreset",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "ThemePrimaryColor",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "ThemeSidebarStyle",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "ThemeTextColor",
                table: "SystemSettings");
        }
    }
}
