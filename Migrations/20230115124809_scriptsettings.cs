using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AutoSiem.Migrations
{
    public partial class scriptsettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Siems",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "Siems",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SettingsId",
                table: "Platforms",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PlatformId",
                table: "Nodes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ScriptSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OperatingSystem = table.Column<int>(type: "INTEGER", nullable: false),
                    isSystemLogs = table.Column<bool>(type: "INTEGER", nullable: false),
                    CustomLogPaths = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScriptSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Platforms_SettingsId",
                table: "Platforms",
                column: "SettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_PlatformId",
                table: "Nodes",
                column: "PlatformId");

            migrationBuilder.AddForeignKey(
                name: "FK_Nodes_Platforms_PlatformId",
                table: "Nodes",
                column: "PlatformId",
                principalTable: "Platforms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Platforms_ScriptSettings_SettingsId",
                table: "Platforms",
                column: "SettingsId",
                principalTable: "ScriptSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Nodes_Platforms_PlatformId",
                table: "Nodes");

            migrationBuilder.DropForeignKey(
                name: "FK_Platforms_ScriptSettings_SettingsId",
                table: "Platforms");

            migrationBuilder.DropTable(
                name: "ScriptSettings");

            migrationBuilder.DropIndex(
                name: "IX_Platforms_SettingsId",
                table: "Platforms");

            migrationBuilder.DropIndex(
                name: "IX_Nodes_PlatformId",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "SettingsId",
                table: "Platforms");

            migrationBuilder.DropColumn(
                name: "PlatformId",
                table: "Nodes");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Siems",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "Siems",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }
    }
}
