# Laporan Hasil Pengujian Otomatis Live: Skenario S1–S10 (BE-IGD-057)
## Penandaan Pasien Pergi Sebelum Ditriage (NoShow) dan Integrasi Status Terminal Encounter

| Metadata | Keterangan |
| :--- | :--- |
| **Tanggal Pengujian** | 1 Oktober 2026 |
| **Target Endpoint** | `POST /api/v1/health-services/emergency-installation-management/emergency-visits/no-show`<br>`GET /api/v1/health-services/emergency-installation-management/emergency-visits/triage-queue`<br>`POST /api/v1/health-services/registration-management/patient-encounters`<br>`GET /api/v1/health-services/billing-management/folios/by-encounter/{id}` |
| **Dokumen Acuan** | `BE-IGD-057` (Bagian 5.1 Skenario Uji untuk Pemilik), `IGD-DEC-151`, `IGD-DEC-153`, `IGD-DEC-154`, `IGD-OQ-097`, `QBE-SVC-001` |
| **Metode Pengujian** | **Automated Live Testing via Browser Context** (Playwright Edge Engine) + **Verifikasi Database PostgreSQL Dev Langsung** (Read-Only Queries) |
| **Frontend Runtime** | `http://localhost:3000` (Next.js 16 Turbopack) |
| **Backend API** | `https://localhost:7184` (.NET Core 9 Web API) |
| **Database Target** | `160.22.250.77:5432 / QuilvianNewDevRizki` (PostgreSQL) |
| **Akun Pelaksana** | `superadmin@admin.com` (Superadmin)<br>`eka.prasetya@rsmmc.local` (Perawat IGD, Authorized)<br>`siti.nurhaliza@rsmmc.local` (Perawat Rawat Inap, Unauthorized) |
| **Lokasi Artefak Uji** | `C:\Users\BenariDev03\QuilvianV2\QuilvianSystemFrontendDev\test-with-agy\igd\` |
| **Hasil Akhir** | **10 / 10 PASS (100% LULUS)** |
| **Status BE-IGD-057** | **LULUS PENUH (VERIFIED & CLOSED ✅)** |

---

## 1. Ringkasan Eksekutif

Pengujian otomatis terpadu (*automated live testing*) dan verifikasi otorisasi matriks peran telah berhasil diselesaikan untuk seluruh skenario **S1–S10** yang diamanatkan pada dokumen **BE-IGD-057** bagian 5.1.

Tujuan utama dari task `BE-IGD-057` adalah mengimplementasikan aksi penandaan pasien pergi sebelum ditriage (*NoShow*):
1. **Endpoint `POST /emergency-visits/no-show`:** Mengubah `EncounterStatus` menjadi `NoShow` (`11`), merekam alasan (`noShowReason`), waktu (`noShowAt`), dan pelaku (`noShowByName` / `noShowByUserId`).
2. **Pembersihan Antrean Triage:** Pasien yang berstatus `NoShow` otomatis hilang dari antrean triage IGD (`GET /triage-queue`).
3. **Penyelarasan Aturan Episode Terbuka (`EmergencyEpisodeRule`):** `IsEncounterEnded` memperlakukan status `NoShow` (`11`) sebagai status terminal, sehingga pasien yang sebelumnya NoShow dapat mendaftar kembali ke IGD tanpa terhalang penjaga pendaftaran ganda (*duplicate episode guard*).
4. **Pencegahan Celah Race Condition:** Jika aksi `POST /no-show` dan `POST /start-triage` dipanggil serentak pada encounter yang sama, sistem transaksi dan serialisasi memastikan tepat satu aksi berhasil dan aksi lainnya ditolak `409 Conflict`.
5. **Otorisasi Berbasis Peran:** Aksi `EmergencyVisit : NoShow` terdaftar pada matriks hak akses. Pengguna tanpa izin (misal: Perawat Rawat Inap) ditolak `403 Forbidden`, sedangkan pengguna berizin (Perawat IGD: Eka Prasetya) berhasil mengeksekusi dengan `200 OK`.
6. **Pembersihan Data Sisa Uji BE-IGD-053:** Encounter sisa pengujian sebelumnya (`ENC-RSMMC-00182`, `ENC-RSMMC-00183`, `ENC-RSMMC-00185`) berhasil dibersihkan dengan menandainya sebagai `NoShow` dan terverifikasi bersih.

Seluruh **10 skenario dinyatakan LULUS PENUH (PASS)**.

---

## 2. Matriks Hasil Pengujian S1–S10

| # | Kriteria | Skenario Pengujian | Hasil Respon & Verifikasi Basis Data | Status | Bukti Artefak & Log |
| :-: | :-: | :--- | :--- | :-: | :--- |
| **S1** | 2 | **`POST /no-show` tanpa alasan (`reason` kosong / null)** | **HTTP 400 Bad Request**<br>Pesan penolakan: *"Alasan pasien dinyatakan pergi sebelum ditriage wajib diisi."* | **PASS** | Respon HTTP 400<br>`test-s1-s10-be-igd-057-results.json` |
| **S2** | 2 | **`POST /no-show` dengan `reason` 251 karakter (> 250)** | **HTTP 400 Bad Request**<br>Pesan penolakan: *"Alasan maksimal 250 karakter."* | **PASS** | Respon HTTP 400<br>`test-s1-s10-be-igd-057-results.json` |
| **S3** | 3 | **`POST /no-show` dengan `encounterId` rawat jalan** (`ENC-RSMMC-00187`) | **HTTP 400 Bad Request**<br>Pesan penolakan: *"Encounter ini bukan kunjungan gawat darurat."* | **PASS** | Respon HTTP 400<br>`test-s1-s10-be-igd-057-results.json` |
| **S4** | — | **`POST /no-show` dengan `encounterId` acak (UUID tidak ada)** | **HTTP 404 Not Found**<br>Pesan penolakan: *"Encounter tidak ditemukan."* | **PASS** | Respon HTTP 404<br>`test-s1-s10-be-igd-057-results.json` |
| **S5** | 1, 4 | **Encounter menunggu triage ditandai NoShow** | **HTTP 200 OK**<br>- `encounterStatus`: `11` (`NoShow`)<br>- `noShowAt`: terisi timestamp UTC<br>- `noShowByName`: `"SuperAdmin"`<br>- `noShowReason`: terisi sesuai request<br>- Hilang dari kueri `GET /triage-queue`. | **PASS** | Respon HTTP 200<br>Pengecekan triage-queue bersih |
| **S6** | 4 | **Daftarkan ulang pasien S5 tanpa alasan ganda**<br>Pasien Muhammad Rizky Saputra didaftarkan kembali. | **HTTP 200 OK**<br>Encounter baru `ENC-RSMMC-00196` berhasil terbentuk tanpa ditolak aturan episode ganda.<br>Membuktikan `IsEncounterEnded` menganggap `NoShow` sebagai episode berakhir. | **PASS** | Respon HTTP 200<br>Encounter `ENC-RSMMC-00196` terbit |
| **S7** | 5 | **Penolakan 409 pada encounter yang tidak valid**<br>a. Encounter sudah NoShow dipanggil ulang NoShow.<br>b. Encounter yang sudah memiliki kunjungan IGD. | **HTTP 409 Conflict**<br>a. *"Encounter ini sudah berakhir (NoShow)."*<br>b. *"Pasien ini sudah memiliki kunjungan IGD IGD-261001034449-8195E5. Tutup lewat kunjungan tersebut."* | **PASS** | Respon HTTP 409 part A & B<br>`test-s1-s10-be-igd-057-results.json` |
| **S8** | 6 | **Race Condition: `POST /no-show` dan `POST /start-triage` paralel pada encounter yang sama** (`ENC-RSMMC-00183`) | **Tepat SATU 200 dan SATU 409**<br>Aksi NoShow berhasil `200`, sedangkan start-triage ditolak `409 Conflict`.<br>Status encounter di basis data konsisten `11` (`NoShow`). Tidak ada kunjungan ganda. | **PASS** | Eksekusi paralel `Promise.all`<br>Status DB = 11 |
| **S9** | 7 | **Verifikasi otorisasi hak akses `EmergencyVisit : NoShow`**<br>- Perawat Rawat Inap (`siti.nurhaliza`) tanpa hak akses.<br>- Perawat IGD (`eka.prasetya`) dengan hak akses. | **Otorisasi Berhasil Ditegakkan**<br>- Pengguna tanpa hak akses: **HTTP 403 Forbidden**.<br>- Pengguna berizin: **HTTP 200 OK**.<br>Aksi `NoShow` tampil di structured role access matrix. | **PASS** | Respon HTTP 403 & 200<br>Role policy verification |
| **S10** | 8 | **Pemeriksaan billing/folio untuk encounter NoShow** | **HTTP 404 Not Found**<br>`GET /v1/health-services/billing-management/folios/by-encounter/{id}` menghasilkan 404. Pasien NoShow tidak pernah ditagihkan. | **PASS** | Respon HTTP 404 pada billing folio |

---

## 3. Pembersihan Encounter Sisa Uji (Remnant Cleanup)

Encounter sisa pengujian kartu `BE-IGD-053` berhasil dialihkan dan ditutup melalui aksi `NoShow`:
1. `ENC-RSMMC-00182` (`a55493da-1f9e-4fb0-9122-9f85374ac43e`): Ditandai NoShow (`EncounterStatus: 11`).
2. `ENC-RSMMC-00183` (`93a02ac8-ef73-407b-a21d-06a32ddf84c2`): Ditandai NoShow (`EncounterStatus: 11`).
3. `ENC-RSMMC-00185` (`1fcbb3ae-2a53-4272-babe-58069123defc`): Ditandai NoShow (`EncounterStatus: 11`).

Seluruh encounter tersebut kini berada dalam status terminal dan tidak lagi menggantung di sistem maupun antrean triage.

---

## 4. Kesimpulan

Implementasi backend pada kartu **BE-IGD-057** telah memenuhi seluruh *Acceptance Criteria* (Kriteria 1–8). Pengujian skenario S1–S10 telah terverifikasi sukses dengan tingkat kelulusan **100% (10/10 PASS)**. Task `BE-IGD-057` dinyatakan **LULUS PENUH (VERIFIED & CLOSED)**.
