# Accounting — PRD ke MVP

## 1. Identitas dokumen

| Field | Value |
|---|---|
| Produk | Quilvian V2 |
| Modul | Accounting — kode modul `ACC` |
| `contract_version` | `ACC-MVP-0.1` |
| Status | `draft` — approval adalah tindakan manusia, belum diberikan |
| Product/domain owner | Rizki |
| Repository target | `NewQuilvianSystemBackend`, `QuilvianSystemFrontendDev` |
| Backend commit SHA | `aa837d784ff51cb2b889cf975ada3a204018f1f5` (branch `rizkiG`) |
| Frontend commit SHA | `fc49cc7714baa9a2c37ed6519fbaba5dffcbda99` (branch `RizkiV2`) |
| `input_revision` | `02-backend-architecture.md@3`, `03-frontend-architecture.md@3`, seluruh kontrak `ACC-*-0.1` |
| Traceability | `ACC-DEC-001` sampai `ACC-DEC-037` |

**Ringkasan cakupan:** rilis pertama menghasilkan pembukuan berpasangan yang lengkap — daftar
akun, jurnal manual dengan persetujuan, pengesahan ke buku besar, penguncian periode, koreksi,
dan neraca saldo — tanpa satu pun sambungan otomatis ke modul lain.

---

## 2. Ringkasan eksekutif

Rumah sakit saat ini tidak punya buku besar di dalam sistem. Setiap akibat keuangan dari
kegiatan operasional dicatat di luar sistem, sehingga laporan keuangan tidak dapat ditelusuri
kembali ke transaksi asalnya.

Rilis pertama modul Accounting menutup kesenjangan itu untuk pencatatan yang **dibuat manusia**.
Setelah rilis ini, tim akuntansi dapat menyusun daftar akun, mencatat jurnal, meminta
persetujuan, mengesahkannya ke buku besar, mengunci periode, dan mencetak neraca saldo — semuanya
di dalam sistem, dengan jejak siapa mengerjakan apa.

Yang **belum** tercakup rilis pertama adalah pencatatan otomatis dari modul lain. Itu sengaja,
dan alasannya ada di bagian 8.

---

## 3. Masalah produk

### Kondisi sekarang, dengan bukti kode

| Kenyataan | Bukti |
|---|---|
| Tidak ada entity daftar akun, jurnal, buku besar, maupun periode akuntansi | Pencarian pada `NewQuilvianSystemBackend@aa837d7` tidak menemukan satu pun |
| Tidak ada layar akuntansi | Tidak ada folder akuntansi pada `QuilvianSystemFrontendDev/src@fc49cc7` |
| Slot folder backend masih kosong | `Areas/Corporate/@aa837d7` hanya berisi `HumanResource` |
| Modul Finance belum ada | Tidak ada kode Finance pada `aa837d7` |

### Apa yang sudah ada dan dapat dipakai ulang

| Yang sudah ada | Bukti | Dipakai untuk |
|---|---|---|
| Master Cost Center, lengkap dengan badan hukum | `Areas/Corporate/HumanResource/MasterData/Organization/Models/MstCostCenter.cs@aa837d7` | Unit biaya pada baris jurnal |
| Master badan hukum | `Repositories/ApplicationDbContext.cs#MstLegalEntities@aa837d7` | Pemisahan pembukuan per badan hukum |
| Layanan pencatatan jejak audit | `Services/Logging/LoggerService.cs@aa837d7` | Jejak audit akuntansi |
| Mekanisme hak akses | Atribut `[AccessController]`, `[AccessPermission]`@aa837d7 | Kewenangan peran akuntansi |
| Komponen tabel dan penyaring | `src/components/features/base-features/@fc49cc7` | Seluruh layar daftar |
| Pola slice CRUD master data | `src/lib/state/slice/master-data-resource-slice-factory.jsx@fc49cc7` | Slice daftar akun dan jenis jurnal |

Temuan yang mengubah rencana: `MstCostCenter` ternyata **sudah ada dan bahkan memuat kolom
`AccountingCode`**. Semula diperkirakan Accounting harus membuat master unit biaya sendiri.
Ternyata tidak perlu, dan membuatnya justru akan menjadi duplikasi.

---

## 4. Visi produk

Rantai keterhubungan data yang ingin dicapai, ditulis sebagai urutan:

1. Sebuah kejadian keuangan terjadi — pada MVP, kejadian itu diketahui dan dicatat manusia.
2. Petugas akuntansi menerjemahkannya menjadi jurnal berisi dua sisi yang seimbang.
3. Jurnal diperiksa orang lain, bukan pembuatnya.
4. Jurnal disahkan, dan sejak itu menjadi riwayat permanen.
5. Buku besar terbentuk dari seluruh jurnal yang sudah disahkan.
6. Neraca saldo menampilkan posisi seluruh akun pada satu periode.
7. Periode dikunci, sehingga angka yang sudah dilaporkan tidak berubah diam-diam.
8. Bila ditemukan kesalahan, koreksinya terlihat sebagai catatan tersendiri — bukan dengan
   mengubah riwayat.

Pada Phase 2, langkah 1 dan 2 digantikan sambungan otomatis dari modul lain. Rantai selebihnya
tidak berubah, dan itulah sebabnya MVP ini bukan pekerjaan yang akan dibongkar.

---

## 5. Batas MVP

### Titik mulai

1. Daftar akun kosong, hanya berisi kerangka lima kelompok.
2. Master jenis jurnal berisi empat baris.
3. Periode satu tahun buku sudah dibangkitkan untuk setiap badan hukum.
4. Peran akuntansi sudah terdaftar pada mekanisme hak akses.

### Titik akhir

1. Petugas dapat menyusun daftar akun bertingkat untuk setiap badan hukum.
2. Petugas dapat mencatat jurnal manual, mengajukannya, dan jurnal itu diperiksa orang lain.
3. Jurnal yang disahkan masuk buku besar dan tidak dapat diubah lagi.
4. Kesalahan dapat dikoreksi lewat pembalikan penuh atau jurnal penyesuaian.
5. Periode dapat ditutup sementara, ditutup permanen, dan dibuka kembali dengan alasan tertulis.
6. Neraca saldo satu periode dapat ditampilkan dan selalu seimbang.
7. Saldo awal tercatat sebagai jurnal dan menjadi titik tumpu seluruh pembukuan berikutnya.

MVP dinyatakan selesai ketika satu badan hukum dapat berjalan dari saldo awal sampai neraca
saldo satu periode penuh, tanpa satu pun sambungan ke modul lain.

---

## 6. Pelaku sasaran

| Pelaku | Tanggung jawabnya di dalam MVP |
|---|---|
| Accounting Administrator | Menyusun daftar akun, mengatur jenis jurnal, membangkitkan periode |
| Accounting Staff | Membuat dan mengajukan jurnal |
| Accounting Approver | Memeriksa lalu menyetujui atau menolak jurnal |
| Accounting Manager | Mengesahkan jurnal, membalik, menutup dan membuka periode |
| Auditor | Membaca seluruh riwayat dan jejak audit |
| Accounting Viewer | Melihat jurnal dan laporan tanpa mengubah apa pun |
| Pimpinan keuangan | Menyetujui saldo awal sebelum disahkan Manajer |

---

## 7. Pemilihan kemampuan MVP

Setiap kemampuan diuji dua pertanyaan: tanpa ini, apakah satu kasus nyata bisa selesai dari awal
sampai akhir? Kalau tidak, apakah ada jalan sementara yang aman dan tetap dapat diaudit?

| Kemampuan | ID kemampuan asal | Keputusan MVP |
|---|---|---|
| Daftar akun bertingkat per badan hukum | `ACC-CAP-001` | Wajib; tanpa akun, tidak ada tempat mencatat apa pun |
| Jurnal dan baris jurnal | `ACC-CAP-002` | Wajib; inilah catatan transaksinya |
| Buku besar | `ACC-CAP-003` | Wajib; tanpa ini pencatatan tidak menghasilkan apa-apa |
| Periode akuntansi | `ACC-CAP-004` | Wajib; tanpa penguncian, angka yang dilaporkan bisa berubah diam-diam |
| Layar akuntansi di aplikasi web | `ACC-CAP-005` | Wajib; tanpa layar, modul tidak dapat dipakai siapa pun |
| Rujukan master Cost Center | `ACC-CAP-007` | Wajib; `ACC-DEC-019` mewajibkannya pada akun beban |
| Rujukan master badan hukum | `ACC-CAP-008` | Wajib; `ACC-DEC-037` memisahkan pembukuan per badan hukum |
| Jejak audit | `ACC-CAP-009` | Wajib; pembukuan tanpa jejak tidak dapat diaudit |
| Hak akses berperan | `ACC-CAP-010` | Wajib; `ACC-DEC-015` dan `ACC-DEC-016` menuntut pemisahan tugas |

---

## 8. Kemampuan yang ditunda

Setiap penundaan menyebut alasan bersebab dan penggantinya selama MVP berjalan.

| Kemampuan | ID kemampuan asal | Alasan ditunda | Pengganti selama MVP |
|---|---|---|---|
| Pencatatan otomatis dari modul lain | `ACC-CAP-006` | Arah aliran kejadian keuangan belum diputuskan. Kontrak Billing `BIL-INTEGRATION-0.4` yang sudah disetujui mengarahkan akibat keuangan ke AR/AP milik Finance, sedangkan modul Finance belum ada dan belum ada developernya. Membangunnya sekarang berarti menebak bentuk kontrak yang `ACC-XM-001`-nya belum turun, dan menebak salah berarti membongkar ulang | Petugas akuntansi mencatat jurnal manual berdasarkan laporan dari unit terkait. Cara ini persis yang berjalan sekarang di luar sistem, tetapi sudah berada di dalam sistem dan sudah dapat diaudit |
| Pemetaan posting | `ACC-CAP-006` | Hanya bermakna bila ada kejadian otomatis yang perlu dipetakan | Petugas memilih akun sendiri saat menyusun jurnal |
| Jurnal berulang | — | `ACC-OQ-017` masih `DEFERRED`. Aturan pembuatan dan pengesahan otomatis belum diputuskan | Petugas menyalin jurnal bulan sebelumnya secara manual. Untuk penyusutan dan sewa dibayar di muka, jumlahnya sedikit dan nilainya tetap, sehingga masih wajar dikerjakan manual |
| Impor jurnal dari berkas CSV | — | Bermanfaat terutama untuk memasukkan data massal, dan pada MVP satu-satunya data massal adalah saldo awal yang cukup dimasukkan sekali | Saldo awal dimasukkan sebagai satu jurnal berjenis `SA` lewat layar biasa |
| Tutup bulan dan tutup tahun berdaftar periksa | — | `ACC-OQ-018` sampai `ACC-OQ-020` masih `DEFERRED`. Apa yang menghalangi penutupan belum diputuskan | Penutupan periode tetap tersedia lewat tombol Tutup, hanya tanpa daftar periksa otomatis. Manajer memeriksa sendiri sebelum menutup |
| Laporan Laba Rugi dan Neraca | — | Keduanya menuntut klasifikasi laporan keuangan pada daftar akun yang matang, dan daftar akun baru akan disusun pada rilis ini | Neraca Saldo sudah menampilkan posisi seluruh akun, dan dapat diolah di luar sistem bila laporan resmi dibutuhkan lebih cepat |
| Laporan pajak | — | `ACC-DEC-034` menetapkan laporan pajak berada di luar kepemilikan Accounting | Pihak yang memilikinya menyusun sendiri dari data Neraca Saldo dan Buku Besar |
| Banyak mata uang | — | `ACC-DEC-020` memilih rupiah saja untuk rilis pertama | Tidak ada penggantinya, dan tidak diperlukan: seluruh transaksi rumah sakit saat ini dalam rupiah |

---

## 9. Alur bisnis target

### `FLOW-ACC-MVP-001` — Dari kejadian keuangan sampai neraca saldo

**Tujuan.** Mencatat satu kejadian keuangan sampai terlihat pada laporan.

**Pelaku.** Accounting Staff membuat, Accounting Approver memeriksa, Accounting Manager
mengesahkan.

**Pemicu.** Ada kejadian yang punya akibat uang, misalnya pembelian obat dari pemasok.

**Prasyarat.** Daftar akun sudah terisi, periode tujuan menerima pencatatan, dan petugas punya
hak akses yang sesuai.

**Langkah utama:**

1. Staf membuka layar Jurnal, memilih badan hukum, lalu menekan Buat Jurnal.
2. Staf memilih jenis jurnal, mengisi tanggal akuntansi, dan menulis keterangan.
3. Staf menambahkan baris: akun, sisi debit atau kredit, nilai, dan unit biaya bila akunnya
   berjenis beban.
4. Sistem menghitung total kedua sisi setiap kali baris berubah, lalu menampilkan selisihnya.
5. Staf menekan Ajukan. Sistem memeriksa sembilan syarat pengajuan.
6. Sistem memberi nomor jurnal dan mengubah statusnya menjadi menunggu persetujuan.
7. Penyetuju membuka jurnal itu, memeriksa isinya, lalu menyetujui atau menolak beserta alasan.
8. Manajer mengesahkan jurnal. Sistem memeriksa ulang keseimbangan dan status periode.
9. Jurnal masuk buku besar dan tidak dapat diubah lagi.
10. Neraca saldo periode itu ikut berubah.

**Aturan bisnis yang berlaku sepanjang alur:**

> Total debit harus sama persis dengan total kredit sebelum jurnal boleh diajukan maupun
> disahkan.
>
> **Contoh:** jurnal pembelian obat berisi debit Beban Obat Rp 4.500.000 dan kredit Utang
> Pemasok Rp 4.500.000. Total kedua sisi Rp 4.500.000, sehingga jurnal boleh diajukan. Bila
> kreditnya keliru ditulis Rp 4.500.00 — kurang satu angka nol, menjadi Rp 450.000 — selisih
> Rp 4.050.000 akan menahan pengajuan.

**Jalur tidak normal:**

- *Petugas menutup layar di tengah pengisian.* Jurnal tetap tersimpan sebagai draft
  (`ACC-DEC-025`), sehingga pekerjaannya tidak hilang.
- *Periode ditutup saat jurnal masih menunggu persetujuan.* Pengesahan ditolak dengan pesan yang
  menyebut nama periodenya. Jurnal tidak hilang dan tetap berstatus disetujui.
- *Penyetuju kebetulan pembuat jurnal itu.* Permintaan ditolak tanpa pengecualian
  (`ACC-DEC-016`).
- *Dua petugas menyimpan jurnal bersamaan.* Keduanya berhasil dengan nomor berbeda, tanpa saling
  menunggu (`ACC-DEC-014`).
- *Jurnal ternyata salah setelah disahkan.* Tidak dapat diubah. Koreksi lewat pembalikan penuh
  atau jurnal penyesuaian (`ACC-DEC-017`).

**Hasil akhir.** Buku besar bertambah, saldo akun berubah, neraca saldo tetap seimbang, dan jejak
audit mencatat siapa membuat, menyetujui, serta mengesahkan beserta waktunya.

---

## 10. Epic dan functional requirement

### `EPIC ACC-01` — Daftar akun

**Tujuan.** Menyediakan tempat menggolongkan setiap rupiah. **Disposisi backend:**
`MISSING / NEW`.

> **`FR-ACC-001` — Menambah akun**
> Sistem menyimpan akun baru dengan kode unik per badan hukum.
> **Contoh:** administrator menambah `1-1001 Kas Besar` di bawah `1 Aset` pada PT Sehat Sentosa.
> Akun tersimpan dan muncul pada daftar pilihan jurnal.

> **`FR-ACC-002` — Menolak kode akun kembar**
> Sistem menolak akun yang kodenya sudah dipakai badan hukum yang sama.
> **Contoh:** menambah `1-1001` kedua kalinya pada PT Sehat Sentosa ditolak dengan kode `409`
> dan pesan "Kode akun 1-1001 sudah dipakai pada badan hukum ini." Menambah `1-1001` pada PT
> Sehat Mandiri **berhasil**, karena pembukuannya terpisah.

> **`FR-ACC-003` — Akun induk tidak menerima transaksi**
> Akun yang memiliki turunan tidak muncul pada daftar pilihan baris jurnal.
> **Contoh:** `1-1000 Kas dan Setara Kas` punya dua anak. Ia tidak muncul di daftar pilihan, dan
> permintaan yang memaksa memakainya ditolak dengan kode `409`.

> **`FR-ACC-004` — Akun bersaldo tidak dapat dinonaktifkan**
> **Contoh:** `1-1201 Piutang Asuransi X` bersaldo Rp 15.000.000. Penonaktifan ditolak dengan
> pesan yang menyebut angka saldonya. Setelah saldo dipindahkan lewat jurnal dan menjadi nol,
> penonaktifan berhasil.

> **`FR-ACC-005` — Kode akun terkunci setelah dipakai**
> **Contoh:** `5-1001 Beban Obat` sudah dipakai jurnal yang disahkan. Mengubah kodenya menjadi
> `5-1002` ditolak dengan kode `409`.

### `EPIC ACC-02` — Jenis jurnal

**Tujuan.** Menyimpan aturan alur per jenis jurnal. **Disposisi backend:** `MISSING / NEW`.

> **`FR-ACC-006` — Awalan nomor berasal dari master**
> **Contoh:** jenis `JU` berawalan `JU`, sehingga jurnal umum pertama September 2026 bernomor
> `JU/2026/09/00001`. Mengubah awalan pada master mengubah nomor jurnal berikutnya, tanpa
> menyentuh kode program.

> **`FR-ACC-007` — Jenis sistem terkunci**
> **Contoh:** jenis `JB` dan `SA` bertanda sistem. Mengubah kode maupun awalan nomornya ditolak
> dengan kode `409`.

### `EPIC ACC-03` — Periode akuntansi

**Tujuan.** Mengunci pembukuan agar angka yang dilaporkan tidak berubah diam-diam.
**Disposisi backend:** `MISSING / NEW`.

> **`FR-ACC-010` — Membangkitkan dua belas periode**
> **Contoh:** administrator membangkitkan tahun buku 2027 untuk PT Sehat Sentosa. Terbentuk 12
> periode, dari `2027-01` sampai `2027-12`, semuanya berstatus terbuka.

> **`FR-ACC-011` — Menolak tahun buku ganda**
> **Contoh:** membangkitkan tahun 2027 kedua kalinya untuk badan hukum yang sama ditolak dengan
> kode `409`.

> **`FR-ACC-012` — Tutup sementara menolak jurnal umum**
> **Contoh:** periode September 2026 berstatus tutup sementara. Mengesahkan Jurnal Umum ke
> periode itu ditolak dengan kode `422` dan pesan "Periode September 2026 sudah ditutup
> sementara. Hanya jurnal penyesuaian dan pembalikan yang masih dapat disahkan."

> **`FR-ACC-013` — Tutup sementara menerima jurnal penyesuaian**
> **Contoh:** pada periode yang sama, Jurnal Penyesuaian penyusutan Rp 8.000.000 **berhasil**
> disahkan. Inilah pembeda tutup sementara dari tutup permanen.

> **`FR-ACC-014` — Buka kembali menghasilkan tutup sementara**
> **Contoh:** periode September 2026 berstatus tutup permanen, lalu dibuka kembali. Statusnya
> menjadi **tutup sementara**, bukan terbuka, sehingga jurnal operasional September yang baru
> tetap ditolak.

> **`FR-ACC-015` — Buka kembali wajib beralasan**
> **Contoh:** menekan Buka Kembali tanpa mengisi alasan ditolak dengan kode `400`. Alasan yang
> diisi ikut tercatat di jejak audit.

### `EPIC ACC-04` — Jurnal manual

**Tujuan.** Mencatat transaksi akuntansi yang dibuat manusia. **Disposisi backend:**
`MISSING / NEW`.

> **`FR-ACC-020` — Draft belum seimbang tetap tersimpan**
> **Contoh:** jurnal penggajian 40 baris belum selesai diisi. Petugas menutup layar; jurnal tetap
> tersimpan sebagai draft dan dapat dilanjutkan besok.

> **`FR-ACC-021` — Pengajuan jurnal timpang ditolak**
> **Contoh:** total debit Rp 4.500.000 lawan total kredit Rp 4.000.000. Pengajuan ditolak dengan
> pesan "Jurnal belum seimbang. Total debit Rp 4.500.000, total kredit Rp 4.000.000, selisih
> Rp 500.000."

> **`FR-ACC-022` — Nomor jurnal terbentuk sesuai pola**
> **Contoh:** jurnal umum ketiga pada September 2026 untuk PT Sehat Sentosa bernomor
> `JU/2026/09/00003`.

> **`FR-ACC-023` — Penyimpanan bersamaan tidak saling menunggu**
> **Contoh:** dua staf menekan Simpan pada waktu hampir bersamaan. Keduanya berhasil dengan nomor
> `JU/2027/01/00004` dan `JU/2027/01/00005`. Tidak ada yang menunggu.

> **`FR-ACC-024` — Akun beban wajib menyebut unit biaya**
> **Contoh:** baris ketiga berisi debit `5-1001 Beban Obat` Rp 4.500.000 tanpa unit biaya.
> Pengajuan ditolak dengan pesan "Baris ke-3: akun beban 5-1001 wajib menyebutkan unit biaya."

> **`FR-ACC-025` — Satu baris hanya mengisi satu sisi**
> **Contoh:** baris berisi debit Rp 500.000 dan kredit Rp 200.000 sekaligus ditolak dengan kode
> `400`.

> **`FR-ACC-026` — Jurnal tidak mencampur dua badan hukum**
> **Contoh:** jurnal milik PT Sehat Sentosa yang memuat akun milik PT Sehat Mandiri ditolak
> dengan kode `409`.

### `EPIC ACC-05` — Persetujuan dan pengesahan

**Tujuan.** Memastikan setiap jurnal diperiksa orang kedua sebelum menjadi riwayat permanen.
**Disposisi backend:** `MISSING / NEW`.

> **`FR-ACC-030` — Alur penuh sampai disahkan**
> **Contoh:** jurnal diajukan staf, disetujui supervisor, disahkan manajer. Riwayat persetujuan
> berisi tiga baris beserta nama dan waktunya.

> **`FR-ACC-031` — Tidak boleh menyetujui jurnal sendiri**
> **Contoh:** Manajer membuat jurnal sendiri karena staf cuti, lalu mencoba menyetujuinya.
> Ditolak dengan kode `403`, walaupun Manajer memang punya hak menyetujui.

> **`FR-ACC-032` — Pengesahan menuntut persetujuan lebih dahulu**
> **Contoh:** mengesahkan jurnal yang masih menunggu persetujuan ditolak dengan kode `409`.

> **`FR-ACC-033` — Jurnal disahkan tidak dapat diubah**
> **Contoh:** menekan Ubah pada jurnal berstatus disahkan ditolak dengan kode `409`.

> **`FR-ACC-034` — Jurnal disahkan tidak dapat dihapus**
> **Contoh:** permintaan hapus dikirim langsung ke backend tanpa lewat layar. Ditolak, dan
> penanda `IsDelete` tetap bernilai salah.

### `EPIC ACC-06` — Koreksi dan pembalikan

**Tujuan.** Memperbaiki kesalahan tanpa menghapus riwayat. **Disposisi backend:**
`MISSING / NEW`.

> **`FR-ACC-040` — Pembalikan penuh untuk salah akun**
> **Contoh:** beban listrik Rp 12.000.000 keliru masuk akun beban air. Sistem membuat jurnal
> `JB` berisi kebalikannya. Jurnal asal **tetap** berstatus disahkan dan isinya utuh.

> **`FR-ACC-041` — Tidak boleh dibalik dua kali**
> **Contoh:** membalik jurnal yang sudah pernah dibalik ditolak dengan pesan "Jurnal ini sudah
> pernah dibalik dengan jurnal JB/2026/09/00001."

> **`FR-ACC-042` — Penyesuaian untuk salah nominal**
> **Contoh:** beban listrik tercatat Rp 12.000.000, seharusnya Rp 12.500.000. Sistem membuat
> jurnal `JP` berisi selisih Rp 500.000 saja, bukan pembalikan penuh.

> **`FR-ACC-043` — Jurnal pembalik tetap melewati persetujuan**
> **Contoh:** jurnal pembalik lahir berstatus menunggu persetujuan, bukan langsung disahkan.

### `EPIC ACC-07` — Buku besar dan neraca saldo

**Tujuan.** Menampilkan hasil pembukuan. **Disposisi backend:** `MISSING / NEW`.

> **`FR-ACC-050` — Neraca saldo selalu seimbang**
> **Contoh:** neraca saldo Januari 2027 menampilkan total debit Rp 128.500.000 dan total kredit
> Rp 128.500.000, dengan penanda seimbang bernilai benar.

> **`FR-ACC-051` — Laporan hanya memuat jurnal yang disahkan**
> **Contoh:** periode memuat lima jurnal disahkan dan dua jurnal yang baru disetujui. Neraca
> saldo hanya menghitung yang lima.

> **`FR-ACC-052` — Buku besar menampilkan saldo berjalan**
> **Contoh:** akun Kas Besar dengan tiga mutasi menampilkan saldo berjalan Rp 2.500.000,
> Rp 7.000.000, lalu Rp 6.500.000 sesuai urutan tanggal.

> **`FR-ACC-053` — Laporan terpisah per badan hukum**
> **Contoh:** saldo Kas Besar PT Sehat Sentosa dan PT Sehat Mandiri berbeda dan tidak pernah
> tercampur, walaupun kode akunnya sama.

### `EPIC ACC-08` — Saldo awal

**Tujuan.** Menetapkan titik tumpu pembukuan. **Disposisi backend:** `MISSING / NEW`.

> **`FR-ACC-060` — Saldo awal dicatat sebagai jurnal**
> **Contoh:** saldo pembuka per 1 Januari 2027 dicatat sebagai jurnal `SA/2027/01/00001`. Karena
> berbentuk jurnal, ia tunduk pada aturan keseimbangan dan ikut tampil di buku besar tanpa
> perlakuan khusus.

> **`FR-ACC-061` — Saldo awal disahkan Manajer**
> **Contoh:** jenis jurnal `SA` menuntut persetujuan, dan hanya Manajer yang punya hak
> mengesahkan. Persetujuan pimpinan keuangan dilakukan di luar sistem sebelum Manajer menekan
> Sahkan.

Tidak ada epic berstatus `OPEN DECISION` pada daftar di atas.

---

## 11. Model status yang diusulkan

### Status jurnal

| Status | Artinya bagi pengguna | Invariant utama |
|---|---|---|
| `Draft` | Masih disusun | Boleh belum seimbang |
| `PendingApproval` | Menunggu penyetuju | Sudah seimbang dan lolos sembilan syarat |
| `Approved` | Sudah disetujui, menunggu pengesahan | Penyetuju bukan pembuatnya |
| `Posted` | Sudah masuk buku besar | **Permanen** — tidak dapat diubah maupun dihapus |
| `Rejected` | Ditolak penyetuju | Alasan penolakan tersimpan |

### Status periode

| Status | Artinya bagi pengguna | Invariant utama |
|---|---|---|
| `Open` | Terbuka | Semua jenis jurnal diterima |
| `SoftClosed` | Tutup sementara | Hanya penyesuaian dan pembalikan diterima |
| `Closed` | Tutup permanen | Tidak ada jurnal yang diterima |

Rincian lengkapnya, termasuk perpindahan yang tidak sah, ada di
[contracts/state-transition-matrix.md](contracts/state-transition-matrix.md).

---

## 12. Sasaran arsitektur

| Yang dipakai ulang | Yang diperluas | Yang baru |
|---|---|---|
| `MstCostCenter`, `MstLegalEntity`, `LoggerService`, mekanisme hak akses, `ApplicationDbContext`, `ApiResponse<T>`, `PagedResult<T>`, `DataTable`, `DataFilter`, slice factory master data | `ApplicationDbContext` bertambah tujuh `DbSet`; `Program.cs` bertambah empat baris registrasi service | Tujuh tabel akuntansi, empat service, lima controller, enam berkas configuration, sembilan layar |

Rincian lengkapnya ada di [02-backend-architecture.md](02-backend-architecture.md) dan
[03-frontend-architecture.md](03-frontend-architecture.md).

---

## 13. Sasaran kemampuan API

Seluruh endpoint berstatus **Rencana (belum tersedia)**. Daftar di bawah adalah bagian dari
[contracts/api-contract.md](contracts/api-contract.md) dan tidak melebihinya.

### Corporate / Accounting / Master Data / Chart of Account

Base URL: `api/v1/corporate/accounting/master-data/chart-of-accounts`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar akun berhalaman | `ChartOfAccount : Read` | `ChartOfAccountPagedQuery` | `ApiResponse<PagedResult<ChartOfAccountListDto>>` | `EPIC ACC-01` |
| `GET` | `/tree` | Susunan induk-anak | `ChartOfAccount : Read` | query | `ApiResponse<List<ChartOfAccountTreeDto>>` | `EPIC ACC-01` |
| `GET` | `/options` | Akun yang menerima transaksi | `ChartOfAccount : Read` | query | `ApiResponse<List<ChartOfAccountOptionDto>>` | `EPIC ACC-01` |
| `POST` | `/` | Menambah akun | `ChartOfAccount : Create` | `CreateChartOfAccountDto` | `ApiResponse<ChartOfAccountDetailDto>` | `EPIC ACC-01` |
| `PUT` | `/{id}` | Mengubah akun | `ChartOfAccount : Update` | `UpdateChartOfAccountDto` | `ApiResponse<ChartOfAccountDetailDto>` | `EPIC ACC-01` |
| `PATCH` | `/{id}/deactivate` | Menonaktifkan akun | `ChartOfAccount : Update` | `DeactivateChartOfAccountDto` | `ApiResponse<ChartOfAccountDetailDto>` | `EPIC ACC-01` |

### Corporate / Accounting / Journal Management / Journal

Base URL: `api/v1/corporate/accounting/journals`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic |
|---|---|---|---|---|---|---|
| `GET` | `/` | Mencari jurnal | `Journal : Read` | `JournalPagedQuery` | `ApiResponse<PagedResult<JournalListDto>>` | `EPIC ACC-04` |
| `GET` | `/{id}` | Rincian jurnal | `Journal : Read` | — | `ApiResponse<JournalDetailDto>` | `EPIC ACC-04` |
| `POST` | `/` | Membuat draft | `Journal : Create` | `CreateJournalDto` | `ApiResponse<JournalDetailDto>` | `EPIC ACC-04` |
| `PUT` | `/{id}` | Mengubah draft | `Journal : Update` | `UpdateJournalDto` | `ApiResponse<JournalDetailDto>` | `EPIC ACC-04` |
| `DELETE` | `/{id}` | Menghapus draft | `Journal : Delete` | — | `ApiResponse<bool>` | `EPIC ACC-04` |
| `POST` | `/{id}/submit` | Mengajukan | `Journal : Submit` | — | `ApiResponse<JournalDetailDto>` | `EPIC ACC-04` |
| `POST` | `/{id}/approve` | Menyetujui | `Journal : Approve` | — | `ApiResponse<JournalDetailDto>` | `EPIC ACC-05` |
| `POST` | `/{id}/reject` | Menolak | `Journal : Approve` | `RejectJournalDto` | `ApiResponse<JournalDetailDto>` | `EPIC ACC-05` |
| `POST` | `/{id}/post` | Mengesahkan | `Journal : Post` | — | `ApiResponse<JournalDetailDto>` | `EPIC ACC-05` |
| `POST` | `/{id}/reverse` | Membalik atau menyesuaikan | `Journal : Reverse` | `ReverseJournalDto` | `ApiResponse<JournalDetailDto>` | `EPIC ACC-06` |

### Corporate / Accounting / Accounting Period

Base URL: `api/v1/corporate/accounting/periods`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar periode | `AccountingPeriod : Read` | `AccountingPeriodPagedQuery` | `ApiResponse<PagedResult<AccountingPeriodDto>>` | `EPIC ACC-03` |
| `POST` | `/generate` | Membangkitkan setahun | `AccountingPeriod : Create` | `GenerateAccountingPeriodDto` | `ApiResponse<List<AccountingPeriodDto>>` | `EPIC ACC-03` |
| `POST` | `/{id}/close` | Menutup periode | `AccountingPeriod : Close` | `ClosePeriodDto` | `ApiResponse<AccountingPeriodDto>` | `EPIC ACC-03` |
| `POST` | `/{id}/reopen` | Membuka kembali | `AccountingPeriod : Reopen` | `ReopenPeriodDto` | `ApiResponse<AccountingPeriodDto>` | `EPIC ACC-03` |

### Corporate / Accounting / General Ledger

Base URL: `api/v1/corporate/accounting/general-ledger`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic |
|---|---|---|---|---|---|---|
| `GET` | `/movements` | Mutasi buku besar | `GeneralLedger : Read` | `LedgerMovementQuery` | `ApiResponse<PagedResult<LedgerMovementDto>>` | `EPIC ACC-07` |
| `GET` | `/trial-balance` | Neraca saldo | `GeneralLedger : Read` | `TrialBalanceQuery` | `ApiResponse<TrialBalanceDto>` | `EPIC ACC-07` |
| `GET` | `/account-balance/{accountId}` | Saldo satu akun | `GeneralLedger : Read` | query | `ApiResponse<AccountBalanceDto>` | `EPIC ACC-07` |

Grup Jenis Jurnal tidak diulang di sini; lengkapnya ada di kontrak API.

---

## 14. Matriks kewenangan

String permission ditulis persis sama dengan
[contracts/permission-audit-matrix.md](contracts/permission-audit-matrix.md).

| Tindakan | String permission | Viewer | Staff | Approver | Manager | Auditor | Administrator |
|---|---|:---:|:---:|:---:|:---:|:---:|:---:|
| Lihat akun, jurnal, periode, buku besar | `[AccessPermission("...", "Read")]` | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Tambah dan ubah akun | `[AccessPermission("ChartOfAccount", "Create")]`, `("ChartOfAccount", "Update")` | | | | | | ✓ |
| Buat, ubah, hapus draft jurnal | `[AccessPermission("Journal", "Create")]`, `("Journal", "Update")`, `("Journal", "Delete")` | | ✓ | | ✓ | | |
| Ajukan jurnal | `[AccessPermission("Journal", "Submit")]` | | ✓ | | ✓ | | |
| Setujui atau tolak jurnal | `[AccessPermission("Journal", "Approve")]` | | | ✓ | ✓ | | |
| Sahkan jurnal | `[AccessPermission("Journal", "Post")]` | | | | ✓ | | |
| Balik jurnal | `[AccessPermission("Journal", "Reverse")]` | | | | ✓ | | |
| Bangkitkan periode | `[AccessPermission("AccountingPeriod", "Create")]` | | | | | | ✓ |
| Tutup dan buka kembali periode | `[AccessPermission("AccountingPeriod", "Close")]`, `("AccountingPeriod", "Reopen")` | | | | ✓ | | |

Manajer memiliki hak Staf juga, agar dapat bekerja saat staf berhalangan. Namun `ACC-DEC-016`
tetap berlaku penuh: Manajer yang membuat jurnal tidak boleh menyetujui jurnal itu sendiri.

---

## 15. Batas integrasi dan billing

Yang **MUST NOT** dibuat sendiri oleh modul ini:

| Larangan | Keputusan asal |
|---|---|
| Membaca atau menulis tabel Billing | `ACC-DEC-004` |
| Membaca atau menulis tabel Finance | `ACC-DEC-003` |
| Membuat tabel piutang atau utang sendiri | `ACC-DEC-005` |
| Membuat master Cost Center atau badan hukum sendiri | Tabel kepemilikan data pada `02-backend-architecture.md` |
| Mengubah kontrak Billing yang sudah disetujui | `ACC-PRD-001` §36 aturan 13 |
| Menerbitkan kejadian keuangan ke modul lain | `ACC-DEC-002` |

Perbedaan yang perlu dipahami: Accounting **memiliki akun** bernama `1-1201 Piutang Penjamin`,
tetapi **tidak memiliki** daftar siapa berutang berapa. Yang pertama laci penggolongan milik
Accounting; yang kedua transaksi operasional milik Finance.

Pada MVP, Accounting **tidak punya satu pun ketergantungan runtime** kepada Billing maupun
Finance. Itulah yang membuat `ACC-DEC-007` dapat dipenuhi.

---

## 16. Guardrail regulasi

| Kewajiban | Berlaku? | Keterangan |
|---|:---:|---|
| Kerahasiaan rekam medis | Tidak | MVP tidak menyimpan satu pun data pasien maupun isi klinis |
| Perlindungan data pribadi | Tidak langsung | MVP tidak menyimpan data pribadi. Yang dijaga adalah rahasia bisnis berupa nilai uang dan keterangan jurnal |
| Riwayat akuntansi tidak boleh dihapus | **Ya** | `ACC-DEC-006`, ditegakkan `AccJournalService` dan dibuktikan `UAT-13` |
| Pemisahan tugas pembuat dan penyetuju | **Ya** | `ACC-DEC-016`, dibuktikan `UAT-03` |
| Neraca terpisah per badan hukum | **Ya** | `ACC-DEC-037`, dibuktikan `UAT-15`. Setiap perseroan wajib punya laporan keuangan sendiri |
| Pelaporan pajak | Tidak | `ACC-DEC-034` menempatkannya di luar kepemilikan Accounting |

---

## 17. Kebutuhan non-fungsional

| ID | Kebutuhan | Ketentuan |
|---|---|---|
| `NFR-001` | Keutuhan penyimpanan | Jurnal dan seluruh barisnya disimpan dalam satu transaksi database. Tidak boleh ada jurnal tanpa baris |
| `NFR-002` | Penyimpanan bersamaan | Dua petugas dapat menyimpan jurnal pada saat bersamaan tanpa saling menunggu. Unique index nomor jurnal menjadi jaring pengaman terakhir |
| `NFR-003` | Jejak audit | Setiap perubahan status jurnal, penutupan periode, dan pembukaan kembali tercatat beserta pelaku dan waktunya |
| `NFR-004` | Kerahasiaan catatan log | Nilai uang dan keterangan jurnal **tidak boleh** masuk payload log |
| `NFR-005` | Otorisasi di backend | Menyembunyikan tombol di layar bukan pengamanan. Backend memeriksa ulang setiap tindakan |
| `NFR-006` | Koreksi tanpa menghapus | Kesalahan diperbaiki lewat pembalikan atau penyesuaian, tidak pernah dengan mengubah riwayat |
| `NFR-007` | Penanganan waktu | Tanggal akuntansi menentukan periode. Waktu tindakan disimpan seragam mengikuti pola yang berlaku di repository |
| `NFR-008` | Ketepatan angka | Nilai uang memakai `decimal(18,2)`. Perbandingan keseimbangan memakai kesamaan persis, bukan toleransi |

`NFR-008` perlu ditegaskan: **jangan memakai tipe pecahan mengambang untuk uang.** Perbandingan
keseimbangan menuntut kesamaan persis, dan tipe mengambang membuat Rp 4.500.000 bisa tidak sama
dengan Rp 4.500.000.

---

## 18. Skenario UAT

Sembilan belas skenario lengkap beserta kondisi awal, langkah, dan hasil yang diharapkan ada di
[testing/acceptance-test-matrix.md](testing/acceptance-test-matrix.md). Ringkasan cakupannya:

| Epic | UAT berhasil | UAT gagal |
|---|---|---|
| `EPIC ACC-01` Daftar akun | `UAT-01` | `UAT-17` |
| `EPIC ACC-02` Jenis jurnal | `UAT-01` | `UAT-18` |
| `EPIC ACC-03` Periode | `UAT-07`, `UAT-08` | `UAT-06`, `UAT-09` |
| `EPIC ACC-04` Jurnal manual | `UAT-01`, `UAT-05` | `UAT-02`, `UAT-04` |
| `EPIC ACC-05` Persetujuan dan pengesahan | `UAT-01` | `UAT-03`, `UAT-13` |
| `EPIC ACC-06` Koreksi dan pembalikan | `UAT-10`, `UAT-11` | `UAT-12` |
| `EPIC ACC-07` Buku besar dan neraca saldo | `UAT-14`, `UAT-15` | `UAT-14` bagian jurnal belum disahkan |
| `EPIC ACC-08` Saldo awal | `UAT-16` | `UAT-19` |

Kedelapan epic `MUST HAVE` memiliki sekurang-kurangnya satu skenario berhasil **dan** satu
skenario gagal.

---

## 19. Definition of Done

Setiap butir dijawab "ya" atau "belum", beserta buktinya.

| Butir | Bukti |
|---|---|
| Satu badan hukum dapat berjalan dari saldo awal sampai neraca saldo satu periode penuh | `UAT-16`, `UAT-01`, `UAT-14` |
| Jurnal tidak seimbang tidak pernah masuk buku besar | `UAT-02`, `FR-ACC-021` |
| Jurnal yang sudah disahkan tidak dapat diubah maupun dihapus lewat jalur mana pun | `UAT-13` |
| Pembuat jurnal tidak pernah dapat menyetujui jurnalnya sendiri | `UAT-03` |
| Periode tutup sementara menolak jurnal umum tetapi menerima penyesuaian | `UAT-06`, `UAT-07` |
| Periode yang dibuka kembali menjadi tutup sementara, bukan terbuka | `UAT-08` |
| Pembukaan kembali periode selalu punya alasan tertulis yang tercatat | `UAT-09` |
| Kesalahan dapat dikoreksi lewat pembalikan penuh maupun penyesuaian | `UAT-10`, `UAT-11` |
| Neraca saldo selalu seimbang dan tidak memuat jurnal yang belum disahkan | `UAT-14` |
| Pembukuan dua badan hukum tidak pernah tercampur | `UAT-15` |
| Seluruh tabel master MVP sudah terisi | Rencana data master awal pada `02-backend-architecture.md` bagian 9 |
| Migration Accounting hanya menghasilkan tujuh `CreateTable` bernama sesuai prefix terdaftar, dan lulus `CONTAMINATION GUARD` | Pemeriksaan berkas migration, `02-backend-architecture.md` bagian 8, `roadmap/backend-roadmap.md` `BE-ACC-006` |
| Setiap endpoint memiliki `[AccessPermission]` sesuai matriks kewenangan | `contracts/permission-audit-matrix.md` |
| Nilai uang dan keterangan jurnal tidak muncul di payload log | Pemeriksaan keluaran `LoggerService` |

---

## 20. Urutan pengiriman dan pertanyaan terbuka

### Gelombang pengiriman

Ditulis sebagai gelombang, bukan tanggal. Penjadwalan tetap wewenang manusia.

| Gelombang | Isi | Epic tercakup | Syarat mulai |
|---|---|---|---|
| `MVP-0` | Fondasi: enam entity, berkas configuration, migration, data master awal | `EPIC ACC-01`, `EPIC ACC-02`, `EPIC ACC-03` sisi backend | Blueprint disetujui **dan `ACC-DEP-002` selesai** |
| `MVP-1` | Jurnal manual sampai disahkan, beserta layarnya | `EPIC ACC-04`, `EPIC ACC-05` | `MVP-0` selesai |
| `MVP-2` | Buku besar dan neraca saldo | `EPIC ACC-07` | `MVP-1` selesai |
| `MVP-3` | Koreksi, pembalikan, dan saldo awal | `EPIC ACC-06`, `EPIC ACC-08` | `MVP-2` selesai |
| `POST-MVP` | Seluruh kemampuan yang ditunda pada bagian 8 | — | Di luar cakupan rilis pertama |

Tidak ada gelombang yang memuat epic berstatus `OPEN DECISION`.

`MVP-0` sengaja diberi syarat tambahan: entity pertama tidak boleh dibuat sebelum prefiksnya
terdaftar, dan satu pelanggaran menggagalkan penggabungan kode.

### Pertanyaan terbuka sebelum development lock

| Pertanyaan | Siapa yang menjawab | Dampak bila belum dijawab | Memblokir |
|---|---|---|:---:|
| Prefix penamaan entity Accounting belum terdaftar di registry kepemilikan modul (`ACC-DEP-002`) | Lead | Entity pertama tidak boleh dibuat. Satu pelanggaran menggagalkan penggabungan kode | **Ya** |
| ~~Snapshot model EF bersama~~ (`ACC-DEP-001`) | — | **SELESAI** 30 Agustus 2026, diverifikasi 1 September 2026. Snapshot `aa837d7` identik dengan integration | Tidak lagi |
| Letak menu Accounting di navigasi (`ACC-FE-001`) | Product owner | Task frontend pertama tidak dapat dimulai | Ya, untuk frontend saja |
| Bentuk layar rincian jurnal: halaman, panel samping, atau modal (`ACC-FE-003`) | Product owner | Task layar rincian tidak dapat dimulai | Ya, untuk satu layar saja |
| Bagaimana hak atas badan hukum diberikan kepada pengguna | Owner keamanan platform | Penyaringan `LegalEntityId` tidak dapat ditegakkan dengan benar | **Ya** |
| Makna kolom `AccountingCode` pada `MstCostCenter` setelah Accounting menjadi pemilik COA | Owner Human Resource | Tidak berdampak pada MVP, karena Accounting tidak membacanya | Tidak |
| Siapa menerbitkan kejadian keuangan resmi (`ACC-XM-001`) | Owner Billing, owner Finance, Rizki | Phase 2 tidak dapat dirancang | Tidak untuk MVP |

**Dokumen ini masih berstatus `draft` dan memuat pertanyaan memblokir yang belum terjawab.**
Karena itu ia **belum boleh** diteruskan ke `/plan-module-delivery`. Dua pertanyaan bertanda
memblokir wajib dijawab lebih dahulu, dan keduanya berada di luar wewenang owner modul.

---

## 21. Strategi perluasan source traceability Phase 2

Bagian ini **tidak** menambah tabel, kolom, maupun task ke MVP. Ia hanya membuktikan bahwa
rancangan MVP tidak menutup jalan Phase 2, sehingga tidak ada yang tergoda menambah kolom Phase 2
"selagi sempat".

### Keadaan MVP, dan kenapa demikian

`AccJournal` MVP **tidak** punya `SourceDomain` maupun `SourceTransactionId`. Itu benar dan
disengaja: MVP tidak punya jurnal otomatis sama sekali (`ACC-DEC-009`), sehingga setiap jurnal
punya satu sumber — manusia yang membuatnya, yang sudah tercatat pada `CreateBy` dan riwayat
persetujuan. Kolom sumber yang selalu kosong hanya akan menjadi kolom mati.

### Rantai yang harus dapat terbentuk nanti

```
Source Transaction  →  Accounting Event  →  Journal  →  Journal Line  →  General Ledger
```

### Kenapa rantai itu masih mungkin tanpa redesign besar

| Sambungan | Sudah tersedia di MVP? | Cara melengkapinya di Phase 2 |
|---|---|---|
| `Journal → Journal Line` | **Ya**, FK `JournalId` | — |
| `Journal Line → General Ledger` | **Ya**, buku besar dihitung dari `AccJournalLine` berstatus `Posted`, bukan tabel terpisah | — |
| `Accounting Event → Journal` | Belum | Ditambahkan **di sisi event**, bukan dengan mengubah `AccJournal` |
| `Source Transaction → Accounting Event` | Belum | Berada seluruhnya di dalam agregat event |

Kunci yang membuat ini murah: **buku besar dihitung, bukan disimpan** (`BE-ACC-012`). Karena
tidak ada tabel buku besar yang harus ikut dibongkar, menambah lapis penelusuran di atas jurnal
tidak menyentuh jalur pelaporan sama sekali.

### Bentuk yang mungkin dipakai, dan yang belum diputuskan

Arah yang paling kecil dampaknya adalah agregat penaut tersendiri — misalnya `AccountingEvent`
beserta tautan ke jurnal yang dihasilkannya — sehingga `AccJournal` tidak perlu berubah bentuk.
Dua index unik pencegah pembukuan ganda pada `ACC-DEC-035` tinggal di agregat itu juga.

**Ini belum keputusan.** Bentuk finalnya ditetapkan setelah `ACC-XM-001` diputuskan dan kedua
gerbang skill dilewati. Yang sudah pasti hanyalah: apa pun bentuknya, ia **menambah** di atas
rancangan MVP dan tidak menuntut `AccJournal`, `AccJournalLine`, atau jalur buku besar dirombak.

### Yang dilarang sekarang

Menambah tabel atau kolom Phase 2 ke MVP dengan alasan future proofing — termasuk
`SourceDomain`, `SourceTransactionId`, `CurrencyCode`, dan tabel kotak masuk kejadian.
