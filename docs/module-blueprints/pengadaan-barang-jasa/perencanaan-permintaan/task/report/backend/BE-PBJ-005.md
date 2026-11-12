# Laporan Perubahan Backend — `BE-PBJ-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PBJ-005` |
| Judul | Gerbang approval Pengadaan |
| Slice | `MVP-0` — fondasi (`EPIC PBJ-01`) |
| Roadmap | `perencanaan-permintaan/roadmap/backend-roadmap.md` revisi 1, kartu `BE-PBJ-005` |
| Trace | `PBJ-DEC-007`, `016`, `019`, `027`, **`038`** (baru); FR-PBJ-001, FR-PBJ-044; INT §1; arsitektur §8, §10 |
| Contract version | `PBJ-PP-API` 1.1, `PBJ-PP-PERM` 1.2 (task ini tidak membuat endpoint) |
| Dependency | `BE-PBJ-001` 🟡, `BE-PBJ-003` 🟡 |
| Klasifikasi | `HIGH` (logika 2, kontrak antarmodul 2, keamanan 2 — menyentuh mesin approval bersama milik HR) |
| Task mode | `BACKEND` (diberikan pemilik 12 November 2026, bersama `BE-PBJ-006`) |
| Target tulis | `Areas/Corporate/ProcurementManagement/Common/**`, `WorkflowService.cs` (Wfl), `Program.cs` (urutan seeder), extension DI, `.gitignore` (izin terpisah); basis data dev tidak disentuh |
| Model | Claude Opus 5.5 |
| Branch / commit | `sukmagp` @ `100c011d` (belum di-commit) |
| Tanggal | 12 November 2026 |
| Status | 🟡 **SEBAGIAN.** Kode lengkap; build dan QBE `PASS`. Belum: bukti DB definisi/langkah (seeder belum pernah dijalankan karena aplikasi tidak dapat start di dev DB) dan instance approval nyata pertama (menunggu `BE-PBJ-009`) |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `Common`; plus `Corporate` / `HumanResource` / `WorkflowManagement` (Wfl, perluasan) |
| Registry | `Corporate / Procurement | ProcurementManagement / Common | Prc | ACTIVE` (`PBJ-DEC-035`); Wfl milik HR, diperluas atas keputusan pemilik `PBJ-DEC-019` dan `PBJ-DEC-038` |
| Keberlakuan | `NEW CODE` untuk Pengadaan; `TOUCHED LEGACY` untuk `WorkflowService.cs` dan `Program.cs` |
| QBE yang berlaku | `QBE-MOD-001..003`, `QBE-NAM-002`, `QBE-SVC-001`, `QBE-TXN-001` (Wfl dulu, idempoten), `QBE-LOG-001`, `QBE-AUD-001`, `QBE-PERM-001` (tanpa hardcode role: penyetuju ditentukan matriks) |
| Hasil checker | QBE `PASS` (15 berkas bersama `BE-PBJ-006`, 0 temuan) |

## 1. Masalah yang diperbaiki

Dokumen Pengadaan (PR, rencana kebutuhan, rencana paket, pagu) butuh persetujuan berjenjang. Mesin approval
bersama (Wfl) sudah ada, tetapi:

1. Wfl selalu mencatat **pengguna yang sedang login** sebagai pengaju. PR baru masuk approval ketika
   **verifikator** menyelesaikan verifikasi (`PBJ-DEC-027`), sehingga yang tercatat sebagai pengaju justru
   verifikator, dan jenjang "atasan pemohon" menunjuk atasan yang salah.
2. Belum ada pintu tunggal dari Pengadaan ke Wfl, belum ada penangan hasil approval untuk dokumen `PBJ_*`,
   dan belum ada definisi alur `PBJ-*`.

## 2. Proses bisnis

1. Modul pemilik dokumen (mis. `BE-PBJ-008` saat verifikasi selesai) memanggil gerbang dengan: jenis dokumen,
   ID, nomor, kode alur, **pemohon asli**, nilai, kode kondisi, dan "pengajuan ke-berapa".
2. Gerbang menyusun kunci idempoten `<jenis>:<id>:<ke-berapa>` dan konteks `{amount, conditionCodes}`.
3. Wfl membuat instance **atas nama pemohon** lalu langsung mengajukannya. Pemanggilan ulang dengan kunci sama
   mengembalikan instance yang sama.
4. Langkah ber-kode kondisi hanya aktif bila kodenya dikirim. Contoh: langkah "Direktur Keuangan" hanya
   muncul bila PR membawa `PBJ_OVER_BUDGET`.
5. Saat Wfl menyelesaikan approval, penangan lifecycle Pengadaan menerima instance `PBJ_*`, memeriksa ulang
   bahwa penyetuju bukan pemohon, lalu meneruskan ke penerap status milik jenis dokumennya.

**Contoh:** Rina (IGD) membuat PR Rp 12 juta; Budi (Tim Pengadaan) menyelesaikan verifikasi. Instance
tercatat diajukan oleh Rina; langkah pertama menunggu atasan Rina, bukan atasan Budi.

**Jalur tidak normal:**
- Definisi `PBJ-*` belum diaktifkan admin (404) atau tanpa langkah aktif → hasil `ConfigurationMissing = true`,
  yang dipetakan pemanggil ke `PBJ-VAL-091` "Alur persetujuan belum dikonfigurasi". Status dokumen tidak berubah.
- Wfl gagal di tengah → dokumen belum diubah (urutan "Wfl dulu"); pengulangan aman berkat kunci idempoten.
- Pemohon mencoba menyetujui pengajuannya sendiri → ditolak Wfl (`AllowSelfApproval = false`) dan diperiksa
  ulang oleh penangan lifecycle.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`WorkflowService.cs` (`CreateAsync`, `SubmitAsync`, resolusi pemohon/atasan, idempotensi),
`HumanResourceContextService`, `WorkflowReferenceLifecycleService.cs`, model `MstWorkflowDefinition`,
`MstWorkflowStep`, `MstApprovalMatrix`, seeder workflow HR.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `HumanResource/WorkflowManagement/Services/WorkflowService.cs` | **Diperbarui.** `CreateAsync`/`SubmitAsync` menjadi pembungkus `CreateCoreAsync`/`SubmitCoreAsync` (perilaku lama tidak berubah). Metode baru `CreateAndSubmitOnBehalfAsync(request, requesterUserId, ct)`: kunci idempoten wajib, konteks pemohon dari `GetByUserIdAsync`, ajukan hanya bila masih `Draft` dengan kunci `+":submit"` |
| `Common/Services/ProcurementApprovalGateway.cs` | **Baru.** `ProcurementApprovalRequest`/`Result`, `BuildIdempotencyKey`, `SubmitAsync` |
| `Common/Services/ProcurementWorkflowLifecycleHandler.cs` | **Baru.** Penangan `PBJ_*`, interface `IProcurementWorkflowStatusApplier`, `ProcurementWorkflowStatusMap` |
| `Common/Seeders/ProcurementWorkflowDefinitionSeeder.cs` | **Baru.** 5 definisi `PBJ-PR`, `PBJ-PR-MENDESAK`, `PBJ-RKB`, `PBJ-RPK`, `PBJ-PAGU`; kategori `Procurement`; `WorkflowStatus = "Draft"`, `IsActive = false`; langkah atasan pemohon + jabatan dengan kode kondisi `PBJ_OUT_OF_PLAN`/`PBJ_OVER_BUDGET`; idempoten |
| `ProcurementManagementServiceCollectionExtensions.cs` | +gateway, +penangan lifecycle |
| `Program.cs` | Seeder definisi dijalankan sesudah `ProcurementMasterDataSeeder` |
| `.gitignore` | `/App_Data/` (risiko `BE-PBJ-004`, izin pemilik terpisah) |

### 3.3 Dampak kontrak, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada endpoint baru. Kontrak antarmodul Wfl bertambah satu metode (delta `PBJ-DEC-038`) |
| Database | Tidak ada migration. Seeder menulis definisi/langkah ke tabel Wfl yang sudah ada **saat aplikasi start** |
| Keamanan/Auth | Penyetuju ditentukan matriks yang diisi admin, tanpa nama peran di kode. Definisi di-seed **tidak aktif tanpa baris matriks** (fail-closed) — delta dari kartu yang menyebut "matriks fallback [NILAI AWAL]": nilai matriks belum diputuskan (`PBJ-OQ-019`), jadi tidak ada penyetuju yang dikarang |

## 4. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` (perintah baku pemilik, bersama `BE-PBJ-006`) | 0 error, 608 warning (= baseline), 2 m 03 s | `PASS` | Log build |
| QBE checker (15 berkas) | 0 pelanggaran, 0 review | `PASS` | Laporan checker |
| Kriteria 1 — satu instance per kunci idempoten | `CreateCoreAsync` memakai pencarian `IdempotencyKey` milik Wfl; submit hanya dari `Draft` dengan kunci turunan | `PASS` (statis) | Review source |
| Kriteria 2 — definisi tidak aktif → `PBJ-VAL-091` | 404 / "belum memiliki step aktif" → `ConfigurationMissing` | `PASS` (statis) | Review source |
| Kriteria 3 — penangan terdaftar dan mengklaim `PBJ_*` | DI + `CanHandle` prefix `PBJ_` | `PASS` (statis) | Review source |
| Kriteria 4 — seeder idempoten | Cari per `WorkflowCode`/`StepCode` sebelum menambah | `PASS` (statis) | Review source; belum dijalankan |
| Perilaku Wfl lama tidak berubah | `CreateAsync`/`SubmitAsync` meneruskan `null` = jalur lama | `PASS` (statis) | Review diff |
| Instance approval nyata, DB definisi/langkah | Tidak dijalankan | `NOT RUN` | Aplikasi tidak dapat start di dev DB (migration rekan tertunda, kolom `ContractEscalationDays`); instance pertama direncanakan di `BE-PBJ-009` |

- **AUTOMATED TEST:** `NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 5. Status migration dan basis data

Tidak ada migration. Basis data dev tidak disentuh.

## 6. Risiko yang tersisa

| Risiko | Penanganan |
| --- | --- |
| Wfl menolak instance baru bila instance lama untuk dokumen yang sama masih `Returned` | Jalur ajukan ulang dirancang di `BE-PBJ-008` (kunci idempoten memakai `SubmissionCount`) |
| Penerap status tidak boleh bergantung pada `WorkflowService`/gateway (siklus DI) | Dicatat pada XML doc interface; diperiksa saat `BE-PBJ-008`/`009` mendaftarkan penerap |
| Approval tidak dapat dipakai sampai admin mengisi matriks dan mengaktifkan definisi | Disengaja; nilai menunggu SOP (`PBJ-OQ-019`) |

## 7. Task berikutnya

`BE-PBJ-009` (rencana kebutuhan) dan `BE-PBJ-010` (pagu) memakai gerbang ini dan membuktikan instance nyata pertama.
