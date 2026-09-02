# Bank Darah — Frontend Architecture

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` · Contract version `v4` — `draft` |
| `last_changed_in` | `v4` |
| Owner | Pemilik proses BDRS · pemilik proses klinis (pembeda golongan darah) |
| `approved_by` / `approved_at` | Kosong — `draft` |
| Sumber | `contracts/api-contract.md` · `contracts/permission-audit-matrix.md` · `00-interview-decisions.md` §7 (`FE-BD-001`..`009`) |
| Frontend SHA | `afbb8ab47a6a309f24cdaf6d72024f0dc1b2c254` cabang `sukmagpV2` |

Revisi ini memfokuskan kedalaman pada backend; frontend mengunci **keberadaan layar dan sumber
datanya**, bukan rupanya. Seluruh layar Bank Darah **baru** (`BD-CAP-020` `Missing`) dan memakai
komponen dasar yang sudah ada (`BD-CAP-021`) — dilarang membuat komponen dasar tandingan.

Hierarki wewenang: keamanan/privasi/invariant → brief produk yang disetujui → konvensi frontend V2 →
`DEV_DISCRETION`.

---

## 1. Kebutuhan layar

| ID layar | Nama | Jenis | Butir hak akses utama |
| --- | --- | --- | --- |
| `FE-BD-01` | Daftar Order Darah (daftar kerja #1) | Daftar | `BloodOrder : Read` |
| `FE-BD-02` | Kerja Order Darah (detail + buat + batal + pemenuhan) | Kerja per entity | `BloodOrder : Read/Create/Update` |
| `FE-BD-03` | Daftar & Kerja Permintaan PMI (buat, catat penerimaan) | Daftar + kerja | `BloodProviderRequest : *` |
| `FE-BD-04` | Daftar Kantong Darah (+ saringan `PendingReview` #2 dan tunggakan bukti darurat #3) | Daftar | `BloodUnit : Read` |
| `FE-BD-05` | Kerja Kantong (alokasi, bukti, pemberian, darurat, koreksi, penyelesaian) | Kerja per entity | `BloodUnit : *` |
| `FE-BD-06` | Pemeriksaan Golongan Darah (sampel, hasil, validasi, penyelesaian konflik) | Kerja per entity | `BloodGroupExam : *` |
| `FE-BD-07` | Daftar Tindakan Bank Darah | Daftar + kerja | `BloodBankProcedure : *` |
| `FE-BD-08` | Setup — Katalog Komponen Darah | Master CRUD | `BloodComponent : *` |
| `FE-BD-09` | Setup — Daftar Alasan Terkendali | Master CRUD | `BloodBankReason : *` |
| `FE-BD-10` | Setup — Lokasi Penyimpanan Darah (aktif/nonaktif) | Master CRUD | `BloodStorageLocation : *` |

Tiga daftar kerja MVP (`DEC-BD-023`): `FE-BD-01`, saringan `PendingReview` pada `FE-BD-04`
(`FE-BD-002`), dan saringan tunggakan bukti darurat pada `FE-BD-04` (`FE-BD-004`). Bukan modul laporan.

**Satu layar baru pada `v2`, bukan tiga.** Storage Location menambah pekerjaan di tiga tempat —
mengelola master, menetapkan lokasi kantong, dan memindahkan kantong — tetapi hanya **satu** yang
menuntut layar baru:

| Pekerjaan | Di mana dikerjakan | Kenapa |
| --- | --- | --- |
| Kelola master lokasi penyimpanan | **`FE-BD-10` (layar baru)** | Master ketiga Setup, sejajar `FE-BD-08` dan `FE-BD-09` (`DEC-BD-035` mengamandemen `DEC-BD-024`) |
| Tetapkan lokasi kantong yang baru diterima | `FE-BD-05` Kerja Kantong | Tindakan pada satu kantong, sejajar alokasi dan pemberian |
| Pindahkan kantong antarlokasi | `FE-BD-05` Kerja Kantong | Tindakan pada satu kantong yang sama |
| Temukan kantong `Received` / di lokasi nonaktif | `FE-BD-04` — **saringan**, bukan daftar kerja baru | `DEC-BD-023` menetapkan tiga daftar kerja; `AC-BD-057` menegaskan polanya |

Menambah daftar kerja keempat dan kelima akan melanggar `DEC-BD-023` dan mengulangi kekeliruan yang
sudah ditolak `AC-BD-057` pada penyelesaian konflik golongan darah.

---

## 2. Peta butir menu

Yang mengikat: layar mana dapat butir menu, route tujuannya, kedalaman nesting. Nama butir, urutan,
ikon, dan pengelompokan visual tetap `DEV_DISCRETION` (`FE-BD-006`). Route final mengikuti konvensi V2;
berkas yang disunting `src/utils/menu-sidebar/menu-items.jsx`.

```text
Bank Darah                               <- tingkat 0
├── Order Darah                          -> /health-services/blood-bank-management/blood-orders
├── Permintaan PMI                       -> .../provider-requests
├── Kantong Darah                        -> .../blood-units
├── Pemeriksaan Golongan Darah           -> .../blood-group-exams
├── Tindakan Bank Darah                  -> .../blood-bank-procedures
└── Setup                                <- grup tingkat 1
    ├── Katalog Komponen Darah           -> /health-services/master-data/blood-components
    ├── Daftar Alasan Terkendali         -> .../master-data/blood-bank-reasons
    └── Lokasi Penyimpanan Darah         -> .../master-data/blood-storage-locations
```

| Butir menu | Tingkat | Induk | Layar | Butir hak akses | Status |
| --- | :---: | --- | --- | --- | --- |
| Bank Darah | 0 | — | — | — | Baru |
| Order Darah | 1 | Bank Darah | `FE-BD-01` | `BloodOrder : Read` | Baru |
| Permintaan PMI | 1 | Bank Darah | `FE-BD-03` | `BloodProviderRequest : Read` | Baru |
| Kantong Darah | 1 | Bank Darah | `FE-BD-04` | `BloodUnit : Read` | Baru |
| Pemeriksaan Golongan Darah | 1 | Bank Darah | `FE-BD-06` | `BloodGroupExam : Read` | Baru |
| Tindakan Bank Darah | 1 | Bank Darah | `FE-BD-07` | `BloodBankProcedure : Read` | Baru |
| Setup | 1 | Bank Darah | — | — | Baru |
| Katalog Komponen Darah | 2 | Setup | `FE-BD-08` | `BloodComponent : Read` | Baru |
| Daftar Alasan Terkendali | 2 | Setup | `FE-BD-09` | `BloodBankReason : Read` | Baru |
| Lokasi Penyimpanan Darah | 2 | Setup | `FE-BD-10` | `BloodStorageLocation : Read` | **Baru pada `v2`** |

**Layar anak (tanpa butir menu sendiri):** `FE-BD-02` dicapai dari baris `FE-BD-01`; `FE-BD-05` dicapai
dari baris `FE-BD-04`. Penetapan dan perpindahan lokasi **tidak** mendapat butir menu sendiri; keduanya
tindakan di dalam `FE-BD-05`. Pendaftaran butir menu **wajib** menjadi acceptance criteria salah satu task
layar, bukan pekerjaan menganggur.

---

## 3. Skema fitur per layar

Skema mengunci isi & sumber data; warna/jarak/ikon/tab/modal tetap `DEV_DISCRETION`. Skema **tidak**
memuat field yang tidak ada pada response `contracts/api-contract.md`.

### `FE-BD-01` Daftar Order Darah

```text
+- Daftar Order Darah -------------------------------------- FE-BD-01 -+
| [cari no. order / nama pasien]   [Unit v] [Komponen v] [Status v]    |
+---------------------------------------------------------------------+
| No. Order | Pasien | Unit | Komponen | Diminta/Diberikan | Status |  |
| --------- | ------ | ---- | -------- | ---------------- | chip  |[Detail]|
+---------------------------------------------------------------------+
| memuat -> kerangka baris                                             |
| kosong -> "Belum ada order darah pada saringan ini." [Atur ulang]   |
| gagal  -> "Data gagal dimuat." [Coba lagi]                          |
+- Halaman 1 dari n -------------------- [< Sebelumnya] [Berikutnya >]+
```

| Wilayah | Isi | Sumber data | Hak akses | Bila kosong/gagal |
| --- | --- | --- | --- | --- |
| Saringan | Cari, unit, komponen, status | `GET /blood-orders` (query) | `BloodOrder : Read` | — |
| Tabel | No. order, pasien, unit, komponen, pemenuhan, status | `GET /blood-orders` | `BloodOrder : Read` | Kosong/gagal seperti skema |
| Tombol "Buat Order" | Buka `FE-BD-02` mode buat | — | `BloodOrder : Create` | Disembunyikan bila tak berhak |

### `FE-BD-02` Kerja Order Darah

| Wilayah | Isi | Sumber data | Hak akses | Bila kosong/gagal |
| --- | --- | --- | --- | --- |
| Kepala | No. order, pasien, kunjungan, unit, dokter, status | `GET /blood-orders/{id}` | `BloodOrder : Read` | Gagal → seluruh layar diganti pesan + coba lagi |
| Form buat (elektronik/manual) | Pasien, kunjungan, komponen+jumlah, dokter, (pelaku input bila manual) | `POST /` · `POST /manual` | `BloodOrder : Create` | Order ganda → tampil tahan + minta alasan (`FE-BD-003`); `POST /confirm-duplicate` |
| Ringkasan pemenuhan | Diminta/diberikan/sisa (menghormati koreksi) | `GET /blood-orders/{id}/fulfillment` | `BloodOrder : Read` | Kosong → "Belum ada pemberian." |
| Tombol Batalkan | Batal + pilih alasan | `POST /{id}/cancel` | `BloodOrder : Update` | Alasan wajib dari daftar |

**`FE-BD-001` (mengikat):** golongan darah **diminta** wajib terlihat jelas berbeda dari golongan darah
**hasil pemeriksaan**. Bukan kebebasan pengembang.

### `FE-BD-04` Daftar Kantong Darah (+ dua daftar kerja)

| Wilayah | Isi | Sumber data | Hak akses | Bila kosong/gagal |
| --- | --- | --- | --- | --- |
| Saringan status | Termasuk preset `PendingReview` (#2), `emergencyPendingEvidence` (#3), **`Received` (belum disimpan)**, dan **`inactiveLocation` (tertahan di lokasi nonaktif)** | `GET /blood-units?status=` / `?emergencyPendingEvidence=true` / `?status=Received` / `?inactiveLocation=true` | `BloodUnit : Read` | — |
| Tabel | No. kantong, komponen, asal permintaan, status, **lokasi penyimpanan saat ini**, penanda berlebih/darurat/**lokasi nonaktif** | `GET /blood-units` | `BloodUnit : Read` | Kosong/gagal seperti skema daftar |

**`FE-BD-002` & `FE-BD-004` (mengikat):** daftar `PendingReview` dan daftar tunggakan bukti darurat
**wajib ada**. Bentuk tampilan bebas.

**`FE-BD-010` (mengikat, baru pada `v2`):** dua saringan baru — kantong `Received` yang belum disimpan,
dan kantong yang tertahan karena lokasinya dinonaktifkan — **wajib ada** sebagai saringan pada layar
ini. Keduanya **MUST NOT** dibuat sebagai daftar kerja tersendiri (`DEC-BD-023`, `AC-BD-057`). Tanpa
keduanya, petugas tidak punya cara menemukan pekerjaan yang dituntut `DEC-BD-037`: memindahkan kantong
keluar dari kulkas yang dinonaktifkan.

**`FE-BD-011` (mengikat):** kolom **lokasi penyimpanan saat ini** wajib terbaca pada tabel kantong, dan
kantong yang lokasinya nonaktif wajib punya penanda yang terlihat tanpa membuka detail. Alasannya
keselamatan operasional: petugas harus dapat melihat sekali pandang kantong mana yang tidak dapat
dipakai, bukan menemukannya saat tombol Alokasikan ditolak.

### `FE-BD-05` Kerja Kantong

| Wilayah | Isi | Sumber data | Hak akses | Bila kosong/gagal |
| --- | --- | --- | --- | --- |
| Kepala | No. kantong, komponen, status, asal permintaan, **lokasi penyimpanan saat ini + penanda bila lokasinya nonaktif** | `GET /blood-units/{id}` | `BloodUnit : Read` | Gagal → pesan + coba lagi |
| **Tombol Tetapkan lokasi penyimpanan** | Pilih lokasi dari daftar **aktif** saja | `POST /{id}/storage-location` · pilihan dari `GET /blood-storage-locations/options` | `BloodUnit : Store` | Hanya muncul saat kantong `Received`. Kantong sudah ditempatkan → `VAL-BD-061` |
| **Tombol Pindahkan lokasi** | Pilih lokasi tujuan dari daftar **aktif** saja + keterangan opsional | `PUT /{id}/storage-location` | `BloodUnit : Store` | Muncul setelah kantong punya lokasi. **Tetap muncul walaupun lokasi asalnya nonaktif** — inilah jalan keluarnya (`DEC-BD-037`) |
| **Riwayat penempatan** | Lokasi, sejak kapan, oleh siapa — berurutan | `GET /{id}/placements` | `BloodUnit : Read` | Kosong → "Kantong belum pernah disimpan." |
| Tombol Alokasikan / Batalkan alokasi | Pilih baris order / alasan | `POST /{id}/allocate` · `/cancel-allocation` | `BloodUnit : Allocate` | Sudah dialokasikan → ditolak dengan pesan terbaca. **Belum disimpan** → `VAL-BD-063`. **Lokasi nonaktif** → `VAL-BD-064` |
| Tombol Catat bukti kecocokan | Terhadap pasien tujuan, **beserta hasil keputusan cocok / tidak cocok** | `POST /{id}/compatibility-evidence` | `BloodUnit : Compatibility` | Hasil wajib dipilih; bukti bertanda tidak cocok tersimpan tetapi **tidak** membuka tombol Berikan (`FE-BD-021`) |
| Tombol Berikan | Gerbang **tiga syarat**: sudah disimpan · lokasi terakhir aktif · bukti berlaku untuk pasien tujuan & belum lewat masa berlaku | `POST /{id}/issue` | `BloodUnit : Issue` | Penanda **bukti lewat masa berlaku** wajib terlihat sebelum tombol ditekan (`FE-BD-008`). **Lokasi nonaktif** → `VAL-BD-065`, dan alasannya wajib terbaca sebagai soal lokasi, bukan soal bukti (`FE-BD-012`) |
| Tombol Jalur darurat | Otorisasi + alasan + **pilihan gerbang yang dilewati** + **peran penerbit** + **keterangan kondisi kedaruratan** | `POST /{id}/emergency-issue` | `BloodUnit : EmergencyIssue` | **Wajib tampak jelas sebagai jalur tidak normal**, bukan tombol setara (`FE-BD-005`). Pilihan gerbang wajib diisi (`VAL-BD-066`) dan **wajib mencerminkan keadaan kantong saat itu** (`FE-BD-013`). Peran penerbit dan kondisi kedaruratan wajib diisi (`VAL-BD-070`, `VAL-BD-071`) |
| **Tombol Ajukan koreksi pencatatan** | Apa keliru/benar + alasan terkendali + **bukti pendukung** | `POST /{id}/corrections` | `BloodUnit : Correct` | Tersembunyi bila tak berhak. Setelah terkirim, koreksi tampil sebagai **menunggu persetujuan** dan angka pemenuhan **belum** berubah (`FE-BD-016`) |
| **Daftar koreksi + tombol Setujui/Tolak** | Koreksi pada kantong ini beserta keadaannya | `GET /{id}/corrections` · `POST /{id}/corrections/{correctionId}/approve` · `/reject` | `BloodUnit : Read` untuk daftar · `BloodUnit : ApproveCorrection` untuk keputusan | Tombol keputusan **tersembunyi pada koreksi yang diajukan pengguna itu sendiri** (`FE-BD-017`). Penolakan menuntut alasan |
| Tombol Penyelesaian (`PendingReview`) | Alihkan / kembalikan / tidak layak — **tiga tombol dengan tiga penjaga berbeda** | `POST /{id}/reallocate` · `/return-to-provider` · `/mark-not-usable` | `BloodUnit : ResolveReallocate` · `: ResolveReturn` · `: ResolveNotUsable` | Alasan wajib. **Ketiganya tampil terpisah** (`FE-BD-020`) |
| Riwayat kantong | **Penempatan**, alokasi, bukti, pemberian, koreksi, penyelesaian | `GET /blood-units/{id}` | `BloodUnit : Read` | Kosong → "Belum ada pergerakan." |

**`FE-BD-012` (mengikat).** Ketika pemberian ditolak karena lokasi nonaktif, pesan yang tampil wajib
menyebut **lokasi**, bukan bukti kecocokan. Petugas yang membaca "bukti tidak berlaku" padahal buktinya
baik-baik saja akan mencatat bukti baru berulang kali dan tetap gagal. Ini sebabnya `VAL-BD-064` dan
`VAL-BD-065` dipisah dari `VAL-BD-018`..`020`.

**`FE-BD-020` (mengikat).** Ketiga tombol penyelesaian `PendingReview` dijaga **tiga butir hak akses
berbeda** dan **wajib tampil terpisah** menurut hak akses pengguna. Petugas yang hanya berwenang
mengembalikan kantong ke PMI melihat tombol itu saja, **tidak** melihat tombol pengalihan. Menampilkan
ketiganya lalu menolak di server akan membatalkan pemisahan yang justru dituju `DEC-BD-043`, dan
membuat petugas mengira sistemnya rusak.

**`FE-BD-021` (mengikat).** Ketika bukti kecocokan yang berlaku menyatakan **tidak cocok**, tombol
Berikan **wajib tertutup** dan alasannya wajib terbaca sebagai soal **hasil pemeriksaan**, bukan soal
bukti yang belum ada. Petugas yang membaca "bukti belum tercatat" padahal buktinya ada dan menyatakan
tidak cocok akan mencatat bukti baru berulang kali — dan pada percobaan keberapa pun, yang benar adalah
kantong itu memang tidak boleh diberikan kepada pasien tersebut.

**`FE-BD-016` (mengikat).** Koreksi yang masih menunggu persetujuan **wajib** terlihat sebagai
menunggu, dan angka pemenuhan order **MUST NOT** berubah selama itu (`INV-BD-033`). Menampilkan angka
yang sudah dikoreksi sebelum keputusan turun adalah kebohongan terhadap pembaca berikutnya: ia akan
membaca jumlah darah yang sudah diberikan berdasarkan koreksi yang bisa saja ditolak.

**`FE-BD-017` (mengikat).** Tombol Setujui dan Tolak **wajib tersembunyi** pada koreksi yang diajukan
pengguna yang sedang membuka layar. Ini bukan sekadar mencegah galat `VAL-BD-073`: seseorang dapat sah
memegang kedua butir hak akses sekaligus, sehingga tombolnya akan muncul bila hanya hak akses yang
diperiksa. Yang menentukan tampil-tidaknya adalah **perbandingan pelaku**, bukan hak akses.

**`FE-BD-018` (mengikat).** Pilihan peran penerbit pada jalur darurat — Dokter BDRS atau DPJP pasien —
**wajib diisi sendiri oleh penerbit** dan tidak boleh disimpulkan layar dari role akun. Seorang dokter
dapat memenuhi kedua peran pada kasus yang berbeda, dan yang direkam `INV-BD-032` adalah **dengan
wewenang apa ia bertindak saat itu**, bukan jabatan apa yang melekat pada akunnya.

**`FE-BD-013` (mengikat).** Pilihan "gerbang yang dilewati" pada jalur darurat **MUST NOT** menjadi
pilihan bebas. Layar sudah tahu keadaan kantong — apakah buktinya kurang, apakah lokasinya nonaktif,
atau keduanya — sehingga pilihan itu disodorkan sudah terisi sesuai keadaan dan tidak dapat diubah ke
keterangan yang tidak benar. Penanda darurat yang isinya salah lebih berbahaya daripada tidak ada
penanda, karena ia menyesatkan pembaca rekam berikutnya (`INV-BD-030`).

**Yang sengaja TIDAK disediakan pada layar ini:** tombol "pindahkan semua kantong dari lokasi ini".
`DEC-BD-037` menetapkan sistem tidak memindahkan kantong dengan sendirinya, dan perpindahan massal
lewat satu tombol akan menghasilkan baris riwayat yang pelakunya seolah satu tindakan padahal darahnya
dipindahkan satu per satu oleh tangan. Petugas memindahkan per kantong.

### `FE-BD-06` Pemeriksaan Golongan Darah

| Wilayah | Isi | Sumber data | Hak akses | Bila kosong/gagal |
| --- | --- | --- | --- | --- |
| Kepala + status golongan darah sah pasien | Hasil sah / **penanda konflik** | `GET /blood-group-exams/patient/{patientId}/valid` | `BloodGroupExam : Read` | Konflik → penanda wajib terlihat & menahan pemakaian (`FE-BD-007`) |
| Catat sampel / hasil | Identifier sampel, ABO, Rhesus | `POST /` · `POST /{id}/result` | `BloodGroupExam : Create/Update` | Pemeriksa/waktu wajib |
| Tombol Validasi | Validasi hasil **rutin** | `POST /{id}/validate` | `BloodGroupExam : Validate` | Tersembunyi bila tak berhak |
| **Penyelesaian konflik** | Histori hasil, hasil ulang, tindakan validator klinis | `POST /conflict-resolution` (menunjuk pemeriksaan ulang) | **`BloodGroupExam : ResolveConflict`** | **Di layar ini, bukan daftar kerja keempat** (`FE-BD-009`, `DEC-BD-033`). Butir hak akses **berbeda** dari tombol Validasi (`DEC-BD-039`) |

**`FE-BD-019` (mengikat).** Tombol Validasi dan tindakan Penyelesaian konflik dijaga **dua butir hak
akses yang berbeda**, sehingga keduanya dapat tampil terpisah: seorang petugas BDRS berwenang validasi
melihat tombol Validasi tetapi **tidak** melihat tindakan penyelesaian konflik. Menyatukan keduanya di
balik satu pemeriksaan hak akses akan membatalkan pemisahan yang justru dituju `DEC-BD-039`.

### `FE-BD-08` / `FE-BD-09` Setup master

Layar CRUD sederhana memakai `base-editor-form.jsx` + `data-table.jsx`. `FE-BD-08` memuat kolom
`CompatibilityEvidenceValidityHours` per komponen. `FE-BD-09` memuat `ReasonCategory`. Keduanya
memakai pola master yang sudah ada; digambar sekali, dirujuk.

### `FE-BD-10` Setup — Lokasi Penyimpanan Darah

Layar CRUD master ketiga, memakai pola yang sama dengan `FE-BD-08`/`FE-BD-09`. Yang membedakan hanya
satu hal: menonaktifkan lokasi punya akibat yang terasa di layar lain, sehingga akibat itu wajib
diberitahukan di sini.

| Wilayah | Isi | Sumber data | Hak akses | Bila kosong/gagal |
| --- | --- | --- | --- | --- |
| Tabel | Kode, nama, urutan, keterangan, penanda aktif/nonaktif | `GET /blood-storage-locations` | `BloodStorageLocation : Read` | Kosong → **peringatan tegas**, lihat `FE-BD-014` |
| Tambah / Ubah | Kode, nama, urutan, keterangan | `POST /` · `PUT /{id}` | `BloodStorageLocation : Create/Update` | Kode ganda → `VAL-BD-067` |
| Saklar Aktif / Nonaktif | Mengubah `IsActive` | `PATCH /{id}/status` | `BloodStorageLocation : Update` | Berhasil, **disertai peringatan jumlah kantong tertahan** (`VAL-BD-068`) |

**`FE-BD-014` (mengikat).** Bila master ini kosong, layar wajib menyatakan akibatnya secara terang:
tanpa satu pun lokasi aktif, **tidak ada kantong darah yang dapat disimpan, dialokasikan, maupun
diberikan**. Keadaan kosong di sini bukan "belum ada data" yang netral seperti pada master lain — ia
menghentikan seluruh modul (`INV-BD-025`). Bunyinya diarahkan pada tindakan: "Belum ada lokasi
penyimpanan darah. Tambahkan minimal satu lokasi aktif sebelum kantong dapat disimpan."

**`FE-BD-015` (mengikat).** Saat pengguna menonaktifkan sebuah lokasi yang masih berisi kantong,
konfirmasi wajib menyebut **berapa banyak kantong** yang akan tertahan dan menyatakan bahwa sistem
**tidak** memindahkannya. Penonaktifan tetap dilanjutkan bila pengguna menyetujui — ia **tidak** ditahan
(`VAL-BD-068`), karena lokasi dinonaktifkan justru ketika ada yang salah dengannya.

**Penghapusan tidak disediakan.** Lokasi hanya dapat dinonaktifkan; riwayat penempatan lama wajib tetap
terbaca. Tombol hapus **MUST NOT** ada, walaupun pola master lain memilikinya.

---

## 4. Aksi per peran

Diturunkan dari `contracts/permission-audit-matrix.md` (bukan dikarang ulang). Tombol yang tak berhak
**MUST disembunyikan**, bukan ditampilkan lalu ditolak `403`.

| Peran | Aksi yang terlihat |
| --- | --- |
| Unit pelayanan / dokter peminta | Buat order, lihat status |
| Petugas Bank Darah | Order, permintaan, penerimaan, **tetapkan lokasi penyimpanan**, **pindahkan lokasi**, alokasi, batal alokasi, bukti, pemberian, tindakan, sampel & hasil golongan darah |
| Petugas BDRS berwenang validasi | Validasi hasil golongan darah **rutin** |
| Dokter BDRS / penanggung jawab klinis | Penyelesaian konflik golongan darah, jalur darurat, **menyetujui atau menolak** koreksi pemberian |
| Pemegang kewenangan klinis BDRS | Mengalihkan kantong `PendingReview` ke pasien lain |
| Pemegang kewenangan operasional BDRS | Mengembalikan kantong ke PMI |
| Dokter peminta | Membatalkan ordernya sendiri dengan alasan klinis |
| DPJP pasien | Jalur darurat untuk pasiennya |
| Admin master Bank Darah | Katalog komponen, daftar alasan, **lokasi penyimpanan darah (termasuk menonaktifkan)** |

---

## 5. Penanganan keadaan (wajib tiap layar daftar & kerja)

| Keadaan | Perilaku |
| --- | --- |
| Memuat | Kerangka baris/awalan, bukan layar kosong |
| Kosong | Kalimat terbaca petugas + tombol atur ulang saringan |
| Gagal | "Data gagal dimuat." + [Coba lagi] |
| Data basi | Setelah aksi status (alokasi/pemberian/validasi), daftar & detail di-*refetch*; token konkurensi `409` → "Data sudah berubah, muat ulang." |
| Pengiriman ganda | Tombol aksi status dinonaktifkan selama proses; server tetap dijaga token konkurensi |
| Konflik golongan darah | Penanda menahan tombol pemberian/alokasi yang menuntut golongan darah sah (`FE-BD-007`) |

---

## 6. Kewenangan UI (`DEV_DISCRETION`) dan yang mengikat

| Decision | Keadaan |
| --- | --- |
| `FE-BD-001` beda tampilan golongan darah diminta vs hasil | **Mengikat** |
| `FE-BD-002` daftar `PendingReview` | **Mengikat** ada; bentuk bebas |
| `FE-BD-003` perilaku order ganda (tahan + alasan) | **Mengikat** |
| `FE-BD-004` daftar tunggakan bukti darurat | **Mengikat** ada |
| `FE-BD-005` tampilan jalur darurat sebagai jalur tidak normal | **Mengikat** |
| `FE-BD-007` penanda hasil bertentangan | **Mengikat** terlihat & menahan |
| `FE-BD-008` penanda bukti lewat masa berlaku | **Mengikat** terlihat sebelum pemberian |
| `FE-BD-009` penyelesaian konflik di layar pemeriksaan | **Mengikat** |
| `FE-BD-006` menu, route, susunan tab, modal, warna, tata letak | `DEV_DISCRETION` — ikuti konvensi V2 |
