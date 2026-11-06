# Laporan Live Browser Testing — CPPT Terintegrasi Layar Perawat Disamakan dengan Dokter (ISSUE-IGD-008)

| Metadata | Nilai |
| :--- | :--- |
| **Tanggal pengujian** | 5 November 2026 |
| **Menu** | Instalasi Gawat Darurat › Ruang Kerja Pemeriksaan IGD (perawat) › Asuhan Keperawatan › tab **Catatan Terintegrasi**; pembanding: Ruang Kerja Dokter IGD › Catatan Dokter › **CPPT Pasien** |
| **Yang diuji** | `FIX-IGD-008-02` — [plan repair 008](../../../docs/plan-repair/plan-repair-008-cppt-perawat-catatan-dokter-hilang.md) |
| **Source** | Frontend `RizkiV2` @ `739e05f68` + perubahan belum di-commit; backend `rizkiG` @ `30f1396d` |
| **Peramban** | Chromium build 1234, mode tampak (*headed*), `slowMo` 300 ms |
| **Akun** | **Perawat IGD** dan **Dokter IGD** (kredensial dari variabel lingkungan) |
| **Pasien** | Kunjungan `IGD-261023035629-36B0C8` — pasien pada lampiran pelapor; memuat 1 SOAP dokter yang sudah masuk CPPT dan 1 catatan perawat pada 4 Nov 2026 |
| **Skrip** | `QuilvianSystemFrontendDev/test-with-agy/igd/uji-issue-008-cppt-perawat-20261105/uji-cppt-perawat-dokter.mjs` |
| **Data yang ditulis** | **Tidak ada** (0 permintaan tulis di kedua sesi) |
| **Hasil** | **8/8 lulus** |

## 1. Ringkasan

Sebelum perbaikan, CPPT di layar perawat menaruh SOAP dokter di kolom **Catatan Perawat**. Layar
perawat mengenali catatan dokter dari kata "Dokter", padahal server menulis profesi baku "Doctor".
Sekarang tab Catatan Terintegrasi perawat memakai **komponen CPPT yang sama** dengan Ruang Kerja
Dokter. Catatan dokter masuk kolom Dokter, ada penyaring jenis catatan, dan kepala menampilkan
identitas serta jumlah catatan. Perawat tidak mendapat tombol verifikasi.

## 2. Endpoint

**[Tags("Health Services / Clinical Management / Patient Integrated Progress Note")]**

| Method | Path | Deskripsi | Auth | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/clinical-management/patient-integrated-progress-notes` | Daftar CPPT satu kunjungan | Bearer; `PatientIntegratedProgressNote : Read` | `encounterId`, `pageNumber=1`, `pageSize=100`, penyaring `noteKind`/`professionType` | `200` + daftar; `professionType` baku "Doctor"/"Nurse" |

## 3. Hasil

| ID | Skenario | Hasil | Status |
| --- | --- | --- | :---: |
| N1 | Kepala CPPT (perawat) | Nama pasien, "No. RM 00-00-02-80", "3 catatan" — bukan "0 catatan"/"No. RM -" | LULUS |
| N2 | Catatan dokter di kolom Dokter (perawat) | "Catatan Dokter (1)" memuat "Dokter Spesialis: dr. Jenah Ali (Dokter Penanggung Jawab)"; kolom Perawat tidak memuat catatan dokter | LULUS |
| N3 | Kotak "Aksi tidak tersedia" | Tidak ada | LULUS |
| N4 | Penyaring | Tampil (Semua/Dokter/Perawat/Profesi Lain); "Dokter" menyembunyikan kolom Perawat | LULUS |
| N5 | Tombol verifikasi untuk perawat | Tidak ada | LULUS |
| N6 | Galat dan penulisan (perawat) | 0 / 0 | LULUS |
| D1 | CPPT Pasien di Ruang Kerja Dokter | Tampilan sama; kepala benar; "Catatan Dokter (1)" sama dengan layar perawat | LULUS |
| D2 | Galat dan penulisan (dokter) | 0 / 0 | LULUS |

Catatan: "3 catatan" pada kepala menghitung seluruh catatan kunjungan, termasuk yang berada di luar
tanggal 4 November.

## 4. Bukti

| Berkas (folder skrip) | Isi |
| --- | --- |
| `screenshots/01-perawat-cppt.png` | Layar perawat: penyaring, bulan November 2026, kolom Dokter (1) dan Perawat (1) |
| `screenshots/02-perawat-filter-dokter.png` | Penyaring "Dokter" aktif |
| `screenshots/03-dokter-cppt.png` | Ruang Kerja Dokter: tampilan sama |
| `hasil-*.json` | Hasil terukur per skenario |

## 5. Yang belum diuji

Riwayat pada tab **SOAP** perawat (komponen lama mode `soapOnly`, `FIX-IGD-008-01`) belum diuji di
browser. Perbaikannya terbatas pada fungsi pengelompokan profesi, dan ESLint bersih.

Laporan ini adalah bukti uji pengembang, **bukan** UAT.
