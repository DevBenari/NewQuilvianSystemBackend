using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddEmergencyAdmissionRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequiresAdmissionRequest",
                schema: "public",
                table: "EmgDispositionType",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "EmgAdmissionRequest",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmergencyVisitId = table.Column<Guid>(type: "uuid", nullable: false),
                    DispositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    RequestedServiceUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedPatientClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProposedInpatientDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    EstimatedLengthOfStayDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    IsCito = table.Column<bool>(type: "boolean", nullable: false),
                    NeedsIntensiveCare = table.Column<bool>(type: "boolean", nullable: false),
                    NeedsIsolation = table.Column<bool>(type: "boolean", nullable: false),
                    NeedsOxygenSupport = table.Column<bool>(type: "boolean", nullable: false),
                    NeedsSpecialMonitoring = table.Column<bool>(type: "boolean", nullable: false),
                    ClinicalIndication = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CarePlanInstruction = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SignatureRequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SignatureRequestedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SignedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SignedByDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    SignerNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SignerPositionSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SignatureDeviceInfo = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    SignatureIpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_EmgAdmissionRequest", x => x.Id);
                    table.CheckConstraint("CK_EmgAdmissionRequest_Los", "\"EstimatedLengthOfStayDays\" BETWEEN 1 AND 90");
                    table.CheckConstraint("CK_EmgAdmissionRequest_State", "(\"Status\" <> 3 OR (\"SignedAt\" IS NOT NULL AND \"SignedByUserId\" IS NOT NULL AND \"SignedByDoctorId\" IS NOT NULL AND \"SnapshotJson\" IS NOT NULL)) AND (\"Status\" <> 4 OR \"CancellationReason\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_EmgAdmissionRequest_EmgDisposition_DispositionId",
                        column: x => x.DispositionId,
                        principalSchema: "public",
                        principalTable: "EmgDisposition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmgAdmissionRequest_EmgVisit_EmergencyVisitId",
                        column: x => x.EmergencyVisitId,
                        principalSchema: "public",
                        principalTable: "EmgVisit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmgAdmissionRequest_MstDoctor_ProposedInpatientDoctorId",
                        column: x => x.ProposedInpatientDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmgAdmissionRequest_MstDoctor_SignedByDoctorId",
                        column: x => x.SignedByDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmgAdmissionRequest_MstPatientClass_RequestedPatientClassId",
                        column: x => x.RequestedPatientClassId,
                        principalSchema: "public",
                        principalTable: "MstPatientClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmgAdmissionRequest_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmgAdmissionRequest_MstServiceUnit_RequestedServiceUnitId",
                        column: x => x.RequestedServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmgAdmissionRequest_RegPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmgAdmissionRequestEvent",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdmissionRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    FromStatus = table.Column<int>(type: "integer", nullable: true),
                    ToStatus = table.Column<int>(type: "integer", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_EmgAdmissionRequestEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmgAdmissionRequestEvent_EmgAdmissionRequest_AdmissionReque~",
                        column: x => x.AdmissionRequestId,
                        principalSchema: "public",
                        principalTable: "EmgAdmissionRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmgAdmissionRequest_DispositionId",
                schema: "public",
                table: "EmgAdmissionRequest",
                column: "DispositionId");

            migrationBuilder.CreateIndex(
                name: "IX_EmgAdmissionRequest_EncounterId",
                schema: "public",
                table: "EmgAdmissionRequest",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_EmgAdmissionRequest_PatientId",
                schema: "public",
                table: "EmgAdmissionRequest",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_EmgAdmissionRequest_ProposedInpatientDoctorId",
                schema: "public",
                table: "EmgAdmissionRequest",
                column: "ProposedInpatientDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_EmgAdmissionRequest_RequestedPatientClassId",
                schema: "public",
                table: "EmgAdmissionRequest",
                column: "RequestedPatientClassId");

            migrationBuilder.CreateIndex(
                name: "IX_EmgAdmissionRequest_RequestedServiceUnitId",
                schema: "public",
                table: "EmgAdmissionRequest",
                column: "RequestedServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_EmgAdmissionRequest_SignedByDoctorId",
                schema: "public",
                table: "EmgAdmissionRequest",
                column: "SignedByDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_EmgAdmissionRequest_Status",
                schema: "public",
                table: "EmgAdmissionRequest",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "UX_EmgAdmissionRequest_Visit_Open",
                schema: "public",
                table: "EmgAdmissionRequest",
                column: "EmergencyVisitId",
                unique: true,
                filter: "\"Status\" IN (1, 2) AND NOT \"IsDelete\"");

            migrationBuilder.CreateIndex(
                name: "IX_EmgAdmissionRequestEvent_Request_OccurredAt",
                schema: "public",
                table: "EmgAdmissionRequestEvent",
                columns: new[] { "AdmissionRequestId", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM public."EmgAdmissionRequest") THEN
                        RAISE EXCEPTION 'Down BE-IGD-072 dihentikan: EmgAdmissionRequest sudah berisi Form Permintaan Rawat Inap. Menghapus tabel ini menghilangkan dokumen klinis pasien.';
                    END IF;
                END $$;
                """);

            migrationBuilder.DropTable(
                name: "EmgAdmissionRequestEvent",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EmgAdmissionRequest",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "RequiresAdmissionRequest",
                schema: "public",
                table: "EmgDispositionType");
        }
    }
}
