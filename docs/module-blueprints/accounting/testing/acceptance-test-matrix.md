# Accounting — Acceptance Test Matrix

| Field | Value |
|---|---|
| `contract_version` | `ACC-TEST-0.1` |
| Status | `draft` — approval adalah tindakan manusia |
| Owner | Rizki (Product/Domain Owner Accounting) |
| `approved_by` / `approved_at` | Belum ada |
| `input_revision` | `02-backend-architecture.md@3`, seluruh kontrak `ACC-*-0.1` |
| Cakupan | MVP tulang punggung akuntansi (`ACC-DEC-009`) |

Matriks ini memuat **jalur berhasil dan jalur gagal**. Setiap epic `MUST HAVE` punya sekurang-
kurangnya satu dari masing-masing.

Seluruh angka pada contoh adalah data samaran. Tidak ada data asli pasien maupun pegawai yang
dipakai — modul ini memang tidak menyimpan keduanya.

## 1. Matriks ringkas

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FR-ACC-001` | Menambah akun baru | Unit + Integrasi | Baris `AccChartOfAccount` tersimpan dengan kode unik per badan hukum |
| `FR-ACC-002` | Kode akun kembar ditolak | Integrasi | `409` beserta pesan yang menyebut kodenya |
| `FR-ACC-003` | Akun induk tidak muncul pada daftar pilihan | Integrasi | `GET /options` tidak memuat akun ber-`IsPostable` salah |
| `FR-ACC-004` | Akun bersaldo gagal dinonaktifkan | Integrasi | `409` beserta jumlah saldonya |
| `FR-ACC-005` | Kode akun bertransaksi gagal diubah | Integrasi | `409` |
| `FR-ACC-010` | Membangkitkan dua belas periode | Integrasi | 12 baris `AccAccountingPeriod` berstatus terbuka |
| `FR-ACC-011` | Tahun buku ganda ditolak | Integrasi | `409` |
| `FR-ACC-012` | Tutup sementara menolak jurnal umum | Integrasi | `422` beserta nama periode |
| `FR-ACC-013` | Tutup sementara menerima jurnal penyesuaian | Integrasi | Jurnal `JP` berhasil disahkan |
| `FR-ACC-014` | Buka kembali menghasilkan tutup sementara | Integrasi | `PeriodStatus` menjadi `SoftClosed`, bukan `Open` |
| `FR-ACC-015` | Buka kembali tanpa alasan ditolak | Integrasi | `400` |
| `FR-ACC-020` | Menyimpan jurnal draft yang belum seimbang | Integrasi | Jurnal tersimpan berstatus `Draft` |
| `FR-ACC-021` | Mengajukan jurnal timpang ditolak | Unit + Integrasi | `400` beserta angka selisihnya |
| `FR-ACC-022` | Nomor jurnal terbentuk sesuai pola | Unit | Nomor sesuai `{prefix}/{yyyy}/{MM}/{00001}` |
| `FR-ACC-023` | Dua petugas menyimpan bersamaan | Integrasi | Dua jurnal tersimpan dengan nomor berbeda |
| `FR-ACC-024` | Akun beban tanpa unit biaya ditolak | Integrasi | `400` beserta nomor barisnya |
| `FR-ACC-025` | Baris mengisi debit dan kredit sekaligus ditolak | Unit | `400` |
| `FR-ACC-026` | Akun beda badan hukum ditolak | Integrasi | `409` |
| `FR-ACC-030` | Menyetujui lalu mengesahkan jurnal | Integrasi | Status menjadi `Posted`, riwayat persetujuan berisi 3 baris |
| `FR-ACC-031` | Menyetujui jurnal sendiri ditolak | Integrasi | `403` |
| `FR-ACC-032` | Mengesahkan jurnal yang belum disetujui ditolak | Integrasi | `409` |
| `FR-ACC-033` | Mengubah jurnal yang sudah disahkan ditolak | Integrasi | `409` |
| `FR-ACC-034` | Menghapus jurnal yang sudah disahkan ditolak | Integrasi | `409`, baris tetap ada dan `IsDelete` tetap salah |
| `FR-ACC-040` | Membalik jurnal penuh | Integrasi | Jurnal `JB` lahir berstatus menunggu persetujuan; jurnal asal tetap `Posted` |
| `FR-ACC-041` | Membalik dua kali ditolak | Integrasi | `409` beserta nomor jurnal pembalik yang sudah ada |
| `FR-ACC-042` | Koreksi lewat penyesuaian | Integrasi | Jurnal `JP` berisi baris selisih saja |
| `FR-ACC-050` | Neraca saldo seimbang | Integrasi | Total debit sama dengan total kredit; `IsBalanced` benar |
| `FR-ACC-051` | Laporan tidak memuat jurnal belum disahkan | Integrasi | Jurnal `Approved` tidak ikut terhitung |
| `FR-ACC-052` | Buku besar menampilkan saldo berjalan | Integrasi | Saldo berjalan bertambah sesuai urutan tanggal |
| `FR-ACC-053` | Laporan terpisah per badan hukum | Integrasi | Saldo PT A tidak tercampur PT B |
| `FR-ACC-060` | Saldo awal masuk sebagai jurnal | Integrasi | Jurnal `SA` berstatus `Posted`, ikut terhitung di neraca saldo |

## 2. Skenario UAT

Ditulis supaya penguji non-teknis dapat menjalankannya sendiri.

### `UAT-01` — Menyusun daftar akun dan mencatat jurnal pertama *(berhasil)*

**Kondisi awal:** modul baru dipasang. Master jenis jurnal sudah terisi empat baris. Periode
tahun 2027 sudah dibangkitkan. Daftar akun sudah berisi kerangka lima kelompok.

**Langkah:**
1. Administrator menambah akun `1-1001 Kas Besar` di bawah `1 Aset`, ditandai menerima transaksi.
2. Administrator menambah akun `4-1001 Pendapatan Rawat Jalan` di bawah `4 Pendapatan`.
3. Staf akuntansi membuat Jurnal Umum tanggal 5 Januari 2027: debit Kas Besar Rp 2.500.000,
   kredit Pendapatan Rawat Jalan Rp 2.500.000.
4. Staf menekan Ajukan.
5. Supervisor menekan Setujui.
6. Manajer menekan Sahkan.

**Hasil yang diharapkan:** jurnal bernomor `JU/2027/01/00001` berstatus disahkan. Buku besar
akun Kas Besar menampilkan satu baris bersaldo Rp 2.500.000. Neraca saldo Januari 2027 seimbang.

### `UAT-02` — Jurnal belum seimbang tertahan *(gagal)*

**Kondisi awal:** periode Januari 2027 terbuka.

**Langkah:** staf membuat jurnal berisi debit Beban Obat Rp 3.000.000, debit Beban Alat Habis
Pakai Rp 1.500.000, kredit Persediaan Farmasi Rp 4.000.000, lalu menekan Ajukan.

**Hasil yang diharapkan:** pengajuan ditolak dengan pesan "Jurnal belum seimbang. Total debit
Rp 4.500.000, total kredit Rp 4.000.000, selisih Rp 500.000." Jurnal **tetap tersimpan** sebagai
draft, sehingga pekerjaan staf tidak hilang.

### `UAT-03` — Menyetujui jurnal sendiri ditolak *(gagal)*

**Kondisi awal:** Manajer Akuntansi membuat sendiri sebuah jurnal, karena staf sedang cuti.
Manajer memiliki hak menyetujui maupun mengesahkan.

**Langkah:** Manajer membuka jurnal buatannya sendiri lalu menekan Setujui.

**Hasil yang diharapkan:** ditolak dengan pesan "Anda tidak dapat menyetujui jurnal yang Anda
buat sendiri." Tombol Setujui idealnya sudah tidak muncul di layar, tetapi walaupun permintaan
dikirim langsung ke backend, backend tetap menolaknya.

### `UAT-04` — Akun beban tanpa unit biaya tertahan *(gagal)*

**Kondisi awal:** akun `5-1001 Beban Obat` berjenis beban sudah ada. Unit biaya `RI-L3 Rawat
Inap Lantai 3` sudah ada dan aktif.

**Langkah:** staf membuat jurnal dengan baris ketiga berisi debit Beban Obat Rp 4.500.000, tetapi
kolom unit biaya dibiarkan kosong. Staf menekan Ajukan.

**Hasil yang diharapkan:** ditolak dengan pesan "Baris ke-3: akun beban 5-1001 wajib menyebutkan
unit biaya." Setelah unit biaya diisi `RI-L3`, pengajuan berhasil.

### `UAT-05` — Dua petugas menyimpan jurnal bersamaan *(berhasil)*

**Kondisi awal:** dua staf akuntansi masing-masing menyusun Jurnal Umum untuk Januari 2027.

**Langkah:** keduanya menekan Simpan pada waktu hampir bersamaan.

**Hasil yang diharapkan:** kedua jurnal tersimpan dengan nomor berbeda, misalnya
`JU/2027/01/00004` dan `JU/2027/01/00005`. Tidak ada yang menunggu, dan tidak ada yang gagal.
Urutan nomor tidak dijamin sesuai urutan penekanan tombol, dan itu wajar.

### `UAT-06` — Periode tutup sementara menolak jurnal umum *(gagal)*

**Kondisi awal:** periode September 2026 berstatus tutup sementara.

**Langkah:** staf menyusun Jurnal Umum bertanggal akuntansi 20 September 2026, lalu jurnal itu
disetujui dan Manajer menekan Sahkan.

**Hasil yang diharapkan:** pengesahan ditolak dengan pesan "Periode September 2026 sudah ditutup
sementara. Hanya jurnal penyesuaian dan pembalikan yang masih dapat disahkan." Jurnal tetap
berstatus disetujui, tidak hilang.

### `UAT-07` — Periode tutup sementara menerima jurnal penyesuaian *(berhasil)*

**Kondisi awal:** sama seperti `UAT-06`.

**Langkah:** staf menyusun Jurnal Penyesuaian bertanggal akuntansi 20 September 2026 untuk
mencatat penyusutan, lalu jurnal disetujui dan disahkan.

**Hasil yang diharapkan:** pengesahan berhasil. Inilah pembeda antara tutup sementara dan tutup
permanen.

### `UAT-08` — Membuka kembali periode yang sudah tutup permanen *(berhasil)*

**Kondisi awal:** periode September 2026 berstatus tutup permanen, laporannya sudah dikirim ke
manajemen. Ditemukan kesalahan besar.

**Langkah:** Manajer menekan Buka Kembali, lalu mengisi alasan "Ditemukan kesalahan pencatatan
beban listrik sebesar Rp 12.000.000 pada pemeriksaan internal."

**Hasil yang diharapkan:** periode menjadi **tutup sementara**, bukan terbuka. Alasan tersimpan
dan tercatat di jejak audit. Setelah itu jurnal penyesuaian bisa masuk, tetapi jurnal umum
September yang baru tetap ditolak.

### `UAT-09` — Membuka kembali tanpa alasan ditolak *(gagal)*

**Langkah:** Manajer menekan Buka Kembali lalu langsung menekan Simpan tanpa mengisi alasan.

**Hasil yang diharapkan:** ditolak dengan pesan "Alasan pembukaan kembali wajib diisi." Status
periode tidak berubah.

### `UAT-10` — Koreksi salah akun lewat pembalikan penuh *(berhasil)*

**Kondisi awal:** jurnal `JU/2026/09/00012` sudah disahkan, berisi debit Beban Air Rp 12.000.000
dan kredit Utang Rp 12.000.000. Seharusnya Beban Listrik, bukan Beban Air.

**Langkah:** Manajer membuka jurnal itu, menekan Balik, memilih pembalikan penuh, dan mengisi
alasan. Setelah jurnal pembalik disetujui dan disahkan, staf membuat jurnal baru yang benar.

**Hasil yang diharapkan:** lahir jurnal `JB/2026/09/00001` berisi kredit Beban Air Rp 12.000.000
dan debit Utang Rp 12.000.000. Jurnal asal **tetap** berstatus disahkan dan isinya tidak berubah
sama sekali. Saldo Beban Air kembali nol.

### `UAT-11` — Koreksi salah nominal lewat penyesuaian *(berhasil)*

**Kondisi awal:** jurnal beban listrik tercatat Rp 12.000.000, seharusnya Rp 12.500.000. Akunnya
sudah benar.

**Langkah:** Manajer menekan Balik, memilih jurnal penyesuaian, lalu mengisi baris selisih: debit
Beban Listrik Rp 500.000 dan kredit Utang Rp 500.000.

**Hasil yang diharapkan:** lahir jurnal `JP` berisi selisih Rp 500.000 saja, bukan pembalikan
penuh Rp 12.000.000. Saldo Beban Listrik menjadi Rp 12.500.000.

### `UAT-12` — Membalik jurnal dua kali ditolak *(gagal)*

**Kondisi awal:** jurnal `JU/2026/09/00012` sudah pernah dibalik dengan `JB/2026/09/00001`.

**Langkah:** Manajer membuka jurnal `JU/2026/09/00012` lalu menekan Balik lagi.

**Hasil yang diharapkan:** ditolak dengan pesan "Jurnal ini sudah pernah dibalik dengan jurnal
JB/2026/09/00001."

### `UAT-13` — Jurnal yang sudah disahkan tidak dapat diubah maupun dihapus *(gagal)*

**Langkah:** penguji mencoba dua hal pada jurnal berstatus disahkan: menekan Ubah, lalu mengirim
permintaan hapus langsung ke backend tanpa lewat layar.

**Hasil yang diharapkan:** keduanya ditolak. Baris jurnal masih ada di database, dan penanda
`IsDelete` tetap bernilai salah. Ini membuktikan `ACC-DEC-006` ditegakkan di backend, bukan hanya
disembunyikan di layar.

### `UAT-14` — Neraca saldo seimbang dan tidak mencampur status *(berhasil)*

**Kondisi awal:** periode Januari 2027 memuat lima jurnal disahkan dan dua jurnal yang baru
disetujui tetapi belum disahkan.

**Langkah:** penguji membuka Neraca Saldo Januari 2027.

**Hasil yang diharapkan:** total debit sama persis dengan total kredit. Dua jurnal yang belum
disahkan **tidak** ikut terhitung. Bila salah satu dari kedua jurnal itu kemudian disahkan,
neraca saldo berubah dan tetap seimbang.

### `UAT-15` — Pembukuan dua badan hukum tidak tercampur *(berhasil)* — **`DEFERRED`**

> **Ditunda oleh `ACC-DEC-041`, 2 September 2026.** MVP berjalan pada **satu** badan hukum,
> sehingga skenario ini tidak dapat dijalankan pada rilis pertama. Ia **tidak dihapus**: seluruh
> penegakan yang diujinya — kode akun unik per badan hukum, dan jurnal yang menolak akun milik
> badan hukum lain — **tetap dibangun** dan tetap diuji lewat test integrasi `BE-ACC-010`.
> `UAT-15` kembali berlaku begitu badan hukum kedua didaftarkan, dan pada saat itu `ACC-DEP-008`
> sudah wajib selesai lebih dahulu.

**Kondisi awal:** PT Sehat Sentosa dan PT Sehat Mandiri sama-sama punya akun berkode
`1-1001 Kas Besar`. Masing-masing punya jurnal sendiri.

**Langkah:** penguji membuka Neraca Saldo untuk PT Sehat Sentosa, lalu untuk PT Sehat Mandiri.

**Hasil yang diharapkan:** saldo Kas Besar keduanya berbeda dan tidak pernah tercampur. Menyusun
jurnal PT Sehat Sentosa yang memuat akun milik PT Sehat Mandiri ditolak.

### `UAT-16` — Saldo awal menjadi titik mulai pembukuan *(berhasil)*

**Kondisi awal:** sistem baru akan dipakai mulai 1 Januari 2027. Saldo akhir dari sistem lama
sudah disiapkan pemilik proses.

**Langkah:** staf membuat jurnal berjenis Saldo Awal bertanggal akuntansi 1 Januari 2027,
mengisi seluruh saldo pembuka. Jurnal disetujui, lalu Manajer mengesahkan setelah mendapat
persetujuan pimpinan keuangan.

**Hasil yang diharapkan:** jurnal `SA/2027/01/00001` berstatus disahkan. Neraca saldo Januari
2027 menampilkan saldo pembuka itu, dan tetap seimbang. Seluruh jurnal berikutnya bertumpu di
atasnya.

### `UAT-17` — Akun bersaldo gagal dinonaktifkan *(gagal)*

**Kondisi awal:** akun `1-1201 Piutang Asuransi X` bersaldo Rp 15.000.000.

**Langkah:** administrator menekan Nonaktifkan pada akun itu.

**Hasil yang diharapkan:** ditolak dengan pesan "Akun masih bersaldo Rp 15.000.000 dan tidak
dapat dinonaktifkan. Pindahkan saldonya lebih dahulu lewat jurnal." Setelah saldonya dipindahkan
lewat jurnal yang disahkan, penonaktifan berhasil.

### `UAT-18` — Mengubah jenis jurnal sistem ditolak *(gagal)*

**Kondisi awal:** master jenis jurnal berisi empat baris. `JB` (Jurnal Pembalik) dan `SA` (Saldo
Awal) bertanda sistem.

**Langkah:** administrator membuka jenis `SA`, mengubah awalan nomornya dari `SA` menjadi `SAL`,
lalu menekan Simpan.

**Hasil yang diharapkan:** ditolak dengan pesan "Jenis jurnal SA dipakai sistem dan kode maupun
awalan nomornya tidak dapat diubah." Awalan nomor tetap `SA`, sehingga jurnal saldo awal yang
sudah ada tidak kehilangan pola nomornya.

**Kenapa skenario ini ada:** proses pembalikan dan saldo awal memanggil jenis jurnal ini
berdasarkan kodenya. Bila kodenya berubah, kedua proses itu berhenti bekerja tanpa pesan yang
jelas.

### `UAT-19` — Saldo awal tidak seimbang ditolak *(gagal)*

**Kondisi awal:** sistem baru akan dipakai mulai 1 Januari 2027. Daftar akun sudah terisi.

**Langkah:** staf membuat jurnal berjenis Saldo Awal berisi debit Kas Besar Rp 150.000.000, debit
Piutang Penjamin Rp 80.000.000, dan kredit Modal Disetor Rp 200.000.000. Lalu menekan Ajukan.

**Hasil yang diharapkan:** ditolak dengan pesan "Jurnal belum seimbang. Total debit
Rp 230.000.000, total kredit Rp 200.000.000, selisih Rp 30.000.000." Jurnal tetap tersimpan
sebagai draft.

**Kenapa skenario ini ada:** saldo awal adalah satu-satunya jurnal yang menjadi titik tumpu
seluruh pembukuan berikutnya. Bila ia masuk dalam keadaan timpang, setiap neraca saldo setelahnya
ikut timpang dan sumbernya sulit dilacak. Skenario ini membuktikan saldo awal tidak mendapat
pengecualian dari aturan keseimbangan, walaupun jenis jurnalnya khusus.

## 3. Catatan menjalankan test integrasi

Test integrasi menerapkan migration dan menulis baris nyata. Ia **wajib** menunjuk database
khusus test yang namanya mengandung `test`.

**Jangan pernah** mengarahkannya ke database pengembangan bersama. Database itu dipakai satu tim,
dan menulis ke sana mengubah data yang sedang dipakai orang lain.

Proyek unit test berjalan tanpa database sama sekali, sehingga aman dijalankan kapan saja.


---

## 4. Finishing ACC-FIN-005 + 008 *(24 November 2026, **`approved`**, `GATE-DESAIN-1124`)*

Diuji lewat Swagger dan layar terhadap database dev, **tanpa SQL tulis** dan tanpa automated test
(`ACC-DEC-081`; kesepakatan kerja Rizki). Data uji dibuat lewat layar Jurnal yang ada. Angka berikut
data samaran.

| Skenario | Requirement | Langkah | Hasil yang diharapkan |
|---|---|---|---|
| `F1` | `FR-FIN-041` | Buat jurnal beban Rp 1.000.000 lalu biarkan Draft; buka laba rugi periodenya | Beban tidak bertambah |
| `F2` | `FR-FIN-011` | Pada tahun buku uji yang sudah ditutup `JT`, buka laba rugi periode terakhirnya | Laba bersih YTD = pendapatan − beban sebenarnya, bukan Rp 0 |
| `F3` | `FR-FIN-011` | Balik penuh jurnal `JT` tahun uji, sahkan pembaliknya, buka laba rugi periode yang sama | Laba bersih tidak berubah dari `F2` |
| `F4` | `FR-FIN-021` | Tahun uji belum ditutup; buka neraca periode pertama tahun berikutnya | Baris Laba Tahun Lalu Belum Ditutup + peringatan; Seimbang |
| `F5` | `FR-FIN-021` | Sesudah `JT` disahkan, buka neraca yang sama | Baris tahun lalu hilang; Laba Ditahan naik sebesar baris itu; total ekuitas tetap |
| `F6` | `FR-FIN-010` | Bandingkan laba rugi YTD periode terakhir tahun buku dengan neraca saldo periode itu | Total pendapatan dan beban sama dengan saldo akun-akunnya, di luar jurnal `JT` |
| `F7` | `FR-FIN-010` | Buka laba rugi pada tahun buku pertama di sistem | Pembanding dan selisih `—` beserta alasannya |
| `F8` | `FR-FIN-012` | Akun Akumulasi Penyusutan bersaldo kredit | Tampil dalam kurung, mengurangi subtotal aset tetap |
| `F9` | `FR-FIN-012` | Akun bernilai nol di semua kolom; centang Tampilkan akun bersaldo nol | Tersembunyi lalu tampil |
| `F10` | `FR-FIN-001` | Buku besar akun beban dengan dua unit biaya; saring satu unit | Saldo awal, berjalan, dan akhir hanya dari unit itu; catatan bagian unit tampil |
| `F11` | `FR-FIN-013` | Laba rugi disaring unit biaya | Hanya baris unit itu; catatan tetap tampil |
| `F12` | `VAL-GL-01` | Kirim `CostCenterId` milik badan hukum lain lewat Swagger | `400` "Unit biaya tidak ditemukan pada badan hukum ini." |
| `F13` | `FR-FIN-003` | Buku besar akun yang menerima jurnal hasil kejadian | Asal "{modul} · {nomor transaksi}"; klik membuka kejadian; nomor jurnal membuka rincian jurnal |
| `F14` | `FR-FIN-005` | Klik baris akun di neraca | Buku besar akun itu terbuka dengan rentang sampai tanggal kolom |
| `F15` | `FR-FIN-004` | Unduh CSV buku besar yang lebih dari satu halaman | Berkas memuat seluruh baris, saldo awal, saldo akhir; angka sama dengan layar |
| `F16` | `FR-FIN-041` | Pengguna tanpa `GeneralLedger : Read` memanggil kedelapan endpoint | `403` semuanya |
| `F17` | `FR-FIN-015` | Buka laba rugi lalu unduh; buka buku besar | Dua baris log tanpa nominal untuk laba rugi; nol baris untuk buku besar |
| `F18` | `FR-FIN-030` | Unduh CSV neraca saldo | Tersedia; isi sama dengan layar |
| `F19` | `VAL-GL-06` | Unduh buku besar lebih dari 50.000 baris | `422` dengan anjuran mempersempit rentang |
| `F20` | `FR-FIN-014` | Buka CSV laba rugi di Excel komputer rumah sakit | Kolom terpisah benar; angka terbaca angka, bukan teks |

`F19` dan `F20` ditambahkan saat desain (`VAL-GL-06`, `evidence/19` butir 14b); `F1`..`F18` berasal dari
decision log.

**Register hasil developer verification (bukan UAT).** Diisi agent dari bukti mentah di
`QuilvianSystemFrontendDev/test-with-agy/accounting/`. Skenario yang tidak tercantum belum dijalankan.

| Skenario | Tanggal | Hasil | Bukti |
|---|---|---|---|
| `F1` | 25 Nov 2026 | `PASS` — jurnal Ditolak dan Diajukan yang menyentuh 4001 tidak masuk laba rugi | `fin-008-02-20261125` `06-jajak-draft` |
| `F2` | 25 Nov 2026 | `PASS` — laba rugi 2019-12 identik sebelum dan sesudah JT/2019/12/00001 | sama, J3 |
| `F3` | 25 Nov 2026 | `PASS` — tetap identik sesudah JB/2019/12/00001 disahkan | sama, J5 |
| `F4` | 25 Nov 2026 | `PASS` (API dan layar) — 2026-09: baris Laba Tahun Lalu Belum Ditutup 270.000, `UnclosedFiscalYears = [2019]`, seimbang | `fin-008-03-20261125` N2, N3, P2; `fin-008-03-20261125` S3 |
| `F5` | 25 Nov 2026 | `PASS` (API; layar: tahun 2020 tidak lagi disebut sesudah JT) — sesudah JT/2020/12/00001: baris tahun lalu turun 330.000 → 270.000, akun laba ditahan naik 60.000, total ekuitas tetap | sama, P3; `fin-008-03-20261125` S3 |
| `F6` | 25 Nov 2026 | `PASS` — YTD 2026-12 = saldo neraca saldo 2026-12 per akun (sebelum data 2019 dibuat) | sama, I3 |
| `F7` | 25 Nov 2026 | `PASS` — tanpa pembanding: `null` + alasan; dengan pembanding (2020 lawan 2019): angka dan selisih | sama, I2, J4 |
| `F8` | 25 Nov 2026 | `PASS` untuk laba rugi — 4-1002 −30.000 mengurangi subtotal, di layar tampil "(Rp 30.000)" dan dibaca "minus"; neraca (API dan layar): 1-1004 tampil (Rp 40.000) dan mengurangi subtotal 1-1000 | sama, J1; `07-uji-layar` L11; `fin-008-03-20261125` P1; `fin-008-03-20261125` S5 |
| `F9` | 25 Nov 2026 | `PASS` untuk laba rugi (API dan layar) dan neraca (API) | sama, I4, I5; `07-uji-layar` L5; `fin-008-03-20261125` N6 |
| `F11` | 25 Nov 2026 | `PASS` (API dan layar) | sama, I6; `07-uji-layar` L6 |
| `F10` | 24 Nov 2026 | `PASS` — akun 4001 unit `CC-MMC-00001` | `fe-acc-fin-005-01-20261124` A1/A2, U3 |
| `F12` | 24 Nov 2026 | `PASS` — `400` pada ketiga endpoint buku besar | sama, A8 |
| `F13` | 24 Nov 2026 | `PASS` untuk asal Manual dan Kejadian | sama, A5, U5, U7 |
| `F14` | 24 + 25 Nov 2026 | `PASS` — saringan dari alamat (U6); klik angka laba rugi membuka Buku Besar dengan rentang kolom (L8); klik saldo neraca membuka Buku Besar dari awal data sampai tanggal kolom, termasuk kolom pembanding (S8, S9) | `fe-acc-fin-005-01-20261124` U6, U8; `fin-008-02-20261125` L8; `fin-008-03-20261125` S8, S9 |
| `F15` | 24 + 25 Nov 2026 | `PASS` — berkas = layar; uji ulang sesudah penguatan identik | sama, A6/U4; `fin-005-01-ujiulang-20261125` U4; `fin-008-01-20261125` B7 |
| `F16` | 24 + 25 Nov 2026 | `PASS` — `403` untuk 8 dari 8 endpoint | `fe-acc-fin-005-01-20261124` U10; `fin-008-01-20261125` B6, V8; `fin-008-02-20261125` I11, L16; `fin-008-03-20261125` N10 |
| `F17` | 24 + 25 Nov 2026 | `PASS` — log unduhan buku besar dan neraca saldo, serta log baca dan unduh laba rugi dan neraca, tanpa nominal | A12; `fin-008-01-20261125` B8; `fin-008-02-20261125` I15; `fin-008-03-20261125` N12 |
| `F18` | 25 Nov 2026 | `PASS` — 5 akun, total dan status sama dengan JSON; CSV dari layar = CSV dari API | `fin-008-01-20261125` B1–B5, V5 |
| `F19` | — | **Ditunda** — runtime `WAIVED` (`ACC-DEC-156`); source tujuh butir terbukti. Untuk neraca saldo (`ACC-DEC-157`) hanya penanganan pesan di layar yang diuji, dengan simulasi (V10) | Laporan `BE-ACC-FIN-005-01` bagian 8 |
| `F20` | — | `NOT RUN` — butuh Excel komputer rumah sakit; milik tim UAT | — |



## 5. Amendment ACC-FIN-001 Master COA *(25 November 2026, **`approved`**, `GATE-DESAIN-1125`)*

Diuji lewat Swagger dan layar terhadap database dev, **tanpa SQL tulis** dan tanpa automated test
(`ACC-DEC-081`; kesepakatan kerja Rizki). Data uji = akun berawalan nama "UJI COA" yang dibuat lewat
layar atau API, lalu dinonaktifkan sesudah uji (akun tidak dapat dihapus). Skenario klasifikasi
(`F34`..`F39`) baru dapat dijalankan sesudah migration `AddChartOfAccountReportClassification`
diterapkan Rizki. Kode akun di bawah mengikuti isi DB dev 25 November 2026 dan bergeser bila data
berubah; yang diuji adalah aturannya.

| Skenario | Requirement | Langkah | Hasil yang diharapkan |
|---|---|---|---|
| `F25` | `FR-FIN-050` | Tambah Akun, induk 1-1000, nama "UJI COA Bank", tanpa kode | Pratinjau `1-1005`; tersimpan `1-1005`; toast menyebut kode dari respons |
| `F26` | `FR-FIN-050` | Pratinjau untuk induk 1-0000 dan 3-0000 | `1-5000` dan `3-4000` — celah tidak diisi (`ACC-DEC-172` butir 1) |
| `F27` | `FR-FIN-053` | `POST` lewat Swagger dengan `accountCode` "9901" di bawah 1-1000, tidak menerima transaksi; lalu Tambah Akun di bawah 9901 lewat layar | Kode 9901 dipertahankan; anaknya `9901.01` |
| `F28` | `FR-FIN-050` | Dua `POST` serentak tanpa kode di bawah induk yang sama (skrip) | Dua akun tersimpan dengan dua kode berurutan; nol `409` |
| `F29` | `VAL-COA-06` | Tambah Akun tanpa induk, jenis Aset | `409` "Akun tingkat 1 untuk jenis Aset sudah ada (1-0000). …" — terlihat sejak pratinjau |
| `F30` | `VAL-COA-14` | `POST` berkode manual 20 karakter "UJI-PANJANG-12345678" sebagai induk; pratinjau anaknya | `409` "Kode induk … terlalu panjang …". Batas 999 dan 99 (`VAL-COA-13`) dibuktikan dari source |
| `F31` | `FR-FIN-051` | `POST` di bawah 1-1000 dengan `accountType` Liabilitas; lalu tanpa `accountType` | Pertama `409` "Jenis akun harus sama dengan akun induk 1-1000 (Aset)."; kedua tersimpan berjenis Aset. Di layar, Jenis Akun terkunci "Aset" |
| `F32` | `VAL-COA-05` | Perbarui akun uji berjenis Aset, pindahkan induk ke 2-1000 | `409` dengan pesan yang sama |
| `F33` | `FR-FIN-052` | Perbarui kode akun uji sebelum dipakai jurnal; regresi: akun yang sudah `Posted` | Sebelum `Posted` boleh; sesudah → `409` (lama) |
| `F34` | `FR-FIN-054` | Perbarui 1-1000: Lancar + Kas dan Setara Kas; buka Rincian 1-1004 | 1-1004 "Lancar (ikut 1-1000)" · "Kas dan Setara Kas (ikut 1-1000)" |
| `F35` | `VAL-COA-08`..`10` | Lewat Swagger: klasifikasi pada 4-1002; sub-klasifikasi pada 2-1001; sub-klasifikasi Persediaan pada 1-4001 sesudah 1-4000 diisi Tidak Lancar | Ketiganya `400` dengan pesan masing-masing |
| `F36` | `VAL-COA-11` | Akun uji di bawah 1-1000 diberi sub-klasifikasi Kas sendiri; lalu ubah 1-1000 menjadi Tidak Lancar | `409` menyebut kode akun uji; 1-1000 tidak berubah |
| `F37` | `FR-FIN-054` | Perbarui 1-1003 menjadi Tidak Lancar | 1-1003 efektif Tidak Lancar, sub-klasifikasi "Tidak berlaku"; 1-1004 tetap Lancar · Kas |
| `F38` | `FR-FIN-057` | Buka neraca 2026-09 sebelum dan sesudah `F34` | Sebelum: peringatan menyebut 1002, 1-1003, 1-1004 (dan akun bersaldo lain yang belum lengkap); sesudah: ketiganya hilang. Total aset, liabilitas, ekuitas, dan status seimbang **sama** di kedua pembukaan. Tautan membuka Daftar COA tersaring "Belum lengkap" |
| `F39` | `FR-FIN-055` | Sesudah `F34`, periksa log; buka neraca periode lain | Log `ChartOfAccount.Classification` memuat kode, nilai sebelum dan sesudah, pelaku; klasifikasi berlaku pada semua periode termasuk kolom pembanding |
| `F40` | `FR-FIN-058` | Akun tanpa `ChartOfAccount : Create` memanggil `GET next-code`; tanpa `ChartOfAccount : Update` memanggil `PUT` berkelompok `classification` | `403` keduanya |
| `F41` | `FR-FIN-056` | Buat jurnal manual ke akun uji yang klasifikasinya belum lengkap | Jurnal diterima seperti biasa (`ACC-DEC-168`) |

Regresi yang wajib diulang sesudah amendment: `F8`, `F9`, `F14` (neraca dan laba rugi memakai hierarki
yang sama), dan Tambah/Perbarui akun lama tanpa klasifikasi (permintaan lama yang tidak mengirim
`classification` tidak mengubah apa pun).

### Register hasil developer verification

| Skenario | Tanggal | Hasil | Bukti |
|---|---|---|---|
| `F25` | 25 Nov 2026 | `PASS` (API dan layar) — pratinjau `1-1005`; simpan otomatis berkode; layar menampilkan usulan dan toast kode final | `fin-001-01-20261125` A1, A4, A5; L2, L5 |
| `F26` | 25 Nov 2026 | `PASS` — `1-5000`, `3-4000`, `1-3001.01` | A2 |
| `F27` | 25 Nov 2026 | `PASS` — `9901` dipertahankan, anak `9901.01` | A7 |
| `F28` | 25 Nov 2026 | `PASS` — dua `POST` serentak → `9901.02`, `9901.03`, nol `409` | A8 |
| `F29` | 25 Nov 2026 | `PASS` (API dan layar) — pesan `409` di bawah Kode Akun, Simpan mati | A3; L4 |
| `F30` | 25 Nov 2026 | `PASS` — kode hasil > 20 → `409`; batas 999/99 `SOURCE VERIFIED` | A9 |
| `F31` | 25 Nov 2026 | `PASS` (API dan layar) — jenis diwarisi dan terkunci di layar; berbeda `409`; jenis tak dikenal `400` | A4, A6, B1, B2; L2 |
| `F32` | 25 Nov 2026 | `PASS` | A11 |
| `F33` | 25 Nov 2026 | `PASS` (API dan layar) — Perbarui Akun tetap berisian kode | A12; L6 |
| `F40` | 25–26 Nov 2026 | `PASS` (API) — `next-code` `403`; `PUT` klasifikasi `403` | A13; `fin-001-02-20261125` K9 |
| `F34` | 26 Nov 2026 | `PASS` (API dan layar) — pewarisan dua tingkat, sumber 1-5000; rincian "(ikut 1-5000)" | `fin-001-02-20261125` K2; L3, L4 |
| `F35` | 26 Nov 2026 | `PASS` (API) — empat penolakan `400` | K4 |
| `F36` | 26 Nov 2026 | `PASS` (API dan layar) — `409` pada ubah klasifikasi dan pindah induk; pesan tampil di form | K5; L6 |
| `F37` | 26 Nov 2026 | `PASS` (API dan layar) | K6; L7, L7b |
| `F39` | 26 Nov 2026 | `PASS` (API) sesudah perbaikan — log memuat kode akun dan nilai lama → baru; putaran pertama `FAIL` (`LoggerService` tidak menyimpan muatan) | K11, K11b |
| `F41` | 26 Nov 2026 | `PASS` — draft jurnal diterima lalu dihapus | K10 |
| `F38` | 26 Nov 2026 | `PASS` (API dan layar) — sebelum: 1002, 1-1003, 1-1004, 1-3002; sesudah 1-1000 diisi: hanya 1-3002; angka, CSV, dan laba rugi identik; peringatan layar ikut berkurang, tidak tercetak, tautan ke Daftar COA tersaring | `fin-001-03-20261126` G1–G6, G9b; V1–V9 |


---

## 6. Amendment ACC-FIN-008-04 Rasio keuangan *(26 November 2026, **`approved`**, `GATE-DESAIN-1126`)*

Diuji lewat API dan layar terhadap database dev, **tanpa SQL tulis** dan tanpa automated test
(`ACC-DEC-081`). DB dev 26 November 2026: total aset Rp 3.230.000, liabilitas Rp 0, ekuitas
Rp 3.230.000; tahun buku 2025 tidak ada; empat akun aset bersaldo tanpa klasifikasi (1002, 1-1003,
1-1004, 1-3002). Skenario yang membutuhkan data lain (liabilitas lancar, tahun buku 2025) memakai data
uji yang dibuat lewat aplikasi **hanya atas izin Rizki**; tanpa izin, jalurnya dibuktikan dari source dan
simulasi respons di layar, dan dicatat apa adanya.

| Skenario | Requirement | Langkah | Hasil yang diharapkan |
|---|---|---|---|
| `F42` | `FR-FIN-059` | `GET financial-ratios` 2026-09 | `200`; tepat delapan baris urutan kode 1..8; Debt to Asset `0.00` persen dan Debt to Equity `0.00` kali berstatus Dihitung; Net Profit Margin berstatus Dihitung bila pendapatan YTD ≠ 0 |
| `F43` | `VAL-RAT-09` | Klasifikasi seluruh akun aset bersaldo diisi sementara (izin), lalu `GET` 2026-09 | Current, Quick, Cash berstatus N/A "Penyebut bernilai nol." (liabilitas lancar Rp 0) |
| `F44` | `FR-FIN-061`, `VAL-RAT-06`, `07` | `GET` 2026-09 apa adanya; lalu dengan satu akun aset lancar tanpa sub-klasifikasi | Pertama: Current/Quick/Cash Tidak dapat dihitung, alasan menyebut 1002, 1-1003, 1-1004, 1-3002; Debt to Asset tetap Dihitung. Kedua: Current Dihitung atau N/A, Quick/Cash Tidak dapat dihitung |
| `F45` | `FR-FIN-063` | Bandingkan pembilang/penyebut Debt to Asset dan Debt to Equity dengan layar Neraca 2026-09; Net Profit Margin dengan kolom YTD Laba Rugi | Sama persis sampai rupiah |
| `F46` | `FR-FIN-059` | Data uji berizin: jurnal liabilitas lancar Rp 400.000 dan klasifikasi lengkap | Current = aset lancar ÷ 400.000, dua desimal, satuan kali; Quick dan Cash sesuai komponen `ACC-DEC-167` |
| `F47` | `FR-FIN-060`, `VAL-RAT-04`, `05` | `GET` 2026-09 (tahun 2025 tidak ada) | ROA/ROE kini Tidak dapat dihitung dengan alasan saldo awal; seluruh kolom pembanding Tidak dapat dihitung "Tahun buku 2025 belum ada di sistem." |
| `F48` | `FR-FIN-062`, `VAL-RAT-08` | Simulasi respons neraca tidak seimbang di layar; dari source untuk backend | Tujuh rasio Tidak sah dengan alasan selisih; Net Profit Margin tetap tampil |
| `F49` | `FR-FIN-064` | Buka menu Akuntansi → Rasio Keuangan; pilih 2026-09 | Butir tepat sesudah Neraca; delapan baris dengan rumus, pembilang ÷ penyebut, lencana status dan alasan; peringatan klasifikasi bertaut ke Bagan Akun tersaring |
| `F50` | `FR-FIN-065` | Unduh CSV; Cetak | CSV ber-BOM, `;`, koma desimal, hasil `0,00%`/`0,00x`, status bertulis; tampilan cetak tanpa filter, tombol, dan peringatan klasifikasi |
| `F51` | `FR-FIN-065` | Periksa log sesudah `F42` dan `F50` | `FinancialStatement.Ratio` dan `GeneralLedger.Export` tercatat; teks pesan memuat jumlah status, tanpa nominal |
| `F52` | `FR-FIN-066` | Akun tanpa `GeneralLedger : Read` memanggil kedua endpoint | `403` |

**Regresi.** Skenario laba rugi dan neraca bagian 4 serta `F38` tetap lulus tanpa perubahan angka maupun susunan,
karena `GetBalanceSheetAsync` dan `GetIncomeStatementAsync` tidak disentuh.

### Register hasil developer verification — bagian 6

| Skenario | Tanggal | Hasil | Bukti (`fin-008-04-20261126`) |
|---|---|---|---|
| `F42` | 26 Nov 2026 | `PASS` (API) — delapan baris; DAR `0.00` persen, DER `0.00` kali, NPM `100.00` persen berstatus Dihitung | H1 |
| `F43` | 26 Nov 2026 | `PASS` (API) — sesudah klasifikasi sementara 1-1000 dan 1-3000: Current/Quick/Cash N/A "Penyebut bernilai nol." | H5 |
| `F44` | 26 Nov 2026 | `PASS` (API) — 4 akun penyebab disebut; bagian 2: hanya sub-klasifikasi kosong → Current N/A, Quick/Cash tidak dapat dihitung menyebut 1-3002 | H3, H7 |
| `F45` | 26 Nov 2026 | `PASS` (API) — DAR/DER = total Neraca (0; 3.230.000; 3.230.000); NPM = Laba Rugi YTD (2.000.000 ÷ 2.000.000) | H2 |
| `F46` | 26 Nov 2026 | `SOURCE VERIFIED` — jurnal liabilitas lancar tidak diizinkan; agregasi pembilang terbukti runtime (3.230.000 / 2.330.000 / 2.330.000) | H6 |
| `F47` | 26 Nov 2026 | `PASS` (API) — pembanding dan ROA/ROE kini tidak dapat dihitung, alasan kontrak | H4 |
| `F48` | 26 Nov 2026 | Backend `SOURCE VERIFIED`; layar `PASS` (simulasi `route.fulfill`) — tujuh Tidak sah, NPM tetap, catatan ekuitas negatif | `fin-008-04-20261126` V6 |
| `F49` | 26 Nov 2026 | `PASS` (layar) — menu sesudah Neraca, delapan baris, lencana + alasan, peringatan klasifikasi bertaut | V1–V4, V7 |
| `F50` | 26 Nov 2026 | `PASS` (API, CSV) — BOM, `;`, `0,00%`/`0,00x`/`100,00%`, status bertulis, empat Catatan; layar: unduh dan cetak `PASS` | H9; V5 |
| `F51` | 26 Nov 2026 | `PASS` — log `FinancialStatement.Ratio` jumlah status di teks pesan, WRN 404, `GeneralLedger.Export`; tanpa nominal | H12 |
| `F52` | 26 Nov 2026 | `PASS` — `403` pada kedua endpoint | H10 |


---

## 7. Amendment ACC-FIN-013 Anggaran *(26 November 2026, **`draft`**)*

Diuji lewat API dan layar terhadap database dev, **tanpa SQL tulis** dan tanpa automated test
(`ACC-DEC-081`), sesudah migration `AddAccountingBudget` diterapkan Rizki. Tahun uji: tahun buku yang sudah
punya dua belas periode di DB dev (2026; Januari–Oktober 2026 berstatus sesuai DB dev saat uji). Uji
persetujuan meninggalkan versi Disetujui/Digantikan yang tidak dapat dihapus — **izin Rizki diminta saat
uji**. Empat mata memakai akun penyetuju uji yang ada.

| Skenario | Requirement | Langkah | Hasil yang diharapkan |
|---|---|---|---|
| `F53` | `FR-FIN-067` | Buat anggaran 2026; buat lagi | Pertama `201` v1 Draft; kedua `409` |
| `F54` | `VAL-BGT-02` | Buat anggaran untuk tahun tanpa periode (mis. 2028) | `422` |
| `F55` | `FR-FIN-068` | Simpan baris: 4-1001 tanpa unit; 5-xxxx dengan unit | `200`; total per bulan Pendapatan/Beban benar |
| `F56` | `VAL-BGT-05`..`08` | Baris akun Aset; akun induk; Beban tanpa unit; nilai negatif; baris kembar | Masing-masing `400` menyebut nomor baris |
| `F57` | `FR-FIN-069` | Layar: isi tahunan Rp 1.000.000, Sebar rata | Jan–Nov Rp 83.333, Des Rp 83.337 |
| `F58` | `FR-FIN-070` | Unduh templat CSV (versi kosong), isi, unggah | Baris tersimpan sama dengan berkas; unduh ulang menghasilkan isi yang sama |
| `F59` | `VAL-BGT-16` | Unggah CSV dengan satu kode akun salah | `400` daftar galat; baris lama tidak berubah |
| `F60` | `FR-FIN-071` | Ajukan; pengaju mencoba menyetujui | Diajukan; `403` |
| `F61` | `FR-FIN-071` | Tarik oleh pengaju; ajukan ulang | Draft; Diajukan |
| `F62` | `FR-FIN-071` | Penyetuju menolak tanpa alasan, lalu dengan alasan; penyusun memperbaiki dan mengajukan ulang | `400`; Ditolak + alasan tampil; Diajukan |
| `F63` | `FR-FIN-071` | Penyetuju menyetujui | Disetujui; riwayat empat aksi |
| `F64` | `FR-FIN-072` | Revisi v1 → v2 berisi salinan baris; buat revisi kedua saat v2 masih Draft | `201`; `409` |
| `F65` | `VAL-BGT-10` | v2 mengubah nilai bulan berperiode bukan Terbuka; lalu hanya bulan Terbuka | `422` menyebut baris; `200` |
| `F66` | `FR-FIN-072` | Setujui v2 | v2 Disetujui, v1 Digantikan; daftar menampilkan keduanya |
| `F67` | `VAL-BGT-15`, `FR-FIN-073` | Hapus v Disetujui; hapus Draft | `409`; terhapus |
| `F68` | `FR-FIN-074` | Akun tanpa `Budget : Approve` menyetujui; tanpa `Budget : Read` membuka daftar | `403`; layar menolak |

**Regresi.** Laporan keuangan, jurnal, dan bagan akun tidak berubah (anggaran tidak menulis ke tabel lain).
