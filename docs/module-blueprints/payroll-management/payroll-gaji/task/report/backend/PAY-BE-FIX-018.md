# Laporan Perubahan Backend — `PAY-BE-FIX-018`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-FIX-018` (ad-hoc lanjutan seri `PAY-BE-FIX`); pasangan `PAY-FE-FIX-018` |
| Judul | Cakupan Grade Gaji, Struktur Gaji, dan Grade Pegawai pada kebijakan tunjangan Shift, Siaga (On Call), dan Risiko, mengikuti kebijakan transportasi |
| Area / Modul | Corporate / HumanResource / MasterData / PayrollAndBenefit (rute `payroll-management/master-data/*-allowance-policies`). Prefix entity `Mst` (legacy, tidak diubah). `TOUCHED LEGACY` |
| Permintaan | Pemilik 23 Nov 2026: "Buat input kebijakan tunjangan lainnya (selain transportasi) mirip input transportasi: Grade Gaji, Struktur Gaji, Grade Pegawai" |
| Klasifikasi | `HEAVY` — 3 entity, 3 konfigurasi, 3 DTO, 3 controller, 1 helper baru, perubahan skema (DDL oleh pemilik) |
| Task mode | `BACKEND` |
| Tanggal | 23 November 2026 |
| Status | 🟡 **Source selesai; belum dibuild.** DDL wajib diterapkan pemilik SEBELUM backend dijalankan (entity kini membaca kolom baru) |

## 1. Keputusan desain

- Tiga kolom baru, semuanya `NULL`, pada `MstShiftAllowancePolicy`, `MstOnCallAllowancePolicy`, `MstHazardAllowancePolicy`: `EmployeeGradeId`, `SalaryGradeId`, `SalaryStructureId`. Data lama tidak diubah dan tidak dimigrasikan; kosong = kebijakan tidak dibatasi pada dimensi itu (perilaku sebelumnya).
- Aturan memakai **satu definisi** yang sama dengan transportasi: `TransportAllowancePolicyScopeRules.ValidateScope` (struktur wajib milik grade, grade/struktur aktif, legal entity/fasilitas sejalan, masa berlaku kebijakan tidak melewati masa berlaku master dan tanggal berakhir wajib bila master punya tanggal berakhir). Pemanggilnya satu helper baru `AllowancePolicyScopeValidator` (HR/PayrollAndBenefit/Services).
- Grade Pegawai hanya diperiksa keberadaan dan keaktifannya.
- **Tidak ikut** (khas transportasi karena sudah punya konsumen): penguncian cakupan oleh penetapan pegawai, larangan mengosongkan grade, cek tumpang tindih, dan aktivasi ulang. Shift/Siaga/Risiko belum punya consumer di mesin payroll (lihat PAY-BE-FIX-012), jadi perubahan ini **tidak mengubah perhitungan gaji**; hanya menyimpan dan memvalidasi cakupan.

## 2. Berkas

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Services/AllowancePolicyScopeValidator.cs` | **Baru** |
| `.../Models/Mst{Shift,OnCall,Hazard}AllowancePolicy.cs` | 3 kolom + 3 navigasi |
| `.../DTOs/{Shift,OnCall,Hazard}AllowancePolicyDtos.cs` | Respons: 3 id + 3 nama; request Create/Update: 3 id |
| `.../Controllers/{Shift,OnCall,Hazard}AllowancePolicyController.cs` | Daftar dan detail memuat id + nama; Create/Update menyimpan; validasi memanggil helper (400 dengan pesan Indonesia) |
| `Repositories/Configurations/.../Mst{Shift,OnCall,Hazard}AllowancePolicyConfiguration.cs` | 3 FK bernama, 1 CHECK, 3 indeks; nama sama dengan DDL |

Tidak ada migration dan tidak ada perubahan `ModelSnapshot` (kebiasaan repo: skema lewat DDL pemilik; `PendingModelChangesWarning` diabaikan).

## 3. DDL (diterapkan pemilik; idempoten, dalam satu transaksi)

Urutan: DDL → backend → frontend. Nama constraint/indeks paling panjang 55 karakter (batas PostgreSQL 63).

```sql
BEGIN;

-- MstShiftAllowancePolicy
ALTER TABLE "public"."MstShiftAllowancePolicy"
    ADD COLUMN IF NOT EXISTS "EmployeeGradeId" uuid NULL,
    ADD COLUMN IF NOT EXISTS "SalaryGradeId" uuid NULL,
    ADD COLUMN IF NOT EXISTS "SalaryStructureId" uuid NULL;

DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_MstShiftAllowancePolicy_EmployeeGrade') THEN
        ALTER TABLE "public"."MstShiftAllowancePolicy" ADD CONSTRAINT "FK_MstShiftAllowancePolicy_EmployeeGrade"
            FOREIGN KEY ("EmployeeGradeId") REFERENCES "public"."MstEmployeeGrade" ("Id") ON DELETE RESTRICT;
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_MstShiftAllowancePolicy_SalaryGrade') THEN
        ALTER TABLE "public"."MstShiftAllowancePolicy" ADD CONSTRAINT "FK_MstShiftAllowancePolicy_SalaryGrade"
            FOREIGN KEY ("SalaryGradeId") REFERENCES "public"."MstSalaryGrade" ("Id") ON DELETE RESTRICT;
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_MstShiftAllowancePolicy_SalaryStructure') THEN
        ALTER TABLE "public"."MstShiftAllowancePolicy" ADD CONSTRAINT "FK_MstShiftAllowancePolicy_SalaryStructure"
            FOREIGN KEY ("SalaryStructureId") REFERENCES "public"."MstSalaryStructure" ("Id") ON DELETE RESTRICT;
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'CK_MstShiftAllowancePolicy_StructureRequiresGrade') THEN
        ALTER TABLE "public"."MstShiftAllowancePolicy" ADD CONSTRAINT "CK_MstShiftAllowancePolicy_StructureRequiresGrade"
            CHECK ("SalaryStructureId" IS NULL OR "SalaryGradeId" IS NOT NULL);
    END IF;
END $$;
CREATE INDEX IF NOT EXISTS "IX_MstShiftAllowancePolicy_SalaryGrade_SalaryStructure" ON "public"."MstShiftAllowancePolicy" ("SalaryGradeId", "SalaryStructureId", "IsActive", "IsDelete");
CREATE INDEX IF NOT EXISTS "IX_MstShiftAllowancePolicy_SalaryStructureId" ON "public"."MstShiftAllowancePolicy" ("SalaryStructureId");
CREATE INDEX IF NOT EXISTS "IX_MstShiftAllowancePolicy_EmployeeGradeId" ON "public"."MstShiftAllowancePolicy" ("EmployeeGradeId");

-- MstOnCallAllowancePolicy
ALTER TABLE "public"."MstOnCallAllowancePolicy"
    ADD COLUMN IF NOT EXISTS "EmployeeGradeId" uuid NULL,
    ADD COLUMN IF NOT EXISTS "SalaryGradeId" uuid NULL,
    ADD COLUMN IF NOT EXISTS "SalaryStructureId" uuid NULL;

DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_MstOnCallAllowancePolicy_EmployeeGrade') THEN
        ALTER TABLE "public"."MstOnCallAllowancePolicy" ADD CONSTRAINT "FK_MstOnCallAllowancePolicy_EmployeeGrade"
            FOREIGN KEY ("EmployeeGradeId") REFERENCES "public"."MstEmployeeGrade" ("Id") ON DELETE RESTRICT;
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_MstOnCallAllowancePolicy_SalaryGrade') THEN
        ALTER TABLE "public"."MstOnCallAllowancePolicy" ADD CONSTRAINT "FK_MstOnCallAllowancePolicy_SalaryGrade"
            FOREIGN KEY ("SalaryGradeId") REFERENCES "public"."MstSalaryGrade" ("Id") ON DELETE RESTRICT;
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_MstOnCallAllowancePolicy_SalaryStructure') THEN
        ALTER TABLE "public"."MstOnCallAllowancePolicy" ADD CONSTRAINT "FK_MstOnCallAllowancePolicy_SalaryStructure"
            FOREIGN KEY ("SalaryStructureId") REFERENCES "public"."MstSalaryStructure" ("Id") ON DELETE RESTRICT;
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'CK_MstOnCallAllowancePolicy_StructureRequiresGrade') THEN
        ALTER TABLE "public"."MstOnCallAllowancePolicy" ADD CONSTRAINT "CK_MstOnCallAllowancePolicy_StructureRequiresGrade"
            CHECK ("SalaryStructureId" IS NULL OR "SalaryGradeId" IS NOT NULL);
    END IF;
END $$;
CREATE INDEX IF NOT EXISTS "IX_MstOnCallAllowancePolicy_SalaryGrade_SalaryStructure" ON "public"."MstOnCallAllowancePolicy" ("SalaryGradeId", "SalaryStructureId", "IsActive", "IsDelete");
CREATE INDEX IF NOT EXISTS "IX_MstOnCallAllowancePolicy_SalaryStructureId" ON "public"."MstOnCallAllowancePolicy" ("SalaryStructureId");
CREATE INDEX IF NOT EXISTS "IX_MstOnCallAllowancePolicy_EmployeeGradeId" ON "public"."MstOnCallAllowancePolicy" ("EmployeeGradeId");

-- MstHazardAllowancePolicy
ALTER TABLE "public"."MstHazardAllowancePolicy"
    ADD COLUMN IF NOT EXISTS "EmployeeGradeId" uuid NULL,
    ADD COLUMN IF NOT EXISTS "SalaryGradeId" uuid NULL,
    ADD COLUMN IF NOT EXISTS "SalaryStructureId" uuid NULL;

DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_MstHazardAllowancePolicy_EmployeeGrade') THEN
        ALTER TABLE "public"."MstHazardAllowancePolicy" ADD CONSTRAINT "FK_MstHazardAllowancePolicy_EmployeeGrade"
            FOREIGN KEY ("EmployeeGradeId") REFERENCES "public"."MstEmployeeGrade" ("Id") ON DELETE RESTRICT;
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_MstHazardAllowancePolicy_SalaryGrade') THEN
        ALTER TABLE "public"."MstHazardAllowancePolicy" ADD CONSTRAINT "FK_MstHazardAllowancePolicy_SalaryGrade"
            FOREIGN KEY ("SalaryGradeId") REFERENCES "public"."MstSalaryGrade" ("Id") ON DELETE RESTRICT;
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_MstHazardAllowancePolicy_SalaryStructure') THEN
        ALTER TABLE "public"."MstHazardAllowancePolicy" ADD CONSTRAINT "FK_MstHazardAllowancePolicy_SalaryStructure"
            FOREIGN KEY ("SalaryStructureId") REFERENCES "public"."MstSalaryStructure" ("Id") ON DELETE RESTRICT;
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'CK_MstHazardAllowancePolicy_StructureRequiresGrade') THEN
        ALTER TABLE "public"."MstHazardAllowancePolicy" ADD CONSTRAINT "CK_MstHazardAllowancePolicy_StructureRequiresGrade"
            CHECK ("SalaryStructureId" IS NULL OR "SalaryGradeId" IS NOT NULL);
    END IF;
END $$;
CREATE INDEX IF NOT EXISTS "IX_MstHazardAllowancePolicy_SalaryGrade_SalaryStructure" ON "public"."MstHazardAllowancePolicy" ("SalaryGradeId", "SalaryStructureId", "IsActive", "IsDelete");
CREATE INDEX IF NOT EXISTS "IX_MstHazardAllowancePolicy_SalaryStructureId" ON "public"."MstHazardAllowancePolicy" ("SalaryStructureId");
CREATE INDEX IF NOT EXISTS "IX_MstHazardAllowancePolicy_EmployeeGradeId" ON "public"."MstHazardAllowancePolicy" ("EmployeeGradeId");

COMMIT;
```

Verifikasi sesudah DDL (harus mengembalikan 9):

```sql
SELECT count(*) FROM information_schema.columns WHERE table_schema = 'public'
  AND table_name IN ('MstShiftAllowancePolicy', 'MstOnCallAllowancePolicy', 'MstHazardAllowancePolicy')
  AND column_name IN ('EmployeeGradeId', 'SalaryGradeId', 'SalaryStructureId');
```

## 4. Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| `dotnet restore` / `dotnet build` | `NOT RUN` (dilarang). **Kompilasi belum dibuktikan** |
| Tes teks pada repo FE `payroll-allowance-policy-scope.test.mjs` | Lihat laporan `PAY-FE-FIX-018` (mencocokkan teks source BE dan DDL ini; bukan eksekusi C#) |
| Uji runtime / DDL terbukti diterapkan | **Belum terpenuhi** |

## 5. Risiko

- Backend dijalankan **sebelum** DDL: query ketiga kebijakan gagal (kolom tidak ada). Terapkan DDL dulu.
- `Include` pada detail dan proyeksi nama pada daftar menambah join pada tiga tabel master kecil; dampak performa diharapkan kecil.
- Pesan masa berlaku datang sebagai kalimat 400; frontend memetakannya ke field tanggal (PAY-FE-FIX-017).
