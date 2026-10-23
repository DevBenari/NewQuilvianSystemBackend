# MCU (Medical Check-Up) — Brief Layar (`MCU-FE-002`)

| Field | Nilai |
| --- | --- |
| Blueprint | `MCU-BP-001` r1 `approved` |
| Keputusan | `MCU-FE-002` — menu, route, tab/modal, tata letak layar MCU |
| Status | `approved` — Yoga Aji Pratama, 2026-10-21 (empat pilihan struktur di bagian 1) |
| Repository | `QuilvianSystemFrontendDev` (v2), SHA `7fb1e1d3e` |
| Masukan | `03-frontend-architecture.md` r1; `MCU-API-v1`, `MCU-PERM-v1`, `MCU-STATE-v1`, `MCU-VAL-v1`; menu dan layar MCU v1 (`7210f5f4d`) sebagai referensi tampilan (`MCU-DEC-016`) |
| Menutup | Task `FE-MCU-000`; mencabut penahan `MCU-FE-002` pada `FE-MCU-003`..`005`, `007`..`009`, `011`..`014` |

Brief ini menjawab **di mana** setiap pekerjaan MCU dilakukan di layar dan **bagaimana bentuknya**.
Aturan bisnis, kode galat, dan isi data tidak ditetapkan di sini; semuanya mengikuti kontrak `v1`.
Tombol hanya disembunyikan atau dinonaktifkan sebagai bantuan; **server tetap penentu akhir**, dan
pesan `400`/`403`/`409` dari server ditampilkan apa adanya.

## 1. Empat keputusan struktur

| No | Hal | Keputusan | Alternatif yang ditolak |
| ---: | --- | --- | --- |
| 1 | Menu | Grup **Medical Check-Up** ringkas: hanya menu yang berfungsi di MVP | Menampilkan menu v1 *Daftar MCU Perusahaan*, *Upload MCU Pasien*, *Laporan Pasien MCU* bertanda *Fitur belum tersedia* |
| 2 | Detail order | **Halaman bertab** dengan header status dan tombol aksi; isi Pemeriksaan MCU di halaman tersendiri per komponen | Drawer dari daftar; dua tab ala v1 (*Order*, *Hasil*) |
| 3 | Formulir panjang | **Halaman tersendiri** untuk Booking, *Walk-in*, dan Check-in. Modal hanya untuk tindakan pendek | Modal besar |
| 4 | Jadwal & Kuota | **Tabel per tanggal** dalam satu bulan | Kalender bulanan |

**Pengganti menu v1 yang tidak ditampilkan:**

| Menu v1 | Di v2 MVP |
| --- | --- |
| Daftar MCU Perusahaan | Penyaring *Perusahaan* di Daftar Pasien MCU |
| Upload MCU Pasien | Tidak ada — impor rombongan `POST-MVP` (`EPIC MCU-07`); admisi membooking satu per satu |
| Laporan Pasien MCU | Tidak ada — `DEC-MCU-005` belum diputuskan |
| Beranda (*dashboard-mcu* v1) | Kartu ringkasan per status di bagian atas Daftar Pasien MCU |

## 2. Menu sidebar

Berkas: `src/utils/menu-sidebar/menu-items.jsx`. Grup baru diletakkan **sesudah Laboratorium, sebelum
Operasi**, di bagian *Pelayanan Kesehatan*. Setiap butir memakai `requiredPermission` sehingga pengguna
tanpa izin baca tidak melihat menunya.

```text
Pelayanan Kesehatan
├─ …
├─ Radiologi
├─ Laboratorium
├─ Medical Check-Up                         ← grup baru
│  ├─ Daftar Pasien MCU                     McuOrder : Read
│  ├─ Jadwal & Kuota                        McuSchedule : Read
│  └─ Master Data
│     ├─ Paket MCU                          McuPackage : Read
│     ├─ Jenis Pemeriksaan MCU              McuReferenceData : Read
│     └─ Kategori Kelayakan                 McuReferenceData : Read
├─ Operasi
└─ …
```

| Label | `key` | `pathname` | `requiredPermission` |
| --- | --- | --- | --- |
| Medical Check-Up (grup) | `healthServicesMcuManagement` | — | — |
| Daftar Pasien MCU | `healthServicesMcuPatients` | `/health-services/medical-checkup-management/mcu-patients` | `{ resource: "McuOrder", action: "Read" }` |
| Jadwal & Kuota | `healthServicesMcuSchedules` | `/health-services/medical-checkup-management/schedules` | `{ resource: "McuSchedule", action: "Read" }` |
| Master Data (sub-grup) | `healthServicesMcuMasterData` | — | — |
| Paket MCU | `healthServicesMcuPackages` | `/health-services/medical-checkup-management/master-data/packages` | `{ resource: "McuPackage", action: "Read" }` |
| Jenis Pemeriksaan MCU | `healthServicesMcuExaminationTypes` | `/health-services/medical-checkup-management/master-data/examination-types` | `{ resource: "McuReferenceData", action: "Read" }` |
| Kategori Kelayakan | `healthServicesMcuFitnessCategories` | `/health-services/medical-checkup-management/master-data/fitness-categories` | `{ resource: "McuReferenceData", action: "Read" }` |

Ikon menu adalah `DEV_DISCRETION` (`MCU-FE-005`), memakai `react-icons/ri` yang sudah diimpor.

## 3. Peta route

Semua route berada di bawah `/health-services/medical-checkup-management` (disingkat `…`).

| Route | Layar | Bentuk | Izin untuk membuka | Task FE |
| --- | --- | --- | --- | --- |
| `…/mcu-patients` | Daftar Pasien MCU | Halaman | `McuOrder : Read` | `FE-MCU-007` |
| `…/mcu-patients/booking` | Booking MCU | Halaman | `McuOrder : Create` | `FE-MCU-008` |
| `…/mcu-patients/walk-in` | Pendaftaran *Walk-in* | Halaman | `McuOrder : Create` + `McuOrder : CheckIn` | `FE-MCU-009` |
| `…/mcu-patients/[orderId]` | Detail order (5 tab) | Halaman | `McuOrder : Read` | `FE-MCU-007`, `011`..`014` |
| `…/mcu-patients/[orderId]/check-in` | Check-in | Halaman | `McuOrder : CheckIn` | `FE-MCU-009` |
| `…/mcu-patients/[orderId]/components/[componentId]/hasil` | Isi Pemeriksaan MCU | Halaman | `McuExamination : Read` (simpan butuh `Update`) | `FE-MCU-011` |
| `…/schedules` | Jadwal & Kuota | Halaman | `McuSchedule : Read` | `FE-MCU-005` |
| `…/master-data/packages` | Daftar paket | Halaman | `McuPackage : Read` | `FE-MCU-003` |
| `…/master-data/packages/create` | Buat paket | Halaman | `McuPackage : Create` | `FE-MCU-003` |
| `…/master-data/packages/[packageId]` | Ubah paket + harga perusahaan | Halaman | `McuPackage : Read` (simpan butuh `Update`) | `FE-MCU-003` |
| `…/master-data/examination-types` | Daftar jenis Pemeriksaan MCU | Halaman | `McuReferenceData : Read` | `FE-MCU-004` |
| `…/master-data/examination-types/create` | Buat jenis + isian | Halaman | `McuReferenceData : Create` | `FE-MCU-004` |
| `…/master-data/examination-types/[examinationTypeId]` | Ubah jenis + isian | Halaman | `McuReferenceData : Read` (simpan butuh `Update`) | `FE-MCU-004` |
| `…/master-data/fitness-categories` | Kategori kelayakan (daftar + modal) | Halaman | `McuReferenceData : Read` | `FE-MCU-004` |

Segmen `hasil` mengikuti pola Radiologi (`rad-patients/[id]/hasil`). Pembuka route tanpa izin melihat
halaman *akses ditolak* yang sudah dipakai modul lain.

## 4. Alur kerja harian di layar

**Contoh perjalanan Budi** (perorangan, Paket Basic Rp 350.000), dipetakan ke layar:

| No | Pelaku | Di layar | Tindakan | Status order sesudahnya |
| ---: | --- | --- | --- | --- |
| 1 | Admisi | Daftar Pasien MCU → **Booking MCU** | Isi identitas sementara Budi, Paket Basic, 27-10-2026 | *Terjadwal* |
| 2 | Admisi | Daftar Pasien MCU (27 Okt) → aksi **Check-in** | Cocokkan pasien, pilih dr. Rina, tandai *puasa terpenuhi* | *Hadir* |
| 3 | Kasir | Layar Billing yang ada | Terima Rp 350.000 | *Hadir* (lunas) |
| 4 | Petugas MCU | Detail order → tombol **Mulai Pemeriksaan** | Konfirmasi | *Dalam Pemeriksaan* |
| 5 | Perawat | Nurse station yang ada | Selesaikan skrining | — |
| 6 | dr. Rina | Detail → tab **Pemeriksaan** → **Isi Hasil** Pemeriksaan Fisik | Isi lalu *Simpan & Selesaikan* | — |
| 7 | Sistem / petugas | Tab Pemeriksaan → **Muat ulang status komponen** | Lab dan Rad selesai | *Menunggu Resume* |
| 8 | dr. Rina | Tab **Resume** | Isi, lalu **Setujui** | *Disetujui Dokter* |
| 9 | Dewi (petugas) | Tab Resume → **Verifikasi** | Periksa kelengkapan | *Terverifikasi* |
| 10 | Dewi | Tab **Penyerahan** → **Cetak Hasil Lengkap**, lalu **Catat Penyerahan** | WhatsApp 0812xxxx | *Diserahkan* |

## 5. Rincian layar

### 5.1 Daftar Pasien MCU — `…/mcu-patients`

**Tujuan:** satu tempat untuk melihat semua peserta pada suatu tanggal dan membuka tindakan berikutnya.
**Pelaku:** seluruh peran MCU.

```text
Daftar Pasien MCU                                  [+ Booking MCU] [+ Walk-in]
Tanggal [27-10-2026 ▼]
┌─────────┬─────────┬────────┬──────────────┬─────────────┬──────────────┐
│Terjadwal│  Hadir  │Dalam P.│Menunggu Res. │Disetujui Dr.│ Terverifikasi│ …
│   12    │    5    │   8    │      3       │      1      │      2       │
└─────────┴─────────┴────────┴──────────────┴─────────────┴──────────────┘
[Cari nama/No. RM/No. Order] [Status ▼] [Sumber ▼] [Perusahaan ▼] [Dokter MCU ▼]
 No  No. Order          No. RM        Nama Peserta  NIK               Paket        Perusahaan  Dokter   Kelengkapan  Status       ⋮
 1   MCU-RSMMC-000123   00012345      Budi Santoso  3201********0001  Paket Basic  —           dr. Rina 2 dari 4     Dalam Pem.   ⋮
 2   MCU-RSMMC-000124   Belum terdaftar Siti Aminah 3202********0007  Paket Karyawan PT Contoh —         —            Terjadwal    ⋮
```

| Bagian | Isi |
| --- | --- |
| Tanggal | Bawaan hari ini (WIB). Mengubah tanggal memuat ulang ringkasan dan tabel |
| Kartu ringkasan | Sembilan status dari `GET orders/summary?date=`; klik kartu = menyaring status itu. Kartu *Tidak Hadir* dan *Dibatalkan* boleh diletakkan di baris kedua (`DEV_DISCRETION`) |
| Penyaring | Status, Sumber (Booking, Walk-in Admisi, …), Perusahaan, Dokter MCU, Cari — metadata dari `GET orders/filters/metadata` |
| Kolom | No, No. Order, No. RM (atau *Belum terdaftar* bila identitas sementara), Nama Peserta, NIK **dimasking**, Paket MCU, Perusahaan (Penjamin), Dokter MCU, Kelengkapan (*n dari m*), Status, ⋮ Aksi. Label kolom meniru v1 bila maknanya sama |
| Klik baris | Membuka detail order |
| Menu ⋮ Aksi | Menurut status + izin + kecocokan dokter (`FE-MCU-001`): *Terjadwal* hari ini → Check-in; *Terjadwal*/*Tidak Hadir* → Jadwal Ulang; *Terjadwal* → Batal; selebihnya → Buka Detail |
| Kosong | *"Belum ada peserta MCU pada tanggal ini."* |
| Gagal muat | Pesan + tombol *Coba lagi* |

Kolom *Status Pembayaran* dan *Surat Jaminan* v1 **tidak** dimasukkan: tidak ada di respons daftar
`MCU-API-v1`. Sebelum pemeriksaan dimulai, status bayar dilihat petugas di layar Billing; sesudahnya
dasar mulai (*Lunas*/*Terjamin*) tampil di detail order.

### 5.2 Booking MCU — `…/mcu-patients/booking`

**Tujuan:** admisi mencatat rencana MCU dari telepon/WhatsApp. **Hasil:** order *Terjadwal*, tanpa
kunjungan maupun tagihan.

```text
← Daftar Pasien MCU
Booking MCU
1. Peserta     (•) Pasien terdaftar  [Cari No. RM / NIK / nama ▼]
               ( ) Peserta baru      Nama [__________] NIK [________________]
                                     Tgl lahir [__-__-____] No. HP [__________]
2. Penjamin    ( ) Perorangan  (•) Perusahaan [PT Contoh ▼]
3. Paket       [Paket Basic — Rp 300.000 (harga PT Contoh) ▼]
               Persyaratan: Puasa 10–12 jam (Gula Darah Puasa)
4. Tanggal     [27-10-2026]  Sisa kuota: 4 dari 30
                                            [Batal]  [Simpan Booking]
```

| Aturan tampilan | Isi |
| --- | --- |
| Pilihan paket | `GET packages/options?companyGuarantorId&date` — harga yang tampil = harga yang berlaku pada tanggal pemeriksaan |
| Persyaratan | Ditampilkan begitu paket dipilih, agar admisi menyampaikannya ke peserta (`MCU-DEC-013`) |
| Sisa kuota | Dari `GET daily-schedules?from&to`; tanggal tutup tidak dapat dipilih |
| Penolakan | Kuota penuh, order ganda, NIK salah — pesan server tampil di atas tombol, isian tidak hilang |
| Sesudah simpan | Kembali ke Daftar Pasien MCU pada tanggal booking, dengan pesan *"Booking MCU-RSMMC-000123 tersimpan."* |

### 5.3 Pendaftaran *Walk-in* — `…/mcu-patients/walk-in`

Sama dengan Booking bagian 1–3, **ditambah** bagian Check-in 2–4 (dokter MCU, persyaratan, ringkasan
harga) pada halaman yang sama. Tanggal terkunci ke hari ini. Tombol akhir: **Daftarkan & Check-in**.
Hasil: order langsung *Hadir* (`FR-MCU-025`).

### 5.4 Check-in — `…/mcu-patients/[orderId]/check-in`

**Tujuan:** peserta yang hadir secara fisik di-check-in dalam satu langkah. **Hasil:** order *Hadir*,
satu kunjungan MCU, satu baris paket di tagihan.

```text
← Detail MCU-RSMMC-000124
Check-in · Siti Aminah · Paket Karyawan · 27-10-2026
1. Cocokkan pasien
   ⚠ NIK cocok dengan RM 00098765 (Siti Aminah), tetapi tanggal lahir berbeda:
     booking 12-03-1990 · data pasien 21-03-1990
     [ ] Saya sudah memeriksa KTP dan pasien ini orang yang sama
   — atau —  [Buat Pasien Baru] (isian awal dari data booking)
2. Dokter MCU  [dr. Rina (dokter jaga) ▼]
3. Persyaratan
   Puasa 10–12 jam (Gula Darah Puasa)
     ( ) Terpenuhi   ( ) Lanjut dengan catatan [_________]
     ( ) Jadwal ulang komponen ke [__-__-____]
4. Ringkasan harga
   Paket Karyawan — harga PT Contoh (berlaku 1 Okt–31 Des 2026)   Rp 300.000
                                                    [Batal]  [Check-in]
```

| Bagian | Sumber | Aturan tampilan |
| --- | --- | --- |
| Cocokkan pasien | `GET orders/{id}/patient-matches` | Tiga keadaan: cocok penuh (ditautkan otomatis), NIK sama tanggal lahir beda (wajib centang konfirmasi, `MCU-DEC-039`), tidak ada (tombol Buat Pasien Baru membuka formulir pasien Patient Management yang ada, berisi isian awal) |
| Dokter MCU | `GET orders/{id}/check-in/preview` | Bawaan = dokter jaga tanggal itu (`MCU-DEC-042`); boleh diganti |
| Persyaratan | `preview` | Setiap persyaratan wajib dipilih salah satu hasil. *Lanjut dengan catatan* mewajibkan catatan |
| Ringkasan harga | `preview` | Harga dan sumbernya (umum / perusahaan + masa berlaku) |
| Tombol Check-in | `POST orders/{id}/check-in` | Nonaktif selama permintaan berjalan. Kirim ulang aman (server idempoten) |

Check-in hanya dapat dibuka pada tanggal pemeriksaan; di tanggal lain, aksi Check-in tidak tampil
(`VAL-MCU-24`).

### 5.5 Detail order — `…/mcu-patients/[orderId]`

```text
← Daftar Pasien MCU
MCU-RSMMC-000123 · Budi Santoso · RM 00012345 · 3201230101900001
Paket Basic · Perorangan (Lunas) · dr. Rina · 27-10-2026 · Sumber: Booking
[Dalam Pemeriksaan]                         [Aksi utama]  [⋮ Aksi lain]
────────────────────────────────────────────────────────────────────────
 Ringkasan | Pemeriksaan | Resume | Penyerahan | Riwayat
```

**Header** memuat identitas lengkap (NIK utuh boleh di detail berizin), paket, penjamin (*Perorangan*
atau nama perusahaan), dasar mulai (*Lunas*/*Terjamin*) **sesudah** pemeriksaan dimulai, dokter MCU,
tanggal, sumber, dan **satu tombol aksi utama** menurut status:

| Status | Aksi utama | Isi ⋮ Aksi lain (bila berizin) |
| --- | --- | --- |
| *Terjadwal* | Check-in (hari H) | Jadwal Ulang, Batal |
| *Tidak Hadir* | Jadwal Ulang | — |
| *Hadir* | Mulai Pemeriksaan | Batal Penuh, Ganti Dokter MCU |
| *Dalam Pemeriksaan* | Buka tab Pemeriksaan | Batal Penuh (bila belum ada komponen dimulai), Ganti Dokter MCU |
| *Menunggu Resume* | Buka tab Resume | Ganti Dokter MCU |
| *Disetujui Dokter* | Verifikasi (petugas) | Kembalikan ke Dokter, Buka Ulang (dokter), Ganti Dokter MCU |
| *Terverifikasi* | Catat Penyerahan | Buka Ulang (dokter) |
| *Diserahkan* | Catat Penyerahan | Buka Ulang (dokter) |
| *Dibatalkan* | — | — |

**Modal tindakan pendek** (semua mewajibkan alasan bila kontrak mewajibkan):

| Modal | Isian | Endpoint |
| --- | --- | --- |
| Jadwal Ulang | Tanggal baru (dengan sisa kuota) | `PATCH orders/{id}/reschedule` |
| Batal / Batal Penuh | Alasan; untuk Batal Penuh tampil akibatnya: *"Pesanan Lab/Rad dibatalkan dan Rp 350.000 dikembalikan."* | `PATCH orders/{id}/cancel` |
| Mulai Pemeriksaan | Konfirmasi; bila belum lunas, pesan server tampil apa adanya | `POST orders/{id}/start-examination` |
| Ganti Dokter MCU | Dokter pengganti, alasan | `PATCH orders/{id}/mcu-doctor` |

**Tab-tab:**

| Tab | Isi |
| --- | --- |
| **Ringkasan** | Data peserta, penjamin, paket dan harga terpakai, hasil penandaan persyaratan saat check-in beserta catatannya, kunjungan terkait |
| **Pemeriksaan** | Lihat 5.6 |
| **Resume** | Lihat 5.7 |
| **Penyerahan** | Lihat 5.8 |
| **Riwayat** | Tabel dari `GET orders/{id}/history`: waktu, dari status → ke status, pelaku, alasan; termasuk penggantian dokter |

Tab yang belum relevan tetap tampil tetapi berisi keterangan, mis. tab Resume pada order *Hadir*:
*"Resume dapat disusun sesudah semua komponen tuntas."*

### 5.6 Tab Pemeriksaan dan halaman Isi Hasil

```text
Kelengkapan: 2 dari 4 tuntas            [Muat ulang status komponen] [+ Tambah Pemeriksaan]
 Komponen            Jenis   Cakupan       Status        Pesanan          Aksi
 Darah Lengkap       Lab     Paket         Selesai       LAB-RSMMC-…
 Gula Darah Puasa    Lab     Paket         Dijadwal ulang (28-10)          ⋮
 Thorax PA           Rad     Paket         Dipesan       RAD-RSMMC-…      ⋮ Batal
 Pemeriksaan Fisik   MCU     Paket         Siap diisi    —                [Isi Hasil →]
 Kolesterol Total    Lab     Di luar paket Dipesan       LAB-RSMMC-…      ⋮
```

| Elemen | Aturan |
| --- | --- |
| Label status komponen | `Pending` *Menunggu*, `Ordered` *Dipesan*, `ReadyToFill` *Siap diisi*, `Rescheduled` *Dijadwal ulang (tanggal)*, `Completed` *Selesai*, `Cancelled` *Dibatalkan* |
| Muat ulang status komponen | `POST components/refresh`; tidak ada polling otomatis |
| Tambah Pemeriksaan | Modal (dokter MCU yang ditetapkan): jenis komponen, prosedur/jenis pemeriksaan, modalitas bila Rad. Keterangan: *"Ditagih terpisah, di luar harga paket."* |
| Batal komponen | Modal (dokter MCU yang ditetapkan): alasan; tampil nilai pengembalian, mis. *"Rp 150.000 akan dikembalikan."* |
| Jadwal ulang komponen | Modal: tanggal baru; keterangan *"Tidak memakai kuota."* |
| Isi Hasil | Hanya untuk komponen *Pemeriksaan MCU* yang *Siap diisi* atau sudah terisi |

**Halaman Isi Pemeriksaan MCU** — `…/mcu-patients/[orderId]/components/[componentId]/hasil`:

```text
← Detail MCU-RSMMC-000123 · Pemeriksaan
Pemeriksaan Fisik · Budi Santoso · dr. Rina
 1. Tekanan darah sistolik *   [120   ] mmHg
 2. Tekanan darah diastolik *  [80    ] mmHg
 3. Berat badan *              [64.5  ] kg
 4. Buta warna                 [Normal ▼]
 5. Kesan umum                 [_____________________________]
Kesimpulan                     [_____________________________]
                         [Simpan Draf]  [Simpan & Selesaikan]
```

| Aturan | Isi |
| --- | --- |
| Urutan isian | Menurut `FieldNumber`; nomor butir ditampilkan |
| Jenis isian | Teks → isian teks; Angka → isian desimal `DECIMAL_FIELD_PROPS`/`parseDecimal` (`6,4` dan `6.4` → `6.4`); Pilihan → `BaseSelectField`; Ya/Tidak → dua pilihan |
| Wajib | Bertanda `*`; diperiksa server saat *Simpan & Selesaikan* |
| Skrining belum selesai | Halaman tetap terbuka baca-saja dengan pesan server *"Skrining awal peserta belum selesai."* |
| Sesudah approve | Seluruh isian baca-saja; pesan *"Hasil terkunci. Minta dokter membuka ulang resume."* |

### 5.7 Tab Resume

```text
Versi 2 · Draf                                     Riwayat versi: [v1 · Dikoreksi ▼]
Kategori kelayakan *    [Fit dengan catatan ▼]
Kondisi umum *          [__________________________________]
Temuan penting          [__________________________________]
Rekomendasi medis *     [__________________________________]
Rekomendasi kerja       [__________________________________]  ← satu-satunya teks
                                                                 untuk perusahaan
                         [Simpan Draf]  [Setujui]        (dokter MCU yang ditetapkan)
─────────────────────────────────────────────────────────
Disetujui dr. Rina · 28-10-2026 10.00      [Kembalikan ke Dokter] [Verifikasi]  (petugas)
```

| Aturan | Isi |
| --- | --- |
| Siapa mengisi | Hanya dokter MCU yang ditetapkan; pengguna lain melihat baca-saja |
| Setujui | Ditolak server bila komponen belum tuntas atau skrining belum selesai — pesannya menyebut komponen, mis. *"Masih ada 1 komponen yang belum tuntas: Thorax PA."* |
| Verifikasi/Kembalikan | Untuk petugas MCU; tidak ada isian yang dapat diubah. Kembalikan memakai modal alasan |
| Buka Ulang | Modal alasan (dokter); versi lama tetap dapat dipilih di *Riwayat versi* bertanda *Dikoreksi* |
| Label *Rekomendasi kerja* | Diberi keterangan bahwa hanya teks ini yang sampai ke perusahaan tanpa persetujuan peserta |

### 5.8 Tab Penyerahan

```text
Cetak                [Cetak Hasil Lengkap]  [Cetak Ringkasan Kelayakan]
Persetujuan pelepasan ke PT Contoh: Belum ada      [Catat Persetujuan]
Riwayat penyerahan                                  [Catat Penyerahan]
 Waktu            Penerima    Isi        Saluran   Tujuan        Versi  Petugas  Pemberitahuan koreksi
 28-10 10.25      Peserta     Lengkap    WhatsApp  0812xxxx      v1     Dewi     ⚠ Perlu [Catat]
 28-10 10.30      PT Contoh   Ringkasan  Email     hrd@…         v1     Dewi     ⚠ Perlu [Catat]
```

| Elemen | Aturan |
| --- | --- |
| Cetak | Data dari `GET result-document?variant=full` / `fitness-summary`; tata cetak memakai pola dokumen cetak Lab yang ada. Ringkasan **tidak** pernah disusun di browser dari data lengkap (`MCU-FE-003`) |
| Catat Penyerahan | Modal: penerima (Peserta/Perusahaan), isi (Lengkap/Ringkasan), saluran (Ambil langsung, WhatsApp, Email, Telepon), tujuan (wajib kecuali ambil langsung), catatan. Isi *Lengkap* ke perusahaan tanpa persetujuan → pesan server, diarahkan ke Ringkasan |
| Catat/Cabut Persetujuan | Modal: jenis bukti (formulir kertas / pernyataan elektronik), nomor formulir; cabut wajib alasan. Bentuk bukti final menunggu `MCU-OQ-003` (go-live) |
| Pemberitahuan koreksi | Baris bertanda *Perlu* muncul sesudah resume dibuka ulang; tombol *Catat* membuka modal waktu + catatan |
| Tombol Kirim otomatis | **Tidak ada** di MVP (`MCU-DEC-027`) |

### 5.9 Jadwal & Kuota — `…/schedules`

```text
Jadwal & Kuota          Bulan [Oktober 2026 ▼]          [Atur Rentang Tanggal]
 Tanggal     Buka   Kuota  Terpakai  Pelampauan  Dokter Jaga   Aksi
 Sen 27-10   Ya     30     34        +5          dr. Rina      [Setujui Pelampauan]
 Sel 28-10   Ya     30     4         —           —             [Setujui Pelampauan]
 Rab 29-10   Tutup  —      —         —           —
```

| Elemen | Aturan |
| --- | --- |
| Atur Rentang Tanggal | Modal (`McuSchedule : Update`): dari–sampai, buka/tutup, kuota, dokter jaga. Kuota di bawah terpakai → pesan server |
| Setujui Pelampauan | Modal (`McuSchedule : ApproveQuotaOverride`, kepala unit): tambahan kuota + alasan wajib |
| Tanggal tanpa jadwal | Tampil *Tutup* |
| Dokter jaga kosong | Tampil *—*; check-in admisi tetap bisa memilih dokter, kiosk kelak menolak (`VAL-MCU-47`) |

### 5.10 Master Data

| Layar | Bentuk | Isi utama |
| --- | --- | --- |
| Daftar paket | Tabel: kode, nama, harga umum, jumlah komponen, aktif; tombol *+ Buat Paket*; aksi aktif/nonaktif | `GET packages` |
| Buat/Ubah paket | Halaman bersekat: **Data paket** (kode, nama, tarif paket umum); **Komponen** (tabel yang dapat ditambah: jenis, prosedur/jenis pemeriksaan, modalitas, nilai pengembalian, nilai jasa, persyaratan); **Total nilai pengembalian** ditampilkan berjalan, mis. *Rp 290.000 dari Rp 350.000*; **Harga perusahaan** (hanya di ubah paket: tabel + modal tambah + aksi *Akhiri*) | `POST/PUT packages`, `company-prices` |
| Daftar jenis Pemeriksaan MCU | Tabel: kode, nama, jumlah isian, aktif | `GET examination-types` |
| Buat/Ubah jenis | Halaman: data jenis + tabel isian (nomor butir, kode, label, jenis, satuan, pilihan, wajib) | `POST/PUT examination-types` |
| Kategori kelayakan | Tabel + modal buat/ubah (kode, nama, aktif) | `fitness-categories` |

Nilai master usulan (kategori *Fit*/*Fit dengan catatan*/*Unfit sementara*/*Unfit*, jenis Pemeriksaan
Fisik dan Tanda Vital) diberi keterangan *usulan* sampai diratifikasi dokter MCU (`MCU-OQ-002`/`003`).

## 6. Perilaku bersama

| Keadaan | Perilaku |
| --- | --- |
| Memuat | Indikator muat pada tabel dan tombol |
| Kiriman ganda | Tombol aksi nonaktif selama permintaan berjalan |
| `409` *diubah petugas lain* | Detail order dimuat ulang otomatis, lalu pesan ditampilkan |
| `403` | Pesan server apa adanya; tombol tidak disembunyikan diam-diam sesudah gagal |
| Privasi | NIK dimasking di daftar; hasil, resume, dan tujuan penyerahan hanya di detail berizin |
| Layar | Dapat dipakai pada lebar 1366 px; tabel lebar boleh bergulir mendatar di dalam kartunya |
| Aksesibilitas | Label pada setiap isian; pesan galat terbaca pembaca layar; formulir Isi Hasil dapat dinavigasi papan ketik |

## 7. Rentang yang diizinkan (`DEV_DISCRETION`)

Boleh diputuskan pengembang tanpa menanyakan pemilik, **selama** struktur di bagian 1–5 tidak berubah:
warna dan ikon (dari satu peta konstanta status), jarak dan lebar kolom, susunan kartu ringkasan dalam
satu atau dua baris, teks bantuan singkat, bentuk indikator muat, dan pilihan komponen UI yang sudah
ada (`BaseSelectField`/`FilterSelect`, tabel standar, modal standar).

**Bukan** `DEV_DISCRETION` — butuh revisi brief: menambah/menghapus menu, mengubah route, memindahkan
tindakan dari halaman ke modal atau sebaliknya, menambah tab, dan menampilkan data yang tidak ada di
kontrak.
