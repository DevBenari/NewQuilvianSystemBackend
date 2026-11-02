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
| Commit backend saat dikerjakan | `8e4991cb` (branch `rizkiG`), perubahan belum di-commit |
| Tanggal | 2026-11-02 |
| Status | 🟡 **SEBAGIAN** — source lengkap dan QBE Strict `PASS` (5 berkas). **Belum:** `dotnet build` oleh Rizki dan uji HTTP. Build terakhir 1 Nov 15.55 lebih tua dari source ini (2 Nov 09.02), dan route masih menjawab `404` pada 2 Nov 09.27 |

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
| `Areas/HealthServices/NurseViewManagement/Controllers/NvwVitalSignIntervalController.cs` | **Baru.** `GET /` dan `PUT /`; tanpa akses `ApplicationDbContext` langsung; log aksi memuat aktor, isi, dan rujukan |
| `Areas/HealthServices/NurseViewManagement/DTOs/NvwVitalSignIntervalDtos.cs` | **Baru.** Request dan response sesuai api §2 |
| `Areas/HealthServices/NurseViewManagement/Services/NvwResult.cs` | +13 baris: kode dan pesan `VAL-NV-09`–`13` persis matriks, ditambah dua pesan delta di §3.3 |
| `Program.cs` | +1 baris: `AddScoped<NvwVitalSignIntervalService>()` |

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
| `dotnet build ./QuilvianSystemBackend.sln -p:RunAnalyzers=false` | Belum memuat source ini | `NOT RUN` | `bin/Debug/net9.0/QuilvianSystemBackend.dll` 1 Nov 15.55, sedangkan source 2 Nov 09.02 |
| Uji HTTP | Belum | `NOT RUN` | `GET` tanpa login dijawab `404` pada 09.27; bila route ada, jawabannya `401` |
| `bash tools/authorization-verifier/verify-authorization.sh` | Belum | `NOT RUN` | Dijalankan sesudah build |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `REQUIRED`. Skenario gabungan dengan layar ada di
[`FE-NV-004.md`](../frontend/FE-NV-004.md) §6, sesuai keputusan Rizki 2 Nov agar uji cukup sekali.
Skripnya sudah siap: `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-nv-004-20261102/uji-interval.mjs`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `NV-AC-004` tanpa interval tidak ada tanda | Terpetakan ke source; belum diuji | `IsConfigured = false` saat kosong. Tanda di papan lingkup `BE-NV-005` |
| `VAL-NV-09`–`13` | Terpetakan ke source; belum diuji | `NvwVitalSignIntervalService.ReplaceAsync` |
| `UAT-08` Kritis 0 ditolak | Terpetakan ke source; belum diuji | `VAL-NV-11` |
| `IsConfigured = false` saat kosong | Terpetakan ke source; belum diuji | `GetActiveAsync` |
| DoD — build lulus | **Belum terpenuhi** | Menunggu Rizki |
| DoD — laporan ada | Terpenuhi | Berkas ini |

**Backend Governance Preflight:** Area `HealthServices` · Module `NurseViewManagement` · prefix `Nvw`
`ACTIVE` · applicability `NEW CODE`; `Program.cs` = `TOUCHED LEGACY` (hanya tambahan satu baris).

**QBE yang berlaku dan dipatuhi:** `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`,
`QBE-DTO-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DEL-001`.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Status migration/database | Tanpa migration. Belum ada penulisan data dari task ini |
| Masalah yang diketahui | **Rujukan pencabutan hanya tersimpan di log aplikasi.** Pencabutan tidak menyisipkan baris, dan rujukan baris lama tidak boleh ditimpa, jadi tabel tidak punya tempat untuknya. Penghapus dan waktunya tetap tercatat pada baris lama. Bila Komite butuh jejak permanen, perlu perubahan kamus data dan migration; itu keputusan Rizki |
| Risiko tersisa | `GET` membaca rujukan dari baris aktif terbaru. Satu himpunan selalu memakai rujukan yang sama, dan kunci transaksi menjaga hal itu |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M Program.cs`, `M Areas/HealthServices/NurseViewManagement/Services/NvwResult.cs`, tiga berkas baru di `Areas/HealthServices/NurseViewManagement/`; laporan dan roadmap di `docs/module-blueprints/nurse-view/`. Belum di-commit |
| Langkah berikutnya | Rizki mematikan backend, menjalankan `dotnet build ./QuilvianSystemBackend.sln -p:RunAnalyzers=false`, lalu menyalakannya lagi. Sesudah itu uji gabungan `FE-NV-004` §6 dijalankan |
