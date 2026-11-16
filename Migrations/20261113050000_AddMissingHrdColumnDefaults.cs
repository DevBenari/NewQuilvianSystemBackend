using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261113050000_AddMissingHrdColumnDefaults")]
    public partial class AddMissingHrdColumnDefaults : Migration
    {
        // Model EF sudah memberi kolom-kolom ini HasDefaultValue/HasDefaultValueSql, dan
        // ApplicationDbContextModelSnapshot.cs sudah memuatnya, tetapi tidak ada migration yang
        // memasang DEFAULT-nya ke tabel. EF tidak mengirim kolom ber-default bila nilainya sama
        // dengan nilai bawaan CLR, sehingga PostgreSQL mengisi NULL dan menolak dengan 23502.
        // Contoh nyata: setiap tambah shift roster (TrxShiftAssignment.IsManualOverride = false)
        // gagal 500 di database yang dibangun dari migration.
        //
        // Migration ini hanya menambah DEFAULT. Tidak mengubah tipe, nullability, maupun data.
        // Snapshot tidak diubah karena sudah benar; migration ini sengaja tanpa Designer.
        //
        // Sengaja TIDAK disertakan (perbaikannya di model, bukan di database):
        // - MstOnboardingTemplateTask.IsRequired (DEFAULT TRUE) dan
        // - MstOnboardingTemplate.TargetCompletionDays (DEFAULT 30).
        // Keduanya diisi dari permintaan pengguna. Dengan DEFAULT terpasang, EF tidak mengirim
        // false/0 sehingga nilai pilihan pengguna diam-diam berubah menjadi TRUE/30. Tanpa DEFAULT,
        // kasus itu gagal terang-terangan (23502), yang lebih aman sampai modelnya diperbaiki.

        private static readonly (string Table, string Column, string Default)[] Defaults =
        {
            ("TrxShiftAssignment", "IsManualOverride", "false"),
            ("HrdClinicalPreceptorship", "CreateDateTime", "CURRENT_TIMESTAMP"),
            ("HrdClinicalPreceptorshipPreceptor", "CreateDateTime", "CURRENT_TIMESTAMP"),
            ("HrdContractCorrection", "CreateDateTime", "CURRENT_TIMESTAMP"),
            ("HrdContractDecision", "CreateDateTime", "CURRENT_TIMESTAMP"),
            ("HrdEmploymentLifecycleSetting", "CreateDateTime", "CURRENT_TIMESTAMP"),
            ("HrdInitialContractBatch", "CreateDateTime", "CURRENT_TIMESTAMP"),
            ("HrdInitialContractBatchRow", "CreateDateTime", "CURRENT_TIMESTAMP"),
            ("HrdOnboardingAccessRequest", "CreateDateTime", "CURRENT_TIMESTAMP"),
            ("HrdOnboardingCase", "CreateDateTime", "CURRENT_TIMESTAMP"),
            ("HrdOnboardingCaseItem", "CreateDateTime", "CURRENT_TIMESTAMP"),
            ("HrdProbation", "CreateDateTime", "CURRENT_TIMESTAMP"),
            ("HrdProbationAssessment", "CreateDateTime", "CURRENT_TIMESTAMP"),
            ("MstOnboardingTemplate", "Version", "1"),
            ("MstOnboardingTemplateTask", "TaskCategory", "'General'"),
            ("MstOnboardingTemplateTask", "DueDayOffset", "0"),
        };

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var (table, column, value) in Defaults)
            {
                migrationBuilder.Sql($@"ALTER TABLE public.""{table}"" ALTER COLUMN ""{column}"" SET DEFAULT {value};");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var (table, column, _) in Defaults)
            {
                migrationBuilder.Sql($@"ALTER TABLE public.""{table}"" ALTER COLUMN ""{column}"" DROP DEFAULT;");
            }
        }
    }
}
