using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddBbkBloodBankProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BbkBloodBankProcedure",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcedureNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BloodOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    BdrsDoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcedureRefId = table.Column<Guid>(type: "uuid", nullable: false),
                    TariffId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcedureCodeSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProcedureNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TariffAmountSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ProcedureStatus = table.Column<int>(type: "integer", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BbkBloodBankProcedure", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BbkBloodBankProcedure_BbkBloodOrder_BloodOrderId",
                        column: x => x.BloodOrderId,
                        principalSchema: "public",
                        principalTable: "BbkBloodOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BbkBloodBankProcedure_MstDoctor_BdrsDoctorId",
                        column: x => x.BdrsDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BbkBloodBankProcedure_MstPatientClass_PatientClassId",
                        column: x => x.PatientClassId,
                        principalSchema: "public",
                        principalTable: "MstPatientClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BbkBloodBankProcedure_MstProcedure_ProcedureRefId",
                        column: x => x.ProcedureRefId,
                        principalSchema: "public",
                        principalTable: "MstProcedure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BbkBloodBankProcedure_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BbkBloodBankProcedure_MstTariff_TariffId",
                        column: x => x.TariffId,
                        principalSchema: "public",
                        principalTable: "MstTariff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodBankProcedure_BdrsDoctorId",
                schema: "public",
                table: "BbkBloodBankProcedure",
                column: "BdrsDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodBankProcedure_BloodOrderId",
                schema: "public",
                table: "BbkBloodBankProcedure",
                column: "BloodOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodBankProcedure_PatientClassId",
                schema: "public",
                table: "BbkBloodBankProcedure",
                column: "PatientClassId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodBankProcedure_ProcedureNumber",
                schema: "public",
                table: "BbkBloodBankProcedure",
                column: "ProcedureNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodBankProcedure_ProcedureRefId",
                schema: "public",
                table: "BbkBloodBankProcedure",
                column: "ProcedureRefId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodBankProcedure_ProcedureStatus",
                schema: "public",
                table: "BbkBloodBankProcedure",
                column: "ProcedureStatus");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodBankProcedure_ServiceUnitId",
                schema: "public",
                table: "BbkBloodBankProcedure",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodBankProcedure_TariffId",
                schema: "public",
                table: "BbkBloodBankProcedure",
                column: "TariffId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BbkBloodBankProcedure",
                schema: "public");
        }
    }
}
