# Laporan Perubahan Backend — `PAY-BE-FIX-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-FIX-011` (ad-hoc, bukan kartu roadmap; melanjutkan seri `PAY-BE-FIX-001..010`) |
| Judul | Penetapan Gaji: Gaji Dasar ditentukan Backend dari `MstSalaryStructure.DefaultBaseSalary`; nominal kiriman klien tidak otoritatif |
| Roadmap | **NOT APPLICABLE** (brief pemilik 2026-11-20; preseden `PAY-BE-FIX-001..010`) |
| Task mode | `BACKEND` (frontend dikerjakan terpisah: `PAY-FE-FIX-011`) |
| Branch / HEAD | `Yasmina` / `d642a9df9de35801ed8332adc0cdcd6127f96353` (cocok snapshot brief). **Working tree sudah kotor sebelum task** oleh pekerjaan lain (`PAY-BE-FIX-010`: status otorisasi/operasional penetapan gaji) pada `WfpSalaryAssignmentController.cs`, `WfpSalaryAssignmentDtos.cs`, dan berkas lain; perubahan task ini dibangun di atasnya, bukan menimpanya |
| Backend Governance Preflight | Area `Corporate`; modul `WorkforceCore` (`Wfp`) dan `PayrollManagement` (`Prl`); keberlakuan `TOUCHED LEGACY`; tanpa entity baru, tanpa migration. QBE yang berlaku: SVC-001 (aturan baru ditaruh di `PayrollSalaryRangeRuleService`, bukan di controller), API-001, PERM-001, VAL-001, DTO-001, AUD-001 |
| Model | Claude Sonnet 5.5 |
| Tanggal | 2026-11-20 |
| Status | 🟡 **Source selesai; QBE ReportOnly PASS (3 berkas, 0 temuan); `dotnet build` NOT RUN (dilarang task); runtime/UAT NOT RUN.** Tanpa migration |

## 1. Root cause

| Gejala | Akar penyebab | Lokasi |
| --- | --- | --- |
| Nominal dapat dimanipulasi dari klien | `Create` dan `Update` menyimpan `request.BaseSalary` apa adanya setelah hanya diperiksa **rentang**; tidak ada kewajiban sama dengan `DefaultBaseSalary` struktur. Nominal apa pun dalam rentang diterima | `WfpSalaryAssignmentController.Create` (`BaseSalary = request.BaseSalary`), `Update` (`entity.BaseSalary = request.BaseSalary`) |
| Tidak ada penentuan nominal di server | `PayrollSalaryRangeRuleService.ValidateSalaryAssignmentAsync` memvalidasi nominal yang diberikan, tidak pernah mengambilnya dari master | `PayrollSalaryRangeRuleService.cs` |
| Masa berlaku struktur tidak diperiksa pada transaksi baru | Pemeriksaan hanya `IsActive`; `EffectiveStartDate`/`EffectiveEndDate` struktur tidak dibaca | `PayrollSalaryRangeRuleService.ValidateSalaryAssignmentAsync` |
| Pengguna tanpa `ViewAmount` harus mengetik nominal baru saat mengubah | Nominal tersimpan tidak dikirim (null) tetapi `BaseSalary` request wajib | `CreateWfpSalaryAssignmentRequest` (catatan terbuka `PAY-BE-FIX-008`) |

## 2. Perubahan backend

### 2.1 Penentuan Gaji Dasar (satu aturan, dipakai Create dan Update)

`ValidateRequestAsync(workforceProfileId, existing, request, ct)` kini mengembalikan **`(IsValid, ErrorMessage, BaseSalary)`**; nominal yang disimpan hanya berasal dari sini.

| Keadaan | Nominal yang disimpan |
| --- | --- |
| Create | `MstSalaryStructure.DefaultBaseSalary` saat ini |
| Update, struktur **atau** grade berganti | `DefaultBaseSalary` struktur baru (perubahan finansial: persetujuan lama dibatalkan seperti aturan `PAY-BE-FIX-008`) |
| Update, struktur dan grade sama | **Nominal tersimpan dipertahankan** (histori tidak ditimpa perubahan master); tanggal mulai yang berubah hanya memeriksa keabsahan struktur pada tanggal baru |
| `BaseSalary` pada request | Opsional (`decimal?`). Diterima **hanya bila sama** (dibulatkan 2 desimal) dengan nominal hasil penentuan server; selain itu **400** dengan pesan yang tidak membocorkan nominal. Tidak pernah disimpan |

Pesan penolakan: Create/penggantian struktur: "Gaji Dasar ditentukan otomatis dari Struktur Gaji yang dipilih dan tidak dapat diisi atau diubah manual…"; struktur tidak berubah: "Gaji Dasar penetapan yang sudah tersimpan tidak dapat diubah dari formulir ini. Untuk mengubah nominal, tutup masa berlaku penetapan ini lalu buat penetapan baru…".

### 2.2 Validasi grade dan struktur (reuse `PayrollSalaryRangeRuleService`)

Metode baru `ResolveSalaryAssignmentBasisAsync(structureId, gradeId, effectiveDate, requireDefaultBaseSalary, ct)` (hanya membaca):

1. Struktur ada dan `IsActive`, bila tidak: 400 "Struktur Gaji tidak ditemukan atau tidak aktif."
2. Struktur milik grade yang dipilih (`SalaryGradeId`), bila tidak: 400 "…tidak sesuai dengan Grade Gaji…".
3. Grade ada dan aktif.
4. **Masa berlaku struktur mencakup tanggal mulai penetapan** (`AssessMasterValidity`: `StructureInactive` / `GradeInactive` / `StructureNotYetEffective` / `StructureExpired`) dengan alasan berisi tanggal.
5. `DefaultBaseSalary > 0`, bila tidak: 400 **"Gaji Dasar belum dikonfigurasi pada Struktur Gaji. Lengkapi master Struktur Gaji terlebih dahulu."** (pengguna tidak diminta mengisi nominal manual).

Sesudahnya pemeriksaan lama berjalan dengan nominal hasil penentuan: kecocokan **mata uang** struktur/grade/request, **rentang** efektif grade ∩ struktur, irisan masa berlaku, dan penetapan utama. Pemeriksaan frekuensi pembayaran tidak berubah.

### 2.3 Perlindungan histori dan approval

- Perubahan finansial dinilai terhadap nominal hasil penentuan server (`HasFinancialChange`/`IsClosureOnly` menerima `validation.BaseSalary`); aturan persetujuan, penguncian payroll final (409, tutup masa berlaku lalu buat penetapan baru), maker-checker, audit tanpa nominal, dan transaksi Serializable **tidak diubah**.
- Mengubah keterangan/metadata dengan struktur tidak berubah **tidak mengubah nominal**, bahkan bila master berubah.
- **Tidak ada sinkronisasi massal, backfill, atau penimpaan** penetapan lama mengikuti master. `Approve`, `UpdateStatus`, dan `SetPrimary` tetap memvalidasi nominal **tersimpan** terhadap rentang (tidak dipaksa sama dengan master).

### 2.4 Respons aditif (`WfpSalaryAssignmentResponse`)

| Field | Arti |
| --- | --- |
| `structureDefaultBaseSalary` | Gaji Dasar master struktur **saat ini**; null tanpa `ViewAmount` |
| `baseSalaryDiffersFromStructureDefault` | Nominal tersimpan ≠ master saat ini (master berubah atau data lama); null tanpa `ViewAmount`. **Hanya penanda untuk ditinjau**, tidak ada yang diubah otomatis |
| `masterValidityStatus` / `masterValidityMessage` | `Valid` \| `StructureInactive` \| `GradeInactive` \| `StructureNotYetEffective` \| `StructureExpired` pada hari ini, beserta alasan |

Field yang sudah ada dan memenuhi daftar brief: `salaryGradeId`, `salaryStructureId`, `currencyCode`, `paymentFrequency`, `structureMinimumSalary`/`structureMaximumSalary` (rentang efektif), `effectiveMinimumSalary`/`effectiveMaximumSalary`. Tidak ada endpoint baru (options struktur/grade dan detail penetapan sudah cukup).

## 3. Kontrak API

| Endpoint | Perubahan | Kompatibilitas |
| --- | --- | --- |
| `POST/PUT …/workforce-profiles/{id}/salary-assignments` | `baseSalary` menjadi **opsional**; tipe JSON tetap angka | Klien lama yang mengirim nominal **sama dengan** hasil server tetap berhasil; nominal berbeda kini 400 (perubahan perilaku yang disengaja). Klien baru boleh tidak mengirimnya. Satu-satunya pemanggil source adalah controller ini; jalur lain (`WfpNewHireAssignmentService`) menulis entity langsung (lihat bagian 6) |
| `GET …/salary-assignments`, `GET …/{id}` | Empat field aditif (bagian 2.4) | Aditif murni |
| `GET …/salary-structures/options?salaryGradeId=` | **Tidak diubah**: sudah memfilter server-side, `search`, `pageNumber/pageSize`, `onlyActive` | — |

## 4. Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode ReportOnly` pada 3 berkas | **PASS** (VIOLATION 0, REVIEW 0) |
| `dotnet build` / `restore` | **NOT RUN** (dilarang task). Tinjauan statis atas tanda tangan: tuple bernama `(IsValid, ErrorMessage, BaseSalary)`, argumen bernama `requireDefaultBaseSalary:` pada posisinya, ekspresi `bool?` target-typed, `new(...)` target-typed pada `record`; belum dikompilasi |
| Tes unit FE yang membaca source BE (`workforce-salary-assignment.test.mjs`, dari root workspace) | 30/30 lulus. Ini pemeriksaan **teks source**, bukan eksekusi |
| `AUTOMATED TEST` backend | `NOT APPLICABLE` (kebijakan `TEST_POLICY.md`; `Tests/` tidak disentuh) |
| Runtime / UAT | **NOT RUN** |
| Migration / database | Tidak ada migration, tidak ada perubahan skema, tidak ada akses database |

### Peta UAT (statis; tidak ada yang dinyatakan PASS runtime)

| UAT | Sisi Backend | Status |
| --- | --- | --- |
| 07 manipulasi `BaseSalary` | Ditolak 400 bila ≠ hasil server; tidak pernah disimpan | Terpetakan di source, **runtime NOT RUN** |
| 08 struktur nonaktif/tidak berlaku | `ResolveSalaryAssignmentBasisAsync` langkah 1, 4 | Terpetakan, NOT RUN |
| 09 `DefaultBaseSalary` kosong | Pesan baku, 400 | Terpetakan, NOT RUN |
| 13 ubah keterangan saja | Struktur sama → nominal tersimpan | Terpetakan, NOT RUN |
| 14 ganti struktur belum final | Nominal baru dari master; perubahan finansial; persetujuan dibatalkan | Terpetakan, NOT RUN |
| 15 sudah dipakai payroll final | Cabang 409 yang ada (`financialChange && !closureOnly`) menilai nominal server | Terpetakan, NOT RUN |
| 16 mata uang/rentang | Pemeriksaan lama berjalan dengan nominal hasil penentuan | Terpetakan, NOT RUN |
| 18 `ViewAmount` | `structureDefaultBaseSalary`/`baseSalary` null tanpa izin; pesan penolakan tanpa nominal | Terpetakan, NOT RUN |
| 19 tunjangan struktur ke Payroll Calculation | Tidak disentuh; `PayrollCalculationService` tetap membaca `BaseSalary` tersimpan sebagai snapshot | Tidak berubah |
| 20 simpan berulang | Penulisan tetap dalam transaksi Serializable + advisory lock per pegawai | Tidak berubah |

## 5. Temuan dan risiko

1. **Urutan deploy: Backend lebih dulu.** Backend lama menolak request tanpa `baseSalary` ("Gaji pokok harus lebih besar dari 0"), jadi FE baru di atas BE lama gagal menyimpan.
2. **Endpoint options struktur** (`SalaryStructureController.GetSalaryStructureOptions`) dan detailnya mengirim `DefaultBaseSalary`/`MinimumBaseSalary`/`MaximumBaseSalary` kepada siapa pun dengan `SalaryStructure : Read`, tanpa `ViewAmount`. **Tidak diubah** (master data, banyak pemanggil; keputusan akses belum ada, lihat bagian 6). FE menyamarkan nominal Gaji Dasar di form penetapan bagi yang tanpa `WfpSalaryAssignment : ViewAmount`.
3. **Jalur pegawai baru** `WfpNewHireAssignmentService` membuat `WfpSalaryAssignment` langsung dengan `BaseSalary = basicSalary` (nominal penawaran) dan struktur pertama yang aktif untuk grade, **tanpa** validasi master. Di luar form Penetapan Gaji, tidak diubah.
4. Edit metadata atas penetapan lama yang strukturnya kini nonaktif atau nominal tersimpannya di luar rentang master saat ini tetap ditolak (perilaku lama dipertahankan; `masterValidityStatus` menjelaskan sebabnya).
5. Tinjauan data lama (tidak dijalankan; baca-saja, pemilik yang menjalankan):

```sql
SELECT a."Id", a."WorkforceProfileId", s."SalaryStructureCode",
       (a."BaseSalary" <> s."DefaultBaseSalary") AS "NominalBedaDariMaster",
       s."IsActive" AS "StrukturAktif", s."EffectiveStartDate", s."EffectiveEndDate"
FROM "WfpSalaryAssignment" a
JOIN "MstSalaryStructure" s ON s."Id" = a."SalaryStructureId"
WHERE NOT a."IsDelete" AND a."IsActive"
  AND (a."BaseSalary" <> s."DefaultBaseSalary" OR NOT s."IsActive"
       OR (s."EffectiveEndDate" IS NOT NULL AND s."EffectiveEndDate" < CURRENT_DATE));
```

## 6. Keputusan yang masih terbuka (tidak diputuskan kode)

| Butir | Keadaan saat ini |
| --- | --- |
| Perubahan `DefaultBaseSalary` master harus selalu menghasilkan penetapan baru pada tanggal efektif tertentu? | **NEEDS BUSINESS DECISION.** Kode tidak menimpa penetapan lama; hanya menandainya (`baseSalaryDiffersFromStructureDefault`). Mengubah nominal = tutup masa berlaku + penetapan baru |
| Struktur yang sudah berakhir sebagai pilihan historis read-only? | Pada penetapan **lama** tetap tampil dengan label dan alasan; untuk transaksi **baru** ditolak Backend. Filter tanggal pada endpoint options belum ada |
| Aturan `ViewAmount` bagi yang boleh memilih struktur tetapi tidak boleh melihat nominal | **NEEDS BUSINESS DECISION.** Sementara: nominal penetapan terselubung di FE dan respons; options master tidak diubah |
| Pembersihan data lama yang menyimpang | Hanya penandaan; tidak ada backfill |

## 7. Daftar berkas (repository backend, branch `Yasmina`)

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/PayrollManagement/Services/PayrollSalaryRangeRuleService.cs` | `ResolveSalaryAssignmentBasisAsync`, `AssessMasterValidity`, `DefaultBaseSalaryNotConfiguredMessage`, `PayrollSalaryAssignmentBasis`, `PayrollSalaryMasterValidity`, `PayrollSalaryMasterValidityResult` |
| `Areas/Corporate/HumanResource/WorkforceCore/Controllers/WfpSalaryAssignmentController.cs` | `ValidateRequestAsync` menentukan nominal; Create/Update menyimpan `validation.BaseSalary`; penilaian perubahan finansial memakai nominal server; `MapResponse` memuat empat field aditif |
| `Areas/Corporate/HumanResource/WorkforceCore/DTOs/WfpSalaryAssignmentDtos.cs` | `BaseSalary` request `decimal?` (tidak otoritatif); empat field respons aditif |

Controller dan DTO juga memuat perubahan `PAY-BE-FIX-010` yang belum di-commit; keduanya tidak dibuang. Tidak ada stage, commit, push, pull, merge, atau deploy.
