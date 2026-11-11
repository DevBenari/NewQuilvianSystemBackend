using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class RenamePayrollTrxAndWfpTablesToPrlPrefix : Migration
    {
        // PAY-MIG-02 — fase 2 Payroll V2: sembilan belas tabel milik Payroll pindah ke prefix
        // registry `Prl`.
        //
        // Prefix modul ini adalah `Prl` menurut baris registry
        // `Corporate | PayrollManagement / Payroll`, yang didaftarkan ACTIVE pada 11 November
        // 2026 atas persetujuan pemilik repository. Tiga belas tabel `Trx*` (run, rincian,
        // masukan, penyesuaian, persetujuan, pembayaran, slip, pembalikan, dan dua tabel jasa
        // medis) dan enam tabel `Wfp*` (profil payroll, pajak, asuransi, dan tiga tabel
        // tunjangan transport) berganti awalan menjadi `Prl`; sisa namanya tidak berubah.
        // Kelas, berkas, konfigurasi, DbSet, dan rujukan sudah dinormalkan di sumber pada task
        // yang sama (PAY-BE-12); migration ini pasangan fisiknya. Tanpa ia, kode baru dan basis
        // data lama akan saling meleset.
        //
        // Polanya mengikuti `RenameMedicalRecordTrxTablesToMrcPrefix` dan migration rename IGD:
        // nama constraint dan index dicari dari katalog Postgres, tidak diketik satu per satu.
        // Sebagian nama buatan EF sudah terpotong pada batas 63 karakter dan berakhiran `~`.
        //
        // Tidak ada DROP+CREATE di sini (QBE-DB-002). Seluruhnya RENAME, sehingga isi tabel
        // tetap utuh, termasuk run, slip, dan bukti pembayaran yang sudah ada.
        //
        // Awalan lama dan baru sama-sama tiga huruf. Karena itu panjang nama tidak berubah,
        // tidak ada nama yang bertambah panjang sampai terpotong, dan titik potong `~` pada nama
        // yang sudah terpotong tetap jatuh di tempat yang sama.
        //
        // Pencocokan nama memakai batas token (awal nama, `_`, `~`, atau akhir nama), bukan
        // substring bebas. Ini penting pada arah balik: `PrlPayroll` (dulu `WfpPayroll`) adalah
        // awalan dari `PrlPayrollRun` (dulu `TrxPayrollRun`), dan substring bebas akan salah
        // memetakan yang kedua. Pencocokan memakai lookahead supaya FK ke tabel yang sama
        // (nama yang memuat satu token dua kali) tetap terganti seluruhnya.
        //
        // Yang SENGAJA tidak berubah: nama kolom, tipe kolom, urutan kolom, nama properti
        // navigasi, isi data, serta nama constraint/index yang tidak memuat nama tabel Payroll.
        // Tabel milik modul lain (mis. `MstWorkforceProfile`, `FinReceivable*`) juga tidak
        // berganti nama; hanya nama constraint FK mereka yang menunjuk tabel Payroll ikut
        // diperbarui.

        private const string Peta = @"
                'TrxMedicalServiceFeeCalculation', 'PrlMedicalServiceFeeCalculation',
                'TrxMedicalServiceFeePayment', 'PrlMedicalServiceFeePayment',
                'TrxPayrollAdjustment', 'PrlPayrollAdjustment',
                'TrxPayrollApproval', 'PrlPayrollApproval',
                'TrxPayrollAttendanceInput', 'PrlPayrollAttendanceInput',
                'TrxPayrollEmployeeComponent', 'PrlPayrollEmployeeComponent',
                'TrxPayrollOvertimeInput', 'PrlPayrollOvertimeInput',
                'TrxPayrollPayment', 'PrlPayrollPayment',
                'TrxPayrollPayslip', 'PrlPayrollPayslip',
                'TrxPayrollReversal', 'PrlPayrollReversal',
                'TrxPayrollRun', 'PrlPayrollRun',
                'TrxPayrollRunEmployee', 'PrlPayrollRunEmployee',
                'TrxPayrollVariableInput', 'PrlPayrollVariableInput',
                'WfpInsurance', 'PrlInsurance',
                'WfpPayroll', 'PrlPayroll',
                'WfpTax', 'PrlTax',
                'WfpTransportAllowance', 'PrlTransportAllowance',
                'WfpTransportAllowancePolicy', 'PrlTransportAllowancePolicy',
                'WfpTransportAllowanceTransaction', 'PrlTransportAllowanceTransaction'";

        private const string PetaBalik = @"
                'PrlMedicalServiceFeeCalculation', 'TrxMedicalServiceFeeCalculation',
                'PrlMedicalServiceFeePayment', 'TrxMedicalServiceFeePayment',
                'PrlPayrollAdjustment', 'TrxPayrollAdjustment',
                'PrlPayrollApproval', 'TrxPayrollApproval',
                'PrlPayrollAttendanceInput', 'TrxPayrollAttendanceInput',
                'PrlPayrollEmployeeComponent', 'TrxPayrollEmployeeComponent',
                'PrlPayrollOvertimeInput', 'TrxPayrollOvertimeInput',
                'PrlPayrollPayment', 'TrxPayrollPayment',
                'PrlPayrollPayslip', 'TrxPayrollPayslip',
                'PrlPayrollReversal', 'TrxPayrollReversal',
                'PrlPayrollRun', 'TrxPayrollRun',
                'PrlPayrollRunEmployee', 'TrxPayrollRunEmployee',
                'PrlPayrollVariableInput', 'TrxPayrollVariableInput',
                'PrlInsurance', 'WfpInsurance',
                'PrlPayroll', 'WfpPayroll',
                'PrlTax', 'WfpTax',
                'PrlTransportAllowance', 'WfpTransportAllowance',
                'PrlTransportAllowancePolicy', 'WfpTransportAllowancePolicy',
                'PrlTransportAllowanceTransaction', 'WfpTransportAllowanceTransaction'";

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
                -- 0. Pemeriksaan tabrakan. Bila tabel asal dan tabel tujuan sama-sama ada,
                --    ada sesuatu yang tidak beres (mis. migration setengah jalan atau tabel
                --    dibuat manual); berhenti sebelum menimpa apa pun.
                FOR i IN 1 .. array_length(peta, 1) / 2 LOOP
                    lama := peta[i * 2 - 1];
                    baru := peta[i * 2];
                    IF EXISTS (
                        SELECT 1 FROM pg_class c
                        JOIN pg_namespace n ON n.oid = c.relnamespace
                        WHERE n.nspname = 'public' AND c.relname = lama AND c.relkind = 'r'
                    ) AND EXISTS (
                        SELECT 1 FROM pg_class c
                        JOIN pg_namespace n ON n.oid = c.relnamespace
                        WHERE n.nspname = 'public' AND c.relname = baru AND c.relkind = 'r'
                    ) THEN
                        RAISE EXCEPTION 'Tabel % dan % sama-sama ada di skema public; rename dibatalkan agar tidak menimpa data.', lama, baru;
                    END IF;
                END LOOP;

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

                -- 2. Constraint mana pun yang namanya memuat nama tabel lama sebagai token,
                --    termasuk FK milik tabel lain yang menunjuk ke sini dan constraint CHECK.
                --    Mengganti nama constraint PK/unique sekaligus mengganti nama index
                --    penopangnya.
                FOR i IN 1 .. array_length(peta, 1) / 2 LOOP
                    lama := peta[i * 2 - 1];
                    baru := peta[i * 2];
                    FOR r IN
                        SELECT t.relname AS tabel, c.conname AS nama
                        FROM pg_constraint c
                        JOIN pg_class t ON t.oid = c.conrelid
                        JOIN pg_namespace n ON n.oid = t.relnamespace
                        WHERE n.nspname = 'public'
                          AND c.conname ~ ('(^|_)' || lama || '(_|~|$)')
                    LOOP
                        EXECUTE format(
                            'ALTER TABLE public.%I RENAME CONSTRAINT %I TO %I',
                            r.tabel, r.nama,
                            regexp_replace(r.nama, '(^|_)' || lama || '(?=_|~|$)', '\1' || baru, 'g'));
                    END LOOP;
                END LOOP;

                -- 3. Sisa index yang tidak ditopang constraint (index biasa dan unique index
                --    bernama, mis. UX_*).
                FOR i IN 1 .. array_length(peta, 1) / 2 LOOP
                    lama := peta[i * 2 - 1];
                    baru := peta[i * 2];
                    FOR r IN
                        SELECT c.relname AS nama
                        FROM pg_class c
                        JOIN pg_namespace n ON n.oid = c.relnamespace
                        WHERE n.nspname = 'public' AND c.relkind = 'i'
                          AND c.relname ~ ('(^|_)' || lama || '(_|~|$)')
                    LOOP
                        EXECUTE format(
                            'ALTER INDEX public.%I RENAME TO %I',
                            r.nama,
                            regexp_replace(r.nama, '(^|_)' || lama || '(?=_|~|$)', '\1' || baru, 'g'));
                    END LOOP;
                END LOOP;
            END
            $qbe$;
            """;

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(Skrip(Peta));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(Skrip(PetaBalik));
        }
    }
}
