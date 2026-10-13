# ISSUE-002 — Pasien Hilang dari Antrean Triage IGD Setelah Tangani Segera dan Kebutuhan Tombol Triage di Pengkajian

```yaml
issue_id: ISSUE-IGD-002
module_id: igd
submodule: igd
layar: "Triage Pasien IGD (FE-IGD-030/035/036) dan Ruang Kerja Pengkajian IGD (FE-IGD-031/032)"
sumber_laporan: "Laporan pelapor 13-10-2026: keluhan pasien hilang di triage, permintaan tab Sedang Ditangani, kolom Belum Ditriase, dan tombol pergi ke triage di pengkajian"
tanggal_issue: "2026-10-13"
status: DALAM_PERBAIKAN
keparahan_tertinggi: High
source_sha_backend: "6a1c6cf9 (rizkiG)"
source_sha_frontend: "701ddfb5d (RizkiV2)"
rencana_perbaikan: ../plan-repair/plan-repair-002-pasien-hilang-triage-tangani-segera.md
ditulis_dengan: "skill diagnose-module-issue"
```

## 1. Ringkasan

Laporan ini menyoroti alur kerja klinis penanganan darurat (*clinical fast-track / Tangani Segera*) pada antrean Triage Instalasi Gawat Darurat (IGD) serta integrasinya dengan layar Pengkajian Pasien IGD (Assesmen IGD). Ketika perawat loket/triage menekan tombol **Tangani Segera** pada pasien gawat darurat yang sedang menunggu triage, sistem memindahkan status kunjungan pasien menjadi `InTreatment` ("Sedang ditangani"). Namun, di antrean layar Triage Pasien, pasien tersebut tiba-tiba **hilang sepenuhnya** dari tampilan.

Hal ini terjadi karena bilah penyaring (*DataFilter tabs*) antrean Triage di frontend saat ini hanya menyediakan dua tab: tab **"Menunggu Triase"** (`queueStatus = WaitingForTriage`) dan tab **"Sudah Ditriase"** (`queueStatus = Triaged`). Tidak tersedia tab untuk pasien berstatus `InTreatment` ("Sedang ditangani"). Karena pasien yang ditangani segera berpindah ke status `InTreatment`, pasien tersebut otomatis tersaring keluar dari tab "Menunggu Triase" dan tidak muncul di tab "Sudah Ditriase".

Pemilik modul menetapkan arahan perbaikan:
1. Tambahkan 1 tab lagi pada antrean Triage yaitu tab **"Sedang Ditangani"**.
2. Tambahkan kolom baru **"STATUS TRIAGE"** pada tabel antrean Triage untuk menandai secara tegas pasien yang **"Belum Ditriase"** (badge kuning) vs **"Sudah Ditriase"** (badge hijau), sehingga perawat triage tidak bingung membedakan pasien yang butuh triage susulan.
3. Pada layar **Pengkajian IGD (Ruang Kerja Pemeriksaan)**, sediakan tombol **"Isi Triage Pasien"** jika pasien ini belum ditriage, dan tombol tersebut **wajib di-hide (dihilangkan)** secara otomatis jika pasien sudah selesai ditriage.

---

## 2. Laporan asli

| No. laporan | Keluhan pelapor (kata-kata pelapor) | Lampiran |
| :---: | --- | --- |
| 1 | "perbaki sekalian ini ketika tangani segera kok pasien nyaa hilang di triage" | Lampiran 1: Tangkapan layar tabel antrean Triage Pasien (`media_1791854344852_64c643f9.png`) |
| 2 | "berartti tambahkan tab 1 lagi sedang ditangani tapi tambahkan kolom belum di triage" | — |
| 3 | "biar nanti perawat pengisi triage ini tidak bingung dan ada button di pengkajian untuk pergi triage jia pasien in belum jika sudah di triage hilang atau hide button ini" | — |

---

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | :---: | --- | --- | --- | --- | --- | --- |
| `ISS-IGD-002-01` | 1, 2 | Antrean Triage hanya memiliki dua tab status sehingga pasien `InTreatment` (Tangani Segera) hilang dari antrean | `BUG` + `DESIGN_CHANGE` | Frontend | High | SUDAH-VERIFIKASI | `FIX-IGD-002-01` |
| `ISS-IGD-002-02` | 2 | Tabel antrean Triage tidak memiliki kolom penanda status triage (Belum Ditriase vs Sudah Ditriase) | `DESIGN_CHANGE` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-IGD-002-02` |
| `ISS-IGD-002-03` | 3 | Layar Pengkajian IGD tidak memiliki tombol navigasi ke form triage untuk pasien penanganan segera | `DESIGN_CHANGE` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-IGD-002-03` |
| `ISS-IGD-002-04` | 3 | Gerbang form triage menganggap `visitStatus >= 3` sudah selesai ditriage, menghalangi triage susulan | `BUG` | Frontend | High | SUDAH-VERIFIKASI | `FIX-IGD-002-04` |
| `ISS-IGD-002-05` | — | Backend `AksiBarisAntrean` mengembalikan aksi kosong bagi baris `InTreatment` pada antrean triage | `CONTRACT_MISMATCH` | Backend | Medium | SUDAH-VERIFIKASI | `FIX-IGD-002-05` |

Radius dampak umum:
1. `src/components/view/health-services/emergency-installation-management/emergency-management-triage-view/emergency-triage-patient-list-view.jsx`
2. `src/components/view/health-services/emergency-installation-management/emergency-management-triage-view/components/emergency-triage-patient-table.jsx`
3. `src/components/view/health-services/emergency-installation-management/emergency-management-triage-view/emergency-triage-form-view.jsx`
4. `src/components/view/health-services/emergency-installation-management/emergency-assessment-view/emergency-assessment-detail-view.jsx`
5. Backend `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyVisitService.cs` (`AksiBarisAntrean`)

---

## 4. Rincian per temuan

### ISS-IGD-002-01 — Antrean Triage hanya memiliki dua tab status sehingga pasien `InTreatment` hilang dari antrean

| Field | Nilai |
| --- | --- |
| **No. laporan** | 1, 2 |
| **Jenis** | `BUG` + `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di `emergency-triage-patient-list-view.jsx:331-359` |
| **Perbaikan** | `FIX-IGD-002-01` |

**Apa yang terjadi.**
Ketika perawat loket/triage menekan tombol **Tangani Segera** pada pasien darurat di antrean Triage, status pasien berubah menjadi "Sedang ditangani" (`InTreatment`). Namun ketika melihat kembali antrean Triage, pasien hilang dari tab "Menunggu Triase" dan tidak ada di tab "Sudah Ditriase".

**Kenapa terjadi.**
Bilah tab filter di `emergency-triage-patient-list-view.jsx` hanya memuat `WaitingForTriage` dan `Triaged`. Backend `EmergencyVisitService.cs:1394-1395` memfilter secara tepat `x.VisitStatus == statusKunjungan.Value`. Pasien `InTreatment` tidak cocok dengan kedua tab tersebut sehingga tidak pernah dikembalikan.

**Rekomendasi.**
Tambahkan tab **"Sedang Ditangani"** (`queueStatus = InTreatment`) pada antrean Triage Pasien.

---

### ISS-IGD-002-02 — Tabel antrean Triage tidak memiliki kolom penanda status triage

| Field | Nilai |
| --- | --- |
| **No. laporan** | 2 |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di `emergency-triage-patient-table.jsx:78-140` |
| **Perbaikan** | `FIX-IGD-002-02` |

**Apa yang terjadi.**
Pada tab "Sedang Ditangani", perawat melihat status kunjungan pasien bertuliskan "Sedang ditangani". Namun perawat tidak dapat membedakan mana pasien yang sudah ditriage sebelumnya lalu masuk ke tindakan, dan mana pasien yang masuk lewat jalur penanganan cepat sehingga belum ditriage sama sekali.

**Rekomendasi.**
Tambahkan kolom **"STATUS TRIAGE"** pada tabel antrean Triage yang menampilkan badge:
- **"Belum Ditriase"** (badge warning kuning) jika pasien belum memiliki rekam triage final.
- **"Sudah Ditriase"** (badge active hijau) jika pasien sudah memiliki penilaian triage final.

---

### ISS-IGD-002-03 — Layar Pengkajian IGD tidak memiliki tombol navigasi ke form triage

| Field | Nilai |
| --- | --- |
| **No. laporan** | 3 |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di `emergency-assessment-detail-view.jsx:390-437` |
| **Perbaikan** | `FIX-IGD-002-03` |

**Apa yang terjadi.**
Saat perawat dialihkan ke Ruang Kerja Pengkajian IGD sesudah menekan "Tangani Segera", perawat fokus memberikan tindakan darurat. Setelah kondisi pasien stabil, perawat ingin mengisi triage pasien tersebut, namun di layar pengkajian tidak ada tombol untuk langsung membuka form pemeriksaan triage. Perawat terpaksa harus kembali ke menu utama dan mencari pasien secara manual.

**Rekomendasi.**
Di header aksi `emergency-assessment-detail-view.jsx`, tambahkan tombol **"Isi Triage Pasien"** yang hanya tampil jika pasien **belum ditriage**. Jika pasien **sudah ditriage**, tombol ini **di-hide (dihilangkan)**.

---

### ISS-IGD-002-04 — Gerbang status form triage menganggap `visitStatus >= 3` sudah selesai ditriage

| Field | Nilai |
| --- | --- |
| **No. laporan** | 3 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di `emergency-triage-form-view.jsx:97-124` |
| **Perbaikan** | `FIX-IGD-002-04` |

**Apa yang terjadi.**
Bila perawat membuka rute pemeriksaan triage untuk pasien `InTreatment`, layar langsung menampilkan panel Riwayat Triage baca-saja karena kode memeriksa `visitStatus >= 3`. Padahal pasien `InTreatment` via Tangani Segera belum pernah ditriage, sehingga riwayatnya kosong dan perawat tidak dapat menginput skor triage susulan.

**Rekomendasi.**
Sesuaikan penentuan `isAlreadyTriaged` agar memeriksa ketersediaan rekam triage final aktual (`hasFinalTriage`), bukan hanya dari angka status kunjungan.

---

### ISS-IGD-002-05 — Backend `AksiBarisAntrean` mengembalikan aksi kosong bagi baris `InTreatment`

| Field | Nilai |
| --- | --- |
| **No. laporan** | — |
| **Jenis** | `CONTRACT_MISMATCH` |
| **Area** | Backend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di `EmergencyVisitService.cs:1532-1538` |
| **Perbaikan** | `FIX-IGD-002-05` |

**Apa yang terjadi.**
Di backend `EmergencyVisitService.cs:1532-1538`, baris dengan status `InTreatment` mengembalikan `AvailableActions = []` (kosong).

**Rekomendasi.**
Untuk baris `InTreatment` yang belum memiliki rekam triage final, kembalikan aksi `FillTriage` agar tombol "Isi Triage" tersedia di menu baris.

---

## 5. Tanya-jawab pelapor

> **T:** Berarti tambahkan tab 1 lagi sedang ditangani tapi tambahkan kolom belum di triage, biar nanti perawat pengisi triage ini tidak bingung dan ada button di pengkajian untuk pergi triage jika pasien ini belum jika sudah di triage hilang atau hide button ini?
>
> **J:** Tepat sekali! Tiga keputusan ini langsung menutup seluruh celah alur kerja klinis:
> 1. Tab **"Sedang Ditangani"** memastikan pasien yang ditangani segera tetap terpantau dengan jelas di antrean Triage.
> 2. Kolom **"STATUS TRIAGE"** dengan badge *"Belum Ditriase"* / *"Sudah Ditriase"* memberi kepastian visual bagi perawat bahwa pasien tersebut sedang ditangani dan masih butuh triage susulan.
> 3. Tombol **"Isi Triage Pasien"** di layar Pengkajian IGD memudahkan perawat langsung mengisi triage susulan dari ruang tindakan, dan tombol tersebut otomatis hilang (*hide*) setelah triage selesai diisi.

---

## 6. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-13 | Dokumen issue dibuat dari laporan keluhan hilangnya pasien setelah Tangani Segera | `diagnose-module-issue` |
| 2026-10-13 | Menambahkan keputusan pemilik: tab Sedang Ditangani, kolom Belum Ditriase, dan tombol pergi ke triage di Pengkajian yang di-hide saat sudah ditriage | `diagnose-module-issue` |
