using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Membuat penyimpanan reconciliation case dan kebijakannya — <c>RJ-BIL-BE-007</c>,
    /// melaksanakan <c>RJ-BIL-GATE-DEC-008</c>.
    ///
    /// <para><b>Yang sengaja dibuang dari hasil bangkitan EF.</b></para>
    ///
    /// EF semula menerbitkan lima <c>CreateTable</c>, bukan dua. Tiga tambahannya —
    /// <c>MstRegister</c>, <c>MstRoomChargePolicy</c>, dan <c>MstTaxRule</c> — bukan tabel baru
    /// dan bukan milik Billing. Ketiganya muncul karena
    /// <c>ApplicationDbContextModelSnapshot.cs</c> kehilangan ketiga entity itu, sehingga EF
    /// menyangka tabelnya belum ada.
    ///
    /// Kenyataannya sebaliknya. <c>MstRoomChargePolicy</c> dan <c>MstTaxRule</c> sudah dibuat
    /// migration <c>20260820084721_AddTaxAndRoomChargePolicies</c>, dan <c>MstRegister</c>
    /// terbukti sudah ada di database melalui galat <c>42P07 relation already exists</c> ketika
    /// migration ini dicoba apa adanya. Membiarkan ketiga <c>CreateTable</c> itu berarti
    /// migration ini gagal di setiap database yang sudah berjalan.
    ///
    /// Ketiganya karena itu dibuang dari migration, tetapi tetap ada pada snapshot — dan itulah
    /// yang memperbaiki keadaan: snapshot kembali merekam bahwa ketiga tabel memang ada.
    ///
    /// <c>MstRegister</c> menyisakan satu lubang yang bukan berasal dari task ini: tidak ada
    /// satu pun migration yang pernah membuatnya, sehingga database yang benar-benar baru tidak
    /// akan memilikinya. Menambalnya dari sini berarti mengambil alih schema modul orang lain,
    /// jadi lubang itu dilaporkan kepada pemiliknya melalui <c>RJ-BIL-NOTICE-001</c>.
    ///
    /// <para><b>Empat puluh lima pasang foreign key.</b></para>
    ///
    /// EF juga menerbitkan 45 <c>DropForeignKey</c> yang langsung diikuti 45 <c>AddForeignKey</c>
    /// pada tabel <c>Bil*</c>. Keduanya diverifikasi berpasangan sempurna — tidak ada yang hanya
    /// di-drop, tidak ada yang hanya ditambahkan — dan seluruhnya memakai
    /// <c>ReferentialAction.Restrict</c>, sama persis dengan definisi aslinya. Karena tidak ada
    /// satu pun perilaku yang berubah, 90 perintah itu dibuang agar migration ini tidak mengunci
    /// puluhan constraint tanpa keperluan.
    /// </summary>
    public partial class AddBillingReconciliationCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BilReconciliationCase",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseNumber = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CaseType = table.Column<int>(type: "integer", nullable: false),
                    SourceContext = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MilestoneFactId = table.Column<Guid>(type: "uuid", nullable: false),
                    MilestoneFactVersion = table.Column<int>(type: "integer", nullable: false),
                    EffectType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ProcessingEffectId = table.Column<Guid>(type: "uuid", nullable: true),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    FolioId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChargeLineId = table.Column<Guid>(type: "uuid", nullable: true),
                    ImpactAmount = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    ImpactDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BlocksFolioClosure = table.Column<bool>(type: "boolean", nullable: false),
                    CaseStatus = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DetectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SlaDueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SlaBreachedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    LastAttemptAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextAction = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ResolutionType = table.Column<int>(type: "integer", nullable: true),
                    ResolutionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_BilReconciliationCase", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstBillingReconciliationPolicy",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseType = table.Column<int>(type: "integer", nullable: false),
                    MaterialityThresholdAmount = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    SlaMinutes = table.Column<int>(type: "integer", nullable: false),
                    DefaultPriority = table.Column<int>(type: "integer", nullable: false),
                    AllowAutoResolveDeterministicDuplicate = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_MstBillingReconciliationPolicy", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BilReconciliationCase_CaseNumber",
                schema: "public",
                table: "BilReconciliationCase",
                column: "CaseNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BilReconciliationCase_CaseStatus",
                schema: "public",
                table: "BilReconciliationCase",
                column: "CaseStatus");

            migrationBuilder.CreateIndex(
                name: "IX_BilReconciliationCase_CaseType_SourceContext_MilestoneFactI~",
                schema: "public",
                table: "BilReconciliationCase",
                columns: new[] { "CaseType", "SourceContext", "MilestoneFactId", "MilestoneFactVersion", "EffectType" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BilReconciliationCase_EncounterId_CaseStatus",
                schema: "public",
                table: "BilReconciliationCase",
                columns: new[] { "EncounterId", "CaseStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_BilReconciliationCase_FolioId_CaseStatus",
                schema: "public",
                table: "BilReconciliationCase",
                columns: new[] { "FolioId", "CaseStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_BilReconciliationCase_OwnerUserId",
                schema: "public",
                table: "BilReconciliationCase",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MstBillingReconciliationPolicy_CaseType",
                schema: "public",
                table: "MstBillingReconciliationPolicy",
                column: "CaseType",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.Sql(
                "INSERT INTO public.\"MstBillingReconciliationPolicy\" (" +
                "\"Id\", \"CaseType\", \"MaterialityThresholdAmount\", \"SlaMinutes\", " +
                "\"DefaultPriority\", \"AllowAutoResolveDeterministicDuplicate\", " +
                "\"Description\", \"IsActive\", \"SortOrder\", " +
                "\"CreateDateTime\", \"CreateBy\", \"UpdateBy\", \"DeleteBy\", \"CancelBy\", " +
                "\"IsCancel\", \"IsDelete\") VALUES (" +
                "'8f1a0c41-6d2e-4b57-9c30-1a7e5b2d4001', 1, 0, 0, 2, false, " +
                "'Outcome Unknown — Hasil pemrosesan tidak diketahui karena jawaban hilang atau timeout.', true, 1, " +
                "NOW(), '00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', false, false) " +
                "ON CONFLICT DO NOTHING;");

            migrationBuilder.Sql(
                "INSERT INTO public.\"MstBillingReconciliationPolicy\" (" +
                "\"Id\", \"CaseType\", \"MaterialityThresholdAmount\", \"SlaMinutes\", " +
                "\"DefaultPriority\", \"AllowAutoResolveDeterministicDuplicate\", " +
                "\"Description\", \"IsActive\", \"SortOrder\", " +
                "\"CreateDateTime\", \"CreateBy\", \"UpdateBy\", \"DeleteBy\", \"CancelBy\", " +
                "\"IsCancel\", \"IsDelete\") VALUES (" +
                "'8f1a0c41-6d2e-4b57-9c30-1a7e5b2d4002', 2, 0, 0, 2, false, " +
                "'Partial Component Failure — Sebagian komponen tagihan berhasil diterapkan dan sebagian gagal.', true, 2, " +
                "NOW(), '00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', false, false) " +
                "ON CONFLICT DO NOTHING;");

            migrationBuilder.Sql(
                "INSERT INTO public.\"MstBillingReconciliationPolicy\" (" +
                "\"Id\", \"CaseType\", \"MaterialityThresholdAmount\", \"SlaMinutes\", " +
                "\"DefaultPriority\", \"AllowAutoResolveDeterministicDuplicate\", " +
                "\"Description\", \"IsActive\", \"SortOrder\", " +
                "\"CreateDateTime\", \"CreateBy\", \"UpdateBy\", \"DeleteBy\", \"CancelBy\", " +
                "\"IsCancel\", \"IsDelete\") VALUES (" +
                "'8f1a0c41-6d2e-4b57-9c30-1a7e5b2d4003', 3, 0, 0, 2, false, " +
                "'Permanent Failure — Pemrosesan gagal menetap dan berhenti dari percobaan ulang otomatis.', true, 3, " +
                "NOW(), '00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', false, false) " +
                "ON CONFLICT DO NOTHING;");

            migrationBuilder.Sql(
                "INSERT INTO public.\"MstBillingReconciliationPolicy\" (" +
                "\"Id\", \"CaseType\", \"MaterialityThresholdAmount\", \"SlaMinutes\", " +
                "\"DefaultPriority\", \"AllowAutoResolveDeterministicDuplicate\", " +
                "\"Description\", \"IsActive\", \"SortOrder\", " +
                "\"CreateDateTime\", \"CreateBy\", \"UpdateBy\", \"DeleteBy\", \"CancelBy\", " +
                "\"IsCancel\", \"IsDelete\") VALUES (" +
                "'8f1a0c41-6d2e-4b57-9c30-1a7e5b2d4004', 4, 0, 0, 2, false, " +
                "'Duplicate Charge — Terdapat dugaan tagihan ganda atas satu fakta klinis.', true, 4, " +
                "NOW(), '00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', false, false) " +
                "ON CONFLICT DO NOTHING;");

            migrationBuilder.Sql(
                "INSERT INTO public.\"MstBillingReconciliationPolicy\" (" +
                "\"Id\", \"CaseType\", \"MaterialityThresholdAmount\", \"SlaMinutes\", " +
                "\"DefaultPriority\", \"AllowAutoResolveDeterministicDuplicate\", " +
                "\"Description\", \"IsActive\", \"SortOrder\", " +
                "\"CreateDateTime\", \"CreateBy\", \"UpdateBy\", \"DeleteBy\", \"CancelBy\", " +
                "\"IsCancel\", \"IsDelete\") VALUES (" +
                "'8f1a0c41-6d2e-4b57-9c30-1a7e5b2d4005', 5, 0, 0, 2, false, " +
                "'Missing Fact — Fakta klinis kanonik ada, tetapi tidak ditemukan keadaan Billing yang sepadan.', true, 5, " +
                "NOW(), '00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', false, false) " +
                "ON CONFLICT DO NOTHING;");

            migrationBuilder.Sql(
                "INSERT INTO public.\"MstBillingReconciliationPolicy\" (" +
                "\"Id\", \"CaseType\", \"MaterialityThresholdAmount\", \"SlaMinutes\", " +
                "\"DefaultPriority\", \"AllowAutoResolveDeterministicDuplicate\", " +
                "\"Description\", \"IsActive\", \"SortOrder\", " +
                "\"CreateDateTime\", \"CreateBy\", \"UpdateBy\", \"DeleteBy\", \"CancelBy\", " +
                "\"IsCancel\", \"IsDelete\") VALUES (" +
                "'8f1a0c41-6d2e-4b57-9c30-1a7e5b2d4006', 6, 0, 0, 2, false, " +
                "'Orphan Charge — Terdapat keadaan Billing tanpa fakta klinis kanonik yang mendasarinya.', true, 6, " +
                "NOW(), '00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', false, false) " +
                "ON CONFLICT DO NOTHING;");

            migrationBuilder.Sql(
                "INSERT INTO public.\"MstBillingReconciliationPolicy\" (" +
                "\"Id\", \"CaseType\", \"MaterialityThresholdAmount\", \"SlaMinutes\", " +
                "\"DefaultPriority\", \"AllowAutoResolveDeterministicDuplicate\", " +
                "\"Description\", \"IsActive\", \"SortOrder\", " +
                "\"CreateDateTime\", \"CreateBy\", \"UpdateBy\", \"DeleteBy\", \"CancelBy\", " +
                "\"IsCancel\", \"IsDelete\") VALUES (" +
                "'8f1a0c41-6d2e-4b57-9c30-1a7e5b2d4007', 7, 0, 0, 2, false, " +
                "'Amount Mismatch — Nilai atau kuantitas pada Billing tidak sepadan dengan fakta klinisnya.', true, 7, " +
                "NOW(), '00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', false, false) " +
                "ON CONFLICT DO NOTHING;");

            migrationBuilder.Sql(
                "INSERT INTO public.\"MstBillingReconciliationPolicy\" (" +
                "\"Id\", \"CaseType\", \"MaterialityThresholdAmount\", \"SlaMinutes\", " +
                "\"DefaultPriority\", \"AllowAutoResolveDeterministicDuplicate\", " +
                "\"Description\", \"IsActive\", \"SortOrder\", " +
                "\"CreateDateTime\", \"CreateBy\", \"UpdateBy\", \"DeleteBy\", \"CancelBy\", " +
                "\"IsCancel\", \"IsDelete\") VALUES (" +
                "'8f1a0c41-6d2e-4b57-9c30-1a7e5b2d4008', 8, 0, 0, 2, false, " +
                "'Version Mismatch — Versi fakta yang diterapkan tidak sepadan dengan versi kanoniknya.', true, 8, " +
                "NOW(), '00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', false, false) " +
                "ON CONFLICT DO NOTHING;");

            migrationBuilder.Sql(
                "INSERT INTO public.\"MstBillingReconciliationPolicy\" (" +
                "\"Id\", \"CaseType\", \"MaterialityThresholdAmount\", \"SlaMinutes\", " +
                "\"DefaultPriority\", \"AllowAutoResolveDeterministicDuplicate\", " +
                "\"Description\", \"IsActive\", \"SortOrder\", " +
                "\"CreateDateTime\", \"CreateBy\", \"UpdateBy\", \"DeleteBy\", \"CancelBy\", " +
                "\"IsCancel\", \"IsDelete\") VALUES (" +
                "'8f1a0c41-6d2e-4b57-9c30-1a7e5b2d4009', 9, 0, 0, 2, false, " +
                "'Stale Projection — Proyeksi pembacaan tertinggal dari keadaan kanonik.', true, 9, " +
                "NOW(), '00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', false, false) " +
                "ON CONFLICT DO NOTHING;");

            migrationBuilder.Sql(
                "INSERT INTO public.\"MstBillingReconciliationPolicy\" (" +
                "\"Id\", \"CaseType\", \"MaterialityThresholdAmount\", \"SlaMinutes\", " +
                "\"DefaultPriority\", \"AllowAutoResolveDeterministicDuplicate\", " +
                "\"Description\", \"IsActive\", \"SortOrder\", " +
                "\"CreateDateTime\", \"CreateBy\", \"UpdateBy\", \"DeleteBy\", \"CancelBy\", " +
                "\"IsCancel\", \"IsDelete\") VALUES (" +
                "'8f1a0c41-6d2e-4b57-9c30-1a7e5b2d4010', 10, 0, 0, 2, false, " +
                "'Unresolved Exception — Pengecualian darurat yang belum diselesaikan.', true, 10, " +
                "NOW(), '00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', " +
                "'00000000-0000-0000-0000-000000000000', false, false) " +
                "ON CONFLICT DO NOTHING;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BilReconciliationCase",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstBillingReconciliationPolicy",
                schema: "public");

        }
    }
}
