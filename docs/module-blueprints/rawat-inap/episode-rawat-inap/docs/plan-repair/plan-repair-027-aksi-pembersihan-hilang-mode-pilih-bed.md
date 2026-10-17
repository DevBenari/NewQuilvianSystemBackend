# PLAN-REPAIR-027 — Kembalikan Alur Pembersihan pada Denah Mode Pilih Bed

```yaml
plan_id: PLAN-REPAIR-EPS-027
issue: ../issue/issue-027-aksi-pembersihan-hilang-mode-pilih-bed.md
status_rencana: IMPLEMENTASI_SELESAI_MENUNGGU_UAT
tanggal_rencana: "2026-10-17"
diputuskan_oleh: "Pemilik Sistem (user sesi)"
tanggal_keputusan: "2026-10-17"
basis_source_backend: "c2bc563ed9332f08fa95b9fe35786fea7a647279 (MHamzah)"
basis_source_frontend: "15ae73984ad26a408e3038566d7e1d1533fe10c3 (HamzahV2; working tree memiliki perubahan lokal yang harus dipertahankan)"
```

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-EPS-027-01` | `ISS-EPS-027-01` | Revisi kontrak tampilan agar mode Pilih dapat membuka pembersihan bila pengguna berwenang | Dokumen | 1 | `FE-RWI-264` | ✅ SELESAI | [Laporan FE-RWI-264](../../task/report/frontend/FE-RWI-264.md): arsitektur, skema, dan flow direvisi |
| `FIX-EPS-027-02` | `ISS-EPS-027-01`, `ISS-EPS-027-T1` | Tampilkan aksi pembersihan berbasis hak dan refresh denah sesudah tindakan | FE | 2 | `FE-RWI-264` | 🟡 SEBAGIAN — UAT pemilik pending | [Laporan FE-RWI-264](../../task/report/frontend/FE-RWI-264.md): source + production build selesai |

**Ringkasan: 1 dari 2 perbaikan selesai; implementasi frontend dan build selesai, tetapi UAT pemilik belum dilaporkan.**

## 2. Solusi terpilih per temuan

### ISS-EPS-027-01 — Cabang mode Pilih menyembunyikan seluruh aksi pembersihan

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A — direkomendasikan** | Pada mode Pilih, tampilkan tombol `Pembersihan` untuk bed yang respons server-nya memuat `StartCleaning`, `CompleteCleaning`, atau `VerifyReadiness`, lalu buka `InpatientBedCleaningDetail` yang sudah ada. Tindakan tetap membutuhkan permission efektif dan pemeriksaan server. | Alur langsung; tidak membuat endpoint/form baru; tidak memberi hak baru; hasil dapat langsung menyegarkan kelayakan bed. | Mengubah desain mode Pilih dan menyentuh komponen bersama, sehingga seluruh consumer perlu diuji. |
| B | Pertahankan mode Pilih tanpa aksi operasional, tetapi tambahkan tombol `Buka Monitoring Bed`. | Pemisahan fungsi tetap jelas. | Pengguna kehilangan konteks pasien, lantai, dan kamar; harus kembali lagi setelah pembersihan. |
| C | Selalu tampilkan `Mulai Pembersihan` berdasarkan status `WaitingCleaning` saja. | Tampak sederhana. | Ditolak: status tidak membuktikan hak, penugasan unit, siklus, atau versi bed; pengguna dapat diarahkan ke tindakan yang pasti ditolak server. |

**Solusi terpilih: Opsi A, disetujui pemilik pada 17 Oktober 2026.**

1. Backend sudah menghitung `AvailableActions` memakai status, unit, penugasan, dan hak pengguna.
2. `InpatientBedCleaningDetail` sudah menangani muat ulang, konfirmasi, versi bed, konflik, dan
   tiga transisi pembersihan.
3. Radius perubahan dapat dibatasi pada komposisi pop-up bersama dan callback refresh mode Pilih.
4. Bed tetap tidak dapat dipilih sampai server mengembalikan `Available`; frontend tidak
   menyimpulkan kesiapan sendiri.

**Kenapa bukan opsi lain.** Opsi B tidak memenuhi kebutuhan untuk memulai pembersihan dari konteks
yang sedang dibuka. Opsi C melemahkan kontrak keamanan dan alur kerja server.

### ISS-EPS-027-T1 — Mode Pilih belum meneruskan callback penyegaran

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A — direkomendasikan** | Teruskan `state.refresh` melalui `onChanged` ke pop-up, lalu pertahankan/bersihkan pilihan bed berdasarkan snapshot baru. | Pola sama dengan mode Monitoring; status dan kelayakan kembali authoritative dari server. | Perlu memastikan refresh gabungan jalur admisi transfer tetap memuat denah dan daftar eligible secara bersamaan. |
| B | Tutup pop-up lalu meminta pengguna menekan `Muat Ulang Denah`. | Perubahan source kecil. | Data usang tetap terlihat dan mudah disalahartikan. |

**Solusi terpilih: Opsi A.** Jalur admisi transfer memiliki adapter refresh gabungan; callback
harus menggunakan fungsi refresh dari state efektif agar denah dan overlay kelayakan tetap selaras.

## 3. Skema tampilan sebelum → sesudah

### SEBELUM — keadaan pada screenshot

```text
┌──────────────────────────── KELAS II 325 ────────────────────────────┐
│ KELAS II | 3 tempat tidur · 0 terisi                                │
│ Ringkasan dependensi (0 HD · 0 LD)                                  │
│                                                                      │
│ 325-A  ● Menunggu Pembersihan                                       │
│        BELUM DAPAT DIPILIH                                           │
│        • Bed belum siap atau masih dipegang                          │
│        • Keadaan saat ini: Pembersihan                               │
│        (tidak ada tombol tindakan)                                   │
│                                                                      │
│ 325-B  ● Tersedia                                  [ Pilih bed ini ] │
│ 325-C  ● Tersedia                                  [ Pilih bed ini ] │
└──────────────────────────────────────────────────────────────────────┘
```

### SESUDAH — target yang direkomendasikan

```text
┌──────────────────────────── KELAS II 325 ────────────────────────────┐
│ KELAS II | 3 tempat tidur · 0 terisi                                │
│ Ringkasan dependensi (0 HD · 0 LD)                                  │
│                                                                      │
│ 325-A  ● Menunggu Pembersihan                                       │
│        BELUM DAPAT DIPILIH                                           │
│        • Bed belum siap atau masih dipegang                          │
│        • Keadaan saat ini: Pembersihan            [ Pembersihan ]    │
│                                                                      │
│ 325-B  ● Tersedia                                  [ Pilih bed ini ] │
│ 325-C  ● Tersedia                                  [ Pilih bed ini ] │
│                                                                      │
│ ┌ Detail operasional 325-A — Menunggu Pembersihan ────────────────┐ │
│ │ [ Muat Ulang ] [ Mulai Pembersihan ]                            │ │
│ │ Riwayat pembersihan terbaru                                      │ │
│ └──────────────────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────────────┘
```

#### Wilayah SESUDAH

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Identitas dan status bed | Nama bed, enam status operasional, konflik bila ada | `FloorPlanBedResponse` | `BedRow`, status dot existing |
| Kelayakan pemilihan | `BELUM DAPAT DIPILIH` dan daftar alasan dari server | `Eligibility.Failures` | `bedEligibility` existing |
| Aksi pembersihan | Tombol `Pembersihan` hanya bila ada aksi pembersihan server dan permission efektif pengguna sesuai | `AvailableActions` + `useEffectivePermissions` | `BaseButton` existing |
| Detail operasional | Riwayat attempt dan tombol tindakan aktual | Endpoint monitoring, cleaning attempts, dan response `AvailableActions` terbaru | `InpatientBedCleaningDetail` existing |
| Aksi pemilihan | `Pilih bed ini` hanya untuk bed dengan `Eligibility.IsEligible = true` | Respons denah/adapter eligibility | `BaseButton` existing |

#### Tombol SESUDAH

| Label | Jenis | Kapan tampil/aktif | Yang terjadi |
| --- | --- | --- | --- |
| `Pembersihan` | Kedua | Tampil bila bed memiliki sekurang-kurangnya satu aksi pembersihan yang diizinkan server dan pengguna memiliki hak baca/aksi terkait | Membuka `InpatientBedCleaningDetail`; belum langsung mengubah status |
| `Mulai Pembersihan` | Utama | Snapshot terbaru masih `WaitingCleaning`, memuat `StartCleaning`, permission sah, dan tidak ada operasi tertunda | Membuka konfirmasi; setelah disetujui mengirim start cleaning dengan `CycleId` dan versi bed terbaru |
| `Selesai Pembersihan` | Utama | Snapshot terbaru `Cleaning/InProgress` dan `CompleteCleaning` diizinkan | Menyelesaikan pekerjaan fisik; bed tetap belum tersedia karena menunggu verifikasi |
| `Verifikasi Kesiapan` | Utama | Snapshot terbaru mengizinkan `VerifyReadiness`; bukti verifikasi telah diisi | Menyetujui atau menolak kesiapan sesuai form existing |
| `Pilih bed ini` | Utama | `Eligibility.IsEligible = true`; tidak sedang memuat atau melakukan tindakan | Menetapkan bed sebagai tujuan admisi/transfer |

#### Keadaan SESUDAH

| Keadaan | Perilaku yang diwajibkan |
| --- | --- |
| Memuat detail pembersihan | Tombol tindakan tidak dapat ditekan; indikator muat existing tampil |
| Tidak berhak | Tombol `Pembersihan` tidak tampil. Alasan bed belum dapat dipilih tetap tampil; tidak ada tombol palsu yang berakhir 403 |
| `WaitingCleaning` + `StartCleaning` | Tombol `Pembersihan` membuka detail yang menawarkan `Mulai Pembersihan` |
| `Cleaning/InProgress` + `CompleteCleaning` | Detail menawarkan `Selesai Pembersihan`; bed tetap tidak bisa dipilih |
| `Cleaning/AwaitingVerification` + `VerifyReadiness` | Detail menawarkan `Verifikasi Kesiapan`; bed tetap tidak bisa dipilih sebelum disahkan |
| Verifikasi disetujui | Denah dimuat ulang; bed menjadi `Available`; alasan penolakan hilang; `Pilih bed ini` tampil bila aturan kelayakan pasien juga lolos |
| Konflik versi/siklus (409) | Tindakan tidak diulang dengan data lama; detail dan denah dimuat ulang, lalu pesan server ditampilkan |
| Gagal validasi (422) | Pesan aturan server ditampilkan; status tidak disimpulkan atau diubah secara optimistis oleh frontend |

## 4. Rincian perbaikan

### FIX-EPS-027-01 — Revisi kontrak tampilan mode Pilih

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-027-01` |
| **Area** | Dokumen blueprint |
| **Jenis perubahan** | Perubahan desain yang harus disetujui pemilik |
| **Berkas yang diubah** | `03-frontend-architecture.md`, `05-skema-tampilan.md`, `flowcharts/15-denah-pilih-bed.md` |
| **Radius dampak** | Pop-up denah mode Pilih pada admisi reguler/bayi/transfer, Bed Transfer, detail episode, dan Workspace Keperawatan |
| **Bergantung pada** | Keputusan `K-027-01` |

**Langkah.**

1. Ubah matriks FE-DNH-02 agar mode Pilih dapat menampilkan aksi pembersihan sebagai tindakan
   sekunder, tanpa mengubah aturan kelayakan `Pilih bed ini`.
2. Nyatakan bahwa tombol hanya berasal dari irisan `AvailableActions` server dan permission
   pengguna; perubahan dokumen tidak memberikan permission baru kepada role mana pun.
3. Tambahkan jalur pembersihan opsional pada alur Pilih Bed: tindakan → refresh → evaluasi
   kelayakan ulang → pilih bed hanya jika akhirnya `Available` dan seluruh aturan pasien lolos.
4. Pertahankan enam status publik dan seluruh aturan privasi `HoldingContext`.

**Acceptance criteria.**

1. Dokumen tidak lagi menyatakan bahwa aksi start/complete/verify hanya boleh ada pada mode Lihat.
2. Dokumen menyatakan contoh konkret: bed `325-A` berstatus `WaitingCleaning` menampilkan
   `Pembersihan` hanya bila respons memuat `StartCleaning` dan pengguna berhak.
3. Dokumen menyatakan bahwa sesudah `StartCleaning`, bed berubah menjadi `Cleaning` tetapi belum
   boleh dipilih; sesudah verifikasi siap, server boleh mengembalikan `Available` dan kelayakan
   pasien dihitung ulang.
4. Tidak ada perubahan pada permission matrix, status, atau kontrak endpoint backend.

**Verifikasi.** Tinjau silang bagian FE-DNH-02, flowchart Pilih Bed, state-transition matrix, dan
permission-audit matrix agar tidak saling membantah.

**Risiko.** Kalimat yang terlalu umum dapat disalahartikan sebagai pemberian hak pembersihan
kepada semua petugas admisi. Mitigasinya adalah menulis guard hak dan penugasan pada setiap tabel
tombol, bukan hanya pada catatan kaki.

### FIX-EPS-027-02 — Tampilkan tindakan pembersihan dan refresh mode Pilih

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-027-01`, `ISS-EPS-027-T1` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-room-popup.jsx`; `floor-plan-selection.jsx`; `floor-plan.module.css` hanya bila susunan aksi memerlukannya |
| **Radius dampak** | Semua consumer `FloorPlanSelection`: langkah Pilih Bed admisi, admisi transfer, Bed Transfer, dan panel transfer Workspace Keperawatan. Mode Monitoring memakai pop-up yang sama dan wajib tetap berfungsi |
| **Bergantung pada** | `K-027-01`, `FIX-EPS-027-01` |

**Langkah.**

1. Pisahkan isi baris mode Pilih menjadi dua tanggung jawab yang dapat muncul bersamaan:
   kelayakan pemilihan dan aksi operasional yang sah.
2. Hitung visibilitas `Pembersihan` dari `AvailableActions` server serta permission efektif
   `InpatientBedManagement : Read` dan aksi terkait. Jangan menurunkan aksi dari warna atau
   `statusCode` saja.
3. Gunakan state `cleaningTarget` dan `InpatientBedCleaningDetail` yang sudah ada; jangan membuat
   modal, hook, service, atau endpoint baru.
4. Teruskan `state.refresh` dari `FloorPlanSelection` sebagai `onChanged`. Untuk adapter admisi
   transfer, gunakan refresh efektif yang memuat denah dan overlay `available-beds` bersama-sama.
5. Sesudah refresh, hapus pilihan aktif bila bed yang sebelumnya dipilih tidak lagi eligible.
6. Pertahankan perilaku mode Monitoring, fokus dialog, reduced motion, dan penanganan operasi
   tidak diketahui yang sudah ada.
7. Jangan menambah atau menjalankan test pada task ini sesuai instruksi pemilik. Validasi command
   agent dibatasi pada `npm run build`; pengujian runtime dilakukan mandiri oleh pemilik.

**Acceptance criteria.**

1. Dengan contoh bed `325-A` berstatus `WaitingCleaning`, respons berisi `StartCleaning`, dan akun
   berhak, pop-up mode Pilih menampilkan `Pembersihan` di samping alasan belum dapat dipilih.
2. Pada kondisi yang sama tetapi `AvailableActions` kosong atau permission ditolak, tombol tidak
   tampil; frontend tidak mengirim permintaan pembersihan.
3. Klik `Pembersihan` membuka detail existing. Detail memuat snapshot terbaru sebelum menawarkan
   `Mulai Pembersihan`.
4. Sukses `StartCleaning` memicu refresh denah. Status berubah menjadi `Cleaning` sesuai respons
   server dan `Pilih bed ini` tetap tidak tampil.
5. Sukses verifikasi siap memicu refresh denah. `Pilih bed ini` hanya tampil bila respons terbaru
   juga menetapkan `Eligibility.IsEligible = true`.
6. Kegagalan 403, 409, dan 422 menampilkan pesan existing tanpa menutup konteks secara keliru atau
   mengubah status secara optimistis.
7. Tombol `Pembersihan`, `Konfirmasi Masuk`, `Batalkan Pesanan`, `Riwayat`, dan `Tutup/Buka bed`
   pada mode Monitoring tidak mengalami regresi.
8. Tidak ada perubahan source backend, migration, konfigurasi, atau database.

**Verifikasi.** Sesuai arahan pemilik, implementer menyiapkan perubahan source tanpa menjalankan
test, lint, browser testing, `dotnet build`, atau migration. Satu-satunya command validasi yang
dijalankan agent adalah `npm run build`. Pengujian runtime dilakukan mandiri oleh pemilik.

**Risiko.** Komponen pop-up sedang memiliki perubahan lokal dari ISSUE-026. Implementasi harus
diterapkan di atas working tree saat ini tanpa menimpa aksi konfirmasi masuk/pembatalan. Risiko
lain adalah refresh jalur admisi transfer hanya memperbarui gambar denah tetapi tidak overlay
kelayakan; test perlu memastikan kedua sumber dimuat ulang.

### Proses bisnis target

**Tujuan.** Memungkinkan pengguna yang memang berwenang mempercepat kesiapan bed tanpa keluar
dari konteks pemilihan pasien.

**Pelaku.** Petugas yang dapat membuka layar Pilih Bed dan secara terpisah memiliki hak serta
penugasan pembersihan/verifikasi pada unit bed. Perubahan ini tidak menambah role atau permission.

**Pemicu.** Pengguna membuka kamar dan menemukan bed berstatus Menunggu Pembersihan, Dalam
Pembersihan, atau Menunggu Verifikasi.

**Prasyarat.** Bed kosong, tidak ditutup, siklus dan versi terbaru tersedia, respons server memuat
aksi yang sesuai, serta pengguna lolos permission dan penugasan unit.

**Langkah utama.**

1. Pengguna membuka kamar pada denah mode Pilih.
2. Sistem menampilkan alasan bed belum dapat dipilih.
3. Bila server mengizinkan aksi pembersihan, sistem menampilkan `Pembersihan`.
4. Pengguna membuka detail; frontend memuat ulang monitoring dan riwayat attempt.
5. Pengguna mengonfirmasi tindakan yang tersedia.
6. Backend memvalidasi status, cycle, version, permission, dan assignment lalu menyimpan transisi.
7. Frontend memuat ulang denah pemilihan beserta kelayakan pasien.
8. Pengguna baru dapat memilih bed setelah server menyatakan bed `Available` dan eligible.

**Perubahan status.**

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| `WaitingCleaning` | Mulai pembersihan | `Cleaning/InProgress` | Pengguna dengan `StartCleaning` dan assignment unit | Bed kosong, cycle/version terbaru |
| `Cleaning/InProgress` | Selesai pembersihan fisik | `Cleaning/AwaitingVerification` | Pengguna dengan `CompleteCleaning` dan assignment unit | Attempt aktif pada cycle terbaru |
| `Cleaning/AwaitingVerification` | Sahkan siap | `Available` | Verifikator dengan `VerifyReadiness` dan assignment sah | Bukti verifikasi wajib; master bed valid |
| `Cleaning/AwaitingVerification` | Tolak kesiapan | `WaitingCleaning` | Verifikator dengan `VerifyReadiness` dan assignment sah | Alasan penolakan wajib |

**Jalur tidak normal.** Permission berubah → tombol hilang setelah refresh; versi/cycle berubah →
HTTP 409 dan baca ulang; validasi ditolak → HTTP 422 dan tampilkan alasan; koneksi terputus setelah
kirim → gunakan mekanisme operasi existing, jangan membuat permintaan baru sembarangan.

**Hasil akhir.** Bed tetap tidak dapat dipilih selama belum siap. Jika verifikasi berhasil dan
aturan penempatan pasien lolos, bed menjadi kandidat pemilihan pada denah yang sama.

### Kontrak API yang digunakan — tidak berubah

#### Health Services / Inpatient Management / Bed Occupancy

Base URL: `api/v1/health-services/inpatient-management/bed-occupancies`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/floor-plan` | Memuat denah mode Pilih beserta status, `AvailableActions`, dan kelayakan setiap bed untuk episode | `InpatientBedOccupancy : Read` | Query `FloorPlanSelectionQuery` | `ApiResponse<FloorPlanResponse>` |

#### Health Services / Inpatient Management / Bed Management

Base URL: `api/v1/health-services/inpatient-management/bed-management`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/monitoring` | Membaca snapshot operasional terbaru sebelum menampilkan tindakan | `InpatientBedManagement : Read` | Query `BedMonitoringQuery` | `ApiResponse<BedMonitoringResponse>` |
| `GET` | `/beds/{bedId}/cleaning-attempts` | Membaca riwayat dan attempt aktif | `InpatientBedManagement : Read` | Route `bedId`, query halaman | `ApiResponse<PagedResult>` |
| `POST` | `/beds/{bedId}/cleaning-attempts` | Memulai pembersihan | `InpatientBedManagement : StartCleaning` | `StartBedCleaningRequest` | `ApiResponse<BedOperationResponse>` |
| `PATCH` | `/cleaning-attempts/{attemptId}/complete` | Menyelesaikan pekerjaan fisik | `InpatientBedManagement : CompleteCleaning` | `CompleteBedCleaningRequest` | `ApiResponse<BedOperationResponse>` |
| `POST` | `/beds/{bedId}/readiness-verifications` | Menyetujui atau menolak kesiapan bed | `InpatientBedManagement : VerifyReadiness` | `VerifyBedReadinessRequest` | `ApiResponse<BedOperationResponse>` |

Arti status yang perlu diverifikasi: `200` berarti tindakan tersimpan; `400` berarti masukan tidak
valid; `403` berarti pengguna tidak berhak atau di luar assignment; `409` berarti versi/siklus
sudah berubah dan harus dimuat ulang; `422` berarti aturan status atau bisnis tidak terpenuhi.

## 5. Urutan pengerjaan

```text
K-027-01 ✅ — persetujuan pemilik atas aksi pembersihan di mode Pilih
└── FIX-EPS-027-01 ✅ — revisi kontrak tampilan dan alur
    └── FIX-EPS-027-02 🟡 — source dan npm run build selesai; UAT pending
        └── UAT oleh pemilik — dilakukan mandiri sesudah handoff
```

## 6. Keputusan pemilik yang dibutuhkan

| No | Keputusan | Pilihan | Dampak | Menahan |
| ---: | --- | --- | --- | --- |
| `K-027-01` | Izinkan tindakan pembersihan dari pop-up denah mode Pilih bagi pengguna yang juga memiliki hak operasional | **A — DISETUJUI.** Tampilkan berdasarkan `AvailableActions` ∩ permission; **B.** Tetap pisah dan hanya beri tautan ke Monitoring Bed | Opsi A mempercepat alur tanpa memberi hak baru, tetapi mengubah kontrak tampilan mode Pilih | Tidak ada; keputusan sudah diberikan 17 Oktober 2026 |

## 7. Dokumen hulu yang ikut direvisi

| Dokumen | Bagian | Sekarang | Menjadi |
| --- | --- | --- | --- |
| `03-frontend-architecture.md` | 16.3, skema FE-DNH-02 | `Mode Lihat: aksi pembersihan...`; mode Pilih hanya `Pilih bed ini` | Mode Pilih tetap menawarkan `Pilih bed ini` hanya pada bed layak, serta dapat membuka pembersihan bila `AvailableActions` dan permission pengguna mengizinkan |
| `03-frontend-architecture.md` | Tabel wilayah FE-DNH-02 baris bed | Lihat: start/complete/verify; Pilih: `Pilih bed ini` | Pilih: `Pilih bed ini` pada bed layak; pembersihan pada bed terkait hanya untuk pengguna berwenang; refresh kelayakan sesudah tindakan |
| `05-skema-tampilan.md` | Langkah Pilih Tempat Tidur | Belum menjelaskan tindakan pembersihan pada pop-up denah mode Pilih | Tambahkan keadaan bed belum siap dan tombol pembersihan bersyarat tanpa mengubah aturan pemilihan |
| `flowcharts/15-denah-pilih-bed.md` | Langkah Buka kamar/Pilih bed | Bed tidak layak hanya menampilkan alasan dan pengguna memilih bed lain | Pengguna berwenang boleh menangani pembersihan dari pop-up, kemudian sistem memuat ulang; pemilihan tetap menunggu status siap dan kelayakan server |

Revisi dokumen hulu tidak dilakukan pada tahap diagnosis ini.

## 8. Verifikasi menyeluruh

Skenario runtime berikut diserahkan kepada pemilik. Agent tidak menjalankan testing; agent hanya
menjalankan `npm run build` setelah implementasi.

1. Gunakan akun yang memiliki `InpatientBedOccupancy : Read`, `InpatientBedManagement : Read`,
   dan `InpatientBedManagement : StartCleaning` pada unit uji.
2. Buka alur Pilih Bed, pilih kamar yang memiliki satu bed `WaitingCleaning` dan satu bed
   `Available`.
3. Pastikan bed `WaitingCleaning` menampilkan alasan tidak layak serta tombol `Pembersihan`;
   bed `Available` menampilkan `Pilih bed ini`.
4. Buka `Pembersihan`, pilih `Mulai Pembersihan`, dan konfirmasi.
5. Pastikan request start berhasil, denah refresh, status menjadi `Cleaning`, dan bed belum dapat
   dipilih.
6. Selesaikan pembersihan lalu verifikasi kesiapan memakai akun yang sesuai. Pastikan bed baru
   dapat dipilih setelah response terbaru menyatakan `Available` dan eligible.
7. Ulangi dengan akun tanpa hak pembersihan. Tombol tidak boleh tampil dan tidak boleh ada request
   ke endpoint mutasi.
8. Paksa stale version/cycle untuk memastikan HTTP 409 memuat ulang data dan tidak menggandakan
   tindakan.
9. Periksa regresi mode Monitoring: konfirmasi masuk, batal pesanan, pembersihan, riwayat, dan
   tutup/buka bed tetap tampil sesuai hak.
10. Agent menjalankan `npm run build`. Pemilik menjalankan pengujian runtime secara mandiri.

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-17 | Rencana dibuat dengan dua perbaikan; menunggu keputusan `K-027-01` | `diagnose-module-issue` |
| 2026-10-17 | Opsi A disetujui; task `FE-RWI-264` dialokasikan dan batas validasi agent dikunci hanya `npm run build` | Pemilik Sistem / `plan-module-delivery` |
| 2026-10-17 | Dokumen dan source diimplementasikan; production build lulus; status tetap sebagian sampai UAT pemilik | `build-module-frontend` |
