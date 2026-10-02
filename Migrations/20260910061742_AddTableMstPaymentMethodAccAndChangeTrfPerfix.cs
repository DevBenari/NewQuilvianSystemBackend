using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddTableMstPaymentMethodAccAndChangeTrfPerfix : Migration
    {
        // Normalisasi tabel resep Farmasi ke prefix canonical `Phm`:
        //
        //   TrxPrescriptionCompoundItem -> PhmPrescriptionCompoundItem
        //   TrxPrescriptionCompound     -> PhmPrescriptionCompound
        //   TrxPrescriptionItem         -> PhmPrescriptionItem
        //
        // Tidak ada DROP+CREATE untuk tabel resep (QBE-DB-002). Seluruhnya RENAME agar isi tabel
        // dan riwayat transaksi resep tetap utuh. EF Tools secara default menghasilkan DROP+CREATE
        // untuk penggantian nama entitas, sehingga badan migration dinormalkan menjadi RENAME.
        //
        // Panjang karakter nama tabel lama dan baru sama (26, 22, 19 karakter), sehingga titik potong
        // identifier 63 karakter Postgres untuk foreign key dan index tidak berubah.
        //
        // Fitur Master Data & Billing Kasir baru:
        // - Pembuatan tabel `MstPaymentMethodAccount` beserta index dan foreign key ke `MstPaymentMethod`.
        // - Penambahan kolom `PaymentMethodAccountId` pada `BilTender`.
        // - Penambahan kolom `PaymentMethodAccountId` dan `ReferenceNumber` pada `BilDepositMovement`.

        private const string Peta = @"
                'TrxPrescriptionCompoundItem', 'PhmPrescriptionCompoundItem',
                'TrxPrescriptionCompound', 'PhmPrescriptionCompound',
                'TrxPrescriptionItem', 'PhmPrescriptionItem'";

        private const string PetaBalik = @"
                'PhmPrescriptionCompoundItem', 'TrxPrescriptionCompoundItem',
                'PhmPrescriptionCompound', 'TrxPrescriptionCompound',
                'PhmPrescriptionItem', 'TrxPrescriptionItem'";

        private static string Skrip(string peta) => $$"""
            DO $qbe$
            DECLARE
                peta CONSTANT text[] := ARRAY[{{peta}}
                ];
                lama text;
                baru text;
                i int;
                r record;
            BEGIN
                -- 1. Nama tabel.
                FOR i IN 1 .. array_length(peta, 1) / 2 LOOP
                    lama := peta[i * 2 - 1];
                    baru := peta[i * 2];
                    IF EXISTS (
                        SELECT 1 FROM pg_class c
                        JOIN pg_namespace n ON n.oid = c.relnamespace
                        WHERE n.nspname = 'public' AND c.relname = lama AND c.relkind = 'r'
                    ) THEN
                        EXECUTE format('ALTER TABLE public.%I RENAME TO %I', lama, baru);
                    END IF;
                END LOOP;

                -- 2. Constraint mana pun yang namanya memuat nama tabel lama, termasuk FK
                --    milik tabel lain yang menunjuk ke sini. Mengganti nama constraint
                --    PK/unique sekaligus mengganti nama index penopangnya.
                FOR i IN 1 .. array_length(peta, 1) / 2 LOOP
                    lama := peta[i * 2 - 1];
                    baru := peta[i * 2];
                    FOR r IN
                        SELECT t.relname AS tabel, c.conname AS nama
                        FROM pg_constraint c
                        JOIN pg_class t ON t.oid = c.conrelid
                        JOIN pg_namespace n ON n.oid = t.relnamespace
                        WHERE n.nspname = 'public' AND c.conname LIKE '%' || lama || '%'
                    LOOP
                        EXECUTE format(
                            'ALTER TABLE public.%I RENAME CONSTRAINT %I TO %I',
                            r.tabel, r.nama, replace(r.nama, lama, baru));
                    END LOOP;
                END LOOP;

                -- 3. Sisa index yang tidak ditopang constraint.
                FOR i IN 1 .. array_length(peta, 1) / 2 LOOP
                    lama := peta[i * 2 - 1];
                    baru := peta[i * 2];
                    FOR r IN
                        SELECT c.relname AS nama
                        FROM pg_class c
                        JOIN pg_namespace n ON n.oid = c.relnamespace
                        WHERE n.nspname = 'public' AND c.relkind = 'i'
                          AND c.relname LIKE '%' || lama || '%'
                    LOOP
                        EXECUTE format('ALTER INDEX public.%I RENAME TO %I',
                                       r.nama, replace(r.nama, lama, baru));
                    END LOOP;
                END LOOP;
            END
            $qbe$;
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Rename tabel transaksi resep dari Trx* ke Phm* (tanpa drop data).
            migrationBuilder.RenameTable(
                name: "TrxPrescriptionCompoundItem",
                schema: "public",
                newName: "PhmPrescriptionCompoundItem");

            migrationBuilder.RenameTable(
                name: "TrxPrescriptionCompound",
                schema: "public",
                newName: "PhmPrescriptionCompound");

            migrationBuilder.RenameTable(
                name: "TrxPrescriptionItem",
                schema: "public",
                newName: "PhmPrescriptionItem");

            migrationBuilder.Sql(Skrip(Peta));

            // 2. Tambah kolom pada modul Billing.
            migrationBuilder.AddColumn<Guid>(
                name: "PaymentMethodAccountId",
                schema: "public",
                table: "BilTender",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentMethodAccountId",
                schema: "public",
                table: "BilDepositMovement",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                schema: "public",
                table: "BilDepositMovement",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            // 3. Buat tabel MstPaymentMethodAccount.
            migrationBuilder.CreateTable(
                name: "MstPaymentMethodAccount",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentMethodId = table.Column<Guid>(type: "uuid", nullable: false),
                    BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AccountNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AccountHolderName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Purpose = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstPaymentMethodAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstPaymentMethodAccount_MstPaymentMethod_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalSchema: "public",
                        principalTable: "MstPaymentMethod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstPaymentMethodAccount_AccountNumber",
                schema: "public",
                table: "MstPaymentMethodAccount",
                column: "AccountNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MstPaymentMethodAccount_PaymentMethodId",
                schema: "public",
                table: "MstPaymentMethodAccount",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPaymentMethodAccount_PaymentMethodId_IsActive_IsDelete",
                schema: "public",
                table: "MstPaymentMethodAccount",
                columns: new[] { "PaymentMethodId", "IsActive", "IsDelete" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Hapus tabel MstPaymentMethodAccount.
            migrationBuilder.DropTable(
                name: "MstPaymentMethodAccount",
                schema: "public");

            // 2. Hapus kolom modul Billing.
            migrationBuilder.DropColumn(
                name: "PaymentMethodAccountId",
                schema: "public",
                table: "BilTender");

            migrationBuilder.DropColumn(
                name: "PaymentMethodAccountId",
                schema: "public",
                table: "BilDepositMovement");

            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                schema: "public",
                table: "BilDepositMovement");

            // 3. Rollback nama tabel transaksi resep dari Phm* kembali ke Trx*.
            migrationBuilder.RenameTable(
                name: "PhmPrescriptionCompoundItem",
                schema: "public",
                newName: "TrxPrescriptionCompoundItem");

            migrationBuilder.RenameTable(
                name: "PhmPrescriptionCompound",
                schema: "public",
                newName: "TrxPrescriptionCompound");

            migrationBuilder.RenameTable(
                name: "PhmPrescriptionItem",
                schema: "public",
                newName: "TrxPrescriptionItem");

            migrationBuilder.Sql(Skrip(PetaBalik));
        }
    }
}
