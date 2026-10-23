# Laporan Pengujian Putaran L1R-SEBELUM: Perekaman Bukti Regresi Penjaga Kunjungan Tertutup (BE-IGD-019 / AT-IGD-088)

**Tanggal Pengujian:** 22 Oktober 2026  
**Modul:** Instalasi Gawat Darurat (IGD) Quilvian  
**Tujuan Pengujian:** Merekam bukti dasar regresi pada *build* backend saat ini (DLL stempel: `2026-10-22 10:55:46.745`) di mana penyesuaian triase sebelumnya melepaskan filter kunjungan tertutup di `EmergencyTriageService` dan menempatkan penjaga `KunjunganSudahDitutup` di dalam blok `if (TriageStatus == Completed)` pada `EmergencyTriageController`.  
**Aktor Penguji:** Dimas Kurniawan (`dimas.kurniawan@rsmmc.local` - Perawat Pelaksana IGD)  
**Lingkungan:** Staging / Localhost (Frontend Next.js Turbopack dev port 3000, Backend Kestrel ASP.NET Core port 7184)  

---

## 1. Ringkasan Eksekutif Hasil Pengujian

| ID Skenario | Nama Skenario & Fokus Pengujian | Target Kunjungan | Ekspektasi Bisnis | Hasil Nyata (Outcome) | Status | Bukti Terlampir |
| :--- | :--- | :--- | :--- | :--- | :---: | :--- |
| **L1R-01** | Ulangi L1-01: Penolakan Simpan Triase Selesai (*Completed*) pada Kunjungan Tertutup | Pasien P-L1-B (`c74ac3e9...`, Status: `Cancelled` / 8) | Ditolak dengan HTTP 409 Conflict dan pesan verbatim: *"Kunjungan IGD sudah ditutup, penilaian tidak dapat diselesaikan."* | HTTP 409 Conflict diterima, pesan verbatim tampil jelas di banner merah layar perawat | **PASS** | `L1R-01-akhir.png`<br>`jaringan-L1R-01.json` |
| **L1R-02** | **[BARU]** Regresi BE-IGD-019 / AT-IGD-088: Simpan Triase Draf (*Draft*) pada Kunjungan Tertutup via API | Pasien P-L1-B (`c74ac3e9...`, Status: `Cancelled` / 8) | Wajib **DITOLAK** karena kunjungan sudah ditutup tidak boleh menerima triase apa pun | **TERSIMPAN DI DATABASE** (HTTP 200 OK, Sequence: 2, ID: `9116d253...`) | **FAIL** *(Regresi Terbukti)* | `jaringan-L1R-02.json` |
| **L1R-03** | **[BARU]** Regresi Jalur Ubah Status: Pembaruan Status Triase Non-Completed pada Kunjungan Tertutup | Pasien P-L1-B (`c74ac3e9...`, Status: `Cancelled` / 8) | Wajib **DITOLAK** karena status triase tidak boleh diubah pada kunjungan yang sudah ditutup | **STATUS BERUBAH DI DATABASE** (HTTP 200 OK, status diubah dari `Draft` ke `InProgress`) | **FAIL** *(Regresi Terbukti)* | `jaringan-L1R-03.json` |
| **L1R-04** | Regresi Jalur Normal: Simpan Triase Selesai pada Kunjungan Berjalan Aktif | Pasien P-L1R-AKTIF (`f308d02f...`, Status Awal: `WaitingForTriage` / 2) | Berhasil disimpan (HTTP 200/201), status kunjungan bertransisi ke `Triaged` (3) | Berhasil disimpan (HTTP 200 OK, Sequence: 1, ID: `5655db06...`), kunjungan bertransisi ke Triaged | **PASS** | `L1R-04-akhir.png`<br>`jaringan-L1R-04.json` |

> **Kesimpulan Tahap 1:**  
> Hasil pengujian persis sesuai perkiraan arsitektur sistem: **L1R-01 PASS, L1R-02 FAIL, L1R-03 FAIL, dan L1R-04 PASS**.  
> Status `FAIL` pada L1R-02 dan L1R-03 adalah **temuan regresi yang valid dan berhasil direkam secara objektif**, membuktikan bahwa penjaga kunjungan tertutup saat ini bocor untuk status triase selain `Completed`.

---

## 2. Analisis Teknis Cacat Regresi (Akar Masalah)

### 2.1. Jalur Pembuatan Triase Baru (`POST /api/v1/.../emergency-triages`)
- **Perilaku Sebelum Regresi (BE-IGD-019):**  
  Di `EmergencyTriageService.ValidateRequestAsync`, terdapat validasi:
  ```csharp
  x => x.Id == request.EmergencyVisitId &&
       !x.IsDelete &&
       x.VisitStatus != EmergencyVisitStatus.Disposed &&
       x.VisitStatus != EmergencyVisitStatus.Completed &&
       x.VisitStatus != EmergencyVisitStatus.Cancelled
  ```
  Pemeriksaan ini menjaga agar tidak ada triase dalam bentuk apa pun yang dapat dibuat jika kunjungan sudah ditutup.
- **Pelepasan Penjaga Kemarin:**  
  Untuk membuat skenario L1-01 mengembalikan HTTP 409 (bukan HTTP 400 dari validator), penyaringan status kunjungan di `ValidateRequestAsync` dilepas, dan penjaga dipindahkan ke `EmergencyTriageController.cs:322`:
  ```csharp
  if (entity.TriageStatus == EmergencyTriageStatus.Completed)
  {
      visit = await _dbContext.Set<EmgVisit>().FirstOrDefaultAsync(...);
      if (KunjunganSudahDitutup(visit.VisitStatus))
          return Conflict(...);
  }
  ```
- **Bukti Kebocoran (L1R-02):**  
  Ketika klien mengirimkan permintaan pembuatan triase dengan `TriageStatus = Draft` (1) atau `InProgress` (2), blok `if (entity.TriageStatus == EmergencyTriageStatus.Completed)` **dilewati sepenuhnya**. Tidak ada validasi yang menolak permintaan tersebut, sehingga data triase baru berhasil masuk dan tersimpan permanen di basis data PostgreSQL (`EmgTriage`) pada kunjungan berstatus `Cancelled` (8).

### 2.2. Jalur Pembaruan Status Triase (`PATCH /api/v1/.../emergency-triages/{id}/triage-status`)
- **Penyebab Kebocoran di Jalur Status (L1R-03):**  
  Di `EmergencyTriageController.cs:553`:
  ```csharp
  if (request.TriageStatus == EmergencyTriageStatus.Completed)
  {
      visit = await _dbContext.Set<EmgVisit>().FirstOrDefaultAsync(...);
      if (KunjunganSudahDitutup(visit.VisitStatus))
          return Conflict(...);
  }
  ```
  Ketika perawat atau sistem mencoba mengubah status triase dari `Draft` ke `InProgress` (atau `Cancelled`), blok pemeriksaan `KunjunganSudahDitutup` diabaikan karena `request.TriageStatus != Completed`. Akibatnya status triase berhasil diperbarui menjadi `InProgress` di basis data, melanggar batas integritas data kunjungan tertutup.

---

## 3. Rincian Pelaksanaan Per Skenario

### 3.1. Skenario L1R-01: Ulangi L1-01 (Kunjungan Tertutup + Simpan Selesai)
- **Aksi:** Membuka antarmuka triase pasien `Pasien Uji Jenah Ali` (Kunjungan `c74ac3e9-6f96-4dce-94d8-f17248fa0352`, Status: `Cancelled` / 8), mengisi keluhan klinis dan indikator ATS merah, lalu menekan tombol *"Simpan Pemeriksaan"*.
- **Panggilan Jaringan:**
  - Metode & URL: `POST https://localhost:7184/api/v1/health-services/emergency-installation-management/emergency-triages`
  - Status HTTP: `409 Conflict`
  - Respons Backend Verbatim:
    ```json
    {
      "success": false,
      "statusCode": 409,
      "message": "Kunjungan IGD sudah ditutup, penilaian tidak dapat diselesaikan.",
      "data": null,
      "errors": null,
      "timestamp": "2026-10-22T11:57:55.2940044+07:00"
    }
    ```
- **Tampilan Antarmuka Pengguna:** Banner galat merah muncul seketika di bagian atas layar dengan pesan teks verbatim *"Kunjungan IGD sudah ditutup, penilaian tidak dapat diselesaikan."*. Tidak terjadi *fallback* ke pesan generik cadangan.
- **Tangkapan Layar:** [L1R-01-akhir.png](file:///C:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-l1r-sebelum-20261022/L1R-01-akhir.png)
- **Status Skenario:** **PASS**

---

### 3.2. Skenario L1R-02: Simpan Triase Baru Draf pada Kunjungan Tertutup via API
- **Aksi:** Mengirim permintaan API untuk membuat triase baru dengan `triageStatus = 1` (`Draft`) pada kunjungan tertutup `c74ac3e9-6f96-4dce-94d8-f17248fa0352`.
- **Payload Permintaan:**
  ```json
  {
    "emergencyVisitId": "c74ac3e9-6f96-4dce-94d8-f17248fa0352",
    "triageLevelId": "c47d1e7a-9a26-4923-ab23-02ebfac89e13",
    "triageSystem": 1,
    "triageStatus": 1,
    "isRetriage": false,
    "startedAt": "2026-10-22T05:04:01.529Z",
    "triageReason": "Uji regresi BE-IGD-019: simpan triase Draft pada kunjungan tertutup",
    "disabilitySummary": "GCS < 9",
    "notes": "Pengujian regresi L1R-02",
    "isActive": true
  }
  ```
- **Hasil Nyata Backend:**
  - Status HTTP: `200 OK`
  - Respons Backend Verbatim:
    ```json
    {
      "success": true,
      "statusCode": 200,
      "message": "Data triage IGD berhasil dibuat.",
      "data": {
        "id": "9116d253-f733-4b6d-a363-49bea3450900",
        "emergencyVisitId": "c74ac3e9-6f96-4dce-94d8-f17248fa0352",
        "sequence": 2,
        "triageStatus": 1,
        "triageReason": "Uji regresi BE-IGD-019: simpan triase Draft pada kunjungan tertutup"
      }
    }
    ```
- **Verifikasi Basis Data PostgreSQL:**
  Ditemukan baris baru di tabel `EmgTriage`:
  - `Id`: `9116d253-f733-4b6d-a363-49bea3450900`
  - `Sequence`: `2`
  - `TriageStatus`: `1` (`Draft`)
  - `EmergencyVisitId`: `c74ac3e9-6f96-4dce-94d8-f17248fa0352` (Kunjungan `Cancelled` / 8)
- **Status Skenario:** **FAIL** *(Bukti regresi terkonfirmasi)*

---

### 3.3. Skenario L1R-03: Pembaruan Status Triase Non-Completed pada Kunjungan Tertutup
- **Aksi:** Mengirim permintaan `PATCH /api/v1/.../emergency-triages/9116d253-f733-4b6d-a363-49bea3450900/triage-status` untuk mengubah status triase draf di atas menjadi `InProgress` (2) pada kunjungan tertutup.
- **Payload Permintaan:**
  ```json
  {
    "triageStatus": 2,
    "notes": "Uji regresi L1R-03: ubah status triase non-completed pada kunjungan tertutup"
  }
  ```
- **Hasil Nyata Backend:**
  - Status HTTP: `200 OK`
  - Respons Backend Verbatim:
    ```json
    {
      "success": true,
      "statusCode": 200,
      "message": "Status triage IGD berhasil diubah.",
      "data": {
        "id": "9116d253-f733-4b6d-a363-49bea3450900",
        "triageStatus": 2,
        "updateDateTime": "2026-10-22T05:04:21.0013587Z"
      }
    }
    ```
- **Verifikasi Basis Data PostgreSQL:**
  Nilai kolom `TriageStatus` pada baris `9116d253-f733-4b6d-a363-49bea3450900` terbukti terupdate menjadi `2` (`InProgress`) pada `2026-10-22 05:04:21.001 UTC.
- **Status Skenario:** **FAIL** *(Bukti regresi jalur ubah status terkonfirmasi)*

---

### 3.4. Skenario L1R-04: Simpan Triase Selesai pada Kunjungan Berjalan Normal
- **Aksi:** Membuka formulir triase untuk pasien `Pasien Pengganti 1791259153562_563` (Kunjungan `f308d02f-89b6-4a17-8221-48510cbb7f4d`, Encounter: `7dab5614-06ba-4d4b-9a6c-b73b6f8bb38b`, Status Awal: `WaitingForTriage` / 2), mengisi keluhan klinis dan indikator prioritas, lalu menekan tombol *"Simpan Pemeriksaan"*.
- **Panggilan Jaringan:**
  - Metode & URL: `POST https://localhost:7184/api/v1/health-services/emergency-installation-management/emergency-triages`
  - Status HTTP: `200 OK`
  - Respons Backend Verbatim:
    ```json
    {
      "success": true,
      "statusCode": 200,
      "message": "Data triage IGD berhasil dibuat.",
      "data": {
        "id": "5655db06-d11c-4763-8e7c-279a8c9d5029",
        "emergencyVisitId": "f308d02f-89b6-4a17-8221-48510cbb7f4d",
        "sequence": 1,
        "triageStatus": 3,
        "completedAt": "2026-10-22T05:04:42.587Z"
      }
    }
    ```
- **Verifikasi Basis Data PostgreSQL:**
  - Triase tersimpan dengan ID `5655db06-d11c-4763-8e7c-279a8c9d5029` (Status: `Completed` / 3).
  - Status kunjungan `f308d02f-89b6-4a17-8221-48510cbb7f4d` sukses bertransisi dari `2` (`WaitingForTriage`) menjadi `3` (`Triaged`).
- **Tangkapan Layar:** [L1R-04-akhir.png](file:///C:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-l1r-sebelum-20261022/L1R-04-akhir.png)
- **Status Skenario:** **PASS**

---

## 4. Spesifikasi Kontrak Endpoint Terkait (Swagger Style)

[Tags("EmergencyTriage")]

| Method | Endpoint Path | Ringkasan Fungsi & Perilaku Saat Ini | Autentikasi / Izin | Respons Status Saat Ini | Status Harapan Sesudah Perbaikan |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/health-services/emergency-installation-management/emergency-triages` | Membuat penilaian triase baru pada kunjungan IGD | Bearer / Cookie (`EmergencyTriage:Create`) | - Kunjungan Tutup + `Completed`: `409 Conflict`<br>- Kunjungan Tutup + `Draft`: `200 OK` **(BOCOR)**<br>- Kunjungan Aktif: `200 OK` | Kunjungan Tutup Wajib **409 Conflict** untuk SEMUA status triase |
| `PATCH` | `/api/v1/health-services/emergency-installation-management/emergency-triages/{id}/triage-status` | Memperbarui status triase yang sudah ada | Bearer / Cookie (`EmergencyTriage:Update`) | - Kunjungan Tutup + Target `Completed`: `409 Conflict`<br>- Kunjungan Tutup + Target `InProgress`: `200 OK` **(BOCOR)** | Kunjungan Tutup Wajib **409 Conflict** untuk SEMUA status triase |

---

## 5. Metadata Rekaman Pengujian

- **Folder Artefak:** `QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-l1r-sebelum-20261022/`
- **Berkas Kode Awal:** `kode-awal.txt` (HEAD Backend: `025bfc6d...`, HEAD Frontend: `52624e6c...`, DLL LastWriteTime: `2026-10-22 10:55:46.745`)
- **Berkas Kode Akhir:** `kode-akhir.txt` (Tidak ada perubahan source code pada Tahap 1)
- **Data Hasil Ringkas:** `hasil.json`
- **Catatan Rekaman Percobaan:** `percobaan.json`
- **Log Jaringan:** `jaringan-L1R-01.json`, `jaringan-L1R-02.json`, `jaringan-L1R-03.json`, `jaringan-L1R-04.json`
- **Gambar Bukti UI:** `L1R-01-akhir.png`, `L1R-04-akhir.png`
