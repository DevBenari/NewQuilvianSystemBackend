# Laporan Perubahan Backend — `BE-NV-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-NV-004` |
| Judul | API interval ukur ulang TTV |
| Slice | `MVP-3` — interval ukur ulang TTV |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/backend-roadmap.md` — kartu `BE-NV-004` |
| Trace | `FR-NV-020`–`022`; `NV-DEC-010`; `G-01`, `G-03`; `VAL-NV-09`–`13`; `contracts/api-contract.md` §2 |
| Contract version | `0.1.0` — `approved` (Rizki Gunawan, 2026-10-28) |
| Dependency | `BE-NV-001` ✅ (tabel `NvwVitalSignInterval` sudah ada) |
| Klasifikasi | `LIGHT` — satu repository, berkas diubah 4–8, logika sedang, endpoint baru sesuai kontrak, tanpa migration, tanpa UI |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/NurseViewManagement/{Controllers,DTOs,Services}`, `Program.cs` (registrasi service), laporan dan status roadmap |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `8e4991cb` (branch `rizkiG`); source di-commit Rizki sebagai `91a3f8e2` |
| Tanggal | 2026-11-02 |
| Status | 🟡 **SEBAGIAN** — QBE Strict `PASS`, build Rizki lulus, uji gabungan 12 dari 13 `PASS`. **Gagal: no. 13** — rujukan pencabutan interval tidak tersimpan di mana pun (§7). Perbaikan sudah ditulis sesuai keputusan Rizki 2 Nov, tetapi **belum di-build dan belum diuji ulang**. Verifikasi developer, bukan UAT |

---

## 1. Masalah yang diperbaiki

Tabel interval sudah ada (`BE-NV-001`) dan sengaja kosong, tetapi belum ada cara untuk mengisinya.
Selama kosong, papan tidak pernah bisa menandai TTV yang lewat jadwal.

**Contoh.** Komite Keperawatan mengesahkan interval EWS Sedang 240 menit lewat
SK-KOMKEP/012/X/2026. Tanpa API ini, angka itu hanya bisa masuk lewat SQL, yang dilarang.

---

## 2. Proses bisnis

**Tujuan:** admin IT memasukkan interval yang disahkan Komite Keperawatan. Tanpa interval, sistem
aman-gagal: tanda lewat jadwal tidak tampil.

**Pelaku:** admin IT (`NurseViewVitalSignInterval : Update`); kepala ruang atau perawat hanya
membaca bila diberi `: Read`.

**Langkah normal:**

1. Admin membaca himpunan aktif. Saat kosong: `IsConfigured = false`, `Items` kosong.
2. Admin mengirim interval untuk tingkat yang disahkan, misalnya Rendah 720 dan Sedang 240, beserta
   nomor SK dan tanggal pengesahan.
3. Sistem menandai hapus seluruh baris aktif lama, lalu menyisipkan baris baru, dalam satu transaksi.
   Baris lama tidak ditimpa, sehingga riwayatnya tetap ada.
4. Bila kemudian admin mengirim Sedang 180 dan Tinggi 60, tingkat Rendah tidak lagi punya interval.
   Ini ganti-seluruh, bukan gabung.
5. Mengirim `Items` kosong berarti mencabut seluruh interval. Rujukan pengesahan tetap wajib.

**Jalur tidak normal (semua `400`, tanpa perubahan data):**

| Kejadian | Pesan | Kode |
| --- | --- | --- |
| Tingkat `Unknown` atau di luar 1–4 | "Tingkat risiko EWS tidak dikenal." | `VAL-NV-09` |
| Tingkat sama dikirim dua kali | "Setiap tingkat risiko hanya boleh diisi satu kali." | `VAL-NV-10` |
| Menit di bawah 1 atau di atas 1440 | "Interval harus antara 1 dan 1440 menit (24 jam)." | `VAL-NV-11` |
| Rujukan kosong atau lebih dari 200 karakter | "Nomor surat/notulen pengesahan Komite Keperawatan wajib diisi." | `VAL-NV-12` |
| Tanggal pengesahan sesudah hari ini (WIB) | "Tanggal pengesahan tidak boleh melewati hari ini." | `VAL-NV-13` |
| Tanggal pengesahan tidak dikirim | "Tanggal pengesahan wajib diisi." | `VAL-NV-13` |
| `Items` tidak dikirim sama sekali | "Daftar interval wajib dikirim. Kirim daftar kosong untuk mencabut seluruh interval." | `NV-INTERVAL-ITEMS` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Governance: `AGENTS.md`, `CLAUDE.md`; suite skill `TEST_POLICY.md`, `BACKEND_ENGINEERING_CONTRACT.md`,
  registry prefix, status roadmap
- Kontrak: `contracts/api-contract.md` §2, `validation-matrix.md`, `permission-audit-matrix.md`,
  `02-backend-architecture.md`, `data/data-dictionary.md`
- Pola: `NvwDisplayDeviceController.cs`, `NvwDisplayDeviceService.cs`, `NvwHttp.cs`, `NvwResult.cs`,
  `NursingShiftController.cs`, `DailyMonitoringService.ReplaceShiftSetAsync`,
  `NumberSeriesAllocator.AcquireSeriesLockAsync`, `ReferralPartnerEligibility.Today`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/NurseViewManagement/Services/NvwVitalSignIntervalService.cs` | **Baru.** `GetActiveAsync`, `ReplaceAsync` |
| `Areas/HealthServices/NurseViewManagement/Controllers/NvwVitalSignIntervalController.cs` | **Baru.** `GET /` dan `PUT /`; tanpa akses `ApplicationDbContext` langsung; log aksi memuat aktor, waktu, dan pesan. Payload yang dikirim ke `LoggerService` tidak ditulis ke log (§7) |
| `Areas/HealthServices/NurseViewManagement/DTOs/NvwVitalSignIntervalDtos.cs` | **Baru.** Request dan response sesuai api §2 |
| `Areas/HealthServices/NurseViewManagement/Services/NvwResult.cs` | +13 baris: kode dan pesan `VAL-NV-09`–`13` persis matriks, ditambah dua pesan delta di §3.3 |
| `Program.cs` | +1 baris: `AddScoped<NvwVitalSignIntervalService>()` |
| `Areas/HealthServices/NurseViewManagement/Services/NvwVitalSignIntervalService.cs` (perbaikan no. 13) | `BuildAuditMessage` menyusun pesan log. Contoh: "Interval ukur ulang TTV diganti: Sedang=240, Kritis=15. Rujukan: SK-KOMKEP/012/X/2026; disahkan 2026-10-20." atau "Seluruh interval dicabut. Rujukan: …; disahkan …." |
| `Areas/HealthServices/NurseViewManagement/Controllers/NvwVitalSignIntervalController.cs` (perbaikan no. 13) | Payload log yang tidak pernah tertulis diganti pesan dari `BuildAuditMessage`. Pesan hanya disusun bila simpan berhasil, jadi `items:[null]` tetap `400`. Respons API tidak berubah |
| `Areas/HealthServices/NurseViewManagement/Controllers/NvwHttp.cs` (perbaikan no. 13) | Parameter opsional `logMessage` pada `RespondAsync`. Pemanggil lain tidak berubah |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Dua endpoint baru persis api §2. **Delta 1:** `Items` yang tidak dikirim ditolak `400`, supaya permintaan yang cacat tidak terbaca sebagai pencabutan seluruh interval. **Delta 2:** `RatifiedOn` yang tidak dikirim diberi pesan sendiri, karena matriks hanya memuat pesan tanggal masa depan |
| Database | Tanpa migration. Menulis `NvwVitalSignInterval`. Penyimpanan dua langkah dalam satu transaksi: baris lama ditandai hapus dan disimpan lebih dulu, baru baris baru disisipkan. Tanpa urutan ini, index unik parsial `(EwsRiskLevel) WHERE IsDelete = false` bisa menolak penyisipan. `pg_advisory_xact_lock` (pola `NumberSeriesAllocator`) mencegah dua admin mencampur dua himpunan |
| Keamanan/Auth | `[AccessPermission]` dan `[AccessAction]` berpasangan untuk `NurseViewVitalSignInterval : Read/Update`. Tidak ada peran yang di-hardcode |
| Angka bawaan (`G-01`) | Tidak ada seed dan tidak ada nilai bawaan di kode |

---

## 4. Dokumentasi endpoint

#### Health Services / Nurse View Management / Vital Sign Interval

Base URL: `api/v1/health-services/nurse-view-management/vital-sign-intervals`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Himpunan aktif beserta rujukan, pencatat, dan `IsConfigured` | `NurseViewVitalSignInterval : Read` |
| `PUT` | `/` | Ganti seluruh himpunan; `Items` kosong berarti mencabut | `NurseViewVitalSignInterval : Update` |

Kode status: `200` berhasil · `400` isian salah · `403` tanpa hak akses.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 5 berkas | VIOLATION 0 · REVIEW 0 · `Final result: PASS`, `Files evaluated: 5` | `PASS` | Keluaran checker 2026-11-02. Percobaan pertama lewat `-File` hanya mengevaluasi 1 berkas dan tidak dipakai |
| Pasangan `[AccessAction]` ↔ `[AccessPermission]` ↔ `ControllerName` | 2/2 cocok | `PASS` | Pemeriksaan manual atas controller |
| Nol komentar baris | Nol hasil | `PASS` | `grep` atas 3 berkas baru |
| Prasyarat data | `NvwVitalSignInterval` 0 baris, 0 aktif, sehingga uji tidak menimpa angka yang ada | `PASS` | `SELECT` baca saja (`default_transaction_read_only=on`) 2026-11-02 |
| `dotnet build ./QuilvianSystemBackend.sln -p:RunAnalyzers=false` | "Build succeeded with 412 warning(s) in 1738,5s", tanpa error | `PASS (pernyataan owner)` | Tangkapan layar Rizki 2 Nov; `QuilvianSystemBackend.dll` 10.05 memuat `NvwVitalSignIntervalController`; proses backend mulai 10.06; `GET` tanpa login dijawab `401` |
| Uji gabungan layar + HTTP | No. 1–12 `PASS`, no. 13 `FAIL` | `FAIL` | [`FE-NV-004.md`](../frontend/FE-NV-004.md) §6; 16 cek HTTP validasi server ada pada no. 9 |
| `bash tools/authorization-verifier/verify-authorization.sh` | Belum | `NOT RUN` | Skrip menjalankan `dotnet build`, jadi perintahnya milik Rizki |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `FAIL` pada no. 13 saja. Skenario gabungan dengan layar ada di
[`FE-NV-004.md`](../frontend/FE-NV-004.md) §6, sesuai keputusan Rizki 2 Nov agar uji cukup sekali.
Bukti: `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-nv-004-20261102/run-2026-11-02T03-16-57-438Z/`.

**Yang terbukti untuk backend (2 Nov):**
- Ganti-seluruh: `GET` hanya memuat tingkat terbaru (`2:180`, `3:60`). `SELECT` baca saja menunjukkan dua baris lama bertanda hapus, dengan penghapus dan waktunya, dan angka lamanya tetap 720 dan 240.
- Validasi: 11 kasus penolakan dijawab `400` dengan pesan dan kode persis matriks, tanpa perubahan data.
- Batas: 1 dan 1440 menit serta tanggal hari ini diterima; pencabutan tanpa rujukan ditolak `VAL-NV-12`.
- Hak akses: akun LOKET mendapat `403` pada `GET` dan `PUT`.
- Keadaan akhir: `GET` `200`, nol baris aktif, enam baris riwayat uji.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `NV-AC-004` tanpa interval tidak ada tanda | Terpenuhi untuk lingkup task ini | No. 1: `IsConfigured = false`. Tanda di papan lingkup `BE-NV-005` |
| `VAL-NV-09`–`13` | Terpenuhi | No. 9 |
| `UAT-08` Kritis 0 ditolak | Sisi API terverifikasi developer; UAT belum dijalankan | No. 9, kasus "Menit 0" |
| `IsConfigured = false` saat kosong | Terpenuhi | No. 1, 8 |
| Rujukan pencabutan tercatat (`G-03`) | **Belum terpenuhi** | No. 13; lihat §7 |
| DoD — build lulus | Terpenuhi | §5, pernyataan owner + DLL |
| DoD — laporan ada | Terpenuhi | Berkas ini |

**Backend Governance Preflight:** Area `HealthServices` · Module `NurseViewManagement` · prefix `Nvw`
`ACTIVE` · applicability `NEW CODE`; `Program.cs` = `TOUCHED LEGACY` (hanya tambahan satu baris).

**QBE yang berlaku dan dipatuhi:** `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`,
`QBE-DTO-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DEL-001`.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Status migration/database | Tanpa migration. Uji meninggalkan enam baris `NvwVitalSignInterval` bertanda hapus berujukan `UJI-DEV-NV-004-*` di DB dev; nol baris aktif |
| Masalah yang diketahui | **No. 13: rujukan pencabutan tidak tersimpan di mana pun.** Pencabutan tidak menyisipkan baris, dan rujukan baris lama tidak boleh ditimpa, jadi tabel tidak punya tempat untuknya. `LoggerService.WriteAsync` memakai `data` hanya untuk mengambil `Path`, `UserId`, dan sejenisnya, sehingga payload tidak ditulis: pencarian `RatificationReference` di log hari uji menghasilkan nol baris. **Koreksi:** versi laporan ini sebelumnya, yang ikut commit `91a3f8e2`, menyatakan rujukan pencabutan tersimpan di log; pernyataan itu keliru. Yang tercatat hanya siapa dan kapan (`DeleteBy`, `DeleteDateTime`, baris log aksi). **Keputusan Rizki 2 Nov:** nomor SK, tanggal, dan angka ditulis ke pesan log aksi, tanpa migration dan tanpa mengubah `LoggerService`. Source sudah diubah; menunggu build. **Batas yang diterima:** berkas log hanya disimpan 14 berkas (`RetainedFileCountLimit`), jadi jejak ini tidak permanen. Catatan serupa untuk `BE-NV-002` §3.3: payload log perangkat juga tidak tertulis; kata sandi tetap tidak masuk log |
| Risiko tersisa | `GET` membaca rujukan dari baris aktif terbaru. Satu himpunan selalu memakai rujukan yang sama, dan kunci transaksi menjaga hal itu |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Source awal ada di commit `91a3f8e2`. Perbaikan no. 13 (`NvwHttp.cs`, controller, service), koreksi dokumen ini, dan status roadmap belum di-commit |
| Langkah berikutnya | Rizki mematikan backend, menjalankan `dotnet build ./QuilvianSystemBackend.sln -p:RunAnalyzers=false`, lalu menyalakannya lagi. Sesudah itu ulangi no. 13 saja: `node test-with-agy/nurse-view/uji-nv-004-20261102/ulang-no13.mjs` |
