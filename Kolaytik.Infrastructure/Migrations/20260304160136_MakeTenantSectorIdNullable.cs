using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kolaytik.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeTenantSectorIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_Sectors_SectorId",
                table: "Tenants");

            migrationBuilder.AlterColumn<Guid>(
                name: "SectorId",
                table: "Tenants",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_Sectors_SectorId",
                table: "Tenants",
                column: "SectorId",
                principalTable: "Sectors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_Sectors_SectorId",
                table: "Tenants");

            migrationBuilder.AlterColumn<Guid>(
                name: "SectorId",
                table: "Tenants",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_Sectors_SectorId",
                table: "Tenants",
                column: "SectorId",
                principalTable: "Sectors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
