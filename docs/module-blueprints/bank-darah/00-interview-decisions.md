# Bank Darah — Interview Decisions

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` |
| Revision | `8` |
| Decision revision | `8` |
| Status | `draft` |
| Pass yang sudah dijalankan | Scope pass (2026-09-02), Closure pass (2026-09-02), Architecture gap closure pass (2026-09-02), Architecture gap final closure pass (2026-09-02), Storage Location closure pass (2026-09-02), Storage Location decision closure pass (2026-09-02), Gerbang pemberian closure pass (2026-09-02), Role & authority closure pass (2026-09-02) |
| Product/domain owner | Pemilik proses Bank Darah / BDRS — nama pejabat berwenang belum disebutkan |
| Backend SHA | `792acb9331a65187d052fffd4a292d3bce2fd828` cabang `sukmagp` |
| Backend SHA pada revisi 5 | `9dc7637adbafb321ad8078d5c52ebe5e4398fe86`. Perbedaan sampai `792acb9` **hanya** dokumen blueprint Bank Darah, nol berkas source aplikasi — sudah diperiksa dengan `git diff --name-only` |
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
| **Dokter BDRS / penanggung jawab klinis** | Penanggung jawab tindakan Bank Darah. Sejak `DEF-BD-004` ditutup, ia memegang tiga wewenang: **menyelesaikan konflik hasil golongan darah**, **menerbitkan otorisasi darurat** (bersama DPJP), dan **menyetujui koreksi pencatatan pemberian**. **Bukan** penahan alur normal — validasi hasil rutin tidak melewatinya. |
| **Petugas BDRS berwenang validasi** | Petugas Bank Darah yang ditunjuk memvalidasi hasil pemeriksaan golongan darah **rutin**. Bukan peran baru di luar BDRS; ia petugas BDRS dengan butir hak akses tambahan. Tidak berwenang menutup konflik. |
| **DPJP pasien** | Dokter penanggung jawab pasien. Menanggung risiko klinis transfusi, dan sejak `DEF-BD-004` berwenang menerbitkan otorisasi darurat untuk pasiennya. |
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
| `INV-BD-027` | Setiap penempatan baru wajib menunjuk lokasi penyimpanan yang sedang aktif. Berlaku untuk penempatan pertama kantong yang baru diterima maupun untuk lokasi tujuan pada perpindahan. Lokasi yang nonaktif tidak pernah dapat menjadi tujuan penyimpanan. | `DEC-BD-037` |
| `INV-BD-028` | Kantong tidak dapat dialokasikan selama penempatan terakhirnya menunjuk lokasi penyimpanan yang nonaktif. Penonaktifan lokasi tidak memindahkan kantong dan tidak mengubah statusnya; ia menutup gerbang alokasi sampai petugas BDRS memindahkan kantong ke lokasi aktif. | `DEC-BD-037` |
| `INV-BD-029` | Pemberian lewat jalur normal wajib memenuhi tiga syarat sekaligus, dinilai ulang pada saat pemberian dicoba: lokasi penyimpanan terakhir kantong sedang aktif, kantong sudah melewati `STORED`, dan bukti kecocokan berlaku untuk pasien tujuan serta belum lewat masa berlakunya. Hasil pemeriksaan pada saat alokasi tidak pernah diwariskan sebagai izin memberikan. | `DEC-BD-038` |
| `INV-BD-031` | Validasi hasil golongan darah rutin dan penyelesaian konflik hasil adalah **dua wewenang terpisah**. Petugas BDRS berwenang validasi dapat memvalidasi hasil rutin; hanya validator klinis yang ditunjuk (Dokter BDRS / penanggung jawab klinis) dapat menutup konflik. | `DEC-BD-039` |
| `INV-BD-032` | Otorisasi darurat wajib menyimpan alasan terkendali, keterangan kondisi kedaruratannya, pelaku **beserta peran yang dipakainya**, waktu, pasien, dan kantong. Otorisasi darurat tidak pernah diterbitkan sistem dan tidak pernah menjadi keputusan otomatis. | `DEC-BD-040` |
| `INV-BD-033` | Koreksi pencatatan pemberian berlaku **hanya setelah disetujui**. Koreksi yang masih menunggu persetujuan tidak mengubah angka pemenuhan order dan tidak mengubah apa pun pada rekam. Riwayat menyimpan peminta, penyetuju, waktu keduanya, dan alasannya. | `DEC-BD-041` |
| `INV-BD-030` | Otorisasi darurat wajib menyatakan gerbang mana yang dilewatinya — bukti kecocokan, lokasi penyimpanan nonaktif, atau keduanya. Penanda darurat tidak pernah berdiri tanpa keterangan itu. | `DEC-BD-038`, `DEC-BD-017` |

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
| `RECEIVED` | Tetapkan lokasi penyimpanan | `STORED` | Lokasi dipilih dari master lokasi penyimpanan darah yang **sedang aktif** (`DEC-BD-035`, `INV-BD-027`) |
| `STORED` | Siap dialokasikan | Tersedia di Bank Darah | Hanya kantong `STORED` yang dapat masuk proses ketersediaan/alokasi (`INV-BD-025`) |
| `STORED` / Tersedia / Dialokasikan | Pindahkan lokasi penyimpanan | Status tidak berubah; lokasi diperbarui | Lokasi tujuan wajib **sedang aktif** (`INV-BD-027`). Perpindahan dicatat sebagai histori; histori penerimaan awal tetap utuh (`INV-BD-026`) |
| `STORED` / Tersedia / Dialokasikan | Lokasi penyimpanannya dinonaktifkan | **Status tidak berubah**; kantong tetap tercatat di lokasi itu | Kantong **tidak** dipindahkan sistem dan **tidak** masuk `PENDING_REVIEW`. Selama masih tercatat di lokasi nonaktif, kantong tidak dapat dialokasikan (`INV-BD-028`, `DEC-BD-037`) |
| Kantong di lokasi nonaktif | Petugas BDRS memindahkan ke lokasi aktif | Status tidak berubah; lokasi diperbarui | Jalur perpindahan biasa. Setelah tercatat, gerbang alokasi terbuka kembali (`DEC-BD-037`) |
| Tersedia | Dialokasikan | Dialokasikan | Order masih aktif, tidak ada alokasi bertentangan, dan **lokasi penempatan terakhir kantong sedang aktif** (`INV-BD-025`, `INV-BD-028`) |
| Dialokasikan | Diberikan ke pasien | Diberikan | Tiga syarat sekaligus, dinilai ulang saat pemberian: lokasi penyimpanan terakhir **sedang aktif**, kantong sudah melewati `STORED`, dan bukti kecocokan berlaku (`INV-BD-029`, `DEC-BD-013`, `DEC-BD-038`) |
| Dialokasikan, lokasi penyimpanannya nonaktif | Dicoba diberikan lewat jalur normal | **Ditolak**; tetap Dialokasikan | Gerbang tertutup walaupun bukti kecocokan masih berlaku. Petugas memindahkan kantong ke lokasi aktif lebih dulu (`INV-BD-029`, `DEC-BD-038`) |
| Dialokasikan | Diberikan lewat jalur darurat | Diberikan, ditandai melewati gerbang | Otorisasi peran berwenang, alasan wajib (`DEC-BD-017`). Penanda wajib menyebutkan gerbang mana yang dilewati: bukti kecocokan, lokasi nonaktif, atau keduanya (`INV-BD-030`) |
| Tersedia / Dialokasikan | Order berakhir | `PENDING_REVIEW` | Tidak dapat dipakai siapa pun sampai diselesaikan |
| `PENDING_REVIEW` | Dialihkan ke pasien lain | `REALLOCATED` | Kelayakan dinyatakan petugas berwenang, alasan wajib. Bukti kecocokan terhadap pasien asal gugur otomatis; pasien tujuan wajib punya bukti sendiri (`DEC-BD-028`) |
| `PENDING_REVIEW` | Dikembalikan ke PMI | `RETURNED_TO_PROVIDER` | Bila proses bisnis PMI mendukung |
| `PENDING_REVIEW` | Dinyatakan tidak layak | `NOT_USABLE` | Kelayakan dinyatakan petugas berwenang, alasan wajib |
| Dialokasikan | Batalkan alokasi | Tersedia, atau `PENDING_REVIEW` bila order asal sudah berakhir | Kantong belum diberikan; alasan dari daftar terkendali; bukti kecocokan yang terlanjur tercatat gugur (`DEC-BD-029`) |
| Dialokasikan, bukti lengkap | Masa berlaku bukti terlampaui | Dialokasikan, bukti tidak lagi berlaku | Terjadi karena waktu berjalan. Gerbang pemberian tertutup kembali; bukti lama tetap tersimpan (`DEC-BD-027`) |
| Diberikan | **Ajukan** koreksi pencatatan | Tetap `Diberikan`; koreksi berstatus menunggu persetujuan | **Petugas BDRS** (`DEC-BD-041`); alasan, data yang dikoreksi, dan bukti pendukung wajib. **Belum berlaku** — angka pemenuhan order belum bergerak (`INV-BD-033`) |
| Diberikan, koreksi menunggu persetujuan | **Setujui** koreksi | Tetap `Diberikan`, dengan catatan koreksi melekat padanya | **Dokter BDRS** (`DEC-BD-041`), bukan peminta yang sama; pemberian asal tidak dihapus; angka pemenuhan order dihitung ulang sejak persetujuan (`DEC-BD-030`, `INV-BD-033`) |
| Diberikan, koreksi menunggu persetujuan | **Tolak** koreksi | Tetap `Diberikan`; koreksi ditolak dan tetap terbaca | Dokter BDRS; alasan penolakan wajib. Rekam tidak berubah sama sekali |

**Catatan penamaan status.** Tabel di atas memakai sebutan Bahasa Indonesia yang dipakai sejak
revisi awal. `DEC-BD-036` membakukan nama rantai utamanya menjadi `RECEIVED` → `STORED` → `AVAILABLE`
→ `ALLOCATED` → `ISSUED`. Pemetaannya: "Tersedia di Bank Darah" = `AVAILABLE`, "Dialokasikan" =
`ALLOCATED`, "Diberikan" = `ISSUED`. `PENDING_REVIEW`, `REALLOCATED`, `RETURNED_TO_PROVIDER`, dan
`NOT_USABLE` tidak berubah. Dokumen turunan memakai nama baku tersebut.

### 5.4 Pemeriksaan golongan darah Bank Darah

| Dari status | Tindakan | Ke status | Syarat |
| --- | --- | --- | --- |
| — | Ambil sampel | Sampel tercatat | Rujukan pasien, waktu, petugas pengambil, identifier sampel |
| Sampel tercatat | Catat hasil ABO dan Rhesus | Hasil tercatat | Pemeriksa dan waktu pemeriksaan tersimpan |
| Hasil tercatat | Validasi hasil | Hasil tervalidasi | Petugas BDRS yang diberi kewenangan validasi (`DEC-BD-039`) |
| Hasil tervalidasi | Muncul hasil tervalidasi baru yang **berbeda** ABO atau Rhesus-nya | Perbedaan tertahan — pasien tidak punya hasil sah | Terjadi otomatis. Ditutup hanya oleh peran validator (`DEC-BD-026`) |
| Perbedaan tertahan | Catat pemeriksaan ulang | Perbedaan masih tertahan | Petugas Bank Darah. Sampel baru dan hasil baru tercatat, lalu divalidasi (`DEC-BD-031`) |
| Perbedaan tertahan, ada hasil baru tervalidasi | Selesaikan perbedaan | Satu hasil sah kembali berlaku | **Validator klinis yang ditunjuk** — Dokter BDRS / penanggung jawab klinis (`DEC-BD-039`) menyatakan hasil baru itu yang berlaku. Wajib ada pemeriksaan ulang tervalidasi; alasan, pelaku, dan waktu tersimpan; seluruh hasil tetap terbaca (`DEC-BD-031`) |

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
| `DEF-BD-004` | ~~Peran mana yang berhak memakai jalur darurat, dan peran mana yang berhak memvalidasi hasil golongan darah~~ | **Ditutup** Role \& authority closure pass oleh `DEC-BD-039`, `DEC-BD-040`, `DEC-BD-041` |

`DEF-BD-001` ditutup oleh `DEC-BD-019`. `DEF-BD-002` ditutup oleh `DEC-BD-020`. `DEF-BD-004` ditutup oleh `DEC-BD-039` sampai `DEC-BD-041`; yang tersisa pada tabel ini hanya `DEF-BD-003`.

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
bahwa angka pemenuhan dihitung dari transaksi nyata dan bukan diketik. Wewenangnya ditetapkan
`DEC-BD-041`: petugas BDRS mengajukan, Dokter BDRS menyetujui, dan angka pemenuhan baru bergerak
setelah persetujuan turun (`INV-BD-033`).
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

### 8.14 Storage Location decision closure pass

Menutup `ARCH-BD-GAP-10`, satu-satunya gap arsitektur yang dibuka pass `hospital-domain-architect`
revisi 4. Gap itu berbunyi: bukti yang ada saat itu hanya mengatur **pilihan ke depan** ketika sebuah
lokasi penyimpanan dinonaktifkan, dan diam soal nasib kantong yang masih berada di dalamnya.
Akibatnya, kulkas yang dinonaktifkan karena rusak tetap menyerahkan darahnya untuk dialokasikan.

Pemilik proses BDRS memutuskan jawabannya. Keputusan itu dicatat di sini sebagai `DEC-BD-037`.

| Decision ID | Menutup | Type | Keputusan | Owner | Status | Approved by/at |
| --- | --- | --- | --- | --- | --- | --- |
| `DEC-BD-037` | `ARCH-BD-GAP-10` — nasib kantong di lokasi penyimpanan yang dinonaktifkan | `Decision` | Lokasi nonaktif tidak menerima penyimpanan baru; kantong existing **tidak** otomatis dipindahkan; perpindahan dilakukan petugas BDRS; sistem hanya melakukan enforcement lewat status lokasi aktif | Pemilik proses BDRS | `draft` | kosong |

Turunannya: `INV-BD-027`, `INV-BD-028`, dan `AC-BD-065` sampai `AC-BD-070`.

**Catatan penomoran.** Keputusan ini diberikan pemilik kebutuhan lebih dulu dalam sesi
`hospital-domain-architect`, dan arsitektur revisi 5 memakai ID `DEC-BD-037`, `INV-BD-027`, serta
`INV-BD-028` sebagai nomor **sementara**. Pass ini mengukuhkan ketiga nomor tersebut apa adanya —
tidak ada tabrakan dengan ID yang sudah terpakai, karena register sebelumnya berhenti di `DEC-BD-036`,
`INV-BD-026`, dan `AC-BD-064`. Dengan pencatatan ini, ketiganya berhenti bersifat sementara.

**Status persetujuan.** `DEC-BD-037` berstatus `draft` dengan `approved_by` dan `approved_at` kosong,
sama seperti `DEC-BD-001` sampai `DEC-BD-036`. Dijawab dalam sesi tidak sama dengan disetujui pejabat
berwenang.

### 8.15 Rincian keputusan Storage Location decision closure pass

**`DEC-BD-037` — perlakuan terhadap lokasi penyimpanan yang dinonaktifkan.**

Ketika petugas BDRS menonaktifkan sebuah lokasi penyimpanan darah, empat hal berlaku:

1. **Lokasi berhenti menerima penyimpanan baru.** Ia tidak lagi muncul sebagai tujuan penempatan mana
   pun — baik penempatan pertama kantong yang baru diterima, maupun tujuan perpindahan kantong yang
   sudah tersimpan (`INV-BD-027`).
2. **Kantong yang sudah ada di dalamnya tidak dipindahkan sistem.** Tidak ada perpindahan otomatis,
   tidak ada perubahan status, dan tidak ada kantong yang dilempar ke `PENDING_REVIEW`. Riwayat
   lokasinya tetap utuh dan tetap terbaca.
3. **Perpindahan dilakukan petugas BDRS**, lewat proses perpindahan lokasi yang sudah ada. Keputusan
   memindahkan darah secara fisik adalah kewenangan operasional BDRS, bukan kewenangan sistem.
4. **Enforcement sistem hanya lewat status lokasi aktif.** Selama penempatan terakhir sebuah kantong
   menunjuk lokasi nonaktif, kantong itu tidak dapat dialokasikan (`INV-BD-028`). Begitu petugas
   memindahkannya ke lokasi aktif dan perpindahan tercatat, gerbangnya terbuka kembali.

*Kenapa sistem tidak memindahkan sendiri.* Menonaktifkan sebuah penanda di layar tidak memindahkan
darah yang ada di dalam kulkas. Sistem yang mencatat perpindahan yang tidak pernah terjadi akan
berbohong tentang letak barang, dan itu persis kebohongan yang dicegah `INV-BD-026`. Yang bisa
dilakukan sistem dengan jujur hanyalah berhenti menawarkan kantong itu untuk dialokasikan sampai ada
manusia yang benar-benar memindahkannya.

*Kenapa gerbangnya alokasi, bukan status.* Menaruh kantong ke `PENDING_REVIEW` secara otomatis pernah
dipertimbangkan dan ditolak: penonaktifan lokasi bisa terjadi karena alasan biasa — penataan ulang,
penggantian nama, penggabungan ruangan — dan melempar seluruh isinya ke jalur penyelesaian akan
menimbulkan pekerjaan administratif besar tanpa ada yang benar-benar salah pada darahnya. Menutup
gerbang alokasi menahan risikonya tanpa mengubah keadaan kantong.

*Batas keputusan.* `DEC-BD-037` mengatur gerbang **alokasi**. Ia tidak mengatur pemberian kantong yang
terlanjur dialokasikan sebelum lokasinya dinonaktifkan — lihat `OQ-BD-015`. Ia juga tidak menambah
pemantauan suhu, kapasitas, atau IoT; batas `DEC-BD-035` tetap berlaku, dan catatan lokasi tetap
merupakan bukti **penempatan**, bukan bukti rantai dingin terjaga.

*Turunan yang mengikuti langsung dari model, bukan aturan tambahan.* Pengalihan kantong ke pasien lain
(`PENDING_REVIEW` → `REALLOCATED`) adalah pengikatan kantong ke baris kebutuhan seorang pasien —
yaitu alokasi dengan nama lain. Karena itu `INV-BD-028` berlaku juga di sana.

*Contoh.* "Kulkas Lama" rusak dan ditandai nonaktif. Tiga kantong sedang berada di dalamnya. Ketiganya
tetap tercatat di "Kulkas Lama" dan statusnya tidak berubah, tetapi tidak satu pun dapat dialokasikan.
Petugas mencoba memindahkan salah satunya ke "Kulkas Lama" yang lain — ditolak, karena tujuan wajib
lokasi aktif. Petugas memindahkan ketiganya ke "Kulkas Besar"; perpindahan tercatat lengkap dengan
pelaku dan waktu, dan ketiganya kembali dapat dialokasikan. Riwayat tetap menunjukkan bahwa ketiganya
pernah berada di "Kulkas Lama", dan sejak kapan sampai kapan.

### 8.16 Gerbang pemberian closure pass

Menutup `OQ-BD-015`, pertanyaan yang dibuka Storage Location decision closure pass: apakah gerbang
lokasi nonaktif ikut berlaku pada **pemberian** kantong yang terlanjur dialokasikan sebelum lokasinya
dinonaktifkan.

`DEC-BD-037` hanya menyebut alokasi, dan arsitektur menerapkannya persis sejauh itu — sehingga pada
revisi 5 sistem masih mengizinkan kantong dari kulkas nonaktif diberikan asalkan alokasinya sudah
terjadi lebih dulu. Pemilik proses menutup celah itu.

| Decision ID | Menutup | Type | Keputusan | Owner | Status | Approved by/at |
| --- | --- | --- | --- | --- | --- | --- |
| `DEC-BD-038` | `OQ-BD-015` — perluasan gerbang lokasi nonaktif ke jalur pemberian | `Decision` | Kantong yang sudah dialokasikan tetapi berada di lokasi nonaktif **tidak boleh diberikan lewat jalur normal**. Gerbang pemberian memastikan tiga hal: lokasi terakhir aktif, kantong sudah melewati `STORED`, dan bukti kecocokan berlaku. Keadaan darurat tetap mengikuti `DEC-BD-017` | Pemilik proses BDRS bersama pemilik proses klinis | `draft` | kosong |

Turunannya: `INV-BD-029`, `INV-BD-030`, dan `AC-BD-072` sampai `AC-BD-076`.

### 8.17 Rincian keputusan gerbang pemberian closure pass

**`DEC-BD-038` — gerbang pemberian memuat gerbang alokasi.**

*Jalur normal.* Pemberian lewat jalur normal menuntut tiga syarat sekaligus:

1. lokasi penyimpanan terakhir kantong sedang **aktif**;
2. kantong sudah melewati `STORED`;
3. bukti kecocokan berlaku untuk pasien tujuan dan belum lewat masa berlakunya.

Ketiganya dinilai **pada saat pemberian dicoba**, bukan diwarisi dari saat alokasi (`INV-BD-029`).
Ini yang membuat keputusan bekerja: lokasi bisa saja masih aktif ketika kantong dialokasikan, lalu
dinonaktifkan sesudahnya. Bila gerbangnya hanya diperiksa saat alokasi, kasus itu lolos begitu saja —
dan justru kasus itulah yang dipersoalkan `OQ-BD-015`.

*Alasan pemilik proses.* Penonaktifan lokasi dapat menandakan masalah operasional atau fasilitas.
Sistem tidak boleh menganggap sebuah kantong aman hanya karena kantong itu sudah dialokasikan lebih
dulu. Batasnya tetap sama seperti `INV-BD-013`: sistem **tidak** menentukan kelayakan darah; yang
dilakukannya adalah menegakkan status lokasi.

*Jalur darurat.* `DEC-BD-017` tetap berlaku tanpa perubahan bentuk. Bila secara klinis kantong harus
diberikan sebelum sempat dipindahkan, pemberian dilakukan lewat otorisasi darurat: peran berwenang,
alasan wajib, pelaku dan waktu tercatat, penanda melekat permanen. Ini **bukan** bypass biasa dan
tidak boleh disajikan sebagai jalan pintas yang setara dengan jalur normal.

*Akibat pada penanda darurat — turunan, bukan aturan tambahan.* Sebelum keputusan ini, penanda darurat
hanya punya satu arti: darah keluar sebelum bukti kecocokan tercatat. Sekarang ada dua sebab yang
mungkin, dan keduanya bisa terjadi bersamaan. Penanda yang tidak menyebutkan sebabnya karena itu
berhenti bermakna bagi pembaca rekam berikutnya. `INV-BD-030` menuntut otorisasi darurat menyatakan
gerbang mana yang dilewati. Ini konsekuensi langsung dari kewajiban audit yang sudah ada, bukan
kebijakan baru — bila pemilik proses menghendaki penanda tunggal tanpa keterangan, `INV-BD-030`
perlu dicabut secara eksplisit.

*Contoh jalur normal.* Kantong `PMI-00933` dialokasikan untuk Tn. S hari Senin, bukti kecocokan
tercatat dan masih berlaku. Selasa pagi "Kulkas Besar" ditandai nonaktif karena pintunya rusak. Selasa
siang petugas mencoba memberikan kantong itu — ditolak, walaupun bukti kecocokannya masih hidup dan
alokasinya sah. Petugas memindahkan kantong ke "Kulkas Kecil", perpindahan tercatat, lalu pemberian
berhasil. Alokasi Tn. S tidak pernah putus sepanjang kejadian ini.

*Contoh jalur darurat.* Keadaan yang sama, tetapi Tn. S mengalami perdarahan hebat dan darah harus
masuk sekarang. Peran berwenang menerbitkan otorisasi darurat dengan alasan yang wajib diisi.
Pemberian berjalan, dan rekam menyimpan penanda permanen yang menyebutkan bahwa yang dilewati adalah
gerbang **lokasi nonaktif**, bukan gerbang bukti kecocokan — karena bukti kecocokannya memang ada dan
berlaku.

### 8.18 Role & authority closure pass

Menutup `DEF-BD-004`, satu-satunya keputusan bisnis yang masih memblokir implementasi tiga jalur:
validasi hasil golongan darah, jalur darurat, dan koreksi pencatatan pemberian.

**Fakta platform yang mendasari bentuk keputusannya.** Quilvian memakai role ASP.NET Identity generik
(`Models/ApplicationRole.cs`) beserta pemetaan hak akses (`Areas/Administrator/Setting/`), dan hanya
`SuperAdmin` serta `User` yang di-seed. Tidak ada taksonomi peran klinis di dalam sistem. Karena itu
`DEF-BD-004` bukan memilih nilai enum yang sudah ada, melainkan menetapkan **jabatan rumah sakit mana
dipetakan ke butir hak akses mana**. Quilvian hanya menerapkan hak akses dan mencatat audit; ia tidak
menilai kompetensi klinis siapa pun.

| Decision ID | Menutup | Type | Keputusan | Owner | Status | Approved by/at |
| --- | --- | --- | --- | --- | --- | --- |
| `DEC-BD-039` | `DEF-BD-004` bagian validator | `Decision` | Validasi hasil golongan darah dipecah menjadi **dua wewenang**: validasi rutin oleh petugas BDRS berwenang validasi, penyelesaian konflik oleh validator klinis yang ditunjuk | Pemilik proses klinis bersama BDRS | `draft` | kosong |
| `DEC-BD-040` | `DEF-BD-004` bagian jalur darurat | `Decision` | Otorisasi darurat dapat diterbitkan **Dokter BDRS atau DPJP pasien**, dengan kelengkapan rekam yang wajib | Pemilik proses klinis bersama BDRS | `draft` | kosong |
| `DEC-BD-041` | `DEF-BD-004` bagian koreksi | `Decision` | Koreksi pencatatan pemberian menjadi proses **dua tahap**: petugas BDRS meminta, Dokter BDRS menyetujui | Pemilik proses klinis bersama BDRS | `draft` | kosong |

Turunannya: `INV-BD-031`, `INV-BD-032`, `INV-BD-033`, dan `AC-BD-077` sampai `AC-BD-088`.

Dengan ketiganya, **`DEF-BD-004` berstatus tertutup**.

### 8.19 Rincian keputusan role & authority closure pass

**`DEC-BD-039` — dua tingkat wewenang pada pemeriksaan golongan darah.**

| Wewenang | Siapa | Berlaku pada |
| --- | --- | --- |
| Validasi hasil rutin | Petugas BDRS yang diberi kewenangan validasi | Hasil pemeriksaan yang tidak bertentangan dengan hasil sah sebelumnya |
| Penyelesaian konflik | Validator klinis yang ditunjuk — Dokter BDRS / penanggung jawab klinis | Hasil yang bertentangan, ketika pasien sedang tidak punya golongan darah sah |

*Kenapa dipisah.* Kedua tindakan tampak mirip tetapi taruhannya berbeda jauh. Memvalidasi hasil rutin
berarti membenarkan satu hasil yang tidak diperselisihkan. Menyelesaikan konflik berarti menyatakan
hasil mana yang berlaku pada saat pasien **sedang tidak punya golongan darah sah sama sekali**
(`INV-BD-018`) — penetapan klinis yang salahnya berakibat fatal.

*Kenapa pemisahan ini tidak memperlambat apa pun.* Gerbang tambahan hanya berlaku pada keadaan yang
memang sudah tertahan: selama konflik berlangsung, alokasi dan pemberian untuk pasien itu sudah
berhenti. Menuntut wewenang yang lebih tinggi di situ tidak menunda pekerjaan yang tadinya berjalan.
Sebaliknya, menaruh gerbang pada validasi rutin akan menahan alur normal, dan itu bertentangan dengan
`DEC-BD-009`.

*Batas.* `DEC-BD-031` tetap berlaku penuh: penyelesaian konflik **wajib** menunjuk pemeriksaan ulang
tervalidasi, dan sistem tidak pernah menghitung mayoritas (`INV-BD-022`). `DEC-BD-039` menetapkan
**siapa** yang menyatakan, bukan mengubah **caranya**.

*Contoh.* Ny. R punya hasil sah O Positif. Hasil baru A Positif divalidasi petugas BDRS berwenang
validasi — validasi rutinnya sah, dan justru validasi itulah yang memunculkan konflik. Sejak saat itu
Ny. R tidak punya golongan darah sah. Petugas mencatat pemeriksaan ulang, hasilnya B Positif dan
divalidasi. Yang menutup konflik dan menyatakan B Positif berlaku **bukan** petugas tadi, melainkan
Dokter BDRS.

**`DEC-BD-040` — otorisasi darurat oleh dua peran.**

Otorisasi darurat dapat diterbitkan **Dokter BDRS** atau **DPJP pasien**.

*Kenapa dua, bukan satu.* Transfusi darurat terjadi tengah malam dan di IGD. Wewenang yang hanya
dipegang satu jabatan yang mungkin tidak berada di tempat membuat jalur darurat tidak terpakai, dan
petugas akan mencari jalan di luar sistem — hasilnya darah tetap keluar, tetapi tanpa jejak. Itu jauh
lebih berbahaya daripada wewenang yang lebih luas tetapi tercatat penuh. Perlindungan `DEC-BD-017`
memang tidak terletak pada sempitnya "siapa", melainkan pada alasan wajib, penanda permanen, dan
munculnya kantong itu pada daftar tunggakan.

*Yang wajib tersimpan pada setiap otorisasi darurat* (`INV-BD-032`):

| Yang disimpan | Keterangan |
| --- | --- |
| Alasan | Dari daftar alasan terkendali (`INV-BD-016`) |
| Kondisi kedaruratan | Keterangan keadaan klinis saat itu. **Wajib diisi**, tidak boleh kosong |
| Pelaku **beserta perannya** | Siapa yang menerbitkan, **dan** apakah ia bertindak sebagai Dokter BDRS atau sebagai DPJP. Tanpa keterangan peran, dua jalur wewenang yang berbeda tidak dapat dibedakan saat audit |
| Waktu | Kapan otorisasi diterbitkan |
| Pasien | Pasien tujuan |
| Kantong darah | Kantong yang dikeluarkan |
| Gerbang yang dilewati | Bukti kecocokan, lokasi nonaktif, atau keduanya (`INV-BD-030`, `DEC-BD-038`) |

*Batas yang ditegaskan.* Otorisasi darurat **bukan** pelewatan audit dan **bukan** keputusan otomatis
sistem. Ia selalu tindakan manusia yang tercatat; sistem tidak pernah menerbitkannya sendiri, sekalipun
keadaan tampak mendesak.

*Contoh.* Tn. S perdarahan hebat di IGD pukul 02.00. Dokter BDRS tidak di tempat. DPJP menerbitkan
otorisasi darurat dengan alasan terkendali "perdarahan masif" dan keterangan kondisi kedaruratannya.
Rekam menyimpan bahwa yang menerbitkan adalah DPJP, bukan Dokter BDRS — sehingga saat ditinjau kemudian,
jalur wewenang yang dipakai terbaca apa adanya.

**`DEC-BD-041` — koreksi pencatatan pemberian menjadi dua tahap.**

| Tahap | Pelaku | Isi |
| --- | --- | --- |
| 1. Permintaan koreksi | Petugas BDRS | Alasan, data yang dikoreksi (apa yang keliru dan apa yang benar), bukti pendukung |
| 2. Persetujuan | Dokter BDRS | Menyetujui atau menolak permintaan koreksi |

*Kenapa ada tahap kedua.* Koreksi tidak pernah mendesak — darahnya sudah diberikan, dan tidak ada
pasien yang menunggu koreksi selesai. Karena itu biaya sebuah gerbang persetujuan hampir nol.
Manfaatnya nyata: koreksi mengubah apa yang dinyatakan rekam tentang transfusi yang **sudah terjadi**
dan ikut mengubah angka pemenuhan order. Satu orang mengubah itu tanpa mata kedua adalah jalan yang
paling wajar bagi kekeliruan untuk tertutup rapi.

*Kenapa petugas tetap yang mencatat, bukan langsung Dokter BDRS.* Yang mengetahui duduk perkaranya
adalah orang yang menyaksikan kekeliruan itu. Menyerahkan penulisan "apa yang keliru, apa yang benar"
kepada orang yang tidak menyaksikannya menghasilkan catatan yang lebih miskin.

*Yang tidak berubah.* `DEC-BD-030` dan `INV-BD-021` tetap berlaku penuh: pemberian asal **tidak pernah**
dihapus atau dibalik, dan koreksi hanya menempel. `DEC-BD-034` juga tetap: koreksi tidak membalik fakta
biaya secara otomatis. Kejadian klinis sebelumnya tidak pernah terhapus oleh koreksi.

*Yang berubah, dan ini yang perlu diserap dokumen desain.* Koreksi kini punya **keadaan**: menunggu
persetujuan, lalu disetujui atau ditolak. Selama masih menunggu, koreksi **belum berlaku** —
`INV-BD-033` menegaskan angka pemenuhan order tidak bergerak sampai persetujuan turun. Ini pengetatan
terhadap `DEC-BD-030` yang semula memperlakukan koreksi sebagai satu tindakan yang langsung berlaku.

*Turunan yang mengikuti dari alasan keputusannya, bukan aturan tambahan yang dikarang.* Peminta dan
penyetuju **wajib orang yang berbeda**. Seluruh manfaat tahap kedua adalah mata kedua; bila satu orang
yang kebetulan memegang kedua butir hak akses dapat menyetujui permintaannya sendiri, gerbangnya tidak
menahan apa pun. Bila pemilik proses menghendaki persetujuan sendiri diizinkan, aturan ini perlu
dicabut secara eksplisit.

*Contoh.* Petugas menyadari nomor kantong pada catatan pemberian Tn. S tertukar dengan kantong lain. Ia
membuat permintaan koreksi berisi nomor yang keliru, nomor yang benar, alasan terkendali, dan bukti
pendukung. Dokter BDRS memeriksa lalu menyetujui. Sejak persetujuan itu, angka pemenuhan order Tn. S
dihitung ulang. Sebelum persetujuan, tidak ada satu pun angka yang berubah. Catatan pemberian aslinya
tetap terbaca utuh sepanjang proses.

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
| `AC-BD-065` | Lokasi penyimpanan nonaktif dipilih sebagai tempat penyimpanan kantong yang baru diterima | Ditolak — hanya lokasi aktif yang dapat menjadi tujuan penyimpanan (`INV-BD-027`) |
| `AC-BD-066` | Lokasi penyimpanan nonaktif dipilih sebagai **tujuan perpindahan** kantong yang sudah tersimpan | Ditolak — aturan yang sama berlaku untuk tujuan perpindahan, bukan hanya penempatan pertama (`INV-BD-027`) |
| `AC-BD-067` | Sebuah lokasi penyimpanan dinonaktifkan sementara masih ada kantong di dalamnya | Kantong tetap tercatat berada di lokasi tersebut, riwayat lokasinya utuh dan tetap terbaca, dan statusnya tidak berubah (`DEC-BD-037`) |
| `AC-BD-068` | Kantong yang penempatan terakhirnya menunjuk lokasi nonaktif dicoba dialokasikan | Ditolak — gerbang alokasi tertutup selama kantong masih tercatat di lokasi nonaktif (`INV-BD-028`) |
| `AC-BD-069` | Sebuah lokasi penyimpanan dinonaktifkan; sistem diminta memindahkan sendiri kantong di dalamnya, atau melemparkannya ke `PENDING_REVIEW` | Tidak ada — tidak ada perpindahan otomatis dan tidak ada perubahan status. Perpindahan adalah tindakan petugas BDRS (`DEC-BD-037`) |
| `AC-BD-070` | Petugas BDRS memindahkan kantong dari lokasi nonaktif ke lokasi aktif, lalu mencoba mengalokasikannya | Berhasil — perpindahan tercatat lengkap dengan pelaku dan waktu, dan gerbang alokasi terbuka kembali (`DEC-BD-037`) |
| `AC-BD-071` | Kantong `PENDING_REVIEW` yang berada di lokasi nonaktif dicoba dialihkan ke pasien lain | Ditolak — pengalihan adalah pengikatan kantong ke baris kebutuhan pasien, yaitu alokasi, sehingga `INV-BD-028` berlaku |
| `AC-BD-072` | Kantong sudah dialokasikan dan bukti kecocokannya masih berlaku, tetapi lokasi penyimpanannya dinonaktifkan sesudah alokasi; dicoba diberikan lewat jalur normal | Ditolak — gerbang pemberian menuntut lokasi terakhir aktif (`INV-BD-029`) |
| `AC-BD-073` | Kantong yang sama dipindahkan petugas ke lokasi aktif, lalu diberikan lewat jalur normal | Berhasil — perpindahan tercatat, gerbang terbuka, alokasi ke pasien tujuan tidak pernah putus |
| `AC-BD-074` | Kantong di lokasi nonaktif harus diberikan segera karena keadaan klinis; ditempuh lewat otorisasi darurat | Diizinkan — peran berwenang, alasan wajib, pelaku dan waktu tercatat, penanda melekat permanen (`DEC-BD-017`) |
| `AC-BD-075` | Pemberian darurat dari lokasi nonaktif dicatat tanpa menyebutkan gerbang mana yang dilewati | Ditolak — otorisasi darurat wajib menyatakan sebabnya: bukti kecocokan, lokasi nonaktif, atau keduanya (`INV-BD-030`) |
| `AC-BD-076` | Gerbang pemberian dicoba dilewati dengan mewarisi hasil pemeriksaan pada saat alokasi, tanpa dinilai ulang | Ditolak — ketiga syarat dinilai ulang tepat pada saat pemberian dicoba (`INV-BD-029`) |
| `AC-BD-077` | Petugas BDRS berwenang validasi memvalidasi hasil golongan darah rutin | Berhasil — validasi rutin tidak menunggu Dokter BDRS (`DEC-BD-039`) |
| `AC-BD-078` | Petugas BDRS berwenang validasi mencoba menutup konflik hasil golongan darah | Ditolak — penyelesaian konflik hanya validator klinis yang ditunjuk (`INV-BD-031`) |
| `AC-BD-079` | Validator klinis menutup konflik dengan menunjuk pemeriksaan ulang tervalidasi | Berhasil — satu hasil sah kembali berlaku; seluruh hasil tetap terbaca |
| `AC-BD-080` | Validator klinis mencoba menutup konflik tanpa pemeriksaan ulang tervalidasi | Ditolak — `DEC-BD-031` tetap berlaku penuh; wewenang tidak menggantikan prasyarat |
| `AC-BD-081` | DPJP pasien menerbitkan otorisasi darurat saat Dokter BDRS tidak di tempat | Berhasil — rekam menyimpan bahwa yang menerbitkan adalah DPJP (`DEC-BD-040`) |
| `AC-BD-082` | Dokter BDRS menerbitkan otorisasi darurat | Berhasil — rekam menyimpan perannya sebagai Dokter BDRS |
| `AC-BD-083` | Petugas Bank Darah tanpa wewenang darurat mencoba menerbitkan otorisasi darurat | Ditolak — hanya Dokter BDRS atau DPJP |
| `AC-BD-084` | Otorisasi darurat dicatat tanpa keterangan kondisi kedaruratan | Ditolak — kondisi kedaruratan wajib diisi (`INV-BD-032`) |
| `AC-BD-085` | Otorisasi darurat dicatat tanpa menyebut peran yang dipakai penerbitnya | Ditolak — peran wajib tersimpan, agar dua jalur wewenang dapat dibedakan saat audit (`INV-BD-032`) |
| `AC-BD-086` | Petugas BDRS mengajukan koreksi pencatatan pemberian | Koreksi tersimpan berstatus menunggu persetujuan; **angka pemenuhan order belum bergerak** (`INV-BD-033`) |
| `AC-BD-087` | Dokter BDRS menyetujui koreksi yang menunggu | Koreksi berlaku; angka pemenuhan dihitung ulang sejak persetujuan; pemberian asal tetap utuh |
| `AC-BD-088` | Petugas yang mengajukan koreksi mencoba menyetujui permintaannya sendiri | Ditolak — peminta dan penyetuju wajib orang yang berbeda (`DEC-BD-041`) |

`AC-BD-071` adalah turunan langsung dari model alokasi yang sudah disepakati, bukan aturan baru:
pengalihan kantong ke pasien lain menghasilkan ikatan alokasi yang sama bentuknya dengan alokasi
pertama. Bila pemilik proses menghendaki pengalihan **dikecualikan** dari gerbang ini, keputusan itu
perlu dinyatakan tersendiri dan `AC-BD-071` dicabut.

Seluruh nama pasien dan nomor kunjungan pada contoh adalah data samaran.

---

## 10. Open Questions dan Blocker

| ID | Isi | Pemilik | Memblokir |
| --- | --- | --- | --- |
| `DEC-BD-016` | Persetujuan pemilik Billing untuk menambah konteks sumber dan jenis efek biaya Bank Darah pada kontrak Billing. Pemicunya sudah jelas: satu tindakan Bank Darah yang selesai | Pemilik BillingManagement | Penyerahan biaya ke Billing |
| `DEF-BD-003` | Apakah semua komponen darah menuntut bukti kecocokan yang sama | Pemilik proses klinis | `IMPLEMENTATION` aturan per komponen |
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
Storage Location closure pass: coverage gap Storage Location, ditutup `DEC-BD-035` dan `DEC-BD-036`.
Storage Location decision closure pass: `ARCH-BD-GAP-10`, ditutup `DEC-BD-037`.
Gerbang pemberian closure pass: `OQ-BD-015`, ditutup `DEC-BD-038`.
Role & authority closure pass: `DEF-BD-004`, ditutup `DEC-BD-039`, `DEC-BD-040`, dan `DEC-BD-041`.

---

## 11. Langkah Berikutnya

**Role & authority closure pass (terbaru).** `DEC-BD-039`, `DEC-BD-040`, dan `DEC-BD-041` menutup
`DEF-BD-004`. Dengan itu **tidak ada satu pun keputusan bisnis yang masih memblokir** — baik `DESIGN`
maupun `IMPLEMENTATION` — pada scope yang sudah dinilai.

Pemblokir yang tersisa tinggal **satu, dan sifatnya administratif**: `BD-DEP-008`, pendaftaran prefix
entity di registry kepemilikan modul. Itu bukan keputusan bisnis dan bukan pekerjaan skill ini.

**Satu keputusan mengubah bentuk desain, bukan hanya mengisi peran.** `DEC-BD-041` menjadikan koreksi
pencatatan pemberian sebagai proses **dua tahap** dengan keadaan menunggu persetujuan. Sebelumnya
koreksi adalah satu tindakan yang langsung berlaku. Akibatnya, set kontrak `v2` **belum tersinkron**
pada bagian berikut dan menuntut pass lanjutan `design-business-module`:

| Artefak | Yang perlu diserap |
| --- | --- |
| `02-backend-architecture.md`, `data/data-dictionary.md` | Keadaan pada koreksi (menunggu / disetujui / ditolak), kolom peminta dan penyetuju beserta waktunya, kolom bukti pendukung; kolom peran penerbit dan kondisi kedaruratan pada otorisasi darurat |
| `contracts/api-contract.md` | Pemecahan endpoint koreksi menjadi ajukan dan setujui/tolak; butir hak akses `ResolveConflict` terpisah dari `Validate` |
| `contracts/state-transition-matrix.md` | Tiga baris koreksi menggantikan satu baris lama (§5.3 register ini sudah diperbarui sebagai acuan) |
| `contracts/validation-matrix.md` | Penolakan menyetujui koreksi sendiri, kondisi kedaruratan kosong, peran penerbit tidak tersimpan |
| `contracts/permission-audit-matrix.md` | Peta peran final menggantikan seluruh baris `UNRESOLVED`; butir hak akses baru untuk penyelesaian konflik dan persetujuan koreksi |
| `03-frontend-architecture.md` | Layar koreksi menjadi dua langkah; pilihan peran saat menerbitkan otorisasi darurat |
| `04-prd-to-mvp.md` | `EPIC BD-06` berubah bentuk; pertanyaan memblokir `DEF-BD-004` dicoret |

**Gerbang pemberian closure pass.** `DEC-BD-038` menutup `OQ-BD-015`, pertanyaan terakhir yang lahir
dari rangkaian Storage Location. Dengan itu **tidak ada gap arsitektur maupun pertanyaan keselamatan
yang masih terbuka** pada scope yang sudah dinilai.

Perlu dicatat bahwa `DEC-BD-038` **mengoreksi** perilaku yang sempat tercatat pada
`03-domain-architecture.md` revisi 5. Revisi itu menyatakan gerbang lokasi nonaktif tidak berlaku pada
pemberian, mengikuti bunyi `DEC-BD-037` apa adanya. Pernyataan itu kini tidak berlaku lagi dan sudah
dikoreksi pada revisi 6. Ini pergantian keputusan yang wajar dalam rangkaian pass, bukan kekeliruan
pencatatan: `DEC-BD-037` memang hanya menyebut alokasi, dan `DEC-BD-038` yang memperluasnya.

Register keputusan kini sejajar dengan arsitektur. `03-domain-architecture.md` revisi 6 sudah menyerap
`DEC-BD-035` sampai `DEC-BD-038` beserta `INV-BD-025` sampai `INV-BD-030`.

**Yang masih perlu disinkronkan ke hilir.**

| Artefak | Yang perlu diserap | Skill pemiliknya |
| --- | --- | --- |
| `02-backend-architecture.md`, `data/`, `contracts/`, `03-frontend-architecture.md` | `MstBloodStorageLocation` beserta penanda aktifnya, status `RECEIVED`/`STORED`, riwayat penempatan kantong, gerbang alokasi (`INV-BD-025` + `INV-BD-028`), **gerbang pemberian tiga syarat** (`INV-BD-029`), **keterangan sebab pada otorisasi darurat** (`INV-BD-030`), penolakan penempatan ke lokasi nonaktif (`INV-BD-027`), serta alur penetapan dan perpindahan lokasi | `design-business-module` |
| `roadmap/00-delivery-plan.md` | Storage Location pindah dari coverage gap ke task berurutan; task alokasi bergantung pada task penyimpanan | `plan-module-delivery` |
| `02-requirement-completeness-assessment.md` | Rumah slice resmi untuk `BR-BD-020`, yang sekarang diperlakukan sebagai perluasan `BD-SLICE-03`, `BD-SLICE-04`, dan `BD-SLICE-10` | `requirement-completeness-gate` |
| `blueprint-manifest.md`, `MODULE-STATUS.md` | `decision_revision` 7, `domain_architecture_revision` 6, penutupan `ARCH-BD-GAP-10` dan `OQ-BD-015` | `manage-module-blueprint` |

Yang **tidak** perlu diulang: `hospital-domain-architect`. Arsitektur revisi 5 sudah memuat seluruh
substansi `DEC-BD-037`; pass ini hanya mengukuhkan pencatatannya.

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
| `DEF-BD-004` | Peran jalur darurat, validator, dan pembuat catatan koreksi | **Ditutup** `DEC-BD-039`, `DEC-BD-040`, `DEC-BD-041` |
| `OQ-BD-010` | Kesediaan PMI menerima pengembalian | Tidak |
| `OQ-BD-012` | Nilai jam masa berlaku bukti kecocokan per komponen (struktur ditutup `DEC-BD-032`) | Tidak. `IMPLEMENTATION` saja |
| `OQ-BD-013` | Tempat penyelesaian perbedaan hasil golongan darah | **Ditutup** `DEC-BD-033` — di layar pemeriksaan golongan darah |
| `OQ-BD-014` | Keadaan kantong setelah koreksi pemberian | Tidak. `IMPLEMENTATION` saja |
| `OQ-BD-015` | Perluasan gerbang lokasi nonaktif ke jalur pemberian | **Ditutup** `DEC-BD-038` — ditahan pada jalur normal, jalur darurat `DEC-BD-017` tetap terbuka |
| `ARCH-BD-GAP-10` | Nasib kantong di lokasi penyimpanan yang dinonaktifkan | **Ditutup** `DEC-BD-037` |
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
