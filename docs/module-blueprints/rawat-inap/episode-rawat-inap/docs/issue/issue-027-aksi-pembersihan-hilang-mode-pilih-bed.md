# ISSUE-027 — Aksi Pembersihan Tidak Tampil pada Pop-up Denah Mode Pilih Bed

```yaml
issue_id: ISSUE-EPS-027
module_id: rawat-inap
submodule: episode-rawat-inap
layar: "Denah Pilih Bed admisi dan transfer — Pop-up kamar (FE-DNH-02)"
sumber_laporan: "Laporan pemilik 17-10-2026: tombol untuk memulai pembersihan tidak terlihat pada bed Menunggu Pembersihan"
tanggal_issue: "2026-10-17"
status: DALAM_PERBAIKAN
keparahan_tertinggi: Medium
source_sha_backend: "c2bc563ed9332f08fa95b9fe35786fea7a647279 (MHamzah)"
source_sha_frontend: "15ae73984ad26a408e3038566d7e1d1533fe10c3 (HamzahV2; audit juga mencakup perubahan working tree yang belum di-commit)"
rencana_perbaikan: ../plan-repair/plan-repair-027-aksi-pembersihan-hilang-mode-pilih-bed.md
```

> **Catatan basis audit.** Source frontend sedang memiliki perubahan lokal yang belum di-commit
> dari perbaikan denah sebelumnya. Nomor baris frontend di dokumen ini mengacu pada working tree
> saat audit, dengan HEAD sebagaimana tercatat pada metadata. Perubahan lokal tersebut tidak
> disentuh oleh analisis ini.

## 1. Ringkasan

Pada pop-up kamar denah **mode Pilih Bed**, bed `325-A` terlihat berstatus **Menunggu
Pembersihan**, tetapi tidak menyediakan jalan masuk ke alur **Mulai Pembersihan**. Kemampuan
pembersihan tidak hilang dari sistem: komponen detail, hook, endpoint, aturan status, dan hak
aksesnya masih tersedia. Tombol tidak muncul karena komponen pop-up memilih cabang tampilan
khusus mode Pilih yang hanya menampilkan tombol `Pilih bed ini` atau alasan bed belum dapat
dipilih; aksi pembersihan berada pada cabang mode Lihat/Monitoring. Karena perilaku ini sesuai
dengan desain denah yang saat ini disetujui, masalah diklasifikasikan sebagai `DESIGN_CHANGE`
dengan keparahan `Medium`, bukan kerusakan backend.

## 2. Laporan asli

| No. laporan | Keluhan pelapor | Lampiran |
| ---: | --- | --- |
| 1 | "Sebelumnya ada flow mulai pembersihan, sekarang kok tidak ada button. Tolong lakukan analisis dan berikan planning." | Screenshot pop-up kamar `KELAS II 325`: `325-A` Menunggu Pembersihan tanpa tombol aksi; `325-B` dan `325-C` menampilkan `Pilih bed ini`. |

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | ID perbaikan |
| --- | ---: | --- | --- | --- | --- | --- | --- |
| `ISS-EPS-027-01` | 1 | Cabang mode Pilih menyembunyikan seluruh aksi pembersihan | `DESIGN_CHANGE` | Frontend + dokumen desain | Medium | `SUDAH-VERIFIKASI` di source dan blueprint; gejala `DARI-CAPTURE` | `FIX-EPS-027-01`, `FIX-EPS-027-02` |

## 4. Rincian per temuan

### ISS-EPS-027-01 — Cabang mode Pilih menyembunyikan seluruh aksi pembersihan

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend + dokumen desain |
| **Keparahan** | Medium |
| **Status bukti** | `SUDAH-VERIFIKASI` di source dan blueprint; gejala `DARI-CAPTURE` |
| **Perbaikan** | `FIX-EPS-027-01`, `FIX-EPS-027-02` |

**Apa yang terjadi.** Pada pop-up kamar mode Pilih, bed yang belum siap tetap ditampilkan agar
petugas memahami kondisi kamar. Namun, untuk bed Menunggu Pembersihan, layar hanya menampilkan
judul `BELUM DAPAT DIPILIH` beserta alasan penolakan. Petugas tidak dapat membuka panel
pembersihan dari pop-up tersebut. Petugas masih dapat memilih bed lain yang sudah Tersedia,
sehingga alur admisi atau transfer tidak sepenuhnya buntu.

**Kenapa terjadi.** Rantai sebabnya sudah dapat dibuktikan sebagai berikut.

1. `FloorPlanSelection` selalu membuka `FloorPlanRoomPopup` dengan `mode="selection"` pada
   `QuilvianSystemFrontendDev/src/components/features/health-services/inpatient-management/floor-plan/floor-plan-selection.jsx:105-112`.
2. `BedRow` menghitung `canClean` dari `bed.availableActions` pada
   `floor-plan-room-popup.jsx:107-114`. Artinya, data aksi pembersihan sebenarnya sudah dibaca.
3. Pada `floor-plan-room-popup.jsx:156-182`, ketika `selectionMode` bernilai benar, komponen hanya
   merender tombol `Pilih bed ini` untuk bed layak atau panel alasan untuk bed tidak layak.
4. Tombol `Pembersihan` baru berada pada cabang alternatif mode Lihat di
   `floor-plan-room-popup.jsx:183-221`. Karena kedua cabang saling meniadakan, nilai `canClean`
   tidak pernah menghasilkan tombol pada mode Pilih.
5. Jalur admisi reguler, admisi transfer, transfer bed, dan Workspace Keperawatan memakai
   `FloorPlanSelection`. Pada jalur admisi transfer, perubahan lokal dari ISSUE-024 baru saja
   memindahkan pemilihan dari daftar lama ke komponen denah ini, sehingga pembatasan mode Pilih
   menjadi lebih terlihat.

**Kemampuan pembersihan tidak hilang.** Bukti `SUDAH-VERIFIKASI`:

- `floor-plan-room-popup.jsx:487-498` masih merender `InpatientBedCleaningDetail` ketika target
  pembersihan dipilih.
- `inpatient-bed-cleaning-detail.jsx:91-109` masih menyediakan `Mulai Pembersihan`, `Selesai
  Pembersihan`, dan `Verifikasi Kesiapan` sesuai aksi yang diizinkan.
- `use-inpatient-bed-cleaning.js:72-85` memeriksa dua lapis: `AvailableActions` dari server dan
  permission efektif frontend. Sebelum tindakan dikirim, data bed juga dimuat ulang.
- Backend menyalin `AvailableActions` ke setiap bed denah pada
  `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpFloorPlanQueryService.cs:264-277`.
- Backend hanya menambahkan `StartCleaning`, `CompleteCleaning`, atau `VerifyReadiness` bila
  status bed, unit, penugasan alur kerja, dan hak pengguna sesuai pada
  `InpBedManagementQueryService.cs:160-176`.
- Endpoint pembersihan masih tersedia pada
  `InpatientBedManagementController.cs:94-127`.

**Batas bukti.** Screenshot tidak memperlihatkan payload jaringan. Karena itu belum dapat
dibuktikan apakah respons untuk pengguna pelapor memuat `AvailableActions: ["StartCleaning"]`
pada bed `325-A`. Hal tersebut tidak mengubah akar tampilan: cabang mode Pilih tetap
menyembunyikan tombol walaupun aksi dikirim server. Saat verifikasi oleh pemilik, respons
`GET /bed-occupancies/floor-plan` atau `GET /bed-management/floor-plan` perlu diperiksa tanpa
menyalin identitas pasien ke laporan. Jika `StartCleaning` tidak ada, penyebab lanjutannya adalah
hak/penugasan pengguna, bukan komponen tombol.

**Apakah ini menyimpang dari desain?** Tidak. Desain yang disetujui secara eksplisit membedakan
mode Lihat dan mode Pilih:

- `03-frontend-architecture.md:1490-1492` menyatakan mode Pilih menampilkan `Pilih bed ini`,
  sedangkan aksi pembersihan/tutup-buka/riwayat berada pada mode Lihat.
- `03-frontend-architecture.md:1524` kembali menetapkan bahwa mode Lihat memiliki
  start/complete/verify, sedangkan mode Pilih hanya memiliki `Pilih bed ini` pada bed layak.

Dokumen integrasi yang lebih umum menyatakan pembersihan tetap dijalankan dari pop-up kamar
(`docs/denah-map-ruangan-rs-mmc/rencana-integrasi-denah-bed.md:72`), tetapi kontrak tampilan yang
lebih rinci membatasinya ke mode Lihat. Karena source mengikuti kontrak tampilan rinci, kebutuhan
pelapor merupakan perubahan desain yang harus disetujui dan dicatat, bukan perbaikan diam-diam.

**Dampak nyata.** Contoh: bed `325-A` baru saja ditinggalkan pasien dan berstatus Menunggu
Pembersihan. Petugas berwenang yang sedang mencari bed untuk pasien berikutnya melihat alasan
bed belum dapat dipilih, tetapi harus meninggalkan konteks pemilihan dan masuk ke Monitoring Bed
untuk memulai pembersihan. Setelah itu petugas harus kembali ke alur semula dan mencari kamar
yang sama. Proses tetap mungkin dilakukan, tetapi berpindah konteks dan memperlambat kesiapan bed.

**Rekomendasi.** Izinkan panel pembersihan dibuka dari mode Pilih hanya bila server mengirim salah
satu aksi pembersihan dan permission efektif pengguna mengizinkannya. Gunakan kembali
`InpatientBedCleaningDetail`; jangan membuat form atau endpoint baru. Sesudah tindakan berhasil,
muat ulang denah pemilihan agar status dan kelayakan bed berubah sesuai hasil server. Bed tetap
tidak dapat dipilih sampai status akhirnya benar-benar `Available`.

## 5. Tanya-jawab pelapor

> **T:** Kenapa sebelumnya ada alur Mulai Pembersihan, tetapi sekarang tombolnya tidak ada?
>
> **J:** Tombolnya tidak dihapus dari sistem; layar pada screenshot sedang memakai mode Pilih
> Bed. Mode ini secara sengaja hanya menampilkan pilihan bed atau alasan bed belum layak, sedangkan
> tombol `Pembersihan` hanya dirender pada mode Lihat/Monitoring. Integrasi denah ke lebih banyak
> alur membuat pemisahan mode tersebut sekarang terlihat pada konteks yang Anda gunakan.

> **T:** Apakah backend pembersihan perlu dibuat ulang?
>
> **J:** Tidak. Endpoint, `AvailableActions`, pemeriksaan permission, panel detail, dan transisi
> status pembersihan masih tersedia. Perubahan utama berada pada aturan tampilan mode Pilih dan
> penyegaran datanya setelah tindakan berhasil.

## 6. Temuan tambahan

### ISS-EPS-027-T1 — Mode Pilih belum meneruskan callback penyegaran sesudah tindakan operasional

| | |
| --- | --- |
| **No. laporan** | — |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | `SUDAH-VERIFIKASI` di source frontend |
| **Perbaikan** | `FIX-EPS-027-02` |
| **Kenapa dicantumkan** | Ditemukan saat menelusuri apa yang akan terjadi bila tombol pembersihan ditampilkan pada mode Pilih |

**Apa yang terjadi.** `FloorPlanSelection` pada `floor-plan-selection.jsx:105-112` tidak
meneruskan `onChanged={state.refresh}` ke `FloorPlanRoomPopup`. Sebaliknya, mode Monitoring sudah
meneruskan callback tersebut pada `floor-plan-monitoring.jsx:125-145`.

**Dampak bila tidak ikut diperbaiki.** Tombol dapat ditampilkan dan tindakan server dapat berhasil,
tetapi baris bed di denah pemilihan berisiko tetap memperlihatkan status lama sampai pengguna
memuat ulang denah secara manual. Contoh: sesudah `StartCleaning`, tampilan masih dapat terbaca
`Menunggu Pembersihan` walaupun server sudah menyimpan `Cleaning`.

**Rekomendasi.** Hubungkan callback refresh milik state denah pada mode Pilih dan pertahankan
pilihan bed hanya bila bed terpilih masih layak pada snapshot baru.

## 7. Pertanyaan dan keputusan yang dibutuhkan

### Untuk pelapor

Tidak ada pertanyaan yang menahan analisis. Saat pengujian implementasi nanti, pelapor cukup
memastikan akun uji memang memiliki `InpatientBedManagement : Read` serta aksi pembersihan yang
sesuai agar skenario tombol terlihat dapat dibedakan dari skenario tidak berhak.

### Untuk pemilik

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| `K-027-01` | Apakah tindakan pembersihan boleh dijalankan langsung dari pop-up denah mode Pilih oleh pengguna yang juga berwenang mengelola pembersihan? | **A.** Tampilkan panel pembersihan berdasarkan `AvailableActions` ∩ permission; **B.** Pertahankan pemisahan dan beri tombol menuju Monitoring Bed | **A**, karena memakai ulang kontrol dan endpoint yang ada, tidak menambah hak, menjaga validasi server, serta menghindari kehilangan konteks pemilihan | `FIX-EPS-027-01`, `FIX-EPS-027-02` |

**Keputusan pemilik 17 Oktober 2026:** Opsi A disetujui. Implementasi frontend boleh
dilanjutkan; pengujian runtime dilakukan mandiri oleh pemilik dan agent hanya menjalankan
`npm run build` sebagai validasi command.

## 8. Catatan pola

ISSUE-024 memperluas pemakaian mode Pilih denah ke admisi transfer, sedangkan ISSUE-026
mengembalikan aksi transaksi pada mode Monitoring/Lihat. Pola yang berulang adalah komponen
denah bersama memiliki cabang mode yang terlalu eksklusif: ketika sebuah layar berpindah dari
daftar lama ke denah, kemampuan yang masih ada di hook atau service tidak otomatis ikut tersedia.
Pencegahannya adalah test kontrak per mode yang memeriksa matriks `status × AvailableActions ×
permission × mode`, bukan hanya mencari teks tombol secara global di source.

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-17 | Issue dibuat dari satu laporan dan satu screenshot; akar masalah diverifikasi pada frontend, backend, dan blueprint | `diagnose-module-issue` |
| 2026-10-17 | Pemilik menyetujui Opsi A; status dipindahkan ke `DALAM_PERBAIKAN` dengan batas frontend saja | Pemilik Sistem / `plan-module-delivery` |
| 2026-10-17 | Source frontend dan production build selesai; issue tetap `DALAM_PERBAIKAN` sampai UAT pemilik dilaporkan | `build-module-frontend` |
