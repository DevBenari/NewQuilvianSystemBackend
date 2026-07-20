using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class changePatientProcedureSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrxPatientProcedure_ConsultationId_ProcedureId_IsDelete",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.CreateIndex(
                name: "UX_TrxPatientProcedure_Consultation_Procedure_Active",
                schema: "public",
                table: "TrxPatientProcedure",
                columns: new[] { "ConsultationId", "ProcedureId" },
                unique: true,
                filter: "\"IsDelete\" = FALSE AND \"IsActive\" = TRUE");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_TrxPatientProcedure_Consultation_Procedure_Active",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_ConsultationId_ProcedureId_IsDelete",
                schema: "public",
                table: "TrxPatientProcedure",
                columns: new[] { "ConsultationId", "ProcedureId", "IsDelete" });
        }
    }
}
