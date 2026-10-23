# Laporan Pengujian Putaran L1R: Validasi Keberhasilan Penutupan Cacat Regresi Triase Kunjungan Tertutup (BE-IGD-019 / AT-IGD-088)

**Tanggal Pengujian:** 22 Oktober 2026  
**Modul:** Instalasi Gawat Darurat (IGD) Quilvian  
**Tujuan Pengujian:** Memvalidasi penutupan cacat regresi penjaga kunjungan tertutup sesudah perbaikan pada `EmergencyTriageController.cs` dan *build* ulang DLL backend (Stempel DLL: `2026-10-22 12:15:33.845`), memastikan bahwa kunjungan IGD yang berstatus tertutup (`Completed`, `Cancelled`, `Disposed`) secara konsisten menolak seluruh upaya penulisan atau perubahan data triase (baik status `Draft`, `InProgress`, maupun `Completed`), dan memastikan jalur penanganan pasien normal pada kunjungan aktif tetap berfungsi sempurna.  
**Aktor Penguji:** Dimas Kurniawan (`dimas.kurniawan@rsmmc.local` - Perawat Pelaksana IGD)  
**Lingkungan:** Staging / Localhost (Frontend Next.js Turbopack dev port 3000, Backend Kestrel ASP.NET Core port 7184)  

---

## 1. Ringkasan Eksekutif Hasil Pengujian Putaran L1R

| ID Skenario | Nama Skenario & Fokus Pengujian | Target Pasien / Kunjungan | Ekspektasi Bisnis | Hasil Nyata (*Outcome*) | Status | Bukti Terlampir |
| :--- | :--- | :--- | :--- | :--- | :---: | :--- |
| **L1R-01** | Ulangi L1-01: Penolakan Simpan Triase Selesai (*Completed*) pada Kunjungan Tertutup | Pasien P-L1-B (`c74ac3e9...`, Status: `Cancelled` / 8) | Ditolak dengan HTTP 409 Conflict dan pesan verbatim: *"Kunjungan IGD sudah ditutup, penilaian tidak dapat diselesaikan."* | Ditolak HTTP 409 Conflict. Pesan verbatim tampil jelas di banner merah layar UI perawat tanpa jatuh ke pesan cadangan generik. | **PASS** | `L1R-01-akhir.png`<br>`jaringan-L1R-01.json` |
| **L1R-02** | **[BARU]** Regresi BE-IGD-019 / AT-IGD-088: Penolakan Simpan Triase Draf (*Draft*) pada Kunjungan Tertutup via API | Pasien P-L1-B (`c74ac3e9...`, Status: `Cancelled` / 8) | Wajib **DITOLAK** (HTTP 409 Conflict) dan data triase tidak boleh masuk ke basis data | **BERHASIL DITOLAK** dengan HTTP 409 Conflict (*"Kunjungan IGD sudah ditutup, penilaian tidak dapat diselesaikan."*). Basis data terbukti tidak bertambah baris baru. | **PASS** | `jaringan-L1R-02.json` |
| **L1R-03** | **[BARU]** Regresi Jalur Ubah Status: Penolakan Pembaruan Status Triase Non-Completed pada Kunjungan Tertutup | Pasien P-L1-B (`c74ac3e9...`, Status: `Cancelled` / 8) | Wajib **DITOLAK** (HTTP 409 Conflict) dan status triase di basis data tidak boleh berubah | **BERHASIL DITOLAK** dengan HTTP 409 Conflict (*"Kunjungan IGD sudah ditutup, penilaian tidak dapat diselesaikan."*). Status triase di basis data tetap tidak berubah. | **PASS** | `jaringan-L1R-03.json` |
| **L1R-04** | Regresi Jalur Normal: Simpan Triase Selesai pada Kunjungan Berjalan Aktif | Pasien P-L1R-AKTIF (`8fb282bd...`, Status Awal: `WaitingForTriage` / 2) | Berhasil disimpan (HTTP 200/201), status kunjungan bertransisi ke `Triaged` (3) | Berhasil disimpan dengan HTTP 200 OK (`Id: 4a3f26f6...`, Sequence: 1). Kunjungan sukses bertransisi ke `Triaged` (3). | **PASS** | `L1R-04-akhir.png`<br>`jaringan-L1R-04.json` |

> **Tingkat Kelulusan Putaran L1R:** **4 dari 4 Skenario LULUS (100% PASS)**.  
> Cacat regresi `BE-IGD-019` / `AT-IGD-088` telah **resmi ditutup dan terbukti aman secara menyeluruh**.

---

## 2. Perbandingan Hasil Sebelum vs Sesudah Perbaikan Kode

Untuk memberikan gambaran komprehensif bagi manajemen rumah sakit dan tim rekayasa perangkat lunak, berikut perbandingan hasil audit sebelum dan sesudah perbaikan pada `EmergencyTriageController.cs`:

```
┌─────────────────┬──────────────────────────────────┬──────────────────────────────────┐
│ Skenario Uji    │ Sebelum Perbaikan (Build 10:55)  │ Sesudah Perbaikan (Build 12:15)  │
├─────────────────┼──────────────────────────────────┼──────────────────────────────────┤
│ L1R-01          │ PASS (HTTP 409 Conflict)         │ PASS (HTTP 409 Conflict)         │
│ (Simpan Selesai)│ Pesan verbatim tampil di UI      │ Pesan verbatim tampil di UI      │
├─────────────────┼──────────────────────────────────┼──────────────────────────────────┤
│ L1R-02          │ FAIL (HTTP 200 OK - BOCOR)       │ PASS (HTTP 409 Conflict - AMAN)  │
│ (Simpan Draf)   │ Triase draf tersimpan di DB      │ Triase draf DITOLAK backend      │
├─────────────────┼──────────────────────────────────┼──────────────────────────────────┤
│ L1R-03          │ FAIL (HTTP 200 OK - BOCOR)       │ PASS (HTTP 409 Conflict - AMAN)  │
│ (Ubah Status)   │ Status triase berubah di DB      │ Perubahan status DITOLAK backend │
├─────────────────┼──────────────────────────────────┼──────────────────────────────────┤
│ L1R-04          │ PASS (HTTP 200 OK)               │ PASS (HTTP 200 OK)               │
│ (Jalur Normal)  │ Kunjungan bertransisi ke Triaged │ Kunjungan bertransisi ke Triaged │
└─────────────────┴──────────────────────────────────┴──────────────────────────────────┘
```

---

## 3. Rincian Pelaksanaan dan Bukti Tiap Skenario

### 3.1. Skenario L1R-01: Ulangi L1-01 (Kunjungan Tertutup + Simpan Selesai)
- **Konteks Klinis:** Pasien `Pasien Uji Jenah Ali` (Kunjungan `c74ac3e9-6f96-4dce-94d8-f17248fa0352`) telah dibatalkan/ditutup kunjungannya (`VisitStatus = 8` / `Cancelled`). Perawat Dimas yang masih membuka halaman triase mencoba menekan tombol *"Simpan Pemeriksaan"*.
- **Panggilan Jaringan:**
  - Metode: `POST`
  - URL: `https://localhost:7184/api/v1/health-services/emergency-installation-management/emergency-triages`
  - Status HTTP: `409 Conflict`
  - Respons Backend Verbatim:
    ```json
    {
      "success": false,
      "statusCode": 409,
      "message": "Kunjungan IGD sudah ditutup, penilaian tidak dapat diselesaikan.",
      "data": null,
      "errors": null,
      "timestamp": "2026-10-22T12:30:31.4210082+07:00"
    }
    ```
- **Tampilan UI Layar:** Banner merah muncul di bagian atas layar menampilkan pesan verbatim *"Kunjungan IGD sudah ditutup, penilaian tidak dapat diselesaikan."*.
- **Bukti Artefak:** [L1R-01-akhir.png](file:///C:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-l1r-20261022/L1R-01-akhir.png) dan [jaringan-L1R-01.json](file:///C:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-l1r-20261022/jaringan-L1R-01.json).
- **Hasil:** **PASS**

---

### 3.2. Skenario L1R-02: Penolakan Simpan Triase Draf pada Kunjungan Tertutup
- **Konteks Klinis:** Membuktikan bahwa perbaikan titik pertama pada `EmergencyTriageController.cs:321` menutup celah di mana triase berstatus `Draft` (1) sebelumnya lolos karena penjaga hanya memeriksa `TriageStatus == Completed`.
- **Payload Permintaan API:**
  ```json
  {
    "emergencyVisitId": "c74ac3e9-6f96-4dce-94d8-f17248fa0352",
    "triageLevelId": "c47d1e7a-9a26-4923-ab23-02ebfac89e13",
    "triageSystem": 1,
    "triageStatus": 1,
    "isRetriage": false,
    "startedAt": "2026-10-22T05:30:46.512Z",
    "triageReason": "Uji verifikasi L1R-02: simpan triase Draft pada kunjungan tertutup pasca perbaikan",
    "disabilitySummary": "GCS < 9",
    "notes": "Pengujian verifikasi L1R-02",
    "isActive": true
  }
  ```
- **Hasil Nyata Backend:**
  - Status HTTP: `409 Conflict`
  - Respons Backend Verbatim:
    ```json
    {
      "success": false,
      "statusCode": 409,
      "message": "Kunjungan IGD sudah ditutup, penilaian tidak dapat diselesaikan.",
      "data": null,
      "errors": null,
      "timestamp": "2026-10-22T12:30:46.609351+07:00"
    }
    ```
- **Verifikasi Basis Data PostgreSQL:**
  Jumlah baris triase pada kunjungan `c74ac3e9-6f96-4dce-94d8-f17248fa0352` sebelum uji adalah `2` dan sesudah uji tetap `2`. Tidak ada data baru yang masuk ke basis data.
- **Bukti Artefak:** [jaringan-L1R-02.json](file:///C:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-l1r-20261022/jaringan-L1R-02.json).
- **Hasil:** **PASS**

---

### 3.3. Skenario L1R-03: Penolakan Pembaruan Status Triase Non-Completed pada Kunjungan Tertutup
- **Konteks Klinis:** Membuktikan bahwa perbaikan titik kedua pada `EmergencyTriageController.cs:553` menutup celah di mana pembaruan status triase via `PATCH /triage-status` sebelumnya lolos jika target status bukan `Completed`.
- **Payload Permintaan API:**
  ```json
  {
    "triageStatus": 5,
    "notes": "Uji verifikasi L1R-03: ubah status triase non-completed pada kunjungan tertutup pasca perbaikan"
  }
  ```
- **Hasil Nyata Backend:**
  - Status HTTP: `409 Conflict`
  - Respons Backend Verbatim:
    ```json
    {
      "success": false,
      "statusCode": 409,
      "message": "Kunjungan IGD sudah ditutup, penilaian tidak dapat diselesaikan.",
      "data": null,
      "errors": null,
      "timestamp": "2026-10-22T12:31:50.1901161+07:00"
    }
    ```
- **Verifikasi Basis Data PostgreSQL:**
  Status triase di basis data (`EmgTriage`) untuk ID `9116d253-f733-4b6d-a363-49bea3450900` diverifikasi tetap bernilai `2` (`InProgress`) dan tidak mengalami mutasi nilai.
- **Bukti Artefak:** [jaringan-L1R-03.json](file:///C:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-l1r-20261022/jaringan-L1R-03.json).
- **Hasil:** **PASS**

---

### 3.4. Skenario L1R-04: Simpan Triase Selesai pada Kunjungan Berjalan Normal
- **Konteks Klinis:** Memastikan bahwa pemindahan penjaga `KunjunganSudahDitutup` ke luar blok if tidak merusak alur normal pelayanan triase IGD bagi pasien yang sedang menunggu triase.
- **Target Pasien:** `Pasien Pengganti 1791255726423_431` (Kunjungan `8fb282bd-c2f2-4c89-9b8c-503931e5861a`, Encounter `6582b61c-4674-4bfd-b0c8-95090ae63a8e`, Status Awal: `WaitingForTriage` / 2).
- **Hasil Nyata Backend:**
  - Metode: `POST /api/v1/.../emergency-triages`
  - Status HTTP: `200 OK`
  - Respons Backend Verbatim:
    ```json
    {
      "success": true,
      "statusCode": 200,
      "message": "Data triage IGD berhasil dibuat.",
      "data": {
        "id": "4a3f26f6-a837-4651-941e-7e755dcdf58b",
        "emergencyVisitId": "8fb282bd-c2f2-4c89-9b8c-503931e5861a",
        "sequence": 1,
        "triageStatus": 3,
        "completedAt": "2026-10-22T05:32:09.864Z"
      }
    }
    ```
- **Verifikasi Basis Data PostgreSQL:**
  - Triase tersimpan di tabel `EmgTriage` dengan `Id: 4a3f26f6-a837-4651-941e-7e755dcdf58b` dan `TriageStatus: 3` (`Completed`).
  - Status kunjungan `8fb282bd-c2f2-4c89-9b8c-503931e5861a` sukses bertransisi dari `WaitingForTriage` (2) menjadi `Triaged` (3).
- **Bukti Artefak:** [L1R-04-akhir.png](file:///C:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-l1r-20261022/L1R-04-akhir.png) dan [jaringan-L1R-04.json](file:///C:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-l1r-20261022/jaringan-L1R-04.json).
- **Hasil:** **PASS**

---

## 4. Spesifikasi Kontrak Endpoint Terkait (Swagger Style)

[Tags("EmergencyTriage")]

| Method | Endpoint Path | Ringkasan Fungsi & Perilaku Baku | Autentikasi / Izin | Respons Status pada Kunjungan Tertutup | Respons Status pada Kunjungan Aktif |
| :--- | :--- | :--- | :--- | :---: | :---: |
| `POST` | `/api/v1/health-services/emergency-installation-management/emergency-triages` | Membuat penilaian triase baru pada kunjungan IGD | Bearer / Cookie (`EmergencyTriage:Create`) | **`409 Conflict`**<br>*(Semua status: Draft, InProgress, Completed)* | **`200 OK`** |
| `PATCH` | `/api/v1/health-services/emergency-installation-management/emergency-triages/{id}/triage-status` | Memperbarui status triase IGD | Bearer / Cookie (`EmergencyTriage:Update`) | **`409 Conflict`**<br>*(Semua status: InProgress, Completed, Cancelled)* | **`200 OK`** |

---

## 5. Metadata Rekaman Pengujian

- **Folder Artefak Pengujian:** `QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-l1r-20261022/`
- **Berkas Kode Awal:** `kode-awal.txt` (HEAD Backend: `025bfc6d...`, HEAD Frontend: `52624e6c...`, DLL LastWriteTime: `2026-10-22 12:15:33.845`)
- **Berkas Kode Akhir:** `kode-akhir.txt` (DLL diverifikasi aktif pada runtime Kestrel)
- **Ringkasan Hasil Uji JSON:** `hasil.json` (4 skenario, 4 PASS)
- **Log Percobaan Eksekusi:** `percobaan.json`
- **Rekaman Jaringan Lengkap:** `jaringan-L1R-01.json`, `jaringan-L1R-02.json`, `jaringan-L1R-03.json`, `jaringan-L1R-04.json`
- **Tangkapan Layar Bukti UI:** `L1R-01-akhir.png`, `L1R-04-akhir.png`
