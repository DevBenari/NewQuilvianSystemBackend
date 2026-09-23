# Hemodialisa — Arsitektur Frontend

| Field | Value |
|---|---|
| Blueprint ID | `HMD-BP-001` |
| Contract version | `HMD-CONTRACT-v1` — `last_changed_in: HMD-CONTRACT-v1`, status `approved` |
| Owner | Muhammad Hamzah |
| `approved_by` / `approved_at` | Muhammad Hamzah / 2026-09-18T15:40:07+07:00 |
| Frontend SHA | `a38683142` — branch `HamzahV2` |
| `input_revision` | `contracts/api-contract.md` r1; `contracts/permission-audit-matrix.md` r1 |

Dokumen ini mengunci **keterjangkauan dan sumber data**, bukan rupa. Warna, jarak, ikon, urutan
butir menu, dan pustaka komponen tetap `DEV_DISCRETION`.

---

## 1. Urutan Kewenangan yang Berlaku

```text
keamanan, privasi, dan invariant
  → brief produk yang disetujui
  → konvensi dan design system project
  → DEV_DISCRETION
```

| Hal | Kewenangan | Sumbernya |
|---|---|---|
| Layar mana yang mendapat butir menu | Mengikat | Dokumen ini |
| Route yang dituju setiap butir | Mengikat | Dokumen ini |
| Sumber data tiap bagian layar | Mengikat | `contracts/api-contract.md` |
| Hak akses yang menjaga tiap tombol | Mengikat | `contracts/permission-audit-matrix.md` |
| Status serologi tidak tampil di layar yang dilihat banyak orang | **Mengikat, alasan privasi** | `contracts/permission-audit-matrix.md` bagian 7 |
| Penempatan modul di bawah Pelayanan Kesehatan | Mengikat | Keputusan produk pada PRD masukan |
| Nama butir menu, urutannya, dan ikonnya | `DEV_DISCRETION` | — |
| Tab, modal, atau laci geser | `DEV_DISCRETION` | — |
| Warna, jarak, bentuk kontrol, pustaka komponen | `DEV_DISCRETION` | — |

---

## 2. Kebutuhan Layar

| ID | Layar | Jenis | Slice asal |
|---|---|---|---|
| `FE-HMD-01` | Beranda Hemodialisa | Ringkasan | — |
| `FE-HMD-02` | Permintaan HD Masuk | Daftar kerja | `S1` |
| `FE-HMD-03` | Daftar Pasien Hemodialisa | Daftar | `S2` |
| `FE-HMD-04` | Jadwal dan Daftar Kerja | Daftar kerja | `S5` |
| `FE-HMD-05` | Kesiapan Unit | Lembar kerja | `S6` |
| `FE-HMD-06` | Ruang Kerja Episode | Layar kerja per pasien — **layar anak** | `S2`, `S3`, `S4` |
| `FE-HMD-07` | Ruang Kerja Sesi | Layar kerja per sesi — **layar anak** | `S7` s/d `S12` |
| `FE-HMD-08` | Master Mesin Hemodialisa | Master | `S6` |
| `FE-HMD-09` | Master Station Hemodialisa | Master | `S6` |
| `FE-HMD-10` | Master Butir Persiapan | Master | `S8` |
| `FE-HMD-11` | Pengaturan Unit Hemodialisa | Master | `S5`, `S6`, `S7` |
| `FE-HMD-12` | Formulir Permintaan HD | **Layar anak milik Rawat Inap** | `S1` |

---

## 3. Peta Butir Menu

### 3.1 Bentuk pohon

```text
Pelayanan Kesehatan
└── Hemodialisa                          <- tingkat 0, anaknya di field `subMenu`
    ├── Beranda Hemodialisa              -> /health-services/hemodialysis-management
    ├── Permintaan Masuk                 -> .../orders
    ├── Daftar Pasien Hemodialisa        -> .../patients
    ├── Jadwal & Daftar Kerja            -> .../worklist
    ├── Kesiapan Unit                    -> .../unit-readiness
    └── Master Data                      <- grup tingkat 1, anaknya di field `subItems`
        ├── Mesin Hemodialisa            -> .../master-data/machines
        ├── Station Hemodialisa          -> .../master-data/stations
        ├── Butir Persiapan              -> .../master-data/checklist-items
        └── Pengaturan Unit              -> .../master-data/settings
```

Nama field bukan pilihan bebas. Resolver sidebar hanya membaca `item.subMenu` pada tingkat 0 dan
`subItems` pada grup di bawahnya. Berkas yang disunting saat implementasi adalah
`src/utils/menu-sidebar/menu-items.jsx`.

Kunci menu bersarang `menuHemodialisa` **sudah dipesan** di
`src/components/features/left-sidebar/left-sidebar-menu-handle.jsx:9@a38683142`, tetapi belum ada
butir menu di baliknya.

### 3.2 Tabel butir menu

| Butir menu | Tingkat | Induk | `pathname` | Layar | Butir hak akses | Status |
|---|:---:|---|---|---|---|---|
| Hemodialisa | 0 | Pelayanan Kesehatan | — | — | — | Baru |
| Beranda Hemodialisa | 1 | Hemodialisa | `/health-services/hemodialysis-management` | `FE-HMD-01` | `HemodialysisSchedule : Read` | Baru |
| Permintaan Masuk | 1 | Hemodialisa | `.../orders` | `FE-HMD-02` | `HemodialysisOrder : Read` | Baru |
| Daftar Pasien Hemodialisa | 1 | Hemodialisa | `.../patients` | `FE-HMD-03` | `HemodialysisEpisode : Read` | Baru |
| Jadwal & Daftar Kerja | 1 | Hemodialisa | `.../worklist` | `FE-HMD-04` | `HemodialysisSchedule : Read` | Baru |
| Kesiapan Unit | 1 | Hemodialisa | `.../unit-readiness` | `FE-HMD-05` | `HemodialysisUnitReadiness : Read` | Baru |
| Master Data | 1 | Hemodialisa | — | — | — | Baru |
| Mesin Hemodialisa | 2 | Master Data | `.../master-data/machines` | `FE-HMD-08` | `HemodialysisMachine : Read` | Baru |
| Station Hemodialisa | 2 | Master Data | `.../master-data/stations` | `FE-HMD-09` | `HemodialysisStation : Read` | Baru |
| Butir Persiapan | 2 | Master Data | `.../master-data/checklist-items` | `FE-HMD-10` | `HemodialysisChecklistItem : Read` | Baru |
| Pengaturan Unit | 2 | Master Data | `.../master-data/settings` | `FE-HMD-11` | `HemodialysisSetting : Read` | Baru |

### 3.3 Layar anak yang sengaja tidak mendapat butir menu

| Layar | Jalan masuknya |
|---|---|
| `FE-HMD-06` Ruang Kerja Episode | Dari `FE-HMD-03` Daftar Pasien, lewat tombol Detail pada satu baris pasien. Route: `.../patients/{patientId}/episodes/{episodeId}` |
| `FE-HMD-07` Ruang Kerja Sesi | Dari `FE-HMD-04` Jadwal dan Daftar Kerja, lewat tombol Buka pada satu baris sesi. Juga dari `FE-HMD-06` pada bagian Riwayat Sesi. Route: `.../sessions/{sessionId}` |
| `FE-HMD-12` Formulir Permintaan HD | Dari layar layanan penunjang milik Rawat Inap — **dua tempat**: ruang kerja dokter dan ruang kerja perawat. Keduanya sudah ada dan sengaja dikosongkan sampai modul ini tersedia |

**Aturan yang mengikat:** setiap layar pada bagian 2 muncul tepat sekali sebagai butir menu,
**atau** tercantum pada tabel di atas beserta jalan masuknya. Layar yang tidak memenuhi salah
satunya dihitung **belum selesai** walaupun kodenya sudah ada dan lulus uji.

Pendaftaran butir menu wajib menjadi salah satu kriteria penerimaan pada task layar, atau task
tersendiri bila tidak masuk task mana pun.

---

## 4. Skema Fitur per Layar

### 4.1 `FE-HMD-04` — Jadwal dan Daftar Kerja

Layar yang paling sering dibuka sepanjang hari.

```text
+- Jadwal & Daftar Kerja Hemodialisa ----------------------- FE-HMD-04 -+
| [Tanggal 18-09-2026]  [Shift v]  [Status v]  [cari nama/nomor RM]    |
+-----------------------------------------------------------------------+
| Kesiapan unit shift ini: SIAP                    [Lihat kesiapan]     |
+-----------------------------------------------------------------------+
| Jam  | Pasien | Station | Mesin | Isolasi | Perawat | Status |        |
| 0700 | ...    | HD-01   | M-01  | —       | ...     | chip   | [Buka] |
| 0800 | ...    | HD-02   | M-03  | perlu   | ...     | chip   | [Buka] |
+-----------------------------------------------------------------------+
| memuat -> kerangka baris, bukan layar kosong                          |
| kosong -> "Belum ada sesi terjadwal pada tanggal dan shift ini."      |
| gagal  -> "Daftar kerja gagal dimuat."            [Coba lagi]         |
+- Halaman 1 dari n --------------------- [< Sebelumnya] [Berikutnya >] +
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Saringan | Tanggal, shift, status, pencarian pasien | — | — | — |
| Pita kesiapan unit | Status kesiapan unit pada tanggal dan shift terpilih | `GET /hemodialysis-unit-readiness` | `HemodialysisUnitReadiness : Read` | Gagal → pita menampilkan "Status kesiapan tidak dapat dimuat"; daftar tetap tampil |
| Tabel daftar kerja | Jam, pasien, station, mesin, penanda isolasi, perawat, status sesi | `GET /hemodialysis-sessions/worklist` | `HemodialysisSchedule : Read` | Kosong → "Belum ada sesi terjadwal pada tanggal dan shift ini." Gagal → "Daftar kerja gagal dimuat." beserta tombol coba lagi |
| Tombol Buka | Membuka ruang kerja sesi | — | `HemodialysisSession : Read` | Tombol yang tidak berhak **disembunyikan**, bukan ditampilkan lalu ditolak |
| Tombol Jadwalkan sesi | Membuka formulir penjadwalan | — | `HemodialysisSchedule : Create` | idem |

**Penanda isolasi menampilkan "perlu" atau "—" saja.** Ia **tidak** menyebut jenis infeksinya.
Layar ini dilihat banyak orang sekaligus di ruang terbuka, dan status serologi adalah informasi
paling sensitif di modul ini.

### 4.2 `FE-HMD-07` — Ruang Kerja Sesi

Layar tempat perawat bekerja sepanjang sesi berlangsung.

```text
+- Sesi HD-SES-2026-000481 - Ibu Sinta (samaran) ----------- FE-HMD-07 -+
| InProgress  |  Mesin M-03  |  Station HD-02  |  mulai 07.12          |
| Dokter penanggung jawab: dr. Rahmat (samaran)                        |
| Alergi: — | Akses: fistula lengan kiri | Isolasi: perlu              |
| [Catat pemantauan] [Catat obat] [Catat kejadian] [Hentikan] [Selesai] |
+---------------+-------------------------------------------------------+
| - Pra-HD      |                                                       |
| - Intra-HD    |            isi bagian terpilih                        |
| - Pasca-HD    |                                                       |
| - Ringkasan   |                                                       |
| - Finalisasi  |                                                       |
+---------------+-------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Kepala konteks | Nomor sesi, nama pasien, status, mesin, station, waktu mulai, dokter penanggung jawab, alergi, akses vaskular, penanda isolasi | `GET /hemodialysis-sessions/{id}` | `HemodialysisSession : Read` | Gagal → **seluruh layar** diganti pesan "Konteks sesi Hemodialisa tidak dapat diverifikasi." beserta tombol coba lagi. Tidak ada satu pun data klinis yang ditampilkan |
| Bagian Pra-HD | Dua belas butir persiapan dan penilaian sebelum tindakan | `GET /hemodialysis-sessions/{id}/checklist`, `GET /hemodialysis-sessions/{id}` | `HemodialysisSession : Read` | Kosong → "Butir persiapan belum diisi." |
| Tombol Lewati butir | Melewati satu butir persiapan dengan alasan | — | `HemodialysisSession : OverrideChecklist` | Tombol disembunyikan bila butirnya tidak boleh dilewati **maupun** bila pengguna tidak berhak |
| Tombol Nyatakan siap | Memindahkan sesi ke keadaan siap | — | `HemodialysisSession : DeclareReady` | Tombol nonaktif bila masih ada butir wajib yang belum terpenuhi, dengan penjelasan bagian mana |
| Tombol Mulai | Memulai cuci darah | — | `HemodialysisSession : Start` | **Wajib dikunci setelah ditekan sekali** sampai jawaban server tiba. Permintaan membawa penanda idempotency |
| Bagian Intra-HD | Riwayat pemantauan, pemberian obat, dan kejadian | `GET /hemodialysis-sessions/{id}/observations` | `HemodialysisObservation : Read` | Kosong → "Belum ada pemantauan pada sesi ini." |
| Bagian Pasca-HD | Penilaian setelah tindakan dan tujuan pasien | `GET /hemodialysis-sessions/{id}` | `HemodialysisSession : Read` | Kosong → "Penilaian setelah tindakan belum diisi." |
| Bagian Finalisasi | Ringkasan kelengkapan, nama penyelesai dokumentasi, dan tombol pengesahan | `GET /hemodialysis-sessions/{id}` | `HemodialysisRecord : Finalize` untuk tombol Sahkan | Tombol Sahkan **disembunyikan** dari perawat |
| Pita status penagihan | Status penyerahan tindakan ke penagihan | `GET /hemodialysis-sessions/{id}/billing-handoff` | `HemodialysisSession : Read` | Gagal → pita menampilkan status tidak diketahui; **tidak** mengubah tampilan status sesi |

**Catatan penting tentang pita status penagihan:** ia ditampilkan terpisah dari status sesi.
Sesi yang sudah disahkan tetap terbaca sudah disahkan walaupun penagihannya gagal. Menggabungkan
keduanya dalam satu penanda akan membuat petugas mengira catatan klinisnya bermasalah.

### 4.3 `FE-HMD-02` — Permintaan HD Masuk

```text
+- Permintaan Hemodialisa Masuk ---------------------------- FE-HMD-02 -+
| [Status v] [Prioritas v] [Tanggal v] [cari nama/nomor RM]            |
+-----------------------------------------------------------------------+
| Waktu | Pasien | Asal | Prioritas | Alasan singkat | Status |        |
| ...   | ...    | ...  | chip      | ...            | chip   | [Buka] |
+-----------------------------------------------------------------------+
| kosong -> "Belum ada permintaan pada saringan ini."   [Atur ulang]    |
| gagal  -> "Daftar permintaan gagal dimuat."           [Coba lagi]     |
+-----------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Tabel permintaan | Waktu, pasien, unit asal, prioritas, alasan singkat, status | `GET /hemodialysis-orders` | `HemodialysisOrder : Read` | Kosong → "Belum ada permintaan pada saringan ini." |
| Tombol Terima | Menerima permintaan | — | `HemodialysisOrder : Accept` | Disembunyikan dari peran yang tidak berhak |
| Tombol Tahan | Menahan permintaan beserta alasan | — | `HemodialysisOrder : Hold` | idem |
| Tombol Tolak | Menolak permintaan beserta alasan klinis | — | `HemodialysisOrder : Reject` | **Disembunyikan dari koordinator.** Hanya dokter |
| Tombol Jadwalkan | Membuka formulir penjadwalan dari permintaan yang sudah diterima | — | `HemodialysisSchedule : Create` | Muncul hanya pada permintaan berstatus diterima |

### 4.4 `FE-HMD-05` — Kesiapan Unit

```text
+- Kesiapan Unit Hemodialisa ------------------------------- FE-HMD-05 -+
| [Tanggal 18-09-2026] [Shift v]        Status unit: DRAFT             |
+-----------------------------------------------------------------------+
| Butir                    | Hasil       | Tanggal hasil | Pemeriksa    |
| Mesin siap               | [pilihan]   | —             | ...          |
| Station siap             | [pilihan]   | —             | ...          |
| Pengolahan air           | [pilihan]   | [tanggal]     | ...          |
| Obat dan BMHP            | [pilihan]   | —             | ...          |
| Ketersediaan staf        | [pilihan]   | —             | ...          |
+-----------------------------------------------------------------------+
| [Simpan pemeriksaan]        [Nyatakan siap]   [Nyatakan tidak siap]   |
+-----------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Daftar butir | Butir pemeriksaan beserta hasil, tanggal hasil, dan pemeriksanya | `GET /hemodialysis-unit-readiness/{id}` | `HemodialysisUnitReadiness : Read` | Kosong → "Lembar pemeriksaan untuk tanggal dan shift ini belum dibuat." beserta tombol membuat |
| Tombol Simpan pemeriksaan | Menyimpan hasil tiap butir | — | `HemodialysisUnitReadiness : Update` | — |
| Tombol Nyatakan siap | Menyatakan unit siap | — | `HemodialysisUnitReadiness : DeclareReady` | Nonaktif bila masih ada butir wajib yang belum terpenuhi, dengan penjelasan butir mana |
| Tombol Nyatakan tidak siap | Menyatakan unit tidak siap beserta alasan | — | `HemodialysisUnitReadiness : DeclareNotReady` | — |

### 4.5 Layar yang berbagi bentuk

Enam layar berikut memakai bentuk **layar daftar** yang sama seperti `FE-HMD-02`, sehingga tidak
digambar ulang. Yang berbeda hanya kolom dan sumber datanya.

| Layar | Kolom utama | Sumber data | Butir hak akses |
|---|---|---|---|
| `FE-HMD-03` Daftar Pasien | Nomor RM, pasien, nomor episode, status episode, status resep, akses vaskular | `GET /hemodialysis-episodes` | `HemodialysisEpisode : Read` |
| `FE-HMD-08` Master Mesin | Kode, nama, unit, status, mesin khusus | `GET /master-data/hemodialysis-machines` | `HemodialysisMachine : Read` |
| `FE-HMD-09` Master Station | Kode, nama, unit, ruang, status, station isolasi | `GET /master-data/hemodialysis-stations` | `HemodialysisStation : Read` |
| `FE-HMD-10` Master Butir Persiapan | Kode, nama butir, kategori, wajib, boleh dilewati | `GET /master-data/hemodialysis-checklist-items` | `HemodialysisChecklistItem : Read` |
| `FE-HMD-11` Pengaturan Unit | Bentuk formulir, bukan daftar | `GET /master-data/hemodialysis-settings/{serviceUnitId}` | `HemodialysisSetting : Read` |
| `FE-HMD-01` Beranda | Kartu ringkasan: sesi hari ini, menunggu, berjalan, selesai, tertahan; ditambah daftar perlu perhatian | `GET /hemodialysis-sessions/worklist`, `GET /hemodialysis-unit-readiness` | `HemodialysisSchedule : Read` |

**Beranda bukan sumber kebenaran baru.** Seluruh angkanya berasal dari daftar kerja dan kesiapan
unit; tidak ada endpoint ringkasan tersendiri, dan tidak ada angka yang dihitung di layar dari
data yang tidak ditampilkan.

### 4.6 `FE-HMD-10` — kolom yang menentukan

Layar master butir persiapan punya satu kolom yang lebih penting dari yang terlihat: **boleh
dilewati**.

| Wilayah | Isi | Sumber data | Butir hak akses | Catatan |
|---|---|---|---|---|
| Kolom Boleh dilewati | Penanda ya atau tidak per butir | `GET /master-data/hemodialysis-checklist-items` | `HemodialysisChecklistItem : Read` | Seluruhnya bernilai **tidak** sampai badan klinis menetapkan |
| Tombol Ubah boleh dilewati | Mengubah penanda itu | — | `HemodialysisChecklistItem : SetOverridable` | **Disembunyikan dari hampir semua peran.** Hanya pemegang akun tata kelola klinis |

Layar inilah yang membuat kebijakan pelewatan butir persiapan dapat diubah **tanpa menyentuh
kode**.

---

## 5. Aksi per Peran

Diturunkan dari `contracts/permission-audit-matrix.md`, tidak dikarang ulang.

| Peran kerja | Layar yang dibuka | Yang dapat dilakukannya |
|---|---|---|
| Dokter atau perawat unit peminta | `FE-HMD-12` | Membuat permintaan HD, membatalkan permintaannya sendiri |
| Petugas administrasi HD | `FE-HMD-03`, `FE-HMD-06` | Membuka dan menutup program HD pasien |
| Koordinator unit HD | `FE-HMD-01`, `FE-HMD-02`, `FE-HMD-04`, `FE-HMD-05`, `FE-HMD-08`, `FE-HMD-09` | Menerima dan menahan permintaan, menjadwalkan sesi, menugaskan petugas, memeriksa kesiapan unit, mengelola mesin dan station |
| Dokter dialisis dan DPJP | `FE-HMD-02`, `FE-HMD-06`, `FE-HMD-07` | Menolak permintaan, menilai kelayakan, membuat dan mengaktifkan resep, memutuskan isolasi, melewati butir persiapan, mengesahkan catatan sesi |
| Perawat dialisis | `FE-HMD-04`, `FE-HMD-07` | Seluruh pekerjaan sepanjang sesi, sampai menyelesaikan dokumentasi |
| Tim PPI | `FE-HMD-06` | Meninjau serologi dan memutuskan kebutuhan isolasi |
| Pemegang akun tata kelola klinis | `FE-HMD-10`, `FE-HMD-11` | Menetapkan butir yang boleh dilewati dan mengubah pengaturan unit |
| Auditor berwenang | Seluruh layar | Hanya membaca |

**Aturan yang mengikat:** tombol yang tidak berhak **disembunyikan**, bukan ditampilkan lalu
ditolak server. Tombol yang muncul lalu menghasilkan penolakan hak akses adalah cacat yang sudah
terbaca sejak desain.

---

## 6. Penanganan Keadaan

### 6.1 Empat keadaan wajib pada setiap daftar

| Keadaan | Yang ditampilkan |
|---|---|
| Memuat | Kerangka baris, bukan layar kosong dan bukan pemutar tunggal di tengah layar |
| Kosong | Kalimat yang menyebut saringan yang sedang aktif, beserta tombol mengatur ulang saringan |
| Gagal | Kalimat gagal beserta tombol coba lagi. Data lama **tidak** ditampilkan seolah-olah masih berlaku |
| Berisi | Data |

### 6.2 Konteks klinis yang gagal diverifikasi

Ini aturan paling keras di sisi frontend, dan berlaku pada `FE-HMD-06` dan `FE-HMD-07`:

```text
Bila konteks pasien atau sesi gagal diverifikasi:

  tidak menampilkan data klinis pasien;
  tidak menyediakan satu pun tombol yang menulis data;
  tidak memakai data yang sudah terlanjur dimuat sebelumnya.

  "Konteks sesi Hemodialisa tidak dapat diverifikasi."
  [Coba lagi]
```

### 6.3 Data basi saat berpindah pasien

Ketika pengguna berpindah dari ruang kerja pasien A ke pasien B, data pasien A **tidak boleh**
tetap tampil selama data pasien B dimuat. Layar dikosongkan lebih dulu, lalu diisi.

Aturan ini terdengar sepele dan justru paling berbahaya bila dilanggar: perawat bisa mencatat
pemantauan pasien B ke dalam sesi pasien A.

### 6.4 Pengiriman ganda

| Tindakan | Perlakuan |
|---|---|
| Tombol Mulai | Dikunci setelah ditekan sekali sampai jawaban server tiba. Permintaan membawa penanda idempotency; bila jaringan lambat dan pengguna menekan lagi, server mengembalikan sesi yang sama |
| Tombol Sahkan | Dikunci setelah ditekan sekali |
| Tombol Simpan pada formulir mana pun | Dikunci selama pengiriman berlangsung |
| Tombol Jalankan ulang penyerahan tagihan | Dikunci; pengulangan berkali-kali tidak pernah menghasilkan tagihan ganda |

### 6.5 Penyegaran data setelah tindakan

| Setelah | Yang disegarkan |
|---|---|
| Permintaan diterima, ditahan, atau ditolak | Daftar permintaan dan beranda |
| Sesi dijadwalkan atau dijadwalkan ulang | Daftar kerja dan beranda |
| Sesi dimulai, dihentikan, atau diselesaikan | Ruang kerja sesi, daftar kerja, dan beranda |
| Catatan sesi disahkan | Ruang kerja sesi, daftar kerja, beranda, dan riwayat sesi pada ruang kerja episode |
| Status mesin diubah | Master mesin, daftar kerja, dan kesiapan unit |
| Kesiapan unit dinyatakan | Kesiapan unit, daftar kerja, dan beranda |

---

## 7. Struktur Berkas Frontend

Mengikuti pola yang sudah dipakai modul Radiologi, yang terbukti lengkap pada `a38683142`.

```text
src/app/health-services/hemodialysis-management/          # Baru
├── page.jsx                                              # FE-HMD-01
├── orders/                                               # FE-HMD-02
├── patients/                                             # FE-HMD-03
│   └── [patientId]/episodes/[episodeId]/                 # FE-HMD-06
├── worklist/                                             # FE-HMD-04
├── sessions/[sessionId]/                                 # FE-HMD-07
├── unit-readiness/                                       # FE-HMD-05
└── master-data/
    ├── machines/                                         # FE-HMD-08
    ├── stations/                                         # FE-HMD-09
    ├── checklist-items/                                  # FE-HMD-10
    └── settings/                                         # FE-HMD-11

src/components/view/health-services/hemodialysis-management/   # Baru
src/lib/services/health-services/hemodialysis-management/      # Baru, satu service per grup endpoint
src/lib/hooks/health-services/hemodialysis-management/         # Baru, termasuk aturan transisi status
src/lib/constants/health-services/hemodialysis-management/     # Baru
src/lib/state/slice/health-services/hemodialysis-management/   # Baru

# Berkas milik modul lain yang ikut berubah
src/utils/menu-sidebar/menu-items.jsx                                      # Diperbarui
src/lib/constants/health-services/inpatient-management/
    └── inpatient-supporting-service-constants.jsx                          # Diperbarui — kartu Hemodialisa menjadi tersedia
src/lib/constants/health-services/inpatient-management/
    └── inpatient-nursing-constants.js                                      # Diperbarui — butir Hemodialisa menjadi tersedia
```

### Komponen yang dipakai ulang, bukan dibuat baru

Impact scan 18 September 2026 menemukan folder kerangka layar klinis sudah memuat 21 komponen.
Tujuh di antaranya menjawab kebutuhan Hemodialisa secara langsung dan **wajib** dipakai ulang:

| Komponen | Dipakai pada |
|---|---|
| `ClinicalWorkspaceShell`, `ClinicalSectionNav`, `ClinicalPageHeader` | Kerangka `FE-HMD-06` dan `FE-HMD-07` |
| `PatientContextHeader` | Kepala konteks pasien pada kedua ruang kerja |
| `ClinicalStateBoundary` | Keempat keadaan pada seluruh daftar |
| `ClinicalCompletionBar` | Kemajuan pengisian butir persiapan pada `FE-HMD-07` |
| `ClinicalTimeline`, `ClinicalTimelineItem` | Riwayat pemantauan pada bagian Intra-HD |
| `ClinicalSafetyAlert` | Penanda komplikasi dan butir persiapan yang belum terpenuhi |
| `ClinicalValidationSummary` | Penjelasan mengapa tombol Nyatakan siap atau Sahkan masih nonaktif |
| `ClinicalAddendumList`, `ClinicalAddendumItem` | Daftar koreksi pada bagian Ringkasan `FE-HMD-07` |
| `ClinicalRevisionHistory`, `ClinicalRevisionItem` | Riwayat penggantian resep pada `FE-HMD-06` |

Membuat komponen baru yang menduplikasi salah satu dari daftar di atas dihitung sebagai
penyimpangan desain, bukan pilihan gaya.

---

## 8. Titik Sentuh dengan Rawat Inap

Dua layar Rawat Inap sudah menyediakan tempat untuk Hemodialisa dan sengaja dikosongkan:

| Layar Rawat Inap | Berkas | Keadaan sekarang | Yang berubah |
|---|---|---|---|
| Layanan penunjang pada ruang kerja dokter | `.../physician-workspace/tabs/supporting-service/` | Kartu Hemodialisa bertanda belum tersedia | Kartu menjadi tersedia dan membuka `FE-HMD-12` |
| Penunjang pada ruang kerja perawat | `.../nursing-workspace/sections/ancillary/nursing-ancillary-section.jsx` | Butir Hemodialisa masuk daftar belum tersedia | Butir menjadi tersedia dan membuka `FE-HMD-12` |

**Yang perlu diputuskan saat implementasi:** kedua layar itu menyiratkan perawat bangsal juga
membuat permintaan penunjang. Siapa yang sah membuat permintaan HD adalah keputusan pemilik,
bukan kesimpulan dari susunan layar. Sampai diputuskan, kedua layar memakai butir hak akses yang
sama — `HemodialysisOrder : Create` — dan pemberiannya kepada peran mana diatur admin.

---

## 9. Kewenangan UI yang Didelegasikan

| Keputusan | Status | Catatan |
|---|---|---|
| Nama butir menu dan urutannya | `DEV_DISCRETION` | Selama route dan hak aksesnya sesuai tabel bagian 3.2 |
| Ikon setiap butir menu | `DEV_DISCRETION` | — |
| Bentuk navigasi di dalam ruang kerja — tab, laci geser, atau daftar samping | `DEV_DISCRETION` | Yang mengikat hanya: kepala konteks tetap terlihat saat berpindah bagian |
| Bentuk kontrol pengisian butir persiapan | `DEV_DISCRETION` | — |
| Warna penanda status | `DEV_DISCRETION` | Yang mengikat: penanda isolasi **tidak** menyebut jenis infeksinya |
| Bentuk grafik atau ringkasan pada beranda | `DEV_DISCRETION` | Yang mengikat: tidak ada angka yang dihitung dari data yang tidak ditampilkan |
| Susunan kolom pada layar daftar | `DEV_DISCRETION` | — |
