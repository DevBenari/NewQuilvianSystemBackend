# Laporan Hasil Pengujian Live Browser: Skenario S1–S6 (BE-IGD-060 / FR-IGD-086)
## Penyelesaian Kunjungan IGD Otomatis saat Disposisi Dilaksanakan & Jalur Manual

| Metadata | Keterangan |
| :--- | :--- |
| **Tanggal Pengujian** | 30 September 2026 |
| **Target Layar** | Pengkajian IGD (`/health-services/emergency-installation-management/emergency-assessment`), Tab Tindak Lanjut |
| **Dokumen Acuan** | `BE-IGD-060` (Bagian 5.1 Skenario Uji untuk Pemilik), `FR-IGD-086`, `FR-IGD-087`, `FR-IGD-089`, `IGD-DEC-163`, `164`, `165`, `166`, `167`, `170` |
| **Metode Pengujian** | **Live Browser Testing** (Playwright Automated Testing via Chromium/Edge Engine) + Verifikasi Database PostgreSQL Dev Langsung |
| **Frontend Runtime** | `http://localhost:3000` (Next.js 16 Turbopack) |
| **Backend API** | `https://localhost:7184` (.NET Core 9 Web API) |
| **Database Target** | `160.22.250.77:5432` / `QuilvianNewDevRizki` (PostgreSQL) |
| **Akun Pelaksana** | `sysadmin_test` / `Test@12345` (SuperAdmin, Actor ID: `0ba84a1a-2559-49ba-a320-10fb1f399d70`) |
| **Lokasi Artefak Uji** | `QuilvianSystemFrontendDev/test-with-agy/igd/` |
| **Hasil Akhir** | **6 / 6 PASS (100% LULUS)** |
| **Status BE-IGD-060** | **LULUS PENUH (VERIFIED & CLOSED ✅)** |

---

## 1. Ringkasan Eksekutif

Pengujian langsung via peramban (*live browser testing*) dan verifikasi data relasional telah berhasil dilaksanakan untuk memverifikasi secara menyeluruh seluruh skenario uji **S1–S6** yang didefinisikan pada dokumen **BE-IGD-060** bagian 5.1.

Fokus inti dari task `BE-IGD-060` adalah otomatisasi penutupan kunjungan IGD ketika keputusan tindak lanjut (disposisi) pasien telah **dilaksanakan** (`Executed`), guna mencegah celah rekam medis di mana encounter pasien tertinggal berstatus terbuka (*open encounter*) dan catatan klinisnya masih dapat diubah tanpa batas waktu.

Pengujian ini membuktikan secara empiris perbedaan arsitektur antara penutupan kunjungan via disposisi (otomatis) dan penutupan manual:
1. **Penutupan Otomatis (Skenario S1):** Pada pasien tanpa observasi aktif dan tanpa kepergian menggantung (Subjek: **Dede Kurniawan**), pelaksanaan disposisi langsung memindahkan status kunjungan ke `Completed` (`9`), menutup encounter menjadi `Completed` (`9`), dan kolom jejak asal **`EmgVisit.ClosedByDispositionId` terisi secara presisi** dengan ID disposisi yang mengeksekusinya (`f169d5d1-e0d3-4ee0-8d31-0ec7690b3c35`).
2. **Penahanan Penutupan Berdasarkan Aturan Klinis (Skenario S2):** Pada pasien yang masih memiliki observasi aktif (Subjek: **Agnes Yuliani**), pelaksanaan disposisi tetap tersimpan tanpa galat (`Executed` / `3`), namun status kunjungan **tetap tertahan pada `Disposed` (`7`)** dan kolom `ClosedByDispositionId` bernilai **NULL**, karena `ValidateVisitClosureAsync` mendeteksi penahan *"Masih ada observasi yang belum diselesaikan."*
3. **Penutupan Manual (Skenario S3):** Lanjutan dari S2, setelah periode observasi aktif dituntaskan/ditutup, kunjungan diselesaikan lewat jalur manual (`PATCH /emergency-visits/{id}/complete`). Kunjungan berhasil `Completed` (`9`) dan encounter `Completed` (`9`), dengan **`ClosedByDispositionId` tetap KOSONG (NULL)**, membuktikan bahwa jalur manual tidak mencatatkan asal penutupan dari disposisi tertentu.
4. **Idempotensi & Pencegahan Regresi (Skenario S4 & S5):** Pengulangan klik *Laksanakan* pada disposisi S1 berjalan aman dengan HTTP 200 tanpa mengubah waktu penyelesaian awal (idempoten). Sementara pembuatan disposisi baru pada kunjungan yang sudah selesai ditolak tegas dengan HTTP 409 Conflict (*"Status kunjungan tidak dapat berubah dari Completed ke Disposed."*), menegakkan integritas aturan `IGD-DEC-166` bahwa kunjungan selesai tidak pernah dibuka kembali.
5. **Integritas Startup & Dependency Injection (Skenario S6):** Seluruh siklus layanan berjalan di backend tanpa galat siklus DI, registrasi service valid, dan health check endpoint merespons HTTP 200.

Seluruh 6 skenario dinyatakan **LULUS (PASS)**.

---

## 2. Matriks Hasil Pengujian S1–S6

| Kode | Kriteria | Skenario Pengujian | Hasil Pengujian | Status | Bukti Artefak & Log |
| :---: | :---: | :--- | :--- | :---: | :--- |
| **S1** | 1 | **Pasien Tanpa Observasi Aktif: Kunjungan Langsung Selesai & Encounter Tertutup**<br>Pasien tanpa observasi aktif, tanpa kepergian menggantung, lalu tindak lanjutnya dilaksanakan lewat tab Tindak Lanjut. | HTTP 200. Kunjungan langsung `Completed` (`9`), `VisitCompletedAt` terisi. `ClosedByDispositionId` terisi ID disposisi. Encounter berstatus `Completed` (`9`), `CompletedAt` terisi sinkron. | **PASS** | `s1_01_assessment_patient_list.png`<br>`s1_02_workspace_loaded.png`<br>`s1_04_form_filled.png`<br>`s1_05_draft_saved.png`<br>`s1_06_confirmed.png`<br>`s1_07_executed_completed.png` |
| **S2** | 2 | **Pasien dengan Observasi Aktif: Kunjungan Tertahan "Tindak Lanjut Ditetapkan"**<br>Pasien yang masih memiliki observasi aktif, lalu tindak lanjutnya dilaksanakan. | HTTP 200. Disposisi tersimpan `Executed` (`3`), namun kunjungan tetap tertahan `Disposed` (`7`), `VisitCompletedAt` = NULL, `ClosedByDispositionId` = NULL. Log mencatat penahan observasi aktif. | **PASS** | `s2_01_agnes_tindak_lanjut_initial.png`<br>`s2_02_agnes_form_filled.png`<br>`s2_03_executed_stays_disposed.png` |
| **S3** | 3 | **Lanjutan S2: Tutup Observasi & Selesaikan Kunjungan Manual**<br>Tutup observasi aktif pasien S2, lalu tekan selesaikan kunjungan lewat endpoint `PATCH /emergency-visits/{id}/complete`. | HTTP 200 `Kunjungan IGD berhasil diselesaikan.` Kunjungan menjadi `Completed` (`9`), `VisitCompletedAt` terisi, encounter `Completed` (`9`), dan `ClosedByDispositionId` **KOSONG (NULL)**. | **PASS** | `s3_01_manual_completed_ui.png`<br>Kueri PostgreSQL S3 |
| **S4** | 4 | **Pengulangan Eksekusi Disposisi (Idempoten)**<br>Tekan *Laksanakan* lagi pada disposisi S1 yang sudah executed dan kunjungannya sudah selesai. | HTTP 200 `Status tindak lanjut IGD berhasil diubah.` Nol galat, nol perubahan status kunjungan, `VisitCompletedAt` tidak bergeser (sama persis dengan waktu eksekusi pertama). | **PASS** | `s4_repeat_execution.png`<br>HTTP 200 Response Data |
| **S5** | 4 | **Disposisi Baru pada Kunjungan Selesai: Penolakan 409 Conflict**<br>Buat draf tindak lanjut baru pada kunjungan S1 yang sudah selesai, konfirmasi, lalu laksanakan. | Ditolak `HTTP 409 Conflict` dengan pesan *"Status kunjungan tidak dapat berubah dari Completed ke Disposed."* Kunjungan tetap `Completed` (`9`) dan tidak dibuka kembali. | **PASS** | `s5_reject_409_new_disposition.png`<br>HTTP 409 Payload |
| **S6** | 5 | **Integritas Arsitektur & DI Startup**<br>Aplikasi berjalan penuh tanpa galat DI, nol siklus ketergantungan antar-layanan IGD, dan endpoint health check aktif. | Backend running pada PID 19828 (Port 7184), Frontend running pada PID 6444 (Port 3000), `GET /health` merespons HTTP 200 OK. Nol siklus dependency injection. | **PASS** | Health Check Status HTTP 200<br>`s1_s6_test_results.json` |

---

## 3. Verifikasi Database Relasional (Kueri Baca-Saja)

Sesuai arahan pada `BE-IGD-060` Bagian 5.1, dilakukan verifikasi langsung pada tabel basis data `public."EmgVisit"` dan `public."RegPatientEncounter"` dengan kueri baca-saja.

### 3.1 Kueri S1: Dede Kurniawan (Penutupan Otomatis via Disposisi)

```sql
SELECT "VisitStatus", "VisitCompletedAt", "ClosedByDispositionId", "UpdateBy"
FROM public."EmgVisit" 
WHERE "Id" = '0703289d-5734-4201-9347-e04233b6f4a4';

SELECT "EncounterStatus", "CompletedAt"
FROM public."RegPatientEncounter" 
WHERE "Id" = '09e83ece-917d-4612-9f44-b9ba67b0edc7';
```

**Hasil Keluaran PostgreSQL:**
```json
{
  "visit": {
    "Id": "0703289d-5734-4201-9347-e04233b6f4a4",
    "VisitStatus": 9,
    "VisitCompletedAt": "2026-09-30 08:11:34.454177+00:00",
    "ClosedByDispositionId": "f169d5d1-e0d3-4ee0-8d31-0ec7690b3c35",
    "UpdateBy": "0ba84a1a-2559-49ba-a320-10fb1f399d70"
  },
  "encounter": {
    "Id": "09e83ece-917d-4612-9f44-b9ba67b0edc7",
    "EncounterStatus": 9,
    "CompletedAt": "2026-09-30 08:11:34.454177+00:00"
  },
  "disposition": {
    "Id": "f169d5d1-e0d3-4ee0-8d31-0ec7690b3c35",
    "DispositionStatus": 3,
    "ExecutedAt": "2026-09-30 08:11:34.454177+00:00"
  }
}
```

> **Verifikasi Kritis S1:**
> - `VisitStatus` = **9 (Completed)**
> - `EncounterStatus` = **9 (Completed)**
> - `ClosedByDispositionId` = **`f169d5d1-e0d3-4ee0-8d31-0ec7690b3c35`** (TERISI ID DISPOSISI PEMICU ✅)

---

### 3.2 Kueri S3: Agnes Yuliani (Penutupan Manual Pasca-Observasi Tuntas)

```sql
SELECT "VisitStatus", "VisitCompletedAt", "ClosedByDispositionId", "UpdateBy"
FROM public."EmgVisit" 
WHERE "Id" = '291ed4fa-1ddc-420a-9968-2bdae0a2d0aa';

SELECT "EncounterStatus", "CompletedAt"
FROM public."RegPatientEncounter" 
WHERE "Id" = '6dd87661-2cd0-4c27-bb63-aac763c7edb3';
```

**Hasil Keluaran PostgreSQL:**
```json
{
  "visit": {
    "Id": "291ed4fa-1ddc-420a-9968-2bdae0a2d0aa",
    "VisitStatus": 9,
    "VisitCompletedAt": "2026-09-30 08:15:00.034762+00:00",
    "ClosedByDispositionId": null,
    "UpdateBy": "0ba84a1a-2559-49ba-a320-10fb1f399d70"
  },
  "encounter": {
    "Id": "6dd87661-2cd0-4c27-bb63-aac763c7edb3",
    "EncounterStatus": 9,
    "CompletedAt": "2026-09-30 08:15:00.034762+00:00"
  },
  "observation": {
    "Id": "8e26b5bb-3752-48e3-9e13-7f1877a2b589",
    "ObservationStatus": 4,
    "EndedAt": "2026-09-30 08:14:59.947056+00:00"
  }
}
```

> **Verifikasi Kritis S3:**
> - `VisitStatus` = **9 (Completed)**
> - `EncounterStatus` = **9 (Completed)**
> - `ClosedByDispositionId` = **`null`** (KOSONG / NULL SESUAI HARAPAN ✅)

---

### 3.3 Tabel Perbandingan Kolom `ClosedByDispositionId` (S1 vs S3)

| Parameter Evaluasi | Skenario S1 (Penutupan Otomatis) | Skenario S3 (Penutupan Manual) | Kesesuaian Desain |
| :--- | :--- | :--- | :---: |
| **Pasien** | Dede Kurniawan (`05989E`) | Agnes Yuliani (`DB364C`) | Sesuai |
| **Pemicu Penutupan** | `PATCH /emergency-dispositions/{id}/disposition-status` (3) | `PATCH /emergency-visits/{id}/complete` | Sesuai |
| **Status Kunjungan Akhir** | `9` (Completed) | `9` (Completed) | Sesuai |
| **Status Encounter Akhir** | `9` (Completed) | `9` (Completed) | Sesuai |
| **Nilai `ClosedByDispositionId`** | **`f169d5d1-e0d3-4ee0-8d31-0ec7690b3c35`** | **`null` (Kosong)** | **IDENTIK DENGAN SPESIFIKASI ✅** |
| **Makna Audit Domain** | Kunjungan diselesaikan oleh tindakan klinis disposisi | Kunjungan diselesaikan oleh aksi administratif petugas manual | **100% VALID** |

---

## 4. Pembahasan Detail Skenario S4, S5, dan S6

### Skenario S4: Pengulangan Eksekusi Disposisi (Idempoten)
- **Operasi:** Mengirim ulang permintaan `PATCH /emergency-dispositions/f169d5d1-e0d3-4ee0-8d31-0ec7690b3c35/disposition-status` dengan payload `{ "dispositionStatus": 3, "notes": "Pengulangan eksekusi aman (idempoten)" }`.
- **Hasil:** Respons HTTP 200 OK dengan pesan *"Status tindak lanjut IGD berhasil diubah."*
- **Verifikasi Kunjungan:** `VisitStatus` tetap 9, dan nilai `VisitCompletedAt` tetap `2026-09-30 08:11:34.454177+00:00` tanpa bergeser 1 milidetik pun.
- **Kesimpulan:** Penanganan pengecualian `Executed → Executed` pada kunjungan yang sudah selesai (Keputusan Desain §3.4 nomor 1) terbukti efektif melindungi pengguna dari galat 409 saat terjadi klik ganda atau percobaan ulang jaringan.

### Skenario S5: Penolakan Disposisi Baru pada Kunjungan Selesai
- **Operasi:** Pada kunjungan S1 yang sudah berstatus `Completed`, dibuat draf disposisi baru (`POST /emergency-dispositions`), dikonfirmasi (`PATCH status: 2`), lalu dicoba dieksekusi (`PATCH status: 3`).
- **Hasil:** Backend menolak tegas pada langkah eksekusi dengan `HTTP 409 Conflict`.
- **Pesan Galat:**
  ```json
  {
    "success": false,
    "statusCode": 409,
    "message": "Status kunjungan tidak dapat berubah dari Completed ke Disposed."
  }
  ```
- **Verifikasi Kunjungan:** `VisitStatus` di database tetap 9. Kunjungan tidak dibuka kembali (`IGD-DEC-166`).

### Skenario S6: Integritas Layanan & Startup
- Backend .NET Core 9 dan Frontend Next.js 16 aktif melayani lalu lintas tanpa crash.
- Layanan `EmergencyVisitService`, `EmergencyDispositionService`, `EmergencyDepartureService`, dan `EmergencyUnitAuthorityService` diinjeksi secara mulus oleh runtime Web API tanpa ketergantungan sirkular (nol siklus DI).
- Health check `GET https://localhost:7184/health` merespons HTTP 200 OK.

---

## 5. Ringkasan Artefak Bukti Pengujian

Artefak tangkapan layar tersimpan pada folder `QuilvianSystemFrontendDev/test-with-agy/igd/`:

| Nama Berkas | Ukuran | Deskripsi Visual |
| :--- | :---: | :--- |
| `s1_01_assessment_patient_list.png` | 484 KB | Daftar pasien Pengkajian IGD menampilkan Dede Kurniawan |
| `s1_02_workspace_loaded.png` | 433 KB | Workspace pengkajian IGD pasien Dede Kurniawan |
| `s1_03_tab_tindak_lanjut.png` | 438 KB | Tampilan awal tab Tindak Lanjut |
| `s1_04_form_filled.png` | 444 KB | Formulir penetapan tindak lanjut terisi lengkap (PLG) |
| `s1_05_draft_saved.png` | 424 KB | Status draf tindak lanjut tersimpan di riwayat |
| `s1_06_confirmed.png` | 425 KB | Tindak lanjut berhasil dikonfirmasi (siap dijalankan) |
| `s1_07_executed_completed.png` | 430 KB | Tindak lanjut dijalankan; kunjungan langsung Selesai |
| `s2_01_agnes_tindak_lanjut_initial.png` | 438 KB | Tab Tindak Lanjut pasien Agnes Yuliani dengan observasi aktif |
| `s2_02_agnes_form_filled.png` | 444 KB | Pengisian formulir tindak lanjut pasien Agnes Yuliani |
| `s2_03_executed_stays_disposed.png` | 430 KB | Disposisi dilaksanakan namun kunjungan tetap tertahan `Disposed` |
| `s3_01_manual_completed_ui.png` | 433 KB | Tampilan antarmuka pasien pasca-penutupan manual kunjungan |
| `s4_repeat_execution.png` | 430 KB | Bukti pelaksanaan ulang disposisi tanpa galat |
| `s5_reject_409_new_disposition.png` | 430 KB | Bukti penolakan HTTP 409 pada disposisi baru kunjungan selesai |
| `s1_s6_test_results.json` | 3 KB | Berkas ringkasan JSON eksekusi pengujian otomatis |

---

## 6. Kesimpulan & Rekomendasi

1. **Pemenuhan Kriteria Penerimaan:** Seluruh 5 kriteria penerimaan teknis pada kartu `BE-IGD-060` telah terpenuhi dan terbukti secara live melalui 6 skenario uji (S1–S6).
2. **Penutupan Task:** Status implementasi dan pengujian task **`BE-IGD-060` resmi dinyatakan LULUS PENUH (VERIFIED & CLOSED ✅)**.
3. **Kesiapan Task Lanjutan:** Lingkungan pengujian dan basis data siap untuk melanjutkan ke task berikutnya dalam rangkaian R3.14:
   - `BE-IGD-061`: Penutupan susulan kunjungan otomatis dari penuntasan observasi, penyelesaian kepergian fisik, dan pemenuhan sikap pesanan penunjang medis.
