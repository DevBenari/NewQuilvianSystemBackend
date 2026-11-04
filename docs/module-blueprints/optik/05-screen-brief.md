# Optik — Brief Layar (`FE-OPT-000`)

| Field | Nilai |
| --- | --- |
| Blueprint | `OPT-BP-001` r1 `approved`; decision log r8 |
| Keputusan | `OPT-FE-001` (menu, daftar layar), `OPT-FE-002` (route, daftar/detail, tab/modal) |
| Status | `approved` — Yoga Aji Pratama, 2026-11-04 (struktur `OPT-DEC-061`..`075` dan seluruh rincian brief ini; menutup `OPT-OQ-018`) |
| Revisi | `r2` 2026-11-04 — pratinjau harga tampil di detail pesanan `Draft` sesudah Simpan Draf, bukan di form saat memilih barang (`OPT-DEC-076`, disetujui Yoga Aji Pratama) |
| Repository | `QuilvianSystemFrontendDev` (v2) `2bb756de7` (branch `YogaV2`); backend `d8fa6e40` + `BE-OPT-003`..`053` di working tree |
| Masukan | `03-frontend-architecture.md` r1; `OPT-API-v1`, `OPT-PERM-v1`, `OPT-STATE-v1`, `OPT-VAL-v1`; brief MCU (`mcu/05-screen-brief.md`) sebagai acuan bentuk |
| Menutup | Task `FE-OPT-000`; mencabut penahan `FE-OPT-010`..`032` (kecuali bagian yang menunggu `OPT-API-v1.1`, lihat bagian 8) |

Brief ini menjawab **di mana** setiap pekerjaan Optik dilakukan di layar dan **bagaimana bentuknya**.
Aturan bisnis, kode galat, dan isi data tidak ditetapkan di sini; semuanya mengikuti kontrak `v1`.
Tombol hanya disembunyikan atau dinonaktifkan sebagai bantuan; **server tetap penentu akhir**, dan
pesan `400`/`403`/`409`/`503` dari server ditampilkan apa adanya.

## 1. Keputusan struktur

| No | Hal | Keputusan | Alternatif yang ditolak | Dasar |
| ---: | --- | --- | --- | --- |
| 1 | Menu | Grup **Optik** ringkas; hanya menu yang berfungsi di MVP | Satu pintu lewat kunjungan; menu v1 bertanda *Fitur belum tersedia* | `OPT-DEC-061` |
| 2 | Kunjungan | **Halaman detail bertab** | Satu halaman panjang; drawer | `OPT-DEC-062` |
| 3 | Form panjang | **Halaman tersendiri** untuk pemeriksaan, resep, pesanan, pendaftaran, keluhan | Modal besar; isi langsung di tab | `OPT-DEC-063`, `065` |
| 4 | Pesanan | **Halaman detail pesanan** sendiri; tindakan pendek lewat **modal** | Hanya di tab kunjungan; Serahkan di halaman sendiri | `OPT-DEC-064`, `066` |
| 5 | Kepala Optik | **Penyaring di Daftar Pesanan** | Menu Persetujuan tersendiri | `OPT-DEC-067` |
| 6 | Keluhan | **Satu alur dari pesanan** `Diserahkan` | Dua langkah manual | `OPT-DEC-068` |
| 7 | Penjualan langsung | Dari **tab Pesanan** kunjungan, form pesanan yang sama | Menu Penjualan Langsung | `OPT-DEC-069` |
| 8 | Laporan | **Satu halaman, 4 tab** | Empat sub-menu | `OPT-DEC-070` |
| 9 | Resep dari pemeriksaan | **Terisi otomatis**, boleh diubah sebelum simpan | Form kosong | `OPT-DEC-071` |
| 10 | Riwayat versi | **Panel di halaman detail** + bandingkan dua versi | Modal riwayat | `OPT-DEC-072` |
| 11 | Cetak | **Cetak Resep** dan **Cetak Bukti Pesanan** dari layar | Hanya resep; tanpa cetak | `OPT-DEC-073` |
| 12 | Master Data | Katalog: daftar + **modal** profil; Pengaturan: **satu halaman** + riwayat | Profil di halaman sendiri | `OPT-DEC-074` |
| 13 | Harga sebelum ditagihkan | **Pratinjau harga** dari backend untuk pesanan *Draft* tersimpan, di detail pesanan dan halaman Ubah | Harga sesudah ditagihkan; harga saat memilih barang (butuh ubah kode Billing); baca master tarif dari layar | `OPT-DEC-075`, `076` |

Sisa lama `src/utils/config.jsx` ("Pendaftaran Pasien Optik"), kunci `menuOptik`, dan guard `/Optik`
**tidak dipakai** (`03-frontend-architecture.md` bagian 6). Perapiannya bukan bagian brief ini.

## 2. Menu sidebar

Berkas: `src/utils/menu-sidebar/menu-items.jsx`. Grup baru diletakkan **sesudah Medical Check-Up,
sebelum Operasi**, di bagian *Pelayanan Kesehatan*. Setiap butir memakai `requiredPermission`, sehingga
pengguna tanpa izin baca tidak melihat menunya.

```text
Pelayanan Kesehatan
├─ …
├─ Laboratorium
├─ Medical Check-Up
├─ Optik                                   ← grup baru
│  ├─ Kunjungan Optik                      OptVisit : Read
│  ├─ Pesanan Optik                        OptOrder : Read
│  ├─ Keluhan & Garansi                    OptWarrantyClaim : Read
│  ├─ Laporan Optik                        OptReport : Read
│  └─ Master Data
│     ├─ Katalog Barang Optik              OptReferenceData : Read
│     └─ Pengaturan Optik                  OptReferenceData : Read
├─ Operasi
└─ …
```

| Label | `key` | `pathname` | `requiredPermission` |
| --- | --- | --- | --- |
| Optik (grup) | `healthServicesOpticalManagement` | — | — |
| Kunjungan Optik | `healthServicesOptVisits` | `/health-services/optical-management/visits` | `{ resource: "OptVisit", action: "Read" }` |
| Pesanan Optik | `healthServicesOptOrders` | `/health-services/optical-management/orders` | `{ resource: "OptOrder", action: "Read" }` |
| Keluhan & Garansi | `healthServicesOptWarrantyClaims` | `/health-services/optical-management/warranty-claims` | `{ resource: "OptWarrantyClaim", action: "Read" }` |
| Laporan Optik | `healthServicesOptReports` | `/health-services/optical-management/reports` | `{ resource: "OptReport", action: "Read" }` |
| Master Data (sub-grup) | `healthServicesOptMasterData` | — | — |
| Katalog Barang Optik | `healthServicesOptItemCatalog` | `/health-services/optical-management/master-data/item-catalog` | `{ resource: "OptReferenceData", action: "Read" }` |
| Pengaturan Optik | `healthServicesOptSettings` | `/health-services/optical-management/master-data/settings` | `{ resource: "OptReferenceData", action: "Read" }` |

Ikon menu adalah `DEV_DISCRETION` (`OPT-FE-004`).

## 3. Peta route

Semua route berada di bawah `/health-services/optical-management` (disingkat `…`). Halaman detail
memakai id-nya sendiri, karena resep dipakai lintas kunjungan dan pesanan dibuka dari menu.

| Route | Layar | Bentuk | Izin untuk membuka | Task FE |
| --- | --- | --- | --- | --- |
| `…/visits` | Daftar Kunjungan Optik | Halaman | `OptVisit : Read` | `FE-OPT-010` |
| `…/visits/register` | Pendaftaran Kunjungan Optik (`?patientId=&claimOrderId=` untuk alur keluhan) | Halaman | `OptVisit : Create` | `FE-OPT-010`, `030` |
| `…/visits/[encounterId]` | Detail kunjungan (5 tab) | Halaman | `OptVisit : Read` | `FE-OPT-010` |
| `…/refraction-exams/[examId]` | Pemeriksaan refraksi: isi (draf) atau baca (final) + riwayat revisi | Halaman | `OptRefractionExam : Read` (isi butuh `Update`) | `FE-OPT-011` |
| `…/refraction-exams/[examId]/revise` | Revisi hasil ukur sesudah final | Halaman | `OptRefractionExam : Revise` | `FE-OPT-011` |
| `…/visits/[encounterId]/prescriptions/new` | Buat resep (`?examId=` = dari pemeriksaan; `?source=external` = salin resep luar) | Halaman | `OptPrescription : Create` | `FE-OPT-012` |
| `…/prescriptions/[prescriptionId]` | Detail resep + riwayat versi + cetak | Halaman | `OptPrescription : Read` | `FE-OPT-012` |
| `…/prescriptions/[prescriptionId]/revise` | Revisi resep yang belum terkunci | Halaman | `OptPrescription : Revise` | `FE-OPT-012` |
| `…/orders` | Daftar Pesanan Optik | Halaman | `OptOrder : Read` | `FE-OPT-021` |
| `…/visits/[encounterId]/orders/new` | Buat pesanan (`?kind=fabrication` atau `?kind=direct`) | Halaman | `OptOrder : Create` | `FE-OPT-020`, `022` |
| `…/orders/[orderId]` | Detail pesanan + modal aksi + cetak bukti | Halaman | `OptOrder : Read` | `FE-OPT-020`, `021`, `022` |
| `…/orders/[orderId]/edit` | Ubah barang selama `Draft` belum ditagihkan | Halaman | `OptOrder : Update` | `FE-OPT-020` |
| `…/warranty-claims` | Daftar Keluhan & Garansi | Halaman | `OptWarrantyClaim : Read` | `FE-OPT-030` |
| `…/visits/[encounterId]/warranty-claims/new` | Catat keluhan (`?orderId=` wajib) | Halaman | `OptWarrantyClaim : Create` | `FE-OPT-030` |
| `…/warranty-claims/[claimId]` | Detail keluhan + modal Periksa Ulang/Putuskan | Halaman | `OptWarrantyClaim : Read` | `FE-OPT-030` |
| `…/reports` | Laporan Optik (4 tab) | Halaman | `OptReport : Read` | `FE-OPT-032` |
| `…/master-data/item-catalog` | Katalog Barang Optik + modal profil | Halaman | `OptReferenceData : Read` (simpan butuh `Update`) | `FE-OPT-031` |
| `…/master-data/settings` | Pengaturan Optik + riwayat | Halaman | `OptReferenceData : Read` (simpan butuh `UpdateSetting`) | `FE-OPT-031` |

Pembuka route tanpa izin melihat halaman *akses ditolak* yang sudah dipakai modul lain
(`access-denied-gate`).

## 4. Alur kerja harian di layar

**Contoh perjalanan Ibu Sari** (rujukan Poli Mata, kacamata frame Rp 450.000 + lensa Rp 600.000, tarif
pemeriksaan Rp 75.000), dipetakan ke layar:

| No | Pelaku | Di layar | Tindakan | Status sesudahnya |
| ---: | --- | --- | --- | --- |
| 1 | Admisi | Kunjungan Optik → **Pendaftaran** | Cari Ibu Sari, cara bayar Tunai, rujukan dari ENC-…-0100 (dr. Bima) | Kunjungan terbuka |
| 2 | Rina (RO) | Detail kunjungan → tab **Pemeriksaan** → **Mulai Pemeriksaan** | Isi visus, autoref, koreksi subjektif, PD; **Simpan Final** | Pemeriksaan *Final*; tarif masuk tagihan |
| 3 | Rina | Halaman pemeriksaan → **Buat Resep** | Form resep terisi dari koreksi subjektif; periksa lalu simpan; **Cetak Resep** | Resep v1 |
| 4 | Dedi (penjualan) | Tab **Pesanan** → **Pesanan Kacamata/Lensa** | Pilih resep berlaku, frame dan lensa dari katalog; **Simpan Draf**, lalu lihat pratinjau harga Rp 1.050.000 di detail pesanan | Pesanan *Draft* |
| 5 | Dedi | Detail pesanan → **Tagihkan** | Konfirmasi | *Menunggu Uang Muka* |
| 6 | Kasir | Layar Billing yang ada | Terima uang muka Rp 525.000 | — |
| 7 | Dedi | Detail pesanan → **Muat ulang status bayar** → **Kirim ke Pengerjaan** | Bengkel sendiri, perkiraan jadi 5 November; **Cetak Bukti Pesanan** | *Dalam Pengerjaan*; resep terkunci |
| 8 | Dedi | Pesanan Optik → detail → **Tandai Siap** | Konfirmasi | *Siap Diambil* |
| 9 | Kasir | Layar Billing | Terima pelunasan Rp 600.000 | — |
| 10 | Dedi | Detail pesanan → **Serahkan** | Centang identitas; diambil Dewi (anak); catatan *fitting* | *Diserahkan*; kunjungan tertutup bila tak ada pesanan terbuka |
| 11 | Dedi | Detail pesanan → **Catat Keluhan** (hari ke-10) | Pendaftaran kunjungan baru (pasien terisi) → form keluhan "pusing saat membaca" | Keluhan *Diajukan* |
| 12 | Rina | Kunjungan keluhan → tab Pemeriksaan; lalu detail keluhan → **Periksa Ulang** | Pilih pemeriksaan ulang, isi temuan | *Diperiksa* |
| 13 | Kepala Optik | Detail keluhan → **Putuskan** | Setujui remake | *Disetujui*; pesanan remake dibuat |

## 5. Rincian layar

### 5.1 Daftar Kunjungan Optik — `…/visits`

**Tujuan:** melihat kunjungan hari ini atau yang masih terbuka, lalu membuka kunjungan. **Pelaku:** semua peran Optik.

```text
Kunjungan Optik                                              [+ Daftarkan Kunjungan]
Tanggal [04-11-2026 ▼]  [ ] Semua yang masih terbuka  [ ] Ada pesanan terbuka  [Cari nama/No. RM/No. Kunjungan]
 No  No. Kunjungan   Tanggal      No. RM    Nama Pasien  Cara Bayar  Rujukan                  Periksa Resep Pesanan  Status
 1   ENC-…-0101      04-11 09.05  00012345  Sari Wulan   Tunai       ENC-…-0100 · dr. Bima    1       1     1        Terbuka
 2   ENC-…-0102      04-11 09.40  00045678  Joko Susilo  Tunai       —                        0       0     0        Terbuka
```

| Bagian | Isi |
| --- | --- |
| Penyaring | Tanggal (bawaan hari ini WIB) → `Date`; *Semua yang masih terbuka* → `OpenOnly=true` dan tanggal diabaikan; *Ada pesanan terbuka* → `HasOpenOrder=true`; Cari → `Search` |
| Kolom | No, No. Kunjungan, Tanggal/jam, No. RM, Nama Pasien, Cara Bayar, Rujukan (nomor kunjungan asal + dokter perujuk, atau —), Periksa, Resep, Pesanan Terbuka (jumlah), Status Kunjungan |
| Klik baris | Membuka detail kunjungan |
| Kosong | *"Belum ada kunjungan Optik pada tanggal ini."* |

### 5.2 Pendaftaran Kunjungan Optik — `…/visits/register`

```text
← Kunjungan Optik
Daftarkan Kunjungan Optik
1. Pasien        [Cari No. RM / NIK / nama ▼]          (terisi dan terkunci pada alur keluhan)
2. Cara bayar    (•) Tunai  ( ) Asuransi [▼]  ( ) Penjamin Perusahaan [▼]
3. Rujukan       [ ] Rujukan Poli Mata
                    Kunjungan asal [ENC-…-0100 · 03-11-2026 · Poli Mata · dr. Bima ▼]
4. Catatan       [______________________]
   (alur keluhan) Keluhan atas pesanan OPT-ORD-RSMMC-000001 — sesudah simpan, lanjut ke form keluhan
                                                   [Batal]  [Daftarkan]
```

| Aturan tampilan | Isi |
| --- | --- |
| Cari pasien | Komponen pencarian pasien yang sudah ada (pola pencarian pasien Lab); pasien baru dibuat di Patient Management, bukan di sini |
| Penjamin | Pilihan asuransi/penjamin pasien dari endpoint penjamin yang sudah dipakai pendaftaran lain (`/admin/options` berizin, bukan `/options` kiosk) |
| Kunjungan asal | Kunjungan rawat jalan pasien yang sama dari `patient-encounters/admin/options?patientId&encounterType=Outpatient&onlyActive=false` (`PatientEncounter : Read`). Pasien lain atau kunjungan non-rawat-jalan ditolak server (`OPT-REG-REJECTED`) |
| Sesudah simpan | Ke detail kunjungan baru. Pada alur keluhan (`claimOrderId`) langsung ke `…/visits/[encounterId]/warranty-claims/new?orderId=` |
| Kirim ganda | Tombol nonaktif selama permintaan; server mengenali pengulangan (`IsReplay`) |

### 5.3 Detail kunjungan — `…/visits/[encounterId]`

```text
← Kunjungan Optik
ENC-…-0101 · Sari Wulan · RM 00012345 · Tunai · 04-11-2026 09.05
Rujukan: ENC-…-0100 · dr. Bima (Sp.M)                                  [Terbuka]
────────────────────────────────────────────────────────────────────────
 Ringkasan | Pemeriksaan | Resep | Pesanan | Keluhan
```

| Tab | Isi | Tombol (bila berizin) |
| --- | --- | --- |
| **Ringkasan** | Data kunjungan dan rujukan; ringkasan empat daftar (nomor, status, waktu) dari `GET visits/{encounterId}` | — |
| **Pemeriksaan** | Daftar pemeriksaan kunjungan: nomor, pemeriksa, status, waktu final; klik → halaman pemeriksaan | **Mulai Pemeriksaan** (`OptRefractionExam : Create`) → `POST refraction-exams`, lalu ke halamannya |
| **Resep** | Resep kunjungan ini, ditambah bagian *Resep berlaku pasien* (`GET prescriptions?patientId&validOnly`): nomor, jenis, asal (internal/luar), versi, berlaku sampai, terkunci | **Buat Resep dari Pemeriksaan** (ada pemeriksaan final), **Salin Resep Luar** (`OptPrescription : Create`) |
| **Pesanan** | Pesanan kunjungan: nomor, jenis, status, jumlah barang, perkiraan jadi; klik → detail pesanan | **Pesanan Kacamata/Lensa**, **Penjualan Langsung** (`OptOrder : Create`) |
| **Keluhan** | Keluhan yang dicatat pada kunjungan ini; klik → detail keluhan | — (keluhan dimulai dari pesanan, 5.9) |

Tab yang kosong berisi keterangan dan aksi sah berikutnya, mis. *"Belum ada pemeriksaan pada kunjungan
ini."* Kunjungan yang sudah tertutup tetap dapat dibaca; tombol pembuat disembunyikan dan server tetap
penentu.

### 5.4 Pemeriksaan refraksi — `…/refraction-exams/[examId]`

```text
← ENC-…-0101 · Pemeriksaan
OPT-EXM-RSMMC-000001 · Sari Wulan · Rina (RO) · SIP 123/…                         [Draf]
Visus        Jauh tanpa koreksi  OD [6/21 ]  OS [6/24 ]   dengan koreksi OD [6/6 ] OS [6/6 ]
             Dekat …
Autoref      OD SPH [-2,50] CYL [-0,50] AXIS [180]   OS …
Koreksi      OD SPH [-2,75] CYL [     ] AXIS [   ] ADD [    ]   OS SPH [-3,25] …
subjektif
PD           Jauh [63,5] Dekat [    ]
                              [Batalkan Pemeriksaan]  [Simpan Draf]  [Simpan Final]
──────────── Riwayat revisi ────────────  (tampil sesudah final; lihat 5.6)
```

| Keadaan | Perilaku |
| --- | --- |
| *Draf* | Isian aktif; **Simpan Draf** (`PUT measurements`), **Simpan Final** (`POST finalize`, modal konfirmasi: *"Tarif pemeriksaan akan ditagihkan."*), **Batalkan Pemeriksaan** (modal alasan) |
| *Final* | Baca-saja; tombol **Revisi** (→ `…/revise`, isian + alasan wajib, tanpa tagih ulang) dan **Buat Resep** (→ `…/visits/[encounterId]/prescriptions/new?examId=`) |
| *Dibatalkan* | Baca-saja dengan alasan batal |
| Tagihan gagal tercatat | `BillingMessage` tampil sebagai peringatan kuning; pemeriksaan tetap final (server mencoba ulang) |
| Isian | SPH/CYL/ADD/PD memakai `DECIMAL_FIELD_PROPS`/`parseDecimal` dari `FE-OPT-001` (tanda minus bisa diketik, `-1,50` tetap `-1,50`); visus teks bebas berpola `6/12`, `1/~`, `0` dengan pesan server bila salah |
| Galat STR/SIP | `403` server tampil apa adanya, mis. *"STR Anda tidak berlaku sejak 31-10-2026."* |

### 5.5 Resep — buat, detail, revisi

**Buat** — `…/visits/[encounterId]/prescriptions/new`:

| Mode | Isi |
| --- | --- |
| Dari pemeriksaan (`?examId=`) | Jenis resep (kacamata/lensa kontak); isian **terisi** dari koreksi subjektif + PD pemeriksaan (`OPT-DEC-071`), dapat diubah. Keterangan: *"Nilai diambil dari OPT-EXM-RSMMC-000001; periksa sebelum menyimpan."* |
| Resep luar (`?source=external`) | Penulis asli, fasilitas asal, tanggal resep, lalu isian. Bila CYL plus, tampil **pratinjau transposisi** dan teks aslinya sebelum simpan (`FE-OPT-001`, `OPT-DEC-045`) |
| Lensa kontak | Power, CYL, AXIS, BC, DIA per mata; merk/jenis; masa pakai. Rentang BC/DIA dari Pengaturan |
| Di luar rentang | Isian ditandai. RO: tombol simpan ditolak server. Sp.M: muncul isian *Alasan melewati rentang* |

**Detail** — `…/prescriptions/[prescriptionId]`:

```text
OPT-RX-RSMMC-000001 · Kacamata · Internal · terbit 04-11-2026 · berlaku s.d. 04-11-2027   🔒 Terkunci (OPT-ORD-RSMMC-000001)
Versi 2 (terkini) · Budi (RO) · 04-11 09.20 · alasan "salah ketik"
      SPH    CYL    AXIS  ADD     PD jauh 63,5
 OD  -2,75  -0,75   180    —
 OS  -3,25    —      —     —                       [Revisi]  [Cetak Resep]
──── Riwayat versi ───────────────────────────────
 v2 Budi 04-11 09.20 "salah ketik"   [✓]
 v1 Rina 04-11 09.10                 [✓]   [Bandingkan]
```

| Elemen | Aturan |
| --- | --- |
| Terkunci | Tanda kunci + nomor pesanan pengunci; tombol **Revisi** disembunyikan (server menolak `409` bila tetap dicoba) |
| Revisi | `…/revise`: isian versi terkini + alasan wajib + `ExpectedVersion` |
| Kedaluwarsa | Label *Kedaluwarsa* bila `IsExpired`; resep tidak muncul di pilihan pesanan |
| Cetak Resep | Kartu resep: identitas pasien, jenis, nilai versi terkini, PD, nama dan nomor izin pemeriksa (atau penulis resep luar), tanggal terbit dan berlaku. Disusun dari `GET prescriptions/{id}`; pola cetak `print-resep` yang ada |

### 5.6 Panel riwayat versi (resep dan pemeriksaan)

Daftar versi (nomor, pelaku, waktu, alasan). Memilih dua versi lalu **Bandingkan** menampilkan dua kolom
berdampingan; nilai yang berbeda ditandai. Versi lama tidak dapat diubah. Data dari daftar `Versions`
/ `Revisions` pada respons detail (tanpa endpoint tambahan).

### 5.7 Form pesanan — `…/visits/[encounterId]/orders/new` dan `…/orders/[orderId]/edit`

```text
← ENC-…-0101 · Pesanan
Pesanan Kacamata/Lensa · Sari Wulan
1. Resep        [OPT-RX-RSMMC-000001 v1 · OD -2,75 / OS -3,25 · berlaku s.d. 04-11-2027 ▼]
                ⓘ Resep akan terkunci saat pesanan dikirim ke pengerjaan
2. Barang       [Kategori ▼] [Cari merk/warna/ukuran] 
                 Kode          Nama                  Kategori  Stok depo  Jml
                 FRM-A-HTM-52  Frame A Hitam 52-18   Frame     19         [1] [Hapus]
                 LNS-AR-156    Lensa AR 1.56         Lensa     12         [1] [Stok ▼]
                 ⓘ Perkiraan harga tampil di detail pesanan sesudah Simpan Draf
                                                        [Batal]  [Simpan Draf]
```

| Aturan | Isi |
| --- | --- |
| Mode | `?kind=fabrication`: resep wajib (kacamata berukuran atau lensa kontak). `?kind=direct`: tanpa bagian resep, kecuali lensa kontak (resep lensa kontak berlaku wajib); lensa kacamata tidak bisa dipilih (`OPT-DEC-020`, `046`) |
| Pilihan resep | `GET prescriptions/options?patientId&prescriptionType` — hanya versi terkini dan belum kedaluwarsa |
| Katalog | `GET reference-data/item-catalog` dengan penyaring kategori/warna/ukuran/indeks; stok tersedia di depo Optik ditampilkan |
| Pasokan lensa | Untuk lensa kacamata: *Stok* atau *Pesan ke lab* (`LensSupply`) |
| Harga | Form tidak menampilkan harga. Pratinjau tampil di detail pesanan `Draft` dan di halaman Ubah sesudah disimpan (`OPT-DEC-076`) |
| Simpan | Menyimpan `Draft`, lalu ke detail pesanan. Penolakan server (resep kedaluwarsa, barang tak bertarif, lensa kontak dan lensa kacamata dalam satu pesanan) tampil di atas tombol, isian tidak hilang |

### 5.8 Detail pesanan — `…/orders/[orderId]`

```text
← Pesanan Optik
OPT-ORD-RSMMC-000001 · Pesanan Kacamata · Sari Wulan · RM 00012345 · ENC-…-0101
Resep OPT-RX-RSMMC-000001 v1 🔒 · Bengkel sendiri · perkiraan jadi 05-11-2026        [Dalam Pengerjaan]
                                               [Aksi utama]  [⋮ Aksi lain]
⚠ Pengajuan batal menunggu keputusan Kepala Optik                [Putuskan]   (bila ada)
──────────────────────────────────────────────────────────────────────────
Barang            Kategori  Pasokan  Jml  Status baris     Harga (tagihan)  Diskon
Frame A Hitam 52  Frame     —        1    Dalam pengerjaan 450.000          —
Lensa AR 1.56     Lensa     Stok     1    Dalam pengerjaan 600.000          —
Status bayar (tagihan INV-…-0042)                          [Muat ulang status bayar]
 Bagian pasien pesanan  Rp 1.050.000    Minimum uang muka 50% = Rp 525.000
 Terbayar (tagihan)     Rp   525.000    Sisa tagihan        Rp 525.000
 Uang muka ✓ terpenuhi                  Lunas ✗
Riwayat status  ·  Pengajuan batal  ·  Garansi (sesudah diserahkan)
```

**Aksi menurut status** (tombol tampil hanya bila berizin; server tetap penentu):

| Status | Aksi utama | Isi ⋮ Aksi lain |
| --- | --- | --- |
| *Draft* (belum ditagihkan) | Tagihkan | Ubah, Batal |
| *Draft* penjualan langsung (sudah ditagihkan) | Serahkan | Ajukan Diskon, Batal, Cetak Bukti Pesanan |
| *Menunggu Uang Muka* | Kirim ke Pengerjaan | Ajukan Diskon, Batal, Cetak Bukti Pesanan |
| *Dalam Pengerjaan* | Tandai Siap | Ajukan Batal, Cetak Bukti Pesanan |
| *Siap Diambil* | Serahkan | Ajukan Batal, Tutup Tidak Diambil (Kepala Optik), Cetak Bukti Pesanan |
| *Tidak Diambil* | Serahkan (penyerahan susulan) | Cetak Bukti Pesanan |
| *Diserahkan* | Catat Keluhan | Cetak Bukti Pesanan |
| *Batal* | — | — |
| Ada pengajuan batal *Diajukan* | Putuskan (Kepala Optik) | aksi lain nonaktif dengan keterangan |

**Modal tindakan pendek:**

| Modal | Isian | Endpoint |
| --- | --- | --- |
| Tagihkan | Konfirmasi; *"Barang tidak bisa diubah sesudah ditagihkan."* | `POST orders/{id}/bill` |
| Kirim ke Pengerjaan | Tujuan (bengkel sendiri / lab luar + pemasok), perkiraan jadi. Bila uang muka belum cukup atau ada pesanan lain belum lunas, pesan server tampil apa adanya (mis. *"Lunasi pesanan OPT-ORD-RSMMC-000001 dulu atau daftarkan kunjungan baru."*) | `POST orders/{id}/send-to-work` |
| Tandai Siap | Konfirmasi | `POST orders/{id}/mark-ready` |
| Serahkan | Centang *identitas dicocokkan* (wajib; tombol nonaktif sebelum dicentang), nama + hubungan pengambil bila diwakilkan, catatan *fitting* | `POST orders/{id}/hand-over` |
| Batal | Alasan wajib | `POST orders/{id}/cancel` |
| Ajukan Batal | Alasan; per baris lensa stok: *sudah dipotong / belum* | `POST orders/{id}/cancellation-requests` |
| Putuskan Batal | Setujui/Tolak + catatan; tampil ringkasan pengajuan | `POST …/cancellation-requests/{requestId}/decision` |
| Tutup Tidak Diambil | Alasan; keterangan *"Tagihan tetap penuh; kacamata disimpan atas nama pasien."* | `POST orders/{id}/close-not-collected` |
| Ajukan Diskon | Baris tagihan, kebijakan diskon Optik, nominal, alasan. Baris dan harga dibaca dari tagihan Billing (`InvoiceId` dari status bayar); di atas batas → *"Menunggu persetujuan Kepala Optik"* | `POST billing-management/billing/invoices/{id}/discounts` (Billing, as-is) |

| Elemen | Aturan |
| --- | --- |
| Pratinjau harga (*Draft* belum ditagihkan) | `GET orders/{id}/price-preview` (`BE-OPT-054`): harga per barang, jumlah, dan perkiraan total, berlabel *"Perkiraan; harga final mengikuti tagihan."* Barang tanpa tarif ditandai *Tarif belum tersedia* dan Tagihkan akan ditolak server. Layar tidak menjumlah sendiri; total dari backend. Sebelum `BE-OPT-054` selesai: *"Harga tampil sesudah ditagihkan"* |
| Harga dan diskon baris (sesudah ditagihkan) | Dibaca dari tagihan Billing, bukan dari pesanan |
| Status bayar | `GET orders/{id}/payment-status`; dimuat ulang sesudah setiap aksi dan lewat tombol; tidak di-cache. Keterangan dasar terbayar: *"dihitung per tagihan kunjungan"* (`OPT-DEC-035`) |
| Cetak Bukti Pesanan | Nomor pesanan, nama pasien, No. RM, nomor kunjungan, barang, tujuan pengerjaan, perkiraan jadi, tanggal cetak. **Tanpa** nilai resep dan tanpa harga |
| Garansi | Sesudah diserahkan: tanggal diserahkan, batas garansi per jenis keluhan dari snapshot pesanan |
| Pesanan remake | Label *Remake* + tautan ke pesanan asal; tidak ada status bayar |

### 5.9 Keluhan & Garansi

**Daftar** — `…/warranty-claims`: penyaring Status (*Diajukan*, *Diperiksa*, *Disetujui*, *Ditolak*) dan
Cari. Kolom: No. Keluhan, Tanggal, No. RM, Nama Pasien, Pesanan asal, Jenis keluhan, Dalam/Luar garansi,
Status. Klik → detail.

**Catat keluhan** — `…/visits/[encounterId]/warranty-claims/new?orderId=`: pesanan asal tampil terkunci
(nomor, tanggal diserahkan, batas garansi); isian jenis keluhan dan uraian. Sesudah simpan, ke detail
keluhan; status garansi dihitung server dan tampil sebagai label.

**Detail** — `…/warranty-claims/[claimId]`:

| Status | Aksi (modal) | Isian |
| --- | --- | --- |
| *Diajukan* | **Periksa Ulang** (RO/Sp.M) | Temuan; pemeriksaan ulang dipilih dari pemeriksaan final pada kunjungan keluhan (opsional) |
| *Diperiksa* | **Putuskan** (Kepala Optik) | Setujui/Tolak, alasan; bila setuju: baris yang dibuat ulang (kosong = semua) dan resep baru bila ada |
| *Disetujui* | — | Tautan ke pesanan remake |
| *Ditolak* | — | Alasan; keterangan *"Proses sebagai pesanan berbayar baru."* |

### 5.10 Daftar Pesanan Optik — `…/orders`

```text
Pesanan Optik
[Menunggu Persetujuan Batal 1] [Perlu Dihubungi 2] [Terlambat 4] [Lewat Batas Ambil 0]
[Cari nama/No. RM/No. Pesanan] [Status ▼]
 No. Pesanan  Tanggal  No. RM    Nama Pasien  Jenis       Barang  Perkiraan jadi  Status            Tanda
 OPT-ORD-RSMMC-000001     04-11    00012345  Sari Wulan   Kacamata    2       05-11-2026      Dalam Pengerjaan  Terlambat 4 hari
```

| Bagian | Isi |
| --- | --- |
| Kartu penyaring | *Perlu Dihubungi* → `NeedsContact`; *Terlambat* → `Overdue`; *Menunggu Persetujuan Batal* → `HasPendingCancellation` dan *Lewat Batas Ambil* → `PastNotCollectedLimit` (`BE-OPT-054`). Klik kartu = menyaring |
| Penyaring | Status (dari metadata), Cari |
| Kolom | No. Pesanan, Tanggal, No. RM, Nama Pasien, Jenis (kacamata/penjualan langsung/remake), Jumlah barang, Perkiraan jadi, Status, Tanda (terlambat/perlu dihubungi/menunggu keputusan batal/lewat batas ambil; dari empat tanda baris `BE-OPT-054`) |
| Klik baris | Detail pesanan |

### 5.11 Laporan Optik — `…/reports`

Tab **Kunjungan per Pemeriksa**, **Penjualan**, **Pesanan Terbuka**, **Keluhan & Garansi**. Penyaring
periode (bawaan awal bulan berjalan s.d. hari ini, paling panjang 366 hari); tab Pesanan Terbuka memakai
*per tanggal*. Isi tabel mengikuti respons `reports/*`. Keterangan pada tab Penjualan: *"Penjualan bruto
dari baris tagihan, sebelum diskon tagihan."* Ekspor berkas tidak termasuk MVP.

### 5.12 Master Data

| Layar | Bentuk | Isi |
| --- | --- | --- |
| Katalog Barang Optik | Tabel: kode, nama, kategori, merk, warna/ukuran/indeks (menurut kategori), stok depo, aktif; penyaring kategori, cari, aktif. Aksi **Ubah Profil** → modal | `GET reference-data/item-catalog`; modal `PUT item-profiles/{drugId}` (isian menurut kategori: frame — merk, warna, ukuran, bahan; lensa — indeks, desain, lapisan; lensa kontak — BC, DIA, masa pakai) |
| Pengaturan Optik | Satu form: minimum uang muka %, pengingat ambil (hari), batas tutup tidak diambil (hari), usia resep (bulan), masa garansi (ukuran hari, lensa bulan, frame bulan), rentang BC/DIA. Keterangan *"Berlaku untuk pesanan sesudah disimpan."* Di bawahnya tabel riwayat perubahan | `GET`/`PUT reference-data/settings` (`ExpectedVersion`) |

Batas diskon tidak ada di Pengaturan Optik: diatur di kebijakan diskon Billing (`OPT-DEC-027`).

## 6. Perilaku bersama

| Keadaan | Perilaku |
| --- | --- |
| Memuat | Kerangka muat per bagian; tombol aksi nonaktif |
| Kirim ganda | Satu `Idempotency-Key` per niat aksi; tombol nonaktif selama permintaan |
| `409` | Pesan server, lalu detail dimuat ulang (status/versi berubah) |
| `403` | Pesan server apa adanya (izin atau STR/SIP); tidak dicoba ulang otomatis |
| `503` | *"Modul pendukung sedang tidak dapat dihubungi"* + tombol *Coba lagi* |
| Label status | Dari `GET reference-data/metadata`, tidak di-hardcode |
| Privasi | Nilai ukur, keluhan, dan temuan tidak ditulis ke `console`; daftar tidak menampilkan nilai resep |
| Layar | Dapat dipakai pada lebar 1366 px; tabel lebar bergulir mendatar di dalam kartunya |
| Aksesibilitas | Label pada setiap isian; pesan galat terbaca pembaca layar; form pemeriksaan dan resep dapat dinavigasi papan ketik |

## 7. Rentang yang diizinkan (`DEV_DISCRETION`)

Boleh diputuskan pengembang tanpa menanyakan pemilik, **selama** struktur di bagian 1–5 tidak berubah:
warna dan ikon (dari satu peta konstanta status), jarak dan lebar kolom, urutan kolom, susunan kartu
penyaring, teks bantuan singkat, bentuk indikator muat, tata letak isian OD/OS, dan pilihan komponen UI
yang sudah ada (`BaseSelectField`/`FilterSelect`, tabel standar, modal standar).

**Bukan** `DEV_DISCRETION` — butuh revisi brief: menambah/menghapus menu, mengubah route, memindahkan
tindakan dari halaman ke modal atau sebaliknya, menambah tab, menampilkan data yang tidak ada di kontrak,
dan menghitung uang di layar.

## 8. Ketergantungan dan catatan

| ID | Hal | Dampak ke layar | Pemilik |
| --- | --- | --- | --- |
| `OPT-API-v1.1` | Task `BE-OPT-054`: (a) dua penyaring + empat tanda di daftar pesanan (`OPT-DEC-067`); (b) pratinjau harga pesanan `Draft` (`OPT-DEC-075`, `076`) | `BE-OPT-054` ✅ selesai 2026-11-04 ([laporan](task/report/backend/BE-OPT-054.md)); kartu penyaring, kolom Tanda, dan pratinjau harga sudah punya endpoint | Yoga Aji Pratama (`/qv-build-be BE-OPT-054`) |
| Izin lintas modul | Petugas pendaftaran Optik butuh `PatientEncounter : Read` (pilihan kunjungan asal); petugas penjualan butuh izin baca tagihan dan `BillingDiscount : Create` untuk Ajukan Diskon | Tanpa izin, bagian itu menampilkan pesan server | Admin akses per departemen-jabatan |
| Pemasok lab luar | Pilihan pemasok untuk Kirim ke Pengerjaan dari endpoint pilihan pemasok yang sudah ada | Diverifikasi di `FE-OPT-020` | Pengembang |
| Cetak | Disusun dari data detail di layar; tidak ada endpoint cetak | — | Pengembang |
