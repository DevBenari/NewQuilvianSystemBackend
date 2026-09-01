# Bank Darah — Interview Decisions

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` |
| Revision | `2` |
| Decision revision | `2` |
| Status | `draft` |
| Pass yang sudah dijalankan | Scope pass (2026-09-02) dan Closure pass (2026-09-02) |
| Product/domain owner | Pemilik proses Bank Darah / BDRS — nama pejabat berwenang belum disebutkan |
| Backend SHA | `9522caacf29371b1fddd1584e9a71ad94fe48d19` cabang `sukmagp` |
| Frontend SHA | `afbb8ab47a6a309f24cdaf6d72024f0dc1b2c254` cabang `sukmagpV2` |

Seluruh keputusan berstatus `draft`. Dijawab dalam sesi wawancara tidak sama dengan disetujui oleh
pemilik yang berwenang.

Audit kemampuan existing sudah dijalankan dan hasilnya ada di `02-existing-capability-map.md`.
Peringatan "scope dikunci tanpa audit" sudah dicabut.

---

## 1. Scope dan Outcome

Bank Darah adalah bagian sistem yang mengurus pemenuhan kebutuhan darah pasien — dari order masuk
sampai kantong darah diberikan, dikembalikan, atau ordernya dibatalkan.

**Alur utamanya:**

```text
Unit pelayanan membuat order darah
  -> Bank Darah memproses order
  -> Bank Darah meminta darah ke PMI atas nama pasien itu
  -> Permintaan diteruskan ke PMI secara manual, di luar sistem
  -> Darah diterima secara fisik oleh petugas Bank Darah
  -> Kantong dicatat, dan stok operasional bertambah di sini
  -> Kantong dialokasikan untuk order pasien
  -> Bukti pemeriksaan kecocokan dicatat
  -> Kantong diberikan kepada pasien
```

### `SCOPE-BD-001` — Batas scope yang sudah dikunci

Status `confirmed` oleh pemilik kebutuhan pada 2026-09-02.

**Di dalam scope**

| ID | Kemampuan | Asal |
| --- | --- | --- |
| BR-BD-001 sampai BR-BD-003 | Daftar order, pemantauan order, detail order | BRD |
| BR-BD-004 | Tindakan Bank Darah, dengan tarif tetap milik Billing | BRD |
| BR-BD-005 sampai BR-BD-008 | Pemantauan kantong, alokasi, pemberian, pemenuhan sebagian | BRD |
| BR-BD-009 dan BR-BD-010 | Pengembalian kantong dan pembatalan order | BRD |
| BR-BD-011 sampai BR-BD-016 | Label golongan darah, sampling, Laboratorium, HCLAB, laporan, setup | BRD |
| `BR-BD-017` | Pencatatan permintaan darah ke PMI sebagai penyedia darah dari luar | Baru, dari `DEC-BD-002` |
| `BR-BD-018` | Pencatatan penerimaan fisik kantong darah dari PMI ke Bank Darah MMC | Baru, dari `DEC-BD-002` |
| `BR-BD-019` | Kontrak penerimaan order darah dari unit pelayanan yang berwenang | Baru, dari `DEC-BD-004` |

**Di luar scope — milik modul lain**

| Data atau kemampuan | Modul pemilik |
| --- | --- |
| Pasien dan registrasi | PatientManagement, RegistrationManagement |
| Dokter dan pegawai | Human Resource — Master Data Workforce |
| Ruangan, poli, department, kelas pasien | Master Data |
| Tarif, invoice, pembayaran | BillingManagement |
| Hasil laboratorium umum dan specimen | LaboratoryManagement |

**Di luar scope — belum menjadi kebutuhan MVP**

Integrasi API PMI · integrasi HCLAB · manajemen donor · produksi darah · mesin crossmatch · mesin
kesesuaian klinis · keputusan klinis otomatis · modul laporan. Ditambah sisa daftar BRD §9:
registrasi donor, pengambilan darah donor, kelayakan donor, skrining penyakit infeksi, karantina,
pelepasan klinis kantong, skrining antibodi, penanganan reaksi transfusi, pemantauan pasca
transfusi, penanganan kedaluwarsa, dan pemusnahan.

---

## 2. Glossary

| ID | Istilah | Arti |
| --- | --- | --- |
| `GLO-BD-001` | **MMC** | Rumah sakit pemilik dan pengguna sistem Quilvian. **Bukan** pemasok darah. |
| `GLO-BD-002` | **PMI** | Palang Merah Indonesia. Penyedia darah dari luar rumah sakit, sekaligus pemegang kebenaran ketersediaan darah. |
| `GLO-BD-003` | **BDRS** | Bank Darah Rumah Sakit — unit di dalam MMC yang menjalankan modul ini. |
| `GLO-BD-004` | **Order aktif** | Order yang belum dibatalkan, belum kedaluwarsa, dan belum terpenuhi seluruhnya. |
| `GLO-BD-005` | **Kantong menunggu keputusan** | Kantong yang ordernya sudah berakhir tetapi nasibnya belum ditetapkan. Tidak boleh dipakai siapa pun sampai diselesaikan lewat `DEC-BD-019`. |
| `GLO-BD-006` | **Bukti kecocokan** | Catatan bahwa pemeriksaan kecocokan darah sudah dinyatakan selesai oleh petugas berwenang. Quilvian mencatat, tidak menghitung. |
| `GLO-BD-007` | **Jalur darurat** | Pemberian darah sebelum bukti kecocokan tercatat, hanya oleh peran berwenang, dengan alasan wajib dan penanda permanen. |

---

## 3. Aktor dan Tanggung Jawab

| Pelaku | Tanggung jawab |
| --- | --- |
| **Dokter / unit pelayanan asal** | Menentukan kebutuhan klinis darah, membuat permintaan, bertanggung jawab atas alasan medisnya. |
| **Petugas Bank Darah / BDRS** | Memproses permintaan, verifikasi administratif, meminta darah ke PMI, menerima darah, mengambil sampel, memeriksa golongan darah, mengalokasikan, dan memberikan. |
| **Dokter BDRS** | Penanggung jawab tindakan Bank Darah bila diperlukan. Kandidat pemegang wewenang jalur darurat — ditetapkan `DEF-BD-004`. **Bukan** penahan alur normal. |
| **PMI** | Penyedia darah dari luar rumah sakit. |

---

## 4. Business Rules dan Invariants

| ID | Aturan | Asal |
| --- | --- | --- |
| `INV-BD-011` | Golongan darah dan Rhesus yang tercatat pada permintaan tidak boleh dipakai sebagai dasar keputusan kesesuaian atau kelayakan pemberian darah. | `DEC-BD-011` |
| `INV-BD-012` | Kantong tidak dapat masuk status diberikan sebelum bukti kecocokan tercatat, kecuali lewat jalur darurat yang tercatat penuh. | `DEC-BD-013`, `DEC-BD-017` |
| `INV-BD-013` | Quilvian tidak pernah menghitung kompatibilitas darah maupun kelayakan kantong. Keduanya dinyatakan manusia; sistem hanya mencatat. | `DEC-BD-013`, `DEC-BD-019` |
| `INV-BD-014` | `MstPatient.BloodType` tidak pernah menjadi sumber klinis. Sumber sah golongan darah adalah hasil pemeriksaan milik Bank Darah. | `DEC-BD-015` |
| `INV-BD-015` | Tidak boleh ada dua sumber sah untuk hasil golongan darah tanpa aturan prioritas yang tertulis. | `DEC-BD-015`, `DEC-BD-018`, `DEC-BD-022` |
| `INV-BD-016` | Alasan pada pembatalan, pengalihan, penetapan tidak layak, dan jalur darurat tidak boleh berupa teks bebas semata. | `DEC-BD-024` |
| — | Bank Darah tidak memiliki stok nasional. Yang dikelola hanya kantong yang sudah masuk proses pelayanan. | `DEC-BD-001` |
| — | Stok operasional bertambah hanya setelah kantong diterima secara fisik. | `DEC-BD-002` |
| — | Tidak ada stok umum. Setiap kantong berasal dari permintaan atas nama satu pasien. | `DEC-BD-003` |
| — | Order ganda dikenali dari pasien, kunjungan, komponen, dan status aktif secara bersamaan. | `DEC-BD-005` |
| — | Kantong yang ordernya berakhir tidak pernah menjadi stok bebas. | `DEC-BD-007`, `DEC-BD-019` |
| — | Kekurangan pengiriman tidak melahirkan permintaan baru untuk kebutuhan yang sama. | `DEC-BD-008` |
| — | Kewenangan unit memesan darah adalah sifat konfigurasi, bukan daftar tetap di kode. Bawaan menolak dulu. | `DEC-BD-012` |

---

## 5. State dan Transition

### 5.1 Permintaan darah ke PMI

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| — | Buat permintaan | `REQUESTED` | Petugas Bank Darah | Ada order pasien yang sah |
| `REQUESTED` | Terima sebagian kantong | `PARTIALLY_FULFILLED` | Petugas Bank Darah | Kantong fisik diterima |
| `REQUESTED` / `PARTIALLY_FULFILLED` | Terima sisa kantong | `FULFILLED` | Petugas Bank Darah | Jumlah diterima sama dengan jumlah diminta |
| `REQUESTED` / `PARTIALLY_FULFILLED` | Batalkan | `CANCELLED` | Pihak berwenang | Alasan tercatat dari daftar alasan terkendali |
| `REQUESTED` / `PARTIALLY_FULFILLED` | Kunjungan pasien berakhir | `CLOSED_ENCOUNTER` | Sistem, otomatis | Sesuai `DEC-BD-014` dan `DEC-BD-020` |

**Contoh berangka.** Bank Darah meminta 3 kantong PRC untuk Tn. S. Hari pertama datang 2 kantong,
permintaan menjadi `PARTIALLY_FULFILLED` dengan sisa 1. Hari kedua datang 1 kantong, permintaan
menjadi `FULFILLED`. Stok operasional bertambah 2 pada hari pertama dan 1 pada hari kedua — bukan 3
sejak permintaan dibuat.

**Contoh penutupan administratif.** Tn. S pulang Senin siang saat permintaan masih menyisakan 1
kantong. Permintaan menjadi `CLOSED_ENCOUNTER` dengan alasan "kunjungan berakhir". Selasa pagi kurir
PMI tetap mengantar kantong sisanya. Kantong itu **tetap dicatat diterima**, tetap membawa rujukan
ke permintaan asal, lalu langsung masuk `PENDING_REVIEW`.

### 5.2 Order darah pasien

| Dari status | Tindakan | Ke status | Syarat |
| --- | --- | --- | --- |
| Aktif | Sebagian kantong diberikan | Terpenuhi sebagian | Dihitung dari transaksi nyata, bukan angka yang diketik |
| Aktif | Seluruh kantong diberikan | Terpenuhi penuh | — |
| Aktif | Dibatalkan pihak berwenang | Dibatalkan | Alasan dari daftar alasan terkendali |
| Aktif | Kunjungan asal berakhir | Kedaluwarsa | Sesuai `DEC-BD-014` |

### 5.3 Kantong darah

| Dari status | Tindakan | Ke status | Syarat |
| --- | --- | --- | --- |
| — | Diterima secara fisik | Tersedia di Bank Darah | Terikat pada permintaan asalnya |
| Tersedia | Dialokasikan | Dialokasikan | Order masih aktif, tidak ada alokasi bertentangan |
| Dialokasikan | Diberikan ke pasien | Diberikan | Bukti kecocokan tercatat (`DEC-BD-013`) |
| Dialokasikan | Diberikan lewat jalur darurat | Diberikan, ditandai tanpa bukti kecocokan | Otorisasi peran berwenang, alasan wajib (`DEC-BD-017`) |
| Tersedia / Dialokasikan | Order berakhir | `PENDING_REVIEW` | Tidak dapat dipakai siapa pun sampai diselesaikan |
| `PENDING_REVIEW` | Dialihkan ke pasien lain | `REALLOCATED` | Kelayakan dinyatakan petugas berwenang, alasan wajib |
| `PENDING_REVIEW` | Dikembalikan ke PMI | `RETURNED_TO_PROVIDER` | Bila proses bisnis PMI mendukung |
| `PENDING_REVIEW` | Dinyatakan tidak layak | `NOT_USABLE` | Kelayakan dinyatakan petugas berwenang, alasan wajib |

### 5.4 Pemeriksaan golongan darah Bank Darah

| Dari status | Tindakan | Ke status | Syarat |
| --- | --- | --- | --- |
| — | Ambil sampel | Sampel tercatat | Rujukan pasien, waktu, petugas pengambil, identifier sampel |
| Sampel tercatat | Catat hasil ABO dan Rhesus | Hasil tercatat | Pemeriksa dan waktu pemeriksaan tersimpan |
| Hasil tercatat | Validasi hasil | Hasil tervalidasi | Peran validator ditetapkan `DEF-BD-004` |

Nama status di atas adalah nama bisnis. Nama teknis dan nilai enum yang sebenarnya ditetapkan pada
fase perancangan, mengikuti kebiasaan penamaan status yang sudah dipakai repository.

---

## 6. Skenario Normal dan Exception

### 6.1 Proses bisnis utama

1. **Tujuan.** Memenuhi kebutuhan darah seorang pasien secara tertelusur dan aman.
2. **Pelaku.** Dokter atau unit pelayanan sebagai pemilik kebutuhan klinis; petugas Bank Darah
   sebagai pelaksana pemenuhan; PMI sebagai penyedia darah.
3. **Pemicu.** Pasien membutuhkan transfusi darah.
4. **Prasyarat.** Pasien terdaftar, kunjungan aktif, unit pelayanan punya kewenangan memesan.
5. **Langkah utama.**
   1. Unit pelayanan membuat order darah elektronik. Bila permintaan masuk lewat formulir kertas,
      petugas Bank Darah yang menginput.
   2. Sistem memeriksa apakah sudah ada order aktif untuk pasien, kunjungan, dan komponen yang sama.
   3. Petugas Bank Darah memproses order dan membuat permintaan darah ke PMI atas nama pasien itu.
   4. Permintaan diteruskan ke PMI secara manual, di luar sistem.
   5. Kantong datang dan diterima secara fisik. Stok operasional bertambah pada langkah ini.
   6. Petugas mengambil sampel dan memeriksa golongan darah pasien bila belum ada hasil tervalidasi.
   7. Petugas mengalokasikan kantong untuk order pasien tersebut.
   8. Bukti pemeriksaan kecocokan dicatat.
   9. Kantong diberikan kepada pasien, dan angka pemenuhan order diperbarui.
6. **Aturan bisnis.** Seluruh aturan pada bagian 4 berlaku.
7. **Perubahan status.** Sesuai tabel pada bagian 5.
8. **Jalur tidak normal.** Lihat tabel 6.2.
9. **Hasil akhir.** Order terpenuhi penuh, kantong berstatus diberikan, dan seluruh riwayatnya
   tersimpan: siapa meminta, kapan diterima, siapa memeriksa golongan darah, siapa menyatakan
   kecocokan, siapa mengalokasikan, dan siapa memberikan.

### 6.2 Jalur tidak normal

| Kejadian | Perilaku yang sudah ditetapkan |
| --- | --- |
| PMI mengirim kurang dari yang diminta | Permintaan tetap terbuka dengan sisa. Pengiriman berikutnya menambah ke permintaan yang sama. |
| PMI belum mengirim sama sekali | Permintaan tetap menunggu. Dilarang membuat permintaan baru untuk kebutuhan yang sama. |
| Order ganda terdeteksi | Ditahan. Dilanjutkan hanya dengan alasan tertulis yang tercatat pelakunya. |
| Pasien pulang sebelum darah diberikan | Order kedaluwarsa. Kantong masuk `PENDING_REVIEW`. Permintaan PMI yang masih kurang menjadi `CLOSED_ENCOUNTER`. |
| Kantong tetap datang setelah permintaan ditutup | Penerimaan tetap dicatat, kantong tetap membawa rujukan permintaan asal, lalu masuk `PENDING_REVIEW`. |
| Pasien kembali dengan kunjungan baru | Order baru boleh dibuat. Order lama tidak dihidupkan kembali. |
| Kebutuhan klinis bertambah | Permintaan baru boleh dibuat bila kebutuhan, komponen, atau jumlahnya berubah. |
| Darah dibutuhkan segera sebelum uji kecocokan selesai | Jalur darurat oleh peran berwenang, alasan wajib, ditandai permanen, bukti kecocokan menyusul. |
| Kantong tidak jadi dipakai | Tidak pernah menjadi stok bebas. Diselesaikan lewat tiga pilihan akhir pada `DEC-BD-019`. |

---

## 7. Frontend Decision Authority

Urutan wewenangnya: keamanan, privasi, dan invariant lebih dulu; lalu ringkasan produk atau UI yang
sudah disetujui; lalu kebiasaan project; baru kemudian kebebasan pengembang.

| Decision ID | Area | Owner | Status | Allowed range | Evidence |
| --- | --- | --- | --- | --- | --- |
| `FE-BD-001` | Pembedaan tampilan golongan darah **diminta** versus **hasil pemeriksaan** | Pemilik proses klinis | `draft` | Wajib terlihat berbeda secara jelas. Bukan kebebasan pengembang | `ASM-BD-004`, `INV-BD-011`, `INV-BD-014` |
| `FE-BD-002` | Daftar pemantauan kantong `PENDING_REVIEW` | Pemilik proses BDRS | `draft` | Wajib ada. Bentuk tampilan bebas | `DEC-BD-007`, `DEC-BD-023` |
| `FE-BD-003` | Perilaku layar saat order ganda terdeteksi | Pemilik proses BDRS | `draft` | Wajib menahan dan meminta alasan tertulis | `ASM-BD-001` |
| `FE-BD-004` | Daftar pemberian darurat yang bukti kecocokannya tertunggak | Pemilik proses BDRS | `draft` | Wajib ada | `DEC-BD-023` |
| `FE-BD-005` | Tampilan jalur darurat | Pemilik proses klinis | `draft` | Harus jelas terlihat sebagai jalur tidak normal, bukan tombol setara alur biasa | `DEC-BD-017` |
| `FE-BD-006` | Menu, route, susunan tab, modal, warna, tata letak | — | `DEV_DISCRETION` | Mengikuti kebiasaan frontend V2 dan komponen dasar yang sudah ada | PRD §8, `BD-CAP-021` |

---

## 8. Decision Log

### 8.1 Scope pass

| Decision ID | Type | Keputusan | Status |
| --- | --- | --- | --- |
| `SCOPE-BD-001` | `Decision` | Batas scope MVP: BR-BD-001..016 ditambah BR-BD-017, BR-BD-018, BR-BD-019 | `confirmed` |
| `DEC-BD-001` | `Decision` | Bank Darah bukan pemilik stok darah; PMI pemegang kebenaran ketersediaan | `draft` |
| `DEC-BD-002` | `Decision` | Permintaan ke PMI dicatat di Quilvian, pengiriman manual, tanpa API pada MVP; stok bertambah hanya setelah penerimaan fisik | `draft` |
| `DEC-BD-003` | `Decision` | Permintaan selalu atas nama satu pasien; tidak ada stok umum | `draft` |
| `DEC-BD-004` | `Decision` | Dua jalur order: elektronik dari pelayanan, dan manual oleh Bank Darah | `draft` |
| `DEC-BD-005` | `Decision` | Deteksi order ganda: pasien + kunjungan + komponen + status aktif | `draft` |
| `DEC-BD-006` | `Decision` | Order berakhir karena terpenuhi, dibatalkan, atau kunjungan ditutup | `draft` |
| `DEC-BD-007` | `Decision` | Kantong yang ordernya berakhir masuk keadaan menunggu keputusan | `draft` |
| `DEC-BD-008` | `Decision` | Permintaan PMI punya lifecycle pemenuhan sendiri | `draft` |
| `DEC-BD-009` | `Decision` | Tidak ada gerbang persetujuan pada lifecycle normal | `draft` |
| `DEC-BD-010` | `Decision` | PMI satu-satunya penyedia darah yang sah | `draft` |
| `DEC-BD-011` | `Decision` | Permintaan menyimpan komponen, jumlah, dan golongan/Rhesus yang diminta — bukan hasil laboratorium | `draft` |
| `DEC-BD-012` rev 2 | `Decision` | Unit pemesan MVP: Rawat Inap, IGD, Rawat Jalan. Daftar tidak dikunci di kode; penambahan lewat konfigurasi. Bawaan menolak dulu | `draft` |
| `DEC-BD-012` rev 1 | `Decision` | Unit pemesan dibatasi tiga unit tanpa ketentuan konfigurasi | `superseded` oleh rev 2 |

### 8.2 Closure pass

| Decision ID | Type | Keputusan | Status |
| --- | --- | --- | --- |
| `DEC-BD-013` | `Decision` | Bukti pemeriksaan kecocokan **wajib** tercatat sebelum darah diberikan. Quilvian mencatat titik pemeriksaan klinis, tidak menghitung kompatibilitas | `draft` |
| `DEC-BD-014` | `Decision` | Sinyal berakhirnya kunjungan mengikuti sumber lifecycle masing-masing jenis kunjungan | `draft` |
| `DEC-BD-015` | `Decision` | Hasil golongan darah dan Rhesus dicatat sebagai hasil pemeriksaan tersendiri milik Bank Darah | `draft` |
| `DEC-BD-016` | `Open Question` | Persetujuan pemilik Billing untuk konteks sumber Bank Darah pada kontrak Billing | `OPEN` |
| `DEC-BD-017` | `Decision` | Jalur darurat tersedia, hanya untuk peran berwenang, dengan alasan wajib dan penanda permanen | `draft` |
| `DEC-BD-018` | `Decision` | Sampling untuk pemeriksaan golongan darah dicatat Bank Darah sendiri, bukan sampel Laboratorium | `draft` |
| `DEC-BD-019` | `Decision` | Kantong `PENDING_REVIEW` diselesaikan lewat tiga pilihan akhir: dialihkan, dikembalikan ke PMI, atau tidak layak | `draft` |
| `DEC-BD-020` | `Decision` | Permintaan PMI yang masih kurang ditutup administratif saat kunjungan berakhir, tanpa menghapus riwayat | `draft` |
| `DEC-BD-021` | `Decision` | Biaya Bank Darah berasal dari tindakan, bukan dari kantong | `draft` |
| `DEC-BD-022` | `Decision` | HCLAB tidak menjadi ketergantungan aktif pada MVP | `draft` |
| `DEC-BD-023` | `Decision` | Menu Laporan tidak masuk MVP. Yang ada hanya tiga daftar kerja operasional | `draft` |
| `DEC-BD-024` | `Decision` | Setup MVP hanya berisi katalog komponen darah dan daftar alasan terkendali | `draft` |

### 8.3 Rincian keputusan closure pass

**`DEC-BD-013` — Bukti kecocokan wajib sebelum pemberian.**
Kantong tidak dapat masuk status diberikan sebelum ada catatan bahwa pemeriksaan kecocokan
dinyatakan selesai. Yang dicatat: status pemeriksaan, petugas yang menyatakan selesai, waktu
pemeriksaan, rujukan kantong, rujukan pasien dan order.
Batas tanggung jawab: Quilvian **tidak** menghitung kompatibilitas, tidak menjalankan mesin uji
silang, tidak menggantikan proses Laboratorium atau BDRS, dan tidak mengambil keputusan klinis
otomatis. Sumber keputusan kecocokan tetap pada proses klinis BDRS dan Laboratorium.

**`DEC-BD-014` — Sinyal berakhirnya kunjungan.**
Rawat Jalan dan IGD memakai status akhir kunjungan: `Completed`, `Cancelled`, atau `NoShow`.
Rawat Inap memakai waktu pasien benar-benar meninggalkan rumah sakit, **bukan** penutupan
administratif episode — karena order darah terikat pada keberadaan pasien dalam episode pelayanan,
bukan pada penyelesaian administrasi. Bank Darah tidak mengubah status kunjungan atau episode; ia
hanya membaca lifecycle dari modul pemiliknya.
**Contoh:** keputusan pulang Senin pagi, pasien benar-benar pulang Senin siang, episode ditutup
Rabu. Order Bank Darah tidak aktif sejak Senin siang.

**`DEC-BD-015` — Sumber sah golongan darah.**
Dicatat sebagai hasil pemeriksaan tersendiri milik Bank Darah, dengan riwayat pemeriksaannya
sendiri. Data minimum: rujukan pasien, rujukan sampel bila tersedia, ABO, Rhesus, pemeriksa, waktu
pemeriksaan, status validasi, audit perubahan. Bank Darah tidak membuat hasil laboratorium umum.
Bila Laboratory Management kelak punya kemampuan ini, wajib ada keputusan kepemilikan dan
penyelarasan sumber kebenaran. `MstPatient.BloodType` tetap data administratif saja.

**`DEC-BD-017` — Jalur darurat.**
Bukan pengganti pemeriksaan kecocokan, bukan keputusan otomatis, bukan jalur normal. Wajib
dilakukan peran berwenang, menyimpan alasan, pelaku, waktu, rujukan pasien, order, dan kantong,
serta diberi penanda bahwa pemberian dilakukan sebelum bukti kecocokan tercatat.
Alur normal: bukti kecocokan → pemberian. Alur darurat: otorisasi darurat → pemberian → bukti
kecocokan menyusul.

**`DEC-BD-018` — Sampling Bank Darah.**
Sampel Bank Darah bukan sampel Laboratorium, tidak membuat pesanan Laboratorium, tidak masuk
tagihan Laboratorium. Tujuannya menjaga penelusuran pasien → sampel → pemeriksaan → validasi →
lifecycle Bank Darah. Data minimum: rujukan pasien, waktu pengambilan, petugas pengambil,
identifier sampel, status sampel, hubungan ke hasil pemeriksaan. Bukan manajemen sampel serba guna.

**`DEC-BD-019` — Penyelesaian kantong menunggu keputusan.**
Tiga pilihan akhir: `REALLOCATED`, `RETURNED_TO_PROVIDER`, `NOT_USABLE`. Quilvian tidak menentukan
kelayakan; itu diputuskan petugas atau proses klinis berwenang. Setiap keputusan wajib mencatat
alasan, pelaku, waktu, kantong terdampak, order asal, pasien asal, dan pasien tujuan bila dialihkan.
Kantong tidak pernah menjadi stok bebas. Pengalihan wajib mempertahankan riwayat pasien asal →
alasan pelepasan → pasien tujuan. Pengembalian ke PMI hanya bila proses bisnis PMI mendukung.

**`DEC-BD-020` — Penutupan administratif permintaan PMI.**
Ditutup dengan alasan "kunjungan berakhir". Tidak menghapus riwayat, tidak menganggap permintaan
tidak pernah terjadi, dan bukan pernyataan bahwa darah tidak diperlukan. Bila kantong tetap datang
setelahnya, penerimaan tetap dicatat, kantong tetap membawa rujukan permintaan asal, dan langsung
masuk `PENDING_REVIEW`.

**`DEC-BD-021` — Sumber biaya.**
Tarif dimiliki Billing. Bank Darah hanya mengirim fakta bisnis tindakan yang selesai. Pemberian
beberapa kantong dalam satu tindakan tidak otomatis membuat beberapa tagihan.

**`DEC-BD-022` — HCLAB.**
Bukti yang diketahui hanya workstation `BANK DARAH`, kode `BBW`, dan Lab Sec `GL`. Belum ada bukti
teknis apa pun: kontrak integrasi, protokol komunikasi, kepemilikan data, pemetaan field, maupun
mekanisme sinkronisasi. Tetap dicatat sebagai temuan penelusuran integrasi.

**`DEC-BD-023` — Laporan.**
Menu Laporan tidak masuk MVP. Yang disediakan hanya tiga daftar kerja operasional: daftar order,
daftar kantong `PENDING_REVIEW`, dan daftar pemberian darurat yang bukti kecocokannya tertunggak.
Ketiganya alat menjalankan proses, bukan modul laporan. Laporan resmi digali setelah modul berjalan.

**`DEC-BD-024` — Isi Setup MVP.**
Hanya dua: katalog komponen darah, dan daftar alasan terkendali untuk pembatalan order, jalur
darurat, kantong menunggu keputusan, pengembalian, penetapan tidak layak, dan tindakan administratif
lain yang membutuhkan alasan. Alasan tidak boleh berupa teks bebas semata, dan perubahan data induk
wajib punya jejak audit. Setup **tidak** mengatur unit pelayanan pemesan darah — itu tetap mengikuti
konfigurasi unit pelayanan sesuai `DEC-BD-012`. Setup **tidak** boleh berisi master pasien, dokter,
unit pelayanan, tarif, maupun aturan klinis kompatibilitas darah.
**Contoh:** ketika petugas menyatakan kantong tidak layak pakai, ia memilih alasan dari daftar —
bukan mengetik bebas — sehingga kelak dapat dihitung berapa kantong gagal pakai per sebab.

### 8.4 Asumsi yang masih terbuka

| ID | Asumsi | Status |
| --- | --- | --- |
| `ASM-BD-001` | Sistem menahan order duplikat; melanjutkan hanya dengan alasan tertulis yang tercatat pelakunya. Tidak ada mode peringatan yang bisa dilewati | `draft` |
| `ASM-BD-002` | Pasien yang masih butuh darah setelah order kedaluwarsa harus dibuatkan order baru pada kunjungan baru. Order lama tidak dihidupkan kembali | `draft` |
| `ASM-BD-003` | Nomor kantong berasal dari PMI. Quilvian tidak menerbitkan nomor kantong sendiri | `draft` |
| `ASM-BD-004` | Golongan darah **diminta** harus terlihat jelas berbeda dari golongan darah **hasil pemeriksaan** di layar | `draft` |
| `ASM-BD-005` | Bila petugas lupa mengisi waktu pasien meninggalkan rumah sakit, order rawat inap tidak akan kedaluwarsa. Ditangani sebagai kualitas data di modul Inpatient, bukan dengan aturan cadangan di Bank Darah | `draft` |
| `ASM-BD-007` | Bila MMC tetap menagih biaya pengganti pengolahan darah per kantong, penagihan itu berjalan di luar Quilvian | `draft` |

`ASM-BD-006` sudah tidak berstatus asumsi. Daftar pemberian darurat yang bukti kecocokannya
tertunggak ditetapkan sebagai daftar kerja wajib pada `DEC-BD-023`.

### 8.5 Keputusan yang sengaja ditunda

| ID | Isi | Memblokir |
| --- | --- | --- |
| `DEF-BD-003` | Apakah semua komponen darah menuntut bukti kecocokan yang sama. Keputusan klinis lanjutan, baru dapat diambil setelah katalog komponen darah tersedia | `IMPLEMENTATION` aturan per komponen |
| `DEF-BD-004` | Peran mana yang berhak memakai jalur darurat, dan peran mana yang berhak memvalidasi hasil golongan darah. Menjadi bagian keputusan hak akses menyeluruh | `IMPLEMENTATION` jalur darurat dan validasi hasil |

`DEF-BD-001` ditutup oleh `DEC-BD-019`. `DEF-BD-002` ditutup oleh `DEC-BD-020`.

### 8.6 Konflik yang sudah diselesaikan

| ID | Masalah | Penyelesaian |
| --- | --- | --- |
| `CONF-BD-001` | BRD §9 menutup integrasi PMI, sementara PMI ditetapkan pemegang kebenaran ketersediaan darah | BRD §9 direvisi: pencatatan masuk scope, sambungan teknis tidak |
| `CONF-BD-002` | Menu `Pesan Baru` di dalam Bank Darah versus dokter perujuk dan ruangan asal | Keduanya benar; ada dua jalur pembuatan order |
| `CONF-BD-003` | Frasa "PMI/MMC" seolah keduanya pemasok | Seluruh frasa dibaca sebagai PMI; MMC adalah organisasi pengguna |
| `CONF-BD-004` | Tidak ada status bernama "ditutup", dan rawat inap punya jalur penutupan tersendiri | Diselesaikan `DEC-BD-014` dengan dua penyesuai berbeda per jenis kunjungan |
| `CONF-BD-005` | `MstPatient.BloodType` mudah dikira hasil pemeriksaan yang sah | Diselesaikan `DEC-BD-015`; sumber sah adalah hasil pemeriksaan milik Bank Darah |

---

## 9. Acceptance Criteria

| ID | Kondisi | Hasil yang diharapkan |
| --- | --- | --- |
| `AC-BD-001` | Pasien A, kunjungan `RI-001`, ada order PRC aktif, dibuat order PRC lagi | Tertahan, sistem meminta alasan |
| `AC-BD-002` | Pasien A, kunjungan `RI-001`, ada order PRC aktif, dibuat order trombosit | Boleh dibuat |
| `AC-BD-003` | Pasien A, kunjungan `RI-002` yang berbeda, dibuat order PRC | Boleh dibuat |
| `AC-BD-004` | Kunjungan rawat jalan `RJ-001` mencapai status `Completed`, ada order PRC belum terpenuhi | Order berhenti menahan order baru |
| `AC-BD-005` | Permintaan PMI 3 kantong PRC, diterima 2 pada hari pertama | Permintaan `PARTIALLY_FULFILLED`, diterima 2, sisa 1 |
| `AC-BD-006` | Permintaan PMI belum dikirim, dibuat permintaan baru untuk kebutuhan yang sama | Ditolak |
| `AC-BD-007` | Kunjungan berakhir sementara 2 kantong sudah diterima fisik | Kedua kantong masuk `PENDING_REVIEW`, tidak dapat dialokasikan ke pasien lain |
| `AC-BD-008` | Ada kantong `PENDING_REVIEW` | Muncul pada daftar kerja pemantauan |
| `AC-BD-009` | Permintaan sudah dikirim tetapi darah belum diterima fisik | Stok operasional tidak bertambah |
| `AC-BD-010` | Order manual tanpa pasien, kunjungan, dokter peminta, atau unit asal | Ditolak |
| `AC-BD-011` | Setiap order yang tersimpan | Menyimpan jejak siapa yang menginput |
| `AC-BD-012` | Golongan darah pada permintaan dipakai untuk menyatakan kantong cocok atau tidak | Ditolak — bukan hasil pemeriksaan yang sah |
| `AC-BD-013` | Unit yang tidak dikonfigurasi berwenang mencoba membuat order darah | Ditolak |
| `AC-BD-014` | Permintaan darah tanpa jumlah kantong | Ditolak |
| `AC-BD-015` | Unit baru diberi kewenangan lewat konfigurasi, tanpa perubahan kode | Unit itu langsung dapat membuat order darah |
| `AC-BD-016` | Unit tanpa konfigurasi kewenangan apa pun | Ditolak — tidak ada kewenangan bawaan |
| `AC-BD-017` | Pasien rawat inap dengan waktu pulang fisik terisi Senin siang, episode baru ditutup Rabu | Order Bank Darah tidak aktif sejak Senin siang, bukan Rabu |
| `AC-BD-018` | Kantong dialokasikan, bukti kecocokan belum tercatat, petugas menekan pemberian | Ditolak |
| `AC-BD-019` | Bukti kecocokan tercatat lengkap, lalu pemberian dilakukan | Berhasil; kantong menjadi diberikan |
| `AC-BD-020` | Pemberian lewat jalur darurat oleh peran berwenang dengan alasan terisi | Berhasil; kantong ditandai diberikan sebelum bukti kecocokan tercatat, dan muncul pada daftar tunggakan |
| `AC-BD-021` | Pemberian lewat jalur darurat oleh peran tidak berwenang | Ditolak |
| `AC-BD-022` | Permintaan PMI masih menyisakan 1 kantong saat kunjungan berakhir | Permintaan menjadi `CLOSED_ENCOUNTER`, riwayatnya tetap utuh |
| `AC-BD-023` | Kantong tetap diantar PMI setelah permintaan `CLOSED_ENCOUNTER` | Penerimaan tetap dicatat, kantong membawa rujukan permintaan asal, lalu masuk `PENDING_REVIEW` |
| `AC-BD-024` | Kantong `PENDING_REVIEW` dialihkan ke pasien lain dengan alasan terisi | Berhasil; riwayat pasien asal, alasan pelepasan, dan pasien tujuan tersimpan |
| `AC-BD-025` | Kantong `PENDING_REVIEW` diselesaikan tanpa mengisi alasan | Ditolak |
| `AC-BD-026` | Satu tindakan Bank Darah selesai dengan 2 kantong diberikan | Satu fakta biaya dikirim ke Billing, bukan dua |
| `AC-BD-027` | Fakta biaya tindakan yang sama dikirim ulang | Billing mengenalinya sebagai kiriman ulang, tidak menagih dua kali |
| `AC-BD-028` | Golongan darah pasien diambil dari `MstPatient.BloodType` untuk keperluan klinis Bank Darah | Ditolak — bukan sumber klinis |
| `AC-BD-029` | Alasan penetapan kantong tidak layak diisi sebagai teks bebas tanpa memilih dari daftar | Ditolak |
| `AC-BD-030` | Hasil pemeriksaan golongan darah dicatat tanpa pemeriksa atau waktu pemeriksaan | Ditolak |

Seluruh nama pasien dan nomor kunjungan pada contoh adalah data samaran.

---

## 10. Open Questions dan Blocker

| ID | Isi | Pemilik | Memblokir |
| --- | --- | --- | --- |
| `DEC-BD-016` | Persetujuan pemilik Billing untuk menambah konteks sumber dan jenis efek biaya Bank Darah pada kontrak Billing. Pemicunya sudah jelas: satu tindakan Bank Darah yang selesai | Pemilik BillingManagement | Penyerahan biaya ke Billing |
| `DEF-BD-003` | Apakah semua komponen darah menuntut bukti kecocokan yang sama | Pemilik proses klinis | `IMPLEMENTATION` aturan per komponen |
| `DEF-BD-004` | Peran pemakai jalur darurat dan peran validator hasil golongan darah | Pemilik proses BDRS dan klinis | `IMPLEMENTATION` jalur darurat dan validasi |
| `OQ-BD-010` | Apakah PMI menerima pengembalian kantong yang sudah keluar. Fakta di luar sistem | Pemilik proses BDRS | Tidak memblokir rancangan |
| `OQ-BD-011` | Isi label golongan darah, kapan boleh dicetak, identifier uniknya, dan perilaku cetak ulang. `DEC-BD-015` baru menutup sumber datanya, bukan mekanik labelnya | Pemilik proses klinis | `DESIGN` label dan pencetakan |
| `BD-DEP-008` | Bank Darah belum terdaftar di registry kepemilikan modul dan prefix | Pemilik registry engineering | `IMPLEMENTATION` backend |
| `BD-DEP-009` | Tiga berkas bukti kebutuhan yang dirujuk BRD tidak ada di repository | Pemilik kebutuhan | Penelusuran bukti ke kebutuhan |

### Pertanyaan yang sudah tertutup

Scope pass: `OQ-BD-002` sampai `OQ-BD-008`.
Closure pass: `DEC-BD-013`, `DEC-BD-014`, `DEC-BD-015`, `DEC-BD-017` sampai `DEC-BD-024`, serta
`DEF-BD-001` dan `DEF-BD-002`.

---

## 11. Langkah Berikutnya

Sepuluh dari dua belas blocker yang masuk closure pass sudah tertutup. Yang tersisa —
`DEC-BD-016`, `DEF-BD-003`, `DEF-BD-004`, `OQ-BD-011` — tidak menghalangi perancangan arsitektur
target untuk alur inti, hanya menahan bagian yang bergantung padanya.

Langkah berikutnya adalah menilai ulang kesiapan pada `02-requirement-completeness-assessment.md`,
lalu mengirim slice yang siap ke `hospital-domain-architect` atau langsung ke
`design-business-module`.
