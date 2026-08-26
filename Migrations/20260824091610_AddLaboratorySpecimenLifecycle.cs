using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddLaboratorySpecimenLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                schema: "public",
                table: "LabOrder",
                type: "timestamp with time zone",
                nullable: true);

            // Nilai bawaan 2 = Requested, bukan 0. Enum LabOrderStatus tidak memiliki anggota
            // bernilai 0, sehingga membiarkan bawaan 0 akan mengisi seluruh baris lama dengan
            // status yang tidak dapat dibaca kembali oleh aplikasi.
            migrationBuilder.AddColumn<int>(
                name: "OrderStatus",
                schema: "public",
                table: "LabOrder",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestedAt",
                schema: "public",
                table: "LabOrder",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RequestedByUserId",
                schema: "public",
                table: "LabOrder",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusBeforeHold",
                schema: "public",
                table: "LabOrder",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                schema: "public",
                table: "LabOrder",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Pengisian data lama.
            //
            // Sebelum migration ini, satu-satunya penanda keadaan LabOrder adalah IsCancel.
            // Baris yang sudah dibatalkan dipetakan ke Cancelled (8); sisanya ke Requested (2),
            // karena endpoint pembuatan yang lama memang berarti "pesanan sudah dikirim ke
            // laboratorium". Tidak ada baris yang kehilangan makna, dan tidak ada baris yang
            // memperoleh status yang belum pernah benar-benar terjadi.
            migrationBuilder.Sql(
                "UPDATE public.\"LabOrder\" SET \"OrderStatus\" = 8 WHERE \"IsCancel\" = true;");

            // Waktu pengiriman pesanan lama tidak pernah dicatat tersendiri, sehingga waktu
            // pembuatan barisnya adalah perkiraan terbaik yang tersedia dan bukan karangan.
            migrationBuilder.Sql(
                "UPDATE public.\"LabOrder\" SET \"RequestedAt\" = \"CreateDateTime\" " +
                "WHERE \"RequestedAt\" IS NULL;");

            migrationBuilder.CreateTable(
                name: "MstLabRejectionReason",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReasonName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsInternalHospitalError = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresNote = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_MstLabRejectionReason", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrxLabSpecimen",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LabOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcedureId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpecimenBarcode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SpecimenSequence = table.Column<int>(type: "integer", nullable: false),
                    SpecimenDescription = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProcedureCodeSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ProcedureNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TariffId = table.Column<Guid>(type: "uuid", nullable: true),
                    TariffCodeSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UnitPriceSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    SpecimenStatus = table.Column<int>(type: "integer", nullable: false),
                    StatusBeforeHold = table.Column<int>(type: "integer", nullable: true),
                    CollectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CollectedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReceivedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectionReasonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectionReasonCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RejectionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SupersededSpecimenId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecollectionCause = table.Column<int>(type: "integer", nullable: true),
                    RecollectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RecollectionAuthorizedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecollectionAuthorizedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_TrxLabSpecimen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxLabSpecimen_LabOrder_LabOrderId",
                        column: x => x.LabOrderId,
                        principalSchema: "public",
                        principalTable: "LabOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLabSpecimen_MstLabRejectionReason_RejectionReasonId",
                        column: x => x.RejectionReasonId,
                        principalSchema: "public",
                        principalTable: "MstLabRejectionReason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLabSpecimen_MstProcedure_ProcedureId",
                        column: x => x.ProcedureId,
                        principalSchema: "public",
                        principalTable: "MstProcedure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLabSpecimen_TrxLabSpecimen_SupersededSpecimenId",
                        column: x => x.SupersededSpecimenId,
                        principalSchema: "public",
                        principalTable: "TrxLabSpecimen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxLabTransitionHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LabOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabSpecimenId = table.Column<Guid>(type: "uuid", nullable: true),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Scope = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FromStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ToStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ReasonNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_TrxLabTransitionHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxLabTransitionHistory_LabOrder_LabOrderId",
                        column: x => x.LabOrderId,
                        principalSchema: "public",
                        principalTable: "LabOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLabTransitionHistory_TrxLabSpecimen_LabSpecimenId",
                        column: x => x.LabSpecimenId,
                        principalSchema: "public",
                        principalTable: "TrxLabSpecimen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLabTransitionHistory_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabOrder_OrderStatus",
                schema: "public",
                table: "LabOrder",
                column: "OrderStatus");

            migrationBuilder.CreateIndex(
                name: "IX_MstLabRejectionReason_ReasonCode",
                schema: "public",
                table: "MstLabRejectionReason",
                column: "ReasonCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLabSpecimen_LabOrderId_SpecimenSequence",
                schema: "public",
                table: "TrxLabSpecimen",
                columns: new[] { "LabOrderId", "SpecimenSequence" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLabSpecimen_ProcedureId",
                schema: "public",
                table: "TrxLabSpecimen",
                column: "ProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLabSpecimen_RejectionReasonId",
                schema: "public",
                table: "TrxLabSpecimen",
                column: "RejectionReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLabSpecimen_SpecimenBarcode",
                schema: "public",
                table: "TrxLabSpecimen",
                column: "SpecimenBarcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxLabSpecimen_SpecimenStatus",
                schema: "public",
                table: "TrxLabSpecimen",
                column: "SpecimenStatus");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLabSpecimen_SupersededSpecimenId",
                schema: "public",
                table: "TrxLabSpecimen",
                column: "SupersededSpecimenId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLabTransitionHistory_EncounterId",
                schema: "public",
                table: "TrxLabTransitionHistory",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLabTransitionHistory_LabOrderId_OccurredAt",
                schema: "public",
                table: "TrxLabTransitionHistory",
                columns: new[] { "LabOrderId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLabTransitionHistory_LabSpecimenId",
                schema: "public",
                table: "TrxLabTransitionHistory",
                column: "LabSpecimenId");

            SeedRejectionReasons(migrationBuilder);
        }

        /// <summary>
        /// Mengisi katalog alasan penolakan sampel dengan baseline implementasi yang ditetapkan
        /// author pada <c>RJ-BIL-OQ-009</c>.
        ///
        /// Ini baseline, bukan SOP klinis final. Lab dan Clinical Governance dapat menambah,
        /// menonaktifkan, atau memperinci alasan melalui master data tanpa perubahan program.
        /// Karena itu pengisian memakai <c>ON CONFLICT DO NOTHING</c>: menjalankan ulang
        /// migration tidak menimpa perubahan yang sudah dilakukan pengguna.
        ///
        /// <c>OTHER</c> mewajibkan catatan tambahan sesuai keputusan author.
        /// </summary>
        private static void SeedRejectionReasons(MigrationBuilder migrationBuilder)
        {
            // Id ditetapkan tetap, bukan dibangkitkan database, agar baris baseline memiliki
            // identitas yang sama di setiap lingkungan dan dapat dirujuk dengan pasti.
            var reasons = new (string Id, string Code, string Name, bool IsInternalError, bool RequiresNote, int Sort)[]
            {
                ("1f2a4c60-0001-4a10-9f01-6b1d0a5e7c01", "IDENTITY_MISMATCH", "Identitas tidak cocok", true, false, 1),
                ("1f2a4c60-0002-4a10-9f01-6b1d0a5e7c02", "LABELING_ISSUE", "Masalah pelabelan", true, false, 2),
                ("1f2a4c60-0003-4a10-9f01-6b1d0a5e7c03", "SPECIMEN_TYPE_OR_CONTAINER_MISMATCH", "Jenis sampel atau wadah tidak sesuai", true, false, 3),
                ("1f2a4c60-0004-4a10-9f01-6b1d0a5e7c04", "INSUFFICIENT_QUANTITY", "Jumlah sampel tidak mencukupi", false, false, 4),
                ("1f2a4c60-0005-4a10-9f01-6b1d0a5e7c05", "SPECIMEN_INTEGRITY_OR_QUALITY_ISSUE", "Mutu atau keutuhan sampel bermasalah", false, false, 5),
                ("1f2a4c60-0006-4a10-9f01-6b1d0a5e7c06", "COLLECTION_ISSUE", "Masalah pada proses pengambilan", true, false, 6),
                ("1f2a4c60-0007-4a10-9f01-6b1d0a5e7c07", "TRANSPORT_OR_STORAGE_ISSUE", "Masalah pengiriman atau penyimpanan", true, false, 7),
                ("1f2a4c60-0008-4a10-9f01-6b1d0a5e7c08", "ORDER_SPECIMEN_MISMATCH", "Sampel tidak sesuai pesanan", true, false, 8),
                ("1f2a4c60-0009-4a10-9f01-6b1d0a5e7c09", "DUPLICATE_OR_NOT_REQUIRED", "Duplikat atau tidak diperlukan", false, false, 9),
                ("1f2a4c60-0010-4a10-9f01-6b1d0a5e7c10", "OTHER", "Lainnya", false, true, 99)
            };

            const string emptyGuid = "00000000-0000-0000-0000-000000000000";

            foreach (var reason in reasons)
            {
                migrationBuilder.Sql(
                    "INSERT INTO public.\"MstLabRejectionReason\" " +
                    "(\"Id\", \"ReasonCode\", \"ReasonName\", \"Description\", " +
                    "\"IsInternalHospitalError\", \"RequiresNote\", \"IsActive\", \"SortOrder\", " +
                    "\"CreateDateTime\", \"CreateBy\", \"UpdateBy\", \"DeleteBy\", \"CancelBy\", " +
                    "\"IsCancel\", \"IsDelete\") VALUES (" +
                    $"'{reason.Id}', '{reason.Code}', '{reason.Name.Replace("'", "''")}', NULL, " +
                    $"{(reason.IsInternalError ? "true" : "false")}, " +
                    $"{(reason.RequiresNote ? "true" : "false")}, true, {reason.Sort}, " +
                    // NOW() sudah bertipe timestamptz. Memakai NOW() AT TIME ZONE 'UTC' akan
                    // menghasilkan timestamp tanpa zona waktu yang kemudian ditafsirkan ulang
                    // memakai zona server, sehingga waktunya bergeser.
                    "NOW(), " +
                    $"'{emptyGuid}', '{emptyGuid}', '{emptyGuid}', '{emptyGuid}', " +
                    "false, false) " +
                    "ON CONFLICT DO NOTHING;");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrxLabTransitionHistory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxLabSpecimen",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstLabRejectionReason",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_LabOrder_OrderStatus",
                schema: "public",
                table: "LabOrder");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                schema: "public",
                table: "LabOrder");

            migrationBuilder.DropColumn(
                name: "OrderStatus",
                schema: "public",
                table: "LabOrder");

            migrationBuilder.DropColumn(
                name: "RequestedAt",
                schema: "public",
                table: "LabOrder");

            migrationBuilder.DropColumn(
                name: "RequestedByUserId",
                schema: "public",
                table: "LabOrder");

            migrationBuilder.DropColumn(
                name: "StatusBeforeHold",
                schema: "public",
                table: "LabOrder");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "public",
                table: "LabOrder");
        }
    }
}
