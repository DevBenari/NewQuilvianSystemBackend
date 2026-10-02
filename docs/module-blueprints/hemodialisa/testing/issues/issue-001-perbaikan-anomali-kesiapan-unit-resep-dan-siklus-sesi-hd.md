# Issue 001: Perbaikan Anomali Kesiapan Unit, Resep, dan Siklus Sesi Hemodialisa

| Parameter | Nilai |
|---|---|
| **ID Masalah** | `HMD-ISSUE-001` |
| **Blueprint Terkait** | `HMD-BP-001` (Hemodialisa) |
| **Tingkat Keparahan** | Tinggi (*High - Lifecycle Blocker*) |
| **Status** | **RESOLVED (Diperbaiki & Terverifikasi)** |
| **Tanggal Ditemukan** | 24 September 2026 |
| **Tanggal Diselesaikan** | 24 September 2026 |
| **Penyelesai** | Google Antigravity Agent |

---

## 1. Deskripsi Permasalahan

Saat dilakukan eksekusi *End-to-End Acceptance Testing* pada modul Hemodialisa berdasarkan `acceptance-test-matrix.md`, ditemukan beberapa kendala teknis dan validasi yang menghalangi transisi status siklus hidup sesi dialisis:

1. **Inkonsistensi Property Name Kesiapan Unit**:
   Endpoint `POST /api/v1/health-services/hemodialysis-management/hemodialysis-unit-readiness` membutuhkan `ReadinessDate`. Penggunaan nama properti `checkDate` menyebabkan nilai tanggal terikat (*model bound*) ke nilai *default* `0001-01-01`. Akibatnya, gerbang kesiapan sesi `HMD-VAL-042` menolak pernyataan sesi siap karena tanggal lembar kesiapan tidak cocok dengan tanggal jadwal sesi (`2026-09-24`).
2. **Ketiadaan Tanggal Uji Laboratorium Air**:
   Butir kesiapan pengolahan air (*Water Treatment*) menuntut nilai `ResultDate` terisi sesuai masa berlaku yang ditetapkan kebijakan rumah sakit (720 jam). Ketiadaan nilai ini memicu penolakan `HMD-VAL-102`.
3. **Urutan Pemanggilan Post-HD vs Complete Sesi**:
   Backend menerapkan invariant `HMD-VAL-060` pada metode `CompleteAsync`, yang memeriksa ketersediaan catatan asesmen Pasca-HD (`HmdAssessmentPhase.Post`) dan penetapan disposisi pasien sebelum sesi dinyatakan selesai secara fisik. Pemanggilan `Complete` mendahului `Post-HD` menghasilkan penolakan transisi status.
4. **Konflik Assertion Unit Test Layanan Penunjang**:
   Pada `tests/unit/inpatient-supporting-service-v2.test.mjs`, pengujian mengasumsikan hanya 2 layanan penunjang backend yang tersedia. Pengaktifan penunjang Hemodialisa (`isAvailable: true`) menyebabkan kegagalan assertion jumlah layanan aktif (seharusnya 3: Lab, Rad, Hmd).

---

## 2. Analisis Akar Masalah (Root Cause Analysis)

- **Backend Binding & Contract Invariant**: Model DTO C# `CreateHmdUnitReadinessRequest` secara ketat memetakan `ReadinessDate`. Model validasi domain rumah sakit Quilvian secara sengaja didesain *fail-safe*: unit hemodialisa tidak boleh melayani pasien jika tidak ada bukti tertulis pengujian air RO yang sah dan masih berlaku.
- **Workflow State Machine**: Urutan klinis di ruang dialisis mengharuskan perawat menimbang pasien setelah jarum AV fistula dicabut, memeriksa tanda vital, dan menentukan apakah pasien boleh pulang atau kembali ke bangsal rawat inap (`post-hd`), baru kemudian mencatat bahwa tindakan dialisis telah tuntas secara fisik (`complete`).
- **Synchronous Unit Test Assertion**: Penambahan kapabilitas baru pada sistem rawat inap (`FE-HMD-07`) belum disinkronkan dengan berkas uji regresi lama.

---

## 3. Tindakan Perbaikan dan Remediasi

1. **Memperbarui Unit Test Layanan Penunjang**:
   - Memodifikasi `QuilvianSystemFrontendDev/tests/unit/inpatient-supporting-service-v2.test.mjs` baris 41–55 agar mengakui 3 layanan aktif (Laboratorium, Radiologi, Hemodialisa) dan 3 layanan non-aktif (Gizi, Rehabilitasi Medis, Bank Darah).
2. **Penyelarasan Kontrak Kesiapan Unit**:
   - Menyelaraskan seluruh pengiriman permintaan kesiapan unit pada field `readinessDate: todayStr`.
   - Mengisi data uji lab air: `resultDate: todayStr`, `referenceNumber: "LAB-H2O-2026-09"`, `note: "Memenuhi baku mutu Permenkes"`.
3. **Penyelarasan Alur Pasca-HD dan Penyelesaian Sesi**:
   - Memastikan asesmen pasca-HD (`PUT /sessions/{id}/post-hd`) dieksekusi sebelum panggilan penyelesaian fisik (`POST /sessions/{id}/complete`).
4. **Pemberian Hak Akses Modul Hemodialisa pada Akun Dokter**:
   - Membuka izin 18 controller dan 67 action Hemodialisa pada posisi Dokter Umum (`cd1cd442-f971-a117-19c1-ae8809230138`) melalui skrip otorisasi terkelola.

---

## 4. Hasil Verifikasi Pasca-Perbaikan

Setelah perbaikan diterapkan:
- **Unit Tests**: 227/227 test lulus (**100% Pass**).
- **E2E Acceptance Test**: 41/41 skenario siklus hidup lulus (**100% Pass**).
- **Playwright Browser UI Test**: 7/7 rute antarmuka pengguna memuat sempurna (**0 Error, 0 404**).
- Seluruh invariant klinis, penguncian rekam medis (HTTP 423), dan handoff penagihan (isBillable: true) beroperasi sesuai rancangan.
