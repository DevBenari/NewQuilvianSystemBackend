# Arsitektur Frontend — Nurse View

| Field | Nilai |
|---|---|
| Blueprint ID | `nurse-view` · revision `1` · `approved` — Rizki Gunawan, 2026-10-28 (`NV-DEC-028`) |
| Owner | Rizki Gunawan |
| Frontend SHA | `76b0c6e4b` (branch `RizkiV2`) |
| Masukan | `00-interview-decisions.md` rev `2`, `contracts/api-contract.md` `0.1.0` |
| Konvensi | Pakai ulang komponen dan gaya yang sudah ada; `globals.css` tidak disentuh |

> **Revision 2 (`approved` `NV-DEC-055`, kontrak `0.2.0`) — 2026-11-05.** Layar tablet bed, Konsol Panggilan,
> strip TV, dan admin tablet ada di
> [Revision 2](#revision-2--tablet-bed-dan-panggilan-pasien). Bagian 1–8 tetap revision 1.

## 1. Kebutuhan layar

| ID | Layar | Pengguna | Keputusan |
|---|---|---|---|
| `FE-NVW-01` | Daftar Perangkat Nurse View | Admin IT | `NV-DEC-016` |
| `FE-NVW-02` | Form Perangkat (buat/ubah) + aksi login & nonaktif | Admin IT | `NV-DEC-016`, `022` |
| `FE-NVW-03` | Interval Ukur Ulang TTV | Admin IT; baca: kepala ruang | `NV-DEC-010` |
| `FE-NVW-04` | Papan Lantai di TV | Akun perangkat | `NV-DEC-006`–`026` |

## 2. Peta butir menu

```text
Administrator                                   <- sudah ada, tingkat 0
└── Master Data (grup yang sudah memuat "Display Antrian")
    ├── Display Nurse View                      -> /administrator/master-data/nurse-view-devices
    └── Interval TTV Nurse View                 -> /administrator/master-data/nurse-view-vital-sign-intervals
```

| Butir menu | Tingkat | Induk | `pathname` | Layar | Butir hak akses | Status |
|---|:---:|---|---|---|---|---|
| Display Nurse View | 2 | Administrator › Master Data | `/administrator/master-data/nurse-view-devices` | `FE-NVW-01` | `NurseViewDisplayDevice : Read` | Baru |
| Interval TTV Nurse View | 2 | Administrator › Master Data | `/administrator/master-data/nurse-view-vital-sign-intervals` | `FE-NVW-03` | `NurseViewVitalSignInterval : Read` | Baru |

| Layar tanpa butir menu | Jalan masuk |
|---|---|
| `FE-NVW-02` Form Perangkat | Dari `FE-NVW-01`: tombol Tambah (`/nurse-view-devices/create`) dan baris daftar (`/nurse-view-devices/[slug]`) |
| `FE-NVW-04` Papan Lantai | Akun perangkat login → `proxy.js` mengalihkan ke `/nurse-view-display` (pola `/queue-display`). Tidak dibuka akun manusia |

Induk dan route mengikuti tetangga terdekat (`queue-display-devices`). Nama butir dan urutan
`DEV_DISCRETION`. Berkas yang disunting: `src/utils/menu-sidebar/menu-items.jsx` (`subMenu`).

## 3. Skema fitur per layar

### 3.1 `FE-NVW-01` Daftar Perangkat

```text
+- Display Nurse View ---------------------------------------- FE-NVW-01 -+
| [cari kode/nama]  [Lantai v]  [Status v]                     [+ Tambah] |
+-------------------------------------------------------------------------+
| Kode       | Nama                 | Lantai | Status   | Online         |
| NVW-000001 | TV Nurse Station L3  | 3      | Aktif    | Online         |
| NVW-000002 | TV Nurse Station L4  | 4      | Nonaktif | Offline 21.00  |
+-------------------------------------------------------------------------+
| memuat -> kerangka baris                                                |
| kosong -> "Belum ada perangkat Nurse View."            [+ Tambah]       |
| gagal  -> "Data perangkat gagal dimuat."               [Coba lagi]      |
+- Halaman 1 dari n ------------------------- [< Sebelumnya] [Berikutnya >]+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Saringan | kata kunci, lantai, status | `GET /display-devices/floors` | `NurseViewDisplayDevice : Read` | Lantai gagal dimuat → saringan lantai disembunyikan |
| Tabel | kode, nama, lantai, status, online | `GET /display-devices` | `NurseViewDisplayDevice : Read` | Lihat skema |
| Tombol Tambah | ke `FE-NVW-02` | — | `NurseViewDisplayDevice : Create` | Disembunyikan bila tidak berhak |

### 3.2 `FE-NVW-02` Form Perangkat

```text
+- Perangkat NVW-000001 -------------------------------------- FE-NVW-02 -+
| Status: Aktif · Online · Login dibuat 28 Okt 10.05 oleh Admin IT        |
| [Buat Login] [Atur Ulang Login] [Nonaktifkan]                           |
+-------------------------------------------------------------------------+
| Nama perangkat  [TV Nurse Station Lantai 3          ]                   |
| Lantai          [3 v]                                                   |
| Lokasi          [Nurse station sayap timur          ]                   |
| Keterangan      [                                   ]                   |
|                                                       [Batal] [Simpan]  |
+-------------------------------------------------------------------------+
| Kotak kata sandi (hanya sesudah Buat/Atur Ulang Login):                 |
|   Nama pengguna nvw-000001 · Kata sandi Xk7#p2Qm9v  [Salin]             |
|   "Kata sandi hanya tampil sekali. Catat sebelum menutup."              |
+-------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Kepala | status, online, jejak login/nonaktif | `GET /display-devices/{id}` | `: Read` | Gagal → pesan + coba lagi |
| Tombol aksi | Buat Login / Atur Ulang Login / Nonaktifkan, mengikuti `AvailableActions` | `POST /{id}/login`, `POST /{id}/login/reset`, `PATCH /{id}/deactivate` | `NurseViewDisplayDevice : Update` | Tombol yang tidak tersedia disembunyikan. `409` → "Data perangkat sudah diubah orang lain…" + muat ulang |
| Form | nama, lantai, lokasi, keterangan | `POST /` atau `PUT /{id}`; pilihan lantai dari `GET /floors` | `: Create` / `: Update` | Pesan validasi dari `validation-matrix.md` di bawah isian |
| Kotak kata sandi | hanya dari response login | `NvwDisplayDeviceLoginResponse` | `: Update` | Tidak disimpan di state global, localStorage, maupun log; hilang saat layar ditutup |
| Nonaktifkan | konfirmasi + alasan wajib | `PATCH /{id}/deactivate` | `: Update` | — |

### 3.3 `FE-NVW-03` Interval Ukur Ulang TTV

```text
+- Interval Ukur Ulang TTV ----------------------------------- FE-NVW-03 -+
| Status: Belum dikonfigurasi — tanda "lewat jadwal" tidak tampil di TV   |
+-------------------------------------------------------------------------+
| Tingkat EWS | Interval (menit) |                                        |
| Rendah      | [ 720 ]          |                                        |
| Sedang      | [ 240 ]          |                                        |
| Tinggi      | [  60 ]          |                                        |
| Kritis      | [  15 ]          |                                        |
| No. SK/Notulen Komite [SK-KOMKEP/012/X/2026]  Tanggal [20-10-2026]      |
| Terakhir diubah: Admin IT, 28 Okt 2026 10.30                   [Simpan] |
+-------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Status | dikonfigurasi atau belum | `GET /vital-sign-intervals` (`IsConfigured`) | `NurseViewVitalSignInterval : Read` | Kosong → "Belum dikonfigurasi — tanda lewat jadwal tidak tampil di TV." |
| Tabel isian | empat tingkat, boleh dikosongkan per tingkat | idem | `: Read` | Gagal → pesan + coba lagi |
| Simpan | ganti-seluruh | `PUT /vital-sign-intervals` | `NurseViewVitalSignInterval : Update` | Tombol dan isian **dibaca saja** bila tidak berhak |

**Tidak ada angka bawaan di form** (`G-01`). Angka pada skema hanya contoh.

### 3.4 `FE-NVW-04` Papan Lantai di TV

```text
+- LANTAI 3 ------------------------------------ 14.20 · Data 14.20 -+
| BANGSAL BEDAH          Perawat jaga: Sari, Dewi, Rina (Pagi)       |
| +-----------+ +-----------+ +-----------+ +-----------+             |
| |305-A !!!  | |301-A      | |301-B      | |302-A      |             |
| |B.S. L 64th| |A.R. P 41th| | Kosong    | | Dipesan — |             |
| |PJ Sari    | |PJ Belum   | |           | | pasien    |             |
| |EWS 8 KRIT | |EWS 2 08.10| |           | | akan datang             |
| |[J][A]     | |lewat 30mn | |           | |           |             |
| +-----------+ +-----------+ +-----------+ +-----------+             |
| BANGSAL ANAK           Roster shift ini belum tersedia              |
| +-----------+ ...                                                   |
+----------------------------------------------- Halaman 1 / 2 ------+
| [pita abu] "Data terakhir 14.18 — koneksi terputus"   (bila basi)   |
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Kepala | lantai, jam sekarang, jam data | `GET /display-runtime/current`, `AsOfUtc` | policy `NurseViewDisplayRead` | `401`/`403` → layar login |
| Kelompok bangsal | nama bangsal, perawat jaga | `Wards[].OnDutyNurses`, `RosterAvailable` | idem | `RosterAvailable = false` → "Roster shift ini belum tersedia" |
| Kartu bed terisi | bed, inisial · L/P · umur, PJ, EWS + TTV + jam, lewat jadwal, ikon J (risiko jatuh tinggi), A (alergi), I (isolasi), tanda "Bed tidak sesuai isolasi" | `Beds[]` | idem | Tanpa PJ → "Belum ada PJ"; tanpa TTV → "Belum ada TTV" |
| Kartu bed kosong | status ringkas abu-abu | `StatusCode`/`StatusLabel` | idem | — |
| Urutan | kartu kritis (`IsCritical`) teratas per bangsal | — | — | — |
| Halaman | ganti halaman otomatis bila tidak muat | — | — | Durasi per halaman `DEV_DISCRETION` |
| Pita data basi | jam data terakhir | tidak ada papan baru > `StaleAfterSeconds` | — | — |
| Lantai tanpa kamar | — | `Wards` kosong | — | "Belum ada kamar terdaftar untuk lantai ini. Hubungi IT." |

## 4. Aksi per peran

Diturunkan dari `contracts/permission-audit-matrix.md` §2. Admin IT mengelola `FE-NVW-01`–`03`;
kepala ruang hanya membaca `FE-NVW-03` bila diberi; akun perangkat hanya `FE-NVW-04`.

## 5. Data, cache, dan pembaruan papan

| Hal | Ketentuan |
|---|---|
| Muat ulang | `GET /board` setiap `RefreshIntervalSeconds` (60) |
| Push | Bergabung `JoinNurseViewFloor`; event `NurseViewBoardChanged` → muat ulang segera (dibatasi paling sering 1×/5 detik) |
| Sambung ulang | Pola `queue-realtime-client.js` (`withAutomaticReconnect`) |
| Data basi | Bila tidak ada papan baru > 120 detik → pita abu-abu; papan lama tetap tampil di bawahnya |
| Tersambung kembali | Muat ulang penuh, pita hilang |
| Penyimpanan | Papan **tidak** disimpan ke localStorage/sessionStorage; hanya di memori |

## 6. Penanganan keadaan layar admin

| Keadaan | Perilaku |
|---|---|
| Memuat | Kerangka baris/form |
| Kosong | Kalimat pada skema |
| Gagal | Pesan + Coba lagi |
| Kirim ganda | Tombol Simpan/Buat Login/Nonaktifkan dikunci selama permintaan berjalan |
| Data diubah orang lain (`409`) | Pesan + muat ulang detail |

## 7. Berkas bersama frontend — direstui lead dengan syarat (`NV-DEC-030`)

Syarat: redirect Kiosk (`/kiosk`) dan display antrean (`/queue-display`) di `proxy.js` serta penanganan login lain di `use-base-login.jsx` **tidak berubah**; Nurse View hanya menambah cabang baru yang meniru cabang display antrean (`NV-AC-023`–`027`).

| Berkas | Perubahan |
|---|---|
| `src/proxy.js` | Konstanta `NURSE_VIEW_DISPLAY_PATH = "/nurse-view-display"`; akun bercookie `isNurseViewAccount` dialihkan ke sana dan dicegah membuka route lain |
| `src/lib/hooks/auth/use-base-login.jsx` | Mengenali klaim/response akun perangkat Nurse View dan menyetel cookie `isNurseViewAccount` |
| `src/utils/menu-sidebar/menu-items.jsx` | Dua butir menu (bagian 2) |
| `src/components/providers/root-layout-shell.jsx`, `src/components/ui/auth-wrapper.jsx`, `src/components/providers/session-activity-manager.jsx` | Masing-masing satu baris `"/nurse-view-display"` pada daftar prefix, persis seperti `/queue-display` (`NV-DEC-032`) |

## 8. Kewenangan UI

| Area | Keputusan | Status |
|---|---|---|
| Isi kartu, urutan kritis, pengelompokan per bangsal, ganti halaman, keterbacaan ±3 m, tanpa suara | Owner | `approved` (`NV-FE-001`–`003`) |
| Warna, ikon, jarak, durasi halaman, tata letak header | Developer | `DEV_DISCRETION` (`NV-FE-004`) |
| Route halaman TV `/nurse-view-display` | Developer, mengikuti pola `/queue-display` | `DEV_DISCRETION` (`NV-FE-005`) |
| Penempatan menu di Administrator › Master Data | Usulan desain, mengikuti "Display Antrian" | Menunggu approval blueprint |
| Bentuk konfirmasi nonaktif (modal atau inline) | Developer | `DEV_DISCRETION` |

## Revision 2 — Tablet bed dan panggilan pasien

| Field | Nilai |
|---|---|
| Revision | `2` · status `approved` — Rizki Gunawan, 2026-11-05 (`NV-DEC-055`) |
| Frontend SHA | `91a11b08f` (branch `RizkiV2`) |
| Masukan | `00-interview-decisions.md` rev `5` (`NV-DEC-034`–`054`, `NV-FE-006`, `007`); `contracts/api-contract.md` `0.2.0` |
| Kewenangan | security/privasi/invariant → keputusan owner → konvensi proyek → `DEV_DISCRETION` |

### R2.1 Kebutuhan layar

| ID | Layar | Pengguna | Status | Keputusan |
|---|---|---|---|---|
| `FE-NVW-04` | Papan Lantai di TV — ditambah **strip Panggilan Aktif** dan penanda kartu bed | Akun TV | Diperbarui | `NV-DEC-044`, `047`, `048` |
| `FE-NVW-05` | Daftar Tablet Bed | Admin IT | Baru | `NV-DEC-035` |
| `FE-NVW-06` | Form Tablet Bed (buat/ubah) + aksi login & nonaktif | Admin IT | Baru | `NV-DEC-035`, `AD-01`–`04` |
| `FE-NVW-07` | Layar Tablet Bed | Pasien/keluarga; perawat untuk "Perawat hadir" | Baru | `NV-DEC-037`–`039`, `041`, `046`, `050`, `054` |
| `FE-NVW-08` | Konsol Panggilan | Perawat, kepala ruang (akun sendiri) | Baru | `NV-DEC-034`, `040`, `043`–`045`, `053` |
| `FE-NVW-09` | Ambang Lama Menunggu | Admin IT; baca: kepala ruang | Baru | `NV-DEC-048` |

### R2.2 Peta butir menu

```text
Administrator                                     <- sudah ada, tingkat 0
└── Master Data                                   <- sudah ada
    ├── Display Nurse View                        <- sudah ada
    ├── Interval TTV Nurse View                   <- sudah ada
    ├── Tablet Bed Nurse View                     -> /administrator/master-data/nurse-view-bed-tablets
    └── Ambang Panggilan Nurse View               -> /administrator/master-data/nurse-view-call-wait-thresholds
Rawat Inap                                        <- sudah ada, tingkat 0 (induk disahkan NV-FE-008)
└── Konsol Panggilan Pasien                       -> /health-services/nurse-view-management/patient-call-console
```

| Butir menu | Tingkat | Induk | `pathname` | Layar | Butir hak akses | Status |
|---|:---:|---|---|---|---|---|
| Tablet Bed Nurse View | 2 | Administrator › Master Data | `/administrator/master-data/nurse-view-bed-tablets` | `FE-NVW-05` | `NurseViewBedTablet : Read` | Baru |
| Ambang Panggilan Nurse View | 2 | Administrator › Master Data | `/administrator/master-data/nurse-view-call-wait-thresholds` | `FE-NVW-09` | `NurseViewPatientCallWaitThreshold : Read` | Baru |
| Konsol Panggilan Pasien | 1 | Rawat Inap (`NV-FE-008`) | `/health-services/nurse-view-management/patient-call-console` | `FE-NVW-08` | `NurseViewPatientCall : Read` | Baru |

| Layar tanpa butir menu | Jalan masuk |
|---|---|
| `FE-NVW-06` Form Tablet Bed | Dari `FE-NVW-05`: tombol Tambah (`/nurse-view-bed-tablets/create`) dan baris daftar (`/nurse-view-bed-tablets/[slug]`) |
| `FE-NVW-07` Layar Tablet Bed | Akun tablet login → `proxy.js` mengalihkan ke `/nurse-view-bed-tablet` (pola `/nurse-view-display`). Tidak dibuka akun manusia |
| `FE-NVW-04` Papan Lantai | Tetap seperti revision 1 |

**Catatan `NV-FE-008`.** Konsol dipakai perawat rawat inap, jadi diusulkan di grup Rawat Inap.
Grup itu milik modul Rawat Inap, sehingga penambahannya dikabarkan ke Muhammad Hamzah.
Alternatifnya butir tingkat 0 sendiri "Nurse View". Route tetap milik Nurse View. **Disahkan owner
2026-11-05 (`NV-DEC-055`): grup Rawat Inap.** Nama dan urutan butir `DEV_DISCRETION`.

### R2.3 Skema fitur per layar

#### `FE-NVW-05` Daftar Tablet Bed

Bentuknya sama dengan `FE-NVW-01`, dengan kolom bed.

```text
+- Tablet Bed Nurse View ------------------------------------- FE-NVW-05 -+
| [cari kode/nama/bed]  [Lantai v]  [Status v]                [+ Tambah]  |
+-------------------------------------------------------------------------+
| Kode       | Nama              | Bed    | Lantai | Status   | Online     |
| NVT-000001 | Tablet Bed 305-A  | 305-A  | 3      | Aktif    | Online     |
| NVT-000002 | Tablet Bed 306-B  | 306-B  | 3      | Belum ada login | —   |
+-------------------------------------------------------------------------+
| memuat -> kerangka baris                                                |
| kosong -> "Belum ada tablet bed."                      [+ Tambah]       |
| gagal  -> "Data tablet gagal dimuat."                  [Coba lagi]      |
+- Halaman 1 dari n ------------------------- [< Sebelumnya] [Berikutnya >]+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Saringan | kata kunci, lantai, status | `GET /bed-tablets/floors` | `NurseViewBedTablet : Read` | Lantai gagal dimuat → saringan lantai disembunyikan |
| Tabel | kode, nama, bed, lantai, status, online | `GET /bed-tablets` | `NurseViewBedTablet : Read` | Lihat skema |
| Tambah | ke `FE-NVW-06` | — | `NurseViewBedTablet : Create` | Disembunyikan bila tidak berhak |

#### `FE-NVW-06` Form Tablet Bed

Bentuknya sama dengan `FE-NVW-02`; isian lantai diganti pemilih bed.

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Kepala | status, online, jejak login/nonaktif, "Ada panggilan aktif" bila ada | `GET /bed-tablets/{id}` | `: Read` | Gagal → pesan + coba lagi |
| Tombol aksi | Buat Login / Atur Ulang Login / Nonaktifkan mengikuti `AvailableActions` | `POST /{id}/login`, `POST /{id}/login/reset`, `PATCH /{id}/deactivate` | `NurseViewBedTablet : Update` | `409` → pesan `validation-matrix.md` + muat ulang |
| Pemilih bed | lantai lalu bed; bed yang sudah punya tablet tidak bisa dipilih | `GET /bed-tablets/floors`, `GET /bed-tablets/bed-options?floorCode=` | `: Read` | Tanpa bed di lantai itu → "Lantai ini belum punya bed aktif." |
| Form | nama, lokasi, keterangan | `POST /` atau `PUT /{id}` | `: Create` / `: Update` | Pesan validasi di bawah isian |
| Kotak kata sandi | hanya dari response login | `NvwBedTabletLoginResponse` | `: Update` | Tidak disimpan di state global, `localStorage`, maupun log |

#### `FE-NVW-07` Layar Tablet Bed

```text
+- Bed 305-A · Kamar Melati 305 ------------------------------ FE-NVW-07 -+
|                                                                         |
|   Siap:                                                                 |
|   [        PANGGIL PERAWAT        ]   [          DARURAT          ]     |
|                                                                         |
|   Menunggu (biasa):                                                     |
|   "Panggilan terkirim · menunggu 1:10"                                  |
|   [ Batal ]                     [ DARURAT ]                             |
|                                                                         |
|   Diklaim:  "Dewi menuju"                                               |
|   Darurat:  "Panggilan DARURAT terkirim · menunggu 0:20" (tanpa Batal)  |
|                                                                         |
|   Gagal kirim: "Panggilan belum terkirim — tekan bel di dinding"        |
|   Bed kosong:  "Bed belum berpenghuni"                                  |
+-------------------------------------------------------------------------+
| ........ area perawat: [ Perawat hadir — tahan 3 detik ] ........       |
+-------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Kepala | kode bed, nama kamar. **Tanpa** nama atau inisial pasien (`G-09`) | `GET /bed-tablet-runtime/current` | policy `NurseViewBedTabletAccess` | `401`/`403` → layar login |
| Tombol panggil | "Panggil perawat", "Darurat" | `POST /bed-tablet-runtime/calls` | idem | Disembunyikan saat `BedKosong`. Gagal → `GagalKirim` |
| Status panggilan | status, lama menunggu, nama perawat bila diklaim | `ActiveCall` | idem | — |
| Batal | hanya bila `CanCancel` | `POST /calls/current/cancel` | idem | `409` → muat ulang keadaan |
| Area perawat | "Perawat hadir — tahan 3 detik", hanya bila ada panggilan aktif, termasuk saat bed kosong (`NV-DEC-050`) | `POST /calls/current/nurse-arrived` | idem | Dilepas < 3 detik → tidak terkirim |
| Pita terputus | "Koneksi terputus — gunakan bel di dinding" | status realtime + muat ulang | — | — |

Ketentuan yang mengikat: dua tombol utama besar dan berbeda jelas; Darurat tidak bisa ditekan
tidak sengaja dari tombol biasa; area perawat terpisah dan tidak menyerupai tombol pasien.
Ukuran, warna, dan susunan `DEV_DISCRETION` (`NV-FE-010`).

#### `FE-NVW-08` Konsol Panggilan

```text
+- Konsol Panggilan · LANTAI 3 ---------------- (ganti lantai) FE-NVW-08 -+
| Sari · 10.01.20      [Aktifkan suara]  [Matikan bunyi]                  |
| "Pengingat ditunda sampai 10.06"   (bila ditunda)                       |
+-------------------------------------------------------------------------+
| Bed   | Pasien          | Jenis    | Menunggu | Status       | Aksi      |
| 331-C | R.A. · P · 72th | DARURAT  | 0:20     | Menunggu     | [Saya tangani] [Tutup] |
| 305-A | B.S. · L · 64th | Panggilan| 1:00 ×2  | Sari menuju  | [Lepas] [Tutup]        |
| 318-B | A.R. · P · 41th | Panggilan| 0:40     | Dewi menuju  | [Ambil alih] [Tutup]   |
+-------------------------------------------------------------------------+
| kosong -> "Tidak ada panggilan aktif di Lantai 3."                      |
| gagal  -> "Daftar panggilan gagal dimuat."              [Coba lagi]     |
| basi   -> pita "Data terakhir 10.00 — koneksi terputus"                 |
+-------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Kepala | lantai terpilih (besar dan jelas, `NV-DEC-043`), nama pengguna, jam | `GET /patient-calls/floors`; pilihan lantai diingat di PC | `NurseViewPatientCall : Read` | Belum ada lantai tersimpan → `PilihLantai` |
| Daftar | bed, inisial · jenis kelamin · umur, jenis, lama menunggu, hitungan tekan, pengklaim, warna lama menunggu | `GET /patient-calls/active?floorCode=` | `NurseViewPatientCall : Read` | Lihat skema |
| Saya tangani / Ambil alih / Lepas / Tutup | mengikuti `AvailableActions` | `POST /{id}/claim`, `/take-over`, `/release`, `/close` | `NurseViewPatientCall : Update` | Disembunyikan bila tidak berhak. `409` → pesan + muat ulang daftar |
| Tutup panggilan | pilihan alasan wajib; keterangan wajib untuk "Lainnya" | `POST /{id}/close` | `: Update` | `400` → pesan di bawah isian |
| Bunyi | per panggilan baru, nada Darurat berbeda, pengingat, tunda | event `NurseViewPatientCallChanged` + daftar | `: Read` | Browser menolak bunyi → tombol "Aktifkan suara" (`G-14`) |

#### `FE-NVW-04` Papan Lantai di TV — tambahan

```text
+- LANTAI 3 ------------------------------------ 10.01 · Data 10.01 -+
| PANGGILAN: 331-C DARURAT 0:20 | 305-A Sari menuju 1:00 | +3 lainnya |
+--------------------------------------------------------------------+
| (papan seperti revision 1; kartu 331-C dan 305-A diberi penanda)    |
+--------------------------------------------------- Halaman 1 / 3 --+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Strip Panggilan Aktif | nomor bed, jenis, lama menunggu, pengklaim; **tanpa** identitas pasien; tetap di atas, tidak ikut berganti halaman (`NV-DEC-047`) | `GET /display-runtime/active-calls` | policy `NurseViewDisplayRead` | Tidak ada panggilan → strip tidak memakan tempat. Tidak muat → "+k lainnya" (`NV-FE-006`) |
| Penanda kartu | kartu bed yang sedang memanggil | `NvwBedCardResponse.ActiveCallPriority` | idem | — |
| Warna lama menunggu | kuning/merah sesuai ambang | `WaitLevel` | idem | Ambang kosong → tanpa warna (`NV-DEC-048`) |

TV tetap **tanpa suara** (`NV-DEC-013`).

#### `FE-NVW-09` Ambang Lama Menunggu

Bentuknya sama dengan `FE-NVW-03`: dua isian menit (kuning, merah), nomor SK/notulen Komite,
tanggal pengesahan, dan status "Belum dikonfigurasi — panggilan tampil tanpa warna" saat kosong.
Sumber `GET`/`PUT /patient-call-wait-thresholds`; hak akses `NurseViewPatientCallWaitThreshold :
Read`/`Update`. **Tanpa angka bawaan** (`G-15`).

### R2.4 Aksi per peran

Diturunkan dari `contracts/permission-audit-matrix.md` §7. Admin IT mengelola `FE-NVW-05`, `06`,
`09`. Perawat dan kepala ruang memakai `FE-NVW-08`. Pasien dan perawat memakai `FE-NVW-07` lewat
akun tablet. Akun TV hanya `FE-NVW-04`.

### R2.5 Data, realtime, dan pembaruan

| Layar | Muat awal | Realtime | Cadangan |
|---|---|---|---|
| `FE-NVW-04` TV | `GET /board` + `GET /active-calls` | `NurseViewPatientCallChanged` di grup lantai → muat ulang strip segera; papan dijadwalkan lewat jalur yang ada (paling sering 1×/5 detik) untuk penanda kartu | Strip ikut muat ulang papan tiap 60 detik |
| `FE-NVW-07` tablet | `GET /current` | `JoinNurseViewBedTablet` → muat ulang keadaan saat event | Tiap 15 detik selama ada panggilan aktif, tiap 60 detik bila tidak (`NV-FE-009`) |
| `FE-NVW-08` konsol | `GET /active?floorCode=` | `JoinNurseViewCallConsole(floorCode)`; ganti lantai = keluar grup lama, masuk grup baru | Tiap 15 detik (`NV-FE-009`); pita basi bila tidak ada daftar baru > 120 detik |

`src/lib/realtime/queue-realtime-client.js` ditambah konstanta event
`NurseViewPatientCallChanged`, callback `onNurseViewPatientCallChanged`, serta pasangan join/leave
konsol dan tablet yang ikut bergabung ulang saat tersambung kembali. Lama menunggu dihitung di
layar dari `FirstPressedAt` dan selisih jam server (`ServerTimeUtc`), bukan dari jam perangkat.

### R2.6 Bunyi Konsol Panggilan (`NV-DEC-045`)

| Kejadian | Bunyi |
|---|---|
| Panggilan biasa baru | Satu bunyi pendek; berkas `/audio/queue-dingdong.mp3` yang sudah ada dapat dipakai |
| Panggilan Darurat baru, atau naik menjadi Darurat | Nada lain yang jelas berbeda (`DEV_DISCRETION`) |
| Masih ada `Waiting` belum diklaim | Pengingat tiap 15–60 detik (`NV-FE-007`) |
| Semua sudah diklaim | Senyap |
| "Matikan bunyi" | Pengingat ditunda; konsol menampilkan "sampai HH:MM"; panggilan baru tetap berbunyi |
| Browser menolak bunyi | Tombol "Aktifkan suara" (pola layar antrean, `NV-CAP-27`) |

### R2.7 Berkas bersama frontend — menunggu restu lead `NV-OQ-028`

| Berkas | Perubahan |
|---|---|
| `src/proxy.js` | Konstanta `NURSE_VIEW_BED_TABLET_PATH = "/nurse-view-bed-tablet"`; akun bercookie `isNurseViewBedTabletAccount` dialihkan ke sana dan dicegah membuka route lain |
| `src/lib/hooks/auth/use-base-login.jsx` | Mengenali akun tablet bed dan menyetel cookie `isNurseViewBedTabletAccount` |
| `src/components/providers/root-layout-shell.jsx`, `src/components/ui/auth-wrapper.jsx`, `src/components/providers/session-activity-manager.jsx` | Masing-masing satu baris `"/nurse-view-bed-tablet"` pada daftar prefix, persis seperti `/nurse-view-display` |
| `src/lib/realtime/queue-realtime-client.js` | Event dan join/leave baru (R2.5); tidak mengubah perilaku grup lain |
| `src/utils/menu-sidebar/menu-items.jsx` | Tiga butir menu (R2.2) |

### R2.8 Prototipe yang dihapus (`NV-DEC-051`)

`src/lib/helpers/nurseCallContext.jsx` dihapus dalam task frontend Konsol Panggilan. Tidak ada
`import` yang merujuknya (`NV-CAP-29`), sehingga penghapusan tidak memutus layar lain.
Buktinya `NV-AC-043`.

### R2.9 Penanganan keadaan dan kirim ganda

| Keadaan | Perilaku |
|---|---|
| Kirim ganda di tablet | Setiap tekan membuat satu `Idempotency-Key` baru. Kirim ulang otomatis memakai kunci yang **sama**, sehingga server tidak membuat jejak ganda |
| Kirim ganda di konsol | Tombol aksi dikunci selama permintaan berjalan; `409` → pesan + muat ulang daftar |
| Gagal kirim di tablet | `GagalKirim`, kirim ulang otomatis dengan jeda bertambah, pesan bel dinding tetap tampil sampai berhasil |
| Data basi di konsol | Pita abu-abu; daftar lama tetap terlihat |
| Penyimpanan | Konsol hanya menyimpan pilihan lantai di `localStorage` PC (dibungkus try/catch). Daftar panggilan tidak disimpan |
| Admin | Sama dengan revision 1 §6 |

### R2.10 Kewenangan UI

| ID | Area | Keputusan | Status |
|---|---|---|---|
| `NV-FE-006` | Kapasitas strip, wajib "+k lainnya" | Developer dalam batas `NV-DEC-044` | `DEV_DISCRETION` (decision log) |
| `NV-FE-007` | Jeda pengingat 15–60 detik dan lama tunda | Developer dalam batas `NV-DEC-045` | `DEV_DISCRETION` (decision log) |
| `NV-FE-008` | Penempatan butir menu Konsol Panggilan | Usulan desain: grup Rawat Inap | `approved` — grup Rawat Inap (`NV-DEC-055`) |
| `NV-FE-009` | Interval muat ulang cadangan tablet dan konsol (usulan 15 detik saat ada panggilan) | Developer, paling lambat 30 detik | `DEV_DISCRETION` — disahkan `NV-DEC-055` |
| `NV-FE-010` | Ukuran, warna, susunan layar tablet; nada Darurat | Developer, dengan syarat R2.3 `FE-NVW-07` | `DEV_DISCRETION` — disahkan `NV-DEC-055` |
| — | Route `/nurse-view-bed-tablet` dan `/health-services/nurse-view-management/patient-call-console` | Developer, mengikuti `NV-FE-005` | `DEV_DISCRETION` |
| — | Isi tablet tanpa identitas, isi strip, isi konsol, urutan, bunyi | Owner | `approved` (`NV-DEC-038`–`047`; `G-09` disahkan `NV-DEC-055`) |
