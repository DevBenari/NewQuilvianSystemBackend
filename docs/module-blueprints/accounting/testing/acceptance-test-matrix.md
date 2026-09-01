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

### `UAT-15` — Pembukuan dua badan hukum tidak tercampur *(berhasil)*

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
