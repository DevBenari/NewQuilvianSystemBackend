# Laporan Audit dan Desain — `PAY-BE-FIX-013`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-FIX-013` (ad-hoc lanjutan seri `PAY-BE-FIX`). Laporan ini mencakup juga sisi frontend; **tidak ada `PAY-FE-FIX-013`** karena tidak ada source frontend yang ditulis |
| Judul | Grade Gaji dan Struktur Gaji pada Master Kebijakan Tunjangan Transportasi, disambungkan dengan keputusan D1 (arsitektur kebijakan umum) |
| Slice | Corporate / Payroll Management (`Prl`), Master Data Payroll and Benefit (`Mst`), Frontend Payroll |
| Roadmap | Tidak ada baris roadmap untuk seri ad-hoc; tidak ada roadmap atau traceability yang diubah (`NOT APPLICABLE`) |
| Klasifikasi | **`EPIC`** — skor 14 (dua repository: 2; berkas diperiksa >20: 2; logika kompleks: 2; kontrak API berubah: 2; schema berubah: 2; keamanan berkaitan: 1; UI banyak halaman: 2; berkas diubah 0: 0; dampak kalkulasi uang ditandai terpisah). Dipecah per fase (bagian 9) |
| Task mode | `CROSS-REPO`, **mode audit dan desain** |
| Target tulis | **Tidak ada source** pada backend maupun frontend. Hanya berkas laporan ini |
| Commit saat dikerjakan | Backend `d642a9df9de35801ed8332adc0cdcd6127f96353` (`Yasmina`); frontend `31df72b9d907796eb106c638a2a350405c866efa` (`yasmina`). Working tree **tidak bersih**: memuat pekerjaan `PAY-BE/FE-FIX-010`, `011`, dan `012` serta pekerjaan pihak lain |
| Model | Claude Sonnet 5.5 |
| Tanggal | 20 November 2026 |
| Status | ⛔ **`BLOCKED_BY_SCHEMA`.** Kolom yang dibutuhkan terbukti belum ada (bagian 1.2). Desain, rencana database, kontrak, dan keputusan sudah lengkap; implementasi menunggu |

## Pemisahan status pekerjaan

| Kategori | Isi |
| --- | --- |
| **Sudah diimplementasikan** | **Tidak ada source.** Audit as-is seluruh rantai Grade Gaji → Struktur Gaji → Kebijakan → Penetapan → Payroll |
| **Baru berupa desain** | Relasi data, rencana database, validasi, kontrak API, perubahan kalkulasi, dropdown bertingkat, filter, tampilan |
| **Belum diverifikasi** | Seluruh perilaku runtime; apakah kolom sudah ada di database target (hanya riwayat migration dan snapshot yang dibaca) |
| **Terblokir karena schema** | Semua yang menulis, memfilter, atau menghitung berdasarkan `SalaryGradeId`/`SalaryStructureId` pada kebijakan transport |
| **Membutuhkan keputusan lanjutan** | Bagian 8 (enam keputusan prompt, ditambah satu keputusan baru **D9**) |

---

## 0. Menyambungkan keputusan D1 dengan prompt ini

**Cara saya membaca D1.** D1 pada `PAY-BE-FIX-012` berbunyi: izin merancang dan membuat migration untuk Opsi A (tabel kebijakan umum + tarif per grade), serta penetapan prefix. Saya membacanya sebagai **persetujuan arah Opsi A dan usulan prefix `Prl`**. Prompt ini secara eksplisit melarang membuat atau menjalankan migration, sehingga saya **tidak** membuat file migration; wewenang itu perlu dikonfirmasi terpisah (bagian 8, pertanyaan 1). **Koreksi dan pembaruan 20 November 2026:** (a) pemilik menjawab bahwa **migrasi dijalankan sendiri**, sehingga tidak ada file migration yang dibuat; (b) baris registry `Prl` ternyata **sudah terdaftar `ACTIVE` sejak 2026-11-11** pada registry backend yang dibaca pemeriksa QBE, jadi kalimat awal ("tidak saya tulis karena registry di repository lain") bertumpu pada pembacaan salinan yang tertinggal. Atas instruksi pemilik, baris identik kini ditambahkan ke salinan kanonik dan mirror di `QuilvianEngineeringSkills`.

**Prompt ini bukan Opsi A.** Ia menambah cakupan grade dan struktur pada tabel transport yang sudah ada, yang pada `PAY-BE-FIX-012` saya nilai sebagai Opsi B (tetap bernama transport, mempertahankan jalur khusus). Analisis penyambungannya:

| Hal | Hasil analisis |
| --- | --- |
| Apakah keduanya bertentangan? | Tidak, **bila** dua kolom baru dirancang sebagai *cakupan* yang berpindah 1:1 ke Opsi A. Pada Opsi A, tabel transport menjadi `LEGACY_PERSISTED` dan barisnya dipindahkan |
| Apakah prompt ini membatalkan "hilangkan jalur Transportasi khusus"? | Tidak membatalkan, tetapi **menambah investasi** pada jalur khusus. Ini risiko yang perlu keputusan sadar (D9) |
| **Koreksi atas desain saya kemarin** | Desain Opsi A memberi tarif per `SalaryGradeId` saja. Audit ini menunjukkan struktur gaji **milik tepat satu grade** dan punya cakupan sendiri (legal entity, lokasi, unit organisasi, kategori pegawai, jenis hubungan kerja). Satu grade dapat punya banyak struktur, sehingga tarif per grade saja **tidak membedakan struktur**. `PrlAllowancePolicyGradeRate` perlu kolom `SalaryStructureId?` (null = semua struktur dalam grade itu, mengikuti keputusan nomor 1) |

Pemetaan kolom bila Opsi A kelak menggantikan tabel transport:

| Tabel transport (sekarang / paket 1) | Opsi A |
| --- | --- |
| `SalaryGradeId`, `SalaryStructureId` (baru) | `PrlAllowancePolicyGradeRate.SalaryGradeId`, `SalaryStructureId?` |
| `EmployeeGradeId`, `LegalEntityId`, `HospitalSiteId` | Cakupan header `PrlAllowancePolicy` |
| `FixedMonthlyAmount` / `MonthlyLimitAmount` | `Amount` / `MaximumAmount` pada tarif |
| `IsProrated`, `IsTaxable`, `IsIncludedInPayroll` | Snapshot dari master atau kebijakan (D6) |
| `PerAttendanceAmount`, `PerDay`, `Reimbursement` | **Tidak pindah**; tetap kelompok aktivitas (D2) |

**Rekomendasi D9 (keputusan baru): dua paket skema.**

| Paket | Isi | Kapan |
| --- | --- | --- |
| **Paket 1** | Dua kolom nullable, FK, dan indeks pada `PrlTransportAllowancePolicy` (bagian 3). DDL kecil, tidak mengubah data | Bila kebutuhan transport per struktur mendesak |
| **Paket 2** | Tabel kebijakan umum + tarif (Opsi A, dengan `SalaryStructureId?`) dan migrasi data transport | Setelah Paket 1 stabil dan keputusan D2/D3 |

Bila kebutuhan tidak mendesak, **langsung ke Paket 2** menghindari investasi ganda pada jalur khusus. Keduanya memerlukan wewenang migration.

---

## 1. Analisis as-is (ROOT CAUSE)

### 1.1 Rantai data

```text
MstEmployeeGrade <── MstSalaryGrade (EmployeeGradeId, nullable)
                          └── MstSalaryStructure (SalaryGradeId, WAJIB; + LegalEntity, HospitalSite, OrganizationUnit,
                                                   EmployeeCategory, EmploymentType, EffectiveStart/End)
WfpSalaryAssignment (SalaryStructureId, SalaryGradeId, EmployeeGradeId, IsPrimary, masa berlaku)
PrlTransportAllowancePolicy (LegalEntity?, HospitalSite?, EmployeeGrade?, PayrollComponent?, metode, nominal, masa berlaku)
      └── PrlTransportAllowance (penetapan pegawai; TransportAllowancePolicyId OPSIONAL; nominal sendiri)
            └── PrlTransportAllowanceTransaction
PayrollCalculationService.BuildTransportAllowanceLines
```

### 1.2 Kolom yang dibutuhkan tidak ada

Blok entity `PrlTransportAllowancePolicy` pada `ApplicationDbContextModelSnapshot.cs` memuat `EmployeeGradeId`, `LegalEntityId`, `HospitalSiteId`, `PayrollComponentId`, nominal, dan masa berlaku, **tanpa** `SalaryGradeId` dan `SalaryStructureId`. Dua berkas migration yang menyebut tabel itu (`20261111100000_RenamePayrollTrxAndWfpTablesToPrlPrefix` dan `20261118035706_AddPayrollStatutoryPolicyTaxInsuranceAndWorkforceAllowance`) serta snapshot tidak menambahkannya. Konfigurasi (`PrlTransportAllowancePolicyConfiguration.cs`) hanya memuat FK ke `LegalEntity`, `HospitalSite`, `EmployeeGrade`, `PayrollComponent` (semua `Restrict`) dan tiga indeks. **Persistensi tidak tersedia.**

### 1.3 Tiga konsep yang berbeda

| Konsep | Sumber | Catatan |
| --- | --- | --- |
| `EmployeeGradeId` | `MstEmployeeGrade` (kepegawaian) | Cakupan kebijakan yang sudah ada |
| `SalaryGradeId` | `MstSalaryGrade` (penggajian) | Punya `EmployeeGradeId` **opsional**; tidak semua grade gaji terhubung ke grade pegawai |
| `SalaryStructureId` | `MstSalaryStructure` | Milik **tepat satu** grade gaji. Karena struktur menentukan grade, menyimpan keduanya adalah denormalisasi yang wajib dijaga konsistensinya |

### 1.4 Perilaku existing yang bermasalah terhadap tujuan prompt

| No | Temuan | Bukti | Dampak |
| ---: | --- | --- | --- |
| 1 | Opsi kebijakan menerjemahkan `salaryGradeId` menjadi `EmployeeGradeId` lewat `MstSalaryGrade.EmployeeGradeId`. Bila grade itu tidak punya grade pegawai, **tidak ada filter sama sekali** dan semua kebijakan muncul | `WfpTransportAllowancePolicyController.cs` baris 194-206 | Pemilihan "kebijakan sesuai grade" tidak dapat diandalkan |
| 2 | Kebijakan dengan `EmployeeGradeId == null` **muncul untuk setiap grade** | Baris 222 | Data legacy tanpa cakupan sudah dianggap berlaku global pada opsi. Bertentangan dengan permintaan "jangan otomatis global"; mengubahnya adalah keputusan (nomor 2) |
| 3 | Modal penetapan pegawai memanggil `PayrollTransportAllowanceService.policyOptions(controller.signal)` **tanpa parameter** | `use-payroll-transport-allowance.js` baris 105 | Semua kebijakan aktif ditawarkan untuk semua pegawai; tidak ada konteks penetapan gaji |
| 4 | Validasi penetapan pegawai hanya memeriksa kebijakan ada dan aktif; `TransportAllowancePolicyId` boleh kosong; nominal penetapan tidak dicocokkan dengan kebijakan | `WfpTransportAllowanceController.cs` baris 712-720; entity `PrlTransportAllowance` | Kebijakan struktur A dapat ditetapkan kepada pegawai berstruktur B |
| 5 | **Kalkulasi tidak membaca segmen penetapan gaji.** `BuildTransportAllowanceLines` hanya menyaring kebijakan aktif, `IsIncludedInPayroll`, dan `FixedMonthly`, lalu memakai masa berlaku penetapan transport | `PayrollCalculationService.cs` baris 1370-1459 | Tidak ada pemeriksaan grade atau struktur saat Payroll; perubahan struktur di tengah bulan tidak berpengaruh |
| 6 | Nominal bulanan = `MonthlyAmount` penetapan bila > 0, selain itu `FixedMonthlyAmount` kebijakan; dibatasi `MonthlyLimitAmount` kebijakan | Baris 1418-1419 | Nominal penetapan **menimpa** kebijakan (sampai batas) |
| 7 | Kebijakan hanya dicegah duplikat **nama**; tidak ada deteksi irisan cakupan dan masa berlaku | Baris 806-811 | Dua kebijakan beririsan tidak ditolak |
| 8 | `Delete` hanya menolak bila ada penetapan pegawai yang merujuknya; ubah dan status tidak memeriksa pemakaian payroll | Baris 528-535 | Perubahan nominal kebijakan memengaruhi perhitungan ulang run yang belum final |

**Yang sudah benar dan dipertahankan:** `FixedMonthly` dihitung otomatis; `PerAttendance`, `PerDay`, `Reimbursement` hanya memberi peringatan (baris 1388-1392); dua penetapan bulanan yang beririsan menggagalkan pegawai (baris 1400-1410); prorata menurut `IsProrated`; `IsTaxable` diteruskan ke dasar pajak.

### 1.5 Risiko ganda (belum terbukti)

Komponen transport dapat muncul dari: Struktur Gaji (`StructureAllowance`), penetapan transport (`TransportAllowance`), tunjangan pegawai (`WorkforceAllowance`, dilewati bila kategori Transport), dan Penghasilan Variabel (`VariableEarning`). Penjaga yang ada hanya melindungi sebagian: struktur terhadap tunjangan pegawai, dan kategori Transport terhadap tunjangan pegawai. **Tidak ada penjaga antara `TransportAllowance` dan `StructureAllowance` bertipe Transport, atau antara `TransportAllowance` dan `VariableEarning`.** Peringatan transport menyuruh pengguna "masukkan lewat Penghasilan Variabel", yang bisa menumpuk dengan `FixedMonthly` bila keduanya diisi.

---

## 2. Desain relasi data (DESAIN)

Pada `PrlTransportAllowancePolicy` (Paket 1):

| Kolom | Tipe | Aturan |
| --- | --- | --- |
| `SalaryGradeId` | `uuid`, **nullable** | FK ke `MstSalaryGrade(Id)`, `Restrict` |
| `SalaryStructureId` | `uuid`, **nullable** | FK ke `MstSalaryStructure(Id)`, `Restrict` |

Aturan keutuhan:

1. `SalaryStructureId` terisi mengharuskan `SalaryGradeId` terisi (check constraint).
2. `Structure.SalaryGradeId == Policy.SalaryGradeId`. Diperiksa **service pada Create dan Update** (permintaan yang dimanipulasi ditolak 400). *Hardening opsional:* kunci komposit `(SalaryStructureId, SalaryGradeId)` ke `MstSalaryStructure(Id, SalaryGradeId)` sehingga database sendiri menolak pasangan yang tidak cocok; membutuhkan indeks unik tambahan pada tabel master struktur, dan itu keputusan terpisah.
3. Baris legacy tetap `null`/`null`: **tidak dimigrasikan, tidak di-backfill**.

`ScopeStatus` (turunan, tidak disimpan): `Scoped` (grade dan struktur), `GradeScoped` (grade saja, hanya bila keputusan nomor 1 mengizinkan), `LegacyUnscoped` (keduanya kosong).

Indeks yang diusulkan: `(SalaryGradeId, SalaryStructureId, IsActive, IsDelete)` untuk filter dan resolusi; `(SalaryStructureId)` untuk pemeriksaan pemakaian master.

Relasi ke cakupan yang ada: `LegalEntityId`, `HospitalSiteId`, `EmployeeGradeId` **dipertahankan** sebagai cakupan tambahan. Struktur juga punya `LegalEntityId` dan `HospitalSiteId` sendiri; kebijakan yang menyebut cakupan berbeda dari struktur yang dipilih harus ditolak (lihat aturan 6 di bagian 4).

---

## 3. Rencana database (Paket 1) — bukan migration, tidak dijalankan

Usulan DDL idempoten (pola `IF NOT EXISTS` seperti migration SDM manual yang sudah ada). **Bukan file migration, tidak dieksekusi, dan tidak ada perubahan pada `Migrations/` atau snapshot.**

> **Koreksi 20 November 2026 (nama identifier).** Versi awal DDL ini memakai dua nama yang melebihi batas 63 karakter PostgreSQL (nama indeks 80 karakter dan nama foreign key 67 karakter). PostgreSQL memotongnya diam-diam, sehingga nama di database tidak akan cocok dengan nama pada model EF. Semua nama di bawah sudah diganti dengan nama eksplisit yang diukur panjangnya (terpanjang 58 karakter) dan **dipasang dengan nama yang sama pada konfigurasi EF** (`HasConstraintName`, `HasDatabaseName`). Bila Anda sempat menyalin DDL versi awal, jangan dipakai.

```sql
ALTER TABLE public."PrlTransportAllowancePolicy"
    ADD COLUMN IF NOT EXISTS "SalaryGradeId" uuid NULL,
    ADD COLUMN IF NOT EXISTS "SalaryStructureId" uuid NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_PrlTransportAllowancePolicy_SalaryGrade') THEN
        ALTER TABLE public."PrlTransportAllowancePolicy"
            ADD CONSTRAINT "FK_PrlTransportAllowancePolicy_SalaryGrade"
            FOREIGN KEY ("SalaryGradeId") REFERENCES public."MstSalaryGrade" ("Id") ON DELETE RESTRICT;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_PrlTransportAllowancePolicy_SalaryStructure') THEN
        ALTER TABLE public."PrlTransportAllowancePolicy"
            ADD CONSTRAINT "FK_PrlTransportAllowancePolicy_SalaryStructure"
            FOREIGN KEY ("SalaryStructureId") REFERENCES public."MstSalaryStructure" ("Id") ON DELETE RESTRICT;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'CK_PrlTransportAllowancePolicy_StructureRequiresGrade') THEN
        ALTER TABLE public."PrlTransportAllowancePolicy"
            ADD CONSTRAINT "CK_PrlTransportAllowancePolicy_StructureRequiresGrade"
            CHECK ("SalaryStructureId" IS NULL OR "SalaryGradeId" IS NOT NULL);
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS "IX_PrlTransportAllowancePolicy_SalaryGrade_SalaryStructure"
    ON public."PrlTransportAllowancePolicy" ("SalaryGradeId", "SalaryStructureId", "IsActive", "IsDelete");
CREATE INDEX IF NOT EXISTS "IX_PrlTransportAllowancePolicy_SalaryStructureId"
    ON public."PrlTransportAllowancePolicy" ("SalaryStructureId");
```

| Aspek | Penilaian |
| --- | --- |
| Data legacy | Tidak disentuh; kolom baru `null` |
| Kunci tabel | `ADD COLUMN` nullable tanpa default tidak menulis ulang tabel; ringan |
| Pelajaran dari kejadian hari ini | Insiden `42P07` pada `HrdOvertimeSetting` terjadi karena scaffold membuat ulang tabel yang sudah ada. Bila DDL ini diterapkan manual, migration yang kelak dibuat harus **idempoten** atau ditandai terapan, dan snapshot harus selaras, agar tidak terulang |
| Kesiapan persistensi | **Tidak siap** sampai DDL ini diterapkan di database target dan entity dipetakan |
| Urutan rilis | DDL lebih dulu, baru backend; backend yang memetakan kolom ini **gagal membaca tabel** (`42703`) bila dideploy sebelum DDL, termasuk kalkulasi Payroll |

---

## 4. Rancangan backend (DESAIN; tidak diterapkan)

**Entity dan konfigurasi.** Tambah `SalaryGradeId?`, `SalaryStructureId?`, navigasi `SalaryGrade`, `SalaryStructure` pada `PrlTransportAllowancePolicy`; FK `Restrict` dan dua indeks pada `PrlTransportAllowancePolicyConfiguration`.

**Validasi (Create dan Update, satu fungsi bersama; QBE-VAL-001):**

1. Grade ditemukan, aktif, belum dihapus. 2. Struktur ditemukan, aktif, belum dihapus. 3. `Structure.SalaryGradeId == SalaryGradeId`. 4. Masa berlaku kebijakan berada **di dalam** masa berlaku struktur dan grade (bila punya tanggal), sehingga tanggal kebijakan tidak membenarkan struktur yang tidak berlaku. 5. Struktur tanpa grade ditolak. 6. `LegalEntityId`/`HospitalSiteId` kebijakan, bila diisi, tidak boleh bertentangan dengan cakupan struktur. 7. Kebijakan baru **wajib** memilih pasangan lengkap bila keputusan nomor 1 menetapkannya wajib; sampai diputuskan, grade tanpa struktur diterima sebagai `GradeScoped`.

Semantik status keabsahan master dapat memakai `PayrollSalaryMasterValidity` yang dibawa `PAY-BE-FIX-011` (nilai `Valid`, `StructureInactive`, `GradeInactive`, `StructureNotYetEffective`, `StructureExpired`) agar konsisten; itu pekerjaan pihak lain yang belum dikomit, jadi hanya dirujuk.

**Endpoint (tidak ada endpoint baru):**

| Endpoint | Perubahan |
| --- | --- |
| `GET /` (daftar) | Filter `salaryGradeId`, `salaryStructureId` terhadap nilai **tersimpan**; respons + `salaryGradeId/Name`, `salaryStructureId/Name`, `scopeStatus` |
| `GET /options` | Tambah `salaryStructureId`, `workforceProfileId`, `effectiveDate`. Bila `workforceProfileId` ada, backend membaca penetapan gaji utama yang berlaku pada `effectiveDate` dan memfilter. Parameter `salaryGradeId` lama **dipertahankan** untuk konsumen existing (jalur terjemahan ke `EmployeeGradeId`) dan hanya berlaku pada kebijakan legacy; kebijakan ber-cakupan memakai nilai tersimpan |
| `GET /{id}` | + dua id, dua nama, `scopeStatus` |
| `POST`, `PUT` | Terima dua id; validasi di atas; `PUT` yang mengubah cakupan kebijakan yang **sudah dirujuk penetapan pegawai** ditolak 409 (buat kebijakan baru/revisi) agar kelayakan penetapan lama tidak berubah diam-diam |
| `PATCH /{id}/status` | Mengaktifkan kebijakan ber-cakupan memeriksa ulang validasi; tidak mengaktifkan relasi tidak valid |
| `DELETE /{id}` | Tidak berubah (ditolak bila dirujuk penetapan) |
| `GET /filters/metadata` | + `CreateFields`/`UpdateFields`/`QueryParameters` untuk dua id |

**Irisan kebijakan.** Tolak 409 untuk kebijakan aktif dengan cakupan **persis sama** (legal entity, lokasi, grade pegawai, grade gaji, struktur) dan masa berlaku beririsan. Cakupan yang saling melingkupi (grade saja versus grade+struktur) **tidak** diberi prioritas otomatis; pengguna memilih eksplisit pada penetapan. Aturan prioritas menunggu keputusan nomor 4.

**Penetapan pegawai (`WfpTransportAllowanceController`).** Pada Create dan Update: bila kebijakan ber-cakupan, backend membaca penetapan gaji utama yang berlaku untuk **seluruh** masa berlaku penetapan transport; semua segmen harus cocok dengan grade dan struktur kebijakan, bila tidak ditolak dengan pesan yang menyebut tanggal yang bentrok. Bila pegawai berganti grade atau struktur, penetapan yang tidak lagi cocok ditandai untuk ditinjau (tidak diubah otomatis). `TransportAllowancePolicyId` tetap opsional untuk data lama.

**Payroll (`BuildTransportAllowanceLines`).** Saat ini tidak menerima segmen gaji. Desain: terima `salarySegments`; untuk kebijakan ber-cakupan, irisan jendela penetapan dengan **segmen yang cocok** sajalah yang dibayar (`hari cocok / hari periode` bila `IsProrated`). Segmen yang tidak cocok dilewati dengan peringatan, jadi kebijakan struktur lama tidak berlaku setelah struktur baru berlaku. Tanpa prorata, pembayaran penuh hanya bila pasangan cocok pada **hari terakhir** jendela. Kebijakan legacy `null`/`null` **tidak berubah**. Segmen saling lepas, sehingga jumlah tidak melebihi satu kali nominal bulanan. `FixedMonthly` tetap satu-satunya metode otomatis. Ini perubahan **jalur uang** (`HEAVY`) dan perlu persetujuan hasil perhitungan sebelum dirilis.

---

## 5. Rancangan frontend (DESAIN; tidak diterapkan)

Mekanisme yang **sudah ada dan dipakai ulang**: registry `salaryGrades` dan `salaryStructures` (`hr-select-resources.js`), pemetaan otomatis `SELECT_RESOURCE_CASCADE.salaryStructures = { salaryGradeId: ["salaryGradeId"] }`, dan dukungan `dependsOn` pada `base-editor-form.jsx` (kolom child nonaktif sampai induk terisi). Tidak ada komponen dropdown baru.

| Bagian | Perubahan |
| --- | --- |
| Konfigurasi field | Tambah `salaryGradeId` (`optionResource: "salaryGrades"`) dan `salaryStructureId` (`optionResource: "salaryStructures"`, `dependsOn: "salaryGradeId"`, `optionParam: "salaryGradeId"`) pada `CREATE_FIELDS`; urutan: Grade Gaji, Struktur Gaji, Grade Pegawai, Komponen Payroll. Grade Pegawai tetap |
| Perubahan induk | `transformFormChange`: `if (name === "salaryGradeId") next.salaryStructureId = ""` (sudah ada pola yang sama untuk `legalEntityId` → `hospitalSiteId`) dan label struktur lama dihapus |
| Placeholder dan kosong | "Pilih Grade Gaji terlebih dahulu"; kosong: "Tidak ada Struktur Gaji aktif untuk Grade Gaji ini." |
| Mode Update | Isi dari `salaryGradeId/Name` dan `salaryStructureId/Name` respons agar nilai tampil sebelum opsi selesai dimuat; jangan mereset nilai existing saat opsi belum ada; master nonaktif tampil sebagai informasi historis |
| Permintaan basi | `base-editor-field.jsx` memakai `useSelectResource` dengan `keepPreviousOptions: true` dan `refetchOnOpen`. **Pembatalan atau pengabaian respons lama saat induk berganti belum saya verifikasi** pada `use-select-resource.jsx`; wajib dibuktikan saat implementasi, karena tuntutan prompt ini mengharuskannya |
| Payload dan validasi | `buildPayload`/`validateForm`/`mapToForm`/`buildInitialForm` memuat dua id; pasangan dikirim lengkap |
| Daftar | Kolom Grade Gaji dan Struktur Gaji (`config.listColumns`); filter Grade Gaji dan Struktur Gaji saling terikat (filter struktur direset saat filter grade berubah); daftarkan di `supportedFilterKeys` hanya setelah backend mendukungnya |
| Detail | Baris Grade Gaji dan Struktur Gaji pada `detailFields`, bersama cakupan Grade Pegawai |
| Penetapan pegawai | `policyOptions` dipanggil **dengan** `workforceProfileId` dan tanggal efektif; pesan bila tidak ada kebijakan sesuai; backend tetap memvalidasi |

**Tidak diaktifkan sebelum kolom tersedia:** semua perubahan di atas yang menulis atau memfilter dua id.

---

## 6. Alur bisnis

```text
Salary Assignment utama pegawai (SalaryStructureId, SalaryGradeId, masa berlaku)
      │ menentukan cakupan kebijakan yang boleh dipilih
      ▼
Kebijakan Transportasi (Grade Gaji + Struktur Gaji [+ Grade Pegawai, Legal Entity, Lokasi])
      │ pegawai TIDAK otomatis menerima; hak lahir dari penetapan
      ▼
Penetapan Transportasi Pegawai (PrlTransportAllowance) ── divalidasi backend terhadap segmen penetapan gaji
      ▼
Payroll: hanya segmen yang cocok dibayar; FixedMonthly saja; satu sumber transport per periode
      ▼
Komponen dan snapshot run; run final tidak diubah
```

Model penetapan existing **dipertahankan**: tidak ada pembayaran otomatis kepada semua pegawai satu grade atau struktur tanpa keputusan pemilik proses.

---

## 7. Hasil static review (22 skenario UAT)

Status: **SRP** = Static Review Passed (kode dibaca), **NV** = Not Verified, **BLOCKED** = `BLOCKED_BY_SCHEMA`. Tidak ada yang dijalankan; "SRP" bukan hasil uji runtime.

| UAT | Skenario | Status | Catatan |
| ---: | --- | --- | --- |
| 01 | Form Create menampilkan Grade Gaji dan Struktur Gaji | BLOCKED | Bergantung kolom |
| 02 | Struktur nonaktif sebelum grade dipilih | BLOCKED | Mekanisme `dependsOn` ada; field belum ditambahkan |
| 03 | Hanya struktur terkait yang muncul | BLOCKED | Filter `salaryGradeId` pada registry ada; field belum ada |
| 04 | Ganti grade me-reset struktur | BLOCKED | Pola reset ada (`legalEntityId`); belum diterapkan |
| 05 | Pencarian, paginasi, loading, kosong | NV | Didukung registry; belum dipakai |
| 06 | Update memuat grade dan struktur existing | BLOCKED | |
| 07 | API menolak kombinasi tidak sesuai | BLOCKED | |
| 08 | API menolak master nonaktif | BLOCKED | |
| 09 | Tabel menampilkan nama grade dan struktur | BLOCKED | |
| 10 | Filter grade dan struktur | BLOCKED | |
| 11 | Data legacy tidak dihapus atau dimigrasikan diam-diam | SRP | Tidak ada perubahan; rancangan menjaga `null` |
| 12 | Grade Pegawai tetap atribut terpisah | SRP | Rancangan mempertahankan `EmployeeGradeId` |
| 13 | Kebijakan struktur A tidak dapat ditetapkan ke pegawai struktur B | BLOCKED | Temuan 4: saat ini **tidak ditolak** |
| 14 | Kebijakan tidak sesuai tidak dibayar | BLOCKED | Temuan 5: saat ini **tidak diperiksa** |
| 15 | Perubahan grade/struktur tengah periode mengikuti tanggal efektif | BLOCKED | Kalkulasi belum membaca segmen |
| 16 | Tidak ada transport ganda pada Payroll Run | SRP (sebagian) | Irisan antar penetapan transport menggagalkan pegawai (baris 1400-1410). Lintas sumber (struktur, variabel) **NV**, lihat 1.5 |
| 17 | `FixedMonthly` sesuai aturan existing | SRP | Baris 1418-1445 dibaca; tidak diubah |
| 18 | Metode lain tidak otomatis | SRP | Baris 1388-1392 hanya memberi peringatan |
| 19 | Perubahan master tidak merusak run final | NV | Snapshot run final tidak diverifikasi |
| 20 | Tanpa izin tidak dapat menulis kebijakan | SRP (sebagian) | `[AccessPermission("TransportAllowancePolicy", ...)]` terbaca pada aksi `options` dan `Delete`; aksi `POST`, `PUT`, dan `PATCH` tidak saya baca atributnya satu per satu (**NV**) |
| 21 | Hapus kebijakan terpakai mengikuti pembatasan existing | SRP | `Delete` baris 528-535 |
| 22 | Endpoint dan form tidak menulis field baru sebelum schema ada | SRP | Tidak ada source yang ditulis |

---

## 8. Keputusan yang dibutuhkan

Pertanyaan yang harus dijawab lebih dulu: **(1)** apakah persetujuan D1 mencakup **pembuatan file migration** untuk Paket 1 (prompt ini melarangnya, sehingga saya menunggu konfirmasi eksplisit), atau Anda akan menerapkan DDL pada bagian 3 sendiri; **(2)** apakah baris registry `Prl` boleh saya tulis, dan ke repository mana. **Dijawab 20 Nov 2026:** pemilik menerapkan migrasi sendiri, dan baris registry sudah ditulis ke salinan kanonik (lihat koreksi pada bagian 0).

| No | Keputusan | Desain konservatif yang dipakai sementara |
| ---: | --- | --- |
| 1 | Kebijakan baru wajib grade **dan** struktur, atau grade saja boleh | Diterima `GradeScoped`; pasangan lengkap dianjurkan |
| 2 | Kebijakan legacy tanpa grade/struktur | Perilaku existing **dipertahankan** (tetap muncul, ditandai `LegacyUnscoped`), tidak dimigrasikan, tidak diperlakukan sebagai hak otomatis |
| 3 | Pegawai otomatis menerima transport per struktur, atau tetap perlu `PrlTransportAllowance` | Tetap perlu penetapan (model existing) |
| 4 | Prioritas bila cakupan beririsan | Tidak ada prioritas otomatis; pengguna memilih; hanya cakupan persis sama yang ditolak |
| 5 | Perubahan kebijakan setelah dipakai Payroll final | Cakupan tidak boleh diubah bila dirujuk penetapan; nominal berlaku untuk perhitungan berikutnya; run final tidak diubah |
| 6 | Prorata transport per segmen saat grade/struktur berubah di tengah bulan | Prorata per segmen yang cocok; tanpa prorata bayar penuh hanya bila cocok pada hari terakhir jendela |
| **D9** | **Paket 1 (jembatan) atau langsung Paket 2 (Opsi A dengan `SalaryStructureId?`)** | Rekomendasi: langsung Paket 2 bila tidak mendesak; Paket 1 bila transport per struktur dibutuhkan segera |

## 9. Dekomposisi (aturan EPIC)

| Fase | Isi | Klasifikasi | Status |
| --- | --- | --- | --- |
| F0 | Audit dan desain (laporan ini) | `MEDIUM` | Selesai |
| F1 | Keputusan D9, nomor 1-6, dan wewenang migration | n/a | Menunggu |
| F2 | DDL Paket 1 diterapkan di database target | n/a | `BLOCKED_BY_SCHEMA` |
| F3 | Backend: entity, konfigurasi, DTO, validasi, CRUD, options | `HEAVY` | Setelah F2 |
| F4 | Backend: validasi penetapan pegawai terhadap penetapan gaji | `HEAVY` | Setelah F3 |
| F5 | Frontend: dropdown bertingkat, daftar, detail, filter | `HEAVY` | Setelah F3 |
| F6 | Kalkulasi: `BuildTransportAllowanceLines` per segmen | `HEAVY` (jalur uang) | Setelah F4 dan keputusan 5-6 |
| F7 | Celah ganda lintas sumber (1.5) | `MEDIUM` | Perlu keputusan prioritas |

## 10. Risiko residual

| Risiko | Penjelasan |
| --- | --- |
| Backend dideploy sebelum DDL | Memetakan kolom yang belum ada menggagalkan pembacaan tabel kebijakan, termasuk Payroll |
| Investasi ganda | Paket 1 menambah kode pada jalur transport yang akan dipensiunkan |
| Perubahan jalur uang | F6 mengubah apa yang dibayar; wajib diuji dengan data nyata sebelum dirilis |
| Pekerjaan paralel | `PAY-BE-FIX-011` dan perubahan frontend pihak lain pada working tree yang sama; tidak disentuh |

## 10a. Catatan registry: tiga salinan dan selisihnya (ditambahkan 20 November 2026)

Registry kepemilikan dan prefix ada dalam **tiga salinan**, dan ketiganya tidak sama:

| Salinan | Fungsi | Keadaan |
| --- | --- | --- |
| `QuilvianEngineeringSkills/agents/rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Kanonik repository skills | Terakhir diubah 2026-09-22 (commit `07daef5`); kini ditambah baris `Prl` |
| `QuilvianEngineeringSkills/Claude/.claude/rules/backend/engineering/...` | Mirror hasil generate | Disinkronkan lewat `tooling/sync-rules.ps1`; identik dengan kanonik |
| `NewQuilvianSystemBackend/docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | **Dibaca pemeriksa QBE** (`Invoke-QbeConformanceCheck.ps1` baris 29) | Paling mutakhir; memuat `Prl` sejak 2026-11-11 |

**Yang saya lakukan atas instruksi pemilik (poin 2):** menambahkan tiga baris identik dengan salinan backend ke salinan kanonik (baris pemilik `Prl`, kepanjangan `Prl = Payroll`, dan satu entri changelog bertanggal 2026-11-20 yang menyatakan ini sinkronisasi tanpa persetujuan baru), lalu menjalankan `sync-rules.ps1` sehingga mirror ikut. Verifikasi: `sync-rules.ps1 -Check` lulus (exit 0) dan hanya dua berkas berubah di repository skills. **Versi plugin tidak saya naikkan, dan tidak ada commit.** Skrip sendiri menyarankan menaikkan versi lalu commit; itu keputusan rilis pemilik.

**Selisih yang ditemukan dan sengaja tidak diselaraskan:**

| Jenis | Isi |
| --- | --- |
| Baris pemilik hanya di backend (28) | Finance: `FinanceManagement` beserta delapan submodul (`BillingIntake`, `Receivable`, `Collection`, `Payable`, `CashManagement`, `AccountingIntegration`, `Purchasing`, `PatientRefund`); Procurement `Prc` (empat baris); Asset `Ast` (enam baris); `Mcu`, `Mdf`, `Nvw`, `Opt`, `Rhb`; `PatientManagement / Membership` |
| Baris hanya di kanonik (3) | `Finance | Finance` (nama lama), `Administrator / HealthServices | Master / Reference / MasterData` (nama area berbeda), dan Nutrition `Gz` |
| **Konflik prefix** | Nutrition: kanonik `Gz`, backend **`Gzi`**. Prefix berbeda untuk modul yang sama; pemilik perlu menentukan mana yang berlaku. Tidak diubah |
| Kepanjangan prefix hanya di backend (8) | `Ast`, `Gzi`, `Mcu`, `Mdf`, `Nvw`, `Opt`, `Prc`, `Rhb` |

Akibat praktis: agent yang membaca mirror Claude (yang dimuat ke konteks) melihat registry yang tertinggal, dan bisa keliru menyimpulkan modul terdaftar tidak ada (seperti kesalahan saya pada `Prl`). Menyelaraskan kanonik dengan backend memerlukan keputusan pemilik terutama untuk konflik `Gz` / `Gzi`, sehingga di luar wewenang yang diberikan.

---

## 11. Pernyataan

`dotnet build`, `npm run build`, migration, dan uji runtime **tidak dijalankan**. Tidak ada source yang ditulis, tidak ada perubahan pada `Migrations/` atau snapshot, tidak ada commit, push, atau deploy. Keadaan kolom pada database target **tidak dibaca**; hanya riwayat migration dan snapshot yang diperiksa. `AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

```text
$ git status --short   (hanya berkas milik task ini)
?? docs/module-blueprints/payroll-management/payroll-gaji/task/report/backend/PAY-BE-FIX-013.md
```
