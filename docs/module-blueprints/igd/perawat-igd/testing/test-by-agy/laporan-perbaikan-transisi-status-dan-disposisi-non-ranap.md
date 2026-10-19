# Laporan Resmi Perbaikan Transisi Status Alur Non-Ranap & Pemicu Status Sedang Ditangani (InTreatment) IGD

| Metadata Dokumen | Rincian / Nilai |
| :--- | :--- |
| **Tanggal Dokumentasi** | 16 Oktober 2026 |
| **Target Modul** | Modul Instalasi Gawat Darurat (IGD) — Ruang Kerja Pemeriksaan & Pengkajian |
| **Topik Perbaikan** | 1. Eliminasi Galat HTTP 409 Conflict pada Eksekusi Tindak Lanjut Non-Ranap (Pulang / Kontrol Poli / Rujuk Balik)<br>2. Pemicu Otomatis dan Manual Transisi Status Kunjungan: *Sudah Ditriage* (`Triaged`) $\rightarrow$ *Sedang Ditangani* (`InTreatment`)<br>3. Jaminan Keamanan Komponen Bersama (*Zero Disruption to Shared Base Components*) |
| **Referensi Keputusan** | `IGD-DEC-311` (Persetujuan Arsitektur Transisi Non-Ranap & Pemicu Status InTreatment) |
| **Task Terkait** | Backend: [`BE-IGD-078`](../../task/report/backend/BE-IGD-078.md) · Frontend: [`FE-IGD-065`](../../task/report/frontend/FE-IGD-065.md) |
| **Aplikasi Frontend** | `QuilvianSystemFrontendDev` (Next.js App Router, React 19, Redux Toolkit) |
| **Aplikasi Backend API** | `NewQuilvianSystemBackend` (ASP.NET Core 9 Web API, Entity Framework Core) |
| **Basis Data Aktif** | PostgreSQL `QuilvianNewDevRizki` (Host: `160.22.250.77`) |
| **Hasil Validasi** | **100% LULUS (PASS)** — Build Backend 0 error, QBE Strict PASS (0 violation), Frontend ESLint 0 error/0 warning, Unit Tests 55/55 PASS |

---

## 1. Ringkasan Eksekutif & Latar Belakang Masalah

Dalam operasional harian Instalasi Gawat Darurat (IGD), kelancaran pencatatan status pasien sangat krusial bagi keselamatan medis, kejelasan antrean, dan ketepatan penagihan biaya (*billing*). Selama audit kesiapan alur IGD, ditemukan dua hambatan alur kerja:

1. **Galat Penolakan Eksekusi Pulang (HTTP 409 Conflict):**
   Ketika dokter telah memutuskan pasien boleh pulang dan perawat menekan tombol **"Jalankan"** pada daftar tindak lanjut pasien pulang/berobat jalan, sistem memunculkan galat penolakan:
   > *"Status kunjungan tidak dapat berubah dari Triaged ke Disposed"* atau *"dari InTreatment ke Disposed"*.
   
   Hal ini terjadi karena aturan baku matriks status IGD mewajibkan kunjungan berstatus **Menunggu Keputusan** (`AwaitingDisposition`) terlebih dahulu sebelum dapat berpindah ke **Tindak Lanjut Ditetapkan** (`Disposed`). Pasien non-ranap yang tidak memerlukan observasi panjang tertahan di status awal sehingga sistem menolak eksekusi kepulangannya.

2. **Kunjungan Tertinggal di Status "Sudah Ditriage":**
   Setelah proses triase selesai, pasien berstatus *Sudah Ditriage* (`Triaged`). Ketika perawat memindahkan pasien ke ruang tindakan dan mulai melakukan pengkajian awal atau mencatat catatan perkembangan terintegrasi (SOAP), status kunjungan tidak otomatis berpindah menjadi *Sedang Ditangani* (`InTreatment`). Akibatnya, papan informasi pasien di IGD tetap menampilkan pasien seolah-olah belum ditangani.

3. **Mandat Keamanan Komponen Bersama (*Base Component*):**
   Perbaikan tampilan dan tombol antarmuka wajib mematuhi batasan konstitusi rekayasa perangkat lunak Quilvian: **Dilarang keras mengubah komponen dasar bersama (*base components*)** demi satu modul tertentu, karena komponen tersebut digunakan juga oleh modul Rawat Jalan/Poliklinik, Rawat Inap, dan Medical Check Up (MCU).

---

## 2. Alur Proses Bisnis Rumah Sakit (Urut & Runtut)

Diagram alur berikut mengilustrasikan perjalanan status kunjungan pasien IGD mulai dari kedatangan hingga selesai kepulangan:

```text
[Pasien Datang di IGD]
       │
       ▼ (Status: Arrived / Menunggu Triage)
[Pemeriksaan Triase oleh Perawat Triase]
       │
       ▼ (Status: Triaged / Sudah Ditriage - Nilai 3)
[Pasien Masuk Ruang Pemeriksaan / Tindakan IGD]
       │
       ├──► JALUR A (Pemicu Manual):
       │    Perawat/Dokter menekan tombol "Mulai Penanganan" di bilah atas
       │    ──► Status seketika berubah menjadi: InTreatment (Sedang Ditangani - Nilai 4)
       │
       └──► JALUR B (Pemicu Otomatis):
            Perawat menyimpan Assesmen Awal atau Catatan SOAP Medis
            ──► Sistem otomatis memicu status menjadi: InTreatment (Sedang Ditangani - Nilai 4)
                   │
                   ▼
[Pemeriksaan Medis, Diagnosis, & Tindakan Dokter/Perawat]
                   │
                   ▼
[Dokter Menetapkan Tindak Lanjut Pasien: PULANG / BEROBAT JALAN]
                   │
                   ▼ (Status Tindak Lanjut: Confirmed)
[Sistem Otomatis Mempromosikan Status Kunjungan]
                   │
                   ├──► Status Kunjungan berpindah ke: AwaitingDisposition (Menunggu Keputusan - Nilai 6)
                   │
                   ▼
[Perawat Memeriksa Edukasi, Resep Obat, & Menekan Tombol "Jalankan"]
                   │
                   ├──► Backend mengevaluasi status:
                   │    • Jika masih Triaged/InTreatment ──► Dipromosikan ke AwaitingDisposition (Safety Net)
                   │    • Transisi sah ke: Disposed (Tindak Lanjut Ditetapkan - Nilai 7)
                   │
                   ▼
[Evaluasi Penutupan Kunjungan Pasien Non-Ranap]
                   │
                   ├──► Memeriksa ketiadaan penahan (resep selesai, lab selesai, observasi tutup)
                   │
                   ▼
[Kunjungan IGD Ditutup Otomatis: Completed (Selesai - Nilai 9)]
Pasien menyelesaikan administrasi kasir/farmasi dan pulang ke rumah.
```

---

## 3. Contoh Konkret Skenario Operasional Rumah Sakit

### Skenario: Pasien Cedera Ringan Ditangani dan Dipulangkan
- **Data Pasien**: Tn. Hendra (32 tahun), datang ke IGD dengan luka lecet di lutut dan siku akibat terpeleset.
- **Tahap 1 (Triase)**: Perawat Triase melakukan pemeriksaan awal dan menetapkan Kategori ATS 4 (Urgensi Rendah). Status kunjungan menjadi **Sudah Ditriage** (`Triaged`).
- **Tahap 2 (Masuk Ruang Tindakan)**: Perawat Siti memposisikan Tn. Hendra di tempat tidur bilik 2. Perawat Siti menekan tombol **"Mulai Penanganan"** di bilah atas layar. Status Tn. Hendra langsung berubah menjadi **Sedang Ditangani** (`InTreatment`) dengan lencana biru primer di seluruh layar monitor IGD.
- **Tahap 3 (Pengkajian & Pembersihan Luka)**: Perawat Siti mengisi formulir Assesmen Awal dan membersihkan luka. Dokter Jaga memeriksa kondisi tulang dan memastikan tidak ada fraktur, lalu menuliskan catatan SOAP dan resep obat jalan (salep antibiotik dan pereda nyeri).
- **Tahap 4 (Keputusan Dokter)**: Dokter Jaga mengonfirmasi disposisi: *"Pulang / Berobat Jalan"*. Seketika backend mengubah status kunjungan Tn. Hendra menjadi **Menunggu Keputusan** (`AwaitingDisposition`).
- **Tahap 5 (Eksekusi Pemulangan)**: Perawat Siti memberikan edukasi perawatan luka di rumah, menyerahkan salinan resep, lalu menekan tombol **"Jalankan"**.
  - **Sebelum Perbaikan**: Sistem melempar pesan galat merah: *HTTP 409 Conflict: Status kunjungan tidak dapat berubah dari Triaged ke Disposed*. Perawat terblokir dan pasien tidak bisa dipulangkan di sistem.
  - **Sesudah Perbaikan**: Tombol *Jalankan* berhasil diproses seketika. Kunjungan Tn. Hendra bertransisi ke `Disposed`, lalu karena seluruh tindakan telah selesai, status otomatis berubah menjadi **Selesai** (`Completed`). Pasien dapat langsung diarahkan ke farmasi dan kasir.

---

## 4. Dokumentasi Spesifikasi API (Bergaya Swagger / OpenAPI)

Seluruh endpoint yang terlibat dalam alur penanganan dan disposisi non-ranap ini telah selaras dengan arsitektur standar Quilvian Web API:

### `[Tags("EmergencyVisit")]` — Pengelolaan Status Kunjungan IGD

| Atribut | Rincian Spesifikasi |
| :--- | :--- |
| **HTTP Method** | `PATCH` |
| **Endpoint Path** | `/v1/health-services/emergency-installation-management/emergency-visits/{id}/visit-status` |
| **Deskripsi Singkat** | Memperbarui status siklus hidup kunjungan IGD secara sah sesuai matriks transisi status. Digunakan oleh pemicu otomatis pasca-pengkajian dan tombol manual *Mulai Penanganan*. |
| **Otorisasi / Izin** | `Bearer Token` · Wewenang: `EmergencyVisit:Update` |
| **Path Parameter** | `id` (Guid, Wajib) — Nomor ID unik kunjungan IGD (`EmergencyVisitId`) |
| **Request Headers** | `Content-Type: application/json`<br>`Accept: application/json` |
| **Request Body** | ```json
{
  "visitStatus": 4,
  "notes": "Mulai penanganan di bilik tindakan IGD."
}
``` |
| **Response 200 OK** | ```json
{
  "statusCode": 200,
  "message": "Status kunjungan berhasil diperbarui.",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "emergencyVisitNumber": "EMG-20261016-0001",
    "visitStatus": 4,
    "visitStatusName": "Sedang ditangani",
    "updateDateTime": "2026-10-16T10:00:00Z"
  }
}
``` |
| **Response 400 / 409** | - `400 Bad Request`: Format data tidak sah atau catatan pembatalan kosong.<br>- `409 Conflict`: Transisi status ditolak oleh matriks `CanTransition`. |

---

### `[Tags("EmergencyDisposition")]` — Eksekusi Tindak Lanjut Pasien IGD

| Atribut | Rincian Spesifikasi |
| :--- | :--- |
| **HTTP Method** | `PUT` / `PATCH` |
| **Endpoint Path** | `/v1/health-services/emergency-installation-management/emergency-dispositions/{id}/disposition-status` |
| **Deskripsi Singkat** | Mengubah status tindak lanjut pasien IGD (misal: dari Dikonfirmasi Dokter menjadi Dijalankan oleh Perawat). Mengandung mekanisme *sequential promotion* otomatis untuk kunjungan non-ranap. |
| **Otorisasi / Izin** | `Bearer Token` · Wewenang: `EmergencyDisposition:Update` |
| **Path Parameter** | `id` (Guid, Wajib) — Nomor ID unik tindak lanjut IGD (`EmergencyDispositionId`) |
| **Request Body** | ```json
{
  "dispositionStatus": 3,
  "notes": "Pasien dijemput keluarga, edukasi obat jalan telah disampaikan."
}
``` *(Keterangan Status: 2 = Confirmed / Dikonfirmasi, 3 = Executed / Dijalankan, 4 = Cancelled / Dibatalkan)* |
| **Response 200 OK** | ```json
{
  "statusCode": 200,
  "message": "Status tindak lanjut berhasil diperbarui.",
  "data": {
    "id": "e4b2d561-9c60-4977-9df0-7d727db68581",
    "emergencyVisitId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "dispositionStatus": 3,
    "dispositionStatusName": "Executed",
    "executedAt": "2026-10-16T10:15:00Z"
  }
}
``` |
| **Response 409 Conflict** | - `409 Conflict`: Bila tindak lanjut rawat inap mencoba dijalankan langsung dari IGD tanpa melalui proses admisi bangsal. |

---

## 5. Ringkasan Bukti Verifikasi Rekayasa

| Tahapan Verifikasi | Parameter / Perintah | Hasil Uji | Keterangan |
| :--- | :--- | :--- | :--- |
| **Kompilasi Backend** | `dotnet build -p:RunAnalyzers=false` | **0 Error(s)** | Kompilasi C# pada branch `rizkiG` sukses tanpa error |
| **Kepatuhan Arsitektur (QBE)** | `Invoke-QbeConformanceCheck.ps1 -Mode Strict` | **PASS (0 VIOLATION)** | Kepatuhan mutlak terhadap aturan rekayasa Quilvian |
| **Kualitas Kode Frontend (ESLint)** | `npx eslint src/components/view/...` | **0 Error, 0 Warning** | Standar kode bersih, lolos optimasi React Compiler |
| **Pengujian Unit Frontend** | `node --test tests/unit/emergency-*.test.mjs` | **55 of 55 Tests PASSED** | Seluruh test suite IGD lulus 100% |
| **Integritas Base Component** | `git status` & `git diff` | **NOL Perubahan di Base** | Direktori `@/components/features/base-features/` dan `@/components/ui/` bersih dari modifikasi spesifik modul |

Dokumentasi ini resmi menjadi acuan operasional dan teknis bagi pemeliharaan modul Instalasi Gawat Darurat (IGD) Quilvian Hospital System.
