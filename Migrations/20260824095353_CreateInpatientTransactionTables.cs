using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class CreateInpatientTransactionTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InpEpisode",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeStatus = table.Column<int>(type: "integer", nullable: false),
                    AdmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DischargeDecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PhysicallyLeftAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PhysicallyLeftByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    MotherEpisodeId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequiresIsolation = table.Column<bool>(type: "boolean", nullable: false),
                    IsolationSource = table.Column<int>(type: "integer", nullable: true),
                    IsolationSetByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsolationSetByDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsolationSetAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsolationNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DischargeType = table.Column<int>(type: "integer", nullable: false),
                    IsClosedWithoutFinancialClearance = table.Column<bool>(type: "boolean", nullable: false),
                    ClosedWithoutClearanceReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_InpEpisode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InpEpisode_AspNetUsers_IsolationSetByUserId",
                        column: x => x.IsolationSetByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpEpisode_AspNetUsers_PhysicallyLeftByUserId",
                        column: x => x.PhysicallyLeftByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpEpisode_InpEpisode_MotherEpisodeId",
                        column: x => x.MotherEpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpEpisode_MstDoctor_IsolationSetByDoctorId",
                        column: x => x.IsolationSetByDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpEpisode_MstPatientClass_PatientClassId",
                        column: x => x.PatientClassId,
                        principalSchema: "public",
                        principalTable: "MstPatientClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpEpisode_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpEpisode_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpEpisode_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InpBedPlacement",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    BedId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndReason = table.Column<int>(type: "integer", nullable: true),
                    TransferReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PlacedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EndedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_InpBedPlacement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InpBedPlacement_AspNetUsers_EndedByUserId",
                        column: x => x.EndedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpBedPlacement_AspNetUsers_PlacedByUserId",
                        column: x => x.PlacedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpBedPlacement_InpEpisode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpBedPlacement_MstBed_BedId",
                        column: x => x.BedId,
                        principalSchema: "public",
                        principalTable: "MstBed",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpBedPlacement_MstPatientClass_PatientClassId",
                        column: x => x.PatientClassId,
                        principalSchema: "public",
                        principalTable: "MstPatientClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpBedPlacement_MstRoom_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "public",
                        principalTable: "MstRoom",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpBedPlacement_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InpBedReservation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    BedId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReservedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReservationStatus = table.Column<int>(type: "integer", nullable: false),
                    ReservedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReleasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_InpBedReservation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InpBedReservation_AspNetUsers_ReservedByUserId",
                        column: x => x.ReservedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpBedReservation_InpEpisode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpBedReservation_MstBed_BedId",
                        column: x => x.BedId,
                        principalSchema: "public",
                        principalTable: "MstBed",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InpClearanceMark",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClearanceItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    MarkedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MarkedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_InpClearanceMark", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InpClearanceMark_AspNetUsers_MarkedByUserId",
                        column: x => x.MarkedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpClearanceMark_InpEpisode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpClearanceMark_MstInpatientClearanceItem_ClearanceItemId",
                        column: x => x.ClearanceItemId,
                        principalSchema: "public",
                        principalTable: "MstInpatientClearanceItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InpCorrectionSession",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    OpenedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OpenedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OpenReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChangedFieldSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_InpCorrectionSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InpCorrectionSession_AspNetUsers_ClosedByUserId",
                        column: x => x.ClosedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpCorrectionSession_AspNetUsers_OpenedByUserId",
                        column: x => x.OpenedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpCorrectionSession_InpEpisode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InpDischargeSummary",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrimaryDiagnosisText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SecondaryDiagnosisText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ProcedureSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DischargeMedicationNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FollowUpInstruction = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReferralDestination = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ClinicalSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SignedByDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_InpDischargeSummary", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InpDischargeSummary_InpEpisode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpDischargeSummary_MstDoctor_SignedByDoctorId",
                        column: x => x.SignedByDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InpDoctorAssignment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssignedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    HandoverReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_InpDoctorAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InpDoctorAssignment_AspNetUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpDoctorAssignment_InpEpisode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpDoctorAssignment_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InpFinancialClearance",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    ClearanceStatus = table.Column<int>(type: "integer", nullable: false),
                    MarkedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MarkedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsManualMarking = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_InpFinancialClearance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InpFinancialClearance_AspNetUsers_MarkedByUserId",
                        column: x => x.MarkedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpFinancialClearance_InpEpisode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InpNurseAssignment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssignedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_InpNurseAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InpNurseAssignment_AspNetUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpNurseAssignment_InpEpisode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpNurseAssignment_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InpStatusHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    FromStatus = table.Column<int>(type: "integer", nullable: true),
                    ToStatus = table.Column<int>(type: "integer", nullable: false),
                    ActionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ActorType = table.Column<int>(type: "integer", nullable: false),
                    ChangedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_InpStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InpStatusHistory_AspNetUsers_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpStatusHistory_InpEpisode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InpDischargeSummaryRevision",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DischargeSummaryId = table.Column<Guid>(type: "uuid", nullable: false),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: false),
                    CorrectionSessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrimaryDiagnosisText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SecondaryDiagnosisText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ProcedureSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DischargeMedicationNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FollowUpInstruction = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReferralDestination = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ClinicalSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    PreviousDischargeType = table.Column<int>(type: "integer", nullable: false),
                    PreviousSignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PreviousSignedByDoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupersededAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SupersededByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_InpDischargeSummaryRevision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InpDischargeSummaryRevision_AspNetUsers_SupersededByUserId",
                        column: x => x.SupersededByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpDischargeSummaryRevision_InpCorrectionSession_Correction~",
                        column: x => x.CorrectionSessionId,
                        principalSchema: "public",
                        principalTable: "InpCorrectionSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpDischargeSummaryRevision_InpDischargeSummary_DischargeSu~",
                        column: x => x.DischargeSummaryId,
                        principalSchema: "public",
                        principalTable: "InpDischargeSummary",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpDischargeSummaryRevision_MstDoctor_PreviousSignedByDocto~",
                        column: x => x.PreviousSignedByDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InpBedPlacement_BedId",
                schema: "public",
                table: "InpBedPlacement",
                column: "BedId");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedPlacement_BedId_Active",
                schema: "public",
                table: "InpBedPlacement",
                column: "BedId",
                unique: true,
                filter: "\"EndDateTime\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedPlacement_EndDateTime",
                schema: "public",
                table: "InpBedPlacement",
                column: "EndDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedPlacement_EndedByUserId",
                schema: "public",
                table: "InpBedPlacement",
                column: "EndedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedPlacement_EpisodeId",
                schema: "public",
                table: "InpBedPlacement",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedPlacement_EpisodeId_SequenceNumber",
                schema: "public",
                table: "InpBedPlacement",
                columns: new[] { "EpisodeId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InpBedPlacement_PatientClassId",
                schema: "public",
                table: "InpBedPlacement",
                column: "PatientClassId");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedPlacement_PlacedByUserId",
                schema: "public",
                table: "InpBedPlacement",
                column: "PlacedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedPlacement_RoomId",
                schema: "public",
                table: "InpBedPlacement",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedPlacement_ServiceUnitId",
                schema: "public",
                table: "InpBedPlacement",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedPlacement_StartDateTime",
                schema: "public",
                table: "InpBedPlacement",
                column: "StartDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedReservation_BedId",
                schema: "public",
                table: "InpBedReservation",
                column: "BedId");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedReservation_BedId_Active",
                schema: "public",
                table: "InpBedReservation",
                column: "BedId",
                unique: true,
                filter: "\"ReservationStatus\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedReservation_EpisodeId",
                schema: "public",
                table: "InpBedReservation",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedReservation_ExpiresAt",
                schema: "public",
                table: "InpBedReservation",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedReservation_ReservationStatus",
                schema: "public",
                table: "InpBedReservation",
                column: "ReservationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedReservation_ReservedByUserId",
                schema: "public",
                table: "InpBedReservation",
                column: "ReservedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InpClearanceMark_ClearanceItemId",
                schema: "public",
                table: "InpClearanceMark",
                column: "ClearanceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InpClearanceMark_EpisodeId_ClearanceItemId",
                schema: "public",
                table: "InpClearanceMark",
                columns: new[] { "EpisodeId", "ClearanceItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InpClearanceMark_MarkedByUserId",
                schema: "public",
                table: "InpClearanceMark",
                column: "MarkedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InpCorrectionSession_ClosedAt",
                schema: "public",
                table: "InpCorrectionSession",
                column: "ClosedAt");

            migrationBuilder.CreateIndex(
                name: "IX_InpCorrectionSession_ClosedByUserId",
                schema: "public",
                table: "InpCorrectionSession",
                column: "ClosedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InpCorrectionSession_EpisodeId",
                schema: "public",
                table: "InpCorrectionSession",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_InpCorrectionSession_EpisodeId_Open",
                schema: "public",
                table: "InpCorrectionSession",
                column: "EpisodeId",
                unique: true,
                filter: "\"ClosedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InpCorrectionSession_EpisodeId_SequenceNumber",
                schema: "public",
                table: "InpCorrectionSession",
                columns: new[] { "EpisodeId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InpCorrectionSession_OpenedAt",
                schema: "public",
                table: "InpCorrectionSession",
                column: "OpenedAt");

            migrationBuilder.CreateIndex(
                name: "IX_InpCorrectionSession_OpenedByUserId",
                schema: "public",
                table: "InpCorrectionSession",
                column: "OpenedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InpDischargeSummary_EpisodeId",
                schema: "public",
                table: "InpDischargeSummary",
                column: "EpisodeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InpDischargeSummary_SignedAt",
                schema: "public",
                table: "InpDischargeSummary",
                column: "SignedAt");

            migrationBuilder.CreateIndex(
                name: "IX_InpDischargeSummary_SignedByDoctorId",
                schema: "public",
                table: "InpDischargeSummary",
                column: "SignedByDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_InpDischargeSummaryRevision_CorrectionSessionId",
                schema: "public",
                table: "InpDischargeSummaryRevision",
                column: "CorrectionSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_InpDischargeSummaryRevision_DischargeSummaryId",
                schema: "public",
                table: "InpDischargeSummaryRevision",
                column: "DischargeSummaryId");

            migrationBuilder.CreateIndex(
                name: "IX_InpDischargeSummaryRevision_DischargeSummaryId_RevisionNumb~",
                schema: "public",
                table: "InpDischargeSummaryRevision",
                columns: new[] { "DischargeSummaryId", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InpDischargeSummaryRevision_PreviousSignedByDoctorId",
                schema: "public",
                table: "InpDischargeSummaryRevision",
                column: "PreviousSignedByDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_InpDischargeSummaryRevision_SupersededAt",
                schema: "public",
                table: "InpDischargeSummaryRevision",
                column: "SupersededAt");

            migrationBuilder.CreateIndex(
                name: "IX_InpDischargeSummaryRevision_SupersededByUserId",
                schema: "public",
                table: "InpDischargeSummaryRevision",
                column: "SupersededByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InpDoctorAssignment_AssignedByUserId",
                schema: "public",
                table: "InpDoctorAssignment",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InpDoctorAssignment_DoctorId",
                schema: "public",
                table: "InpDoctorAssignment",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_InpDoctorAssignment_EndDateTime",
                schema: "public",
                table: "InpDoctorAssignment",
                column: "EndDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_InpDoctorAssignment_EpisodeId",
                schema: "public",
                table: "InpDoctorAssignment",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_InpDoctorAssignment_EpisodeId_Active",
                schema: "public",
                table: "InpDoctorAssignment",
                column: "EpisodeId",
                unique: true,
                filter: "\"EndDateTime\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InpDoctorAssignment_EpisodeId_SequenceNumber",
                schema: "public",
                table: "InpDoctorAssignment",
                columns: new[] { "EpisodeId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InpDoctorAssignment_StartDateTime",
                schema: "public",
                table: "InpDoctorAssignment",
                column: "StartDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_InpEpisode_AdmittedAt",
                schema: "public",
                table: "InpEpisode",
                column: "AdmittedAt");

            migrationBuilder.CreateIndex(
                name: "IX_InpEpisode_ClosedAt",
                schema: "public",
                table: "InpEpisode",
                column: "ClosedAt");

            migrationBuilder.CreateIndex(
                name: "IX_InpEpisode_DischargeDecidedAt",
                schema: "public",
                table: "InpEpisode",
                column: "DischargeDecidedAt");

            migrationBuilder.CreateIndex(
                name: "IX_InpEpisode_EncounterId",
                schema: "public",
                table: "InpEpisode",
                column: "EncounterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InpEpisode_EpisodeNumber",
                schema: "public",
                table: "InpEpisode",
                column: "EpisodeNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InpEpisode_EpisodeStatus",
                schema: "public",
                table: "InpEpisode",
                column: "EpisodeStatus");

            migrationBuilder.CreateIndex(
                name: "IX_InpEpisode_IsClosedWithoutFinancialClearance",
                schema: "public",
                table: "InpEpisode",
                column: "IsClosedWithoutFinancialClearance");

            migrationBuilder.CreateIndex(
                name: "IX_InpEpisode_IsolationSetByDoctorId",
                schema: "public",
                table: "InpEpisode",
                column: "IsolationSetByDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_InpEpisode_IsolationSetByUserId",
                schema: "public",
                table: "InpEpisode",
                column: "IsolationSetByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InpEpisode_MotherEpisodeId",
                schema: "public",
                table: "InpEpisode",
                column: "MotherEpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_InpEpisode_PatientClassId",
                schema: "public",
                table: "InpEpisode",
                column: "PatientClassId");

            migrationBuilder.CreateIndex(
                name: "IX_InpEpisode_PatientId",
                schema: "public",
                table: "InpEpisode",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_InpEpisode_PatientId_Present",
                schema: "public",
                table: "InpEpisode",
                column: "PatientId",
                unique: true,
                filter: "\"EpisodeStatus\" = 1 OR (\"EpisodeStatus\" = 2 AND \"PhysicallyLeftAt\" IS NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_InpEpisode_PhysicallyLeftAt",
                schema: "public",
                table: "InpEpisode",
                column: "PhysicallyLeftAt");

            migrationBuilder.CreateIndex(
                name: "IX_InpEpisode_PhysicallyLeftByUserId",
                schema: "public",
                table: "InpEpisode",
                column: "PhysicallyLeftByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InpEpisode_RequiresIsolation",
                schema: "public",
                table: "InpEpisode",
                column: "RequiresIsolation");

            migrationBuilder.CreateIndex(
                name: "IX_InpEpisode_ServiceUnitId",
                schema: "public",
                table: "InpEpisode",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_InpFinancialClearance_ClearanceStatus",
                schema: "public",
                table: "InpFinancialClearance",
                column: "ClearanceStatus");

            migrationBuilder.CreateIndex(
                name: "IX_InpFinancialClearance_EpisodeId",
                schema: "public",
                table: "InpFinancialClearance",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_InpFinancialClearance_EpisodeId_SequenceNumber",
                schema: "public",
                table: "InpFinancialClearance",
                columns: new[] { "EpisodeId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InpFinancialClearance_MarkedAt",
                schema: "public",
                table: "InpFinancialClearance",
                column: "MarkedAt");

            migrationBuilder.CreateIndex(
                name: "IX_InpFinancialClearance_MarkedByUserId",
                schema: "public",
                table: "InpFinancialClearance",
                column: "MarkedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InpNurseAssignment_AssignedByUserId",
                schema: "public",
                table: "InpNurseAssignment",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InpNurseAssignment_EmployeeId",
                schema: "public",
                table: "InpNurseAssignment",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_InpNurseAssignment_EndDateTime",
                schema: "public",
                table: "InpNurseAssignment",
                column: "EndDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_InpNurseAssignment_EpisodeId",
                schema: "public",
                table: "InpNurseAssignment",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_InpNurseAssignment_EpisodeId_Active",
                schema: "public",
                table: "InpNurseAssignment",
                column: "EpisodeId",
                unique: true,
                filter: "\"EndDateTime\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InpNurseAssignment_EpisodeId_SequenceNumber",
                schema: "public",
                table: "InpNurseAssignment",
                columns: new[] { "EpisodeId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InpNurseAssignment_StartDateTime",
                schema: "public",
                table: "InpNurseAssignment",
                column: "StartDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_InpStatusHistory_ActorType",
                schema: "public",
                table: "InpStatusHistory",
                column: "ActorType");

            migrationBuilder.CreateIndex(
                name: "IX_InpStatusHistory_ChangedAt",
                schema: "public",
                table: "InpStatusHistory",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_InpStatusHistory_ChangedByUserId",
                schema: "public",
                table: "InpStatusHistory",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InpStatusHistory_EpisodeId",
                schema: "public",
                table: "InpStatusHistory",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_InpStatusHistory_EpisodeId_SequenceNumber",
                schema: "public",
                table: "InpStatusHistory",
                columns: new[] { "EpisodeId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InpStatusHistory_ToStatus",
                schema: "public",
                table: "InpStatusHistory",
                column: "ToStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InpBedPlacement",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InpBedReservation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InpClearanceMark",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InpDischargeSummaryRevision",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InpDoctorAssignment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InpFinancialClearance",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InpNurseAssignment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InpStatusHistory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InpCorrectionSession",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InpDischargeSummary",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InpEpisode",
                schema: "public");
        }
    }
}
