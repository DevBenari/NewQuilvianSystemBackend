# Laporan UAT — Amandemen `0.4.0` (Indikator ringkasan pulang dan rencana kontrol)

| Field | Isi |
|---|---|
| Gelombang | `MVP-9` (`BE-47`, `BE-48`, `FE-19`) dan `MVP-10` (`BE-49`, `BE-50`, `BE-51`) |
| Tanggal | 11 November 2026 |
| Kontrak | API `0.4.0` (`RM-DEC-108`) |
| Lingkungan | Backend lokal (kode `BE-47`–`BE-51`, belum di-commit; build 11 November 11.44, lebih baru dari seluruh source yang berubah) `https://localhost:7184` terhadap devYoga (`QuilvianNewDevYoga`, riwayat migration 400); dinyalakan dengan `Seeders__ContinueOnFailure=true`. Frontend `next dev` port 3710 untuk `FE-19` (E2E API tiruan) |
| Akun | Superadmin (`SeedSuperAdmin`, kredensial dibaca dari konfigurasi, tidak dicetak); satu akun **non-superadmin** yang diberikan Yoga Aji Pratama untuk uji `403` (sandi dipakai lewat variabel lingkungan sekali jalan, tidak dicetak, tidak disimpan) |
| Sifat uji | **Baca saja.** Tidak ada penulisan ke database selain login. Skrip: scratchpad sesi, `uat-mvp10.mjs` |

## 1. Keadaan devYoga yang menjadi dasar

| Hal | Keadaan | Bukti |
|---|---|---|
| Migration `20261102050639_AddStructuredFollowUpToInpDischargeSummary` | Diterapkan | Kueri baca-saja `__EFMigrationsHistory` |
| `InpEpisode` | **0 baris** (tidak ada episode Rawat Inap sama sekali) | Kueri baca-saja |
| `InpDischargeSummary` | **0 baris** | Kueri baca-saja |
| Migration tertunda | Tujuh migration SDM (sudah tercatat sebelumnya). Akibatnya worker SLA HR (`HrServiceSlaMonitoringWorker`) mencatat galat `42703 column t.PriorityTier does not exist` saat startup — **`EXISTING / ENVIRONMENT ISSUE`**, tidak terkait rekam medis | Log backend |

## 2. Hasil

| Kasus | Harapan | Hasil | Klasifikasi |
|---|---|---|---|
| `GET` indikator tanpa sesi | `401` | `401` | `PASS` |
| `GET /filters/metadata` (superadmin) | `200`, empat kolom, 366 hari | `200`; `within24h`, `between24And48h`, `over48h`, `notYetDue`; `maxPeriodDays` 366 | `PASS` |
| `GET` Oktober 2026 | `200`, bukan `503` — pembaca `BE-50` berjalan di PostgreSQL | `200`, 73 ms; `totalData` 0; `dataNotes` lengkap; `generatedAt` terisi | `PASS` |
| `GET` setahun 2026 (365 hari) dan tepat 366 hari | `200` | `200`, masing-masing 25 ms | `PASS` |
| Periode kosong / terbalik / 367 hari / `pageSize` 101 | `400` berpesan kontrak (`VAL-RM-C01`–`C04`) | Keempatnya `400` dengan pesan persis kontrak | `PASS` |
| `doctorId` tidak dikenal | `200` kosong, bukan `400`/`404` | `200`, `totalData` 0 | `PASS` |
| Bentuk paging | `PagedResult` project | `pageNumber`, `totalPage`, `totalData` (catatan rilis C3, penyegaran `0.4.1`) | `PASS` |
| **Akun non-superadmin tanpa izin** (`AT-RM-94`) | `403` | Login `200`; tarikan **`403`** "Anda tidak memiliki akses ke menu atau fitur ini." | `PASS` |
| Audit | Satu baris per tarikan berhasil, tanpa nama dokter atau data pasien; tarikan ditolak tidak diaudit sebagai tarikan | 8 baris untuk 8 tarikan berhasil (skrip dijalankan dua kali); contoh: "Menarik indikator ketepatan ringkasan pulang periode 2026-10-01 s.d. 2026-10-31, saringan dokter -, profil HR -, 0 baris dokter, ditarik 2026-11-11T06:37:30Z." | `PASS` |
| Galat endpoint indikator di log backend | Tidak ada | Tidak ada | `PASS` |
| `UAT-24` rencana kontrol tampil di berkas (`FE-19`) | Bagian tampil di layar | E2E API tiruan 2 lulus (`FE-19`) | `PASS` (API tiruan) |

## 3. Yang tetap terbuka dan terlacak

| Butir | Sebab | Kapan ditutup |
|---|---|---|
| Angka indikator **berisi** dari data nyata (`AT-RM-95`, `96`, `99`–`101` di lingkungan nyata) | devYoga tidak punya episode Rawat Inap | Saat ada pasien Rawat Inap yang pulang di devYoga atau lingkungan uji lain; logikanya sudah terbukti di uji integrasi `BE-50` |
| Kinerja periode panjang pada volume data nyata | Tabel kosong; 25–73 ms belum bermakna | Sama dengan butir di atas |
| `UAT-24` terhadap data nyata | `InpDischargeSummary` kosong | Sama dengan butir di atas |
| Tanda tangan ulang `409` (`BE-48`) lewat HTTP nyata | Tidak ada ringkasan pulang untuk ditandatangani; uji ini juga akan menulis | Saat data Rawat Inap tersedia; logikanya terbukti `AT-RM-107` |

## 4. Kebersihan sesudah uji

Backend 7184/5107 dan `next dev` 3710 dimatikan; port bebas. Berkas rahasia sesi sementara dihapus.
Tidak ada kredensial di laporan ini maupun di berkas skrip.
