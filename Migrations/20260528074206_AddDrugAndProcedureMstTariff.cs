using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class AddDrugAndProcedureMstTariff : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DrugId",
                schema: "public",
                table: "MstTariff",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProcedureId",
                schema: "public",
                table: "MstTariff",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstTariff_DrugId",
                schema: "public",
                table: "MstTariff",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_MstTariff_DrugId_PatientClassId_IsActive_IsDelete",
                schema: "public",
                table: "MstTariff",
                columns: new[] { "DrugId", "PatientClassId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstTariff_ProcedureId",
                schema: "public",
                table: "MstTariff",
                column: "ProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_MstTariff_ProcedureId_PatientClassId_IsActive_IsDelete",
                schema: "public",
                table: "MstTariff",
                columns: new[] { "ProcedureId", "PatientClassId", "IsActive", "IsDelete" });

            migrationBuilder.AddForeignKey(
                name: "FK_MstTariff_MstDrug_DrugId",
                schema: "public",
                table: "MstTariff",
                column: "DrugId",
                principalSchema: "public",
                principalTable: "MstDrug",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstTariff_MstProcedure_ProcedureId",
                schema: "public",
                table: "MstTariff",
                column: "ProcedureId",
                principalSchema: "public",
                principalTable: "MstProcedure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstTariff_MstDrug_DrugId",
                schema: "public",
                table: "MstTariff");

            migrationBuilder.DropForeignKey(
                name: "FK_MstTariff_MstProcedure_ProcedureId",
                schema: "public",
                table: "MstTariff");

            migrationBuilder.DropIndex(
                name: "IX_MstTariff_DrugId",
                schema: "public",
                table: "MstTariff");

            migrationBuilder.DropIndex(
                name: "IX_MstTariff_DrugId_PatientClassId_IsActive_IsDelete",
                schema: "public",
                table: "MstTariff");

            migrationBuilder.DropIndex(
                name: "IX_MstTariff_ProcedureId",
                schema: "public",
                table: "MstTariff");

            migrationBuilder.DropIndex(
                name: "IX_MstTariff_ProcedureId_PatientClassId_IsActive_IsDelete",
                schema: "public",
                table: "MstTariff");

            migrationBuilder.DropColumn(
                name: "DrugId",
                schema: "public",
                table: "MstTariff");

            migrationBuilder.DropColumn(
                name: "ProcedureId",
                schema: "public",
                table: "MstTariff");
        }
    }
}
