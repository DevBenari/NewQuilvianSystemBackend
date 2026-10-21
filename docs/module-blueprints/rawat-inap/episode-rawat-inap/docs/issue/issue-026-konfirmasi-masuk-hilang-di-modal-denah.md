# ISSUE-026 — Aksi Konfirmasi Masuk Hilang dari Modal Denah Monitoring Bed

```yaml
issue_id: ISSUE-EPS-026
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Bed Management — Monitoring Denah dan Pop-up Kamar (FE-BM-01 / FE-DNH-02)"
sumber_laporan: "Laporan pemilik sistem 17-10-2026: bed berstatus Dipesan pada pop-up denah tidak menyediakan cara untuk dikonfirmasi masuk"
tanggal_issue: "2026-10-17"
status: SELESAI
keparahan_tertinggi: Blocker
source_sha_backend: "c2bc563ed9332f08fa95b9fe35786fea7a647279 (MHamzah)"
source_sha_frontend: "15ae73984ad26a408e3038566d7e1d1533fe10c3 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-026-konfirmasi-masuk-hilang-di-modal-denah.md
```

---

## 1. Ringkasan

Pada route `/health-services/inpatient-management/bed-board`, tab Monitoring sekarang hanya merender denah dan pop-up kamar. Bed berstatus `Reserved`/`Dipesan` tetap menampilkan identitas yang diizinkan dan sisa waktu reservasi, tetapi tidak menyediakan tombol **Konfirmasi Masuk** maupun **Batalkan Pesanan**. Tidak ada jalan alternatif pada layar yang sama karena kartu, tabel, dan modal detail lama sudah berhenti dirender.

Backend dan orkestrasi frontend untuk kedua tindakan tersebut masih tersedia. Endpoint `POST /placements` dan `PATCH /reservations/{id}/cancel` masih aktif, sedangkan hook `useInpatientBedBoardActions` masih menangani permission, pembacaan ulang data terbaru, `ExpectedBedVersion`, dialog, kegagalan, idempotensi, dan toast. Regresi terjadi ketika `FE-RWI-247` mengganti monitoring lama dengan denah: integrasi baru hanya menyambungkan aksi pembersihan, riwayat, serta buka/tutup bed dan tidak menyambungkan kembali aksi reservasi/penempatan yang sudah ada.

Masalah ini adalah `BUG` dan `CONTRACT_MISMATCH`. Dampaknya `Blocker` untuk alur operasional karena episode berstatus konsep dengan reservasi aktif tidak dapat dikonfirmasi menjadi masuk/`Admitted` melalui UI Bed Management yang berlaku.

---

## 2. Laporan Asli

| No. laporan | Kalimat pelapor | Lampiran |
| ---: | --- | --- |
| 1 | "ini bagaimana cara ya agar bisa di konfirmasi masuk" | Tangkapan layar pop-up kamar pada denah Monitoring: satu bed berstatus `Dipesan` hanya menampilkan tombol `Riwayat`; identitas pasien pada lampiran tidak disalin ke dokumen ini. |

---

## 3. Ringkasan Temuan

| ID | No. laporan | Judul temuan | Jenis | Area | Keparahan | Status bukti | ID perbaikan |
| --- | ---: | --- | --- | --- | --- | --- | --- |
| `ISS-EPS-026-01` | 1 | Tombol **Konfirmasi Masuk** tidak dirender pada bed `Reserved` di pop-up denah | `BUG`, `CONTRACT_MISMATCH` | Frontend | `Blocker` | `SUDAH-VERIFIKASI` | `FIX-EPS-026-01` |
| `ISS-EPS-026-T1` | — | Tombol **Batalkan Pesanan** ikut hilang dari layar yang sama | `BUG`, `CONTRACT_MISMATCH` | Frontend | `High` | `SUDAH-VERIFIKASI` | `FIX-EPS-026-01` |
| `ISS-EPS-026-T2` | — | Kontrak hasil refresh hook aksi lama tidak langsung cocok dengan bentuk respons denah | `INTEGRATION_GAP` | Frontend | `High` | `SUDAH-VERIFIKASI` | `FIX-EPS-026-01` |
| `ISS-EPS-026-T3` | — | Test monitoring denah tidak menjaga keberadaan kedua aksi reservasi | `TEST_GAP` | Frontend | `Medium` | `SUDAH-VERIFIKASI` | `FIX-EPS-026-01` |

---

## 4. Rincian per Temuan

### `ISS-EPS-026-01` — Tombol Konfirmasi Masuk tidak dirender pada bed `Reserved`

| | |
| --- | --- |
| **Lokasi kode** | `floor-plan-room-popup.jsx:106-111,171-200,205-218`; `floor-plan-monitoring.jsx:102-114`; `bed-management-view.jsx:124-132` |
| **Jenis** | `BUG`, `CONTRACT_MISMATCH` |
| **Area** | Frontend |
| **Keparahan** | Blocker |
| **Status bukti** | SUDAH-VERIFIKASI pada source frontend SHA di metadata |
| **Perbaikan** | `FIX-EPS-026-01` |

**Apa yang terjadi.** `BedRow` pada `floor-plan-room-popup.jsx` hanya menghitung aksi pembersihan, tutup bed, dan buka bed. Cabang aksi mode Lihat hanya merender `Pembersihan`, `Riwayat`, serta `Tutup/Buka bed`. Komponen tersebut juga tidak menerima prop untuk konfirmasi masuk atau pembatalan reservasi. Parent `FloorPlanMonitoring` hanya meneruskan callback riwayat dan ketersediaan. Karena `BedManagementView` kini hanya merender `FloorPlanMonitoring` untuk tab Monitoring, pengguna tidak memiliki tombol lain untuk menyelesaikan penempatan.

**Kenapa terjadi.** Laporan implementasi `FE-RWI-247` menyatakan kartu/tabel lama dan modal detail lama tidak lagi dirender, lalu mencatat aksi pop-up baru hanya untuk pembersihan, riwayat, serta tutup/buka bed. Pada migrasi tersebut, consumer `useInpatientBedBoardActions` ikut hilang dari pohon render meskipun hook-nya tidak dihapus.

**Bukti bahwa kemampuan belum hilang dari sistem.** `use-inpatient-bed-board-actions.jsx:27-80` masih memuat ulang data sebelum dialog; baris `96-108` masih mengirim payload penempatan ke `placements`; baris `113-122` masih menangani sukses dan refresh. Backend `InpatientBedOccupancyController.cs:238-281` masih menyediakan `POST placements` dengan permission `InpatientBedOccupancy:Create`. Desain layar juga eksplisit menyatakan tombol `Konfirmasi Masuk` aktif untuk status `Reserved`, petugas berhak, dan metadata episode tersedia (`05-skema-tampilan.md:1031-1036`).

**Dampak.** Reservasi dapat dibuat, tetapi tidak dapat dituntaskan menjadi placement dari UI Monitoring yang berlaku. Episode tidak dapat beralih ke `Admitted` melalui alur normal petugas.

### `ISS-EPS-026-T1` — Tombol Batalkan Pesanan ikut hilang

| | |
| --- | --- |
| **Lokasi kode** | Rantai source yang sama dengan `ISS-EPS-026-01` |
| **Jenis** | `BUG`, `CONTRACT_MISMATCH` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-EPS-026-01` |

Hook existing masih mengekspor `canCancel`, `requestCancelReservation`, dan `submitCancelReservation` (`use-inpatient-bed-board-actions.jsx:163-174`), sementara backend masih menyediakan `PATCH reservations/{id}/cancel` dengan permission `InpatientBedOccupancy:Update` (`InpatientBedOccupancyController.cs:194-231`). Namun tidak ada consumer pada modal denah. Akibatnya petugas juga tidak dapat melepas reservasi aktif dari tab Monitoring.

### `ISS-EPS-026-T2` — Bentuk hasil refresh denah perlu adapter

| | |
| --- | --- |
| **Lokasi kode** | `use-inpatient-bed-board-actions.jsx:38-45`; `use-floor-plan.js:179-216,240-269,359-362` |
| **Jenis** | `INTEGRATION_GAP` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-EPS-026-01` |

Hook aksi mencari bed terbaru pada `fresh.beds` atau `fresh.serviceUnits[].rooms[].beds`. Sebaliknya, `state.refresh()` milik denah mengembalikan `normalizeFloorPlanResponse`, dengan koleksi bed berada di `fresh.rooms[].beds` dan `fresh.roomDataByKey`. Jika hook hanya dipasang kembali tanpa adapter, proses penyiapan selalu menganggap reservasi berubah dan menolak membuka dialog. Adapter kecil wajib meratakan `fresh.rooms[].beds` menjadi kontrak `{ beds }` sebelum diberikan kepada hook aksi.

### `ISS-EPS-026-T3` — Test tidak menjaga aksi reservasi setelah migrasi denah

`tests/unit/floor-plan-monitoring.test.mjs` saat ini memeriksa normalisasi status, privasi, abort/stale guard, serta keberadaan aksi buka/tutup bed. Test tidak mengharuskan `Konfirmasi Masuk`, `Batalkan Pesanan`, consumer `useInpatientBedBoardActions`, dialog kegagalan penempatan, maupun adapter refresh tetap ada. Karena itu penghapusan kemampuan operasional lolos meskipun test fokus denah dinyatakan lulus.

---

## 5. Tanya-Jawab Pelapor

| Pertanyaan pelapor | Jawaban berbasis bukti sistem |
| --- | --- |
| "Bagaimana agar bisa dikonfirmasi masuk?" | Pada build saat ini **tidak ada langkah UI yang dapat dilakukan dari modal tersebut**; tombolnya memang tidak dirender dan layar monitoring lama sudah tidak dipakai. Perilaku yang benar adalah: klik kamar → pada bed `Dipesan` klik **Konfirmasi Masuk** → sistem membaca ulang bed → dialog konfirmasi tampil → setelah server menyetujui, modal tertutup dan bed berubah menjadi `Terisi`. Kemampuan API dan hook masih ada, jadi perbaikannya terutama penyambungan ulang frontend, bukan membuat proses bisnis baru. |
| Apakah pengguna perlu memanggil API secara manual? | Tidak. Pemanggilan langsung API melewati guard UI, penanganan konflik versi, dialog, dan umpan balik yang sudah disediakan aplikasi. Solusi yang aman adalah memperbaiki integrasi modal denah. |

---

## 6. Temuan Tambahan

1. Respons denah sudah membawa `StatusCode`, `OperationalVersion`, `AvailableActions`, dan `HoldingContext` (`InpFloorPlanQueryService.cs:264-277`; `use-floor-plan.js:81-110`). Data minimum untuk menyiapkan placement tersedia ketika pengguna memang berhak membaca identitas episode.
2. `AvailableActions` pada proyeksi bed saat ini diisi untuk aksi pembersihan (`InpBedManagementQueryService.cs:160-186`). Aksi placement/cancel existing karena itu harus mengikuti kontrak eksplisit Papan Tempat Tidur: status `Reserved`, metadata reservation/episode, permission efektif, refresh terbaru, dan validasi commit server; frontend tidak boleh mengarang kelayakan klinis.
3. Issue 012 sebelumnya sudah memperbaiki loading tombol dan auto-close modal untuk aksi yang sama. Implementasi baru seharusnya memakai ulang hook beserta callback `onSuccess`, bukan menulis ulang alur transaksi.
4. Nama pasien dan nomor rekam medis pada screenshot merupakan data sensitif. Dokumen ini sengaja tidak menyalin nilai tersebut.

---

## 7. Pertanyaan dan Keputusan yang Dibutuhkan

Tidak ada keputusan bisnis baru yang diperlukan. Blueprint sudah menetapkan kedua tombol dan backend sudah menyediakan endpoint-nya. Keputusan implementasi yang direkomendasikan adalah menyetujui `FIX-EPS-026-01` sebagai perbaikan regresi frontend dengan reuse penuh atas hook, modal, failure list, operation recovery, dan toast existing.

---

## 8. Catatan Pola

Pola masalahnya adalah **capability loss during view replacement**: sebuah layar diganti secara visual, tetapi capability operasional pada consumer lama tidak dimasukkan ke checklist parity layar baru. Pencegahannya adalah test parity berbasis capability, bukan hanya test visual/normalisasi: setiap penggantian kartu/tabel menjadi denah wajib membuktikan aksi existing tetap tersedia sesuai status dan permission.

---

## 9. Riwayat Dokumen

| Tanggal | Penulis | Perubahan |
| --- | --- | --- |
| 2026-10-17 | Codex | Analisis awal ISSUE-EPS-026 dari laporan pengguna, source frontend/backend, blueprint, kontrak, dan laporan task FE-RWI-247. |
| 2026-10-17 | Codex / Pemilik Sistem | Ditutup oleh `FE-RWI-263`; test fokus, lint, dan build lulus, lalu pemilik mengonfirmasi hasil runtime "sudah bisa". |

