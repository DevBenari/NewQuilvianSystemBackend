# Laporan Perubahan Backend — `PAY-BE-14`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-14` |
| Judul | Fondasi skema dan konstanta |
| Slice | `MVP-0` — fondasi untuk `EPIC PGJ-01`, `PGJ-03`, `PGJ-04`, `PGJ-05` |
| Roadmap | `docs/module-blueprints/payroll-management/payroll-gaji/roadmap/backend-roadmap.md`, kartu `PAY-BE-14` |
| Trace | FR-PGJ-001, 030, 035; PRL-DEC-02, 04, 07, 13, 32, 46; `../../../02-backend-architecture.md` bagian 4.1, 4.4, 7, 8; `../../../data/data-dictionary.md` bagian 2 |
| Contract version | `PGJ-C1` — `approved` (Yasmin, 2026-11-12) |
| Dependency | Tidak ada di roadmap. Prasyarat database: migration `20261111100000_RenamePayrollTrxAndWfpTablesToPrlPrefix` sudah diterapkan |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, berkas diperiksa 2 (> 20), berkas diubah 2 (11), logika bisnis 0, kontrak API 0, database 2 (schema dan migration), keamanan 0, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: model, configuration, konstanta, opsi, migration; dokumen blueprint `payroll-gaji/` untuk laporan dan penandaan status |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6722d7ca` (branch `Yasmina`) |
| Tanggal | 2026-11-12 |
| Status | 🟡 — source dan migration selesai dan lolos pemeriksaan statis; **`dotnet build` tidak dijalankan atas instruksi pemilik**, migration belum diterapkan ke database |

---

## 1. Masalah yang diperbaiki

Tujuh epic MVP Payroll Gaji membutuhkan tempat menyimpan data yang belum ada:

- **Tunjangan tetap per Struktur Gaji** belum punya tabel. Satu-satunya sumber tunjangan tetap
  hari ini adalah angka tunggal `TotalAllowance` di profil pegawai.
- **Persetujuan insentif, bonus, dan honor** belum bisa dicatat. Tabel input variabel hanya punya
  kolom "diajukan" dan "diverifikasi", tanpa kolom "disetujui" atau "ditolak" beserta alasannya.
- Mesin hitung belum punya kode baris untuk tunjangan per jenis dan transport, dan belum punya
  saklar masa peralihan serta batas simulasi.

Task ini hanya menyiapkan fondasinya. **Belum ada perilaku yang berubah**: perhitungan gaji, layar,
dan endpoint bekerja persis seperti sebelumnya sampai task `PAY-BE-16` dan seterusnya memakai
fondasi ini.

## 2. Proses bisnis

Task ini tidak mengubah proses bisnis yang berjalan. Proses yang **disiapkan**:

1. Petugas Payroll akan mengisi tunjangan tetap per jenis pada Struktur Gaji (`PAY-BE-16`).
   **Contoh:** Struktur "GR-05 Perawat Pelaksana" memuat Tunjangan Jabatan nominal tetap
   Rp 750.000 dan Tunjangan Keahlian 5% dari gaji pokok. Kedua baris itu kelak tersimpan di
   `MstSalaryStructureAllowance`.
2. Petugas akan memasukkan insentif dan menyetujuinya (`PAY-BE-19`). Waktu persetujuan dan
   penyetujunya tersimpan di kolom baru `ApprovedAt` dan `ApprovedByUserId`. Penolakan tersimpan
   di `RejectedAt`, `RejectedByUserId`, dan alasannya di `DecisionReason`.
3. Selama masa peralihan, run tetap memakai angka profil bila struktur belum berisi tunjangan,
   karena saklar `AllowProfileAllowanceFallback` bawaannya menyala (`PAY-BE-17`).

Jalur tidak normal yang dijaga skema:

- Jenis tunjangan yang sama tidak bisa tersimpan dua kali pada satu struktur. Index unik parsial
  menolaknya di database walaupun validasi aplikasi terlewat.
- Struktur atau jenis tunjangan yang masih dirujuk baris tunjangan tidak bisa dihapus permanen
  (`Restrict`).
- Akun penyetuju yang dihapus tidak menghapus input. Kolom penyetujunya menjadi kosong
  (`SetNull`) dan jejak waktunya tetap ada.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Governance: `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`;
  `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; suite `rules/backend/` (TASK_RULES,
  DATABASE_RULES, REPORT_TEMPLATE, TASK_CLASSIFICATION, TEST_POLICY);
  `tooling/qbe/Invoke-QbeConformanceCheck.ps1` (logika `QBE-MOD-002`, `QBE-ENT-001`,
  `QBE-CFG-001`, `QBE-NAM-001`).
- Pola source: `MstSalaryStructure.cs` dan configuration-nya, `PrlPayrollVariableInput.cs` dan
  configuration-nya, `PayrollStatusCatalog.cs`, `PayrollPolicyOptions.cs`,
  `ApplicationDbContext.cs`, `ApplicationDbContextModelSnapshot.cs`, Designer
  `20261111100000_RenamePayrollTrxAndWfpTablesToPrlPrefix`, `Migrations/MigrationMetadata.g.cs`,
  serta migration `20261111030000_AlignHrdColumnsWithModel` dan
  `20261111040000_ResolveHrdSchemaResidualDrift`.
- Blueprint: `02-backend-architecture.md`, `data/data-dictionary.md`, `roadmap/backend-roadmap.md`.

**Impact scan.** SHA baseline blueprint `89b9fd69` sudah bergeser ke `6722d7ca` (merge
`QuilvianIntegrationBackend`, 246 berkas). Berkas dalam cakupan task ini (model, configuration,
konstanta, opsi Payroll) tidak berubah. Snapshot dan `ApplicationDbContext` berubah oleh migration
HR baru. Dua migration HR baru (`20261111030000` dan `20261111040000`) menyebut tabel Payroll,
tetapi hanya tabel `Trx*` milik tim integrasi yang memang tidak ikut di-rename, sehingga urutannya
terhadap migration rename aman. Snapshot hasil merge masih memuat `PrlPayrollVariableInput` dan
`PrlTransportAllowance`, tanpa sisa nama lama `TrxPayrollVariableInput` atau
`WfpTransportAllowance`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Models/MstSalaryStructureAllowance.cs` | **Baru.** Entity tunjangan tetap per struktur (mewarisi `IdentityModel`), beserta konstanta `SalaryStructureAllowanceCalculationMethods` (`FixedAmount`, `PercentageOfBaseSalary`) |
| `Repositories/Configurations/Corporate/HumanResource/MasterData/PayrollAndBenefit/MstSalaryStructureAllowanceConfiguration.cs` | **Baru.** Tabel `public."MstSalaryStructureAllowance"`, panjang dan presisi kolom, nilai bawaan, relasi `Restrict`, index unik parsial (`SalaryStructureId`, `AllowanceTypeId`) dengan filter `"IsDelete" = false`, index `AllowanceTypeId` |
| `Areas/Corporate/PayrollManagement/Models/PrlPayrollVariableInput.cs` | Tambah `ApprovedAt`, `ApprovedByUserId`, `RejectedAt`, `RejectedByUserId`, `DecisionReason` (maks 500), serta navigasi `ApprovedByUser`, `RejectedByUser` |
| `Repositories/Configurations/Corporate/PayrollManagement/PrlPayrollVariableInputConfiguration.cs` | Tipe kolom waktu, panjang `DecisionReason`, relasi user baru `SetNull` |
| `Repositories/ApplicationDbContext.cs` | `DbSet<MstSalaryStructureAllowance> MstSalaryStructureAllowances` |
| `Areas/Corporate/PayrollManagement/Constants/PayrollStatusCatalog.cs` | Kode mesin `PAY_STRUCTURE_ALLOWANCE`, `PAY_TRANSPORT_ALLOWANCE`; sumber input `PayrollManualInput`; kelas `PayrollEngineComponentSources` (`SalaryStructureAllowance`, `TransportAllowance`) |
| `Areas/Corporate/PayrollManagement/Constants/PayrollManualInputTypes.cs` | **Baru.** `Incentive`, `Bonus`, `AdditionalDutyHonorarium` |
| `Areas/Corporate/PayrollManagement/Options/PayrollPolicyOptions.cs` | `AllowProfileAllowanceFallback` (bawaan `true`), `SimulationMaxEmployees` (bawaan `50`) |
| `Migrations/20261112090000_AddSalaryStructureAllowanceAndVariableInputDecision.cs` | **Baru.** `Up`: lima kolom, tabel baru, empat index, dua FK user. `Down`: kebalikannya. Ditulis manual |
| `Migrations/20261112090000_AddSalaryStructureAllowanceAndVariableInputDecision.Designer.cs` | **Baru.** Dibangun dari snapshot hasil sunting; badannya identik dengan snapshot, kepalanya hanya berbeda nama migration dari Designer `20261111100000` |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | 140 baris tambahan, 0 baris dihapus: blok definisi dan relasi `MstSalaryStructureAllowance`, lima properti, dua index, dua relasi, dan dua navigasi pada `PrlPayrollVariableInput` |

Dua migration pada `02-module-map.md` (`AddSalaryStructureAllowance` dan
`AddPayrollVariableInputDecisionColumns`) digabung menjadi satu berkas, sebagaimana diizinkan
dokumen itu, supaya cukup satu Designer.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada endpoint atau DTO yang berubah |
| Database | Satu tabel baru dan lima kolom baru yang boleh kosong. Tidak ada perubahan atau penghapusan tabel dan kolom lama. **Migration belum diterapkan**; menunggu pemilik |
| Keamanan/Auth | `NOT APPLICABLE` — tidak ada hak akses baru pada task ini. Kolom nominal baru (`Amount`, `Percentage`) sensitif dan tidak boleh masuk log pada task berikutnya |

**Selisih terhadap pola yang ada.** Relasi user lama pada `PrlPayrollVariableInput`
(`SubmittedByUserId`, `VerifiedByUserId`) memakai `Restrict`. Dua relasi baru memakai `SetNull`,
mengikuti kontrak `PGJ-C1` yang sudah disetujui (`data-dictionary.md` bagian 2.2). Saya tidak
mengubah kontrak sepihak. Bila pemilik ingin seragam `Restrict`, kontrak dan configuration
diubah bersama pada revisi berikutnya.

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menyentuh endpoint.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet restore` dan `dotnet build` | Tidak dijalankan atas instruksi pemilik ("tanpa build dotnet automatis") | `NOT RUN` | — |
| `dotnet ef migrations has-pending-model-changes` | Tidak dijalankan (larangan yang sama) | `NOT RUN` | — |
| `dotnet ef database update --no-build --configuration Release` (dijalankan pemilik, 2026-11-12) | `Applying migration '20261112090000_AddSalaryStructureAllowanceAndVariableInputDecision'.` lalu `Done.` | `PASS` | Keluaran terminal pemilik. Karena `--no-build`, migration ini hanya dapat ditemukan bila assembly Release sudah memuat kode task ini; build Release yang memuatnya pernah berhasil (bukti tidak langsung, keluaran build tidak dilampirkan) |
| Kecocokan kolom model, snapshot, dan migration `MstSalaryStructureAllowance` | 18 kolom identik di ketiganya (8 kolom domain + 10 kolom audit `IdentityModel`) | `PASS` | Skrip pembanding statis |
| Lima kolom baru `PrlPayrollVariableInput` ada di snapshot dan migration | Kelimanya ada | `PASS` | Skrip pembanding statis |
| Badan Designer sama dengan badan snapshot | Identik | `PASS` | `diff` |
| Snapshot hanya bertambah | 140 tambahan, 0 dihapus | `PASS` | `git diff --stat` |
| Keseimbangan kurung pada 9 berkas C# yang disentuh | Seimbang semua | `PASS` | Skrip penghitung |
| `QBE-ENT-001` entity baru mewarisi `IdentityModel` | Terpenuhi | `PASS` | `MstSalaryStructureAllowance : IdentityModel` |
| `QBE-CFG-001` configuration khusus | Terpenuhi | `PASS` | `MstSalaryStructureAllowanceConfiguration` |
| `QBE-MOD-002` dan `QBE-NAM-002` kepemilikan dan prefix | Simulasi resolver checker atas seluruh registry: kandidat `Hrd` (kedalaman 0) dan `Mst` (kedalaman 1); terpilih `Master / Reference / MasterData`, prefix `Mst`, ACTIVE | `PASS` | Port logika `Resolve-RegistryOwnership` |
| `QBE-NAM-001`, `QBE-CODE-002/003`, `QBE-SVC-001` | Tidak ada `Trx*`, alokasi nomor, atau controller pada diff | `PASS` | `grep` pada baris baru |
| Checker resmi `Invoke-QbeConformanceCheck.ps1` | Tidak dijalankan; agen tidak punya `pwsh` | `NOT RUN` | — |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT APPLICABLE` — tidak ada perilaku runtime baru yang bisa diuji di layar atau API.

**Tidak dijalankan:** `dotnet restore`, `dotnet build`, `dotnet ef`, `database update`, dan
checker QBE resmi. Build dilarang oleh instruksi pemilik pada task ini. Database update adalah
wewenang pemilik.

**Langkah verifikasi untuk pemilik**, berurutan:

```bash
dotnet build
dotnet ef migrations has-pending-model-changes   # harapan: tidak ada perubahan tertunda
dotnet ef migrations list                         # 20261112090000_... muncul paling akhir
dotnet ef database update                         # di DB uji, setelah 20261111100000 diterapkan
```

Pemeriksaan pasca-update (baca-saja):

```sql
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_schema = 'public' AND table_name = 'MstSalaryStructureAllowance'
ORDER BY column_name;                               -- harapan: 18 baris

SELECT column_name FROM information_schema.columns
WHERE table_schema = 'public' AND table_name = 'PrlPayrollVariableInput'
  AND column_name IN ('ApprovedAt','ApprovedByUserId','RejectedAt','RejectedByUserId','DecisionReason');
                                                    -- harapan: 5 baris

SELECT indexname, indexdef FROM pg_indexes
WHERE schemaname = 'public' AND tablename = 'MstSalaryStructureAllowance';
                                                    -- harapan: PK, index AllowanceTypeId, index unik berfilter IsDelete
```

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Kolom, panjang, presisi, index unik parsial, dan `Restrict`/`SetNull` sesuai `data/data-dictionary.md` | Terpenuhi (statis) | Configuration dan migration bagian 3.2; pembanding bagian 5 |
| 2. Migration hanya menambah, tanpa `DROP` tabel atau kolom lama | Terpenuhi | `Up` hanya `AddColumn`, `CreateTable`, `CreateIndex`, `AddForeignKey` |
| 3. Tidak ada perubahan perilaku runtime | Terpenuhi (statis) | Konstanta dan opsi baru belum dirujuk kode mana pun; tidak ada service atau controller yang berubah |
| DoD: build hijau | **Sebagian** — bukti tidak langsung | Database update `--no-build` menemukan migration dalam assembly Release; keluaran build dan `has-pending-model-changes` belum dilampirkan |
| DoD: QBE tanpa pelanggaran memblokir | Terpenuhi untuk aturan yang dicek (statis); checker resmi `NOT RUN` | Bagian 5 |
| DoD: berkas migration ada | Terpenuhi | Dua berkas `20261112090000_*` |
| DoD: laporan ditulis | Terpenuhi | Berkas ini |
| DoD: database update oleh pemilik | Terpenuhi | Pemilik, 2026-11-12: migration `20261112090000` diterapkan (`Done.`) |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | SHA baseline blueprint (`89b9fd69`) sudah lama; impact scan bagian 3.1 menyatakan cakupan task ini tidak terdampak. `has-pending-model-changes` atas snapshot hasil merge `6722d7ca` belum pernah dibuktikan, sehingga selisih yang muncul saat pemilik menjalankannya bisa berasal dari merge itu, bukan dari task ini |
| Masalah yang diketahui | Relasi user baru `SetNull`, lama `Restrict` (bagian 3.3) |
| Risiko tersisa | Kesalahan ketik pada migration atau snapshot yang ditulis manual baru tertangkap saat `dotnet build` dan `has-pending-model-changes` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M` 6 berkas (`PayrollStatusCatalog.cs`, `PrlPayrollVariableInput.cs`, `PayrollPolicyOptions.cs`, `ApplicationDbContextModelSnapshot.cs`, `ApplicationDbContext.cs`, `PrlPayrollVariableInputConfiguration.cs`); `??` 5 berkas (model, configuration, konstanta baru, dua berkas migration); ditambah dokumen laporan dan penandaan roadmap. Tidak ada yang di-stage atau di-commit |
| Langkah berikutnya | Pemilik menjalankan langkah verifikasi bagian 5. Setelah build hijau, tanda task naik ke ✅. Task yang terbuka: `PAY-BE-16` dan `PAY-BE-19` (gelombang 2), dan task gelombang 1 lain (`PAY-BE-15`, `PAY-BE-22`, `PAY-BE-23`) |
