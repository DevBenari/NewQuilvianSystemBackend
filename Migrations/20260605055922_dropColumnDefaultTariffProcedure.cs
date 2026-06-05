using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class dropColumnDefaultTariffProcedure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstProcedure_MstTariff_DefaultTariffId",
                schema: "public",
                table: "MstProcedure");

            migrationBuilder.DropIndex(
                name: "IX_MstProcedure_DefaultTariffId",
                schema: "public",
                table: "MstProcedure");

            migrationBuilder.DropIndex(
                name: "IX_MstProcedure_ProcedureCode",
                schema: "public",
                table: "MstProcedure");

            migrationBuilder.DropColumn(
                name: "DefaultTariffId",
                schema: "public",
                table: "MstProcedure");

            migrationBuilder.CreateIndex(
                name: "IX_MstProcedure_ProcedureCode",
                schema: "public",
                table: "MstProcedure",
                column: "ProcedureCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstProcedure_ProcedureType_IsActive_IsDelete",
                schema: "public",
                table: "MstProcedure",
                columns: new[] { "ProcedureType", "IsActive", "IsDelete" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MstProcedure_ProcedureCode",
                schema: "public",
                table: "MstProcedure");

            migrationBuilder.DropIndex(
                name: "IX_MstProcedure_ProcedureType_IsActive_IsDelete",
                schema: "public",
                table: "MstProcedure");

            migrationBuilder.AddColumn<Guid>(
                name: "DefaultTariffId",
                schema: "public",
                table: "MstProcedure",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstProcedure_DefaultTariffId",
                schema: "public",
                table: "MstProcedure",
                column: "DefaultTariffId");

            migrationBuilder.CreateIndex(
                name: "IX_MstProcedure_ProcedureCode",
                schema: "public",
                table: "MstProcedure",
                column: "ProcedureCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MstProcedure_MstTariff_DefaultTariffId",
                schema: "public",
                table: "MstProcedure",
                column: "DefaultTariffId",
                principalSchema: "public",
                principalTable: "MstTariff",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
