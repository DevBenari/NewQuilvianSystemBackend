# Arsitektur Frontend — Nurse View

| Field | Nilai |
|---|---|
| Blueprint ID | `nurse-view` · revision `1` · `approved` — Rizki Gunawan, 2026-10-28 (`NV-DEC-028`) |
| Owner | Rizki Gunawan |
| Frontend SHA | `76b0c6e4b` (branch `RizkiV2`) |
| Masukan | `00-interview-decisions.md` rev `2`, `contracts/api-contract.md` `0.1.0` |
| Konvensi | Pakai ulang komponen dan gaya yang sudah ada; `globals.css` tidak disentuh |

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

## 8. Kewenangan UI

| Area | Keputusan | Status |
|---|---|---|
| Isi kartu, urutan kritis, pengelompokan per bangsal, ganti halaman, keterbacaan ±3 m, tanpa suara | Owner | `approved` (`NV-FE-001`–`003`) |
| Warna, ikon, jarak, durasi halaman, tata letak header | Developer | `DEV_DISCRETION` (`NV-FE-004`) |
| Route halaman TV `/nurse-view-display` | Developer, mengikuti pola `/queue-display` | `DEV_DISCRETION` (`NV-FE-005`) |
| Penempatan menu di Administrator › Master Data | Usulan desain, mengikuti "Display Antrian" | Menunggu approval blueprint |
| Bentuk konfirmasi nonaktif (modal atau inline) | Developer | `DEV_DISCRETION` |
