# Laporan Perubahan Backend — `BE-RHB-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RHB-004` |
| Judul | Jenis dokumen rehab di mesin keutuhan Rekam Medis |
| Slice | S-A — Fondasi (`MVP-0`) |
| Roadmap | `docs/module-blueprints/rehab-medik/roadmap/backend-roadmap.md` — kartu `BE-RHB-004` |
| Trace | RHB-DEC-007, RHB-DEC-019; INT-RHB-05; `02-backend-architecture.md` §7.2 |
| Contract version | Set kontrak `v1`, `approved` 2026-11-05 |
| Dependency | `BE-RHB-001` ✅ |
| Klasifikasi | `STANDARD` — menyentuh layanan bersama milik modul lain (Rekam Medis), tanpa schema dan tanpa endpoint |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: 3 berkas `MedicalRecordManagement` + 1 berkas baru `RehabilitationManagement` + dokumen blueprint `rehab-medik` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6bb3fba7c0dab893647b87018a73c55359c19d08` (`sukmagp`), belum di-commit |
| Tanggal | 2026-11-05 |
| Status | **✅ Selesai** (2026-11-06). Source selesai, build `PASS`, dan QBE Strict `PASS` (2026-11-05). Butir DoD terakhir, bukti persetujuan pemilik Rekam Medis (`RHB-OQ-012`), dipenuhi 2026-11-06 lewat konfirmasi owner ([evidence](../../../evidence/RHB-OQ-012-persetujuan-rekam-medis.md)). |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `MedicalRecordManagement` (disentuh) dan `HealthServices` / `RehabilitationManagement` (`Rhb`, `ACTIVE`) |
| Applicability | `TOUCHED LEGACY` untuk 3 berkas RM, karena nilai enum dan butir himpunan hanya ditambah di ujung; `NEW CODE` untuk gateway |
| Registry | `Rhb` `ACTIVE` (2026-11-05). Tidak ada entity baru. |
| QBE yang berlaku | QBE-NAM (prefix `Rhb` pada gateway), QBE-SVC-001 (service tanpa generic repository), QBE-API-001 (kontrak RM existing tidak berubah) |
| Persetujuan modul lain | Pemilik Rekam Medis (Yoga Aji Pratama). Owner Rehab Medik menyatakan persetujuan pada 2026-11-05 (RHB-DEC-019); bukti tertulis = `RHB-OQ-012`, **belum** dilampirkan. |

## 1. Masalah yang diperbaiki

Mesin keutuhan Rekam Medis hanya mengunci dokumen yang jenisnya terdaftar di himpunan
`JenisYangDitegakkan`. Dokumen yang jenisnya tidak terdaftar tetap tampil "Ditandatangani",
tetapi isinya masih bisa disunting. Temuan ini pernah terjadi di modul Hemodialisa.

**Contoh.** Fisioterapis menandatangani catatan sesi pukul 10.00. Tanpa task ini, petugas lain
masih bisa mengubah jumlah menit terapi pukul 14.00, padahal catatan itu sudah sah sebagai
rekam medis.

## 2. Proses bisnis

1. Layanan rehab (task `BE-RHB-009` dan seterusnya) membuat dokumen draf, lalu memanggil
   `RegisterDraftAsync` di transaksi yang sama.
2. Sebelum setiap suntingan, layanan memanggil `EnsureMutableAsync`. Dokumen yang sudah
   ditandatangani atau terkunci ditolak.
3. Saat ditandatangani, layanan memanggil `SignAsync`. Mulai saat itu dokumen tidak bisa
   disunting lagi; koreksi hanya lewat addendum RM yang sudah ada.
4. Draf yang dibatalkan ditandai `MarkCancelledAsync`.
5. Saat kunjungan ditutup, mesin RM yang sudah ada (`LockOpenDocumentsForEncounterAsync`) mengunci
   draf yang masih terbuka sebagai *terkunci tanpa tanda tangan*.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs` (himpunan, `RegisterAsync`, `EnsureMutableAsync`, `SignAsync`, `MarkCancelledAsync`, `LockOpenDocumentsForEncounterAsync`)
- `MedicalRecordManagement/Services/MedicalRecordTimelineService.cs` (label jenis, `JenisBelumTersedia`)
- `MedicalRecordManagement/Enums/ClinicalDocumentKind.cs`
- Pola pemakaian di `HemodialysisManagement` (gateway HD) sebagai pembanding

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `MedicalRecordManagement/Enums/ClinicalDocumentKind.cs` | Tambah `RehabAssessment = 47`, `RehabProgram = 48`, `RehabSession = 49`, `RehabProgressReview = 50`, `RehabDischargeSummary = 51`, `RehabEducation = 52` di ujung enum, beserta komentar asal keputusan. Nilai 1–46 tidak berubah. |
| `MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs` | Keenam jenis ditambahkan ke `JenisYangDitegakkan`, disertai komentar persetujuan yang merujuk `RHB-OQ-012`. Logika lain tidak diubah. |
| `MedicalRecordManagement/Services/MedicalRecordTimelineService.cs` | **Delta:** enam label Bahasa Indonesia ditambahkan ke switch label jenis ("Asesmen Rehab Medik", "Program Terapi Rehab", "Sesi Terapi Rehab", "Reasesmen Rehab", "Ringkasan Akhir Rehab", "Edukasi dan Persetujuan Rehab"). Tanpa label ini, timeline akan menampilkan nama enum mentah pada daftar "belum tersedia". Berkas ini tidak tercantum di cakupan kartu. |
| `RehabilitationManagement/Services/RhbDocumentIntegrityGateway.cs` (baru) | Pembungkus tipis: `RegisterDraftAsync`, `EnsureMutableAsync`, `SignAsync`, `MarkCancelledAsync`, `FindAsync`, serta konstanta `RehabDocumentKinds`. Jenis yang bukan rehab, atau yang belum ditegakkan, ditolak dengan exception (kesalahan pemrograman, bukan input pengguna). |
| `Program.cs` | `AddScoped<RhbDocumentIntegrityGateway>()` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada endpoint baru. Timeline RM kini mencantumkan enam jenis rehab di `UnavailableKinds` dengan label yang terbaca. |
| Database | Tidak ada perubahan schema. Kolom `DocumentKind` sudah `int`. Tidak ada migration. |
| Keamanan/Auth | Memperketat: dokumen rehab yang sudah ditandatangani menjadi tidak bisa disunting. |

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — tidak ada endpoint yang dibuat atau diubah.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Review diff RM (`git diff --stat`) | 3 berkas RM, +47/−2; hanya penambahan di ujung enum, himpunan, dan switch | `PASS` | §3.2 |
| QBE conformance `-Mode Strict` (14 berkas `BE-RHB-004` + `BE-RHB-005`) | 0 violation | `PASS` | Dijalankan agent 2026-11-05 |
| `dotnet build` Debug, perintah owner (`-m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false`) dijalankan agent | `0 Error(s)`, 426 warning, 5 menit 10 detik, `EXIT=0` (12:25:26–12:30:37) | `PASS` | 0 warning di folder `RehabilitationManagement`. Warning di `ClinicalDocumentIntegrityService.cs` baris 595–601 (CS1573 pada `LockOpenDocumentsForEncounterAsync`) sudah ada sebelumnya dan bukan baris yang diubah task ini. DLL ditulis 12:30:34. |
| Uji runtime penandatanganan | — | `NOT RUN` | Belum ada layanan rehab yang memanggil gateway. Pembuktian runtime terjadi di `BE-RHB-009`. |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Nilai 1–46 tidak bergeser | Terpenuhi | Diff `ClinicalDocumentKind.cs` |
| (2) `DitegakkanUntuk(kind)` true untuk 47–52 | Terpenuhi (statis) | Keenamnya ada di `JenisYangDitegakkan`; `DitegakkanUntuk` membaca himpunan itu |
| (3) Gateway tidak menyimpan sendiri dan ikut transaksi pemanggil | Terpenuhi, dengan satu catatan | `RegisterDraftAsync` dan `MarkCancelledAsync` tidak menyimpan. **Catatan:** `SignAsync` meneruskan ke `ClinicalDocumentIntegrityService.SignAsync` milik RM, yang memang menyimpan sendiri. Agar tidak mengubah layanan RM, aturan ini dituliskan di keterangan kelas gateway: pemanggil yang mengubah status dokumen rehab bersamaan dengan tanda tangan wajib membuka transaksi lebih dulu. |
| (4) Timeline menyatakan 47–52 "belum tersedia" sampai `BE-RHB-015` | Terpenuhi (statis) | `JenisBelumTersedia` = seluruh jenis yang bukan bawaan dan belum punya pembaca. Belum ada pembaca rehab, sehingga keenamnya otomatis masuk. |
| DoD: kriteria terbukti | Terpenuhi | Tabel ini |
| DoD: bukti persetujuan RM dilampirkan (`evidence/`) | Terpenuhi (2026-11-06) | `evidence/RHB-OQ-012-persetujuan-rekam-medis.md`. Owner menyatakan Yoga Aji Pratama sudah menyetujui. Bentuk bukti: pernyataan owner, belum ada artefak dari Yoga sendiri. |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Penguncian saat kunjungan ditutup (`LockOpenDocumentsForEncounterAsync`) juga berlaku untuk dokumen rehab. Draf sesi rawat jalan yang belum ditandatangani saat kunjungan RJ ditutup akan menjadi *terkunci tanpa tanda tangan* dan tidak bisa ditandatangani lagi. Hal ini perlu diperhatikan `BE-RHB-012` (sesi) dan layar sesi `FE`: tampilkan peringatan sebelum kunjungan ditutup. |
| Masalah yang diketahui | Delta cakupan: `MedicalRecordTimelineService.cs` ikut diubah (§3.2). Perubahan ini hanya menambah label. |
| Risiko tersisa | Tanpa bukti `RHB-OQ-012`, perubahan pada himpunan milik RM belum tercatat resmi sebagai disetujui pemiliknya. |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu kali pemadatan konteks percakapan. Pekerjaan dilanjutkan dari build yang sudah selesai, tanpa penyuntingan ulang. |
| Status Git | `M` tiga berkas RM, `M Program.cs`, `?? Areas/HealthServices/RehabilitationManagement/` (bersama task lain) |
| Langkah berikutnya | Lampirkan bukti persetujuan RM ke `evidence/RHB-OQ-012.*`, lalu kartu ini bisa ✅. `BE-RHB-009` dan `BE-RHB-010` tetap menunggu `BE-RHB-008`. |
