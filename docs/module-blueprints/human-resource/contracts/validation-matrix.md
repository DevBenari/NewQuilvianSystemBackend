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
