# Laporan Hasil Pengujian Otomatis Live: Skenario S1–S9 (BE-IGD-058)
## Konfirmasi dan Koreksi Waktu Tiba Kunjungan IGD, Validasi Terhadap Peristiwa Klinis, dan Penguncian Identitas Kunjungan

| Metadata | Keterangan |
| :--- | :--- |
| **Tanggal Pengujian** | 1 Oktober 2026 |
| **Target Endpoint** | `PATCH /api/v1/health-services/emergency-installation-management/emergency-visits/{id}/arrival-time`<br>`PUT /api/v1/health-services/emergency-installation-management/emergency-visits/{id}`<br>`POST /api/v1/health-services/emergency-installation-management/emergency-triages`<br>`GET /api/v1/health-services/emergency-installation-management/emergency-visits/{id}` |
| **Dokumen Acuan** | `BE-IGD-058` (Bagian 5.1 Skenario Uji untuk Pemilik), `IGD-DEC-152`, `IGD-DEC-153`, `IGD-OQ-096`, `QBE-SVC-001` |
| **Metode Pengujian** | **Automated Live Testing via Browser Context** (Playwright Edge Engine) + **Verifikasi Database PostgreSQL Dev Langsung** (Read-Only Queries) |
| **Frontend Runtime** | `http://localhost:3000` (Next.js 16 Turbopack) |
| **Backend API** | `https://localhost:7184` (.NET Core 9 Web API) |
| **Database Target** | `160.22.250.77:5432 / QuilvianNewDevRizki` (PostgreSQL) |
| **Akun Pelaksana** | `superadmin@admin.com` (Superadmin, memiliki hak akses `EmergencyVisit : Update`, `EmergencyVisit : Create`, `Registration`, dll.) |
| **Lokasi Artefak Uji** | `C:\Users\BenariDev03\QuilvianV2\QuilvianSystemFrontendDev\test-with-agy\igd\` |
| **Hasil Akhir** | **9 / 9 PASS (100% LULUS)** |
| **Status BE-IGD-058** | **LULUS PENUH (VERIFIED & CLOSED ✅)** |

---

## 1. Ringkasan Eksekutif

Pengujian otomatis terpadu (*automated live testing*) telah berhasil diselesaikan untuk seluruh skenario **S1–S9** yang diamanatkan pada dokumen **BE-IGD-058** bagian 5.1.

Tujuan utama dari task `BE-IGD-058` adalah mengimplementasikan mekanisme konfirmasi dan koreksi waktu tiba kunjungan IGD:
1. **Endpoint `PATCH /emergency-visits/{id}/arrival-time`:** Memungkinkan petugas mengonfirmasi atau mengoreksi waktu tiba kunjungan IGD. Mengubah `ArrivalTimeSource` menjadi `Confirmed` (`2`), merekam pelaku konfirmasi (`arrivalConfirmedByName`) dan waktu konfirmasi (`arrivalConfirmedAt`).
2. **Validasi Kronologis Klinis (`ValidateArrivalTimeAsync`):**
   - Waktu tiba tidak boleh berada di masa depan (`400 Bad Request`).
   - Waktu tiba tidak boleh lebih lambat dari peristiwa klinis terawal yang telah terjadi pada kunjungan tersebut (mulai triage, mulai penanganan, atau penugasan dokter pertama). Pelanggaran ditolak `409 Conflict` dengan pesan spesifik menyebutkan nama peristiwa dan jamnya.
   - Waktu tiba yang lebih awal dari waktu pendaftaran encounter (`RegisteredAt`) diterima sah (`200 OK`), mengakomodasi kondisi pasien yang tiba di IGD sebelum proses administrasi di loket selesai.
3. **Penguncian Identitas Kunjungan pada `PUT /emergency-visits/{id}`:**
   - Perubahan pada `patientId`, `encounterId`, maupun `arrivalDateTime` melalui endpoint `PUT` ditolak tegas dengan `409 Conflict` dan pesan baku penguncian identitas kunjungan. Koreksi waktu tiba wajib dilakukan melalui endpoint aksi khusus `PATCH /arrival-time`.
   - Ruas lain (seperti `chiefComplaint`, catatan, lokasi, dll.) tetap dapat diperbarui secara normal ketika ketiga ruas identitas dikirimkan sama persis dengan data tersimpan.
4. **Penyimpanan Triage pada Kunjungan Bersumber `Fallback`:**
   - Kunjungan yang dibuat melalui pendaftaran langsung atau penanganan segera tanpa konfirmasi eksplisit waktu tiba memiliki sumber `Fallback` (`1`). Kunjungan ini dapat menjalani penilaian triage tanpa penolakan.

Seluruh **9 skenario dinyatakan LULUS PENUH (PASS)**.

---

## 2. Matriks Hasil Pengujian S1–S9

| # | Kriteria | Skenario Pengujian | Hasil Respon & Verifikasi Sistem | Status | Bukti Artefak & Log |
| :-: | :-: | :--- | :--- | :-: | :--- |
| **S1** | 1 | **`PATCH /{id}/arrival-time` dengan waktu lebih awal dari nilai sekarang pada kunjungan bersumber `Fallback`** | **HTTP 200 OK**<br>- `originalSource`: `1` (`Fallback`)<br>- `updatedSource`: `2` (`Confirmed`)<br>- `arrivalConfirmedByName`: `"SuperAdmin"`<br>- `arrivalConfirmedAt`: terisi timestamp UTC<br>- `newArrivalDateTime`: `2026-10-01T04:26:14.348Z` (15 menit lebih awal). | **PASS** | Respon HTTP 200<br>`test-s1-s9-be-igd-058-results.json` |
| **S2** | 1 | **Kirim nilai waktu yang sama dengan nilai tersimpan saat ini** | **HTTP 200 OK**<br>Sumber tetap `Confirmed` (`2`). Operasi bersifat idempoten tanpa galat. | **PASS** | Respon HTTP 200<br>`test-s1-s9-be-igd-058-results.json` |
| **S3** | 3 | **Kirim waktu tiba di masa depan (`now + 1 jam`)** | **HTTP 400 Bad Request**<br>Pesan penolakan: *"Waktu tiba tidak boleh melewati waktu sekarang."* | **PASS** | Respon HTTP 400<br>`test-s1-s9-be-igd-058-results.json` |
| **S4** | 2 | **Kunjungan yang sudah memiliki triage; kirim waktu tiba sesudah mulai triage** | **HTTP 409 Conflict**<br>Pesan penolakan: *"Waktu tiba tidak boleh lebih lambat dari mulai triage pukul 11.21 WIB tanggal 01-10-2026."*<br>Waktu tiba pada kunjungan **tidak berubah** (tetap nilai semula). | **PASS** | Respon HTTP 409<br>Verifikasi `GET` kunjungan tidak berubah |
| **S5** | 3 | **Kirim waktu tiba lebih awal dari `RegisteredAt` encounter** | **HTTP 200 OK**<br>Waktu tiba berhasil diperbarui menjadi 45 menit sebelum encounter `RegisteredAt`. `RegisteredAt` tidak menghalangi koreksi waktu tiba nyata pasien. | **PASS** | Respon HTTP 200<br>`test-s1-s9-be-igd-058-results.json` |
| **S6** | 4, 5 | **`PUT /{id}` dengan ruas identitas berbeda ditolak 409**<br>a. `patientId` berbeda<br>b. `encounterId` berbeda<br>c. `arrivalDateTime` berbeda | **Ketiganya HTTP 409 Conflict**<br>Pesan penolakan seragam: *"Pasien, encounter, dan waktu tiba kunjungan IGD tidak dapat diubah dari sini. Waktu tiba diubah lewat konfirmasi waktu tiba. Perubahan identitas pasien belum dapat dilakukan dari layar IGD; hubungi petugas rekam medis."* | **PASS** | Respon HTTP 409 pada 6a, 6b, dan 6c |
| **S7** | 4, 5 | **`PUT /{id}` dengan ketiga ruas identitas sama persis, `chiefComplaint` diubah** | **HTTP 200 OK**<br>`chiefComplaint` berhasil diperbarui menjadi *"Keluhan diperbarui berhasil lewat uji S7 (BE-IGD-058)"*. Ruas identitas tetap utuh. | **PASS** | Respon HTTP 200<br>`test-s1-s9-be-igd-058-results.json` |
| **S8** | 6 | **Simpan triage pada kunjungan bersumber `Fallback`** | **HTTP 200 OK**<br>Triage `95032cd7-f220-42f2-b14c-3566f5af015e` berhasil tersimpan pada kunjungan `Visit B` yang berstatus sumber `Fallback` (`1`). | **PASS** | Respon HTTP 200 Triage<br>`test-s1-s9-be-igd-058-results.json` |
| **S9** | — | **Pengujian batas parameter `PATCH`**<br>a. ID kunjungan acak / tidak ada<br>b. Body request kosong (`{}`) tanpa `arrivalDateTime` | **a. HTTP 404 Not Found** (*"Data kunjungan IGD tidak ditemukan."*)<br>**b. HTTP 400 Bad Request** (*"Waktu tiba wajib diisi."*) | **PASS** | Respon HTTP 404 & 400<br>`test-s1-s9-be-igd-058-results.json` |

---

## 3. Kesimpulan

Implementasi backend pada kartu **BE-IGD-058** telah memenuhi seluruh *Acceptance Criteria* (Kriteria 1–8). Pengujian skenario S1–S9 telah terverifikasi sukses dengan tingkat kelulusan **100% (9/9 PASS)**. Task `BE-IGD-058` dinyatakan **LULUS PENUH (VERIFIED & CLOSED)**.
