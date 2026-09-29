using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddMasterNursingDiagnosisSdki : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstNursingDiagnosisGroup",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    GroupName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_MstNursingDiagnosisGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstNursingDiagnosis",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    SubCategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Definition = table.Column<string>(type: "text", nullable: true),
                    TerminologySystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "SDKI"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_MstNursingDiagnosis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstNursingDiagnosis_MstNursingDiagnosisGroup_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "public",
                        principalTable: "MstNursingDiagnosisGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MstNursingDiagnosisEtiology",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NursingDiagnosisId = table.Column<Guid>(type: "uuid", nullable: false),
                    EtiologyName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_MstNursingDiagnosisEtiology", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstNursingDiagnosisEtiology_MstNursingDiagnosis_NursingDiag~",
                        column: x => x.NursingDiagnosisId,
                        principalSchema: "public",
                        principalTable: "MstNursingDiagnosis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MstNursingDiagnosisIntervention",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NursingDiagnosisId = table.Column<Guid>(type: "uuid", nullable: false),
                    PillarType = table.Column<int>(type: "integer", nullable: false),
                    InterventionCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InterventionName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ActionDescription = table.Column<string>(type: "text", nullable: false),
                    TerminologySystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "SIKI"),
                    IsDefaultRecommendation = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_MstNursingDiagnosisIntervention", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstNursingDiagnosisIntervention_MstNursingDiagnosis_Nursing~",
                        column: x => x.NursingDiagnosisId,
                        principalSchema: "public",
                        principalTable: "MstNursingDiagnosis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MstNursingDiagnosisOutcome",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NursingDiagnosisId = table.Column<Guid>(type: "uuid", nullable: false),
                    OutcomeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OutcomeName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Expectation = table.Column<string>(type: "text", nullable: true),
                    TerminologySystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "SLKI"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_MstNursingDiagnosisOutcome", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstNursingDiagnosisOutcome_MstNursingDiagnosis_NursingDiagn~",
                        column: x => x.NursingDiagnosisId,
                        principalSchema: "public",
                        principalTable: "MstNursingDiagnosis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstNursingDiagnosis_Code",
                schema: "public",
                table: "MstNursingDiagnosis",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstNursingDiagnosis_GroupId",
                schema: "public",
                table: "MstNursingDiagnosis",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_MstNursingDiagnosis_Name",
                schema: "public",
                table: "MstNursingDiagnosis",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_MstNursingDiagnosisEtiology_NursingDiagnosisId",
                schema: "public",
                table: "MstNursingDiagnosisEtiology",
                column: "NursingDiagnosisId");

            migrationBuilder.CreateIndex(
                name: "IX_MstNursingDiagnosisGroup_GroupCode",
                schema: "public",
                table: "MstNursingDiagnosisGroup",
                column: "GroupCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstNursingDiagnosisIntervention_NursingDiagnosisId",
                schema: "public",
                table: "MstNursingDiagnosisIntervention",
                column: "NursingDiagnosisId");

            migrationBuilder.CreateIndex(
                name: "IX_MstNursingDiagnosisIntervention_PillarType",
                schema: "public",
                table: "MstNursingDiagnosisIntervention",
                column: "PillarType");

            migrationBuilder.CreateIndex(
                name: "IX_MstNursingDiagnosisOutcome_NursingDiagnosisId",
                schema: "public",
                table: "MstNursingDiagnosisOutcome",
                column: "NursingDiagnosisId");

            migrationBuilder.CreateIndex(
                name: "IX_MstNursingDiagnosisOutcome_OutcomeCode",
                schema: "public",
                table: "MstNursingDiagnosisOutcome",
                column: "OutcomeCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstNursingDiagnosisEtiology",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstNursingDiagnosisIntervention",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstNursingDiagnosisOutcome",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstNursingDiagnosis",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstNursingDiagnosisGroup",
                schema: "public");
        }
    }
}
