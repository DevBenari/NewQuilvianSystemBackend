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
