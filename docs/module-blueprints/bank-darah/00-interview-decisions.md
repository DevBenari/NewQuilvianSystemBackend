# Bank Darah — Interview Decisions

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` |
| Revision | `5` |
| Decision revision | `5` |
| Status | `draft` |
| Pass yang sudah dijalankan | Scope pass (2026-09-02), Closure pass (2026-09-02), Architecture gap closure pass (2026-09-02), Architecture gap final closure pass (2026-09-02), Storage Location closure pass (2026-09-02) |
| Product/domain owner | Pemilik proses Bank Darah / BDRS — nama pejabat berwenang belum disebutkan |
| Backend SHA | `9dc7637adbafb321ad8078d5c52ebe5e4398fe86` cabang `sukmagp` |
| Backend SHA saat bukti diaudit | `9522caacf29371b1fddd1584e9a71ad94fe48d19`. Perbedaan terhadap SHA di atas hanya berisi dokumen blueprint Bank Darah, nol berkas source aplikasi, sehingga `02-existing-capability-map.md` **tidak basi secara isi** |
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
| `BR-BD-020` | Pencatatan lokasi penyimpanan fisik kantong darah (kulkas darah) dan perpindahannya | Baru, dari `DEC-BD-035`, `DEC-BD-036` |

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
| `INV-BD-017` | Jumlah sisa permintaan ke PMI tidak pernah bernilai negatif. Kantong yang melebihi jumlah diminta tetap dicatat diterima, tidak menjadi milik order, dan langsung masuk `PENDING_REVIEW`. | `DEC-BD-025` |
| `INV-BD-018` | Seorang pasien memiliki paling banyak satu hasil golongan darah yang sah. Bila hasil tervalidasi terbaru berbeda dari hasil sah sebelumnya, pasien itu tidak punya hasil sah sama sekali sampai perbedaannya diselesaikan peran validator. | `DEC-BD-026` |
| `INV-BD-019` | Bukti kecocokan yang sudah lewat masa berlakunya tidak membuka gerbang pemberian. Selama nilai masa berlaku belum dikonfigurasi, gerbang pemberian tertutup. | `DEC-BD-027` |
| `INV-BD-020` | Bukti kecocokan terikat pada pasangan kantong dan pasien tertentu. Pengalihan kantong ke pasien lain menggugurkannya, dan bukti lama tidak pernah dipakai ulang untuk pasien tujuan. | `DEC-BD-028` |
| `INV-BD-021` | Pemberian tidak pernah dihapus atau dibalik. Satu-satunya jalur koreksi adalah catatan koreksi tambahan yang mempertahankan pemberian asal. | `DEC-BD-030` |
| `INV-BD-022` | Konflik golongan darah hanya dapat ditutup lewat pemeriksaan ulang yang tervalidasi dan pernyataan validator. Sistem tidak pernah menutup konflik dengan memilih salah satu hasil lama tanpa pemeriksaan baru, dan tidak pernah menghitung mayoritas. | `DEC-BD-031` |
| `INV-BD-023` | Masa berlaku bukti kecocokan disimpan per komponen pada katalog komponen darah dan selalu dibaca dari konfigurasi. Tidak pernah ada angka masa berlaku yang ditanam di kode. | `DEC-BD-032` |
| `INV-BD-024` | Koreksi pencatatan Bank Darah tidak pernah membalik atau mengubah fakta biaya secara otomatis. Keputusan peninjauan biaya adalah wewenang Billing. | `DEC-BD-034` |
| `INV-BD-025` | Kantong darah tidak dapat dialokasikan sebelum memiliki lokasi penyimpanan dan melalui proses penyimpanan (status `STORED`). Storage Location adalah bagian kesiapan operasional kantong, bukan atribut informasi semata. | `DEC-BD-036` |
| `INV-BD-026` | Perpindahan lokasi penyimpanan kantong dicatat sebagai histori perpindahan yang hanya bertambah; ia tidak pernah mengubah histori penerimaan awal kantong. | `DEC-BD-036` |

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
| `REQUESTED` / `PARTIALLY_FULFILLED` | Terima kantong melebihi jumlah diminta | `FULFILLED`, sisa berhenti di 0 | Petugas Bank Darah | Kantong berlebih tetap dicatat, tetap membawa rujukan permintaan asal, lalu masuk `PENDING_REVIEW` (`DEC-BD-025`) |

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
| — | Diterima secara fisik dari PMI | `RECEIVED` | Terikat pada permintaan asalnya; belum punya lokasi penyimpanan, belum dapat dialokasikan (`DEC-BD-036`) |
| `RECEIVED` | Tetapkan lokasi penyimpanan | `STORED` | Lokasi dipilih dari master lokasi penyimpanan darah yang aktif (`DEC-BD-035`) |
| `STORED` | Siap dialokasikan | Tersedia di Bank Darah | Hanya kantong `STORED` yang dapat masuk proses ketersediaan/alokasi (`INV-BD-025`) |
| `STORED` / Tersedia / Dialokasikan | Pindahkan lokasi penyimpanan | Status tidak berubah; lokasi diperbarui | Perpindahan dicatat sebagai histori; histori penerimaan awal tetap utuh (`INV-BD-026`) |
| Tersedia | Dialokasikan | Dialokasikan | Order masih aktif, tidak ada alokasi bertentangan |
| Dialokasikan | Diberikan ke pasien | Diberikan | Bukti kecocokan tercatat (`DEC-BD-013`) |
| Dialokasikan | Diberikan lewat jalur darurat | Diberikan, ditandai tanpa bukti kecocokan | Otorisasi peran berwenang, alasan wajib (`DEC-BD-017`) |
| Tersedia / Dialokasikan | Order berakhir | `PENDING_REVIEW` | Tidak dapat dipakai siapa pun sampai diselesaikan |
| `PENDING_REVIEW` | Dialihkan ke pasien lain | `REALLOCATED` | Kelayakan dinyatakan petugas berwenang, alasan wajib. Bukti kecocokan terhadap pasien asal gugur otomatis; pasien tujuan wajib punya bukti sendiri (`DEC-BD-028`) |
| `PENDING_REVIEW` | Dikembalikan ke PMI | `RETURNED_TO_PROVIDER` | Bila proses bisnis PMI mendukung |
| `PENDING_REVIEW` | Dinyatakan tidak layak | `NOT_USABLE` | Kelayakan dinyatakan petugas berwenang, alasan wajib |
| Dialokasikan | Batalkan alokasi | Tersedia, atau `PENDING_REVIEW` bila order asal sudah berakhir | Kantong belum diberikan; alasan dari daftar terkendali; bukti kecocokan yang terlanjur tercatat gugur (`DEC-BD-029`) |
| Dialokasikan, bukti lengkap | Masa berlaku bukti terlampaui | Dialokasikan, bukti tidak lagi berlaku | Terjadi karena waktu berjalan. Gerbang pemberian tertutup kembali; bukti lama tetap tersimpan (`DEC-BD-027`) |
| Diberikan | Catat koreksi pencatatan | Tetap `Diberikan`, dengan catatan koreksi melekat padanya | Peran berwenang (`DEF-BD-004`); pemberian asal tidak dihapus; alasan wajib; angka pemenuhan order dihitung ulang (`DEC-BD-030`) |

### 5.4 Pemeriksaan golongan darah Bank Darah

| Dari status | Tindakan | Ke status | Syarat |
| --- | --- | --- | --- |
| — | Ambil sampel | Sampel tercatat | Rujukan pasien, waktu, petugas pengambil, identifier sampel |
| Sampel tercatat | Catat hasil ABO dan Rhesus | Hasil tercatat | Pemeriksa dan waktu pemeriksaan tersimpan |
| Hasil tercatat | Validasi hasil | Hasil tervalidasi | Peran validator ditetapkan `DEF-BD-004` |
| Hasil tervalidasi | Muncul hasil tervalidasi baru yang **berbeda** ABO atau Rhesus-nya | Perbedaan tertahan — pasien tidak punya hasil sah | Terjadi otomatis. Ditutup hanya oleh peran validator (`DEC-BD-026`) |
| Perbedaan tertahan | Catat pemeriksaan ulang | Perbedaan masih tertahan | Petugas Bank Darah. Sampel baru dan hasil baru tercatat, lalu divalidasi (`DEC-BD-031`) |
| Perbedaan tertahan, ada hasil baru tervalidasi | Selesaikan perbedaan | Satu hasil sah kembali berlaku | Peran validator (`DEF-BD-004`) menyatakan hasil baru itu yang berlaku. Wajib ada pemeriksaan ulang tervalidasi; alasan, pelaku, dan waktu tersimpan; seluruh hasil tetap terbaca (`DEC-BD-031`) |

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
| PMI mengirim **lebih** dari yang diminta | Seluruh kantong tetap dicatat diterima. Permintaan menjadi `FULFILLED` dengan sisa 0, bukan angka negatif. Kantong berlebih masuk `PENDING_REVIEW` dengan alasan "kiriman melebihi permintaan". |
| Hasil golongan darah baru berbeda dari hasil sah sebelumnya | Ditahan. Pasien tidak punya golongan darah sah sampai peran validator menyelesaikan perbedaannya. Kedua hasil tetap tersimpan. |
| Pemberian tertunda sampai bukti kecocokan kedaluwarsa | Gerbang pemberian tertutup kembali. Bukti lama tetap tersimpan sebagai riwayat; pemberian menuntut bukti baru. |
| Kantong dialihkan ke pasien lain | Bukti kecocokan terhadap pasien asal gugur otomatis. Pasien tujuan wajib punya bukti kecocokan sendiri, walaupun golongan darahnya kebetulan sama. |
| Petugas salah memilih kantong saat alokasi | Alokasi boleh dibatalkan petugas Bank Darah sendiri selama kantong belum diberikan, dengan alasan dari daftar terkendali. |
| Pencatatan pemberian ternyata keliru | Dikoreksi lewat catatan koreksi tersendiri. Pemberian asal tidak pernah dihapus maupun dibalik. |

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
| `FE-BD-007` | Penanda hasil golongan darah yang sedang **bertentangan** | Pemilik proses klinis | `draft` | Wajib terlihat dan wajib menahan pemakaian. Bukan kebebasan pengembang | `DEC-BD-026`, `INV-BD-018` |
| `FE-BD-008` | Penanda bukti kecocokan yang sudah lewat masa berlaku | Pemilik proses klinis | `draft` | Wajib terlihat sebelum petugas menekan pemberian. Bentuk tampilannya bebas | `DEC-BD-027`, `INV-BD-019` |
| `FE-BD-009` | Penyelesaian konflik golongan darah dilakukan di dalam layar pemeriksaan golongan darah, bukan daftar kerja baru | Pemilik proses BDRS | `draft` | Wajib menampilkan histori hasil, status konflik, hasil pemeriksaan baru, status validasi, dan tindakan penyelesaian validator. Tidak menambah daftar kerja | `DEC-BD-033`, `OQ-BD-013` |

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

### 8.7 Architecture gap closure pass

Enam gap arsitektur yang dibuka `03-domain-architecture.md` ditutup pada sesi ini. Tidak ada satu pun
keputusan sebelumnya yang dibuka ulang.

| Decision ID | Menutup | Type | Keputusan | Status |
| --- | --- | --- | --- | --- |
| `DEC-BD-025` | `ARCH-BD-GAP-01` | `Decision` | Kelebihan kiriman PMI tetap dicatat diterima; sisa permintaan berhenti di 0; kantong berlebih masuk `PENDING_REVIEW` | `draft` |
| `DEC-BD-026` | `ARCH-BD-GAP-02` | `Decision` | Hasil golongan darah tervalidasi terbaru yang berlaku; bila berbeda dari hasil sah sebelumnya, ditahan sampai peran validator menyelesaikannya | `draft` |
| `DEC-BD-027` | `ARCH-BD-GAP-03` | `Decision` | Bukti kecocokan punya masa berlaku dalam satuan jam, ditetapkan lewat konfigurasi, bukan dikunci di kode | `draft` |
| `DEC-BD-028` | `ARCH-BD-GAP-04` | `Decision` | Pengalihan kantong ke pasien lain menggugurkan bukti kecocokan terhadap pasien asal secara otomatis | `draft` |
| `DEC-BD-029` | `ARCH-BD-GAP-05` | `Decision` | Alokasi keliru boleh dibatalkan petugas Bank Darah sendiri selama kantong belum diberikan, dengan alasan terkendali | `draft` |
| `DEC-BD-030` | `ARCH-BD-GAP-06` | `Decision` | Pemberian tidak pernah dihapus atau dibalik; koreksi hanya lewat catatan koreksi yang mempertahankan pemberian asal | `draft` |

Owner keputusan: `DEC-BD-025`, `DEC-BD-029` pemilik proses BDRS · `DEC-BD-026`, `DEC-BD-027`,
`DEC-BD-028` pemilik proses klinis · `DEC-BD-030` pemilik proses klinis bersama BDRS.
Seluruhnya berstatus `draft`. Dijawab dalam sesi wawancara tidak sama dengan disetujui pejabat
berwenang, dan `approved_by` beserta `approved_at` masih kosong.

### 8.8 Rincian keputusan architecture gap closure pass

**`DEC-BD-025` — Kelebihan kiriman dari PMI.**
Penerimaan fisik tidak pernah ditolak sistem. Bila PMI mengantar lebih banyak dari jumlah yang
diminta, seluruh kantong tetap dicatat diterima dan tetap membawa rujukan ke permintaan asalnya.
Jumlah sisa permintaan dijaga tidak pernah menjadi angka negatif: begitu jumlah diminta terpenuhi,
permintaan menjadi `FULFILLED` dan sisa berhenti di 0. Kantong yang melebihi jumlah diminta **tidak**
menjadi milik order pasien itu. Kantong tersebut langsung masuk `PENDING_REVIEW` dengan alasan
"kiriman melebihi permintaan", lalu diselesaikan lewat tiga pilihan akhir `DEC-BD-019`.
*Alasan pemilihan.* Bentuk ini memakai ulang mekanisme yang sudah disepakati, menjaga angka "jumlah
diminta" tetap jujur sebagai alat menilai kepatuhan PMI, dan tidak pernah membiarkan darah yang
secara fisik sudah ada di kulkas menjadi tidak tercatat. Bentuknya persis sama dengan perlakuan
kantong yang tetap datang setelah `CLOSED_ENCOUNTER` pada `DEC-BD-020`.
*Batas keputusan.* Keputusan ini **tidak** membuka stok bebas. `DEC-BD-003` tetap berlaku penuh:
kantong berlebih tetap terikat pada permintaan asal sampai diselesaikan, dan tidak boleh dialokasikan
langsung ke order mana pun — termasuk order pasien yang sama.
*Yang perlu ditambahkan.* Satu kode alasan baru pada daftar alasan terkendali `DEC-BD-024`, yaitu
"kiriman melebihi permintaan".
*Contoh berangka.* Bank Darah meminta 2 kantong PRC untuk Tn. S. PMI mengantar 3. Kantong ke-1 dan
ke-2 masuk sebagai penerimaan biasa, dan permintaan menjadi `FULFILLED` dengan sisa 0 — bukan
minus 1. Kantong ke-3 tetap dicatat diterima, tetap menunjuk permintaan asal, lalu langsung masuk
`PENDING_REVIEW`. Nasibnya ditetapkan terpisah: dialihkan ke pasien lain, dikembalikan ke PMI, atau
dinyatakan tidak layak.

**`DEC-BD-026` — Hasil golongan darah mana yang berlaku.**
Yang berlaku adalah hasil tervalidasi **terbaru**. Tetapi bila nilai ABO atau Rhesus pada hasil
tervalidasi terbaru **berbeda** dari hasil sah sebelumnya, sistem menahan: untuk sementara pasien itu
**tidak punya hasil golongan darah yang sah sama sekali**, dan seluruh gerbang yang menuntut golongan
darah sah ikut tertutup, sampai perbedaannya diselesaikan peran validator (`DEF-BD-004`). Kedua hasil
tetap tersimpan utuh; tidak ada yang dihapus maupun ditimpa.
*Alasan pemilihan.* Golongan darah seseorang tidak berubah. Hasil yang berbeda hampir selalu berarti
sampel tertukar atau salah catat — persis kejadian paling berbahaya bila lewat begitu saja.
Perilaku menahan ini juga menjaga `INV-BD-013`: sistem tidak memutuskan apa pun secara klinis, ia
berhenti dan memanggil manusia.
*Batas keputusan.* Sistem tidak menilai mana hasil yang benar dan tidak menyarankan pilihan. Ia hanya
menyatakan bahwa ada perbedaan, menahan pemakaiannya, lalu menyimpan siapa yang menyelesaikan
perbedaan itu beserta alasan dan waktunya. Keputusan ini mempertegas `INV-BD-015`, bukan
menggantikannya.
*Contoh.* Ny. R punya hasil tervalidasi O Positif dari kunjungan Januari. Pada kunjungan Mei, hasil
tervalidasi baru menyatakan A Positif. Sejak saat itu Ny. R tidak punya golongan darah sah. Alokasi
dan pemberian yang menuntut golongan darah sah tertahan sampai validator menyelesaikan perbedaan
tersebut. Riwayat kedua hasil tetap terbaca setelahnya.

**`DEC-BD-027` — Masa berlaku bukti kecocokan.**
Bukti kecocokan punya masa berlaku dalam satuan jam. Nilainya adalah **konfigurasi**, bukan angka
yang dikunci di dalam kode — pola yang sama dengan `DEC-BD-012`. Setelah masa berlakunya lewat,
gerbang pemberian tertutup kembali: bukti lama tetap tersimpan penuh sebagai riwayat, tetapi tidak
lagi membuka pemberian, dan pemberian menuntut bukti baru.
Sistem tidak menebak nilai itu dan tidak memakai bawaan "tanpa batas". Selama nilainya belum
ditetapkan pemilik proses klinis, gerbang pemberian bersifat **fail-closed**: pemberian ditahan, dan
sistem menyatakan bahwa konfigurasi masa berlaku belum ditetapkan.
Nilai jamnya sendiri **tidak diputuskan** pada sesi ini dan dicatat sebagai `OQ-BD-012`.
*Batas keputusan.* Keputusan ini mengatur kapan bukti berhenti berlaku, bukan bagaimana bukti
dihasilkan. `DEC-BD-013` tetap berlaku penuh — Quilvian mencatat titik pemeriksaan, tidak pernah
menghitung kompatibilitas.
*Contoh.* Kantong diuji kecocokan Senin pukul 16.00. Bila masa berlaku dikonfigurasi 48 jam,
pemberian pada Rabu pukul 10.00 masih diizinkan, sedangkan pemberian pada Kamis pukul 09.00 ditolak
dan menuntut bukti baru. Bukti Senin tetap terbaca pada riwayat kantong, hanya tidak lagi membuka
gerbang.

**`DEC-BD-028` — Bukti kecocokan gugur saat kantong dialihkan.**
Bukti kecocokan selalu bermakna "kantong ini cocok untuk pasien ini", bukan "kantong ini baik".
Karena itu, begitu kantong `PENDING_REVIEW` dialihkan ke pasien lain (`REALLOCATED`), seluruh bukti
kecocokan terhadap pasien asal **gugur otomatis**. Bukti lama tetap terbaca sebagai riwayat milik
pasien asal dan tidak dihapus. Gerbang pemberian untuk pasien tujuan tertutup sampai ada bukti
kecocokan baru terhadap pasien tujuan itu sendiri.
Pernyataan kelayakan oleh petugas berwenang tetap berjalan seperti yang sudah ditetapkan
`DEC-BD-019`, dan rantai riwayat pasien asal → alasan pelepasan → pasien tujuan tetap tidak
boleh putus.
*Batas keputusan.* Sistem tidak pernah menyimpulkan bahwa bukti lama "masih cocok" karena golongan
darah kedua pasien kebetulan sama. Penyimpulan semacam itu dilarang `INV-BD-011` dan `INV-BD-013`.
*Contoh.* Kantong PRC sudah diuji kecocokan untuk Tn. S. Tn. S kemudian pulang, dan kantong masuk
`PENDING_REVIEW`. Kantong dialihkan ke Ny. R. Bukti terhadap Tn. S langsung tidak berlaku. Pemberian
ke Ny. R ditolak sampai ada bukti kecocokan baru terhadap Ny. R — walaupun golongan darah keduanya
sama.

**`DEC-BD-029` — Pembatalan alokasi sebelum pemberian.**
Alokasi yang keliru boleh dibatalkan **petugas Bank Darah sendiri** selama kantongnya belum
diberikan, dengan alasan dari daftar alasan terkendali `DEC-BD-024`. Setelah dibatalkan:

- bila order asalnya masih aktif, kantong kembali menjadi tersedia;
- bila order asalnya sudah berakhir, kantong masuk `PENDING_REVIEW` sesuai `DEC-BD-007`;
- bukti kecocokan yang terlanjur tercatat gugur mengikuti `DEC-BD-028`, karena pasien tujuannya
  berubah.

Alokasi yang kantongnya **sudah** diberikan tidak dapat dibatalkan. Jalurnya adalah koreksi pada
`DEC-BD-030`.
*Alasan pemilihan.* Memilih kantong yang keliru adalah kekeliruan administratif biasa, bukan tindakan
klinis. Mengunci kantong sampai ada peran lebih tinggi menghambat pelayanan tanpa menambah
keselamatan sedikit pun.
*Batas keputusan.* Pembatalan tidak menghapus apa pun. Riwayat alokasi → pembatalan → alokasi
baru tersimpan utuh dan memang harus terbaca.
*Contoh.* Petugas mengalokasikan kantong `PMI-00871` ke order Tn. S, lalu menyadari kantong itu
seharusnya untuk Ny. R. Ia membatalkan alokasi dengan alasan "salah pilih kantong". Kantong kembali
tersedia dan dapat dialokasikan ke order Ny. R. Riwayat kantong menampilkan ketiga kejadian itu
berurutan, lengkap dengan pelaku dan waktunya.

**`DEC-BD-030` — Koreksi pencatatan pemberian.**
Pemberian **tidak pernah** dihapus dan **tidak pernah** dibalik. Kekeliruan pencatatan dikoreksi
lewat **catatan koreksi** tersendiri yang menunjuk pemberian asal dan menyimpan: apa yang keliru, apa
yang benar, alasan dari daftar terkendali, pelaku, dan waktu. Pemberian asal tetap terbaca selamanya.
Angka pemenuhan order dihitung ulang dengan menghormati catatan koreksi itu, tetap sesuai aturan
bahwa angka pemenuhan dihitung dari transaksi nyata dan bukan diketik. Wewenangnya ada pada peran
berwenang yang ditetapkan `DEF-BD-004`.
*Batas keputusan — penting.* Catatan koreksi **bukan** jalur untuk memindahkan darah yang sudah
diberikan ke pasien lain. Ia mencatat bahwa pencatatannya keliru; ia tidak menyatakan bahwa darahnya
tidak jadi masuk ke tubuh pasien. Pengalihan kantong hanya sah lewat jalur `REALLOCATED` pada kantong
yang belum diberikan.
*Alasan pemilihan.* Darah yang sudah masuk ke tubuh pasien adalah fakta klinis yang tidak boleh
hilang dari rekam jejak, apa pun kekeliruan pencatatannya. Pola "hanya bisa ditambah, tidak pernah
ditimpa" ini juga sudah dipakai repository lewat riwayat perpindahan status (`BD-CAP-009`).
*Yang belum ditetapkan.* Nasib kantong yang tercatat keliru sebagai diberikan — secara fisik
kantong itu mungkin masih ada — tidak ditetapkan otomatis oleh sistem dan dicatat sebagai
`OQ-BD-014`.
*Contoh.* Petugas mencatat pemberian dengan nomor kantong `PMI-00871`, padahal yang benar-benar
diberikan adalah `PMI-00817`. Ia membuat catatan koreksi yang menunjuk pemberian asal, menyebut nomor
kantong yang keliru dan yang benar, memilih alasan "salah nomor kantong", lalu menyimpannya.
Pemberian asal tetap terbaca; angka pemenuhan order Tn. S dihitung ulang; dan kantong `PMI-00871`
tidak diam-diam kembali menjadi tersedia.

### 8.9 Pertanyaan baru yang lahir dari pass ini

| ID | Isi | Pemilik | Memblokir |
| --- | --- | --- | --- |
| `OQ-BD-012` | Berapa jam masa berlaku bukti kecocokan, dan apakah nilainya sama untuk semua komponen darah. Bentuk aturannya sudah dikunci `DEC-BD-027`; yang belum ada hanya angkanya | Pemilik proses klinis | `IMPLEMENTATION` gerbang pemberian. **Tidak** memblokir `DESIGN` |
| `OQ-BD-013` | Di mana perbedaan hasil golongan darah diselesaikan. `DEC-BD-026` menuntut ada tempat menyelesaikannya, sedangkan `DEC-BD-023` sudah mengunci MVP pada tepat tiga daftar kerja | Pemilik proses BDRS | `DESIGN` satu layar saja. Usulan yang tidak memperluas scope: penyelesaian dilakukan di dalam layar pemeriksaan golongan darah, bukan sebagai daftar kerja keempat |
| `OQ-BD-014` | Keadaan kantong yang tercatat keliru sebagai diberikan, setelah pencatatannya dikoreksi. Secara fisik kantong itu mungkin masih ada | Pemilik proses BDRS | `IMPLEMENTATION` jalur koreksi. **Tidak** memblokir `DESIGN` catatan koreksinya |

`OQ-BD-013` dan bagian struktur `OQ-BD-012` sudah **ditutup** pada pass berikutnya — lihat §8.10.

### 8.10 Architecture gap final closure pass

Tiga gap arsitektur yang dibuka `03-domain-architecture.md` revisi 2 — `ARCH-BD-GAP-07`,
`ARCH-BD-GAP-08`, dan `ARCH-BD-GAP-09` — ditutup pada sesi ini, bersama `OQ-BD-013` dan bagian
struktur `OQ-BD-012`. Tidak ada satu pun keputusan sebelumnya yang dibuka ulang. Atas permintaan
pemilik, tujuh hal ini sengaja **tidak** dibuka ulang: PMI sebagai penyedia darah, MMC sebagai
pengguna sistem, unit pemesan MVP, pemenuhan sebagian, ketiadaan gerbang persetujuan pada alur
normal, HCLAB di luar MVP, dan laporan di luar MVP.

| Decision ID | Menutup | Type | Keputusan | Status |
| --- | --- | --- | --- | --- |
| `DEC-BD-031` | `ARCH-BD-GAP-07` | `Decision` | Konflik golongan darah diselesaikan lewat pemeriksaan ulang tervalidasi (Model C); sistem tidak menghitung mayoritas | `draft` |
| `DEC-BD-032` | `ARCH-BD-GAP-08`, struktur `OQ-BD-012` | `Decision` | Masa berlaku bukti kecocokan disimpan per komponen pada katalog komponen darah, dari konfigurasi | `draft` |
| `DEC-BD-033` | `OQ-BD-013` | `Decision` | Penyelesaian konflik dilakukan di dalam workflow pemeriksaan golongan darah, tanpa daftar kerja baru | `draft` |
| `DEC-BD-034` | `ARCH-BD-GAP-09` | `Decision` | Koreksi pencatatan tidak membalik biaya otomatis; keputusan peninjauan biaya milik Billing lewat `DEC-BD-016` | `draft` |

Owner keputusan: `DEC-BD-031` pemilik proses klinis · `DEC-BD-032` pemilik proses klinis bersama
pemilik proses BDRS · `DEC-BD-033` pemilik proses BDRS · `DEC-BD-034` pemilik proses BDRS bersama
pemilik BillingManagement. Seluruhnya berstatus `draft`. Dijawab dalam sesi wawancara tidak sama
dengan disetujui pejabat berwenang, dan `approved_by` beserta `approved_at` masih kosong.

### 8.11 Rincian keputusan architecture gap final closure pass

**`DEC-BD-031` — Penyelesaian konflik golongan darah lewat pemeriksaan ulang (Model C).**
Ketika hasil golongan darah tervalidasi terbaru berbeda ABO atau Rhesus-nya dari hasil sah
sebelumnya, hasil lama **tidak** ditimpa, pasien tetap berada dalam keadaan "golongan darah konflik",
dan pemakaian hasil itu untuk seluruh lifecycle Bank Darah **ditahan**. Konflik dianggap selesai
hanya bila empat syarat terpenuhi bersama: ada hasil pemeriksaan baru, sampel baru tercatat, hasil
baru tervalidasi petugas berwenang, dan validator menyatakan hasil itu yang berlaku.
*Batas keputusan.* Quilvian **tidak** menentukan hasil mana yang benar. Ia hanya mencatat histori
hasil sebelumnya, pemeriksaan ulang, keputusan validator, serta waktu dan pelakunya. Model mayoritas
"dua dari tiga" ditolak, karena menuntut sistem menghitung mayoritas — persis yang dilarang
`INV-BD-013`.
*Konsekuensi yang perlu ditegaskan.* Bila sampel ulang justru menghasilkan nilai ketiga yang berbeda
dari kedua hasil yang bentrok, nilai itu tetap boleh menjadi hasil sah **asalkan** validator
menyatakannya. Sistem tidak pernah memaksa hasil baru cocok dengan salah satu hasil lama.
*Contoh.* Ny. R punya hasil tervalidasi O Positif dari Januari. Pada Mei muncul hasil tervalidasi
baru A Positif, dan sejak itu Ny. R tidak punya golongan darah sah. Petugas mengambil sampel baru,
mencatat pemeriksaan ulang, dan hasilnya tervalidasi B Positif. Validator menyatakan hasil ketiga ini
yang berlaku. Sejak saat itu Ny. R punya satu golongan darah sah kembali; ketiga hasil — O Positif,
A Positif, B Positif — tetap terbaca sebagai riwayat.

**`DEC-BD-032` — Masa berlaku bukti kecocokan sebagai atribut per komponen.**
Masa berlaku bukti kecocokan disimpan sebagai atribut pada katalog komponen darah (`BD-DOM-13`), satu
nilai untuk tiap komponen. Sebagai gambaran, tiap komponen darah — misalnya PRC (*Packed Red Cells*,
sel darah merah pekat), TC (*Thrombocyte Concentrate*, konsentrat trombosit), dan FFP (*Fresh Frozen
Plasma*, plasma beku segar) — memikul atribut masa berlaku (nama kerja
`CompatibilityEvidenceValidityHours`; nama teknis final dibekukan pada fase perancangan). Nilai jamnya
**tidak** ditentukan sistem, melainkan oleh kebijakan klinis MMC. Quilvian hanya membaca dan
menerapkan konfigurasi itu, dan **tidak** pernah menyimpan angka masa berlaku yang ditanam di kode.
*Alasan pemilihan.* Setiap komponen darah dapat memiliki kebijakan klinis yang berbeda, sehingga
konfigurasi per komponen lebih luwes daripada satu nilai global.
*Batas keputusan.* Penambahan atribut ini **bukan** penambahan menu Setup baru. Ia tetap berada di
dalam katalog komponen darah yang sudah disepakati `DEC-BD-024`, sehingga scope Setup tidak melebar.
*Contoh.* PRC dikonfigurasi masa berlaku 48 jam, sedangkan TC dikonfigurasi 24 jam. Bukti kecocokan
sebuah kantong PRC yang diperiksa Senin pukul 16.00 masih membuka gerbang sampai Rabu pukul 16.00,
sedangkan kantong TC dengan bukti pada jam yang sama gerbangnya sudah tertutup sejak Selasa pukul
16.00. Keduanya dibaca dari katalog, bukan dari angka yang ditanam di kode.

**`DEC-BD-033` — Penyelesaian konflik di dalam workflow pemeriksaan golongan darah.**
Konflik hasil golongan darah diselesaikan di dalam alur pemeriksaan golongan darah (`BD-AGG-04`),
**bukan** lewat daftar kerja operasional terpisah. Layar pemeriksaan wajib menampilkan lima hal:
histori hasil sebelumnya, status konflik, hasil pemeriksaan baru, status validasi, dan tindakan
penyelesaian oleh validator. Penyelesaiannya mengikuti `DEC-BD-031`.
*Batas keputusan.* Tidak ada daftar kerja operasional keempat yang dibuat, sehingga `DEC-BD-023` yang
mengunci MVP pada tepat tiga daftar kerja tetap utuh. Pemantauan konflik ditopang dua hal: status
konflik yang melekat pada pemeriksaan golongan darah, dan mekanisme *fail-closed* yang menahan
pemakaian hasil konflik untuk proses Bank Darah — sehingga konflik tidak dapat terlewat diam-diam.
*Contoh.* Saat petugas hendak mengalokasikan kantong untuk seorang pasien yang golongan darahnya
sedang konflik, gerbang menolak sendiri karena pasien itu tidak punya golongan darah sah. Petugas
membuka layar pemeriksaan pasien tersebut, melihat status konflik, mencatat pemeriksaan ulang, dan
setelah hasil baru tervalidasi serta dinyatakan validator, alokasi dapat dilanjutkan.

**`DEC-BD-034` — Batas tanggung jawab koreksi terhadap biaya.**
Koreksi pencatatan Bank Darah **tidak** secara otomatis membalik atau mengubah fakta biaya. Batas
tanggung jawabnya tegas. Bank Darah: mencatat kejadian koreksi, mempertahankan histori pemberian
asli, menyediakan informasi koreksi bila diperlukan, dan **tidak** menentukan pembatalan atau koreksi
biaya. BillingManagement: menjadi pemilik keputusan apakah fakta biaya perlu ditinjau, dan menentukan
apakah charge tetap, dikoreksi, atau dibalik sesuai kebijakan Billing.
*Batas keputusan.* Tidak ada asumsi bahwa setiap koreksi Bank Darah otomatis mengubah biaya. Biaya
berasal dari **tindakan** (`DEC-BD-021`), bukan dari kantong, dan koreksi bersifat hanya-tambah
(`DEC-BD-030`). Bila Billing membutuhkan informasi koreksi, Bank Darah menyediakan kejadian atau
notifikasi domain sesuai kontrak yang disepakati pada `DEC-BD-016`. Keputusan peninjauan biaya untuk
kasus tepi — koreksi yang menghapus satu-satunya pemberian di bawah sebuah tindakan — tetap Open
Question milik Billing, menempel pada `DEC-BD-016`.
*Contoh.* Sebuah tindakan Bank Darah selesai dengan satu kantong diberikan, dan fakta biayanya sudah
terkirim ke Billing. Kemudian petugas membuat catatan koreksi yang menyatakan pemberian itu salah
catat. Bank Darah tidak menarik atau membalik biaya apa pun; ia hanya menerbitkan kejadian koreksi.
Apakah biaya tindakan itu ditinjau ulang diputuskan Billing, bukan Bank Darah.

### 8.12 Storage Location closure pass

Menutup coverage gap "Storage Location" yang dibuka roadmap `plan-module-delivery`. Owner kebutuhan
menetapkan Storage Location **masuk MVP**. Temuan sumber: master `MstDrugStorageLocation` sudah ada di
`Areas/HealthServices/MasterData/Models/` (tipe `ColdStorage`, rentang suhu, rak/shelf/bin) — capability
audit `BD-CAP-006` melewatkannya; celah cakupan audit, bukan basi. Master itu berorientasi farmasi dan
tidak dipakai.

| Decision ID | Menutup | Type | Keputusan | Status |
| --- | --- | --- | --- | --- |
| `DEC-BD-035` | Coverage gap Storage Location (ownership & scope) | `Decision` | Storage Location darah = master baru `MstBloodStorageLocation` milik BDRS; bukan reuse master farmasi; generalisasi ditunda POST-MVP | `draft` |
| `DEC-BD-036` | Coverage gap Storage Location (lifecycle) | `Decision` | Storage Location menjadi gerbang kesiapan operasional kantong: `RECEIVED` → `STORED` → tersedia → dialokasikan → diberikan | `draft` |

Owner: `DEC-BD-035` dan `DEC-BD-036` pemilik proses BDRS. Keduanya `draft`; `approved_by`/`approved_at`
kosong.

`DEC-BD-024` (Setup MVP tepat dua hal) **diamandemen**: Setup Bank Darah kini memuat **tiga** master —
katalog komponen darah, daftar alasan terkendali, dan master lokasi penyimpanan darah. Perluasan ini
disanksi owner lewat keputusan memasukkan Storage Location ke MVP; substansi `DEC-BD-024` lainnya tetap.

### 8.13 Rincian keputusan Storage Location closure pass

**`DEC-BD-035` — Master lokasi penyimpanan darah milik Bank Darah.**
Lokasi penyimpanan fisik kantong darah (contoh: Kulkas Besar, Kulkas Kecil) dicatat sebagai **master
baru `MstBloodStorageLocation`** yang dimiliki dan dikelola pemilik proses BDRS. Kulkas darah MMC adalah
fasilitas penyimpanan khusus Bank Darah; cold storage farmasi dan storage darah adalah dua konsep
berbeda — berbeda owner, aturan bisnis, dan lifecycle. Karena itu master farmasi `MstDrugStorageLocation`
**tidak** dipakai ulang.
*Scope MVP.* Hanya: master lokasi penyimpanan darah, penanda lokasi aktif/nonaktif, referensi lokasi ke
Blood Unit, dan perpindahan lokasi.
*Batas keputusan — di luar MVP.* Monitoring suhu, IoT, kapasitas storage, dan inventory warehouse umum
**tidak** termasuk. Karena master ini bebas dari flag/atribut farmasi, tidak ada rentang suhu yang ikut
terbawa. Generalisasi menjadi `MstStorageLocation` bersama dapat dievaluasi setelah MVP bila ada
kebutuhan lintas domain.
*Contoh.* Petugas BDRS mendaftarkan "Kulkas Besar" dan "Kulkas Kecil" sebagai lokasi aktif. Kulkas lama
yang rusak ditandai nonaktif sehingga tidak dapat dipilih untuk penyimpanan baru, tetapi kantong yang
sudah tercatat di sana tetap terbaca pada riwayat.

**`DEC-BD-036` — Storage Location sebagai gerbang kesiapan operasional kantong.**
Blood Unit memiliki lifecycle: `RECEIVED` → `STORED` → tersedia → dialokasikan → diberikan.
Ketentuannya: kantong yang baru diterima dari PMI masuk status `RECEIVED`; kantong **belum boleh
dialokasikan** sebelum memiliki lokasi penyimpanan; setelah petugas menetapkan lokasi, kantong menjadi
`STORED`; hanya kantong `STORED` yang dapat masuk proses ketersediaan/alokasi.
*Invariant.* Kantong tidak dapat dialokasikan apabila belum memiliki Storage Location atau belum melalui
proses penyimpanan (`INV-BD-025`). Storage Location bukan sekadar atribut informasi, melainkan bagian
kesiapan operasional kantong.
*Perpindahan lokasi.* Dicatat sebagai histori perpindahan yang hanya bertambah dan **tidak** mengubah
histori penerimaan awal kantong (`INV-BD-026`).
*Catatan pemodelan.* Apakah `STORED` dan keadaan tersedia/`Available` menjadi dua status terpisah atau
`STORED` sekaligus berarti tersedia untuk alokasi, ditetapkan pada pass `hospital-domain-architect` —
tanpa menciptakan status per atribut.
*Contoh.* Kantong `PMI-00912` diterima Senin pagi → `RECEIVED`. Petugas mencoba mengalokasikannya
langsung untuk Tn. S → ditolak karena belum punya lokasi. Petugas menaruhnya di "Kulkas Besar" dan
menetapkan lokasi → `STORED`. Kini kantong dapat dialokasikan. Selasa kantong dipindah ke "Kulkas
Kecil"; perpindahan tercatat, sedangkan catatan bahwa kantong diterima Senin pagi dari permintaan
asalnya tetap utuh.

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
| `AC-BD-031` | Permintaan PMI 2 kantong PRC, yang datang 3 kantong | Permintaan `FULFILLED` dengan sisa 0 — bukan minus 1. Ketiga kantong tercatat diterima dan membawa rujukan permintaan asal |
| `AC-BD-032` | Kantong ke-3 pada `AC-BD-031` | Masuk `PENDING_REVIEW` dengan alasan "kiriman melebihi permintaan", dan muncul pada daftar kerja pemantauan |
| `AC-BD-033` | Kantong berlebih itu dicoba dialokasikan langsung ke order pasien yang sama | Ditolak — wajib lewat penyelesaian `DEC-BD-019` lebih dulu |
| `AC-BD-034` | Pasien punya hasil tervalidasi O Positif, lalu muncul hasil tervalidasi baru A Positif | Pasien tidak punya hasil sah; gerbang yang menuntut golongan darah sah tertahan; kedua hasil tetap tersimpan |
| `AC-BD-035` | Hasil tervalidasi baru bernilai sama dengan hasil sah sebelumnya | Hasil terbaru berlaku tanpa penahanan apa pun |
| `AC-BD-036` | Perbedaan hasil pada `AC-BD-034` diselesaikan peran validator | Tepat satu hasil sah kembali berlaku; pelaku, alasan, dan waktu tersimpan; riwayat kedua hasil tetap terbaca |
| `AC-BD-037` | Perbedaan hasil dicoba diselesaikan oleh peran yang bukan validator | Ditolak |
| `AC-BD-038` | Bukti kecocokan tercatat, masa berlakunya sudah terlampaui, petugas menekan pemberian | Ditolak; bukti lama tetap tersimpan sebagai riwayat dan pemberian menuntut bukti baru |
| `AC-BD-039` | Bukti kecocokan tercatat dan masih di dalam masa berlaku | Pemberian berhasil |
| `AC-BD-040` | Nilai masa berlaku bukti kecocokan belum dikonfigurasi | Pemberian ditahan, dan sistem menyatakan konfigurasi masa berlaku belum ditetapkan — tidak memakai nilai bawaan tebakan |
| `AC-BD-041` | Kantong dengan bukti kecocokan terhadap pasien A dialihkan ke pasien B, lalu pemberian ke B dicoba | Ditolak; bukti terhadap A tidak lagi membuka gerbang, tetapi tetap terbaca sebagai riwayat pasien A |
| `AC-BD-042` | Setelah pengalihan itu, bukti kecocokan baru terhadap pasien B tercatat | Pemberian ke B berhasil |
| `AC-BD-043` | Petugas Bank Darah membatalkan alokasi dengan alasan terkendali, kantong belum diberikan, order asal masih aktif | Berhasil; kantong kembali tersedia; riwayat alokasi dan pembatalan tersimpan |
| `AC-BD-044` | Alokasi dibatalkan sementara order asalnya sudah berakhir | Kantong masuk `PENDING_REVIEW`, bukan menjadi tersedia |
| `AC-BD-045` | Pembatalan alokasi tanpa memilih alasan dari daftar terkendali | Ditolak |
| `AC-BD-046` | Alokasi yang kantongnya sudah diberikan dicoba dibatalkan | Ditolak — jalurnya adalah catatan koreksi `DEC-BD-030` |
| `AC-BD-047` | Catatan koreksi pemberian dibuat peran berwenang dengan alasan terisi | Berhasil; pemberian asal tetap terbaca; angka pemenuhan order dihitung ulang |
| `AC-BD-048` | Pencatatan pemberian dicoba dihapus atau dianulir | Ditolak — satu-satunya jalur adalah catatan koreksi |
| `AC-BD-049` | Catatan koreksi dipakai untuk memindahkan pemberian ke pasien lain | Ditolak — koreksi mencatat kekeliruan pencatatan, bukan memindahkan darah antarpasien |
| `AC-BD-050` | Catatan koreksi dibuat oleh peran yang tidak berwenang | Ditolak |
| `AC-BD-051` | Perbedaan hasil golongan darah dicoba diselesaikan tanpa mencatat pemeriksaan ulang baru | Ditolak — penyelesaian menuntut pemeriksaan ulang tervalidasi (`DEC-BD-031`) |
| `AC-BD-052` | Pemeriksaan ulang tervalidasi tercatat dan validator menyatakan hasil baru yang berlaku | Berhasil; satu hasil sah kembali berlaku; histori seluruh hasil tetap terbaca |
| `AC-BD-053` | Pemeriksaan ulang menghasilkan nilai ketiga yang berbeda dari dua hasil bentrok, validator menyatakannya berlaku | Diterima — sistem tidak memaksa hasil baru cocok dengan salah satu hasil lama |
| `AC-BD-054` | Konflik dicoba ditutup dengan sistem memilih otomatis hasil "mayoritas" | Ditolak — sistem tidak menghitung mayoritas atau memutus klinis |
| `AC-BD-055` | Komponen PRC dan TC dikonfigurasi masa berlaku bukti kecocokan berbeda | Kedua nilai diterapkan sesuai komponennya masing-masing, dibaca dari katalog |
| `AC-BD-056` | Nilai masa berlaku dicoba ditanam di kode, bukan dibaca dari konfigurasi katalog | Ditolak — melanggar `INV-BD-023` |
| `AC-BD-057` | Konflik golongan darah muncul; petugas mencari daftar kerja operasional keempat untuk menyelesaikannya | Tidak ada; penyelesaian dilakukan di dalam layar pemeriksaan golongan darah (`DEC-BD-033`) |
| `AC-BD-058` | Koreksi pencatatan pemberian dibuat; sistem mencoba otomatis membalik fakta biaya tindakan | Ditolak — koreksi tidak mengubah biaya; keputusan peninjauan milik Billing (`DEC-BD-034`) |
| `AC-BD-059` | Kantong baru diterima dari PMI | Status `RECEIVED`; belum dapat dialokasikan |
| `AC-BD-060` | Kantong `RECEIVED` (belum punya lokasi) dicoba dialokasikan | Ditolak — kantong harus memiliki lokasi penyimpanan dan berstatus `STORED` lebih dulu (`INV-BD-025`) |
| `AC-BD-061` | Petugas menetapkan lokasi penyimpanan pada kantong `RECEIVED` | Kantong menjadi `STORED` dan dapat masuk proses ketersediaan/alokasi |
| `AC-BD-062` | Lokasi penyimpanan nonaktif dipilih untuk penyimpanan kantong baru | Ditolak — hanya lokasi aktif yang dapat dipilih |
| `AC-BD-063` | Kantong `STORED` dipindahkan ke lokasi lain | Perpindahan tercatat sebagai histori; histori penerimaan awal tetap utuh (`INV-BD-026`) |
| `AC-BD-064` | Sistem diminta mencatat suhu atau kapasitas storage pada MVP | Tidak ada — monitoring suhu, IoT, dan kapasitas di luar scope MVP (`DEC-BD-035`) |

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
| `OQ-BD-012` | Berapa jam masa berlaku bukti kecocokan per komponen. Struktur penyimpanannya ditutup `DEC-BD-032` (per komponen di katalog); yang tersisa hanya angka jamnya dari kebijakan klinis MMC | Pemilik proses klinis | `IMPLEMENTATION` gerbang pemberian. **Tidak** memblokir `DESIGN` |
| `OQ-BD-014` | Keadaan kantong yang tercatat keliru sebagai diberikan, setelah pencatatannya dikoreksi | Pemilik proses BDRS | `IMPLEMENTATION` jalur koreksi |

### Pertanyaan yang sudah tertutup

Scope pass: `OQ-BD-002` sampai `OQ-BD-008`.
Closure pass: `DEC-BD-013`, `DEC-BD-014`, `DEC-BD-015`, `DEC-BD-017` sampai `DEC-BD-024`, serta
`DEF-BD-001` dan `DEF-BD-002`.
Architecture gap closure pass: `ARCH-BD-GAP-01` sampai `ARCH-BD-GAP-06`, ditutup berturut-turut
oleh `DEC-BD-025` sampai `DEC-BD-030`.
Architecture gap final closure pass: `ARCH-BD-GAP-07`, `ARCH-BD-GAP-08`, `ARCH-BD-GAP-09`, dan
`OQ-BD-013`, ditutup oleh `DEC-BD-031` sampai `DEC-BD-034`. Bagian struktur `OQ-BD-012` ditutup
`DEC-BD-032`; sisa angka jamnya tetap terbuka sebagai masukan konfigurasi.

---

## 11. Langkah Berikutnya

**Storage Location closure pass (terbaru).** `DEC-BD-035` dan `DEC-BD-036` menutup coverage gap Storage
Location yang dibuka roadmap. Karena keputusan ini menambah konsep domain baru (`MstBloodStorageLocation`)
dan mengubah lifecycle kantong (`RECEIVED` → `STORED`), tiga artefak hilir **belum tersinkron** dan perlu
pass lanjutan sebelum implementasi:
- `03-domain-architecture.md` — pass ulang `hospital-domain-architect` untuk menyerap konsep master
  lokasi, memperluas lifecycle `BD-AGG-03`, dan menambahkan `INV-BD-025`/`INV-BD-026`.
- `02-backend-architecture.md`, `data/`, `contracts/`, `03-frontend-architecture.md` — pass ulang
  `design-business-module` untuk menambahkan `MstBloodStorageLocation`, status `RECEIVED`/`STORED`, kolom
  lokasi pada kantong, serta endpoint penetapan & perpindahan lokasi.
- `roadmap/00-delivery-plan.md` — Storage Location pindah dari coverage gap ke task P0.

Amandemen `DEC-BD-024`: Setup Bank Darah kini tiga master (komponen, alasan, lokasi penyimpanan).

Enam gap arsitektur `ARCH-BD-GAP-01` sampai `ARCH-BD-GAP-06` sudah tertutup oleh `DEC-BD-025` sampai
`DEC-BD-030`. Dengan itu, dua slice yang semula berhenti karena alasan keselamatan — jalur
pemberian dan jalur pengalihan pada `BD-AGG-03`, serta aturan hasil mana yang berlaku pada
`BD-AGG-04` — sudah punya aturan bisnisnya, termasuk jalur pembatalan alokasi dan jalur koreksi
pemberian.

**Yang masih terbuka dan pemiliknya.**

| ID | Memblokir | Apakah menahan perancangan? |
| --- | --- | --- |
| `DEC-BD-016` | Penyerahan fakta biaya ke Billing | Ya, hanya bagian penyerahan biayanya |
| `OQ-BD-011` | Mekanik label golongan darah | Ya, hanya slice label |
| `DEF-BD-003` | Aturan bukti kecocokan per komponen | Tidak. `IMPLEMENTATION` saja |
| `DEF-BD-004` | Peran jalur darurat, validator, dan pembuat catatan koreksi | Tidak. `IMPLEMENTATION` saja |
| `OQ-BD-010` | Kesediaan PMI menerima pengembalian | Tidak |
| `OQ-BD-012` | Nilai jam masa berlaku bukti kecocokan per komponen (struktur ditutup `DEC-BD-032`) | Tidak. `IMPLEMENTATION` saja |
| `OQ-BD-013` | Tempat penyelesaian perbedaan hasil golongan darah | **Ditutup** `DEC-BD-033` — di layar pemeriksaan golongan darah |
| `OQ-BD-014` | Keadaan kantong setelah koreksi pemberian | Tidak. `IMPLEMENTATION` saja |
| `BD-DEP-008` | Pendaftaran registry kepemilikan modul dan prefix | Tidak. `IMPLEMENTATION` backend |
| `BD-DEP-009` | Tiga berkas bukti kebutuhan yang hilang | Tidak |

**Yang sudah dikerjakan sesudah pass ini.** Pass ulang `hospital-domain-architect` dijalankan pada
hari yang sama dan menghasilkan `03-domain-architecture.md` revisi 2, yang menyerap `DEC-BD-025`
sampai `DEC-BD-030` ke dalam `BD-AGG-02`, `BD-AGG-03`, dan `BD-AGG-04`. Statusnya tetap
`DOMAIN_ARCHITECTURE_PARTIAL`, dengan dua hal yang berhenti: satu perpindahan pada `BD-AGG-04` dan
satu kumpulan atribut pada `BD-DOM-13`.

**Architecture gap final closure pass.** Pass arsitektur itu memunculkan tiga gap baru yang kini
sudah **ditutup** pada sesi `grill-me` lanjutan ini: `ARCH-BD-GAP-07` ditutup `DEC-BD-031` (Model C
pemeriksaan ulang), `ARCH-BD-GAP-08` ditutup `DEC-BD-032` (atribut masa berlaku per komponen), dan
`ARCH-BD-GAP-09` ditutup `DEC-BD-034` (batas koreksi terhadap biaya). Ditambah `OQ-BD-013` ditutup
`DEC-BD-033` (penyelesaian konflik di layar pemeriksaan). Dengan begitu **dua hal yang menahan
`BD-AGG-04` dan `BD-DOM-13` sudah terselesaikan**: perpindahan penyelesaian konflik kini punya
prasyarat yang dapat diuji, dan kumpulan atribut katalog komponen darah sudah boleh dibekukan.

**Langkah berikutnya yang disarankan.** Jalankan ulang `hospital-domain-architect` untuk menyerap
`DEC-BD-031` sampai `DEC-BD-034` dan menilai apakah slice yang di dalam scope naik ke
`DOMAIN_ARCHITECTURE_READY`, lalu lanjut ke `design-business-module`. Perbarui juga
`blueprint-manifest.md` dan `MODULE-STATUS.md` lewat `manage-module-blueprint` agar `decision_revision`
dan daftar blocker ikut tersinkron. Yang tetap di luar scope dan tidak dibuka ulang: penyerahan biaya
ke Billing (`DEC-BD-016`) dan mekanik label golongan darah (`OQ-BD-011`).
