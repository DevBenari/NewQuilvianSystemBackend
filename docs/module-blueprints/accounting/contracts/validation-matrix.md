# Accounting — Validation Matrix

| Field | Value |
|---|---|
| `contract_version` | `ACC-VALIDATION-0.3` |
| Status | `draft` — approval adalah tindakan manusia |
| Owner | Rizki (Product/Domain Owner Accounting) |
| `approved_by` / `approved_at` | Belum ada |
| `input_revision` | `00-interview-decisions.md@3`, `02-backend-architecture.md@3` |
| Traceability | `ACC-DEC-014`, `ACC-DEC-016`, `ACC-DEC-019` sampai `ACC-DEC-025`, `ACC-DEC-027`, `ACC-DEC-037` |
| Perubahan `0.1` → `0.2` | Bagian 8 ditambahkan: aturan mata uang MVP (`ACC-DEC-020`, `ACC-DEC-021`) dimaterialisasikan |
| Dampak kompatibilitas | Kontrak baru |

Pesan pada kolom "Pesan bagi pengguna" ditulis apa adanya untuk ditampilkan di layar. Ditulis
dalam Bahasa Indonesia yang dipahami orang umum, bukan istilah teknis.

## 1. Daftar akun

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| Kode akun wajib | Tambah, ubah | `AccountCode` kosong atau lebih dari 20 karakter | "Kode akun wajib diisi dan maksimal 20 karakter." | `400` |
| Nama akun wajib | Tambah, ubah | `AccountName` kosong atau lebih dari 200 karakter | "Nama akun wajib diisi dan maksimal 200 karakter." | `400` |
| Kode unik per badan hukum | Tambah, ubah | Sudah ada akun berkode sama pada badan hukum yang sama | "Kode akun {kode} sudah dipakai pada badan hukum ini." | `409` |
| Tingkat akun masuk akal | Tambah, ubah | `AccountLevel` di luar 1 sampai 5 | "Tingkat akun harus antara 1 sampai 5." | `400` |
| Induk harus badan hukum sama | Tambah, ubah | `ParentAccountId` menunjuk akun milik badan hukum berbeda | "Akun induk harus berasal dari badan hukum yang sama." | `409` |
| Induk tidak boleh dirinya sendiri | Ubah | `ParentAccountId` sama dengan `Id`, atau membentuk lingkaran | "Akun tidak dapat menjadi induk bagi dirinya sendiri." | `409` |
| Akun induk tidak menerima transaksi | Tambah, ubah | `IsPostable` benar padahal akun punya anak | "Akun induk tidak dapat menerima transaksi. Gunakan akun turunannya." | `409` |
| Menambah anak ke akun bertransaksi | Tambah | Akun yang hendak dijadikan induk sudah punya baris jurnal disahkan | "Akun {kode} sudah memiliki transaksi, sehingga tidak dapat diberi akun turunan." | `409` |
| Kode tidak berubah setelah dipakai | Ubah | `AccountCode` diubah padahal sudah ada baris jurnal disahkan | "Kode akun tidak dapat diubah karena sudah dipakai pada jurnal yang disahkan." | `409` |
| Akun bersaldo tidak dinonaktifkan | Nonaktifkan | Saldo akun bukan nol | "Akun masih bersaldo Rp {jumlah} dan tidak dapat dinonaktifkan. Pindahkan saldonya lebih dahulu lewat jurnal." | `409` |

**Contoh aturan saldo.** Akun `1-1201 Piutang Asuransi X` bersaldo Rp 15.000.000. Petugas menekan
Nonaktifkan. Sistem menolak dengan pesan "Akun masih bersaldo Rp 15.000.000 dan tidak dapat
dinonaktifkan. Pindahkan saldonya lebih dahulu lewat jurnal." Setelah petugas memindahkan
saldonya ke `1-1209 Piutang Lain-lain` lewat jurnal yang disahkan, saldo menjadi nol dan
penonaktifan berhasil.

## 2. Jenis jurnal

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| Kode jenis wajib dan unik | Tambah, ubah | Kosong, lebih dari 10 karakter, atau sudah dipakai | "Kode jenis jurnal wajib diisi, maksimal 10 karakter, dan belum boleh dipakai jenis lain." | `400` atau `409` |
| Awalan nomor wajib | Tambah, ubah | `NumberPrefix` kosong | "Awalan nomor jurnal wajib diisi." | `400` |
| Jenis sistem terkunci | Ubah | Mengubah kode atau awalan nomor pada jenis bertanda sistem | "Jenis jurnal {kode} dipakai sistem dan kode maupun awalan nomornya tidak dapat diubah." | `409` |

## 3. Jurnal — saat disimpan sebagai draft

Aturan pada bagian ini berlaku sejak penyimpanan. Perhatikan bahwa keseimbangan **tidak** termasuk
di sini, sesuai `ACC-DEC-025`.

| Aturan | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|
| Badan hukum wajib | `LegalEntityId` kosong | "Badan hukum wajib dipilih." | `400` |
| Jenis jurnal wajib | `JournalTypeId` kosong atau tidak aktif | "Jenis jurnal wajib dipilih." | `400` |
| Tanggal akuntansi wajib | `AccountingDate` kosong | "Tanggal akuntansi wajib diisi." | `400` |
| Periode harus ada | Tidak ada periode yang memuat tanggal akuntansi pada badan hukum itu | "Belum ada periode akuntansi untuk {bulan tahun}. Minta administrator membangkitkan periode tahun buku ini." | `422` |
| Keterangan wajib | `Description` kosong atau lebih dari 500 karakter | "Keterangan jurnal wajib diisi dan maksimal 500 karakter." | `400` |
| Satu baris satu sisi | Ada baris yang mengisi debit dan kredit sekaligus, atau keduanya nol | "Baris ke-{n}: isi salah satu saja, debit atau kredit, dan nilainya harus lebih dari nol." | `400` |
| Nilai tidak negatif | Ada `DebitAmount` atau `CreditAmount` bernilai negatif | "Baris ke-{n}: nilai tidak boleh negatif. Untuk membalik arah, pindahkan ke sisi sebaliknya." | `400` |
| Nomor baris unik | Ada `LineNumber` kembar dalam satu jurnal | "Nomor baris tidak boleh kembar." | `400` |
| Akun wajib ada dan aktif | `AccountId` tidak ditemukan atau tidak aktif | "Baris ke-{n}: akun tidak ditemukan atau sudah tidak aktif." | `400` |
| Akun harus menerima transaksi | Akun yang dipilih adalah akun induk | "Baris ke-{n}: akun {kode} adalah akun induk dan tidak dapat menerima transaksi." | `409` |
| Akun harus badan hukum sama | Akun milik badan hukum berbeda dari jurnalnya | "Baris ke-{n}: akun {kode} bukan milik badan hukum jurnal ini." | `409` |
| Cost Center wajib pada akun beban | Akun berjenis beban tetapi `CostCenterId` kosong | "Baris ke-{n}: akun beban {kode} wajib menyebutkan unit biaya." | `400` |
| Cost Center harus aktif dan sesuai | Cost Center tidak aktif, atau milik badan hukum berbeda | "Baris ke-{n}: unit biaya tidak aktif atau bukan milik badan hukum jurnal ini." | `409` |
| **Periode menerima jenis jurnal ini** | Status periode menolak jenis jurnal itu | "Periode {nama periode} sudah {status}. {keterangan jenis jurnal yang masih diterima}." | `422` |

**Baris terakhir diratifikasi owner 3 September 2026** (`ACC-TD-014`), menaikkan
`ACC-VALIDATION` dari `0.2` ke `0.3`. Sebelumnya aturan itu hanya terdaftar di bagian 4 — saat
pengajuan dan pengesahan — sehingga draft `JU` ke periode yang sudah tutup sementara tetap
tersimpan dan baru ditolak saat diajukan. Memeriksanya sejak penyimpanan menolak lebih awal
dengan pesan yang sama, dan tidak menghilangkan data apa pun.

Pemeriksaan di bagian 4 **tetap wajib dan tidak berkurang**: periode dapat berubah status sesudah
draft tersimpan, dan hanya pemeriksaan kedualah yang mencegah jurnal masuk ke periode yang sudah
terkunci.

**Contoh pesan bernomor baris.** Petugas mengisi baris ketiga dengan akun `5-1001 Beban Obat`
tetapi lupa mengisi unit biaya. Pesan yang muncul: "Baris ke-3: akun beban 5-1001 wajib
menyebutkan unit biaya." Nomor baris disertakan supaya petugas langsung tahu baris mana yang
harus diperbaiki, tanpa menebak.

## 4. Jurnal — saat diajukan dan saat disahkan

Sembilan syarat berikut diperiksa saat pengajuan, lalu **diperiksa ulang** saat pengesahan.
Seluruh aturan bagian 3 juga tetap berlaku.

| Aturan | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|
| Minimal dua baris | Jurnal punya kurang dari dua baris | "Jurnal harus memiliki sekurang-kurangnya dua baris." | `400` |
| Debit sama dengan kredit | Total debit tidak sama persis dengan total kredit | "Jurnal belum seimbang. Total debit Rp {debit}, total kredit Rp {kredit}, selisih Rp {selisih}." | `400` |
| Periode menerima jenis ini | Status periode menolak jenis jurnal ini | "Periode {nama periode} sudah {status}. {keterangan jenis jurnal yang masih diterima}." | `422` |
| Bukan menyetujui jurnal sendiri | Penyetuju sama dengan pembuat jurnal | "Anda tidak dapat menyetujui jurnal yang Anda buat sendiri." | `403` |
| Alasan penolakan wajib | Menolak tanpa mengisi alasan | "Alasan penolakan wajib diisi." | `400` |

**Contoh pesan selisih.** Petugas menyusun jurnal berisi debit Beban Obat Rp 3.000.000, debit
Beban Alat Habis Pakai Rp 1.500.000, dan kredit Persediaan Farmasi Rp 4.000.000. Saat menekan
Ajukan, muncul pesan: "Jurnal belum seimbang. Total debit Rp 4.500.000, total kredit
Rp 4.000.000, selisih Rp 500.000." Angka selisih disertakan supaya petugas tidak perlu
menghitung sendiri.

**Contoh pesan periode.** Jurnal Umum hendak disahkan ke periode yang sudah tutup sementara.
Pesan yang muncul: "Periode September 2026 sudah ditutup sementara. Hanya jurnal penyesuaian dan
pembalikan yang masih dapat disahkan."

## 5. Pembalikan dan koreksi

| Aturan | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|
| Hanya jurnal disahkan yang dapat dibalik | Jurnal belum berstatus disahkan | "Hanya jurnal yang sudah disahkan yang dapat dibalik." | `409` |
| Tidak boleh dibalik dua kali | Sudah ada jurnal pembalik yang menunjuk jurnal ini | "Jurnal ini sudah pernah dibalik dengan jurnal {nomor}." | `409` |
| Alasan wajib | `Reason` kosong | "Alasan pembalikan wajib diisi." | `400` |
| Cara koreksi wajib dipilih | `CorrectionType` kosong | "Pilih cara koreksi: pembalikan penuh atau jurnal penyesuaian." | `400` |
| Penyesuaian wajib punya baris | Cara koreksi penyesuaian tetapi `AdjustmentLines` kosong | "Jurnal penyesuaian harus memiliki baris selisih." | `400` |
| Penyesuaian harus seimbang | Baris selisih tidak seimbang | "Baris penyesuaian belum seimbang. Selisih Rp {selisih}." | `400` |
| Periode tujuan menerima | Periode tujuan jurnal pembalik menolak | "Periode {nama periode} tidak menerima jurnal pembalik." | `422` |

## 6. Periode akuntansi

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| Tahun buku belum ada | Bangkitkan | Periode tahun buku itu sudah pernah dibangkitkan | "Periode tahun buku {tahun} sudah pernah dibuat untuk badan hukum ini." | `409` |
| Tahun buku masuk akal | Bangkitkan | Tahun di luar 2000 sampai 2100 | "Tahun buku tidak masuk akal." | `400` |
| Alasan pembukaan wajib | Buka kembali | `Reason` kosong | "Alasan pembukaan kembali wajib diisi." | `400` |
| Hanya periode tertutup yang dibuka | Buka kembali | Periode masih terbuka | "Periode ini masih terbuka." | `409` |
| Hanya periode terbuka yang ditutup | Tutup | Periode sudah tutup permanen | "Periode ini sudah ditutup permanen." | `409` |
| Periode tidak dapat dihapus | Hapus | Selalu | "Periode akuntansi tidak dapat dihapus." | `409` |

## 7. Aturan yang bukan validasi isian

Empat hal berikut sering disangka validasi, padahal bukan. Semuanya ditegakkan di service dan
tidak pernah bergantung pada isian yang dikirim frontend.

| Hal | Kenapa bukan validasi isian |
|---|---|
| Nomor jurnal | Dibangkitkan sistem saat penyimpanan, tidak pernah dikirim pengguna |
| Periode akuntansi jurnal | Ditentukan sistem dari tanggal akuntansi, tidak pernah dipilih pengguna |
| `TotalDebit` dan `TotalCredit` pada jurnal | Dihitung sistem dari baris, nilai yang dikirim frontend diabaikan |
| Pengguna yang mengajukan, menyetujui, dan mengesahkan | Diambil dari pengguna yang sedang masuk, tidak pernah dari isian form |

Aturan terakhir penting untuk keamanan: **jangan pernah menerima identitas pelaku dari isian
form.** Bila diterima dari form, siapa pun dapat mengaku sebagai orang lain, dan `ACC-DEC-016`
menjadi tidak berarti.

## 8. Mata uang — `ACC-DEC-020` dan `ACC-DEC-021`

Bagian ini memateralisasikan dua keputusan yang sudah tertutup. Ia tidak membuka keduanya
kembali dan tidak menambah kemampuan apa pun ke MVP.

| Aspek | Ketentuan MVP |
|---|---|
| Base currency | `IDR` |
| Mata uang transaksi yang diterima untuk posting | `IDR` **saja** |
| Keseimbangan `TotalDebit` = `TotalCredit` | Diukur dalam `IDR` |
| Kolom `CurrencyCode` pada tabel jurnal MVP | **Tidak ada** |

Jurnal MVP seluruhnya dibuat manusia lewat layar Jurnal Manual dan implisit berdenominasi `IDR`.
Karena tidak ada jalur masuk mata uang lain, tidak ada kolom mata uang, dan **tidak ada validasi
isian mata uang di MVP** — tidak ada isian yang perlu divalidasi.

### Yang berlaku ketika Phase 2 menerima kejadian dari luar

Envelope kejadian Finance/AR/AP → Accounting **wajib** membawa `CurrencyCode` walaupun MVP hanya
`IDR`, supaya penolakan dapat dilakukan secara sah. Bila `CurrencyCode != "IDR"`:

| Ketentuan | Isi |
|---|---|
| Konversi otomatis | **Jangan dilakukan** |
| Posting ke buku besar | **Jangan dilakukan** |
| Hasil | State pemrosesan tertolak yang eksplisit dan terlihat, dapat diambil ulang setelah keputusan multi-currency turun |

Kontraknya ada di [cross-module-contract.md](cross-module-contract.md) bagian 4.

### `DEFERRED` — jangan ditambahkan ke MVP

Posting multi-currency, kurs, selisih kurs terealisasi, selisih kurs belum terealisasi, dan
revaluasi mata uang asing. Kelimanya menunggu keputusan tersendiri.
