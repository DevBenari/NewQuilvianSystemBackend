# Human Resource — Matriks Validasi

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Dokumen | `contracts/validation-matrix.md` |
| `contract_version` | `v6` |
| `last_changed_in` | `v8` — bagian 13 (`S-D2`) ditambahkan 3 November 2026; bagian lain tidak berubah sejak `v7` |
| Status | `draft` — **belum** `approved` |
| Owner | Technical owner (`HRD-DEC-015`) |
| `approved_by` / `approved_at` | **Hanya bagian 12** (`S-D1` Perencanaan Tenaga Kerja): `approved` oleh Pengguna — pemilik teknis dan produk (`HRD-DEC-015`) — pada 2026-11-03. Bagian lain dokumen ini **belum** disetujui, sehingga status dokumen tetap `draft` |
| `input_revision` | `00-interview-decisions.md` revision `16` (`HRD-DEC-046` s.d. `HRD-DEC-057`); `contracts/state-transition-matrix.md` `v6` |
| `input_hash` — decision log | `26da64e129b0c34a17b67b0a41f47986b1aedae92a600077d936b0741eb574ee` |
| Backend SHA | `e0ee42c752a5f92c5b1663ff88bef07a5859f79f` |
| Dampak kompatibilitas | Tidak ada aturan yang dihapus. Aturan baru bersifat memperketat, bukan melonggarkan |

---

## 0. Cara membaca dokumen ini

Dokumen ini adalah **satu-satunya** tempat kalimat penolakan hidup di seluruh blueprint HR.
Flowchart pada `flowcharts/` **MUST NOT** menyalin kalimat-kalimat ini; ia cukup menyebut sebab
penolakannya dengan singkat.

| Kolom | Isi |
| --- | --- |
| Aturan | Nama aturan yang mudah dirujuk |
| Berlaku pada | Data atau tindakan yang dijaga |
| Kondisi | Kapan penolakan terjadi |
| Pesan bagi pengguna | Kalimat yang **benar-benar dibaca petugas**, dalam Bahasa Indonesia, bukan istilah teknis |
| Kode | Kode status HTTP beserta penanda internalnya bila ada |

Penanda `[EXISTING]`, `[DECISION]`, dan `[OPEN]` dipakai sama seperti dokumen lain.

### 0.1 Aturan penulisan pesan

| Yang benar | Yang salah |
| --- | --- |
| "Saldo cuti Anda tidak mencukupi. Sisa saldo 2 hari, permohonan 5 hari." | "Insufficient leave balance" |
| "Periode ini tidak dapat ditutup karena masih ada 18 pengecualian yang belum diselesaikan." | "Validation failed: blocking exceptions exist" |
| "Kode jenis cuti sudah dipakai data lain, jadi tidak bisa disimpan." | "Unique constraint violation on LeaveTypeCode" |

Pesan **MUST** menyebut **apa yang terjadi** dan **apa yang bisa dilakukan pengguna
berikutnya**. Pesan yang hanya menyebut kegagalan tanpa jalan keluar dianggap belum selesai
ditulis.

---

## 1. Aturan yang berlaku lintas seluruh modul

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Wajib masuk | Seluruh endpoint HR | Pengguna belum masuk atau sesinya berakhir | "Sesi Anda sudah berakhir. Silakan masuk kembali." | `401` |
| Hak akses per aksi | Seluruh endpoint yang dijaga `[AccessPermission]` | Pengguna tidak memiliki butir hak akses yang diminta | "Anda tidak punya hak akses untuk tindakan ini." | `403` |
| Kepemilikan layanan mandiri | Seluruh endpoint di bawah `self-services` | Pengguna mencoba membuka atau mengubah data milik pegawai lain | "Data ini bukan milik Anda." | `403` |
| Data tidak ditemukan | Seluruh endpoint yang memakai identifier | Baris tidak ada, atau sudah ditandai terhapus | "Data yang Anda cari tidak ditemukan atau sudah dihapus." | `404` |
| Isian wajib | Seluruh endpoint yang menerima isian | Ada isian wajib yang kosong | "Isian *nama isian* wajib diisi." | `400` |
| Kode master unik | Seluruh master data HR | Kode yang dimasukkan sudah dipakai baris lain yang masih aktif | "Kode *nilai* sudah dipakai data lain, jadi tidak bisa disimpan." | `409` |
| Master yang sedang dipakai | Seluruh master data HR | Baris master dinonaktifkan atau dihapus padahal masih dirujuk transaksi berjalan | "Data ini masih dipakai *jumlah* transaksi yang sedang berjalan, jadi belum bisa dinonaktifkan." | `409` |
| Alasan wajib pada penolakan | Seluruh aksi tolak, minta perbaikan, kembalikan, batalkan, dan balikkan | Alasan tidak diisi | "Alasan wajib diisi supaya keputusan ini dapat ditelusuri." | `400` |

---

## 2. Kehadiran

### 2.1 Rekaman mentah kehadiran

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Rekaman mentah tidak dapat diubah | `HrdAttendanceRawLog` | Ada upaya mengubah atau menghapus isi rekaman mentah | Tidak ada endpointnya sama sekali — pencegahannya di tingkat rancangan, bukan pesan `[EXISTING]` | — |
| Pegawai tidak dikenali | Pemasukan rekaman mentah | Identitas pada rekaman tidak cocok dengan profil workforce mana pun | "Rekaman ini tidak dapat dikaitkan dengan pegawai mana pun. Periksa nomor identitas pada mesin absensi." | `422` |
| Rekaman kembar | Pemasukan rekaman mentah | Sudah ada rekaman dengan pegawai, waktu, dan jenis kejadian yang sama | "Rekaman ini sudah pernah masuk sebelumnya, jadi tidak dicatat dua kali." | `200` beserta penanda kembar |
| Waktu tidak masuk akal | Pemasukan rekaman mentah | Waktu kejadian berada di masa depan | "Waktu pada rekaman ini berada di masa depan, jadi tidak dapat diproses." | `422` |

### 2.2 Pemrosesan kehadiran

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Periode harus dapat disunting | Pemrosesan dan pemrosesan ulang | Periode kehadiran hari itu berstatus `Closed` atau `Cancelled` | "Periode kehadiran untuk tanggal ini sudah ditutup. Buka kembali periodenya lebih dulu bila memang perlu diproses ulang." | `409` |
| Hari sudah terkunci payroll | Pemrosesan ulang satu hari | Hari itu sudah ditandai terkunci karena sudah diserahkan ke payroll | "Data kehadiran tanggal ini sudah diserahkan ke payroll dan tidak dapat diproses ulang. Batalkan serah terimanya lebih dulu bila memang perlu." | `409` |
| Jadwal tidak dapat diselesaikan | Pemrosesan | Tidak ditemukan jadwal kerja yang berlaku untuk pegawai pada tanggal itu | Hari itu tetap diproses, tetapi memunculkan pengecualian bertipe jadwal tidak dapat diselesaikan. Petugas melihatnya di daftar pengecualian `[EXISTING]` | `200` beserta pengecualian |
| Pemrosesan satu hari tidak mengubah hari lain | Pemrosesan ulang | — | Bukan penolakan, melainkan invariant: satu transaksi per pegawai per hari `[EXISTING]` | — |

### 2.3 Periode kehadiran

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Pengecualian pemblokir menghalangi penutupan | Menutup periode | Masih ada pengecualian bertanda pemblokir payroll yang berstatus `Open` atau `UnderReview` | "Periode ini belum bisa ditutup karena masih ada *jumlah* pengecualian yang belum diselesaikan. Selesaikan atau abaikan dengan alasan tercatat lebih dulu." | `409` `[EXISTING]` |
| Koreksi berjalan menghalangi penutupan | Menutup periode | Masih ada permohonan koreksi yang belum selesai | "Periode ini belum bisa ditutup karena masih ada *jumlah* permohonan koreksi yang sedang berjalan." | `409` beserta penanda koreksi aktif `[EXISTING]` |
| Hanya periode yang dapat disunting yang boleh ditutup | Menutup periode | Statusnya bukan `Open` maupun `Reopened` | "Periode dengan status *status* tidak dapat ditutup." | `409` `[EXISTING]` |
| Hanya periode tertutup yang boleh dibuka kembali | Membuka kembali periode | Statusnya bukan `Closed` | "Hanya periode berstatus Closed yang dapat dibuka kembali." | `409` `[EXISTING]` |
| Hari yang sudah tertaut payroll menghalangi pembukaan kembali | Membuka kembali periode | Ada hari dalam periode yang sudah masuk snapshot payroll | "Periode ini tidak dapat dibuka kembali karena datanya sudah diserahkan ke payroll. Batalkan serah terimanya lebih dulu." | `409` `[EXISTING]` |
| Pekerjaan terjadwal yang berjalan menghalangi pembukaan kembali | Membuka kembali periode | Masih ada pekerjaan pemrosesan yang berjalan | "Periode ini sedang diproses. Tunggu sampai pemrosesan selesai." | `409` `[EXISTING]` |
| Periode tertutup tidak dapat dibatalkan | Membatalkan periode | Statusnya `Closed` | "Periode yang sudah ditutup tidak dapat dibatalkan." | `409` `[EXISTING]` |
| Rentang periode tidak boleh bertumpuk | Membuat periode | Rentang tanggalnya bertumpuk dengan periode lain yang aktif | "Rentang tanggal ini bertumpuk dengan periode *kode periode*." | `409` |

### 2.4 Koreksi kehadiran

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Hanya data sendiri | Membuat permohonan koreksi lewat layanan mandiri | Hari yang dikoreksi bukan milik pegawai yang mengajukan | "Anda hanya dapat mengajukan koreksi untuk kehadiran Anda sendiri." | `403` `[EXISTING]` |
| Alasan wajib | Mengajukan koreksi | Alasan kosong | "Alasan koreksi wajib diisi." | `400` |
| Periode harus dapat disunting saat penerapan | Menerapkan koreksi | Periode kehadiran hari itu sudah tertutup | "Koreksi ini tidak dapat diterapkan karena periode kehadirannya sudah ditutup." | `409` |
| `Applied` tidak dapat diturunkan | Menyelaraskan status dengan mesin persetujuan | Permohonan sudah berstatus `Applied` | "Permohonan ini sudah diterapkan dan tidak dapat dikembalikan ke status sebelumnya. Ajukan permohonan koreksi baru bila masih ada yang perlu diperbaiki." | `409` `[DECISION]` `HRD-DEC-022` — **belum ditegakkan hari ini** |
| Unggahan bukti kedua menggantikan yang pertama | Mengunggah bukti | Sudah ada bukti sebelumnya | Bukan penolakan. Berkas lama diganti dan berkas fisiknya dihapus setelahnya `[EXISTING]` | `200` |
| Pegawai yang diwakili wajib disebut | Mengajukan koreksi atas nama pegawai | Pegawai yang diwakili tidak diisi | "Pilih pegawai yang Anda wakili." | `400` `[DECISION]` `HRD-DEC-028` |
| Alasan mewakili wajib | Mengajukan koreksi atas nama pegawai | Alasan mengapa pegawai tidak dapat mengajukan sendiri tidak diisi | "Jelaskan mengapa pegawai tidak dapat mengajukan sendiri." | `400` `[DECISION]` `HRD-DEC-028` |

### 2.5 Pengecualian kerja di luar jadwal

`[DECISION]` `HRD-DEC-013` dan `HRD-DEC-025`.

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Tidak pernah otomatis menjadi lembur | Pemrosesan kehadiran | Ditemukan aktivitas kerja nyata di luar jadwal yang sah | Bukan penolakan. Sistem membuat pengecualian yang **menunggu keputusan atasan**, tidak menghitungnya sebagai lembur `[DECISION]` | — |
| Hanya pengecualian jenis itu yang dapat diklasifikasikan | Mengklasifikasikan pengecualian | Jenis pengecualiannya bukan kerja di luar jadwal | "Pengecualian jenis ini tidak memerlukan klasifikasi." | `422` `[DECISION]` |
| Klasifikasi wajib punya alasan bila tidak dikompensasi | Mengklasifikasikan pengecualian | Klasifikasi "tercatat tanpa kompensasi" dipilih tanpa alasan | "Jelaskan alasan mengapa jam kerja ini tidak dikompensasi." | `400` `[DECISION]` |

### 2.6 Serah terima kehadiran ke payroll

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Putaran payroll tidak boleh terminal | Menjalankan serah terima | Status putaran payroll sudah `Approved`, `Paid`, `Posted`, `Closed`, `Cancelled`, atau `Reversed` | "Putaran payroll ini sudah tidak menerima data baru." | `409` `[EXISTING]` |
| Serah terima bersifat idempoten | Menjalankan serah terima | Data yang sama sudah pernah diserahkan | Bukan penolakan. Sistem mengembalikan hasil yang sama dan **tidak** membuat snapshot kedua `[EXISTING]` | `200` beserta penanda idempoten |
| Data belum siap tidak ikut serah terima | Menjalankan serah terima | Ada hari yang masih punya pengecualian pemblokir | "*Jumlah* hari kehadiran belum siap diserahkan karena masih ada pengecualian yang belum diselesaikan." | `422` |

---

## 3. Cuti

### 3.1 Pengajuan cuti

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Saldo harus mencukupi | Mengajukan cuti | Jumlah hari yang diminta melebihi sisa saldo yang tersedia | "Saldo cuti Anda tidak mencukupi. Sisa saldo *sisa* hari, permohonan *jumlah* hari." | `422` `[EXISTING]` |
| Jenis cuti harus berlaku bagi pegawai | Mengajukan cuti | Jenis cuti yang dipilih tidak berlaku bagi jenis kepegawaian pemohon | "Jenis cuti ini tidak berlaku untuk Anda." | `422` |
| Tanggal tidak boleh bertabrakan | Mengajukan cuti | Rentang tanggalnya bertumpuk dengan permohonan cuti lain yang masih berjalan | "Anda sudah punya pengajuan cuti pada tanggal *tanggal*." | `409` |
| Cuti per jam hanya untuk jenis yang mengizinkan | Mengajukan cuti per jam | Jenis cuti yang dipilih tidak mengizinkan mode per jam | "Jenis cuti ini tidak dapat diambil per jam." | `422` `[EXISTING]` |
| Per jam dan setengah hari saling meniadakan | Mengajukan cuti | Keduanya dipilih bersamaan | "Pilih salah satu: setengah hari atau per jam, tidak keduanya." | `400` `[EXISTING]` |
| Jumlah menit wajib pada cuti per jam | Mengajukan cuti per jam | Jumlah menit tidak diisi | "Isi jam mulai dan jam selesai untuk cuti per jam." | `400` `[EXISTING]` |
| Perhitungan harus dari backend | Mengajukan cuti | — | Bukan penolakan, melainkan invariant. Frontend **MUST NOT** menghitung sendiri jumlah hari yang dipotong | — |

**Aturan perhitungan cuti per jam beserta contohnya** `[EXISTING]`.

> **Aturan.** Jumlah hari yang dipotong dihitung dari jumlah menit yang diminta dibagi menit
> kerja terjadwal pada hari itu, lalu dibulatkan sampai empat angka di belakang koma.
>
> **Contoh 1.** Seorang perawat dengan jadwal 480 menit sehari mengajukan cuti per jam selama
> 120 menit. Perhitungannya 120 ÷ 480 = 0,25 hari. Saldonya berkurang 0,25 hari, bukan 1 hari.
>
> **Contoh 2.** Pegawai lain dengan jadwal 420 menit sehari mengajukan 120 menit pada hari itu.
> Perhitungannya 120 ÷ 420 = 0,2857 hari. Angkanya berbeda dari contoh pertama, dan itu memang
> benar, karena porsi hari kerjanya berbeda.
>
> **Yang tersimpan pada buku besar saldo adalah pecahan hari**, bukan jam maupun menit.
> Konversinya terjadi satu kali di titik perhitungan.

> **Catatan teknis yang perlu diketahui pemilik teknis, bukan aturan yang disetujui.** Bila
> jadwal hari itu tidak dapat diselesaikan, sistem hari ini memakai angka bawaan 480 menit yang
> **tertulis di dalam kode**, bukan diambil dari master mana pun dan bukan pula kebijakan yang
> pernah diverifikasi ke pemilik produk. Apakah angka itu boleh dipakai, atau perhitungan
> seharusnya berhenti sampai jadwal yang sah tersedia, masih `[OPEN]` — `HRD-Q-48`.

### 3.2 Batas waktu dan nilai kebijakan yang belum diputuskan

| Aturan | Keadaan | Pemilik |
| --- | --- | --- |
| Berapa lama pengajuan menunggu sebelum menjadi kedaluwarsa, dan apa akibatnya bagi pegawai | `[OPEN]` `HRD-Q-26` | Pemilik produk |
| Berapa hari hak cuti per jenis pegawai | `[OPEN]` `HRD-Q-06` | Pemilik produk |
| Berapa lama sisa cuti yang dibawa berlaku sebelum hangus | `[OPEN]` `HRD-Q-06` | Pemilik produk |
| Apakah penyesuaian saldo wajib melewati persetujuan | `[OPEN]` `HRD-Q-27` | Pemilik produk |
| Apakah sisa hari dikembalikan penuh saat pemanggilan kembali | `[OPEN]` `HRD-Q-29` | Pemilik produk |

**Larangan yang mengikat dokumen ini:** angka-angka itu **MUST NOT** ditulis di sini. Menuliskan
"12 hari" atau "3 hari kerja" akan membuat pembaca berikutnya mengiranya sudah disetujui, padahal
tidak ada seorang pun yang pernah memutuskannya.

### 3.3 Pembatalan cuti

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Cuti yang sudah selesai tidak dibatalkan lewat jalur ini | Mengajukan pembatalan | Permohonan cuti sudah berstatus `Completed` | "Cuti ini sudah selesai dijalankan dan tidak dapat dibatalkan lewat pembatalan biasa." | `409` |
| Pengembalian saldo bersifat proporsional | Menerapkan pembatalan | Pembatalan terjadi setelah tanggal mulai cuti | Bukan penolakan. Yang dikembalikan hanya sisa hari yang belum terlewat `[EXISTING]` | `200` |

> **Contoh berangka.** Seorang pegawai mengambil cuti 5 hari mulai 1 September. Pada 3 September
> ia membatalkan sisanya. Dua hari sudah terlewat, sehingga yang dikembalikan ke saldo adalah
> **3 hari**, bukan 5. Layar pembatalan **MUST** menampilkan angka ini sebelum pegawai
> mengonfirmasi, supaya ia tidak mengira seluruh 5 hari akan kembali.

### 3.4 Pembalikan eksekusi cuti

`[DECISION]` `HRD-DEC-023` — enam syarat yang **wajib** dipenuhi.

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Butuh hak akses khusus | Membalikkan eksekusi cuti | Pengguna tidak memegang butir hak akses pembalikan | "Anda tidak punya hak akses untuk membalikkan eksekusi cuti." | `403` `[DECISION]` — `MISSING` hari ini |
| Alasan wajib | Membalikkan eksekusi cuti | Alasan tidak diisi | "Alasan pembalikan wajib diisi." | `400` `[DECISION]` — `MISSING` hari ini |
| Periode payroll terkunci menghalangi mutasi langsung | Membalikkan eksekusi cuti berstatus `Completed` | Periode payroll terkait sudah terkunci atau final | "Cuti ini sudah masuk payroll yang terkunci. Buat transaksi penyesuaian terpisah, jangan mengubah riwayat cuti yang sudah berjalan." | `409` `[DECISION]` — `MISSING` hari ini |
| Sudah pernah dibalik | Membalikkan eksekusi cuti | Eksekusi sudah berstatus `Reversed` | "Eksekusi cuti ini sudah pernah dibalikkan." | `409` `[EXISTING]` |

### 3.5 Pemanggilan kembali dari cuti

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Hanya cuti yang sedang berjalan | Mengajukan pemanggilan kembali | Permohonan cuti belum berstatus `Taken` | "Pemanggilan kembali hanya berlaku untuk cuti yang sedang berjalan." | `422` |
| Konfirmasi bukan syarat persetujuan | Menyetujui pemanggilan kembali | Pegawai belum mengonfirmasi | Bukan penolakan. `[DECISION]` `HRD-DEC-024` — persetujuan pemanggilan kembali adalah keputusan organisasi, bukan keputusan pegawai | — |
| Alasan wajib pada penandaan pemberitahuan tersampaikan | HR Manager menandai pemberitahuan sudah tersampaikan | Alasan tidak diisi | "Jelaskan alasan Anda menandai pemberitahuan ini sudah tersampaikan." | `400` `[DECISION]` — `MISSING` hari ini |

### 3.6 Penyesuaian saldo

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Alasan wajib | Membuat penyesuaian | Alasan tidak dipilih | "Pilih alasan penyesuaian saldo." | `400` `[EXISTING]` |
| Arah harus sesuai kebijakan alasan | Membuat penyesuaian | Alasan yang dipilih hanya mengizinkan penambahan, tetapi yang diminta pengurangan | "Alasan *nama alasan* hanya dapat dipakai untuk menambah saldo." | `422` |
| Saldo terkunci tidak dapat disesuaikan | Memasukkan penyesuaian ke buku besar | Saldo tujuan berstatus terkunci atau tertutup | "Saldo cuti pegawai ini sedang terkunci, jadi tidak dapat disesuaikan." | `409` |
| Saldo tidak boleh berubah tanpa jejak | Seluruh perubahan saldo | — | Bukan penolakan, melainkan invariant: setiap perubahan saldo **wajib** disertai satu baris buku besar di dalam transaksi yang sama `[EXISTING]` | — |

---

## 4. Lembur

### 4.1 Pengajuan lembur

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Rentang waktu tidak boleh bertumpuk | Mengajukan lembur | Rentang waktunya bertumpuk dengan pengajuan lembur lain milik pegawai yang sama | "Anda sudah punya pengajuan lembur pada rentang waktu ini." | `409` beserta penanda tumpang tindih `[EXISTING]` |
| Periode lembur harus terbuka | Mengajukan lembur | Periode lembur pada tanggal itu sudah ditutup | "Periode lembur untuk tanggal ini sudah ditutup." | `409` |
| Nominal berasal dari backend | Seluruh perhitungan lembur | — | Bukan penolakan, melainkan invariant. Frontend **MUST NOT** menghitung tarif sendiri `[EXISTING]` | — |
| Kelayakan lembur | Mengajukan lembur | Pegawai tidak memenuhi kebijakan lembur yang berlaku | "Anda belum memenuhi syarat pengajuan lembur menurut kebijakan yang berlaku." | `422` — isi kebijakannya `[OPEN]` `HRD-Q-06` |

### 4.2 Realisasi dan verifikasi lembur

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Kehadiran harus tersedia | Menghitung realisasi | Kehadiran hari itu belum diproses | "Kehadiran tanggal ini belum diproses, jadi realisasi lembur belum dapat dihitung." | `422` beserta penanda kehadiran belum siap `[EXISTING]` |
| Kehadiran harus ditemukan | Menghitung realisasi | Tidak ditemukan kehadiran pada tanggal itu | "Tidak ditemukan data kehadiran pada tanggal ini." | `422` `[EXISTING]` |
| Kehadiran harus lengkap | Menghitung realisasi | Kehadiran hari itu tidak lengkap, misalnya tidak ada absen pulang | "Data kehadiran tanggal ini belum lengkap, jadi lembur belum dapat dihitung." | `422` `[EXISTING]` |
| Harus ada waktu yang bertumpuk | Menghitung realisasi | Waktu lembur yang diajukan tidak bertumpuk sama sekali dengan kehadiran nyata | "Waktu lembur yang diajukan tidak cocok dengan kehadiran Anda pada hari itu." | `422` `[EXISTING]` |
| Tarif harus dapat ditentukan | Menghitung realisasi | Tidak ditemukan tarif yang berlaku untuk jenis hari dan pita waktunya | "Tarif lembur untuk hari ini belum diatur. Hubungi HR." | `422` `[EXISTING]` |
| Kebijakan memblokir | Menghitung realisasi | Kebijakan lembur melarang lembur pada kondisi itu | "Lembur pada kondisi ini tidak diizinkan menurut kebijakan yang berlaku." | `422` `[EXISTING]` |

### 4.3 Serah terima lembur ke payroll

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Realisasi wajib terverifikasi | Menyerahkan lembur ke payroll | Realisasi belum berstatus terverifikasi | "Lembur ini belum diverifikasi, jadi belum dapat diserahkan ke payroll." | `409` `[EXISTING]` |
| Verifikasi aktif wajib disetujui | Menyerahkan lembur ke payroll | Verifikasi aktif terbaru belum disetujui | "Verifikasi lembur ini belum disetujui." | `409` `[EXISTING]` |
| Putaran payroll tidak boleh terkunci | Membatalkan serah terima lembur | Putaran payroll sudah terkunci atau final | "Putaran payroll ini sudah terkunci, jadi serah terima tidak dapat dibatalkan." | `409` `[EXISTING]` |

---

## 5. Penjadwalan Kerja

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Bentrok jadwal terdeteksi sebelum disimpan | Menempatkan jadwal atau menyusun roster | Pegawai sudah punya jadwal lain yang bertabrakan pada tanggal itu | "Pegawai ini sudah punya jadwal pada tanggal *tanggal*." | `409` |
| Periode tertutup tidak dapat diubah jadwalnya | Mengubah jadwal | Tanggalnya berada dalam periode kehadiran yang sudah ditutup | "Jadwal pada periode kehadiran yang sudah ditutup tidak dapat diubah." | `409` |
| Perubahan berlaku surut lewat koreksi terkendali | Mengubah jadwal | Tanggalnya berada di masa lalu, atau menyentuh periode yang sudah diproses | "Perubahan jadwal untuk tanggal yang sudah lewat harus melalui permohonan koreksi, tidak dapat diubah langsung." | `409` `[DECISION]` `HRD-DEC-027` — **`MISSING` hari ini** |
| Penempatan saat ini dan yang akan datang tidak butuh persetujuan | Menempatkan jadwal | Tanggalnya sekarang atau yang akan datang, dan periodenya masih dapat disunting | Bukan penolakan. `[DECISION]` `HRD-DEC-027` — jangan membuat persetujuan untuk setiap suntingan kecil | — |
| Kecukupan tenaga minimum | Menerbitkan roster | Ada hari yang jumlah petugasnya kurang dari batas minimum unit | "Roster ini belum dapat diterbitkan karena *jumlah* hari belum memenuhi jumlah tenaga minimum." | `422` **Rencana** |

### 5.1 Tukar shift

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Hanya rekan yang dituju yang boleh menjawab | Menjawab permohonan tukar shift | Yang menjawab bukan pegawai yang dituju | "Hanya rekan yang dituju yang dapat menjawab permohonan ini." | `403` `[EXISTING]` |
| Harus berstatus menunggu jawaban rekan | Menjawab permohonan tukar shift | Statusnya bukan menunggu jawaban rekan | "Permohonan ini sudah tidak menunggu jawaban Anda." | `409` `[EXISTING]` |
| Rekan harus menerima lebih dulu | Meneruskan ke persetujuan atasan | Rekan belum menerima | "Permohonan ini belum dapat diteruskan ke atasan karena rekan yang dituju belum menerima." | `409` `[EXISTING]` — **tidak dapat dilewati** |
| Aturan istirahat antar shift | Mengajukan tukar shift | Pertukaran menghasilkan jeda istirahat yang lebih pendek dari yang diizinkan | "Pertukaran ini membuat jeda istirahat Anda terlalu pendek." | `422` — nilai jeda minimumnya `[OPEN]` `HRD-Q-06` |
| Jadwal harus masih dapat diubah | Menerapkan tukar shift | Salah satu tanggal sudah masuk periode kehadiran yang tertutup | "Salah satu tanggal yang ditukar sudah masuk periode kehadiran yang ditutup." | `409` |

---

## 6. Persetujuan Bersama

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Hanya penyetuju yang ditugaskan | Seluruh aksi persetujuan | Pelakunya bukan penyetuju yang ditugaskan pada baris itu | "Pengajuan ini tidak ditugaskan kepada Anda." | `403` `[EXISTING]` — **gate yang benar-benar berlaku** |
| Tugas harus sedang giliran | Seluruh aksi persetujuan | Langkahnya belum menjadi giliran, atau sudah diputuskan | "Pengajuan ini sudah tidak menunggu keputusan Anda." | `409` `[EXISTING]` |
| Alasan wajib pada keputusan negatif | Menolak, meminta perbaikan, mengembalikan | Alasan tidak diisi | "Alasan wajib diisi supaya pemohon tahu apa yang harus diperbaiki." | `400` |
| Definisi workflow harus ada | Mengajukan transaksi apa pun | Tidak ditemukan definisi workflow untuk jenis transaksi itu | "Jalur persetujuan untuk jenis pengajuan ini belum diatur. Hubungi HR." | `422` |
| Penyetuju harus dapat ditentukan | Mengajukan transaksi apa pun | Tidak ada satu pun penyetuju yang dapat ditentukan dari aturan langkah | "Belum ada penyetuju yang dapat ditentukan untuk pengajuan ini. Hubungi HR." | `422` |
| Delegasi tidak dapat disetujui sendiri | Menyetujui delegasi | Yang menyetujui adalah pemberi delegasi atau penerimanya | "Delegasi tidak dapat disetujui oleh pemberi maupun penerimanya sendiri." | `403` `[EXISTING]` — **pola pemisahan peran yang benar** |
| Persetujuan otomatis default mati | Batas waktu terlampaui | Definisi workflow tidak secara eksplisit mengizinkan persetujuan otomatis | Bukan penolakan. Pengajuan tetap menunggu; yang berjalan hanya pengingat dan eskalasi `[DECISION]` `HRD-DEC-030` | — |

**Aturan yang mengikat kotak masuk terpadu** `[DECISION]` `HRD-DEC-018`: kotak masuk **MUST NOT**
menyeragamkan aturan validasi. Permohonan cuti tetap diperiksa dengan aturan saldo cuti;
permohonan lembur tetap diperiksa dengan aturan kelayakan lembur. Perbedaan itu **tetap berlaku**
walaupun keduanya tampil pada satu halaman.

---

## 7. Administrasi Kepegawaian

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Perubahan data tidak langsung berlaku | Mengajukan perubahan data pribadi | — | Bukan penolakan, melainkan invariant. Data profil berubah hanya setelah permohonan diterapkan `[EXISTING]` | — |
| Sekurang-kurangnya satu rincian | Mengajukan perubahan data | Tidak ada satu pun rincian perubahan yang diisi | "Isi sekurang-kurangnya satu data yang ingin diubah." | `400` |
| Tanggal berlaku wajib | Menempatkan organisasi, jabatan, atasan, atau gaji | Tanggal mulai berlaku tidak diisi | "Tanggal mulai berlaku wajib diisi." | `400` |
| Hanya satu penempatan utama | Menandai penempatan sebagai utama | Sudah ada penempatan lain yang ditandai utama pada tanggal yang sama | "Sudah ada penempatan utama pada tanggal ini." | `409` |
| Nominal gaji tidak ditampilkan pada daftar lintas-pegawai | Membuka daftar penetapan gaji seluruh pegawai | — | Bukan penolakan. Nominal disembunyikan secara baku (`HRD-DEC-033`) dan tidak dikirim backend | — |
| Berlaku surut ke periode tertutup ditolak | Menetapkan penempatan atau SK dengan tanggal berlaku di masa lalu | Periode kehadiran yang tersentuh sudah tertutup (`HRD-DEC-050`) | "Tanggal berlaku jatuh pada periode kehadiran yang sudah ditutup. Tanggal efektif hanya diterima pada periode kehadiran yang masih terbuka." | `409` |

---

### 7.0 Validasi Workforce Core & Dokumen Peristiwa Kepegawaian — `HRD-DEC-046` s.d. `HRD-DEC-057`

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VR-WFC-01` Satu orang satu profil workforce | Pendaftaran Pegawai, Dokter, Pengguna Eksternal | Nomor NIK KTP atau Paspor sudah terdaftar pada profil workforce yang ada (`HRD-DEC-051`) | "Nomor identitas ini sudah terdaftar atas nama *nama*. Profil tidak dapat digandakan; sistem akan menautkan peran ke profil tersebut." | `409` |
| `VR-WFC-02` Kunci unit & jabatan pada form master | Form edit Master Data Pegawai | Pengguna mencoba mengubah unit organisasi atau jabatan dari form master (`HRD-DEC-047`) | "Unit dan jabatan tidak dapat diubah langsung dari formulir master data. Gunakan dokumen mutasi atau promosi resmi." | `422` |
| `VR-WFC-03` Tepat satu unit utama per tanggal | Penempatan organisasi (`WfpOrganizationAssignment`) | Penempatan utama baru beririsan dengan penempatan utama lama tanpa menutup yang lama (`HRD-DEC-049`) | "Pegawai sudah memiliki penempatan unit utama yang aktif pada tanggal ini. Penempatan utama tidak boleh bertumpuk." | `409` |
| `VR-WFC-04` Penempatan kembar persis ditolak | Penempatan organisasi, jabatan, atasan | Penempatan dengan unit dan jabatan yang sama persis diajukan pada rentang tanggal yang beririsan (`HRD-DEC-049`) | "Penempatan dengan unit dan jabatan yang sama persis sudah ada pada rentang tanggal ini." | `409` |
| `VR-WFC-05` Berlaku surut ke periode tertutup ditolak | Dokumen SK (`HrdPersonnelActionOrder`) dan penempatan | Tanggal efektif jatuh pada periode kehadiran yang sudah berstatus `Closed` (`HRD-DEC-050`) | "Tanggal berlaku jatuh pada periode kehadiran yang sudah ditutup. Buka kembali periodenya lebih dulu bila memang perlu." | `409` |
| `VR-WFC-06` Alasan wajib berlaku surut | Dokumen SK dan penempatan | Tanggal efektif lebih awal dari tanggal pengajuan tanpa alasan (`HRD-DEC-050`) | "Alasan penetapan tanggal berlaku surut wajib diisi." | `400` |
| `VR-WFC-07` Wajib unggah scan SK resmi bertandatangan | Pengajuan Dokumen SK | Dokumen SK diajukan (`/submit`) tanpa melampirkan berkas scan SK resmi yang bertandatangan (`HRD-DEC-056`) | "Lampiran berkas scan SK resmi yang sudah ditandatangani pejabat berwenang wajib diunggah sebelum dokumen diajukan." | `400` |
| `VR-WFC-08` Penyetuju SK berbeda dari pembuat | Persetujuan Dokumen SK (`/approve`) | Akun pengguna yang menyetujui SK sama dengan pembuat dokumen SK (`HRD-DEC-031`, `HRD-DEC-056`) | "Anda tidak dapat menyetujui dokumen yang Anda buat sendiri. Dokumen dialihkan kepada pejabat penyetuju yang berwenang." | `403` |
| `VR-WFC-09` SK hanya batal sebelum tanggal berlaku | Pembatalan Dokumen SK (`/cancel-request`) | Dokumen SK sudah berstatus `Applied` atau tanggal efektif sudah lewat (`HRD-DEC-057`) | "Dokumen peristiwa yang sudah berlaku tidak dapat dibatalkan. Terbitkan dokumen SK baru untuk mengembalikan posisi pegawai." | `409` |
| `VR-WFC-10` Data pribadi sensitif dilarang edit langsung | Berkas profil pegawai (`MstWorkforceProfile`) | HR mencoba mengubah langsung rekening bank, NIK, NPWP, atau tanggungan tanpa permohonan (`HRD-DEC-052`) | "Data pribadi sensitif (rekening bank, NIK, NPWP, tanggungan) tidak dapat diubah langsung. Gunakan permohonan perubahan data berbukti dokumen." | `422` |
| `VR-WFC-11` Pemisahan verifikator perubahan data | Verifikasi perubahan data (`EmployeeProfileChange`) | Petugas yang memverifikasi sama dengan pengaju (termasuk HR pengaju atas nama) (`HRD-DEC-052`) | "Petugas yang memverifikasi perubahan data tidak boleh sama dengan yang membuat permohonan." | `403` |
| `VR-WFC-12` Batasan Employee 360 atasan langsung | Akses Employee 360 (`Employee360`) | Pengguna membuka Employee 360 pegawai yang bukan bawahan langsungnya hari ini (`HRD-DEC-053`) | "Anda tidak memiliki kewenangan untuk melihat data pegawai ini." | `403` |
| `VR-WFC-13` Batch data awal sekali per pegawai | Batch data awal penempatan (`HrdInitialPlacementBatch`) | Pegawai yang sudah pernah masuk batch yang disetujui dimasukkan lagi ke batch baru (`HRD-DEC-054`) | "Pegawai ini sudah pernah terdaftar pada batch data awal yang disetujui. Perubahan selanjutnya wajib lewat SK mutasi/promosi resmi." | `422` |

---

### 7.1 Perubahan penempatan dan remunerasi — `HRD-DEC-031`

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Persetujuan wajib sebelum berlaku | Empat jenis transaksi terpisah `HRD-DEC-036`: penetapan gaji, penempatan organisasi, penempatan jabatan, penetapan atasan | Penempatan diberlakukan sementara status persetujuannya belum disetujui | "Perubahan ini belum disetujui, sehingga belum dapat diberlakukan." | `409` |
| Penyetuju tidak boleh sama dengan pembuat | Sama | Pengguna yang menyetujui adalah pengguna yang mengajukan perubahan itu | "Anda tidak dapat menyetujui perubahan yang Anda ajukan sendiri. Perubahan ini diteruskan kepada pejabat yang berwenang." | `403` |
| Alasan penolakan wajib diisi | Sama | Penolakan dikirim tanpa alasan | "Alasan penolakan wajib diisi." | `400` |
| Kekurangan personel bukan pengecualian | Sama | Unit hanya punya satu petugas dan mencoba menyetujui sendiri | "Unit ini belum memiliki pejabat penyetuju yang berbeda. Perubahan diteruskan ke otoritas di atasnya." | `403` |
| Penyelesaian penyetuju otomatis | Sama | Pemrakarsa adalah satu-satunya pemegang butir `: Approve` di unitnya | Tidak ada pesan penolakan. Tugas **ditugaskan ulang** ke penyetuju tingkat lebih tinggi yang berwenang, dan muncul di daftar pengawasan HR bila tidak ada yang dapat ditentukan | `200` |

**Keadaan hari ini:** keempat aturan di atas **belum ditegakkan**. Endpoint persetujuan gaji
memakai butir hak akses yang sama dengan buat dan ubah, dan tidak ada pemeriksaan status
persetujuan sebelum penempatan berlaku. Ini tercatat sebagai `IMPLEMENTATION_WORK`, bukan sebagai
perilaku yang sudah ada.

### 7.3 Keamanan data gaji dan slip gaji — `HRD-DEC-037` s.d. `HRD-DEC-040`

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Slip gaji hanya milik sendiri | Slip gaji layanan mandiri | Pegawai meminta slip gaji milik pegawai lain, walaupun pengenalnya benar | "Anda hanya dapat melihat slip gaji milik Anda sendiri." | `403` |
| Kepemilikan diturunkan backend | Sama | Permintaan menyertakan pengenal pegawai atau profil workforce dari layar | Pengenal itu **diabaikan**. Kepemilikan diturunkan dari pengguna yang masuk | — |
| Otentikasi bertingkat wajib | Data gaji dan slip gaji | Data diminta tanpa sesi gaji sensitif yang berlaku | "Masukkan kata sandi Anda untuk membuka data gaji." | `401` |
| Sesi gaji sensitif kedaluwarsa | Sama | Sesi melewati batas waktu bawaan lima menit | "Sesi data gaji sudah berakhir. Masukkan kata sandi Anda lagi." | `401` |
| Sesi batal saat keadaan akun berubah | Sama | Pengguna keluar, sesi utama tidak sah, akun dinonaktifkan, atau kata sandi berubah | Sama seperti baris di atas | `401` |
| Konfigurasi kebijakan gaji hanya HR Manager | Kebijakan dan master gaji | Pengguna selain pemegang kewenangan itu membaca atau mengubah | "Anda tidak memiliki hak atas konfigurasi kebijakan gaji." | `403` |
| Faktor gaji tidak ditambah sendiri | Sama | Usulan faktor di luar Golongan, Level, Status kerja, dan Masa studi | Ditolak pada tinjauan, bukan oleh sistem | — |
| Riwayat kebijakan tidak dihapus | Sama | Perubahan kebijakan mencoba menimpa versi lama | "Kebijakan lama tidak dapat dihapus. Buat versi baru dengan tanggal berlaku." | `409` |
| Unduhan slip gaji lewat endpoint terautentikasi | Berkas slip gaji | Berkas diakses lewat URL statis yang dapat ditebak | Permintaan ditolak; berkas tidak dikembalikan | `403` |

**Catatan bentuk jawaban.** Baris kedua **bukan** penolakan — ia menegaskan bahwa pengenal dari
layar tidak dipakai sama sekali. Menolaknya justru akan membocorkan informasi tentang keberadaan
pengenal itu; mengabaikannya tidak.

**Keadaan hari ini:** seluruh aturan pada bagian 7.3 **belum ditegakkan**. Sesi gaji sensitif,
otentikasi bertingkat, dan endpoint unduhan terautentikasi belum ada. Ini `IMPLEMENTATION_WORK`
turunan `HRD-DEC-038` dan `HRD-DEC-040`, bukan perilaku yang sudah berjalan.

### 7.2 Keterlihatan nominal gaji — `HRD-DEC-033`

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Nominal disembunyikan pada daftar lintas pegawai | Daftar penetapan gaji lintas pegawai | Permintaan daftar dikirim pengguna tanpa butir sensitif nominal | Tidak ada pesan penolakan. Jawaban tetap `200`, dan **kolom nominal tidak disertakan** | `200` |
| Nominal pada detail memerlukan butir sensitif | Detail penetapan gaji satu pegawai | Pengguna tanpa butir `WfpSalaryAssignment : ViewAmount` meminta detail | "Anda tidak memiliki hak untuk melihat nominal gaji." Data non-nominal tetap dikembalikan | `200` dengan nominal tersamarkan |
| Butir baca umum tidak membuka nominal | Sama | Pengguna memegang butir baca umum saja | Sama seperti baris di atas | `200` dengan nominal tersamarkan |
| Keterlihatan massal tidak diberikan | Laporan atau ekspor gaji lintas pegawai | Permintaan ekspor nominal massal | "Ekspor nominal gaji lintas pegawai belum tersedia." | `403` |

**Catatan bentuk jawaban yang penting.** Penyembunyian nominal **MUST** dilakukan dengan tidak
menyertakan nilainya pada jawaban, **bukan** dengan mengirim nilainya lalu menyembunyikannya di
layar. Menyembunyikan di layar berarti nominal tetap terkirim melalui jaringan dan tetap terbaca
siapa pun yang membuka alat pengembang peramban.

### 7.4 Kebijakan gaji dan penyesuaiannya — `HRD-DEC-041` s.d. `HRD-DEC-043`

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Hanya pendidikan terverifikasi | Evaluasi kebijakan gaji | Jenjang pendidikan belum diverifikasi HR | "Jenjang pendidikan belum diverifikasi, sehingga belum dapat dipakai untuk penyesuaian gaji." | `422` |
| Verifikasi bukan oleh pencatatnya | Verifikasi pendidikan | Pihak yang mencatat pendidikan mencoba memverifikasinya sendiri | "Verifikasi pendidikan harus dilakukan petugas yang berbeda." | `403` |
| Bukti dokumen wajib | Sama | Verifikasi dilakukan tanpa bukti dokumen terlampir | "Bukti pendidikan wajib dilampirkan sebelum diverifikasi." | `400` |
| Gaji tidak berubah diam-diam | Perubahan faktor pegawai | Perubahan pendidikan, golongan, level, atau status kerja mencoba mengubah gaji efektif langsung | Tidak ada perubahan gaji. Yang terbentuk adalah **calon penyesuaian** | `200` |
| Calon tidak mengubah gaji | Calon penyesuaian gaji | Calon diterima lalu mencoba langsung memberlakukan gaji | "Penyesuaian gaji harus melewati pengajuan dan persetujuan penetapan gaji." | `409` |
| Kebijakan berlaku tidak disunting | Versi kebijakan gaji | Versi berstatus berlaku mencoba disunting | "Kebijakan yang sudah berlaku tidak dapat diubah. Buat versi baru." | `409` |
| Riwayat kebijakan dipertahankan | Sama | Versi lama mencoba dihapus | "Versi kebijakan sebelumnya tidak dapat dihapus." | `409` |
| Kewenangan kebijakan gaji | Sama | Selain `HR Manager` membaca atau mengubah kebijakan gaji | "Anda tidak memiliki hak atas konfigurasi kebijakan gaji." | `403` |
| Masa kerja bukan faktor gaji | Evaluasi kebijakan gaji | Usulan memakai masa kerja sebagai kriteria kebijakan gaji | Ditolak pada tinjauan. **Di luar cakupan MVP saat ini** — `HRD-DEC-045` | — |

**Keadaan hari ini:** seluruh aturan pada bagian 7.4 **belum ditegakkan**. Baris kedua khususnya —
verifikasi pendidikan hari ini memakai butir hak akses yang sama dengan buat dan ubah.
`IMPLEMENTATION_WORK` turunan `HRD-DEC-041` dan `HRD-DEC-043`.

## 8. Pengembangan Orang dan Lifecycle

### 8.1 Penilaian kinerja

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Seluruh rincian wajib berskor sebelum final | Memfinalkan penilaian | Masih ada rincian KPI yang belum diberi skor | "Masih ada *jumlah* penilaian yang belum diberi skor." | `422` `[EXISTING]` — guard nyata |
| Penilaian final tidak dapat diubah | Mengubah penilaian atau rinciannya | Penilaian sudah difinalkan | "Penilaian ini sudah difinalkan dan tidak dapat diubah." | `409` `[EXISTING]` |
| Pengakuan hanya setelah final | Mengakui hasil penilaian | Penilaian belum difinalkan | "Penilaian ini belum difinalkan." | `409` `[EXISTING]` |
| Satu pegawai satu penilaian per siklus | Membuat penilaian | Sudah ada penilaian untuk pegawai dan siklus yang sama | "Pegawai ini sudah dinilai pada siklus *nama siklus*." | `409` |
| Pegawai hanya melihat hasilnya sendiri | Membuka penilaian | Penilaian milik pegawai lain | "Data ini bukan milik Anda." | `403` |

### 8.2 Pelatihan dan kompetensi

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Tanggal selesai tidak boleh mendahului tanggal mulai | Mencatat pelatihan | Tanggal selesai lebih awal dari tanggal mulai | "Tanggal selesai tidak boleh lebih awal dari tanggal mulai." | `400` |
| Sertifikat sebagai bukti | Mencatat pelatihan yang menghasilkan sertifikat | Nomor sertifikat atau berkasnya tidak ada | "Lampirkan sertifikat atau isi nomor sertifikatnya." | `400` |
| Peringatan sebelum masa berlaku habis | Sertifikat dan asesmen kompetensi | Masa berlaku mendekati habis | Bukan penolakan. Sistem menandai sebagai akan kedaluwarsa; jumlah harinya `[OPEN]` `HRD-Q-06` | — |
| Kompetensi tidak menentukan kewenangan klinis | Seluruh pencatatan kompetensi | — | Bukan penolakan, melainkan **batas**. Keterkaitan dengan kewenangan klinis `BLOCKED` — bagian `S-C1` | — |

### 8.3 Tindakan disiplin

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Status harus dari daftar yang sah | Mengubah status tindakan disiplin | Nilai status tidak ada di daftar | "Status tindakan disiplin tidak valid." | `400` `[EXISTING]` |
| Urutan perpindahan status | Mengubah status tindakan disiplin | Perpindahan tidak mengikuti urutan yang wajar | **Belum ada.** Hari ini kode hanya memeriksa keanggotaan daftar, bukan urutan. Ini kelemahan yang dicatat, bukan aturan yang berlaku | — |
| Pemisahan peran pada persetujuan | Menyetujui tindakan disiplin | Yang menyetujui adalah pembuatnya sendiri | **`[OPEN]` `HRD-Q-51`** — hari ini **tidak dicegah**. Apakah perlu dicegah adalah keputusan pemilik proses | — |
| Tingkatan izin untuk data paling rahasia | Membaca tindakan disiplin bertanda paling rahasia | Pengguna tidak memegang izin khusus | **`[OPEN]` `HRD-Q-52`** — tingkatan izin itu **belum ada** | — |
| Draft saja yang dapat diubah | Mengubah tindakan disiplin | Statusnya bukan draft | "Tindakan disiplin yang sudah diterbitkan tidak dapat diubah." | `409` `[EXISTING]` |

### 8.4 Pengunduran diri dan offboarding

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Tanggal terakhir bekerja wajib | Mengajukan pengunduran diri | Tanggal tidak diisi | "Tanggal terakhir bekerja wajib diisi." | `400` `[EXISTING]` |
| Hanya draft yang dapat diajukan | Mengajukan pengunduran diri | Statusnya bukan draft | "Pengajuan ini sudah dikirim sebelumnya." | `409` `[EXISTING]` |
| Serah terima hanya dari yang disetujui | Menjalankan serah terima | Pengunduran diri belum disetujui | "Serah terima hanya dapat dijalankan setelah pengunduran diri disetujui." | `409` `[EXISTING]` |
| Serah terima bersifat idempoten | Menjalankan serah terima | Serah terima sudah pernah dijalankan | Bukan penolakan. Sistem mengembalikan hasil yang sama `[EXISTING]` | `200` |
| Riwayat kepegawaian tidak dihapus | Pengunduran diri dan pemberhentian | — | Bukan penolakan, melainkan invariant. Pengunduran diri **tidak** menghapus riwayat kehadiran, cuti, kinerja, maupun payroll | — |
| Aset belum kembali menghalangi penutupan | Menutup daftar periksa offboarding | Masih ada tugas pengembalian aset yang belum selesai | "Daftar periksa ini belum dapat ditutup karena masih ada *jumlah* tugas yang belum selesai." | `409` **Rencana** |
| Pencabutan akun tidak otomatis | Serah terima offboarding | — | Bukan penolakan, melainkan **temuan**: pencabutan akun aplikasi **tidak** berjalan otomatis. Kontrak ke Identity `[OPEN]` `HRD-DEP-003` | — |

---

## 9. Payroll sisi HR

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Putaran terminal menolak data baru | Seluruh penulisan snapshot masukan | Status putaran payroll sudah terminal | "Putaran payroll ini sudah tidak menerima data baru." | `409` `[EXISTING]` |
| Tidak ada endpoint yang mengubah status pembayaran | Seluruh modul HR | — | Bukan penolakan, melainkan **invariant batas modul**. `[DECISION]` `HRD-DEC-009` — rantai HR berhenti setelah serah terima dijalankan | — |
| Bentuk serah terima ke Finance | Serah terima payroll | — | **`[OPEN]` `HRD-Q-10`** — **MUST NOT** dirancang | — |
| Perilaku bila Finance menolak batch | Serah terima payroll | — | **`[OPEN]` `HRD-Q-11`** — **MUST NOT** dirancang | — |

---

## 10. Aturan validasi yang **belum** ditegakkan, disusun berdasarkan besar akibatnya

Tabel ini adalah daftar pekerjaan pengerasan validasi. Setiap barisnya adalah aturan yang
**sudah diputuskan** tetapi **belum ada di kode**.

| No | Aturan | Sumber keputusan | Akibat bila dibiarkan |
| ---: | --- | --- | --- |
| 1 | `Applied` pada koreksi kehadiran tidak dapat diturunkan lewat sinkronisasi | `HRD-DEC-022` | Kehadiran harian dimutasi ulang; angka yang sudah masuk payroll berubah tanpa jejak yang jelas |
| 2 | Pembalikan eksekusi cuti wajib memenuhi enam syarat | `HRD-DEC-023` | Saldo dan kehadiran berubah tanpa alasan tercatat, bahkan setelah payroll terkunci |
| 3 | Perubahan jadwal berlaku surut wajib lewat koreksi terkendali | `HRD-DEC-027` | Kehadiran pada periode yang sudah diproses dihitung ulang dengan jadwal berbeda tanpa disadari |
| 4 | Penandaan pemberitahuan pemanggilan kembali wajib punya alasan dan jejak audit | `HRD-DEC-024` | Pegawai dapat ditandai sudah diberi tahu tanpa bukti apa pun |
| 5 | Alasan wajib pada klasifikasi pengecualian kerja di luar jadwal | `HRD-DEC-025` | Jam kerja dokter diputuskan tidak dikompensasi tanpa alasan tercatat |
| 6 | Kelengkapan permohonan koreksi atas nama pegawai | `HRD-DEC-028` | Koreksi dibuat atas nama orang lain tanpa jejak siapa yang membuatnya dan mengapa |
| 7 | Persetujuan otomatis hanya bila definisi workflow mengizinkan | `HRD-DEC-030` | Bila mesin dibangun tanpa pagar ini, seluruh transaksi HR dapat disetujui otomatis |

---

## 11. Traceability

| Kelompok aturan | Decision ID | Flow |
| --- | --- | --- |
| Rekaman mentah tidak berubah; periode dan pengecualian | `HRD-DEC-022`, `HRD-DEC-025` | `flows/02-attendance.md` |
| Koreksi kehadiran dan permohonan atas nama pegawai | `HRD-DEC-022`, `HRD-DEC-028` | `flows/07-attendance-correction.md` |
| Saldo, pengajuan, pembatalan, pembalikan, pemanggilan kembali cuti | `HRD-DEC-023`, `HRD-DEC-024` | `flows/03-leave.md` |
| Cuti per jam dan pemisahannya dari izin pulang cepat | `HRD-DEC-029`; `HRD-Q-47`, `HRD-Q-48` | `flows/08-early-leave-permission.md` |
| Kelayakan, realisasi, verifikasi, dan serah terima lembur | — | `flows/04-overtime.md` |
| Bentrok jadwal, roster, perubahan berlaku surut | `HRD-DEC-026`, `HRD-DEC-027` | `flows/05-work-scheduling.md` |
| Tukar shift dua tahap | — | `flows/06-shift-change-swap.md` |
| Kewenangan persetujuan, delegasi, batas waktu | `HRD-DEC-011`, `HRD-DEC-018`, `HRD-DEC-030` | `flows/09-unified-approval.md` |
| Perubahan data, penempatan, penetapan gaji | `HRD-DEC-012`; `HRD-Q-18`, `HRD-Q-19`, `HRD-Q-20` | `flows/01-employee-administration.md` |
| Batas payroll | `HRD-DEC-009`; `HRD-Q-10`, `HRD-Q-11` | `flows/10-payroll-processing-handoff.md` |
| Pengunduran diri dan offboarding | — | `flows/11-lifecycle-offboarding.md` |
| Pelatihan dan kompetensi | — | `flows/12-competency-training.md` |
| Penilaian kinerja | — | `flows/13-performance-management.md` |
| Tindakan disiplin | `HRD-Q-51`, `HRD-Q-52` | `flows/14-employee-relations-discipline.md` |

Aturan validasi untuk kredensial, kewenangan klinis, kesehatan kerja staf, perencanaan tenaga
kerja, rekrutmen, benefit, tiket HR, perjalanan dinas, dan reimbursement **tidak** ditulis di
sini. Seluruhnya `BLOCKED` atau `DEFERRED`.

---

## 12. Perencanaan Tenaga Kerja — `S-D1` (`last_changed_in` `v7`, 3 November 2026)

Seluruh aturan di bawah **belum ditegakkan di kode**. Nomor `VR-WFP-nn` stabil dan dirujuk
`testing/acceptance-test-matrix.md` bagian 15.

### 12.1 Standar kebutuhan tenaga dan angka beban

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VR-WFP-01` Satu draf terbuka per unit | Membuat draf standar | Sudah ada draf atau versi yang dikembalikan untuk unit itu | "Unit ini masih punya draf standar yang belum selesai. Lanjutkan draf itu atau batalkan lebih dulu." | `409` |
| `VR-WFP-02` Metode yang tersedia | Menyimpan baris standar | Metode Analisis Beban Kerja dipilih | "Metode Analisis Beban Kerja belum tersedia pada rilis ini. Hitung di luar sistem lalu isi hasilnya sebagai jumlah tetap." | `422` |
| `VR-WFP-03` Isian lengkap per metode | Mengajukan standar | Jumlah tetap tanpa angka minimum; rasio tanpa rasio, satuan beban, jumlah shift, atau faktor cadangan; cakupan layanan tanpa jumlah shift, orang per shift, atau faktor cadangan | "Baris jabatan {jabatan} belum lengkap untuk metode {metode}: {isian yang kurang}." | `422` |
| `VR-WFP-04` Faktor cadangan dan pembulatan dipilih sendiri | Mengajukan standar | Faktor cadangan kosong pada metode yang memerlukannya, atau cara pembulatan belum dipilih | "Faktor cadangan dan cara pembulatan wajib dipilih. Sistem tidak menetapkan nilai bawaan." | `422` |
| `VR-WFP-05` Faktor cadangan minimal 1 | Menyimpan baris | Faktor cadangan < 1,00 | "Faktor cadangan paling kecil 1,00." | `400` |
| `VR-WFP-06` Tanggal berlaku tidak mundur | Mengajukan standar | Tanggal berlaku sebelum hari ini | "Tanggal berlaku standar tidak boleh di masa lalu." | `422` |
| `VR-WFP-07` Satu jabatan satu baris | Menyimpan standar | Jabatan yang sama muncul dua kali | "Jabatan {jabatan} sudah ada pada standar ini." | `409` |
| `VR-WFP-08` Sumber angka beban wajib | Mengisi angka beban | Keterangan sumber kosong | "Tuliskan sumber angka beban, misalnya rekap sensus harian." | `400` |
| `VR-WFP-09` Angka beban tidak ganda | Mengisi angka beban | Sudah ada isian untuk unit, jabatan, dan tanggal mulai periode yang sama | "Angka beban periode ini sudah diisi. Ubah isian yang ada." | `409` |

### 12.2 Formasi dan batch formasi awal

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VR-WFP-10` Formasi tidak dapat diubah langsung | Endpoint tulis apa pun ke formasi | — | Tidak ada endpointnya; method lain menjawab "Formasi hanya berubah lewat dokumen yang disetujui." | `405` |
| `VR-WFP-11` Sekali per unit × jabatan | Menyimpan atau mengajukan batch | Unit × jabatan sudah punya formasi, atau sudah ada di batch lain yang belum ditolak/dibatalkan | "{unit} — {jabatan} sudah punya formasi awal. Perubahan formasi dilakukan lewat addendum." | `409` |
| `VR-WFP-12` Tidak ada baris gagal | Mengajukan batch | `InvalidRows > 0` | "Masih ada {n} baris yang belum valid. Perbaiki semua baris sebelum mengajukan." | `422` |
| `VR-WFP-13` Scan dokumen wajib | Mengajukan batch | Scan dokumen sumber belum diunggah | "Unggah scan dokumen formasi resmi sebelum mengajukan." | `422` |
| `VR-WFP-14` Jenis berkas | Mengunggah scan | Bukan PDF, JPG, atau PNG, atau lebih dari 10 MB | "Berkas harus PDF, JPG, atau PNG dengan ukuran paling besar 10 MB." | `400` |

### 12.3 Rencana tahunan dan addendum

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VR-WFP-15` Satu rencana tahunan per tahun | Membuat rencana | Sudah ada rencana tahunan untuk rumah sakit dan tahun yang sama yang tidak ditolak atau dibatalkan | "Rencana tahunan {tahun} sudah ada. Perubahan di tengah tahun memakai addendum." | `409` |
| `VR-WFP-16` Addendum hanya dari rencana yang disetujui | Membuat addendum | Rencana induk belum `Approved` | "Addendum hanya dapat dibuat dari rencana yang sudah disetujui." | `422` |
| `VR-WFP-17` Rencana yang disetujui terkunci | Mengubah rencana | Status `Approved` | "Rencana yang sudah disetujui tidak dapat diubah. Buat addendum." | `409` |
| `VR-WFP-18` Grade wajib per baris | Mengajukan rencana atau addendum | Baris tanpa grade | "Pilih grade untuk {unit} — {jabatan}. Biaya dihitung dari gaji tengah grade." | `422` |
| `VR-WFP-19` Alasan bila berbeda dari rekomendasi | Mengajukan | Usulan ≠ rekomendasi dan alasan kosong | "Usulan {unit} — {jabatan} berbeda dari rekomendasi {angka}. Tuliskan alasannya." | `422` |
| `VR-WFP-20` Koreksi Finance hanya pada langkahnya | Koreksi biaya | Pemanggil bukan penyetuju langkah Finance yang sedang aktif | "Koreksi biaya hanya dapat dilakukan Finance pada langkah persetujuan Finance." | `403` |
| `VR-WFP-21` Tarif belum tersedia harus dikoreksi | Menyetujui langkah Finance | Ada baris "tarif belum tersedia" tanpa angka koreksi Finance | "Ada baris yang tarif gradenya belum tersedia. Isi angka koreksi sebelum menyetujui." | `422` |

### 12.4 Permintaan headcount dan pemenuhannya

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VR-WFP-22` Sisa kursi cukup | Mengajukan permintaan dalam formasi atau pengganti | Jumlah porsi kerja kursi > sisa kursi | "Sisa kursi {jabatan} {unit} hanya {sisa}. Kurangi jumlah, atau ajukan sebagai tambahan di luar formasi." | `422` |
| `VR-WFP-23` Sisa kursi diperiksa ulang | Persetujuan terakhir `T11` | Sisa kursi tidak lagi cukup karena permintaan lain lebih dulu disetujui | "Sisa kursi sudah terpakai permintaan lain. Permintaan ini dikembalikan untuk disesuaikan." | `409` |
| `VR-WFP-24` Pengganti menyebut orangnya | Mengajukan permintaan pengganti | Kursi tanpa orang yang digantikan, atau orang itu belum berstatus akan kosong/keluar | "Pilih pegawai yang digantikan. Pegawai itu harus sudah tercatat akan keluar atau sudah keluar." | `422` |
| `VR-WFP-25` Sementara punya alasan dan akhir | Mengajukan permintaan sementara | Tanggal akhir atau orang yang tidak tersedia kosong | "Permintaan sementara wajib menyebut pegawai yang digantikan sementara dan tanggal berakhirnya." | `422` |
| `VR-WFP-26` Di luar formasi punya alasan | Mengajukan permintaan di luar formasi | Alasan kosong | "Tuliskan alasan penambahan kursi di luar formasi." | `422` |
| `VR-WFP-27` Rencana pemenuhan lengkap | Mengajukan | Jumlah kursi luar + internal ≠ jumlah kursi | "Tentukan sumber pemenuhan setiap kursi: rekrut luar atau pindahan internal." | `422` |
| `VR-WFP-28` Porsi kerja kursi | Menyimpan kursi | `SeatFte` di luar 0,10–1,00 | "Porsi kerja satu kursi antara 0,10 dan 1,00." | `400` |
| `VR-WFP-29` Penempatan harus cocok | Menautkan pemenuhan | Unit atau jabatan penempatan berbeda dengan permintaan, atau penempatan bukan utama | "Penempatan ini untuk {unit} — {jabatan}, tidak cocok dengan permintaan." | `422` |
| `VR-WFP-30` Satu penempatan satu kursi | Menautkan pemenuhan | Penempatan atau SK sudah menautkan kursi lain yang tidak dibatalkan | "Penempatan ini sudah memenuhi kursi {nomor permintaan}." | `409` |
| `VR-WFP-31` Sumber sesuai jenis kursi | Menautkan pemenuhan | Kursi internal ditautkan ke penempatan hasil rekrut, atau kursi luar ditautkan ke SK | "Kursi internal dipenuhi lewat SK; kursi rekrut luar dipenuhi lewat penempatan pegawai baru." | `422` |
| `VR-WFP-32` Lepas tautan hanya sebelum berlaku | Melepas tautan | Kursi sudah `Fulfilled` | "Kursi ini sudah terisi dan tidak dapat dilepas." | `409` |
| `VR-WFP-33` Perubahan rencana pemenuhan | Mengubah sumber kursi | Pemanggil bukan HR Manager, HR Manager adalah pembuat permintaan, kursi sudah terisi, atau alasan kosong | "Perubahan rencana pemenuhan hanya oleh HR Manager yang bukan pembuat permintaan, untuk kursi yang belum terisi, dengan alasan." | `403` / `422` |
| `VR-WFP-34` Tarik kembali sebelum disetujui | Menarik kembali | Permintaan sudah siap direkrut | "Permintaan sudah disetujui. Gunakan pembatalan kursi." | `409` |
| `VR-WFP-35` Batal hanya kursi kosong | Mengajukan pembatalan | Kursi yang dipilih sudah `Fulfilled` atau `Linked` | "Kursi {nomor} sudah terisi atau sedang ditautkan dan tidak dapat dibatalkan." | `422` |
| `VR-WFP-36` Tutup lowongan dulu | Mengajukan pembatalan | Masih ada lowongan terbuka untuk kursi itu | "Masih ada lowongan terbuka untuk kursi ini. Tutup atau kurangi lowongan lebih dulu." | `409` |

### 12.5 Biaya, potret, dan pengaturan

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VR-WFP-37` Biaya hanya bagi yang berhak | Membaca rencana atau permintaan | Pemanggil tanpa `WorkforcePlanningCost : Read` | Bukan penolakan; kolom biaya dikirim kosong dan layar menulis "Biaya tidak ditampilkan untuk peran Anda." | — |
| `VR-WFP-38` Kepala Unit hanya unitnya | Membaca atau mengajukan | Unit di luar cakupan organisasi pengguna | Daftar tidak memuatnya; detail menjawab "Data tidak ditemukan." | `404` |
| `VR-WFP-39` Potret tidak dapat diubah | Endpoint tulis apa pun ke potret | — | Tidak ada endpointnya | `405` |
| `VR-WFP-40` Alasan perubahan pengaturan | Mengubah pengaturan | Alasan kosong | "Tuliskan alasan perubahan pengaturan." | `400` |
| `VR-WFP-41` Batas hari wajar | Mengubah pengaturan | Batas cuti panjang atau umur permintaan < 1 atau > 365 | "Isi angka antara 1 dan 365 hari." | `400` |


---

## 13. Rekrutmen dan Hiring — `S-D2` (`last_changed_in` `v8`, 3 November 2026)

Seluruh aturan di bawah diturunkan dari `00-interview-decisions.md` bagian 33 (`HRD-DEC-073` s.d. `HRD-DEC-080`).
Nomor `VR-REC-nn` stabil dan dirujuk `testing/acceptance-test-matrix.md`.

### 13.1 Requisition dan Lowongan Kerja

| Aturan | Berlaku Pada | Kondisi Pelanggaran | Pesan bagi Pengguna | Kode |
| --- | --- | --- | --- | :---: |
| `VR-REC-01` Requisition wajib punya formasi | Membuat Requisition | `HeadcountRequestId` tidak sah atau status bukan `APPROVED` | "Requisition wajib merujuk pada Permintaan Headcount yang telah disetujui Direktur." | `422` |
| `VR-REC-02` Kursi tidak melebihi alokasi | Menyimpan Requisition | `RequestedSeats` > sisa kursi alokasi luar | "Jumlah kursi yang diminta ({minta}) melebihi sisa alokasi formasi luar ({sisa})." | `422` |
| `VR-REC-03` Lowongan merujuk Requisition aktif | Membuat Lowongan | `JobRequisitionId` belum disetujui atau kuota 0 | "Lowongan hanya dapat dibuka dari Requisition yang telah disetujui." | `400` |
| `VR-REC-04` Batas tanggal lowongan wajar | Menyimpan Lowongan | `ClosingDate` < `PublishDate` | "Tanggal batas akhir lowongan tidak boleh mendahului tanggal publikasi." | `400` |

### 13.2 Registrasi Pelamar dan Penapisan STR Klinis

| Aturan | Berlaku Pada | Kondisi Pelanggaran | Pesan bagi Pengguna | Kode |
| --- | --- | --- | --- | :---: |
| `VR-REC-05` Format dan Unik NIK KTP | Mendaftar Pelamar | NIK bukan 16 digit angka | "Nomor Induk Kependudukan (NIK) wajib 16 digit angka sah." | `400` |
| `VR-REC-06` Deteksi Pelamar Eks-Karyawan | Mendaftar Pelamar | NIK sudah terdaftar di `WorkforceProfile` | Bukan galat; sistem mengaktifkan alur rehire dan menautkan profil lama (`HRD-DEC-075`). | — |
| `VR-REC-07` STR Wajib untuk Posisi Klinis | Penapisan Berkas | Posisi klinis tanpa nomor STR atau dokumen STR kosong | "Posisi tenaga klinis wajib melampirkan Surat Tanda Registrasi (STR) yang sah." | `422` |
| `VR-REC-08` Hard Gate Kadaluwarsa STR | Penapisan Berkas | Tanggal masa berlaku STR < tanggal hari ini | "STR kandidat telah melewati masa berlaku (kadaluwarsa). Kandidat gugur administrasi." | `422` |

### 13.3 Evaluasi Wawancara dan Uji Klinis Unit

| Aturan | Berlaku Pada | Kondisi Pelanggaran | Pesan bagi Pengguna | Kode |
| --- | --- | --- | --- | :---: |
| `VR-REC-09` Hak Veto Klinis Unit | Input Uji Klinis | Asesor Unit memilih `NOT_RECOMMENDED` | "Kandidat dinyatakan Tidak Layak Klinis oleh Kepala Unit. Kandidat gugur demi patient safety." | `403` |
| `VR-REC-10` Larangan Override Veto | Menyetujui Offer | Mencoba membuat offer untuk kandidat terkena veto klinis | "Kandidat tidak dapat diberikan penawaran kerja karena tidak memenuhi standar kelaikan klinis." | `403` |
| `VR-REC-11` Formulir Independen | Submit Evaluasi | Penilai HR mengisi form klinis, atau sebaliknya | "Formulir evaluasi wajib diisi oleh peran penilai yang berwenang." | `403` |

### 13.4 Surat Penawaran Kerja dan Persetujuan Bertingkat

| Aturan | Berlaku Pada | Kondisi Pelanggaran | Pesan bagi Pengguna | Kode |
| --- | --- | --- | --- | :---: |
| `VR-REC-12` Penawaran Wajib Bersyarat | Menerbitkan Offer | Dokumen offer tidak memuat klausul syarat kelaikan MCU | "Surat penawaran kerja wajib mencantumkan klausul kelaikan fisik MCU pra-kerja." | `422` |
| `VR-REC-13` Validasi Plafon Skala Upah | Menyusun Offer | Gaji penawaran < `MinimumSalary` | "Penawaran gaji berada di bawah batas minimum skala upah grade terkait." | `400` |
| `VR-REC-14` Eskalasi Gaji di Atas Plafon | Persetujuan Offer | Penawaran > `MaximumSalary` tanpa approval Dirut | "Penawaran melebihi plafon maksimum grade wajib disetujui Direktur Keuangan dan Direktur Utama." | `403` |
| `VR-REC-15` MCU Hanya Pasca-Penawaran | Menjadwalkan MCU | Kandidat belum menyepakati penawaran bersyarat | "Pemeriksaan MCU pra-kerja hanya dijadwalkan setelah kandidat menandatangani penawaran bersyarat." | `409` |

### 13.5 Kelaikan MCU, Talent Pool, dan Onboarding

| Aturan | Berlaku Pada | Kondisi Pelanggaran | Pesan bagi Pengguna | Kode |
| --- | --- | --- | --- | :---: |
| `VR-REC-16` Masking Diagnosa Medis | Membaca MCU | Akun HR mengakses detail rekam medis dokter | "Detail rekam medis bersifat rahasia medis. HR hanya berhak melihat kesimpulan status kelaikan kerja." | `403` |
| `VR-REC-17` Syarat Serah Terima Onboarding | Eksekusi Handoff | Status kelaikan MCU != `FIT` | "Serah terima onboarding hanya dapat diproses bila status MCU kandidat FIT." | `422` |
| `VR-REC-18` Promosi Cepat Talent Pool | Promosi Cadangan | Kursi lowongan terkait masih terisi kandidat lain | "Promosi dari Talent Pool hanya diizinkan bila kursi lowongan kembali terbuka karena kandidat batal." | `409` |
| `VR-REC-19` Masa Berlaku Talent Pool | Promosi Cadangan | Kandidat cadangan telah berada di Talent Pool > 6 bulan | "Masa aktif kandidat cadangan di Talent Pool telah kadaluwarsa (> 6 bulan). Wajib uji klinis ulang." | `422` |
| `VR-REC-20` Larangan Swa-Setuju | Persetujuan Offer | Penyetuju penawaran sama dengan pembuat draf | "Penyetuju penawaran kerja wajib berbeda dari pembuat draf penawaran." | `403` |


---

## 14. Onboarding dan Kontrak Kerja — `S-C4-ONB` (`last_changed_in` `v9`, 4 November 2026)

Status `approved` — Pengguna (pemilik teknis dan produk, `HRD-DEC-015`), 2026-11-04, beserta kelima pilihan desain (`HRD-DEC-096`). Jawaban pemilik: "setujui semua ya dan kerjakan". Seluruh aturan di bawah **belum ditegakkan di kode**. Nomor `VR-ONB-nn` stabil dan
dirujuk `testing/acceptance-test-matrix.md` bagian 17. Nama orang pada pesan adalah contoh data samaran.

### 14.1 Kasus onboarding dan checklist

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VR-ONB-01` Satu kasus per serah terima | Serah terima dijalankan | Sudah ada kasus untuk serah terima itu | Tidak ada pesan; kasus yang sudah ada dikembalikan, tidak ada kasus kedua | `200` |
| `VR-ONB-02` Syarat serah terima | Serah terima dijalankan | Tawaran belum `FinalAccepted`, kelaikan bukan `Fit`/`FitWithRestrictions`, atau profil tidak berstatus `PENDING_ONBOARDING` | "Kandidat belum dapat diserahkan ke onboarding: {syarat yang belum terpenuhi}." | `422` |
| `VR-ONB-03` Butir HRD | Centang lewat jalur Kepala Unit, atau sebaliknya | `ResponsibleRole` butir berbeda dari jalur yang dipakai | "Butir ini diisi oleh {HRD/Kepala Unit}." | `422` |
| `VR-ONB-04` Cakupan unit | Kepala Unit membuka atau mencentang | Unit kasus di luar cakupan organisasinya | "Data tidak ditemukan." | `404` |
| `VR-ONB-05` Bukti wajib | Mencentang butir wajib bukti | Tidak ada berkas | "Unggah bukti untuk butir {nama butir} sebelum menandainya selesai." | `422` |
| `VR-ONB-06` Butir otomatis | Mencentang butir otomatis | — | "Butir ini terisi otomatis dari sistem dan tidak dapat dicentang manual." | `422` |
| `VR-ONB-07` Atasan langsung sah | Memilih atasan langsung | Orang terpilih adalah pegawai baru itu sendiri, tidak aktif, atau tidak punya penempatan aktif | "Pilih atasan langsung yang aktif dan bukan pegawai yang bersangkutan." | `422` |
| `VR-ONB-08` Preceptor sah | Memilih atau mengganti preceptor | Orang terpilih bukan tenaga klinis, tidak aktif, tidak berpenempatan aktif di unit yang sama, sedang didampingi sendiri, atau pegawai baru itu sendiri | "Preceptor harus tenaga klinis aktif di {unit} yang sudah bekerja mandiri." | `422` |
| `VR-ONB-09` Jenis dan ukuran berkas | Setiap unggahan | Bukan PDF, JPG, atau PNG, atau lebih dari 10 MB | "Berkas harus PDF, JPG, atau PNG dengan ukuran paling besar 10 MB." | `400` |
| `VR-ONB-10` Batal centang | Membatalkan centang | Kasus bukan `InProgress`; butir otomatis; hadir hari pertama sesudah kontrak ditandatangani | "Centang butir ini tidak dapat dibatalkan: {sebab}." | `422` |
| `VR-ONB-11` Butir wajib lengkap | Nyatakan Siap Kerja | Masih ada butir wajib `Pending` | "Masih ada {n} butir wajib yang belum selesai: {daftar butir}." | `422` |
| `VR-ONB-12` Pemisahan peran atasan langsung | Nyatakan Siap Kerja | Petugas yang menyatakan siap kerja sama dengan yang memilih atasan langsung | "Atasan langsung dipilih oleh Anda sendiri. Siap kerja harus dinyatakan petugas HRD lain." | `422` |
| `VR-ONB-13` Batal bergabung sebelum hadir | Batal Bergabung | Hadir hari pertama sudah tercatat | "Rina sudah tercatat hadir pada {tanggal}. Keluarnya diproses sebagai pengunduran diri." | `422` |
| `VR-ONB-14` Alasan batal | Batal Bergabung | Alasan belum dipilih atau catatan kosong | "Pilih alasan dan tuliskan catatan batal bergabung." | `400` |
| `VR-ONB-15` Versi usang | Seluruh perubahan | `rowVersion` berbeda | "Data ini sudah diubah orang lain. Muat ulang lalu ulangi." | `409` |
| `VR-ONB-16` Kasus tertutup | Perubahan apa pun pada kasus | Kasus `Completed` atau `Cancelled` | "Kasus onboarding ini sudah {selesai/dibatalkan} dan tidak dapat diubah." | `422` |
| `VR-ONB-17` Hadir tidak sebelum tanggal mulai | Mencatat hadir hari pertama | Tanggal hadir sebelum tanggal mulai rencana atau sesudah hari ini | "Tanggal mulai kerja Rina {tanggal}. Kehadiran hari pertama belum dapat dicatat." | `422` |

### 14.2 Template onboarding

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VR-ONB-20` Satu template aktif per lapis | Mengaktifkan template Umum atau Klinis | Sudah ada template aktif di lapis itu | "Sudah ada Template {Umum/Klinis} yang aktif. Nonaktifkan dulu template itu." | `409` |
| `VR-ONB-21` Satu template aktif per unit | Mengaktifkan template Unit | Unit itu sudah punya template aktif | "{Unit} sudah punya template aktif." | `409` |
| `VR-ONB-22` Butir sistem terkunci | Menyimpan template | Butir sistem dihapus, atau jenis, penanggung jawab, maupun tanda wajibnya diubah | "Butir {nama} adalah butir sistem dan tidak dapat dihapus atau diubah jenisnya." | `422` |
| `VR-ONB-23` Jenis sertifikat | Menyimpan butir otomatis sertifikat | Jenis sertifikat tidak dipilih | "Pilih jenis sertifikat untuk butir otomatis ini." | `400` |
| `VR-ONB-24` Unit pada lapis | Menyimpan template | Lapis Unit tanpa unit, atau lapis Umum/Klinis dengan unit | "Template Unit wajib memilih unit; template Umum dan Klinis tidak memakai unit." | `400` |
| `VR-ONB-25` Kode butir unik | Menyimpan template | Kode butir sama muncul dua kali | "Kode butir {kode} sudah dipakai di template ini." | `409` |

### 14.3 Kontrak kerja

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VR-ONB-30` Scan wajib | Tandai Sudah Ditandatangani | Tidak ada berkas | "Unggah scan kontrak yang sudah ditandatangani pegawai dan pejabat rumah sakit." | `422` |
| `VR-ONB-31` Isi draf terkunci | Mengubah isi draf kontrak | — | Tidak ada endpointnya; layar menampilkan "Ubah lewat revisi tawaran di Rekrutmen." | `405` |
| `VR-ONB-32` Riwayat kontrak 02 baca saja | Endpoint tulis riwayat kontrak 02 | — | "Kontrak dikelola lewat menu Onboarding & Kontrak." | `409` |
| `VR-ONB-33` Isian kepegawaian terkunci | Menyimpan master pegawai | Mengubah jenis atau status kepegawaian, tanggal masuk, tanggal akhir kontrak, atau tanggal akhir masa percobaan pegawai yang sudah punya kontrak | "Isian ini mengikuti kontrak kerja dan diubah lewat menu Onboarding & Kontrak." | `409` |
| `VR-ONB-34` Masa percobaan tidak diisi tangan | Batch, koreksi, atau endpoint mana pun | Request membawa tanggal masa percobaan | "Masa percobaan dihitung sistem dari jenis kontrak dan tidak dapat diisi manual." | `422` |
| `VR-ONB-35` Keputusan hanya untuk PKWT berjalan | Mengajukan keputusan | Kontrak bukan PKWT, atau bukan `Active`/`LapsedWithoutDecision` | "Keputusan perpanjangan hanya untuk kontrak PKWT yang masih berjalan atau baru berakhir." | `422` |
| `VR-ONB-36` Batas 5 tahun PKWT | Mengajukan atau menyetujui Perpanjang; koreksi; batch | Total PKWT melewati 5 tahun | "Total PKWT Budi akan menjadi {x} tahun {y} bulan, melebihi batas 5 tahun. Pilih Angkat jadi Pegawai Tetap atau Tidak Diperpanjang." | `422` |
| `VR-ONB-37` Satu keputusan terbuka | Mengajukan keputusan | Sudah ada keputusan `Submitted`, `Returned`, atau `Approved` untuk kontrak itu | "Kontrak ini sudah punya keputusan yang {sedang diproses/disetujui}." | `409` |
| `VR-ONB-38` Pengaju sah | Mengajukan keputusan atau penilaian masa percobaan | Pemanggil bukan atasan langsung aktif pegawai dan tidak memegang cakupan unit pegawai | "Data tidak ditemukan." | `404` |
| `VR-ONB-39` Isian keputusan | Mengajukan keputusan | Perpanjang tanpa lama 1–60 bulan; Tidak Diperpanjang tanpa alasan | "Isi lama perpanjangan 1–60 bulan." / "Tuliskan alasan tidak diperpanjang." | `400` |
| `VR-ONB-40` Catat pemberitahuan | Mencatat tanggal pemberitahuan | Keputusan bukan Tidak Diperpanjang yang disetujui; tanggal sebelum tanggal persetujuan atau sesudah hari ini | "Tanggal pemberitahuan harus antara tanggal persetujuan dan hari ini." | `422` |
| `VR-ONB-41` Isian koreksi | Mengajukan koreksi | Alasan kosong atau scan perbaikan tidak ada | "Tuliskan alasan dan unggah scan dokumen perbaikan yang sudah ditandatangani." | `400` |
| `VR-ONB-42` Jenis kontrak tidak dapat dikoreksi | Mengajukan koreksi jenis kontrak | Kontrak sudah punya masa percobaan atau keputusan yang disetujui | "Jenis kontrak tidak dapat dikoreksi karena sudah ada {masa percobaan/keputusan} yang mengikutinya." | `422` |
| `VR-ONB-43` Satu koreksi terbuka | Mengajukan koreksi | Sudah ada koreksi `Submitted` atau `Returned` | "Kontrak ini masih punya koreksi yang sedang diproses." | `409` |
| `VR-ONB-44` Nilai sesudah koreksi sah | Mengajukan dan menerapkan koreksi | PKWT tanpa tanggal berakhir; tanggal mulai sesudah tanggal berakhir; nomor kontrak dipakai kontrak lain pegawai itu; batas 5 tahun | "Isi kontrak sesudah koreksi tidak sah: {sebab}." | `422` |
| `VR-ONB-45` Scan susulan | Unggah scan kontrak | Kontrak bukan data awal, atau sudah punya scan | "Scan susulan hanya untuk kontrak data awal yang belum punya scan." | `422` |
| `VR-ONB-46` Penahan jadwal | Penjadwalan 06 menjadwalkan pegawai | Hasil kesiapan `ContractHold` pada tanggal itu | "Kontrak Budi berakhir {tanggal} tanpa kontrak lanjutan yang ditandatangani. Jadwal sesudah tanggal itu belum dapat dibuat." | `422` (dikembalikan 06) |
| `VR-ONB-47` Belum siap kerja | Penjadwalan 06 menjadwalkan pegawai | Hasil kesiapan `NotReady` | "Rina belum dinyatakan siap kerja." | `422` (dikembalikan 06) |

### 14.4 Tawaran kerja `S-D2` (`HRD-DEC-095`)

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VR-ONB-50` Jenis kontrak wajib | Mengajukan tawaran untuk persetujuan | Jenis kontrak kosong | "Pilih jenis kontrak pada tawaran." | `422` |
| `VR-ONB-51` Lama kontrak | Menyimpan dan mengajukan tawaran | Jenis kontrak mensyaratkan tanggal berakhir tetapi lama kosong atau di luar 1–60 bulan; atau jenis tanpa tanggal berakhir tetapi lama diisi | "Isi lama kontrak 1–60 bulan untuk {PKWT}." / "Kontrak {PKWTT} tidak memakai lama kontrak." | `422` |

### 14.5 Masa percobaan

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VR-ONB-55` Masih berjalan | Mengajukan atau mengajukan ulang hasil | Status bukan `Running`, atau hari ini sesudah `EndDate` | "Masa percobaan Andi sudah berakhir pada {tanggal}." | `422` |
| `VR-ONB-56` Satu penilaian terbuka | Mengajukan hasil | Sudah ada penilaian `Submitted` atau `Returned` | "Masih ada penilaian yang sedang diproses." | `409` |
| `VR-ONB-57` Isian penilaian | Mengajukan hasil | Alasan kosong; atau Tidak Lulus tanpa lampiran | "Tuliskan alasan penilaian." / "Unggah lampiran penilaian untuk hasil Tidak Lulus." | `400` / `422` |
| `VR-ONB-58` Keputusan tepat waktu | Menyetujui di kotak masuk | Hari ini sesudah `EndDate` | "Masa percobaan sudah lewat; pegawai ditetapkan lulus otomatis." | `422` |
| `VR-ONB-59` Tanpa perpanjangan | Endpoint perpanjangan | — | Tidak ada endpointnya | `405` |

### 14.6 Pendampingan klinis

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VR-ONB-60` Lampiran siap mandiri | Siap Mandiri | Tidak ada lampiran | "Unggah form penilaian kompetensi atau logbook pendampingan." | `422` |
| `VR-ONB-61` Hanya saat didampingi | Siap Mandiri | Status bukan `Accompanied` | "Pendampingan belum berjalan atau sudah selesai." | `422` |
| `VR-ONB-62` Ganti preceptor | Mengganti preceptor | Alasan kosong; atau preceptor tidak sah (`VR-ONB-08`) | "Tuliskan alasan penggantian preceptor." | `400` / `422` |
| `VR-ONB-63` Cakupan unit | Kepala Unit membuka atau bertindak | Unit di luar cakupan | "Data tidak ditemukan." | `404` |

### 14.7 Batch data awal kontrak

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VR-ONB-65` Sekali per pegawai | Menyimpan atau mengajukan batch | Pegawai sudah ada di batch lain yang belum ditolak/dibatalkan, atau sudah punya kontrak | "Budi sudah masuk batch data awal {nomor}." | `409` per baris |
| `VR-ONB-66` Isian PKWT | Memeriksa baris | PKWT tanpa tanggal berakhir atau tanpa tanggal PKWT pertama | "Baris {n}: PKWT wajib berisi tanggal berakhir dan tanggal mulai PKWT pertama." | Baris tidak sah |
| `VR-ONB-67` Batas 5 tahun | Memeriksa baris | Total PKWT melewati 5 tahun | "Baris {n}: total PKWT sudah {x} tahun {y} bulan, melebihi 5 tahun." | Baris tidak sah |
| `VR-ONB-68` Pegawai sah | Memeriksa baris | Profil tidak aktif, berstatus `PENDING_ONBOARDING`, atau tidak ditemukan | "Baris {n}: pegawai tidak dapat dimasukkan ke data awal: {sebab}." | Baris tidak sah |
| `VR-ONB-69` Tidak ada baris gagal | Mengajukan batch | `InvalidRows > 0` | "Masih ada {n} baris yang belum valid. Perbaiki semua baris sebelum mengajukan." | `422` |
| `VR-ONB-70` Nomor dan tanggal | Memeriksa baris | Nomor kontrak kosong; tanggal mulai sesudah tanggal berakhir | "Baris {n}: {sebab}." | Baris tidak sah |

### 14.8 Akun, akses, dan pengaturan

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VR-ONB-75` Syarat akun | Pembuatan akun otomatis | Email atau tanggal lahir pegawai kosong; email sudah dipakai akun lain | Permintaan berstatus `Failed` dengan pesan, contoh "Email pegawai belum diisi." Kasus tetap terbentuk | — |
| `VR-ONB-76` Tandai akses selesai | Menandai akses kerja | Bukan `WorkAccess` berstatus `Requested` | "Permintaan ini tidak sedang menunggu akses kerja." | `422` |
| `VR-ONB-77` Ulang akun | Mengulang pembuatan akun | Bukan `AccountCreation` berstatus `Failed` | "Akun tidak perlu dibuat ulang." | `422` |
| `VR-ONB-80` Rentang pengaturan | Mengubah pengaturan | Pengingat kontrak di luar 14–180; eskalasi tidak kurang dari pengingat; pengingat masa percobaan di luar 1–60; tanda terlambat di luar 1–30 hari | "Nilai {pengaturan} harus antara {batas bawah} dan {batas atas} hari." | `400` |


---

## 15. Aturan Validasi Slice `S-B4` — Penjadwalan, Roster, dan On-Call (`last_changed_in` `v10`, 4 November 2026)

| Field | Value |
| --- | --- |
| Status | `draft` — Siap diajukan untuk approval pengguna |
| `contract_version` | `v10` |
| `input_revision` — decision log | `00-interview-decisions.md` revision `20`, bagian 36 (`HRD-DEC-097` s.d. `HRD-DEC-103`) |
| Tanggal | 2026-11-04 |

### 15.1 Aturan Pemeriksaan Memblokir (`BLOCKING` / `B1`–`B7`)

Pelanggaran pada kelompok ini menyebabkan tombol **Terbitkan** terkunci (*disabled*) dan permintaan API `Publish` ditolak (HTTP 422).

| Kode Aturan | Berlaku pada | Kondisi Pelanggaran | Pesan bagi Pengguna | Kode HTTP |
| --- | --- | --- | --- | :---: |
| `VR-SCH-01` (`B1`) | Pemeriksaan & Terbit | Staf dijadwalkan dua shift yang jam kerjanya bertabrakan | "{Nama} dijadwalkan dua shift yang bertabrakan pada {Tanggal}." | `422` |
| `VR-SCH-02` (`B2`) | Pemeriksaan & Terbit | Staf dijadwalkan pada hari cuti sehari penuh yang berstatus disetujui | "{Nama} sedang cuti yang telah disetujui pada {Tanggal}." | `422` |
| `VR-SCH-03` (`B3`) | Pemeriksaan & Terbit | Staf tidak ditempatkan di unit tersebut pada tanggal shift (misal sudah mutasi) | "{Nama} sudah tidak ditempatkan di unit {Unit} sejak {Tanggal}." | `422` |
| `VR-SCH-04` (`B4`) | Pemeriksaan & Terbit | Pegawai belum dinyatakan siap kerja oleh HRD (`HRD-DEC-088`) | "{Nama} belum dinyatakan siap kerja oleh HRD." | `422` |
| `VR-SCH-05` (`B5`) | Pemeriksaan & Terbit | Tanggal shift berada sesudah tanggal akhir kontrak yang belum diputuskan perpanjangannya (`HRD-DEC-089`) | "Kontrak {Nama} berakhir pada {Tanggal} tanpa keputusan perpanjangan." | `422` |
| `VR-SCH-06` (`B6`) | Pemeriksaan & Terbit | Status kesiapan kerja profil pegawai tidak dapat dibaca dari sistem (`INT-HR-31`) | "Status kesiapan pegawai tidak dapat dibaca. Coba lagi atau hubungi HRD." | `422` |
| `VR-SCH-07` (`B7`) | Pemeriksaan & Terbit | Staf bertanda Didampingi dijadwalkan pada shift yang tidak memiliki perawat Mandiri (`HRD-DEC-100`) | "{Nama} berstatus Didampingi dan tidak boleh berdinas pada shift tanpa perawat Mandiri." | `422` |

### 15.2 Aturan Pemeriksaan Peringatan Beralasan (`WARNING` / `W1`–`W7`)

Peringatan pada kelompok ini memunculkan indikator kuning. Roster tetap dapat terbit setelah Kepala Unit memilih alasan wajib dari daftar.

| Kode Aturan | Berlaku pada | Kondisi Peringatan | Opsi Alasan Wajib | Pesan Peringatan |
| --- | --- | --- | --- | --- |
| `VR-SCH-08` (`W1`) | Pemeriksaan & Terbit | Jeda istirahat antar shift < `MinRestHoursBetweenShifts` (setelan: 11 jam) | `Kekurangan tenaga`, `Permintaan pegawai`, `Lainnya` | "Jeda istirahat {Nama} antara dua shift kurang dari {n} jam." |
| `VR-SCH-09` (`W2`) | Pemeriksaan & Terbit | Hari kerja berturut-turut tanpa hari libur > `MaxConsecutiveWorkDays` (setelan: 6 hari) | `Kekurangan tenaga`, `Permintaan pegawai`, `Lainnya` | "{Nama} dijadwalkan dinas {n} hari berturut-turut tanpa hari libur." |
| `VR-SCH-10` (`W3`) | Pemeriksaan & Terbit | Shift malam berturut-turut > `MaxConsecutiveNightShifts` (setelan: 2 malam) | `Kekurangan tenaga`, `Permintaan pegawai`, `Lainnya` | "{Nama} dijadwalkan dinas malam {n} malam berturut-turut." |
| `VR-SCH-11` (`W4`) | Pemeriksaan & Terbit | Ada pengajuan cuti yang masih berstatus pending pada tanggal shift | `Menunggu keputusan cuti`, `Lainnya` | "Terdapat pengajuan cuti yang belum diputuskan untuk {Nama} pada {Tanggal}." |
| `VR-SCH-12` (`W5`) | Pemeriksaan & Terbit | STR, SIP, atau kredensial kedaluwarsa pada tanggal shift (`HRD-DEC-005`) | `STR/SIP dalam proses perpanjangan`, `Lainnya` | "Kredensial/STR {Nama} kedaluwarsa pada {Tanggal}." |
| `VR-SCH-13` (`W6`) | Pemeriksaan & Terbit | Jumlah staf pada shift kurang dari target kuota unit (`HRD-DEC-100`) | `Kekurangan tenaga`, `Volume pasien/BOR rendah`, `Lainnya` | "Jumlah tenaga pada Shift {Shift} {Tanggal} hanya {x} orang (target kuota: {y} orang)." |
| `VR-SCH-14` (`W7`) | Pemeriksaan & Terbit | Kualifikasi khusus shift (misal PJ Shift/kompetensi ICU) belum terpenuhi | `Disupervisi dokter jaga`, `Kekurangan tenaga mahir`, `Lainnya` | "Komposisi kualifikasi perawat pada Shift {Shift} {Tanggal} belum terpenuhi." |

### 15.3 Aturan Penggantian Shift & Koreksi Mundur

| Kode Aturan | Berlaku pada | Kondisi Pelanggaran | Pesan bagi Pengguna | Kode HTTP |
| --- | --- | --- | --- | :---: |
| `VR-SCH-15` | Ganti Petugas | Kategori alasan kosong saat penggantian shift pasca-terbit | "Kategori alasan penggantian petugas wajib dipilih." | `400` |
| `VR-SCH-16` | Ganti Petugas | Staf pengganti yang dipilih melanggar aturan `B1`–`B7` | "Staf pengganti tidak dapat ditugaskan: {alasan bentrok/cuti}." | `422` |
| `VR-SCH-17` | Koreksi Mundur | Tanggal shift yang dikoreksi mundur melebihi 3 hari kalender ke belakang (`today - date > 3`) | "Koreksi mundur jadwal maksimal 3 hari kalender ke belakang. Hubungi HRD untuk koreksi lebih lanjut." | `422` |
| `VR-SCH-18` | Mutasi Roster | Mencoba menambah, mengubah, atau menghapus shift pada periode berstatus `Closed` | "Periode roster telah ditutup dan terkunci mutlak oleh Payroll. Perubahan tidak diizinkan." | `403` |

### 15.4 Aturan Penugasan Siaga (On-Call)

| Kode Aturan | Berlaku pada | Kondisi Pelanggaran | Pesan bagi Pengguna | Kode HTTP |
| --- | --- | --- | --- | :---: |
| `VR-SCH-19` | Jadwal Siaga | Jam mulai penugasan siaga tidak sebelum jam selesai | "Waktu mulai siaga harus lebih awal daripada waktu selesai." | `400` |
| `VR-SCH-20` | Jadwal Siaga | Staf sedang berstatus cuti disetujui pada rentang waktu siaga | "Tenaga medis {Nama} sedang cuti pada periode siaga tersebut." | `422` |
| `VR-SCH-21` | Pencatatan Call-In | Waktu kedatangan dokter lebih awal daripada waktu telepon panggilan dilakukan | "Waktu kedatangan tidak boleh mendahului waktu panggilan." | `400` |


---

## 16. Bagian `S-B1` — Matriks Validasi Administrasi Kehadiran dan Presensi Staf Rumah Sakit (Sub-modul 07, `last_changed_in` `v11`, 4 November 2026)

| Kode Validasi | Objek / Input | Syarat / Batasan Bisnis | Respons Penolakan / Tindakan Sistem | Kode HTTP |
| :--- | :--- | :--- | :--- | :---: |
| `VR-ATT-01` | `HrdAttendanceRawLog` | Rekaman mentah presensi biometrik bersifat *immutable*; dilarang update/delete | Operasi modifikasi log mentah ditolak sistem | `405 Method Not Allowed` |
| `VR-ATT-02` | `HrdAttendanceDaily` | Dilarang mengubah presensi pada tanggal di dalam periode `Closed` | "Periode presensi telah ditutup dan berstatus terkunci mutlak." | `409 Conflict` |
| `VR-ATT-03` | `ClosePeriodCommand` | Penutupan periode ditolak bila masih ada koreksi berstatus pending (`HRD-DEC-108`) | "Penutupan periode ditolak: masih terdapat N permohonan koreksi berstatus pending." | `409 Conflict` |
| `VR-ATT-04` | `ReopenPeriodCommand`| Buka kembali periode hanya oleh HR Manager dan wajib menyertakan alasan audit | "Wewenang buka kembali periode hanya dimiliki oleh HR Manager dan alasan wajib diisi." | `403 Forbidden` / `400 Bad Request` |
| `VR-ATT-05` | `AttendanceCorrection`| Petugas HR pembuat koreksi on-behalf dilarang menjadi approver (`HRD-DEC-028`) | "Penyetuju koreksi on-behalf harus merupakan atasan langsung pegawai, bukan petugas HR pemohon." | `403 Forbidden` |
| `VR-ATT-06` | `AttendanceCorrection`| Tanggal shift yang dikoreksi tidak boleh mundur > 3 hari tanpa izin HR Manager | "Batas toleransi koreksi tanggal mundur maksimal 3 hari kalender ke belakang." | `422 Unprocessable Entity` |
| `VR-ATT-07` | `OfficialDutyRequest` | Rentang waktu penugasan dinas luar wajib valid (`EndAt > StartAt`) | "Waktu selesai penugasan dinas luar harus lebih besar dari waktu mulai." | `400 Bad Request` |
| `VR-ATT-08` | `ScheduleResolver` | Tap masuk malam dan tap keluar pagi dikunci pada tanggal shift mulai (`HRD-DEC-104`)| Record kehadiran harian dikaitkan penuh ke tanggal shift hari H | Sukses Sistem |
| `VR-ATT-09` | `CalculationEngine` | Terlambat di dalam grace period = 0 menit; lewat grace period = hitung penuh (`HRD-DEC-105`)| LateMinutes dihitung penuh sejak jam mulai shift resmi bila lewat toleransi | Sukses Sistem |
| `VR-ATT-10` | `PeriodCloseGuard` | Catatan 1 tap belum dikoreksi otomatis dikonversi menjadi Absent (`HRD-DEC-106`) | Status IncompletePunch bergeser menjadi Absent tanpa jam kerja terhitung | Sukses Sistem |
| `VR-ATT-11` | `EarlyLeavePermission`| Izin pulang cepat administratif tidak memotong kuota saldo cuti (`HRD-DEC-107`)| Pengecualian pulang cepat berstatus Waived; kuota saldo cuti tahunan tidak berkurang | Sukses Sistem |
| `VR-ATT-12` | `ReopenPeriodGuard` | Aksi reopen otomatis membatalkan serah terima payroll (`HRD-DEC-108`) | Memicu AttendancePayrollHandoff.Rollback bila status handoff Executed | Sukses Sistem |
| `VR-ATT-13` | `OfficialDutyRequest` | Dinas luar yang disetujui atasan dihitung jam kerja penuh OnDuty (`HRD-DEC-109`) | Status harian OnDuty, jam kerja setara hadir kerja Present untuk payroll | Sukses Sistem |
| `VR-ATT-14` | `OfficialDutyCheckIn`| Koordinat GPS dan foto selfie wajib diisi saat tap mobile dinas luar | "Koordinat GPS dan foto bukti pelaksanaan tugas luar wajib disertakan." | `400 Bad Request` |
| `VR-ATT-15` | `AttendanceCorrection`| Unggah bukti berita acara/lampiran wajib diisi bila policy RS mengaktifkannya | "Lampiran dokumen bukti koreksi presensi wajib diunggah." | `400 Bad Request` |
| `VR-ATT-16` | `EarlyLeavePermission`| Waktu rencana keluar (`RequestedEarlyLeaveAt`) wajib berada di dalam jam shift aktif | "Waktu izin pulang cepat harus berada di antara jam mulai dan jam selesai shift." | `400 Bad Request` |
| `VR-ATT-17` | `AttendanceDaily` | Jam kerja efektif tidak boleh bernilai negatif | EffectiveWorkMinutes disetel minimal 0 menit | Sukses Sistem |
| `VR-ATT-18` | `RawLogIngestion` | Tap ganda berturut-turut dalam 3 menit diabaikan sebagai duplikasi mesin | Hanya log pertama yang diproses oleh schedule resolver | Sukses Sistem |
| `VR-ATT-19` | `AttendanceSetting` | Nilai toleransi keterlambatan minimal 0 menit dan maksimal 60 menit | "Nilai toleransi keterlambatan harus berada pada rentang 0 s.d. 60 menit." | `400 Bad Request` |
| `VR-ATT-20` | `OfficialDutyRequest` | Penugasan dinas luar tidak boleh bertabrakan dengan cuti sehari penuh yang disetujui| "Staf sedang memiliki jadwal cuti yang telah disetujui pada tanggal tersebut." | `409 Conflict` |


---

## 17. Bagian `S-B2` — Matriks Validasi Administrasi Cuti & Izin Pegawai Rumah Sakit (Sub-modul 08, `last_changed_in` `v12`, 4 November 2026)

Matriks validasi ini mengunci seluruh aturan integritas data, validasi model, aturan bisnis rumah sakit, dan kode respon HTTP untuk **Slice `S-B2` — Administrasi Cuti & Izin Pegawai Rumah Sakit**.

| Kode Aturan | Kategori | Kondisi yang Diperiksa | Respon / Kode HTTP | Pesan Kesalahan Baku | Rujukan Keputusan |
| --- | --- | --- | :---: | --- | --- |
| `VR-LEV-01` | Integritas Saldo | `DurationDays > CurrentAvailableBalance` pada cuti jenis *deductible* | `400 Bad Request` | "Saldo cuti Anda tidak mencukupi untuk durasi yang diajukan." | `LEV-B1`, `HRD-DEC-110` |
| `VR-LEV-02` | Master Data | `LeaveTypeId` tidak ditemukan atau `IsActive = false` | `400 Bad Request` | "Jenis cuti atau perizinan tidak valid atau telah dinonaktifkan." | Master Data |
| `VR-LEV-03` | Validasi Tanggal | `EndDate < StartDate` | `400 Bad Request` | "Tanggal selesai cuti tidak boleh lebih awal dari tanggal mulai." | Standar Validasi |
| `VR-LEV-04` | Cuti Setengah Hari | `IsHalfDay = true` tetapi `DurationDays != 0.5` | `400 Bad Request` | "Durasi cuti setengah hari wajib tepat 0.5 hari kerja." | `HRD-ASM-20` |
| `VR-LEV-05` | Cuti Setengah Hari | `IsHalfDay = true` dan jenis cuti bukan Cuti Tahunan | `400 Bad Request` | "Fasilitas cuti setengah hari hanya berlaku untuk jenis Cuti Tahunan." | `HRD-ASM-20` |
| `VR-LEV-06` | Cuti Sakit & Surat Dokter | Cuti sakit $\ge 2$ hari kerja tanpa mengunggah berkas surat dokter | `400 Bad Request` | "Cuti sakit 2 hari kerja atau lebih wajib melampirkan berkas Surat Keterangan Sakit resmi dokter." | `HRD-DEC-112`, `LEV-B3` |
| `VR-LEV-07` | Validasi Lampiran | Format berkas bukan PDF, JPG, atau PNG, atau ukuran berkas > 5MB | `400 Bad Request` | "Format berkas tidak didukung atau ukuran melebihi batas maksimal 5MB." | Standar Media |
| `VR-LEV-08` | Metadata Surat Sakit | Cuti sakit $\ge 2$ hari tanpa nama dokter pemeriksa atau nomor SIP dokter | `400 Bad Request` | "Nama faskes penerbit dan nomor Surat Izin Praktik (SIP) dokter pemeriksa wajib diisi." | `HRD-DEC-112` |
| `VR-LEV-09` | Privasi Medis K3RS | Diagnosa ICD-10 diakses oleh aktor selain Tim K3RS / HR Medis | `403 Forbidden` | "Akses ditolak: Diagnosa medis pegawai dilindungi undang-undang kerahasiaan medis." | `HRD-DEC-112`, `LEV-W3` |
| `VR-LEV-10` | Penjadwalan Roster | Tanggal cuti beririsan dengan shift aktif terbit tanpa koordinasi pengganti | `409 Conflict` | "Anda telah terdaftar dinas shift pada roster terbit. Koordinasikan ganti petugas sebelum mengajukan cuti." | `LEV-B2`, `S-B4` |
| `VR-LEV-11` | Batas Kuota Berurutan | Pengajuan cuti tahunan berturut-turut > 12 hari kerja tanpa dispensasi Direktur RS | `400 Bad Request` | "Pengajuan cuti tahunan berturut-turut melebihi batas maksimal 12 hari kerja." | `HRD-ASM-22`, `LEV-B7` |
| `VR-LEV-12` | Kebijakan Carry-Forward | Pemindahan saldo sisa akhir tahun > 6 hari kerja | `200 OK` (Truncated) | Saldo sisa dipotong maksimal 6 hari; kelebihan saldo otomatis dialokasikan ke mutasi `Forfeiture`. | `HRD-DEC-110`, `LEV-B8` |
| `VR-LEV-13` | Kedaluwarsa Saldo | Sisa saldo carry-forward digunakan setelah tanggal 30 Juni | `400 Bad Request` | "Saldo cuti bawaan tahun sebelumnya telah kedaluwarsa per 30 Juni." | `HRD-DEC-110`, `LEV-W2` |
| `VR-LEV-14` | Status Pemanggilan Cuti | Inisiasi *Leave Recall* pada permohonan yang statusnya bukan `Active` | `400 Bad Request` | "Pemanggilan dinas darurat hanya dapat dilakukan pada pegawai yang sedang menjalani cuti aktif." | `HRD-DEC-113` |
| `VR-LEV-15` | Wewenang Recall | Aktor penyetuju pemanggilan cuti bukan bertindak sebagai HR Manager | `403 Forbidden` | "Hanya HR Manager yang memiliki wewenang menyetujui pemanggilan cuti darurat rumah sakit." | `HRD-DEC-113`, `LEV-B5` |
| `VR-LEV-16` | Nilai Hari Pemulihan Recall | `RestoredDays <= 0` atau melebihi sisa durasi hari cuti riil | `400 Bad Request` | "Jumlah hari pemulihan cuti tidak valid." | `HRD-DEC-113` |
| `VR-LEV-17` | Pembatalan Pre-Leave | Pembatalan mandiri diajukan setelah tanggal mulai cuti tiba (`Today >= StartDate`) | `400 Bad Request` | "Pembatalan mandiri hanya dapat diajukan sebelum tanggal pelaksanaan cuti dimulai." | `HRD-ASM-21` |
| `VR-LEV-18` | Penyesuaian Saldo Audit | `AuditDocumentNumber` atau `Reason` kosong pada permintaan `POST /adjustments` | `400 Bad Request` | "Nomor dokumen audit resmi dan alasan tertulis wajib diisi untuk penyesuaian saldo." | `HRD-DEC-115`, `LEV-B6` |
| `VR-LEV-19` | Immutabilitas Cuti Selesai | Pemanggilan endpoint `POST /{id}/reverse` pada cuti berstatus `Completed` | `409 Conflict` | "Operasi ditolak: Permohonan cuti telah selesai dan berstatus immutable. Pembalikan langsung dilarang." | `HRD-DEC-115`, `LEV-B4` |
| `VR-LEV-20` | Penguncian Payroll Closed | Operasi modifikasi data cuti pada periode yang telah berstatus `PayrollClosed` | `409 Conflict` | "Periode penggajian telah ditutup. Data transaksi cuti terkunci mutlak." | `HRD-DEC-114`, `HRD-DEC-115` |
| `VR-LEV-21` | Safe Staffing Mitigation | Aksi *Approve with Mitigation* dilakukan tanpa mengisi catatan rencana pengganti | `400 Bad Request` | "Catatan mitigasi penugasan staf pengganti wajib diisi saat menyetujui cuti berperingatan kuota." | `HRD-DEC-111` |
| `VR-LEV-22` | Kontak Darurat | Nomor telepon kontak darurat tidak valid atau kosong | `400 Bad Request` | "Nomor telepon kontak darurat yang dapat dihubungi selama cuti wajib diisi." | Standar K3RS |


---

## 18. Bagian `S-B3` — Matriks Validasi Administrasi Lembur Pegawai Rumah Sakit (Sub-modul 09, `last_changed_in` `v13`, 4 November 2026)

Matriks validasi ini mengunci seluruh aturan integritas data, validasi model, aturan bisnis rumah sakit, dan kode respon HTTP untuk **Slice `S-B3` — Administrasi Lembur Pegawai Rumah Sakit**.

| Kode Aturan | Kategori | Kondisi yang Diperiksa | Respon / Kode HTTP | Pesan Kesalahan Baku | Rujukan Keputusan |
| --- | --- | --- | :---: | --- | --- |
| `VR-OT-01` | Validasi Pra-Dinas | SPKL kategori `Planned` dengan `PlanStartTime <= CurrentTime` | `400 Bad Request` | "Waktu mulai rencana lembur terencana harus di masa depan." | `HRD-DEC-116`, `OT-B1` |
| `VR-OT-02` | Darurat Cito | Klaim darurat cito tanpa nomor RM atau uraian tindakan klinis | `400 Bad Request` | "Nomor Rekam Medis / Insiden dan ringkasan tindakan klinis darurat wajib diisi." | `HRD-DEC-116`, `OT-B3` |
| `VR-OT-03` | Anti-Self-Approval | `ApproverUserId == RequestEmployeeUserId` saat persetujuan SPKL | `403 Forbidden` | "Akses ditolak: Pejabat struktural dilarang menyetujui formulir lemburnya sendiri." | `HRD-DEC-120`, `OT-B5` |
| `VR-OT-04` | Kuota PP 35/2021 | Lembur reguler dengan akumulasi harian > 4 jam atau mingguan > 18 jam | `400 Bad Request` | "Pengajuan melebihi kuota maksimal lembur (4 jam/hari atau 18 jam/minggu) sesuai regulasi PP 35/2021." | `HRD-DEC-117`, `OT-B4` |
| `VR-OT-05` | Batas Manajerial | Pegawai `JobLevel >= Manager` memilih kompensasi `FinancialPayout` | `400 Bad Request` | "Pejabat struktural/manajerial tidak berhak atas kompensasi uang lembur finansial (Overtime Exempt)." | `HRD-DEC-118`, `OT-B6` |
| `VR-OT-06` | Tenggat Pasca-Dinas | Klaim pasca-dinas diserahkan > H+1 hari kerja setelah dinas selesai | `400 Bad Request` | "Batas waktu pelaporan klaim darurat medis pasca-dinas telah kedaluwarsa (maksimal H+1 hari kerja)." | `HRD-DEC-116`, `OT-B2` |
| `VR-OT-07` | Min-Duration Rule | Perhitungan durasi sah dari `min(claimed_minutes, biometric_minutes)` | `200 OK` (Auto) | Durasi sah dihitung otomatis dari nilai fisik terkecil untuk mencegah lembur fiktif. | `HRD-DEC-119`, `OT-W3` |
| `VR-OT-08` | Pembulatan Waktu | Pembulatan durasi lembur fisik ke kelipatan 30 menit ke bawah | `200 OK` (Auto) | Durasi dibulatkan ke kelipatan 30 menit terdekat ke bawah (`HRD-ASM-23`). | `HRD-ASM-23`, `OT-B8` |
| `VR-OT-09` | Ambang Batas Minimal| Durasi kelebihan fisik terverifikasi < 30 menit | `200 OK` (Nol Jam) | Durasi lembur kurang dari 30 menit tidak dihitung kompensasi (0 jam). | `HRD-ASM-23` |
| `VR-OT-10` | Discrepancy Flagging| Selisih jam SPKL vs mesin presensi > 30 menit atau lupa tap biometrik | `200 OK` (Flagged) | Status klaim ditandai `FlaggedDiscrepancy` dan ditahan dari rekonsiliasi payroll. | `HRD-DEC-119`, `OT-W2` |
| `VR-OT-11` | Bukti Investigasi | Koreksi manual discrepancy tanpa catatan evaluasi atau nomor dokumen logbook | `400 Bad Request` | "Catatan evaluasi dan nomor dokumen logbook fisik ruangan wajib disertakan pada penyesuaian manual." | `HRD-DEC-119` |
| `VR-OT-12` | Eskalasi Darurat | Klaim cito melebihi batas kuota disetujui tanpa otorisasi Direktur Medis | `403 Forbidden` | "Klaim lembur darurat yang melebihi kuota PP 35/2021 wajib memperoleh otorisasi Direktur Pelayanan Medik." | `HRD-DEC-117`, `OT-W1` |
| `VR-OT-13` | Masa Berlaku CTO | Permohonan cuti CTO diajukan setelah melewati 90 hari kalender sejak terbit | `400 Bad Request` | "Hak cuti pengganti (CTO) telah kedaluwarsa (masa berlaku 90 hari kalender telah terlampaui)." | `HRD-DEC-118`, `HRD-ASM-25` |
| `VR-OT-14` | Non-Cash CTO | Permintaan konversi saldo CTO kedaluwarsa menjadi uang tunai | `400 Bad Request` | "Hak cuti pengganti (CTO) yang hangus tidak dapat dikompensasikan menjadi uang tunai." | `HRD-ASM-25` |
| `VR-OT-15` | Penutupan Cut-off | SPKL diajukan pada periode lembur yang sedang dalam proses cut-off | `200 OK` (Deferred)| SPKL otomatis dialokasikan ke periode penggajian bulan kalender berikutnya. | `HRD-DEC-121` |
| `VR-OT-16` | Immutabilitas Kunci | Modifikasi data lembur pada periode berstatus `CutoffLocked` atau `Closed` | `409 Conflict` | "Periode lembur telah dikunci/ditutup. Modifikasi data dilarang mutlak." | `HRD-DEC-121`, `OT-B7` |
| `VR-OT-17` | Retrospektif Payroll| SPKL pending disetujui setelah tanggal cut-off bulanan | `200 OK` (Retro) | Klaim ditandai `IsRetrospectiveAdjustment = true` dan masuk rapel payroll berikutnya. | `HRD-DEC-121`, `OT-W5` |
| `VR-OT-18` | Konflik Cuti | Jadwal dinas lembur bertabrakan dengan permohonan cuti sehari penuh yang disetujui| `409 Conflict` | "Pegawai sedang dalam jadwal cuti aktif yang telah disetujui pada tanggal tersebut." | Integritas Sub-modul 08 |
| `VR-OT-19` | Jam Kerja Reguler | Lembur dijadwalkan tepat di dalam rentang jam shift dinas reguler staf | `409 Conflict` | "Jam dinas lembur tidak boleh bertabrakan dengan jam kerja dinas reguler staf." | Integritas Sub-modul 06 |
| `VR-OT-20` | Anggota Tim Kosong | SPKL kelompok diajukan tanpa mendaftarkan satupun pegawai pelaksana | `400 Bad Request` | "SPKL kelompok wajib memuat sekurang-kurangnya satu orang pegawai pelaksana." | `HRD-ASM-26` |
| `VR-OT-21` | Validasi Unit | `UnitId` tidak terdaftar atau dalam status tidak aktif pada master organisasi | `400 Bad Request` | "Unit kerja rumah sakit tidak valid atau telah dinonaktifkan." | Master Data |
| `VR-OT-22` | Batas Waktu 24 Jam | Total jam lembur terencana melebihi 24 jam dalam satu hari kalender | `400 Bad Request` | "Durasi lembur tidak valid: Melebihi total jam kalender dalam satu hari." | Batas Logika |
| `VR-OT-23` | Pembatalan Terlambat| Pembatalan SPKL terencana diajukan setelah jam dinas lembur dimulai | `400 Bad Request` | "Pembatalan rencana lembur hanya dapat diajukan sebelum jam pelaksanaan dinas dimulai." | Standar Rencana |
| `VR-OT-24` | Penolakan Direktur | Eksekusi pembayaran pada klaim cito yang ditolak oleh Direktur Pelayanan Medik | `403 Forbidden` | "Klaim lembur ditolak oleh Direktur Pelayanan Medik dan tidak dapat diproses lebih lanjut." | `HRD-DEC-117` |
| `VR-OT-25` | Buka Kunci Periode | Pembukaan kembali periode lembur yang telah ditutup tanpa izin Direktur HR | `403 Forbidden` | "Pembukaan kembali (reopen) periode lembur yang ditutup memerlukan otorisasi Direktur HR." | Tata Kelola Audit |

---

## 19. Bagian `S-B5` — Matriks Validasi Penggajian Pegawai Rumah Sakit (Sub-modul 10, `last_changed_in` `v14`, 5 November 2026)

Matriks validasi ini mengunci seluruh aturan integritas data, validasi model FluentValidation, aturan bisnis kontrol finansial rumah sakit, dan kode respon HTTP untuk **Slice `S-B5` — Penggajian Pegawai Rumah Sakit**.

| Kode Aturan | Kategori | Kondisi yang Diperiksa | Respon / Kode HTTP | Pesan Kesalahan Baku | Rujukan Keputusan |
| :--- | :--- | :--- | :---: | :--- | :--- |
| `VR-PAY-01` | Prasyarat Waktu Kerja | Inisiasi payroll saat periode Sub-07, 08, 09 belum berstatus `Closed/Reconciled` | `422 Unprocessable Entity` | "Gagal inisiasi payroll: Periode kehadiran, cuti, atau lembur belum resmi ditutup/direkonsiliasi." | `HRD-DEC-122`, `PAY-B1` |
| `VR-PAY-02` | Immutabilitas Run | Percobaan edit/hapus data pada siklus payroll yang berstatus `ApprovedFinal` atau `Closed` | `409 Conflict` | "Transaksi ditolak: Siklus penggajian telah dikunci permanen (immutable)." | `HRD-DEC-125`, `PAY-B2` |
| `VR-PAY-03` | Penugasan Gaji Pokok | Pegawai aktif tanpa data penugasan gaji (`WfpSalaryAssignment`) yang sah | `400 Bad Request` | "Data penugasan gaji pokok dan grade kepegawaian tidak ditemukan untuk pegawai terkait." | `HRD-DEC-123` |
| `VR-PAY-04` | Kategori PTKP Pajak | Status PTKP pegawai belum terdaftar saat kalkulasi PPh 21 TER | `400 Bad Request` | "Status PTKP pegawai wajib dilengkapi untuk menentukan kategori TER (A/B/C) sesuai PMK 168/2023." | `HRD-DEC-123`, `HRD-ASM-28` |
| `VR-PAY-05` | Plafon BPJS JP | Dasar upah BPJS Ketenagakerjaan JP melebihi batas batas plafon regulasi | `200 OK` (Capped) | Dasar perhitungan iuran JP dibatasi otomatis pada plafon maksimal Rp 10.042.300. | `HRD-DEC-123` |
| `VR-PAY-06` | Pembulatan Net Pay | Nilai nominal gaji bersih memiliki nilai desimal sen | `200 OK` (Auto) | Gaji bersih (*Net Pay*) dibulatkan otomatis ke bawah (*floor*) ke kelipatan Rp 100 terdekat. | `HRD-DEC-123` |
| `VR-PAY-07` | Justifikasi Variansi | Pengajuan review ke Manajer saat masih ada `VarianceAlert` tanpa justifikasi tertulis | `400 Bad Request` | "Pengajuan ditolak: Seluruh lonjakan variansi gaji (>= 20%) wajib memiliki catatan penjelasan resmi." | `HRD-DEC-124`, `PAY-B3` |
| `VR-PAY-08` | Anti-Self-Approval | Pengguna approver mencoba menyetujui payroll run yang dibuat oleh dirinya sendiri | `403 Forbidden` | "Akses ditolak: Pejabat pembuat dilarang menyetujui paket penggajian miliknya sendiri (SoD violation)." | `HRD-DEC-125`, `PAY-B4` |
| `VR-PAY-09` | Evaluasi HR Manager | Persetujuan Tahap 2 oleh HR Manager tanpa mengisi catatan verifikasi audit sampling | `400 Bad Request` | "Catatan evaluasi audit sampling wajib disertakan pada persetujuan manajerial SDM." | `HRD-DEC-125` |
| `VR-PAY-10` | Wewenang Direksi | Otorisasi final Tahap 3 dijalankan oleh pengguna di luar peran Direktur SDM / Keuangan | `403 Forbidden` | "Akses ditolak: Otorisasi final penguncian payroll hanya dapat dilakukan oleh Direksi." | `HRD-DEC-125` |
| `VR-PAY-11` | Kesiapan Handoff | Penyerahan berkas handoff dipanggil sebelum status run mencapai `ApprovedFinal` | `400 Bad Request` | "Paket serah terima Finance hanya dapat dibentuk setelah otorisasi final Direksi selesai." | `HRD-DEC-126` |
| `VR-PAY-12` | Checksum Integritas | Berkas serah terima ke Finance tanpa kode verifikasi checksum SHA-256 | `422 Unprocessable Entity` | "Gagal membentuk serah terima: Checksum integritas berkas data korup atau tidak valid." | `HRD-DEC-126`, `PAY-B8` |
| `VR-PAY-13` | Format Rekening Bank | Format nomor rekening tujuan bank mitra RS tidak valid (panjang karakter keliru) | `400 Bad Request` | "Nomor rekening bank pegawai tidak sesuai format standar bank mitra rumah sakit." | `HRD-DEC-126`, `PAY-W6` |
| `VR-PAY-14` | Batas Minimum THP | Potongan angsuran menyebabkan Take-Home Pay (THP) kurang dari 30% gaji pokok | `400 Bad Request` | "Total potongan pinjaman melebihi batas toleransi: Gaji bersih tidak boleh kurang dari 30% gaji pokok." | `HRD-DEC-123`, `PAY-W3` |
| `VR-PAY-15` | Checksum Mismatch | Checksum payload saat verifikasi Finance tidak cocok dengan rekaman HRD | `422 Unprocessable Entity` | "Integritas data terlanggar: Checksum berkas serah terima tidak cocok dengan data asli HR." | `HRD-DEC-126`, `PAY-B8` |
| `VR-PAY-16` | Alasan Penolakan Finance| Finance menolak batch serah terima tanpa mengisi alasan tertulis minimal 15 karakter | `400 Bad Request` | "Catatan alasan penolakan batch serah terima wajib diisi spesifik (minimal 15 karakter)." | `HRD-DEC-126`, `PAY-B7` |
| `VR-PAY-17` | Rollback Guard | Pembukaan kunci data dijalankan tanpa adanya sinyal penolakan resmi Finance | `403 Forbidden` | "Pembukaan kunci payroll dilarang tanpa adanya sinyal penolakan resmi dari Departemen Keuangan." | `HRD-DEC-126` |
| `VR-PAY-18` | Jadwal Rilis Slip Gaji | Akses lembar slip gaji di ESS sebelum tanggal rilis resmi atau konfirmasi bayar Finance | `403 Forbidden` | "Dokumen slip gaji belum dirilis: Menunggu konfirmasi pembayaran transfer bank dari Finance." | `HRD-DEC-127`, `PAY-B5` |
| `VR-PAY-19` | Autentikasi PIN Wajib | Membuka rincian slip gaji di ESS tanpa menyertakan verifikasi PIN sekunder yang sah | `401 Unauthorized` | "Autentikasi sekunder diperlukan: Masukkan PIN 6-digit untuk membuka slip gaji." | `HRD-DEC-127` |
| `VR-PAY-20` | Penalti Salah PIN | Percobaan salah PIN slip gaji mencapai $\ge 3$ kali berturut-turut | `429 Too Many Requests`| "Akses slip gaji dibekukan sementara selama 30 menit akibat 3 kali kesalahan input PIN." | `HRD-DEC-127`, `PAY-B6` |
| `VR-PAY-21` | Format PIN Sekunder | Pendaftaran/reset PIN baru tidak berupa 6-digit numerik atau kombinasi berulang | `400 Bad Request` | "PIN slip gaji wajib terdiri dari 6-digit angka acak dan bukan kombinasi berulang (misal 111111)." | `HRD-DEC-127` |
| `VR-PAY-22` | Proteksi Password PDF | Berkas PDF slip gaji di-generate tanpa proteksi kata sandi dan watermark | `422 Unprocessable Entity` | "Gagal memproses dokumen: Seluruh berkas PDF slip gaji wajib dilindungi kata sandi kombinasi NIK." | `HRD-DEC-127` |
| `VR-PAY-23` | Batas Cut-Off Setting | Pengaturan tanggal cut-off bulanan di luar rentang tanggal 15 hingga 25 | `400 Bad Request` | "Tanggal cut-off bulanan hanya dapat diatur dalam rentang tanggal 15 s.d. 25 setiap bulannya." | `HRD-ASM-27` |
| `VR-PAY-24` | Batas Ambang Variansi | Pengaturan ambang batas variansi di luar rentang 5% hingga 50% | `400 Bad Request` | "Ambang persentase peringatan variansi anomali harus berada di antara 5.00% hingga 50.00%." | `HRD-ASM-29` |
| `VR-PAY-25` | Validitas Dokumen Rapel| Penyertaan rapel retrospektif tanpa nomor SK atau ID transaksi rekonsiliasi asal | `400 Bad Request` | "Komponen rapel retrospektif wajib menautkan ID referensi transaksi asal yang sah." | `HRD-DEC-123`, `PAY-W5` |

## 20. Bagian `S-B6` — Matriks Validasi Restitusi & Pemotongan Penggajian Pegawai Rumah Sakit (Sub-modul 11, `last_changed_in` `v15`, 5 November 2026)

Bagian ini mendefinisikan matriks aturan validasi bisnis, validasi format, dan penjaga keselamatan finansial (**`VR-DED-01` s.d. `VR-DED-25`**) untuk **Sub-modul 11 (Restitution & Payroll Deduction Integration — Slice `S-B6`)**.

---

| Kode Aturan | Kategori Validasi | Target Entitas / Kolom | Kondisi Kegagalan / Pemicu Error | Pesan Kesalahan Sistem (Error Message) | Tingkat Keparahan |
| :--- | :--- | :--- | :--- | :--- | :---: |
| `VR-DED-01` | Hard Invariant | `PayrollDeductionEngine` | Total potongan non-regulasi melebihi 50% dari penghasilan kotor (*Gross Pay*) bulan berjalan (`HRD-DEC-128`) | *"Pelanggaran Batas Regulasi: Total potongan non-regulasi tidak boleh melebihi 50% dari penghasilan kotor (Pasal 65 PP 36/2021). Sisa tagihan otomatis ditunda ke bulan depan."* | `FATAL / BLOCKED` |
| `VR-DED-02` | Hard Invariant | `PayrollDeductionEngine` | Pemotongan dieksekusi melangkahi urutan prioritas berjenjang (Ganti Rugi $
ightarrow$ Kasbon RS $
ightarrow$ Koperasi $
ightarrow$ Paguyuban) | *"Pelanggaran Hirarki: Eksekusi potongan wajib mengikuti urutan prioritas berjenjang 1 hingga 4 secara mutlak."* | `FATAL / BLOCKED` |
| `VR-DED-03` | Hard Invariant | `TrxPayrollDeductionItem` | Tagihan yang melampaui plafon 50% tidak dialihkan statusnya ke penundaan (*carry-over*) | *"Kesalahan Alur: Tagihan yang melebihi batas 50% wajib diberi status DeferredDueToSalaryCap dan dijadwalkan ulang."* | `FATAL / BLOCKED` |
| `VR-DED-04` | Hard Invariant | `TrxPayrollRestitution` | Percobaan mengedit atau merevisi slip gaji final periode lampau untuk memperbaiki salah potong (`PAY-B6`) | *"Integritas Finansial: Slip gaji final masa lalu haram diedit ulang. Pengembalian wajib melalui transaksi restitusi koreksi berjalan."* | `FATAL / BLOCKED` |
| `VR-DED-05` | Hard Invariant | `TrxPayrollRestitution` | Kompensasi salah potong masa lalu dikenakan potongan pajak PPh 21 pada slip gaji berjalan | *"Aturan Perpajakan: Kompensasi pengembalian salah potong (RestitutionAdjustment) wajib bebas pajak untuk menghindari pajak berganda."* | `FATAL / BLOCKED` |
| `VR-DED-06` | Hard Invariant | `TrxCashierDisbursementVoucher`| Klaim yang telah dicairkan tunai di kasir RS dimasukkan ke dalam antrean transfer slip gaji bulanan | *"Bahaya Kerugian RS: Klaim ini telah dicairkan tunai di Kasir RS (DisbursedViaCashier) dan dilarang ditransfer ulang via gaji."* | `FATAL / BLOCKED` |
| `VR-DED-07` | Hard Invariant | `ExitClearanceReceivable`| Pemotongan pesangon akhir PHK/Resign di atas 50% dieksekusi pada siklus gaji bulanan normal | *"Batas Wewenang: Pengecualian batas 50% hanya sah diberlakukan pada slip penutupan pemisahan kerja (Final Clearance Payslip)."* | `FATAL / BLOCKED` |
| `VR-DED-08` | Hard Invariant | `TrxEmployeeRestitutionClaim`| Atasan menyetujui pengajuan klaim restitusi atas dirinya sendiri (*Self-Approval*) | *"Pelanggaran SoD: Anda tidak diizinkan menyetujui klaim restitusi Anda sendiri. Pengajuan dialihkan ke atasan 1 tingkat di atas Anda."* | `FATAL / BLOCKED` |
| `VR-DED-09` | Format / Batas | `ExpenseDate` | Tanggal pengeluaran pada kuitansi klaim lebih lampau dari 60 hari kalender | *"Batas Kedaluwarsa Nota: Kuitansi klaim telah kedaluwarsa (maksimal 60 hari sejak tanggal pengeluaran)."* | `ERROR` |
| `VR-DED-10` | Bisnis / Batas | `ClaimAmount` | Total klaim kacamata kerja melebihi sisa plafon tahunan yang ditetapkan untuk golongan pemohon | *"Plafon Tidak Mencukupi: Nilai klaim kacamata melebihi sisa batas plafon tahunan golongan Anda."* | `ERROR` |
| `VR-DED-11` | Bisnis / Batas | `ClaimAmount` | Total klaim faskes luar melebihi sisa plafon tahunan yang ditetapkan untuk golongan pemohon | *"Plafon Tidak Mencukupi: Nilai klaim faskes luar melebihi sisa batas plafon tahunan golongan Anda."* | `ERROR` |
| `VR-DED-12` | Kelayakan Staf | `EmployeeId` | Pegawai belum memenuhi masa kerja minimal yang disyaratkan untuk benefit tersebut | *"Belum Memenuhi Syarat: Masa kerja Anda belum memenuhi ketentuan minimum untuk mengajukan kategori manfaat ini."* | `ERROR` |
| `VR-DED-13` | Waktu Berlaku | `CashierVoucher` | Percobaan verifikasi voucher kasir cito yang telah melewati batas 48 jam sejak diterbitkan | *"Voucher Kedaluwarsa: Voucher kasir darurat telah melewati batas waktu 48 jam dan tidak dapat dicairkan lagi."* | `ERROR` |
| `VR-DED-14` | Keamanan Token | `SecurityTokenHash` | Hash token yang dimasukkan petugas kasir tidak cocok dengan data terenkripsi sistem | *"Otorisasi Ditolak: Token otorisasi kasir tidak valid atau tidak cocok dengan data voucher."* | `FATAL / BLOCKED` |
| `VR-DED-15` | Validasi Berkas | `ReceiptAttachmentUrl` | Format berkas bukti bukan PDF/JPG/PNG atau ukuran melebihi 5 MB | *"Format Berkas Tidak Sah: Lampiran kuitansi wajib berupa file PDF, JPG, atau PNG dengan ukuran maksimal 5 MB."* | `ERROR` |
| `VR-DED-16` | Kelengkapan Medis| `MedicalResumeUrl` | Klaim faskes luar atau kacamata diajukan tanpa mengunggah resume medis / resep dokter sah | *"Dokumen Kurang: Pengajuan kategori ini wajib melampirkan resume medis dokter atau resep optik yang sah."* | `ERROR` |
| `VR-DED-17` | Batas Waktu | `DeductionSchedule` | Percobaan penambahan jadwal cicilan bulan berjalan setelah tanggal cut-off 20 pukul 23:59:59 | *"Periode Terkunci: Jadwal cicilan bulan berjalan telah dikunci per tanggal 20. Jadwal baru akan berlaku pada bulan depan."* | `ERROR` |
| `VR-DED-18` | Integritas Data | `RemainingPrincipal` | Nilai sisa pokok pinjaman bernilai negatif | *"Integritas Data: Nilai sisa pokok pinjaman tidak boleh lebih kecil dari nol."* | `FATAL / BLOCKED` |
| `VR-DED-19` | Integritas Data | `MonthlyInstallment` | Nilai angsuran per bulan bernilai nol atau negatif | *"Input Tidak Valid: Besaran angsuran bulanan wajib bernilai positif lebih besar dari nol."* | `ERROR` |
| `VR-DED-20` | Kebijakan SDM | `DeductionSchedule` | Penjeda cicilan diajukan melebihi 3 bulan kalender berturut-turut tanpa persetujuan Direktur | *"Batas Maksimal Jeda: Penjeda pemotongan cicilan maksimal 3 bulan berturut-turut dan wajib melampirkan SK Direksi."* | `ERROR` |
| `VR-DED-21` | Integrasi Exit | `ExitClearanceReceivable`| Formulir Exit Clearance Sub-20 disahkan padahal pegawai masih memiliki tunggakan piutang | *"Exit Clearance Terblokir: Pegawai masih memiliki sisa piutang tertunggak yang belum disetor ke Kasir RS."* | `FATAL / BLOCKED` |
| `VR-DED-22` | Anti-Fraud | `ReceiptNumber` | Ditemukan nomor kuitansi yang sama persis telah diajukan sebelumnya oleh staf mana pun | *"Duplikasi Bukti Nota: Nomor kuitansi ini telah terdaftar pada pengajuan klaim sebelumnya (Indikasi Duplikasi)."* | `FATAL / BLOCKED` |
| `VR-DED-23` | Kewenangan HR | `ApprovedAmount` | Petugas HR C&B menyetujui nominal yang lebih besar daripada nominal klaim diajukan | *"Batas Nilai: Nominal yang disetujui tidak boleh melampaui nominal kuitansi yang diajukan oleh pegawai."* | `ERROR` |
| `VR-DED-24` | Integrasi Fin | `ExternalReferenceId` | Data cicilan ditarik dari Finance tanpa mencantumkan ID kontrak piutang eksternal yang valid | *"Data Tidak Lengkap: Rekod piutang Finance wajib memiliki kode referensi piutang (AR Code) yang valid."* | `ERROR` |
| `VR-DED-25` | Otorisasi Akses | `DeductionSchedule` | Pembatalan jadwal cicilan aktif dilakukan tanpa hak akses khusus `HR_Deduction_Admin` | *"Akses Ditolak: Pembatalan jadwal cicilan pinjaman hanya dapat dilakukan oleh Administrator Penggajian."* | `FATAL / BLOCKED` |


---

## 21. Matriks Validasi Sub-Modul 12 (Slice S-D3 Benefit Management)

Bagian ini mendefinisikan 25 aturan validasi bisnis teknis (`VR-BEN-01` s.d. `VR-BEN-25`) untuk **Sub-Modul 12 (Benefit Management — Slice `S-D3`)**, mencakup penegakan batas kuota keluarga inti sah, batasan umur anak kuliah, aturan non-negatif saldo plafon internal RS, mandatory nomor SEP BPJS Kesehatan pada klaim COB, aktivasi bertahap, dan integritas penutupan hak pada Exit Clearance.

---

### Tabel Spesifikasi Aturan Validasi (`VR-BEN-01` s.d. `VR-BEN-25`)

| ID Aturan | Nama Validasi | Entitas Target | Pemicu (Trigger) | Logika Validasi / Kondisi Prasyarat | Pesan Penolakan (Error Message) | Tingkat Keparahan |
| :--- | :--- | :--- | :--- | :--- | :--- | :---: |
| `VR-BEN-01` | Batas Kuota Pasangan Sah | `TrxEmployeeBenefitDependent` | Pendaftaran Pasangan | Jumlah pasangan aktif dengan `RelationshipType == 'Spouse'` dalam 1 enrollment tidak boleh melebihi 1 orang. | "Pendaftaran pasangan ditolak. Pegawai hanya berhak mendaftarkan maksimal 1 pasangan sah." | `Error` |
| `VR-BEN-02` | Batas Kuota Anak Sah | `TrxEmployeeBenefitDependent` | Pendaftaran Anak | Jumlah anak aktif dengan `RelationshipType == 'Child'` dalam 1 enrollment tidak boleh melebihi 3 orang. | "Pendaftaran anak ditolak. Kuota pertanggungan anak sah maksimal adalah 3 orang." | `Error` |
| `VR-BEN-03` | Format NIK Kependudukan | `TrxEmployeeBenefitDependent` | Input NIK Tanggungan | Kolom `national_id_number` wajib berupa string numerik tepat 16 digit. | "Format NIK tidak valid. NIK wajib terdiri dari tepat 16 digit angka kependudukan." | `Error` |
| `VR-BEN-04` | Format Nomor Kartu Keluarga | `TrxEmployeeBenefitDependent` | Input Nomor KK | Kolom `family_card_number` wajib berupa string numerik tepat 16 digit. | "Nomor Kartu Keluarga tidak valid. Nomor KK wajib terdiri dari tepat 16 digit angka." | `Error` |
| `VR-BEN-05` | Batas Usia Standar Anak | `TrxEmployeeBenefitDependent` | Pendaftaran Anak | Tanggal lahir anak harus memastikan usia anak <= 21 tahun jika `is_active_student == false`. | "Pendaftaran anak ditolak. Batas usia tanggungan anak non-mahasiswa adalah maksimal 21 tahun." | `Error` |
| `VR-BEN-06` | Batas Usia Mahasiswa Aktif | `TrxEmployeeBenefitDependent` | Pendaftaran Anak Kuliah | Jika `is_active_student == true`, usia anak maksimal 25 tahun 0 bulan pada tanggal pendaftaran. | "Pendaftaran anak ditolak. Batas usia maksimal anak berstatus mahasiswa aktif adalah 25 tahun." | `Error` |
| `VR-BEN-07` | Mandatory Surat Dekanat Kuliah | `TrxEmployeeBenefitDependent` | Pendaftaran Anak Kuliah | Jika anak berusia > 21 tahun, berkas `student_verification_letter_path` wajib diunggah. | "Surat Keterangan Mahasiswa Aktif dari perguruan tinggi wajib dilampirkan untuk anak usia di atas 21 tahun." | `Error` |
| `VR-BEN-08` | Masa Berlaku Surat Kuliah | `TrxEmployeeBenefitDependent` | Validasi Tahunan Surat | Kolom `student_verification_expiry_date` tidak boleh melewati tanggal 31 Agustus tahun ajaran berjalan. | "Masa berlaku surat keterangan kuliah telah kedaluwarsa. Harap unggah surat keterangan mahasiswa baru." | `Error` |
| `VR-BEN-09` | Format Nomor BPJS Kesehatan | `TrxEmployeeInsuranceEnrollment`| Input Nomor BPJS Kes | Kolom `bpjs_kesehatan_number` jika diisi wajib terdiri dari 13 digit angka valid. | "Nomor kartu BPJS Kesehatan tidak valid. Nomor kepesertaan BPJS Kesehatan wajib 13 digit angka." | `Error` |
| `VR-BEN-10` | Format Nomor BPJS Ketenagakerjaan| `TrxEmployeeInsuranceEnrollment`| Input Nomor BPJS TK | Kolom `bpjs_ketenagakerjaan_number` jika diisi wajib terdiri dari 11 digit angka valid. | "Nomor kartu BPJS Ketenagakerjaan tidak valid. Nomor KPJ BPJS TK wajib 11 digit angka." | `Error` |
| `VR-BEN-11` | Aktivasi Cepat BPJS TK H+0 | `TrxEmployeeInsuranceEnrollment`| Event Onboarding | Tanggal efektif `effective_date` program BPJS TK JKK & JKM wajib sama dengan hari pertama kerja H+0. | "Aktivasi BPJS TK ditolak. Perlindungan JKK & JKM wajib langsung berlaku efektif pada tanggal mulai bekerja." | `Error` |
| `VR-BEN-12` | Penahanan Masa Percobaan | `TrxEmployeeInsuranceEnrollment`| Pendaftaran Baru | Fasilitas BPJS Kesehatan, asuransi swasta & subsidi RS wajib berstatus `PendingActivation` selama masa kerja < 3 bulan. | "Aktivasi paket kesehatan ditolak. Pegawai masih berada dalam masa evaluasi percobaan (belum lulus probation)." | `Error` |
| `VR-BEN-13` | Verifikasi Kelulusan Percobaan | `TrxEmployeeInsuranceEnrollment`| Aktivasi Pasca-Percobaan| Parameter `probationEvaluationId` wajib merujuk ke SK kelulusan evaluasi yang sah (`ProbationPassed`). | "Aktivasi paket benefit ditolak. SK pengangkatan atau kelulusan masa percobaan 3 bulan tidak ditemukan." | `Error` |
| `VR-BEN-14` | Saldo Plafon Non-Negatif | `HrdBenefitBalanceMovement` | Transaksi Kasir SIMRS | Nilai `balance_after` setelah pemotongan biaya tindakan di loket kasir SIMRS wajib `>= 0.00`. | "Transaksi pemotongan saldo ditolak. Sisa saldo plafon tidak mencukupi untuk menanggung seluruh biaya tindakan."| `Error` |
| `VR-BEN-15` | Maksimal Pemotongan Saldo | `HrdBenefitBalanceMovement` | Deduct Saldo SIMRS | Nilai mutasi pengurangan saldo maksimal sebesar sisa saldo berjalan sebelum transaksi (`balance_before`). | "Nominal pemotongan plafon tidak boleh melebihi sisa saldo berjalan." | `Error` |
| `VR-BEN-16` | Mandatory SEP Klaim COB | `TrxBenefitClaim` | Submit Klaim COB ESS | Untuk jenis klaim rawat inap berpenjamin sekunder, kolom `sep_bpjs_number` wajib terisi dan tidak boleh null. | "Pengajuan klaim COB ditolak. Nomor Surat Eligibilitas Peserta (SEP) BPJS Kesehatan wajib disertakan." | `Error` |
| `VR-BEN-17` | Format Nomor SEP BPJS | `TrxBenefitClaim` | Input Nomor SEP | Format `sep_bpjs_number` wajib mengikuti standar BPJS Kesehatan (panjang 19 digit alfanumerik). | "Format nomor SEP tidak valid. Nomor SEP BPJS Kesehatan wajib 19 karakter (contoh: 0041R0011126V000123)." | `Error` |
| `VR-BEN-18` | Konsistensi Breakdown Biaya Klaim| `TrxBenefitClaim` | Submit Klaim Biaya | `total_hospital_bill` wajib sama dengan `primary_payer_covered_amount + secondary_claim_amount + out_of_pocket_excess_amount`. | "Perhitungan rincian biaya tidak konsisten. Total kuitansi wajib sama dengan penjumlahan klaim primer, sekunder, dan ekses." | `Error` |
| `VR-BEN-19` | Batas Waktu Kadaluwarsa Klaim | `TrxBenefitClaim` | Submit Klaim ESS | Tanggal pelayanan `service_date` tidak boleh lebih dari 60 hari kalender sebelum tanggal submit `submitted_at`. | "Pengajuan klaim kedaluwarsa. Batas maksimal pengajuan klaim biaya kesehatan adalah 60 hari sejak tanggal perawatan." | `Error` |
| `VR-BEN-20` | Mandatory Kuitansi Asli Klaim | `TrxBenefitClaimDocument` | Unggah Berkas Klaim | Pengajuan klaim sekunder wajib memuat minimal 1 berkas bertipe `Receipt_Original` atau `Hospital_Invoice`. | "Dokumen klaim tidak lengkap. Berkas salinan kuitansi asli rumah sakit dan rincian biaya wajib diunggah." | `Error` |
| `VR-BEN-21` | Format & Ukuran Berkas Digital | `TrxBenefitClaimDocument` | Unggah Lampiran | Format berkas wajib PDF, JPG, atau PNG dengan ukuran maksimal 5 MB (5.242.880 bytes). | "Unggahan berkas gagal. Format wajib PDF/JPG/PNG dan ukuran maksimal berkas adalah 5 MB." | `Error` |
| `VR-BEN-22` | Hangus Saldo Akhir Tahun | `HrdBenefitBalanceMovement` | Reset 1 Januari | Mutasi reset `YearEndExpiryReset` wajib menghapus sisa saldo tahun berjalan menjadi Rp 0.00. | "Penyelarasan saldo akhir tahun tidak valid. Saldo sisa tahun lalu tidak dapat diakumulasikan ke tahun baru." | `Error` |
| `VR-BEN-23` | Tanggal Efektif Terminasi | `TrxEmployeeInsuranceEnrollment`| Penonaktifan Resign/PHK| Kolom `termination_date` wajib sama dengan `EffectiveTerminationDate` yang disetujui pada berkas kepegawaian. | "Tanggal pengakhiran benefit tidak valid. Tanggal terminasi wajib selaras dengan tanggal resmi keluar kerja." | `Error` |
| `VR-BEN-24` | Blokir Exit Clearance Tunggakan | `TrxEmployeeInsuranceEnrollment`| Verifikasi Exit Clearance| Pegawai dilarang memperoleh tanda bebas kewajiban medis di Sub-20 jika memiliki ekses tagihan berobat belum lunas. | "Persetujuan Exit Clearance ditolak. Pegawai masih memiliki tunggakan ekses tagihan perawatan medis di RS." | `Error` |
| `VR-BEN-25` | Alasan Penolakan Verifikasi | `TrxEmployeeBenefitDependent` | Penolakan Berkas HR | Jika status diubah menjadi `Rejected`, kolom `rejection_reason` wajib diisi minimal 15 karakter. | "Alasan penolakan dokumen kependudukan wajib diisi secara rinci agar pegawai dapat melakukan perbaikan." | `Error` |


---

## 22. Matriks Aturan Validasi Sub-Modul 13 (Slice S-D4 HR Service Management & Request — Layanan Administrasi, Helpdesk SDM & Penerbitan Dokumen Kepegawaian RS)

Bagian ini mendefinisikan seluruh aturan validasi (*validation rules*) untuk **Sub-Modul 13 (Slice `S-D4`)**, mencakup penegakan SLA 3-tier, kelayakan prioritas Cito, penarikan otomatis nominal gaji dari Sub-10, proteksi PIN finansial, format penomoran gapless, serta protokol revokasi dokumen kepegawaian rumah sakit.

### Tabel Spesifikasi Aturan Validasi (`VR-HRS-01` s.d. `VR-HRS-25`)

| ID Aturan | Nama Validasi | Entitas Target | Pemicu (Trigger) | Logika Validasi / Kondisi Prasyarat | Pesan Penolakan (Error Message) | Tingkat Keparahan |
| :--- | :--- | :--- | :--- | :--- | :--- | :---: |
| `VR-HRS-01` | Mandatory Berkas Persyaratan | `TrxHrServiceRequest` | Submit Tiket ESS | Jika `MstHrServiceType.requires_attachment == true`, minimal 1 berkas pada `TrxHrServiceRequestAttachment` wajib diunggah. | "Pengajuan tiket ditolak. Jenis layanan ini mewajibkan pengunggahan berkas bukti pendukung." | `Error` |
| `VR-HRS-02` | Mandatory Disposisi Prioritas Cito | `TrxHrServiceRequest` | Submit Tiket Cito | Jika `priority_tier == 'Cito'`, wajib terdapat lampiran berkas bertipe `DisposisiCito` yang ditandatangani atasan klinis. | "Prioritas Cito (SLA 4 Jam) ditolak. Permohonan cito wajib melampirkan surat disposisi darurat dari Kepala Ruangan/Komite Medik." | `Error` |
| `VR-HRS-03` | Validasi Target Jam SLA | `MstHrServiceType` | Konfigurasi SLA Master | Nilai `sla_target_hours` wajib berada di rentang 1 s.d. 168 jam (Cito: maks 4 jam, Standar: maks 24 jam, Khusus: maks 72 jam). | "Durasi SLA tidak valid. Jam kerja target SLA wajib antara 1 hingga 168 jam." | `Error` |
| `VR-HRS-04` | Format Nomor Tiket Unik | `TrxHrServiceRequest` | Pembuatan Tiket Baru | Kolom `ticket_number` wajib mengikuti format `TKT-YYYYMM-XXXXX` dan unik secara sistem. | "Nomor tiket tidak valid atau terjadi duplikasi nomor tiket sistem." | `Error` |
| `VR-HRS-05` | Mandatory Slip Gaji Final Sub-10 | `TrxEmployeeDocumentRequest` | Draf Surat Finansial | Pembuatan draf surat berpenghasilan (`is_financial_sensitive == true`) wajib merujuk ke slip gaji Sub-10 yang berstatus finalized. | "Pembuatan surat keterangan gaji ditolak. Data slip gaji pegawai periode terakhir belum berstatus finalized di sistem Payroll." | `Error` |
| `VR-HRS-06` | Larangan Edit Manual Nominal Gaji | `TrxEmployeeDocumentRequest` | Penyusunan Draf Surat | Kolom `salary_bruto_amount` dan `salary_netto_amount` dilarang menerima input manual di form (wajib ditarik otomatis dari `INT-HR-56`). | "Manipulasi nominal gaji terdeteksi. Angka gaji pada draf surat wajib ditarik otomatis dari riwayat slip gaji resmi." | `Error` |
| `VR-HRS-07` | Mandatory Approval C&B Finansial | `TrxEmployeeDocumentRequest` | Otorisasi Surat Gaji | Surat finansial dilarang diterbitkan (`Authorized`) sebelum disetujui oleh Supervisor Kompensasi & Benefit (`CbApproved`). | "Penerbitan surat gaji ditolak. Dokumen wajib diverifikasi dan disetujui terlebih dahulu oleh Supervisor Kompensasi & Benefit." | `Error` |
| `VR-HRS-08` | Pemisahan Wewenang Otorisasi | `TrxEmployeeDocumentRequest` | Otorisasi Dokumen | ID pengguna penandatangan (`authorized_signer_user_id`) dilarang sama dengan ID pegawai pemohon (`requested_by_employee_id`). | "Otorisasi surat ditolak. Pejabat dilarang menandatangani surat keterangan resmi atas nama dirinya sendiri (Anti-Self-Approval)." | `Error` |
| `VR-HRS-09` | Format Pola Penomoran Surat | `MstEmployeeDocumentType` | Konfigurasi Penomoran | Format `numbering_pattern` wajib mengandung token `[NO_URUT]` dan `[TAHUN]`. | "Format penomoran surat tidak valid. Pola penomoran wajib menyertakan variabel [NO_URUT] dan [TAHUN]." | `Error` |
| `VR-HRS-10` | Integritas Nomor Seri Gapless | `TrxEmployeeDocumentIssuance` | Pengambilan Nomor Surat | Nilai nomor urut pada naskah dinas wajib persis `last_sequence_number + 1` dan dikunci transaksi atomik database. | "Gagal menerbitkan nomor surat. Terjadi konflik konkurensi penomoran naskah dinas." | `Error` |
| `VR-HRS-11` | Format 4-Digit PIN Finansial | `TrxEmployeeDocumentIssuance` | Penerbitan Surat Gaji | Kolom `security_pin_hash` wajib dihasilkan dari tepat 4 karakter numerik angka (0000 - 9999). | "PIN proteksi dokumen tidak valid. PIN verifikasi wajib terdiri dari tepat 4 angka numerik rahasia." | `Error` |
| `VR-HRS-12` | Pembatasan Percobaan PIN Publik | Portal Verifikasi Publik | Verifikasi PIN Gaji | Percobaan input PIN salah dibatasi maksimal 3 kali berturut-turut per alamat IP/jam sebelum dikunci selama 30 menit. | "Batas percobaan PIN terlampaui. Akses pembukaan nominal gaji dikunci sementara selama 30 menit demi keamanan." | `Error` |
| `VR-HRS-13` | Masa Berlaku Dokumen Sah | `TrxEmployeeDocumentIssuance` | Kalkulasi Expired | Kolom `valid_until_date` dihitung otomatis: `issued_date + validity_period_days` (default 90 hari kalender). | "Masa berlaku dokumen tidak valid. Tanggal kedaluwarsa dokumen wajib dihitung sesuai master jenis naskah dinas." | `Error` |
| `VR-HRS-14` | Mandatory Berita Acara Revokasi | `TrxEmployeeDocumentIssuance` | Pencabutan Surat | Pencabutan keabsahan dokumen wajib menyertakan `revocation_bap_number` dan `revocation_reason` minimal 20 karakter. | "Pencabutan dokumen ditolak. Nomor Berita Acara Pemeriksaan (BAP) dan alasan pembatalan tertulis wajib diisi secara rinci." | `Error` |
| `VR-HRS-15` | Format & Batas Ukuran Lampiran | `TrxHrServiceRequestAttachment`| Unggah Berkas Bukti | Berkas wajib berekstensi PDF, JPG, atau PNG dengan ukuran maksimal 5 MB (5.242.880 bytes). | "Unggah berkas gagal. Format wajib PDF/JPG/PNG dan ukuran maksimal berkas adalah 5 MB." | `Error` |
| `VR-HRS-16` | Validasi Token Template HTML | `MstEmployeeDocumentType` | Simpan Template Dokumen| Token variabel di dalam `template_body_html` wajib merupakan himpunan bagian dari token yang didukung sistem. | "Penyimpanan template ditolak. Ditemukan variabel token tidak dikenal yang tidak didukung mesin persuratan." | `Error` |
| `VR-HRS-17` | Mandatory Nomor Rekening Baru | `HrdProfileChangeHistory` | Permohonan Rekening Bank | Permohonan pemutakhiran rekening payroll wajib memuat Nama Bank, Nomor Rekening (numerik), dan Nama Pemilik Rekening sesuai KTP. | "Data permohonan rekening bank tidak lengkap. Nama Bank, Nomor Rekening, dan Nama Pemilik wajib diisi lengkap." | `Error` |
| `VR-HRS-18` | Mandatory Scan Buku Tabungan | `TrxHrServiceRequestAttachment`| Ubah Rekening Payroll | Permohonan ubah rekening payroll wajib melampirkan berkas bertipe `BukuTabungan` yang memperlihatkan nama & nomor rekening jelas. | "Berkas permohonan ditolak. Scan halaman depan buku tabungan asli wajib diunggah sebagai bukti kepemilikan rekening." | `Error` |
| `VR-HRS-19` | Wajib Keterangan Alasan Penolakan | `TrxHrServiceRequest` | Penolakan Tiket HR | Jika tiket diubah status menjadi `Rejected`, kolom `resolution_summary` wajib diisi minimal 15 karakter. | "Penolakan tiket wajib menyertakan alasan penolakan yang jelas agar staf pemohon dapat memperbaiki permohonannya." | `Error` |
| `VR-HRS-20` | Imutabilitas Dokumen Terbit | `TrxEmployeeDocumentIssuance` | Edit Dokumen Terbit | Perubahan nilai kolom pada dokumen yang telah berstatus `Active` dilarang (hanya status yang boleh berubah ke `Revoked`/`Expired`). | "Pembaruan dokumen ditolak. Dokumen resmi yang telah diterbitkan bersifat kekal dan tidak dapat diedit secara sepihak." | `Error` |
| `VR-HRS-21` | Larangan Hapus Fisik Dokumen | `TrxEmployeeDocumentIssuance` | Hapus Dokumen | Perintah `DELETE` pada tabel penerbitan dokumen dilarang keras di tingkat database maupun API (*hard delete prohibited*). | "Penghapusan berkas naskah dinas resmi dilarang demi menjaga kepatuhan rekam jejak audit hukum rumah sakit." | `Error` |
| `VR-HRS-22` | Otomasi Kliring Paklaring Sub-20 | `TrxEmployeeDocumentRequest` | Draf Surat Paklaring | Pengajuan Surat Pengalaman Kerja (Paklaring) ditolak jika pegawai belum dinyatakan lulus bebas kewajiban pada Exit Clearance Sub-20. | "Penerbitan Paklaring ditolak. Staf belum menyelesaikan tahapan bebas tanggungan (Exit Clearance) kepegawaian." | `Error` |
| `VR-HRS-23` | Larangan Paklaring Terminasi Berat| `TrxEmployeeDocumentRequest` | Draf Surat Paklaring | Pegawai yang berhenti karena Pemutusan Hubungan Kerja Tidak Dengan Hormat (PHK-TDH Sub-19) dilarang diterbitkan surat rekomendasi standar. | "Penerbitan surat rekomendasi ditolak. Staf tercatat memiliki riwayat sanksi terminasi pelanggaran berat rumah sakit." | `Error` |
| `VR-HRS-24` | Validasi Hash Integritas Dokumen | `TrxEmployeeDocumentIssuance` | Generate QR Code | Nilai `verification_hash` wajib merupakan hash SHA-256 sepanjang tepat 64 karakter heksadesimal unik. | "Integritas dokumen cacat. Hash verifikasi naskah dinas gagal dibangkitkan oleh sistem." | `Error` |
| `VR-HRS-25` | Larangan Auto-Apply Tanpa Approval| `HrdProfileChangeHistory` | Mutasi Profil Otomatis| Eksekusi mutasi otomatis ke `MstWorkforceProfile` dilarang berjalan jika tiket sumber belum disetujui resmi oleh analis HR. | "Pembaruan profil dibatalkan. Data master tidak dapat dimutakhirkan sebelum tiket permohonan memperoleh persetujuan sah." | `Error` |


---

## 23. Matriks Aturan Validasi Sub-Modul 14 (Slice `S-C1` Credentialing, License & Clinical Privilege)

Bagian ini mendefinisikan seluruh aturan validasi (*validation rules*) untuk **Sub-Modul 14 (Slice `S-C1`)**, mencakup verifikasi sumber primer (PSV) STR seumur hidup & SIP 5 tahunan, tata kelola 3 komite mandiri, penilaian panel Mitra Bestari, hierarki otorisasi tunggal Direktur Utama, penegakan Hard-Stop di SIMRS/EMR (`INT-HR-69`), life-saving emergency override, kewenangan sementara dokter tamu, serta siklus 3 tahunan rekredensialing.

### Tabel Spesifikasi Aturan Validasi (`VR-CRD-01` s.d. `VR-CRD-25`)

| ID Aturan | Nama Validasi | Entitas Target | Tahap Evaluasi | Kondisi Gagal & Pesan Penolakan | Tingkat Keparahan |
| :--- | :--- | :--- | :--- | :--- | :---: |
| `VR-CRD-01` | Kelengkapan Berkas Permohonan Kredensial | `TrxCredentialingApplication` | Submit Berkas ESS | Gagal jika nomor STR/SIP kosong atau file ijazah/sertifikat kompetensi belum diunggah. Pesan: "Dokumen STR, SIP, ijazah, dan bukti kompetensi klinis wajib diunggah lengkap." | `Error` |
| `VR-CRD-02` | Keharusan Audit Verifikasi PSV Kemenkes | `WfpCredentialLicense` | Disposisi Sidang | Gagal jika `is_psv_verified == false` saat berkas akan diteruskan ke panel reviewer. Pesan: "Dokumen STR nakes wajib lolos verifikasi primer (PSV) ke portal KKI/SatuSehat sebelum dinilai komite." | `Error` |
| `VR-CRD-03` | Batas Masa Berlaku SIP Maksimal 5 Tahun | `WfpCredentialLicense` | Input Lisensi | Gagal jika `expiry_date` SIP diatur lebih dari 5 tahun sejak `issue_date`. Pesan: "Sesuai regulasi, masa berlaku Surat Izin Praktik (SIP) maksimal 5 tahun sejak tanggal terbit izin." | `Error` |
| `VR-CRD-04` | Prasyarat Logbook Minimum Tindakan Invasif | `TrxClinicalPrivilegeRequest` | Submit Checklist Whitebook | Gagal jika tindakan berisiko tinggi diajukan tanpa mencantumkan jumlah kasus logbook minimal. Pesan: "Permohonan tindakan bedah/invasif wajib melampirkan bukti logbook pengalaman kasus." | `Error` |
| `VR-CRD-05` | Kewajiban Penetapan Supervisor Level 3 | `TrxSpkRkkIssuance` | Penyusunan Draf SPK | Gagal jika tindakan Level 3 (Supervisi) tidak memiliki dokter supervisor terdaftar. Pesan: "Setiap butir kewenangan klinis di bawah supervisi wajib menetapkan dokter supervisor pendamping." | `Error` |
| `VR-CRD-06` | Penolakan Order SIMRS Lisensi Kedaluwarsa | `ClinicalPrivilegeGate` | Real-Time Gate SIMRS (`INT-HR-69`) | Gagal jika SIP kedaluwarsa atau SPK berstatus suspended saat nakes mengorder tindakan. Pesan: "Akses ditolak: Lisensi praktik (SIP) atau SPK nakes telah kedaluwarsa/ditangguhkan." | `Fatal` |
| `VR-CRD-07` | Batas Waktu Co-Signature Supervisor 24 Jam | `WfpClinicalPrivilege` | Approval Order Supervisi | Gagal jika order tindakan supervisi belum disetujui supervisor setelah 24 jam. Pesan: "Order tindakan supervisi dibatalkan otomatis karena melampaui batas waktu co-signature 24 jam." | `Error` |
| `VR-CRD-08` | Justifikasi Medis Cito Life-Saving Override | `TrxClinicalPrivilegeEmergencyOverrideLog` | Eksekusi Bypass EMR | Gagal jika teks alasan darurat klinis diisi kurang dari 20 karakter. Pesan: "Justifikasi medis darurat penyelamatan nyawa wajib diisi secara rinci dan objektif." | `Error` |
| `VR-CRD-09` | Batas Durasi Emergency Privilege 72 Jam | `TrxSpkRkkIssuance` | Fast-Track Bencana | Gagal jika durasi kewenangan darurat bencana diatur melebihi 72 jam. Pesan: "Kewenangan klinis darurat kebencanaan massal dibatasi maksimal 72 jam sejak diterbitkan." | `Error` |
| `VR-CRD-10` | Validitas Kode 3 Komite Mandiri | `MstCredentialingCommittee` | Konfigurasi Master | Gagal jika kode komite selain `MEDIK`, `KEPERAWATAN`, `NAKES_LAIN`. Pesan: "Kode komite profesi wajib salah satu dari: MEDIK, KEPERAWATAN, atau NAKES_LAIN." | `Error` |
| `VR-CRD-11` | Keharusan Minimal 2 Reviewer Mitra Bestari | `TrxCredentialingApplication` | Penetapan Peer Review | Gagal jika panel Mitra Bestari ditetapkan kurang dari 2 orang sejawat sebidang. Pesan: "Panel Mitra Bestari (Peer Review) wajib beranggotakan minimal 2 dokter/nakes seprofesi sebidang." | `Error` |
| `VR-CRD-12` | Otorisasi Tunggal Direktur Utama Rumah Sakit | `TrxSpkRkkIssuance` | Pengesahan Akhir | Gagal jika penandatangan SPK/RKK bukan akun Direktur Utama RS yang sah. Pesan: "Surat Penugasan Klinis (SPK) dan RKK wajib disahkan secara tunggal oleh Direktur Utama RS." | `Fatal` |
| `VR-CRD-13` | Batas Waktu Temporary Privilege Maks 3 Bulan | `TrxSpkRkkIssuance` | Otorisasi Konsultan Tamu | Gagal jika masa berlaku dokter tamu diatur > 90 hari atau perpanjangan > 1 kali. Pesan: "Kewenangan klinis sementara dokter konsultan tamu maksimal 3 bulan (dapat diperpanjang 1 kali)." | `Error` |
| `VR-CRD-14` | Batas Masa Berlaku SPK/RKK Maksimal 3 Tahun | `TrxSpkRkkIssuance` | Pengesahan Dirut | Gagal jika `effective_end_date` melebihi 3 tahun dari `effective_start_date`. Pesan: "Masa berlaku Surat Penugasan Klinis (SPK) standar rumah sakit dikunci maksimal 3 tahun." | `Error` |
| `VR-CRD-15` | Anti-Self-Review Mitra Bestari | `TrxClinicalPrivilegeAssessment` | Asesmen Portofolio | Gagal jika `peer_reviewer_workforce_id == applicant_workforce_id`. Pesan: "Reviewer Mitra Bestari dilarang menilai berkas permohonan kredensial dirinya sendiri." | `Fatal` |
| `VR-CRD-16` | Otomasi Suspended SPK Siklus 3 Tahun | `WfpClinicalPrivilege` | Scheduler Jatuh Tempo H-0 | Terpicu jika hingga H-0 pukul 00:00 SPK rekredensialing baru belum sah. Pesan: "Kewenangan klinis otomatis ditangguhkan (Suspended) karena masa berlaku SPK telah habis." | `Warning` |
| `VR-CRD-17` | Integritas Token QR Code Keabsahan Publik | `TrxSpkRkkIssuance` | Generate Dokumen Sah | Gagal jika token verifikasi QR kurang dari 32 karakter kriptografis unik. Pesan: "Token QR Code verifikasi dokumen wajib berintegritas tinggi dan terhindar dari duplikasi." | `Error` |
| `VR-CRD-18` | Blokir Jadwal Shift Sub-06 Saat SIP Kedaluwarsa | `TrxRosterSchedule` | Integrasi `INT-HR-71` | Gagal jika nakes dijadwalkan dinas saat status lisensi SIP expired. Pesan: "Tenaga medis tidak dapat dijadwalkan shift karena SIP telah kedaluwarsa." | `Fatal` |
| `VR-CRD-19` | Sinkronisasi Nilai OPPE Sub-15 untuk Rekredensial | `TrxRecredentialingApplication` | Inisiasi H-60 | Peringatan jika nilai kinerja klinis OPPE dokter di Sub-15 berada di bawah standar mutu (< 75). Pesan: "Peringatan: Nilai evaluasi kinerja klinis OPPE di bawah standar minimal komite medik." | `Warning` |
| `VR-CRD-20` | Peringatan Dini Radar SIP H-90 s.d. H-7 | `WfpCredentialLicense` | Scheduler Harian | Terpicu saat sisa masa berlaku SIP mencapai 90, 60, 30, dan 7 hari kerja. Pesan: "Peringatan: Masa berlaku Surat Izin Praktik (SIP) dokter akan segera berakhir." | `Info` |
| `VR-CRD-21` | Batas Audit KMKP Override Darurat 7 Hari | `TrxClinicalPrivilegeEmergencyOverrideLog` | Audit Mutu KMKP | Gagal jika sidang audit insiden darurat belum diputuskan setelah 7 hari kerja sejak kejadian. Pesan: "Audit keselamatan pasien terhadap bypass wewenang darurat wajib tuntas dalam 7 hari kerja." | `Warning` |
| `VR-CRD-22` | Batas Maksimal 3 Tempat Praktik SIP | `WfpCredentialLicense` | Registrasi Lisensi | Peringatan jika dokter telah mendaftarkan 3 faskes tempat praktik di portal KKI. Pesan: "Perhatian: Dokter telah memiliki 3 SIP aktif terdaftar sesuai batas maksimal regulasi Kemenkes." | `Warning` |
| `VR-CRD-23` | Tanggal Efektif SPK Tidak Boleh Mundur | `TrxSpkRkkIssuance` | Otorisasi Dirut | Gagal jika `effective_start_date` lebih awal dari tanggal penandatanganan digital Direktur Utama. Pesan: "Tanggal mulai berlaku SPK tidak boleh mendahului tanggal pengesahan Direktur Utama." | `Error` |
| `VR-CRD-24` | Larangan Supervisor untuk Tindakan Ditolak | `WfpClinicalPrivilege` | Penyusunan RKK | Gagal jika butir tindakan Level 1-2 (Ditolak) disematkan nama dokter supervisor. Pesan: "Tindakan yang tidak diberikan kewenangan klinis dilarang memiliki supervisor." | `Error` |
| `VR-CRD-25` | Keharusan BAP Pengadilan Etik untuk Revokasi | `TrxClinicalPrivilegeRevocation` | Pencabutan Kewenangan | Gagal jika pencabutan SPK dilakukan tanpa mengunggah berkas Berita Acara Sidang Etik Komite. Pesan: "Pencabutan kewenangan klinis wajib melampirkan berkas putusan resmi sidang etik komite." | `Fatal` |

---

## 24. Matriks Validasi Sub-Modul 15: OPPE, FPPE & Recredentialing (`S-C1-OPPE`)

### Tabel Spesifikasi Aturan Validasi (`VR-OPP-01` s.d. `VR-OPP-25`)

| ID Aturan | Nama Validasi | Entitas Target | Tahap Evaluasi | Kondisi Gagal & Pesan Penolakan | Tingkat Keparahan |
| :--- | :--- | :--- | :--- | :--- | :---: |
| `VR-OPP-01` | Komposisi Bobot OPPE Wajib Total 100% | `TrxOppeEvaluationPeriod` | Pembuatan Periode | Gagal jika `quantitative_weight + qualitative_weight != 100.00`. Pesan: "Total pembobotan kuantitatif SIMRS dan kualitatif telaah sejawat wajib tepat 100% (standar 60:40)." | `Error` |
| `VR-OPP-02` | Penugasan Peer Review Sekelompok KSM | `TrxOppeStaffEvaluationDetail` | Penetapan Reviewer | Gagal jika penilai sejawat bukan berasal dari kelompok spesialisasi KSM yang sama. Pesan: "Dokumen telaah sejawat wajib dievaluasi oleh dokter sejawat dari KSM sebidang." | `Error` |
| `VR-OPP-03` | Anti-Self-Review pada Penilaian Sejawat | `TrxOppeStaffEvaluationDetail` | Pengisian Peer Review | Gagal jika `reviewer_workforce_profile_id == staff_workforce_profile_id`. Pesan: "Dokter dilarang melakukan telaah sejawat terhadap kinerja klinis dirinya sendiri." | `Fatal` |
| `VR-OPP-04` | Wajib Catatan Kualitatif untuk Skor Rendah | `TrxOppeStaffEvaluationDetail` | Submit Peer Review | Gagal jika skor likert <= 2 tetapi kolom catatan naratif diisi kurang dari 25 karakter. Pesan: "Pemberian skor rendah (<= 2) wajib menyertakan ulasan deskriptif dan alasan klinis yang jelas." | `Error` |
| `VR-OPP-05` | Otomasi Pemicu FPPE dari Insiden Sentinel | `TrxFppeEngagement` | Deteksi SIMRS/KMKP | Terpicu jika terdeteksi minimal 1 kejadian sentinel keselamatan pasien di SIMRS. Pesan: "Insiden sentinel keselamatan pasien otomatis memicu penerbitan penugasan investigasi FPPE." | `Fatal` |
| `VR-OPP-06` | Pemicu Komplikasi Bedah Melebihi 2 SD | `TrxFppeEngagement` | Analisis Tren Kuantitatif | Terpicu jika angka komplikasi bedah dokter > 2 standar deviasi rata-rata KSM selama 3 bulan. Pesan: "Tingkat komplikasi bedah melampaui 2 deviasi standar memicu inisiasi evaluasi terarah FPPE." | `Error` |
| `VR-OPP-07` | Pemicu Keterlambatan Resume EMR > 48 Jam | `TrxFppeEngagement` | Evaluasi Timeliness EMR | Terpicu jika keterlambatan pengisian resume medis > 48 jam melebihi 20% kasus selama 2 bulan. Pesan: "Keterlambatan resume medis kronis melampaui 20% volume kasus memicu penerbitan FPPE." | `Warning` |
| `VR-OPP-08` | Kualifikasi Senioritas Dokter Proctor FPPE | `TrxFppeEngagement` | Penunjukan Proctor | Gagal jika proctor yang ditunjuk memiliki masa kerja klinis < 3 tahun atau bukan dokter spesialis penuh. Pesan: "Dokter proctor FPPE wajib merupakan dokter spesialis senior berpengalaman minimal 3 tahun." | `Error` |
| `VR-OPP-09` | Target Kasus Proctoring Staf Baru | `TrxFppeEngagement` | Onboarding Dokter Baru | Gagal jika target kasus diatur kurang dari 10 kasus atau durasi observasi kurang dari 3 bulan. Pesan: "Proctoring staf baru wajib memantau minimal 10 kasus klinis pertama dalam kurun waktu 3 bulan." | `Error` |
| `VR-OPP-10` | Target Kasus Proctoring Prosedur Baru | `TrxFppeEngagement` | Pengajuan Prosedur Baru | Gagal jika target kasus diatur kurang dari 5 kasus atau durasi observasi kurang dari 6 bulan. Pesan: "Proctoring penambahan prosedur baru wajib memantau minimal 5 kasus dalam kurun waktu 6 bulan." | `Error` |
| `VR-OPP-11` | Pseudonimisasi Rekam Medis pada Logbook | `TrxFppeCaseLog` | Entri Logbook Kasus | Gagal jika nomor rekam medis diinput dalam format identitas terbuka tanpa hashing/pseudonim. Pesan: "Nomor RM pasien wajib disamarkan (pseudonimisasi) demi kepatuhan privasi data klinis." | `Fatal` |
| `VR-OPP-12` | Verifikasi Mandiri Proctor Sebelum Sidang | `TrxFppeCaseLog` | Penutupan Kasus FPPE | Gagal jika terdapat logbook kasus yang belum ditandatangani dan diverifikasi oleh proctor. Pesan: "Seluruh catatan kasus dalam logbook proctoring wajib diverifikasi oleh dokter pendamping." | `Error` |
| `VR-OPP-13` | Batas Perpanjangan Proctoring Maks 3 Bulan | `TrxFppeEvaluationConclusion` | Sidang Pleno FPPE | Gagal jika masa perpanjangan proctoring diberikan melebihi 3 bulan atau diperpanjang > 1 kali. Pesan: "Perpanjangan observasi proctoring FPPE dibatasi maksimal 3 bulan untuk 1 kali kesempatan." | `Error` |
| `VR-OPP-14` | Otorisasi Sidang Pleno Putusan FPPE | `TrxFppeEvaluationConclusion` | Penetapan Vonis | Gagal jika berita acara putusan FPPE tidak ditandatangani oleh Ketua Subkomite Mutu Profesi. Pesan: "Putusan evaluasi terarah FPPE wajib disahkan secara resmi oleh sidang pleno Komite Medik." | `Fatal` |
| `VR-OPP-15` | Bobot Progresif Komposit 3 Tahun Rekredensial | `RecredentialingComposite` | Kalkulasi Komposit | Gagal jika formula pembobotan 3 tahun menyimpang dari rasio baku: Tahun 1 (25%), Tahun 2 (35%), Tahun 3 (40%). Pesan: "Rapor komposit 3 tahun rekredensialing wajib menggunakan pembobotan baku 25%-35%-40%." | `Error` |
| `VR-OPP-16` | Otomasi Kriteria Tier Hijau Rekredensialing | `RecredentialingComposite` | Klasifikasi Kelayakan | Terpenuhi jika nilai komposit 3 tahun >= 80.0 dan tidak memiliki riwayat insiden sentinel. Pesan: "Nakes berhak memperoleh rekomendasi jalur cepat (Tier Hijau Fast-Track) perpanjangan SPK." | `Info` |
| `VR-OPP-17` | Kriteria Wajib Klarifikasi Tier Kuning | `RecredentialingComposite` | Klasifikasi Kelayakan | Terpicu jika nilai komposit berada di rentang 65.0 - 79.9 atau terdapat catatan penundaan resume EMR. Pesan: "Nakes masuk Tier Kuning: Wajib menjalani wawancara klarifikasi dan pembinaan KSM." | `Warning` |
| `VR-OPP-18` | Kriteria Sidang Khusus Tier Merah | `RecredentialingComposite` | Klasifikasi Kelayakan | Terpicu jika skor komposit < 65.0 atau memiliki riwayat kegagalan FPPE / rujukan sidang etik. Pesan: "Nakes masuk Tier Merah: Wajib disidangkan dalam pleno khusus terkait pembatasan wewenang." | `Fatal` |
| `VR-OPP-19` | Wajib Catatan Justifikasi Override Tier | `RecredentialingComposite` | Penyesuaian Tier | Gagal jika Ketua Komite mengubah tier hasil kalkulator tanpa mengunggah nota pertimbangan klinis. Pesan: "Perubahan tier rekomendasi rekredensialing wajib melampirkan dasar justifikasi klinis tertulis." | `Error` |
| `VR-OPP-20` | Batasan 4 Putusan Sah Sidang Rekredensial | `RecredVerdict` | Penetapan Hasil Pleno | Gagal jika putusan di luar 4 status: `Perpanjang As-Is`, `Upgrade`, `Restricted`, atau `Revoked`. Pesan: "Putusan sidang rekredensialing wajib salah satu dari 4 keputusan baku komite medik." | `Error` |
| `VR-OPP-21` | Pembatasan Wewenang Menjadi Level 3 Supervisi | `ClinicalPrivilegeSync` | Dispatch SIMRS (`INT-HR-76`)| Terpicu saat putusan sidang berstatus `Restricted`. Wewenang mandiri diturunkan menjadi Level 3. Pesan: "Wewenang klinis disinkronkan ke SIMRS dengan status Level 3 (Wajib Co-Signature 24 Jam)." | `Warning` |
| `VR-OPP-22` | Hard-Stop Blokir Order SIMRS untuk Revokasi | `ClinicalPrivilegeSync` | Dispatch SIMRS (`INT-HR-76`)| Terpicu saat putusan berstatus `Revoked`. Sistem mengirim sinyal pemblokiran total hak order di EMR. Pesan: "Akses peresepan dan penjadwalan tindakan operasi di SIMRS diblokir total (Hard-Stop)." | `Fatal` |
| `VR-OPP-23` | Keharusan Pencatatan Audit Log Buka Dokumen | `HrdClinicalPeerReviewAuditLog` | Akses Lembar Ulasan | Gagal jika request pembukaan dokumen telaah sejawat dieksekusi tanpa perekaman data log audit. Pesan: "Pelanggaran privasi: Pembukaan lembar ulasan sejawat wajib mencatat log audit permanen." | `Fatal` |
| `VR-OPP-24` | Integritas Status Final Rapor OPPE | `TrxOppeEvaluationPeriod` | Edit Pasca Finalisasi | Gagal jika pengguna mencoba mengedit nilai pada periode yang telah berstatus `Finalized`. Pesan: "Periode evaluasi OPPE yang telah difinalisasi dikunci permanen dan tidak dapat diubah." | `Error` |
| `VR-OPP-25` | Validasi Struktur Kontrak Dispatch SIMRS | `SimrsSyncContract` | Transmisi `INT-HR-76` | Gagal jika payload sinkronisasi tidak menyertakan kode dokter, ID tindakan, level, dan timestamp. Pesan: "Payload transmisi ke SIMRS wajib memenuhi skema kontrak pertukaran data yang valid." | `Error` |

## 25. Matriks Validasi Sub-Modul 16: Learning, Training & Competency (Slice S-C2)

Matriks aturan validasi ini memuat 25 aturan penegakan integritas bisnis (`VR-TRN-01` s.d. `VR-TRN-25`) untuk menjamin keselamatan pasien, kepatuhan akreditasi STARKES 2024, akuntabilitas anggaran diklat, penegakan disiplin ikatan dinas, dan keabsahan portofolio SKP nakes.

| Kode Aturan | Kategori | Nama Validasi | Deskripsi & Aturan Bisnis | Respon Error & Kode HTTP |
| :--- | :--- | :--- | :--- | :--- |
| `VR-TRN-01` | Akreditasi | Mandatori 5 Pelatihan Wajib STARKES KPS 8 | Seluruh pegawai rumah sakit (100%) wajib memiliki bukti keikutsertaan 5 pelatihan dasar: BHD, PPI, Keselamatan Pasien (SKP), K3RS & Penanggulangan Kebakaran (Code Red), serta Komunikasi Efektif SBAR (`HRD-DEC-158`). | `400 Bad Request` — Pelatihan wajib akreditasi tidak boleh dihapus atau dikecualikan untuk pegawai aktif. |
| `VR-TRN-02` | Siklus | Masa Berlaku Maksimal 24 Bulan | Sertifikat pelatihan wajib akreditasi memiliki masa kedaluwarsa maksimal 24 bulan (2 tahun). Sistem menolak konfigurasi masa berlaku melampaui 24 bulan untuk kategori wajib akreditasi. | `400 Bad Request` — Masa berlaku pelatihan wajib rumah sakit dibatasi maksimal 24 bulan sesuai regulasi akreditasi. |
| `VR-TRN-03` | Presensi | Ambang Minimal Kehadiran Peserta | Peserta diklat non-klinis wajib memenuhi minimal 80.00% jam pelajaran, sedangkan tenaga medis klinis wajib memenuhi 100.00% presensi sesi untuk berhak mengikuti evaluasi kelulusan (`HRD-DEC-159`). | `422 Unprocessable Entity` — Kehadiran peserta belum memenuhi ambang batas minimal untuk mengikuti ujian kelulusan. |
| `VR-TRN-04` | Evaluasi | Nilai Kelulusan Teori Post-Test | Peserta dinyatakan lulus ujian teori post-test apabila memperoleh skor minimal 75.00 (skala 0 - 100). Skor di bawah ambang batas dinyatakan remidi. | `400 Bad Request` — Skor post-test di bawah ambang batas minimal kelulusan (75.00). |
| `VR-TRN-05` | Simulasi | Kewajiban Ujian Praktik Klinis Langsung (OSCE) | Pelatihan keterampilan klinis (BHD, ACLS, ICU Ventilator, Triase) wajib menyelenggarakan ujian simulasi langsung (*OSCE*) dengan ambang kelulusan minimal 80.00 (`HRD-DEC-159`). | `422 Unprocessable Entity` — Pelatihan klinis mensyaratkan kelulusan ujian praktik OSCE sebelum sertifikat dapat diterbitkan. |
| `VR-TRN-06` | Legalitas | Tanda Tangan Digital Assessor OSCE | Lembar rubrik checklist simulasi OSCE klinis wajib memuat tanda tangan digital / hash kriptografis sah dari instruktur/dokter assessor terverifikasi. | `400 Bad Request` — Pengesahan ujian simulasi OSCE wajib menyertakan tanda tangan digital assessor yang sah. |
| `VR-TRN-07` | Kapasitas | Pengendalian Kuota Peserta Sesi Kelas | Pendaftaran peserta baru akan ditolak jika jumlah pendaftar yang disetujui telah mencapai kuota kapasitas maksimal ruang atau simulator kelas. | `409 Conflict` — Kuota kelas pelatihan telah penuh, silakan pilih jadwal batch sesi berikutnya. |
| `VR-TRN-08` | Konflik | Pencegahan Bentrok Jadwal Mengajar & Belajar | Sistem menolak pendaftaran jika jadwal sesi pelatihan yang dipilih berbenturan tanggal dan jam dengan penugasan kelas lain atau jadwal dinas jaga non-dispensasi. | `409 Conflict` — Jadwal sesi pelatihan bentrok dengan penugasan atau jadwal lain yang sedang aktif. |
| `VR-TRN-09` | Radar | Notifikasi Radar Kedaluwarsa Bertingkat | Sistem wajib memicu evaluasi otomatis harian untuk mendeteksi sertifikat yang kedaluwarsa dalam rentang H-90, H-60, dan H-30 hari, serta mengirimkan notifikasi multi-kanal. | `200 OK` (Worker) — Peringatan radar bertingkat diproses dan dikirimkan ke sasaran staf. |
| `VR-TRN-10` | Toleransi | Masa Tenggang (*Grace Period*) 30 Hari | Pegawai yang masa berlaku sertifikatnya habis diberikan masa tenggang 30 hari kalender untuk mendaftar kelas penyegaran sebelum sanksi pemblokiran dinas diterapkan. | `400 Bad Request` — Batas grace period pendaftaran penyegaran telah terlampaui. |
| `VR-TRN-11` | Keselamatan | Pengamanan Shift Dinas (*Buddy System*) H-0 | Staf yang sertifikat pelatihan wajib kritisnya mati (H-0 Overdue) secara otomatis ditandai *Flag Overdue* dan wajib dijadwalkan berdampingan dengan staf senior (*buddy system*) di Sub-06 (`HRD-DEC-160`). | `422 Unprocessable Entity` — Penugasan shift mandiri ditolak; pegawai berstatus Overdue wajib dipasangkan dengan staf pendamping. |
| `VR-TRN-12` | Kredensial | Pembekuan Mandiri Kewenangan RKK Level 4 | Apabila sertifikat spesialisasi klinis (misal ACLS/Ventilator) mati tanpa perpanjangan, kewenangan klinis mandiri (Level 4) diturunkan otomatis ke Level 3 supervisi (`INT-HR-77`). | `400 Bad Request` — Penurunan kewenangan klinis otomatis diberlakukan karena prasyarat kompetensi kedaluwarsa. |
| `VR-TRN-13` | Perencanaan | Kelengkapan Usulan Program Rencana Diklat (TNA) | Dokumen rencana tahunan diklat wajib mencantumkan target kompetensi, estimasi anggaran, jadwal tentatif, dan target jumlah peserta per departemen. | `400 Bad Request` — Dokumen usulan rencana program tahunan belum memenuhi standar kelengkapan TNA. |
| `VR-TRN-14` | Anggaran | Pagu Anggaran Pelatihan Unit Tidak Boleh Defisit | Total estimasi biaya pendaftaran diklat yang diajukan tidak boleh melampaui sisa saldo pagu anggaran tahunan unit pengusul (`remaining_budget` >= 0). | `422 Unprocessable Entity` — Sisa pagu anggaran diklat departemen tidak mencukupi untuk membiayai pelatihan yang diajukan. |
| `VR-TRN-15` | Realisasi | Komitmen Anggaran Otomatis Saat Approval Diklat | Saat pendaftaran diklat disetujui Diklat, sistem memindahkan saldo dari `remaining_budget` ke `committed_budget` guna mencegah *over-budgeting*. | `400 Bad Request` — Gagal melakukan alokasi komitmen anggaran pelatihan. |
| `VR-TRN-16` | Ikatan Dinas| Kewajiban Akta Ikatan Dinas Biaya > Rp 10 Jt / > 1 Bulan | Staf yang mengikuti diklat eksternal dengan biaya pembiayaan RS > Rp 10.000.000 atau durasi > 30 hari wajib menerbitkan dan menandatangani akta ikatan dinas retensi kerja (`HRD-DEC-162`). | `422 Unprocessable Entity` — Pelatihan wajib dilengkapi kontrak ikatan dinas sebelum pencairan pembiayaan dilakukan. |
| `VR-TRN-17` | Penalti | Formula Ganti Rugi Wanprestasi Ikatan Dinas | Staf yang resign sebelum ikatan dinas selesai dikenakan tagihan penalti ganti rugi: biaya penuh ditambah denda 50% atau prorata sisa bulan ikatan (`HRD-DEC-162`). | `400 Bad Request` — Tagihan ganti rugi wanprestasi ikatan dinas dihitung berdasarkan formula kontrak yang sah. |
| `VR-TRN-18` | Regulasi | Pembatasan Masa Retensi Maksimal 36 Bulan | Durasi retensi kerja pada perjanjian ikatan dinas dibatasi maksimal 36 bulan (3 tahun) guna menjaga kepatuhan terhadap perundang-undangan ketenagakerjaan RI. | `400 Bad Request` — Masa ikatan dinas retensi kerja tidak boleh melebihi batas maksimal 36 bulan. |
| `VR-TRN-19` | Verifikasi | Validasi Sertifikat Eksternal oleh Bagian Diklat | Sertifikat pelatihan yang diperoleh staf dari luar rumah sakit berstatus `Unverified` hingga disetujui oleh verifikator Diklat sebelum diakui pada profil kepegawaian. | `422 Unprocessable Entity` — Sertifikat eksternal belum diverifikasi dan tidak dapat digunakan sebagai prasyarat kredensial. |
| `VR-TRN-20` | Remedi | Batasan Kesempatan Remedial Maksimal 2 Kali | Kesempatan remedial untuk ujian teori post-test dan simulasi OSCE dibatasi maksimal 2 kali percobaan dalam rentang waktu paling lambat 30 hari kalender. | `400 Bad Request` — Batas maksimal remedial (2 kali) telah habis; peserta dinyatakan tidak lulus dan wajib mengulang kelas penuh. |
| `VR-TRN-21` | Penomoran | Format Baku Nomor Seri Sertifikat Rumah Sakit | Nomor registrasi sertifikat terbitan internal wajib mengikuti pola: `SRT/HRD-TRN/{TAHUN}/{BULAN}/{5_DIGIT_SEQUENCE}` dan terjamin unik (*no duplication*). | `400 Bad Request` — Format nomor sertifikat tidak sesuai dengan pola penomoran resmi RS. |
| `VR-TRN-22` | Kemenkes | Verifikasi ID Sertifikat Plataran Sehat | Entri sertifikat dengan nomor registrasi Plataran Sehat Kemenkes wajib mematuhi format ID standar Kemenkes RI (`INT-HR-80`). | `400 Bad Request` — Format ID sertifikat Plataran Sehat Kemenkes tidak valid atau tidak terverifikasi. |
| `VR-TRN-23` | Portofolio | Pembatasan Poin SKP per Kegiatan | Bobot perolehan SKP untuk setiap kegiatan pembelajaran dibatasi sesuai standar Kemenkes (maksimal 5 SKP per workshop/seminar, 10 SKP per kursus klinis intensif). | `400 Bad Request` — Poin SKP yang diklaim melebihi batas ketentuan Kemenkes untuk jenis kegiatan tersebut. |
| `VR-TRN-24` | Target Nakes| Target Akumulasi 50 SKP dalam 5 Tahun | Sistem memetakan target akumulasi 50 SKP tenaga medis dalam 5 tahun untuk perpanjangan SIP, mencakup 3 kategori: Pembelajaran, Pelayanan, dan Pengabdian Masyarakat. | `200 OK` — Kalkulasi visual tracker 50 SKP berhasil diperbarui. |
| `VR-TRN-25` | Terpadu | Umpan Data Pelatihan ke Evaluasi OPPE Sub-15 | Data kelulusan diklat dan perolehan SKP nakes otomatis diteruskan sebagai bukti pilar *Medical Knowledge* pada evaluasi kinerja klinis berkelanjutan OPPE (`INT-HR-78`). | `200 OK` — Riwayat pelatihan berhasil disinkronkan ke pilar OPPE dokter. |

## 26. Matriks Validasi Sub-Modul 17: Performance Management & Clinical Career (Slice S-C3)

Matriks aturan validasi ini memuat 25 aturan penegakan integritas bisnis (`VR-PRF-01` s.d. `VR-PRF-25`) untuk menjamin akuntabilitas penilaian kinerja, objektivitas kalibrasi, kepatuhan pembinaan PIP, keselamatan klinis jenjang profesi, dan keadilan sanggah banding.

| Kode Aturan | Kategori | Nama Validasi | Deskripsi & Aturan Bisnis | Respon Error & Kode HTTP |
| :--- | :--- | :--- | :--- | :--- |
| `VR-PRF-01` | Siklus | Larangan Overlapping Periode Kinerja Aktif | Sistem menolak pembukaan siklus evaluasi kinerja baru jika masih terdapat siklus aktif pada tipe yang sama yang belum ditutup resmi. | `409 Conflict` — Siklus kinerja sebelumnya masih aktif dan belum disahkan penutupannya. |
| `VR-PRF-02` | Target | Batasan Butir Sasaran Kerja Individu | Sasaran kerja individu pegawai wajib terdiri dari minimal 3 dan maksimal 8 butir target spesifik dengan akumulasi total bobot tepat 100.00%. | `400 Bad Request` — Jumlah butir sasaran harus 3-8 butir dan total bobot harus tepat 100%. |
| `VR-PRF-03` | Komposisi | Rasio Baku 60% KPI + 40% Core Values BerAKHLAK | Skor komposit evaluasi kinerja wajib menerapkan formula baku: 60% capaian hasil kerja KPI kuantitatif + 40% perilaku Core Values BerAKHLAK (`HRD-DEC-164`). | `422 Unprocessable Entity` — Skor komposit tidak dapat dihitung karena salah satu komponen belum dinilai. |
| `VR-PRF-04` | Alur | Penegakan Urutan Penilaian Tiga Tingkat | Penilaian wajib melewati urutan: Self-Assessment -> Penilaian Karu -> Penelaahan Kabid. Tahapan tidak boleh dilompati atau mendahului atasan langsung. | `400 Bad Request` — Berkas penilaian belum diserahkan oleh penilai pada tahapan sebelumnya. |
| `VR-PRF-05` | Kalibrasi | Kewajiban Nota Justifikasi Override Skor | Setiap perubahan skor kinerja pegawai pada sidang pleno kalibrasi wajib mencantumkan alasan tertulis resmi (*override notes*) dan risalah komite (`HRD-DEC-165`). | `400 Bad Request` — Penyesuaian skor kalibrasi wajib disertai nota pertimbangan komite. |
| `VR-PRF-06` | Privasi | Kerahasiaan Skor Sebelum Rilis Resmi | Lembar evaluasi dan sebaran kurva kalibrasi bersifat rahasia dan tidak dapat diakses pegawai sebelum waktu rilis massal yang ditetapkan Direksi. | `403 Forbidden` — Hasil evaluasi kinerja masih dalam tahap sidang kalibrasi dan belum dirilis resmi. |
| `VR-PRF-07` | Predikat | Standarisasi Predikat Rentang Skor Kinerja | Predikat kinerja ditetapkan baku: Sangat Baik (A: >= 90.0), Baik (B: 75.0 - 89.9), Cukup (C: 65.0 - 74.9), Kurang (D: 50.0 - 64.9), Sangat Kurang (E: < 50.0). | `400 Bad Request` — Rentang skor komposit tidak valid untuk penentuan predikat kinerja. |
| `VR-PRF-08` | Pembinaan | Mandatori Program PIP untuk Skor Akhir < 65 | Pegawai yang memperoleh skor komposit akhir < 65.00 secara otomatis diterbitkan kontrak pembinaan formal PIP 60 hari kalender (`HRD-DEC-167`). | `200 OK` (Sistem) — Pemicu otomatis program PIP diaktifkan pada profil pegawai. |
| `VR-PRF-09` | Monitoring | Kepatuhan Logbook Mingguan Mentor PIP | Mentor dan atasan wajib mengisi catatan evaluasi mingguan minimal 1 kali per 7 hari kalender selama 8 minggu masa pembinaan PIP. | `400 Bad Request` — Logbook mingguan wajib diisi sebelum tanggal batas akhir minggu bersangkutan. |
| `VR-PRF-10` | Batas PIP | Pembatasan Perpanjangan PIP Maksimal 1 Kali | Perpanjangan program PIP hanya diperbolehkan tepat 1 kali dengan durasi paling lama 30 hari kalender jika ada rekomendasi tren perbaikan positif. | `422 Unprocessable Entity` — Batas perpanjangan PIP (1 kali 30 hari) telah habis; sidang wajib menetapkan putusan akhir. |
| `VR-PRF-11` | Karir | Pembekuan Mutasi/Promosi Selama PIP Aktif | Pegawai yang sedang menjalani masa kontrak PIP aktif dibekukan sementara haknya untuk mengajukan mutasi internal, rotasi, atau promosi jabatan. | `422 Unprocessable Entity` — Pengajuan mutasi/promosi ditolak karena pegawai sedang berstatus PIP aktif. |
| `VR-PRF-12` | Sanggah | Batas Waktu Pengajuan Banding Maksimal 7 Hari | Pegawai berhak mengajukan sanggah banding nilai resmi maksimal 7 hari kalender sejak pengumuman rilis nilai di ESS (`HRD-DEC-169`). | `400 Bad Request` — Batas waktu pengajuan sanggah banding (7 hari kalender) telah berakhir. |
| `VR-PRF-13` | Putusan | SLA Majelis Banding Maksimal 14 Hari Kalender | Majelis komite banding wajib memutus perkara sanggahan nilai pegawai maksimal 14 hari kalender sejak diterimanya berkas eskalasi. | `400 Bad Request` — Batas waktu SLA penyelesaian banding komite terlampaui. |
| `VR-PRF-14` | Gate Klinis | Prasyarat Skor Kinerja Minimal 75 untuk Jenjang | Kenaikan jenjang karier klinis perawat/nakes (PK I s.d. PK V) mensyaratkan skor evaluasi kinerja tahunan minimal 75.00 (Predikat "Baik", `HRD-DEC-168`). | `422 Unprocessable Entity` — Skor kinerja tahunan belum memenuhi ambang batas minimal kelayakan (75.00). |
| `VR-PRF-15` | Masa Kerja | Validasi Masa Kerja Klinis Minimum per Jenjang | Permohonan jenjang klinis wajib memenuhi syarat masa kerja klinis minimum sesuai regulasi Permenkes 40/2017 (misal PK I ke PK II min 3 tahun). | `400 Bad Request` — Masa kerja klinis pada jenjang saat ini belum mencukupi batas persyaratan regulasi. |
| `VR-PRF-16` | Kredensial | Wajib Sertifikasi Kompetensi Aktif Sub-16 | Permohonan jenjang klinis wajib didukung sertifikat kompetensi klinis yang berstatus aktif dan tidak kedaluwarsa di Sub-Modul 16 (`INT-HR-83`). | `422 Unprocessable Entity` — Sertifikat kompetensi klinis prasyarat ditemukan kedaluwarsa atau belum lengkap. |
| `VR-PRF-17` | Keselamatan | Bebas Insiden Keselamatan Pasien Sentinel Sub-15 | Nakes yang memiliki catatan pelanggaran klinis atau insiden sentinel aktif di Sub-15 OPPE/FPPE dilarang mengajukan kenaikan jenjang klinis. | `422 Unprocessable Entity` — Permohonan ditolak karena terdapat catatan insiden keselamatan pasien aktif. |
| `VR-PRF-18` | Wewenang | Pengesahan Jenjang Klinis Wajib Rekomendasi Komite| Kenaikan jenjang klinis tidak boleh diputuskan sepihak oleh atasan unit; wajib didukung rekomendasi sah sidang Komite Keperawatan/Nakes. | `400 Bad Request` — Penerbitan SK jenjang klinis memerlukan berita acara rekomendasi resmi komite. |
| `VR-PRF-19` | Rubrik | Skala Likert 1-5 untuk Evaluasi Perilaku | Evaluasi perilaku Core Values BerAKHLAK wajib dinilai menggunakan skala 1 (Sangat Kurang) s.d. 5 (Sangat Baik) berpedoman pada rubrik baku. | `400 Bad Request` — Nilai evaluasi perilaku harus berada dalam rentang skala likert 1 s.d. 5. |
| `VR-PRF-20` | Metrik | Validasi Polaritas Indikator Kinerja | Capaian metrik berpolaritas negatif (misal: tingkat komplikasi/infeksi) dihitung semakin kecil semakin baik; polaritas positif sebaliknya. | `400 Bad Request` — Perhitungan persentase capaian KPI tidak sesuai dengan polaritas metrik. |
| `VR-PRF-21` | Absensi | Prasyarat Kehadiran Efektif Minimal 90% | Pegawai wajib memenuhi minimal 90.00% jam kerja kehadiran (Sub-07) untuk berhak mendapatkan penilaian kinerja penuh tanpa penalti pengurangan. | `422 Unprocessable Entity` — Jam kehadiran kurang dari 90%; skor akhir dikenakan faktor koreksi kehadiran. |
| `VR-PRF-22` | Bukti | Wajib Unggah Berkas Dukung untuk Nilai Ekstrem | Pengajuan skor mandiri (Self-Assessment) bernilai maksimal (Skor 5) atau capaian > 120% wajib melampirkan berkas bukti dokumen pendukung sah. | `400 Bad Request` — Penilaian skor maksimal memerlukan unggahan berkas bukti dokumen pendukung. |
| `VR-PRF-23` | Mutasi | Pembaruan Profil Kepegawaian Pasca SK Jenjang | Penerbitan SK Direktur Utama atas jenjang klinis baru secara otomatis memutakhirkan level kepangkatan di Sub-02 dan kewenangan RKK di Sub-14. | `200 OK` (Event) — Profil level kepegawaian dan rincian kewenangan klinis berhasil disinkronkan. |
| `VR-PRF-24` | Remunerasi | Umpan Skor Kinerja ke Pengali Insentif Sub-10 | Skor akhir komposit kinerja terkalibrasi menjadi faktor pengali resmi (*performance multiplier*) perhitungan insentif tahunan di Sub-10 (`INT-HR-81`). | `200 OK` — Skor kinerja berhasil dipetakan ke koefisien pengali remunerasi pegawai. |
| `VR-PRF-25` | Kompetensi | Umpan Gap Kinerja ke Rencana Pelatihan Sub-16 | Butir penilaian kinerja yang bernilai rendah (< 60) otomatis dipetakan sebagai rekomendasi kebutuhan pelatihan (*IDP*) ke Sub-16 (`INT-HR-82`). | `200 OK` — Kebutuhan pengembangan kompetensi berhasil ditambahkan ke rencana TNA diklat. |


---

## 27. Matriks Validasi Sub-Modul 18: Hubungan Industrial & Terminasi (Slice `S-C5`)

| Kode Aturan | Kategori / Entitas Target | Aturan Validasi / Ekspresi Logika | Pesan Kesalahan / Konsekuensi Bila Gagal |
|---|---|---|---|
| `VR-EMP-01` | Laporan Insiden | Wajib memuat `employee_id`, `incident_date` (tidak boleh di masa depan), `incident_location`, dan `description` minimal 50 karakter | *"Data dasar laporan insiden tidak lengkap atau deskripsi kronologi kejadian terlalu singkat."* |
| `VR-EMP-02` | Just Culture Insiden | Jika `category = 'ClinicalSafety'`, maka `kmkp_audit_reference_number` wajib terisi dan terdaftar di Sub-15 | *"Insiden yang melibatkan keselamatan pasien wajib menyertakan nomor registrasi audit keselamatan pasien KMKP."* |
| `VR-EMP-03` | Penjatuhan Sanksi | Dilarang menerbitkan sanksi SP jika `just_culture_classification = 'HumanError'` | *"Prinsip Just Culture: Insiden kategori Human Error murni dialihkan ke pembinaan/diklat Sub-16, bukan sanksi SP."* |
| `VR-EMP-04` | Pemeriksaan BAP | `investigator_names` minimal memuat 2 pemeriksa dan tidak boleh sama dengan `employee_id` terperiksa | *"Tim pemeriksa BAP minimal terdiri dari 2 orang independen tanpa konflik kepentingan."* |
| `VR-EMP-05` | Pemeriksaan BAP | BAP tidak dapat disahkan jika `employee_defense_statement` kosong tanpa alasan penolakan tertulis | *"Berita Acara Pemeriksaan wajib memuat lembar pembelaan tertulis pegawai terperiksa."* |
| `VR-EMP-06` | Sanksi SP | Tanggal kedaluwarsa SP 1, 2, 3 wajib tepat `ValidUntil = IssuedDate + 6 Months` kalender | *"Masa berlaku Surat Peringatan (SP 1, 2, 3) wajib tepat 6 bulan kalender sesuai Pasal 52 PP 35/2021."* |
| `VR-EMP-07` | Teguran Lisan | Masa berlaku Teguran Lisan maksimal 90 hari kalender (`IssuedDate + 3 Months`) | *"Masa berlaku teguran lisan tidak boleh melebihi 3 bulan kalender."* |
| `VR-EMP-08` | Tingkat Sanksi SP | SP 2 hanya dapat diterbitkan bila staf memiliki SP 1 berstatus `Active`, kecuali ditandai sebagai pelanggaran berat mendesak | *"Penerbitan SP 2 mensyaratkan adanya SP 1 yang masih aktif, kecuali untuk pelanggaran berat langsung."* |
| `VR-EMP-09` | Direct SP 3 Bypass | Penerbitan SP 3 langsung tanpa SP 1/2 wajib melampirkan persetujuan Direktur SDM dan kategori `RecklessMisconduct` | *"Penerbitan langsung SP 3 wajib melalui persetujuan Direktur SDM dengan bukti pelanggaran berat."* |
| `VR-EMP-10` | Sanggah Banding SP | Pengajuan banding atas sanksi SP dibatasi maksimal 7 hari kerja sejak `IssuedDate` | *"Batas waktu pengajuan sanggah banding sanksi disiplin telah kedaluwarsa (maksimal 7 hari kerja)."* |
| `VR-EMP-11` | Skorsing Durasi | Durasi skorsing investigasi (`expected_end_date - start_date`) maksimal 180 hari kalender (6 bulan) | *"Masa skorsing investigasi tidak boleh melampaui batas maksimal 6 bulan kalender sesuai regulasi."* |
| `VR-EMP-12` | Skorsing Hak Upah | `wage_payment_status` wajib bernilai `'PaidFull'` (upah pokok dan tunjangan tetap 100%) | *"Pelanggaran Pasal 157A UU Ketenagakerjaan: Pegawai dalam masa skorsing wajib menerima upah penuh."* |
| `VR-EMP-13` | Skorsing Akses | Saat skorsing aktif, `is_system_access_revoked` dan `is_physical_access_revoked` wajib bernilai `TRUE` | *"Demi keamanan rumah sakit, akses SIMRS dan kartu fisik wajib dibekukan selama masa skorsing."* |
| `VR-EMP-14` | SLA Grievance | Batas waktu tanggapan mediasi tiket keluhan `target_resolution_date` maksimal 14 hari kerja dari tanggal pengajuan | *"Target resolusi tiket keluhan kerja melebihi standar SLA pelayanan kepegawaian (14 hari kerja)."* |
| `VR-EMP-15` | Whistleblower Enkripsi | Payload tiket whistleblower wajib terenkripsi (`encrypted_report_payload` bukan teks polos) | *"Data pengaduan whistleblower wajib dienkripsi pada level payload sebelum dikirim ke server."* |
| `VR-EMP-16` | Hak Akses Whistleblower | Pengguna yang memiliki relasi atasan langsung terhadap pihak terlapor dilarang mengakses tiket pengaduan | *"Akses ditolak: Atasan langsung terlapor dilarang membaca berkas penyelidikan whistleblower."* |
| `VR-EMP-17` | Forum Bipartit | `management_representatives` dan `union_representatives` masing-masing minimal memuat 1 nama delegasi | *"Forum perundingan Bipartit wajib dihadiri oleh perwakilan kedua belah pihak secara sah."* |
| `VR-EMP-18` | Naskah Perjanjian PB | `joint_agreement_document_url` wajib berekstensi `.pdf` dan tidak boleh kosong jika status = `CompletedAgreementReached` | *"Naskah Perjanjian Bersama (PB) wajib dilampirkan dalam format PDF resmi."* |
| `VR-EMP-19` | Notice Resign | Tanggal pemberitahuan resign sukarela (`notice_date`) minimal 30 hari kalender sebelum `effective_date` | *"Pemberitahuan pengunduran diri sukarela wajib memenuhi ketentuan One Month Notice (30 hari)."* |
| `VR-EMP-20` | Masa Kerja Pesangon | `service_years` dan `service_months` wajib dihitung akurat dari `HireDate` hingga `effective_date` | *"Perhitungan masa kerja pesangon wajib sinkron dengan tanggal mulai kerja sah di profil pegawai."* |
| `VR-EMP-21` | Kompensasi PKWT | Nilai kompensasi PKWT wajib dihitung proporsional: `(service_months / 12) * base_monthly_wage` | *"Formula kompensasi PKWT tidak sesuai dengan ketentuan Pasal 15 s.d. 17 PP No. 35 Tahun 2021."* |
| `VR-EMP-22` | Uang Penggantian Hak | Komponen UPH cuti wajib memvalidasi sisa cuti tahunan aktif dikalikan upah per hari (`base_wage / 21` atau `25`) | *"Nilai Uang Penggantian Hak cuti tahunan tidak sesuai dengan sisa saldo cuti sah di Sub-08."* |
| `VR-EMP-23` | Otorisasi Pesangon | `is_approved_by_finance_director` wajib bernilai `TRUE` sebelum status perhitungan menjadi `Approved` | *"Lembar kalkulasi pesangon wajib mendapatkan pengesahan digital resmi dari Direktur Keuangan."* |
| `VR-EMP-24` | Kelengkapan Kliring | Seluruh item checklist dari 5 departemen (Unit, Aset, Farmasi, Keuangan, IT) wajib berstatus `Approved` | *"Proses exit clearance belum dapat diselesaikan karena masih ada checklist departemen yang pending/ditolak."* |
| `VR-EMP-25` | Hard-Block Payroll | `is_final_payroll_unlocked` dilarang diset `TRUE` jika `overall_clearance_status != 'Completed'` | *"Hard-Block Keamanan: Payroll dan pesangon akhir dilarang dirilis sebelum exit clearance selesai 100%."* |
